using UnityEngine;
using Mirror;

public class NpcHareket : NetworkBehaviour
{
    [Header("Hareket ve Savaş Ayarları")]
    [SerializeField] private float hiz = 2f;
    [SerializeField] private int hasar = 100;
    [SerializeField] private bool agresif = false;
    [SerializeField] private float gorusAlani = 15f; // GDD: "Her seviye geminin görüş alanı 15 birimdir"
    [SerializeField] private float atisMenzili = 8f;
    [SerializeField] private float atisAraligi = 3.5f;

    [Header("Hedef Durumu")]
    private Transform hedefOyuncu;
    private NetworkIdentity hedefNetId;
    private float sonAtisZamani;
    private Vector3 devriyeHedefi;
    private bool devriyeModunda = false;
    private float devriyeBeklemeZamani;

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

        // 1. Aktif hedef varsa kontrol et ve takip et / saldır
        if (hedefOyuncu != null)
        {
            float mesafe = Vector2.Distance(transform.position, hedefOyuncu.position);

            // Görüş alanı dışına çıkarsa hedefi bırak (GDD: 15 birim)
            if (mesafe > gorusAlani)
            {
                HedefiTemizle();
                return;
            }

            // Menzile yaklaş
            if (mesafe > atisMenzili)
            {
                Vector2 yon = (hedefOyuncu.position - transform.position).normalized;
                transform.position += (Vector3)(yon * hiz * Time.deltaTime);
            }

            // GDD: Atış menzilindeyse ateş et
            if (mesafe <= atisMenzili && Time.time - sonAtisZamani >= atisAraligi)
            {
                AtesEt();
            }
        }
        else
        {
            // 2. Hedef yokken: Agresif ise görüş alanı içindeki oyuncuları tara
            if (agresif)
            {
                EnYakinOyuncuyuBul();
            }

            // 3. GDD: "İlk Durum: Uyku modu Hareketsiz (oyuncu görene kadar)..."
            // Serbest veya sakin devriye/uyku hareketi
            BostaKalmaHareketi();
        }
    }

    private void AtesEt()
    {
        if (hedefOyuncu == null) return;

        // GDD Kuralı: "Tutorial aşamasındaki oyuncu gemisinin canı yarıya inerse NPC ler bu oyuncu gemilerine ateş etmez."
        OyuncuCanBari oyuncuCan = hedefOyuncu.GetComponent<OyuncuCanBari>();
        if (oyuncuCan != null)
        {
            float canOrani = (float)oyuncuCan.GuncelCan / oyuncuCan.MaksCan;
            if (canOrani <= 0.5f)
            {
                // Canı %50 veya daha az olan tutorial/korumalı oyuncuya ateş etme
                return;
            }

            sonAtisZamani = Time.time;
            // GDD: "atılan gülleler kesinlikle hedefi bulur ıskalama gibi bir durum oyunda yoktur."
            oyuncuCan.HasarAl(hasar);
            RpcAtesEfekti(hedefOyuncu.position);
        }
    }

    [ClientRpc]
    private void RpcAtesEfekti(Vector3 hedefPos)
    {
        // İleride gülle parçacığı / animasyonu burada tetiklenir
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

    private void BostaKalmaHareketi()
    {
        if (devriyeModunda)
        {
            float mesafe = Vector2.Distance(transform.position, devriyeHedefi);
            if (mesafe > 0.2f)
            {
                Vector2 yon = (devriyeHedefi - transform.position).normalized;
                transform.position += (Vector3)(yon * (hiz * 0.5f) * Time.deltaTime);
            }
            else
            {
                devriyeModunda = false;
                devriyeBeklemeZamani = Time.time + Random.Range(4f, 10f); // Bekleme süresi
            }
        }
        else if (Time.time >= devriyeBeklemeZamani)
        {
            // Yakınlarda rastgele sakin bir nokta seç
            Vector2 rastgeleNokta = (Vector2)transform.position + Random.insideUnitCircle * 4f;
            devriyeHedefi = rastgeleNokta;
            devriyeModunda = true;
        }
    }

    private void HedefiTemizle()
    {
        hedefOyuncu = null;
        hedefNetId = null;
        devriyeModunda = false;
        devriyeBeklemeZamani = Time.time + 3f;
    }
}
