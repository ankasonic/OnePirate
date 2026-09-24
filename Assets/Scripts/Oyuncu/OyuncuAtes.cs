using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System.Collections;

public class OyuncuAtes : NetworkBehaviour
{
    [Header("Atış Ayarları")]
    [Tooltip("Güllelerin çıkış noktası (Geminin merkezi)")]
    [SerializeField] private Transform atisNoktasi;
    
    [Tooltip("İki atış arasındaki bekleme süresi (Saniye)")]
    [SerializeField] private float atisBeklemeSuresi = 1f;

    [Header("Hasar Ayarları")]
    [Tooltip("Geminin temel hasarı (Gülle çarpanı olmadan önceki hasar)")]
    [SerializeField] private float gemiTemelHasari = 100f;

    [Header("Gülle Prefabları")]
    [SerializeField] private GameObject demirGullePrefab;
    [SerializeField] private GameObject alevGullePrefab;
    [SerializeField] private GameObject buzGullePrefab;
    [SerializeField] private GameObject kuruKafaGullePrefab;
    [SerializeField] private GameObject asitGullePrefab;

    private OyuncuHedefSecimi hedefSecimi;
    private GulleSecimi gulleSecimi;
    private OyuncuCanBari canBari;
    
    private float sonAtisZamani = 0f;

    private void Awake()
    {
        hedefSecimi = GetComponent<OyuncuHedefSecimi>();
        gulleSecimi = GetComponent<GulleSecimi>();
        canBari = GetComponent<OyuncuCanBari>();
        
        if (atisNoktasi == null)
        {
            atisNoktasi = transform; // Atış noktası atanmadıysa geminin kendisini kullan
        }
    }

    private bool otomatikAtes = false;
    public bool OtomatikAtes => otomatikAtes;

    private void Update()
    {
        if (!isLocalPlayer) return;

        // Geminin canı 0 ise ateş edemez
        if (canBari != null && canBari.GuncelCan <= 0)
        {
            otomatikAtes = false;
            return;
        }

        if (Keyboard.current != null)
        {
            // 1. Space tuşu ile otomatik ateşi başlat
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                otomatikAtes = true;
                AtesEtmeyiDene();
            }

            // 2. Q tuşu ile ateşi durdur
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                otomatikAtes = false;
            }
        }

        // Otomatik ateş aktifse ve bekleme süresi dolduysa ateş et
        if (otomatikAtes)
        {
            if (hedefSecimi == null || !hedefSecimi.HedefVarMi)
            {
                // Hedef yoksa veya öldüyse otomatik ateşi durdur
                otomatikAtes = false;
            }
            else
            {
                AtesEtmeyiDene();
            }
        }
    }

    private void AtesEtmeyiDene()
    {
        // Bekleme süresi kontrolü
        if (Time.time - sonAtisZamani < atisBeklemeSuresi)
        {
            // İsterseniz buraya "Yeniden yükleniyor..." gibi bir görsel/ses eklenebilir
            return;
        }

        // Hedef yoksa ateş edemez
        if (!hedefSecimi.HedefVarMi)
        {
            Debug.Log("[OyuncuAtes] Hedef seçili değil!");
            return;
        }

        Transform hedef = hedefSecimi.GuncelHedef;
        GulleTuru aktifGulleTuru = gulleSecimi.SeciliGulle;
        GulleOzellikleri gulleOzellikleri = GulleVeritabani.Getir(aktifGulleTuru);

        // Hedef geçerlilik kontrolü (NPC vs Oyuncu)
        bool hedefNpc = hedef.CompareTag("NPC");
        if (hedefNpc && !gulleOzellikleri.npcYeVurabilir)
        {
            Debug.Log($"[OyuncuAtes] {aktifGulleTuru} güllesi NPC'lere karşı kullanılamaz!");
            return;
        }

        // Stok kontrolü ve harcama (Sadece yeterli miktar varsa veya demire düşülürse ateşlenir)
        // GulleHarcaVeHasarKatsayisiHesapla Server'da çalıştığı için biz istemcide doğrudan komut gönderiyoruz
        // Stok düşme işlemi ve hasar hesaplaması sunucuda gerçekleşir.
        
        sonAtisZamani = Time.time;
        
        // Sunucuya atış komutu gönder
        CmdAtesEt(hedef.gameObject, aktifGulleTuru);
    }

    /// <summary>
    /// Buz güllesinin şifa etkisini sunucuda uygular.
    /// Gülle NetworkIdentity içermediğinden istemciden doğrudan [Server] metot çağrılamaz;
    /// bu nedenle ateş eden oyuncunun NetworkBehaviour'u üzerinden Command olarak iletilir.
    /// </summary>
    [Command]
    public void CmdSifaVer(GameObject hedefObj, int miktar)
    {
        if (hedefObj == null || miktar <= 0) return;

        // Oyuncu şifası
        OyuncuCanBari oyuncuCan = hedefObj.GetComponent<OyuncuCanBari>();
        if (oyuncuCan != null)
        {
            oyuncuCan.CanYenile(miktar);
            return;
        }

        // NPC şifası (Buz güllesi NPC'ye atıldığında)
        NpcHasar npcHasar = hedefObj.GetComponent<NpcHasar>();
        if (npcHasar != null)
        {
            npcHasar.CanYenile(miktar);
        }
    }

    /// <summary>
    /// Gülle hedefe ulaştığında hasar etkisini sunucuda uygular.
    /// </summary>
    [Command]
    public void CmdHasarVer(GameObject hedefObj, int miktar)
    {
        if (hedefObj == null || miktar <= 0) return;

        OyuncuCanBari oyuncuCan = hedefObj.GetComponent<OyuncuCanBari>();
        if (oyuncuCan != null)
        {
            oyuncuCan.HasarAl(miktar);
            return;
        }

        NpcHasar npcHasar = hedefObj.GetComponent<NpcHasar>();
        if (npcHasar != null)
        {
            npcHasar.HasarAl(miktar, netIdentity);
        }
    }

    [Command]
    private void CmdAtesEt(GameObject hedefObj, GulleTuru gulleTuru)
    {
        if (hedefObj == null) return;

        // Sunucu tarafında güvenlik ve stok kontrolü
        GulleOzellikleri gulleOzellikleri = GulleVeritabani.Getir(gulleTuru);
        bool hedefNpc = hedefObj.CompareTag("NPC");
        
        if (hedefNpc && !gulleOzellikleri.npcYeVurabilir) return;

        // Gülle stoktan düşülür ve hasar katsayısı alınır
        float hasarKatsayisi = gulleSecimi.GulleHarcaVeHasarKatsayisiHesapla();
        
        // Gülle turu "Demir" e düşmüş olabilir (Stok bittiyse), güncel olanı alalım
        GulleTuru gerceklesenGulleTuru = gulleSecimi.SeciliGulle;
        GulleOzellikleri gerceklesenGulleOzellikleri = GulleVeritabani.Getir(gerceklesenGulleTuru);

        // Gülle prefabını atanan diziden al
        GameObject gullePrefab = GetGullePrefab(gerceklesenGulleTuru);
        if (gullePrefab == null)
        {
            Debug.LogError($"[OyuncuAtes] {gerceklesenGulleTuru} için prefab atanmamış!");
            return;
        }

        // Hasarı hesapla: Temel Hasar * Katsayı (örn: yarım gülle varsa yarım hasar)
        int nihaiHasar = Mathf.RoundToInt(gemiTemelHasari * hasarKatsayisi);

        // Tüm istemcilerde ve sunucuda gülle uçuşunu görsel olarak başlat
        RpcGulleFirlat(hedefObj, gerceklesenGulleTuru, nihaiHasar);
    }

    [ClientRpc]
    private void RpcGulleFirlat(GameObject hedefObj, GulleTuru gulleTuru, int hasar)
    {
        if (hedefObj == null) return;

        GameObject gullePrefab = GetGullePrefab(gulleTuru);
        if (gullePrefab == null)
        {
            Debug.LogError($"[OyuncuAtes] {gulleTuru} için prefab atanmamış!");
            return;
        }

        Vector3 cikisPos = atisNoktasi != null ? atisNoktasi.position : transform.position;
        GameObject gulle = Instantiate(gullePrefab, cikisPos, Quaternion.identity);
        Gulle gulleScript = gulle.GetComponent<Gulle>();
        if (gulleScript != null)
        {
            gulleScript.Baslat(hedefObj.transform, hasar, netIdentity);
        }
    }

    private GameObject GetGullePrefab(GulleTuru tur)
    {
        return tur switch
        {
            GulleTuru.Demir => demirGullePrefab,
            GulleTuru.Alev => alevGullePrefab,
            GulleTuru.Buz => buzGullePrefab,
            GulleTuru.KuruKafa => kuruKafaGullePrefab,
            GulleTuru.Asit => asitGullePrefab,
            _ => demirGullePrefab
        };
    }
}
