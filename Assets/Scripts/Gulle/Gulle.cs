using System.Collections;
using UnityEngine;
using Mirror;

public class Gulle : MonoBehaviour
{
    [Header("Gülle Türü")]
    [SerializeField] private GulleTuru gulleTuru = GulleTuru.Demir;

    [Header("Hareket Ayarları")]
    [SerializeField] private float hiz = 18f;

    [Header("Görsel / Efekt Referansları")]
    [SerializeField] private GameObject hasarYazisiPrefab;

    [Header("Parabolik Hareket")]
    [Tooltip("Güllenin havada çizdiği maksimum tepe yüksekliği çarpanı")]
    [SerializeField] private float yayYukseklikCarpani = 0.28f;

    private Transform hedefTransform;
    private Vector3 baslangicPozisyonu;
    private Vector3 sonHedefPozisyon;
    private Vector3 baslangicOlcegi;
    private float toplamMesafe;
    private float toplamSure;
    private float gecenSure;
    private int hasarMiktari;
    private NetworkIdentity atesEden;
    private bool kritikMi;
    private bool hedefeUlasti = false;

    public GulleTuru GulleTuru => gulleTuru;

    private void Awake()
    {
        baslangicOlcegi = transform.localScale;
    }

    public void Baslat(Transform hedef, int hasar, NetworkIdentity atesEdenKimlik, bool kritik = false)
    {
        hedefTransform = hedef;
        hasarMiktari = hasar;
        atesEden = atesEdenKimlik;
        kritikMi = kritik;

        baslangicPozisyonu = transform.position;
        transform.localScale = baslangicOlcegi; // Havuzdan geliyorsa scale'i resetle

        if (hedefTransform != null)
            sonHedefPozisyon = hedefTransform.position;
        else
            sonHedefPozisyon = transform.position;

        toplamMesafe = Vector3.Distance(baslangicPozisyonu, sonHedefPozisyon);
        // Minimum 0.3 saniye, mesafeye göre süreyi hesapla
        toplamSure = Mathf.Max(0.3f, toplamMesafe / hiz);
        gecenSure = 0f;
        hedefeUlasti = false;

        // 5 saniye içinde hedefe ulaşamazsa sahnede takılı kalmasın
        CancelInvoke(nameof(IptalEtVeYokOl));
        Invoke(nameof(IptalEtVeYokOl), 5f);
    }
    
    private void IptalEtVeYokOl()
    {
        NesneHavuzu.Birak(gameObject);
    }

    private void Update()
    {
        if (hedefeUlasti) return;

        if (hedefTransform != null)
        {
            sonHedefPozisyon = hedefTransform.position;
        }

        gecenSure += Time.deltaTime;
        float t = Mathf.Clamp01(gecenSure / toplamSure);

        // Zemin doğrusal pozisyonu (hedefe doğru)
        Vector3 zeminPos = Vector3.Lerp(baslangicPozisyonu, sonHedefPozisyon, t);

        // Parabolik Yay Eğrisi: 4 * h * t * (1 - t)
        float maksYukseklik = Mathf.Clamp(toplamMesafe * yayYukseklikCarpani, 1f, 4.5f);
        float yayYuksekligi = 4f * maksYukseklik * t * (1f - t);

        Vector3 oncekiPos = transform.position;
        transform.position = zeminPos + new Vector3(0f, yayYuksekligi, 0f);

        // Havada yükselirken hafif büyüme (3D derinlik hissi)
        float tepeOrani = yayYuksekligi / maksYukseklik;
        transform.localScale = baslangicOlcegi * (1f + tepeOrani * 0.35f);

        // Uçuş açısı / dönüşü
        Vector3 hareketYonu = transform.position - oncekiPos;
        if (hareketYonu.sqrMagnitude > 0.0001f)
        {
            float aci = Mathf.Atan2(hareketYonu.y, hareketYonu.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, aci);
        }

        // Hedefe ulaştı mı?
        if (t >= 1f || Vector3.Distance(zeminPos, sonHedefPozisyon) <= 0.15f)
        {
            hedefeUlasti = true;
            transform.position = sonHedefPozisyon;
            Vur();
        }
    }

    private void Vur()
    {
        GulleOzellikleri ozellik = GulleVeritabani.Getir(gulleTuru);

        // 1. Hasar Yazısını Hedef Üzerinde Göster
        HasarYazisiGoster(sonHedefPozisyon, hasarMiktari, ozellik.yaziRengi, ozellik.sifami, kritikMi);

        // 2. Hedefe Hasarı veya Şifayı Uygula
        if (hedefTransform != null)
        {
            if (ozellik.sifami)
            {
                // ŞİFA DURUMU (Buz Güllesi): Kesinlikle HasarAl çağrılamaz, sadece CanYenile / CmdSifaVer çağrılır
                if (atesEden != null && atesEden.isLocalPlayer)
                {
                    OyuncuAtes atesComp = atesEden.GetComponent<OyuncuAtes>();
                    if (atesComp != null)
                    {
                        atesComp.CmdSifaVer(hedefTransform.gameObject, hasarMiktari);
                    }
                }
                else if (NetworkServer.active)
                {
                    OyuncuCanBari oCan = hedefTransform.GetComponent<OyuncuCanBari>();
                    if (oCan != null) oCan.CanYenile(hasarMiktari);

                    NpcHasar nCan = hedefTransform.GetComponent<NpcHasar>();
                    if (nCan != null) nCan.CanYenile(hasarMiktari);
                }
            }
            else
            {
                // HASAR DURUMU (Demir, Alev, Asit, Kuru Kafa):
                // Oyuncu kontrolü
                OyuncuCanBari oyuncuCan = hedefTransform.GetComponent<OyuncuCanBari>();
                if (oyuncuCan != null)
                {
                    if (atesEden != null && atesEden.isLocalPlayer)
                    {
                        OyuncuAtes atesComp = atesEden.GetComponent<OyuncuAtes>();
                        if (atesComp != null)
                            atesComp.CmdHasarVer(hedefTransform.gameObject, hasarMiktari);
                    }
                    else if (NetworkServer.active)
                    {
                        oyuncuCan.HasarAl(hasarMiktari);
                    }

                    // Asit güllesi: 3 vuruş etkisi (1s sonra %50, 2s sonra %25)
                    if (ozellik.ucluVurus)
                    {
                        AsitDotUygula(oyuncuCan, hasarMiktari);
                    }
                }

                // NPC kontrolü
                NpcHasar npcHasar = hedefTransform.GetComponent<NpcHasar>();
                if (npcHasar != null && ozellik.npcYeVurabilir)
                {
                    if (atesEden != null && atesEden.isLocalPlayer)
                    {
                        OyuncuAtes atesComp = atesEden.GetComponent<OyuncuAtes>();
                        if (atesComp != null)
                            atesComp.CmdHasarVer(hedefTransform.gameObject, hasarMiktari);
                    }
                    else if (NetworkServer.active)
                    {
                        npcHasar.HasarAl(hasarMiktari, atesEden);
                    }
                }
            }
        }

        // Gülle görselini yok et
        CancelInvoke(nameof(IptalEtVeYokOl));
        NesneHavuzu.Birak(gameObject);
    }

    private void HasarYazisiGoster(Vector3 konum, int miktar, Color renk, bool sifaMi, bool kritik)
{
    if (hasarYazisiPrefab == null) return;

    // Random.Range kaldırıldı: Tam tepe orta noktasına hizalandı
    Vector3 yaziPos = konum + new Vector3(0f, 0.5f, 0f);
    GameObject yaziObj = NesneHavuzu.Al(hasarYazisiPrefab, yaziPos, Quaternion.identity);
    HasarYazisi yaziComp = yaziObj.GetComponent<HasarYazisi>();
    if (yaziComp != null)
    {
        yaziComp.Baslat(miktar, renk, sifaMi, kritik);
    }
}

    private void AsitDotUygula(OyuncuCanBari hedef, int ilkHasar)
    {
        // Asit DoT coroutine'ini bağımsız bir MonoBehaviour üzerinde çalıştır
        AsitHasarYoneticisi.AsitDotBaslat(hedef, ilkHasar, hasarYazisiPrefab);
    }
}

// Asit güllesinin zamanlı ek hasarlarını yöneten yardımcı sınıf
public class AsitHasarYoneticisi : MonoBehaviour
{
    private static AsitHasarYoneticisi ornek;

    public static void AsitDotBaslat(OyuncuCanBari hedef, int ilkHasar, GameObject yaziPrefab)
    {
        if (ornek == null)
        {
            GameObject obj = new GameObject("[AsitHasarYoneticisi]");
            ornek = obj.AddComponent<AsitHasarYoneticisi>();
            DontDestroyOnLoad(obj);
        }

        ornek.StartCoroutine(ornek.AsitDotRutin(hedef, ilkHasar, yaziPrefab));
    }

    private IEnumerator AsitDotRutin(OyuncuCanBari hedef, int ilkHasar, GameObject yaziPrefab)
    {
        // 1 saniye sonra 2. vuruş (ilk hasarın yarısı)
        yield return new WaitForSeconds(1f);
        if (hedef != null && hedef.GuncelCan > 0)
        {
            int ikinciHasar = Mathf.Max(1, ilkHasar / 2);
            hedef.HasarAl(ikinciHasar);
            HasarYazisiOlustur(hedef.transform.position, ikinciHasar, new Color(0.6f, 0.9f, 0.1f, 1f), yaziPrefab);
        }
        else yield break;

        // 2 saniye sonra (toplam 3. saniyede) 3. vuruş (ilk hasarın çeyreği)
        yield return new WaitForSeconds(1f);
        if (hedef != null && hedef.GuncelCan > 0)
        {
            int ucuncuHasar = Mathf.Max(1, ilkHasar / 4);
            hedef.HasarAl(ucuncuHasar);
            HasarYazisiOlustur(hedef.transform.position, ucuncuHasar, new Color(0.6f, 0.9f, 0.1f, 1f), yaziPrefab);
        }
    }

    private void HasarYazisiOlustur(Vector3 konum, int miktar, Color renk, GameObject yaziPrefab)
{
    if (yaziPrefab == null) return;

    // Random.Range kaldırıldı: Tam tepe orta noktasına hizalandı
    Vector3 pos = konum + new Vector3(0f, 0.5f, 0f);
    GameObject yaziObj = NesneHavuzu.Al(yaziPrefab, pos, Quaternion.identity);
    HasarYazisi yazi = yaziObj.GetComponent<HasarYazisi>();
    if (yazi != null)
    {
        yazi.Baslat(miktar, renk, false, false);
    }
}
}
