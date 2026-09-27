using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// GDD Kuralları:
/// - "Wasd ile haritada kamera ile gezinir."
/// - "Mouse tekerlekleri ile kamerada zoom yapılabilir."
/// - Mirror Network uyumlu: Sadece local player'ın kamerasını yönetir.
/// - Gemi takibi ve serbest kamera modları arasında geçişi destekler.
/// </summary>
public class OyuncuKamera : NetworkBehaviour
{
    [Header("Kamera Referansı")]
    [SerializeField] private Camera hedefKamera;
    [SerializeField] private float kameraZ = -10f;

    [Header("Gemi Takip Ayarları")]
    [Tooltip("Kameranın gemiyi takip ederkenki yumuşaklık katsayısı")]
    [SerializeField] private float takipYumusatma = 0f;
    [Tooltip("Gemiye kilitlenildiğinde uygulanan pozisyon ofseti")]
    [SerializeField] private Vector2 takipOfseti = Vector2.zero;

    [Header("Serbest Gezinme (WASD) Ayarları")]
    [Tooltip("WASD ile haritada gezinme açık mı?")]
    [SerializeField] private bool wasdGezinmeAktif = true;
    [Tooltip("WASD ile haritada kameranın hareket hızı")]
    [SerializeField] private float panHizi = 120f;
    [Tooltip("WASD hareketin pürüzsüzlüğü için yumuşatma katsayısı")]
    [SerializeField] private float panYumusatma = 0f;
    [Tooltip("Oyuncu mouse ile gemiyi hareket ettirdiğinde kamera otomatik gemiye dönsün mü?")]
    [SerializeField] private bool hareketEttigindeOdaklan = true;

    [Header("Zoom Ayarları (Mouse Tekerleği)")]
    [SerializeField] private bool zoomAktif = true;
    [SerializeField] private float minZoom = 4f;       // En yakın
    [SerializeField] private float maxZoom = 10f;      // En uzak
    [SerializeField] private float varsayilanZoom = 8f;
    [SerializeField] private float zoomAdimi = 1.5f;

    [Header("Harita Sınırları (Opsiyonel)")]
    [SerializeField] private bool sinirlarAktif = true;
    [SerializeField] private Vector2 minSinirlar = new Vector2(-50f, -70f);
    [SerializeField] private Vector2 maxSinirlar = new Vector2(80f, 60f);

    private Vector3 kameraHedefPozisyon;
    private Vector3 panHizi_Hız = Vector3.zero;
    private float hedefZoom;
    private bool serbestModda = false;

    public bool SerbestModda => serbestModda;
    public Camera HedefKamera => hedefKamera;

    private void Start()
    {
        if (!isLocalPlayer) return;

        KameraHazirla();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        KameraHazirla();
    }

    private void KameraHazirla()
    {
        if (hedefKamera == null)
        {
            hedefKamera = Camera.main;
            if (hedefKamera == null)
            {
                hedefKamera = FindFirstObjectByType<Camera>();
            }
        }

        if (hedefKamera != null)
        {
            hedefZoom = Mathf.Clamp(varsayilanZoom, minZoom, maxZoom);
            hedefKamera.orthographic = true;
            hedefKamera.orthographicSize = hedefZoom;

            // Kamerayı direkt oyuncunun üzerine ışınla (ani kayma olmasın)
            AnindaGemiyeOdakla();
        }
    }

    private void LateUpdate()
    {
        // Yalnızca bu istemciye ait oyuncu kamerayı kontrol edebilir
        if (!isLocalPlayer) return;

        if (hedefKamera == null)
        {
            KameraHazirla();
            if (hedefKamera == null) return;
        }

        GirdileriIsle();
        ZoomGuncelle();
        KamerayiGuncelle();
    }

    private void GirdileriIsle()
    {
        Vector2 hareketGirdisi = Vector2.zero;

        // 1. WASD / Yön Tuşları ile Haritada Gezinme (Yeni Input System + Fallback)
        if (wasdGezinmeAktif)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) hareketGirdisi.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) hareketGirdisi.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) hareketGirdisi.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) hareketGirdisi.x += 1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) hareketGirdisi.y += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) hareketGirdisi.y -= 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) hareketGirdisi.x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) hareketGirdisi.x += 1f;
            }
        }

        // Pürüzsüz hareket için hedef hızı hesapla
        Vector3 hedefHiz = (Vector3)(hareketGirdisi.normalized * panHizi);

        // Eğer girdi varsa hedef hıza doğru hızlandir, yoksa yavaşla
        if (hareketGirdisi.sqrMagnitude > 0.01f)
        {
            serbestModda = true;
            panHizi_Hız = Vector3.Lerp(panHizi_Hız, hedefHiz, panYumusatma * Time.deltaTime);
        }
        else
        {
            // Girdi yoksa yavaşça durur
            panHizi_Hız = Vector3.Lerp(panHizi_Hız, Vector3.zero, panYumusatma * Time.deltaTime);
        }

        // Kamera pozisyonunu hız vektörüne göre güncelle
        kameraHedefPozisyon += panHizi_Hız * Time.deltaTime;

        // 2. Kamerayı Tekrar Gemiye Kilitleme (C tuşu veya Mouse ile gemi hareket komutu)
        bool odaklanIstendi = false;

        if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
        {
            odaklanIstendi = true;
        }

       if (hareketEttigindeOdaklan)
{
    // Tıklama yerine geminin Gerçek Hareketini (Rigidbody2D hızı veya girdi tuşlarını) kontrol edin
    // Örnek: Gemide hareket varken serbest modu kapat
    if (TryGetComponent<Rigidbody2D>(out var rb) && rb.linearVelocity.sqrMagnitude > 0.1f)
    {
        serbestModda = false;

}

        }

        if (odaklanIstendi)
        {
            serbestModda = false;
        }

        // Eğer serbest modda değilsek kameranın hedefi doğrudan geminin pozisyonudur
        if (!serbestModda)
        {
            panHizi_Hız = Vector3.zero;
            kameraHedefPozisyon = new Vector3(
                transform.position.x + takipOfseti.x,
                transform.position.y + takipOfseti.y,
                kameraZ
            );
        }
    }

    private void KamerayiGuncelle()
    {
        // Harita sınırları varsa uygula
        if (sinirlarAktif)
        {
            if (!HaritaSiniri.SinirlarHesaplandi)
            {
                HaritaSiniri.Hesapla();
            }

            Vector2 min = HaritaSiniri.SinirlarHesaplandi ? HaritaSiniri.MinSinir : minSinirlar;
            Vector2 max = HaritaSiniri.SinirlarHesaplandi ? HaritaSiniri.MaxSinir : maxSinirlar;

            float yPadding = hedefKamera.orthographicSize;
            float xPadding = yPadding * hedefKamera.aspect;

            // Eğer zoom çok fazla açıldıysa padding sınırları geçebilir, bunu korumak için:
            float xMin = min.x + xPadding;
            float xMax = max.x - xPadding;
            if (xMin > xMax) xMin = xMax = (min.x + max.x) / 2f;

            float yMin = min.y + yPadding;
            float yMax = max.y - yPadding;
            if (yMin > yMax) yMin = yMax = (min.y + max.y) / 2f;

            kameraHedefPozisyon.x = Mathf.Clamp(kameraHedefPozisyon.x, xMin, xMax);
            kameraHedefPozisyon.y = Mathf.Clamp(kameraHedefPozisyon.y, yMin, yMax);
        }
        kameraHedefPozisyon.z = kameraZ;

        // Serbest modda (WASD) ani hareket, gemiye takip modunda yumuşak hareket
        if (serbestModda)
        {
            hedefKamera.transform.position = kameraHedefPozisyon;
        }
        else
        {
            hedefKamera.transform.position = Vector3.Lerp(hedefKamera.transform.position, kameraHedefPozisyon, takipYumusatma * Time.deltaTime);
        }
    }

    private void ZoomGuncelle()
    {
        if (!zoomAktif) return;

        // Harita sınırlarına göre kameranın ekran dışını asla göremeyeceği dinamik maksimum zoom tavanı
        Vector2 min = HaritaSiniri.SinirlarHesaplandi ? HaritaSiniri.MinSinir : minSinirlar;
        Vector2 max = HaritaSiniri.SinirlarHesaplandi ? HaritaSiniri.MaxSinir : maxSinirlar;
        float haritaGenislik = Mathf.Max(10f, max.x - min.x);
        float haritaYukseklik = Mathf.Max(10f, max.y - min.y);
        float aspect = hedefKamera.aspect > 0.01f ? hedefKamera.aspect : (16f / 9f);

        // Pencere büyütülse/küçültülse dahi kameranın denizin dışını göstermesini engelleyen tavan:
        float maxGuvenliZoom = Mathf.Min(haritaGenislik / (2f * aspect), haritaYukseklik / 2f);
        float dinamikMaxZoom = Mathf.Min(maxZoom, maxGuvenliZoom);

        float scroll = 0f;

        // Mouse Tekerleği Algılama
        if (Mouse.current != null)
        {
            float y = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(y) > 0.01f)
                scroll = Mathf.Sign(y);
        }

        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Scroll ileri (+1) -> Yakınlaş (orthographicSize küçülür)
            // Scroll geri (-1) -> Uzaklaş (orthographicSize büyür)
            hedefZoom = Mathf.Clamp(hedefZoom - (scroll * zoomAdimi), minZoom, dinamikMaxZoom);
        }
        else
        {
            hedefZoom = Mathf.Clamp(hedefZoom, minZoom, dinamikMaxZoom);
        }

        // Anında doğrudan uygula
        hedefKamera.orthographicSize = hedefZoom;

        // Zoom değiştiğinde kamera pozisyonunu sınırlara göre yeniden kontrol et
        // (köşedeyken zoom yapıldığında harita dışını göstermemeyi sağlar)
        if (sinirlarAktif)
        {
            float yPadding = hedefKamera.orthographicSize;
            float xPadding = yPadding * hedefKamera.aspect;

            float xMin = min.x + xPadding;
            float xMax = max.x - xPadding;
            if (xMin > xMax) xMin = xMax = (min.x + max.x) / 2f;

            float yMin = min.y + yPadding;
            float yMax = max.y - yPadding;
            if (yMin > yMax) yMin = yMax = (min.y + max.y) / 2f;

            Vector3 currentPos = hedefKamera.transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, xMin, xMax);
            currentPos.y = Mathf.Clamp(currentPos.y, yMin, yMax);
            hedefKamera.transform.position = currentPos;
        }
    }

    /// <summary>
    /// Kamerayı yumuşak bir şekilde oyuncunun gemisine geri kilitler.
    /// </summary>
    public void GemiyiOdakla()
    {
        serbestModda = false;
    }

    /// <summary>
    /// Kamerayı ani kayma olmadan direkt geminin pozisyonuna ışınlar.
    /// </summary>
    public void AnindaGemiyeOdakla()
    {
        serbestModda = false;
        kameraHedefPozisyon = new Vector3(
            transform.position.x + takipOfseti.x,
            transform.position.y + takipOfseti.y,
            kameraZ
        );

        if (hedefKamera != null)
        {
            hedefKamera.transform.position = kameraHedefPozisyon;
        }
    }
}

