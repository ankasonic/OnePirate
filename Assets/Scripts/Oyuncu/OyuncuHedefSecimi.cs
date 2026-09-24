using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// GDD ve Kullanıcı İstekleri:
/// - "npc nin üzerine mouse ile gelip sol tık yapınca ya da tab ile ekrandaki seçimler arasında hedefi değiştirir"
/// - "Ateş etme tuşu space tuşudur, hedefe kilitlendikten (Mouse sol tık ile hedefi seçtikten) sonra space tuşu ile ateş edilebilir."
/// - Mouse hover: Hedeflenebilir bir birimin üzerindeyken görsel geri bildirim.
/// - Tab tuşu: Yalnızca ekranda (kamera görüş alanında) olan canlı hedefler arasında sıralı geçiş yapar.
/// - Kırmızı hedef göstergesi: Kilitlenilen hedefin altında yumuşakça dönen kilit göstergesi.
/// - Mirror Network: Yalnızca local player için çalışır.
/// </summary>
public class OyuncuHedefSecimi : NetworkBehaviour
{
    [Header("Kamera")]
    [SerializeField] private Camera oyuncuKamerasi;

    [Header("Görsel Gösterge Ayarları")]
    [SerializeField] private Sprite hedefGostergesiSprite;
    [SerializeField] private Color kilitRengi = new Color(1f, 0.2f, 0.2f, 0.9f);       // Seçili hedef (Kırmızı)
    [SerializeField] private Color hoverRengi = new Color(1f, 0.85f, 0.2f, 0.6f);     // Üzerine gelinen (Sarı)
    [SerializeField] private float gostergeBoyutu = 1.3f;
    [SerializeField] private float donusHizi = 60f;
    [SerializeField] private Vector2 gostergeOfseti = new Vector2(0f, 0f);

    [Header("Seçim Ayarları")]
    [Tooltip("Boş denize tıklandığında hedef seçimi iptal edilsin mi? (Gemi sürüşünü kolaylaştırmak için varsayılan: false)")]
    [SerializeField] private bool boslugaTiklayincaHedefiTemizle = false;
    [Tooltip("Maksimum hedefleme mesafesi (Görüş / Atış menzili dışındaki hedefleri sınırlandırmak için)")]
    [SerializeField] private float maksimumHedefMesafesi = 30f;

    // Aktif Hedef Bilgileri
    private Transform guncelHedef;
    private Transform hoverHedef;

    // Gösterge Nesneleri
    private GameObject kilitGostergeObjesi;
    private SpriteRenderer kilitGostergeRenderer;

    private GameObject hoverGostergeObjesi;
    private SpriteRenderer hoverGostergeRenderer;

    // Public Erişim
    public Transform GuncelHedef => guncelHedef;
    public bool HedefVarMi => guncelHedef != null;
    public NpcHasar SeciliNpc => guncelHedef != null ? guncelHedef.GetComponent<NpcHasar>() : null;
    public OyuncuCanBari SeciliOyuncu => guncelHedef != null ? guncelHedef.GetComponent<OyuncuCanBari>() : null;

    public event Action<Transform> HedefDegistiOlayi;

    private void Awake()
    {
        GostergeleriOlustur();
    }

    private void Start()
    {
        if (!isLocalPlayer) return;

        KamerayiBul();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        KamerayiBul();
    }

    private void KamerayiBul()
    {
        if (oyuncuKamerasi == null)
        {
            var oyuncuKameraComp = GetComponent<OyuncuKamera>();
            if (oyuncuKameraComp != null && oyuncuKameraComp.HedefKamera != null)
            {
                oyuncuKamerasi = oyuncuKameraComp.HedefKamera;
            }
            else
            {
                oyuncuKamerasi = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (oyuncuKamerasi == null)
        {
            KamerayiBul();
            if (oyuncuKamerasi == null) return;
        }

        // 1. Hedefin Canlılığını ve Geçerliliğini Kontrol Et
        GuncelHedefiDogrula();

        // 2. Mouse ile Hedef Üzerine Gelme (Hover) Tespiti
        HoverTespitiYap();

        // 3. Mouse Sol Tık ile Seçim
        SolTikKontrol();

        // 4. Tab Tuşu ile Ekrandaki Hedefler Arasında Geçiş
        TabTusuKontrol();

        // 5. Escape Tuşu ile Hedefi Bırakma
        IptalTusuKontrol();

        // 6. Görsel Göstergelerin Pozisyon ve Animasyonlarını Güncelle
        GostergeleriGuncelle();
    }

    // -------------------------------------------------------------
    // MOUSE TIKLAMA İLE HEDEF SEÇİMİ
    // -------------------------------------------------------------
    private void SolTikKontrol()
    {
        bool solTik = false;
        if (Mouse.current != null)
            solTik = Mouse.current.leftButton.wasPressedThisFrame;
        

        if (!solTik) return;

        Vector2 fareDunyaPozisyonu = FareDunyaPozisyonunuAl();
        Collider2D hit = Physics2D.OverlapPoint(fareDunyaPozisyonu);

        if (hit != null)
        {
            Transform adayHedef = HedefKokuBul(hit);
            if (adayHedef != null && HedeflenebilirMi(adayHedef))
            {
                HedefSec(adayHedef);
                return;
            }
        }

        // Boş alana tıklandıysa ve ayar açıksa hedefi temizle
        if (boslugaTiklayincaHedefiTemizle)
        {
            HedefiTemizle();
        }
    }

    // -------------------------------------------------------------
    // MOUSE HOVER (ÜZERİNE GELME) GERİ BİLDİRİMİ
    // -------------------------------------------------------------
    private void HoverTespitiYap()
    {
        Vector2 fareDunyaPozisyonu = FareDunyaPozisyonunuAl();
        Collider2D hit = Physics2D.OverlapPoint(fareDunyaPozisyonu);

        if (hit != null)
        {
            Transform adayHedef = HedefKokuBul(hit);
            if (adayHedef != null && HedeflenebilirMi(adayHedef))
            {
                hoverHedef = adayHedef;
                return;
            }
        }

        hoverHedef = null;
    }

    // -------------------------------------------------------------
    // TAB TUŞU İLE EKRANDAKİ HEDEFLER ARASINDA GEZİNME
    // -------------------------------------------------------------
    private void TabTusuKontrol()
    {
        bool tabBasildi = false;
        if (Keyboard.current != null)
            tabBasildi = Keyboard.current.tabKey.wasPressedThisFrame;
        

        if (!tabBasildi) return;

        List<Transform> ekrandakiHedefler = EkrandakiGecerliHedefleriTopla();

        if (ekrandakiHedefler.Count == 0)
        {
            return;
        }

        // Mesafeye göre sırala (en yakından en uzağa)
        ekrandakiHedefler.Sort((a, b) =>
        {
            float mesafeA = Vector2.Distance(transform.position, a.position);
            float mesafeB = Vector2.Distance(transform.position, b.position);
            return mesafeA.CompareTo(mesafeB);
        });

        int secilecekIndeks = 0;

        // Mevcut hedef ekrandaki hedefler listesindeyse bir sonrakine geç
        if (guncelHedef != null)
        {
            int mevcutIndeks = ekrandakiHedefler.IndexOf(guncelHedef);
            if (mevcutIndeks != -1)
            {
                secilecekIndeks = (mevcutIndeks + 1) % ekrandakiHedefler.Count;
            }
        }

        HedefSec(ekrandakiHedefler[secilecekIndeks]);
    }

    private void IptalTusuKontrol()
    {
        bool escBasildi = false;
        if (Keyboard.current != null)
            escBasildi = Keyboard.current.escapeKey.wasPressedThisFrame;
        

        if (escBasildi && guncelHedef != null)
        {
            HedefiTemizle();
        }
    }

    // -------------------------------------------------------------
    // EKRANDAKİ HEDEFLERİ FİLTRELEME
    // -------------------------------------------------------------
    private List<Transform> EkrandakiGecerliHedefleriTopla()
    {
        List<Transform> adaylar = new List<Transform>();

        // 1. NPC'leri tara
        NpcHasar[] npcler = FindObjectsByType<NpcHasar>(FindObjectsSortMode.None);
        foreach (var npc in npcler)
        {
            if (npc != null && HedeflenebilirMi(npc.transform))
            {
                if (EkrandaMi(npc.transform.position))
                {
                    adaylar.Add(npc.transform);
                }
            }
        }

        // 2. Diğer Oyuncuları tara
        OyuncuCanBari[] oyuncular = FindObjectsByType<OyuncuCanBari>(FindObjectsSortMode.None);
        foreach (var oyuncu in oyuncular)
        {
            if (oyuncu != null && oyuncu.transform != transform && HedeflenebilirMi(oyuncu.transform))
            {
                if (EkrandaMi(oyuncu.transform.position))
                {
                    adaylar.Add(oyuncu.transform);
                }
            }
        }

        return adaylar;
    }

    private bool EkrandaMi(Vector3 dunyaPozisyonu)
    {
        if (oyuncuKamerasi == null) return false;

        // Kameranın ekran koordinatlarına çevir (0 ile 1 aralığında olmalı)
        Vector3 viewPos = oyuncuKamerasi.WorldToViewportPoint(dunyaPozisyonu);
        bool gorunur = viewPos.x >= 0.02f && viewPos.x <= 0.98f &&
                       viewPos.y >= 0.02f && viewPos.y <= 0.98f &&
                       viewPos.z > 0f;

        if (!gorunur) return false;

        // Çok uzaktaysa hedeflemeye dahil etme
        float mesafe = Vector2.Distance(transform.position, dunyaPozisyonu);
        return mesafe <= maksimumHedefMesafesi;
    }

    // -------------------------------------------------------------
    // HEDEF GEÇERLİLİK KURALLARI
    // -------------------------------------------------------------
    public bool HedeflenebilirMi(Transform hedef)
    {
        if (hedef == null) return false;
        if (hedef == transform) return false; // Kendini hedef alamaz
        if (!hedef.gameObject.activeInHierarchy) return false;

        // Hayalet / Gizli yönetici kontrolü
        if (hedef.name.Contains("Hayalet") || hedef.CompareTag("Admin")) return false;

        // NPC Can Kontrolü
        if (hedef.TryGetComponent<NpcHasar>(out var npc))
        {
            return npc.GuncelCan > 0;
        }

        // Oyuncu Can Kontrolü
        if (hedef.TryGetComponent<OyuncuCanBari>(out var oyuncu))
        {
            return oyuncu.GuncelCan > 0;
        }

        // Kule veya genel hedef kontrolü
        if (hedef.CompareTag("Kule") || hedef.CompareTag("NPC") || hedef.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    private Transform HedefKokuBul(Collider2D collider)
    {
        if (collider == null) return null;

        // NPC bileşeni
        var npc = collider.GetComponentInParent<NpcHasar>();
        if (npc != null) return npc.transform;

        // Oyuncu bileşeni
        var oyuncu = collider.GetComponentInParent<OyuncuCanBari>();
        if (oyuncu != null && oyuncu.transform != transform) return oyuncu.transform;

        // Tag kontrolü
        if (collider.CompareTag("NPC") || collider.CompareTag("Kule") || collider.CompareTag("Player"))
        {
            return collider.transform.root != transform ? collider.transform.root : null;
        }

        return null;
    }

    private void GuncelHedefiDogrula()
    {
        if (guncelHedef != null)
        {
            if (!HedeflenebilirMi(guncelHedef))
            {
                HedefiTemizle();
            }
        }
    }

    // -------------------------------------------------------------
    // HEDEF SEÇME VE TEMİZLEME METOTLARI
    // -------------------------------------------------------------
    public void HedefSec(Transform yeniHedef)
    {
        if (yeniHedef == guncelHedef) return;

        guncelHedef = yeniHedef;
        HedefDegistiOlayi?.Invoke(guncelHedef);
    }

    public void HedefiTemizle()
    {
        if (guncelHedef == null) return;

        guncelHedef = null;
        HedefDegistiOlayi?.Invoke(null);
    }

    // -------------------------------------------------------------
    // GÖRSEL HEDEF GÖSTERGELERİ (KİLİT & HOVER ÇEMBERİ)
    // -------------------------------------------------------------
    private void GostergeleriOlustur()
    {
        // 1. Kilit Göstergesi (Kırmızı Çember)
        kilitGostergeObjesi = new GameObject("[HedefKilitGostergesi]");
        kilitGostergeObjesi.transform.SetParent(transform, false);
        kilitGostergeRenderer = kilitGostergeObjesi.AddComponent<SpriteRenderer>();
        kilitGostergeRenderer.sortingOrder = 5; // Su üstü, gemi altı
        kilitGostergeRenderer.color = kilitRengi;
        kilitGostergeObjesi.transform.localScale = Vector3.one * gostergeBoyutu;
        kilitGostergeObjesi.SetActive(false);

        // 2. Hover Göstergesi (Sarı Çember)
        hoverGostergeObjesi = new GameObject("[HedefHoverGostergesi]");
        hoverGostergeObjesi.transform.SetParent(transform, false);
        hoverGostergeRenderer = hoverGostergeObjesi.AddComponent<SpriteRenderer>();
        hoverGostergeRenderer.sortingOrder = 4;
        hoverGostergeRenderer.color = hoverRengi;
        hoverGostergeObjesi.transform.localScale = Vector3.one * (gostergeBoyutu * 1.15f);
        hoverGostergeObjesi.SetActive(false);

        GostergeSpriteGuncelle();
    }

    private void GostergeSpriteGuncelle()
    {
        if (hedefGostergesiSprite != null)
        {
            if (kilitGostergeRenderer != null) kilitGostergeRenderer.sprite = hedefGostergesiSprite;
            if (hoverGostergeRenderer != null) hoverGostergeRenderer.sprite = hedefGostergesiSprite;
        }
    }

    private void GostergeleriGuncelle()
    {
        // 1. Kilitli Hedef Göstergesi
        if (guncelHedef != null)
        {
            if (!kilitGostergeObjesi.activeSelf) kilitGostergeObjesi.SetActive(true);

            Vector3 hedefPos = guncelHedef.position + (Vector3)gostergeOfseti;
            hedefPos.z = 0f;
            kilitGostergeObjesi.transform.position = hedefPos;
            kilitGostergeObjesi.transform.Rotate(0f, 0f, donusHizi * Time.deltaTime);

            // Hafif canlı nabız (pulse) efekti
            float pulse = 1f + (Mathf.Sin(Time.time * 6f) * 0.05f);
            kilitGostergeObjesi.transform.localScale = Vector3.one * (gostergeBoyutu * pulse);
        }
        else
        {
            if (kilitGostergeObjesi.activeSelf) kilitGostergeObjesi.SetActive(false);
        }

        // 2. Hover Göstergesi (Kilitli hedefle aynı değilse göster)
        if (hoverHedef != null && hoverHedef != guncelHedef)
        {
            if (!hoverGostergeObjesi.activeSelf) hoverGostergeObjesi.SetActive(true);

            Vector3 hoverPos = hoverHedef.position + (Vector3)gostergeOfseti;
            hoverPos.z = 0f;
            hoverGostergeObjesi.transform.position = hoverPos;
            hoverGostergeObjesi.transform.Rotate(0f, 0f, -donusHizi * 0.5f * Time.deltaTime);
        }
        else
        {
            if (hoverGostergeObjesi.activeSelf) hoverGostergeObjesi.SetActive(false);
        }
    }

    private Vector2 FareDunyaPozisyonunuAl()
    {
        if (oyuncuKamerasi == null) return Vector2.zero;

        Vector3 mouseScreenPos = Vector3.zero;
        if (Mouse.current != null)
            mouseScreenPos = Mouse.current.position.ReadValue();
        

        mouseScreenPos.z = -oyuncuKamerasi.transform.position.z;
        return oyuncuKamerasi.ScreenToWorldPoint(mouseScreenPos);
    }

    public void GostergeSpriteAyarla(Sprite sprite)
    {
        hedefGostergesiSprite = sprite;
        GostergeSpriteGuncelle();
    }
}

