using UnityEngine;
using Mirror;

public class NpcHareket : NetworkBehaviour
{
    [Header("Hareket ve Savaş Ayarları")]
    [SerializeField] private float hiz = 1f;
    [SerializeField] private int hasar = 100;
    [SerializeField] private bool agresif = false;
    [SerializeField] private float gorusAlani = 15f; // GDD: "Her seviye geminin görüş alanı 15 birimdir"
    [SerializeField] private float atisMenzili = 8f;
    [SerializeField] private float atisAraligi = 3.5f;

    [Header("Mühimmat Ayarları")]
    [SerializeField] private GameObject alevGullePrefab;
    [SerializeField] private Transform atisNoktasi;

    [Header("Devriye Ayarları")]
    private Transform hedefOyuncu;
    private NetworkIdentity hedefNetId;
    private float sonAtisZamani;
    private Vector3 devriyeHedefi;
    private bool devriyeModunda = false;
    private float devriyeHizi = 0.5f;
    private float devriyeYonDegistirmeZamani = 0f;
    private float devriyeYonDegistirmeAraligi = 8f;

    public float Hiz => hiz;
    public int Hasar => hasar;
    public bool Agresif => agresif;

    public void Ayarla(float yeniHiz, int yeniHasar, bool yeniAgresif)
    {
        hiz = yeniHiz;
        hasar = yeniHasar;
        agresif = yeniAgresif;
    }

    private void Start()
    {
        devriyeHedefi = transform.position;

        // Z ekseninde dönmeyi fizik seviyesinde kilitle
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if (!isServer) return;

        // Z rotasyonunu her kare sıfırla
        if (transform.rotation != Quaternion.identity)
            transform.rotation = Quaternion.identity;

        // Görüş alanında oyuncu var mı kontrol et
        bool oyuncuGoruldu = OyuncuGorulduMu();

        if (oyuncuGoruldu)
        {
            // Devriye moduna gir
            if (!devriyeModunda)
            {
                devriyeModunda = true;
            }

            // Agresif ise ve hedef yoksa en yakını bul
            if (agresif && hedefOyuncu == null)
            {
                EnYakinOyuncuyuBul();
            }

            // Aktif hedef varsa kontrol et ve takip et / saldır
            if (hedefOyuncu != null)
            {
                float mesafe = Vector2.Distance(transform.position, hedefOyuncu.position);

                // Görüş alanı dışına çıkarsa hedefi bırak
                if (mesafe > gorusAlani)
                {
                    HedefiTemizle();
                }
                else
                {
                    // Menzile yaklaş
                    if (mesafe > atisMenzili)
                    {
                        Vector2 yon = (hedefOyuncu.position - transform.position).normalized;
                        transform.position += (Vector3)(yon * hiz * Time.deltaTime);
                    }

                    // Atış menzilindeyse ateş et
                    if (mesafe <= atisMenzili && Time.time - sonAtisZamani >= atisAraligi)
                    {
                        AtesEt();
                    }
                }
            }
            else
            {
                // Hedef yoksa devriye hareketi yap
                DevriyeHareketi();
            }
        }
        else
        {
            // Hiç oyuncu görülmüyorsa uyku moduna dön (Tamamen hareketsiz)
            if (devriyeModunda)
            {
                devriyeModunda = false;
                HedefiTemizle();
            }
        }
    }

    private void AtesEt()
    {
        if (hedefOyuncu == null) return;

        sonAtisZamani = Time.time;

        // GDD: NPC'ler kendilerine ateş eden oyunculara alev güllesi atar
        RpcAlevGullesiAt(hedefOyuncu.gameObject, hasar);
    }

    [ClientRpc]
    private void RpcAlevGullesiAt(GameObject hedefObj, int hasarMiktari)
    {
        if (hedefObj == null) return;

        if (alevGullePrefab == null)
        {
            Debug.LogWarning("[NpcHareket] Alev güllesi prefabı atanmamış! Lütfen inspector'dan atayın.");
            return;
        }

        Vector3 cikisPos = atisNoktasi != null ? atisNoktasi.position : transform.position;

        GameObject gulle = NesneHavuzu.Al(alevGullePrefab, cikisPos, Quaternion.identity);
        Gulle gulleScript = gulle.GetComponent<Gulle>();
        if (gulleScript != null)
        {
            // NPC'nin kendi netIdentity'sini atıyoruz (böylece hasarı NPC vermiş olacak)
            gulleScript.Baslat(hedefObj.transform, hasarMiktari, netIdentity);
        }
    }

    public void SaldiranaKarsilikVer(NetworkIdentity saldiran)
    {
        if (saldiran != null && hedefOyuncu == null)
        {
            hedefNetId = saldiran;
            hedefOyuncu = saldiran.transform;
        }
    }

    private void EnYakinOyuncuyuBul()
    {
        OyuncuHareket[] oyuncular = FindObjectsByType<OyuncuHareket>(FindObjectsSortMode.None);
        float enKisaMesafe = gorusAlani;
        Transform secilenOyuncu = null;
        NetworkIdentity secilenId = null;

        foreach (var oyuncu in oyuncular)
        {
            float m = Vector2.Distance(transform.position, oyuncu.transform.position);
            if (m <= enKisaMesafe)
            {
                enKisaMesafe = m;
                secilenOyuncu = oyuncu.transform;
                secilenId = oyuncu.GetComponent<NetworkIdentity>();
            }
        }

        if (secilenOyuncu != null)
        {
            hedefOyuncu = secilenOyuncu;
            hedefNetId = secilenId;
        }
    }

    private bool OyuncuGorulduMu()
    {
        OyuncuHareket[] oyuncular = FindObjectsByType<OyuncuHareket>(FindObjectsSortMode.None);
        foreach (var oyuncu in oyuncular)
        {
            float mesafe = Vector2.Distance(transform.position, oyuncu.transform.position);
            if (mesafe <= gorusAlani)
            {
                return true;
            }
        }
        return false;
    }

    private void DevriyeHareketi()
    {
        // Belirli aralıklarla yön değiştir
        if (Time.time >= devriyeYonDegistirmeZamani)
        {
            Vector2 rastgeleYon = Random.insideUnitCircle.normalized;
            Vector2 rastgeleNokta = (Vector2)transform.position + rastgeleYon * 8f;
            devriyeHedefi = rastgeleNokta;
            devriyeYonDegistirmeZamani = Time.time + devriyeYonDegistirmeAraligi;
        }

        // Hedef noktaya doğru yavaşça ilerle
        float mesafe = Vector2.Distance(transform.position, devriyeHedefi);
        if (mesafe > 0.5f)
        {
            Vector2 yon = (devriyeHedefi - transform.position).normalized;
            transform.position += (Vector3)(yon * hiz * devriyeHizi * Time.deltaTime);
        }
    }

    private void HedefiTemizle()
    {
        hedefOyuncu = null;
        hedefNetId = null;
    }
}