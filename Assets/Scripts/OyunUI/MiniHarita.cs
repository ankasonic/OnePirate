using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

/// <summary>
/// GDD Minimap:
/// - Sol üstte sabit boyutlu bilgilendirme paneli (tıklanamaz / büyütülemez).
/// - Kara siyah, deniz turkuaz; sınırlar her zaman görünür.
/// - Görüş alanı (15) içindeki hedefler: kendi gemi yeşil, NPC beyaz,
///   diğer oyuncular kırmızı, kuleler pembe.
/// - Altta harita kodu + koordinat (ör. "2-1  |  AV 11").
/// </summary>
public class MiniHarita : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage haritaGorsel;
    [SerializeField] private RawImage isaretGorsel;
    [SerializeField] private TextMeshProUGUI altBilgiText;
    [SerializeField] private int dokuBoyutu = 256;
    [SerializeField] private float guncellemeAraligi = 0.05f;

    [Header("Görüş")]
    [SerializeField] private float gorusAlani = 15f;

    [Header("Renkler (GDD)")]
    [SerializeField] private Color denizRengi = new Color(0.25f, 0.78f, 0.78f, 1f); // turkuaz
    [SerializeField] private Color karaRengi = Color.black;
    [SerializeField] private Color kendiGemiRengi = Color.green;
    [SerializeField] private Color npcRengi = Color.white;
    [SerializeField] private Color digerOyuncuRengi = Color.red;
    [SerializeField] private Color kuleRengi = new Color(1f, 0.4f, 0.75f, 1f); // pembe
    [SerializeField] private Color sinirRengi = new Color(0.1f, 0.1f, 0.15f, 1f);

    private Texture2D haritaDokusu;
    private Texture2D isaretDokusu;
    private Color32[] isaretPikselleri;
    private float zamanlayici;
    private Transform yerelOyuncu;
    private Vector2 minSinir;
    private Vector2 maxSinir;
    private bool haritaHazir;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TiklamayiEngelle();
        HaritayiYenidenKur();
    }

    private void OnDestroy()
    {
        if (haritaDokusu != null) Destroy(haritaDokusu);
        if (isaretDokusu != null) Destroy(isaretDokusu);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        yerelOyuncu = null;
        HaritayiYenidenKur();
    }

    private void Update()
    {
        zamanlayici += Time.deltaTime;
        if (zamanlayici < guncellemeAraligi) return;
        zamanlayici = 0f;

        if (!haritaHazir)
            HaritayiYenidenKur();

        IsaretleriGuncelle();
        AltBilgiyiGuncelle();
    }

    private void TiklamayiEngelle()
    {
        // Raycast açık kalsın ki tıklama dünyaya geçmesin;
        // hareket/saldırı dinleyicisi yok → yalnızca tık yutar.
        if (haritaGorsel != null) haritaGorsel.raycastTarget = true;
        if (isaretGorsel != null) isaretGorsel.raycastTarget = false;
        if (altBilgiText != null) altBilgiText.raycastTarget = false;

        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    private void HaritayiYenidenKur()
    {
        HaritaSiniri.Hesapla();
        if (!HaritaSiniri.SinirlarHesaplandi) return;

        minSinir = HaritaSiniri.MinSinir;
        maxSinir = HaritaSiniri.MaxSinir;

        HaritaZemininiPisir();
        IsaretDokusuHazirla();
        haritaHazir = true;
    }

    private void HaritaZemininiPisir()
    {
        if (haritaDokusu == null || haritaDokusu.width != dokuBoyutu)
        {
            if (haritaDokusu != null) Destroy(haritaDokusu);
            haritaDokusu = new Texture2D(dokuBoyutu, dokuBoyutu, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (haritaGorsel != null)
                haritaGorsel.texture = haritaDokusu;
        }

        var kara = GameObject.Find("Kara")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        Color32[] pikseller = new Color32[dokuBoyutu * dokuBoyutu];

        float genislik = Mathf.Max(0.01f, maxSinir.x - minSinir.x);
        float yukseklik = Mathf.Max(0.01f, maxSinir.y - minSinir.y);

        for (int y = 0; y < dokuBoyutu; y++)
        {
            for (int x = 0; x < dokuBoyutu; x++)
            {
                // Kenar çerçevesi (harita sınırı)
                if (x == 0 || y == 0 || x == dokuBoyutu - 1 || y == dokuBoyutu - 1)
                {
                    pikseller[y * dokuBoyutu + x] = sinirRengi;
                    continue;
                }

                float u = x / (float)(dokuBoyutu - 1);
                float v = y / (float)(dokuBoyutu - 1);
                Vector3 dunya = new Vector3(minSinir.x + u * genislik, minSinir.y + v * yukseklik, 0f);

                bool karaMi = false;
                if (kara != null)
                {
                    Vector3Int hucre = kara.WorldToCell(dunya);
                    karaMi = kara.HasTile(hucre);
                }

                pikseller[y * dokuBoyutu + x] = karaMi ? (Color32)karaRengi : (Color32)denizRengi;
            }
        }

        haritaDokusu.SetPixels32(pikseller);
        haritaDokusu.Apply(false);
    }

    private void IsaretDokusuHazirla()
    {
        if (isaretDokusu == null || isaretDokusu.width != dokuBoyutu)
        {
            if (isaretDokusu != null) Destroy(isaretDokusu);
            isaretDokusu = new Texture2D(dokuBoyutu, dokuBoyutu, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (isaretGorsel != null)
                isaretGorsel.texture = isaretDokusu;
        }

        isaretPikselleri = new Color32[dokuBoyutu * dokuBoyutu];
    }

    private void IsaretleriGuncelle()
    {
        if (isaretDokusu == null || isaretPikselleri == null) return;

        // Şeffaf temizle
        Color32 seffaf = new Color32(0, 0, 0, 0);
        for (int i = 0; i < isaretPikselleri.Length; i++)
            isaretPikselleri[i] = seffaf;

        if (yerelOyuncu == null)
            YerelOyuncuyuBul();

        if (yerelOyuncu == null)
        {
            isaretDokusu.SetPixels32(isaretPikselleri);
            isaretDokusu.Apply(false);
            return;
        }

        Vector3 merkez = yerelOyuncu.position;
        float gorusKare = gorusAlani * gorusAlani;

        // Kendi gemisi — görüşten bağımsız (her zaman)
        NoktaCiz(merkez, kendiGemiRengi, 3);

        // Diğer oyuncular
        var oyuncular = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < oyuncular.Length; i++)
        {
            var go = oyuncular[i];
            if (go == null || go.transform == yerelOyuncu) continue;
            if ((go.transform.position - merkez).sqrMagnitude > gorusKare) continue;
            NoktaCiz(go.transform.position, digerOyuncuRengi, 2);
        }

        // NPC'ler
        var npcler = GameObject.FindGameObjectsWithTag("NPC");
        for (int i = 0; i < npcler.Length; i++)
        {
            var go = npcler[i];
            if (go == null) continue;
            if ((go.transform.position - merkez).sqrMagnitude > gorusKare) continue;
            NoktaCiz(go.transform.position, npcRengi, 2);
        }

        // Kuleler
        var kuleler = GameObject.FindGameObjectsWithTag("Kule");
        for (int i = 0; i < kuleler.Length; i++)
        {
            var go = kuleler[i];
            if (go == null) continue;
            if ((go.transform.position - merkez).sqrMagnitude > gorusKare) continue;
            NoktaCiz(go.transform.position, kuleRengi, 2);
        }

        isaretDokusu.SetPixels32(isaretPikselleri);
        isaretDokusu.Apply(false);
    }

    private void NoktaCiz(Vector3 dunyaPos, Color renk, int yaricap)
    {
        if (!DunyaToPiksel(dunyaPos, out int cx, out int cy)) return;

        Color32 c = renk;
        int r2 = yaricap * yaricap;
        for (int dy = -yaricap; dy <= yaricap; dy++)
        {
            for (int dx = -yaricap; dx <= yaricap; dx++)
            {
                if (dx * dx + dy * dy > r2) continue;
                int x = cx + dx;
                int y = cy + dy;
                if (x < 0 || y < 0 || x >= dokuBoyutu || y >= dokuBoyutu) continue;
                isaretPikselleri[y * dokuBoyutu + x] = c;
            }
        }
    }

    private bool DunyaToPiksel(Vector3 dunyaPos, out int px, out int py)
    {
        float genislik = maxSinir.x - minSinir.x;
        float yukseklik = maxSinir.y - minSinir.y;
        if (genislik <= 0.01f || yukseklik <= 0.01f)
        {
            px = py = 0;
            return false;
        }

        float u = (dunyaPos.x - minSinir.x) / genislik;
        float v = (dunyaPos.y - minSinir.y) / yukseklik;
        px = Mathf.RoundToInt(Mathf.Clamp01(u) * (dokuBoyutu - 1));
        py = Mathf.RoundToInt(Mathf.Clamp01(v) * (dokuBoyutu - 1));
        return true;
    }

    private void AltBilgiyiGuncelle()
    {
        if (altBilgiText == null) return;

        string haritaKodu = HaritaKodunuAl();
        string koordinat = "--";

        if (yerelOyuncu == null)
            YerelOyuncuyuBul();

        if (yerelOyuncu != null)
        {
            if (!HaritaSiniri.SinirlarHesaplandi)
                HaritaSiniri.Hesapla();
            koordinat = KoordinatSistemi.Formatla(yerelOyuncu.position);
        }

        altBilgiText.text = $"{haritaKodu}  |  {koordinat}";
    }

    private static string HaritaKodunuAl()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            if (s.name == "Oyun_UI" || s.name == "Baslangic" || s.name == "Login") continue;
            return s.name;
        }
        return "--";
    }

    private void YerelOyuncuyuBul()
    {
        if (NetworkClient.localPlayer != null)
        {
            yerelOyuncu = NetworkClient.localPlayer.transform;
            return;
        }

        var oyuncu = GameObject.FindGameObjectWithTag("Player");
        if (oyuncu != null)
            yerelOyuncu = oyuncu.transform;
    }
}
