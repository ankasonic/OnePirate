using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NpcSpawner : NetworkBehaviour
{
    [Header("Spawn Ayarları")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int toplamNpcSayisi = 40; // GDD: 40 adet NPC
    [SerializeField] private NpcTuru haritaNpcTuru = NpcTuru.BorisJuckson;
    [SerializeField] private Vector2 spawnAlaniMin = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 spawnAlaniMax = new Vector2(20f, 20f);

    private readonly List<GameObject> aktifNpcler = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(NpcleriBaslat());
    }

    private IEnumerator NpcleriBaslat()
    {
        // 1-1 sahnesi aktif olana kadar bekle (Baslangic'tan gecis sirasinda kaybolmasin)
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "1-1")
        {
            yield return new WaitForSeconds(0.2f);
        }

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
        if (npcPrefab == null) return;

        GameObject npcObj = Instantiate(npcPrefab, konum, Quaternion.identity);
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
