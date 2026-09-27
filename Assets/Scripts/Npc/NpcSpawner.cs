using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NpcSpawner : NetworkBehaviour
{
    [Header("Spawn Ayarları")]
    [Tooltip("GDD: Spawnable Prefabs listesindeki variant prefabı (NPC_Boris vs) sürükleyin")]
    [SerializeField] private GameObject npcVariantPrefab;
    [SerializeField] private int toplamNpcSayisi = 40; // GDD: 40 adet NPC
    [SerializeField] private NpcTuru haritaNpcTuru = NpcTuru.BorisJuckson;
    [SerializeField] private Vector2 spawnAlaniMin = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 spawnAlaniMax = new Vector2(20f, 20f);

    private readonly List<GameObject> aktifNpcler = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private Coroutine spawnCoroutine;

    public override void OnStartServer()
    {
        base.OnStartServer();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Eğer sunucu başladığında zaten 1-1 veya 2-1'deysek hemen başlat
        string aktifSahne = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (aktifSahne == "1-1" || aktifSahne == "2-1")
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(NpcleriBaslat());
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "1-1" || scene.name == "2-1")
        {
            aktifNpcler.Clear();
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(NpcleriBaslat());
        }
    }

    private IEnumerator NpcleriBaslat()
    {
        yield return new WaitForSeconds(1f);

        // GDD: 10'ar gruplar halinde spawn olur
        int grupBoyutu = 10;
        int olusturulan = 0;

        while (olusturulan < toplamNpcSayisi)
        {
            int buGrupta = Mathf.Min(grupBoyutu, toplamNpcSayisi - olusturulan);
            Vector2 grupMerkezi = RastgeleKonum();

            for (int i = 0; i < buGrupta; i++)
            {
                Vector2 pos = grupMerkezi + Random.insideUnitCircle * 3f;
                TekNpcOlustur(pos);
                olusturulan++;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    [Server]
    private void TekNpcOlustur(Vector2 konum)
    {
        if (npcVariantPrefab == null) return;

        GameObject npcObj = Instantiate(npcVariantPrefab, konum, Quaternion.identity);
        NpcHasar hasar = npcObj.GetComponent<NpcHasar>();
        if (hasar != null)
        {
            hasar.NpcAyarla(haritaNpcTuru);
        }

        NetworkServer.Spawn(npcObj);
        aktifNpcler.Add(npcObj);
    }

    [Server]
    public void NpcOldu(NpcHasar hasarBileseni)
    {
        if (hasarBileseni != null)
        {
            aktifNpcler.Remove(hasarBileseni.gameObject);
            NetworkServer.Destroy(hasarBileseni.gameObject);

            // GDD: "Respawn: Öldürüldüğünde anında, rastgele konum"
            TekNpcOlustur(RastgeleKonum());
        }
    }

    private Vector2 RastgeleKonum()
    {
        float x = Random.Range(spawnAlaniMin.x, spawnAlaniMax.x);
        float y = Random.Range(spawnAlaniMin.y, spawnAlaniMax.y);
        return new Vector2(x, y);
    }
}
