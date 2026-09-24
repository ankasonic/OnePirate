using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class OyuncuCanBari : NetworkBehaviour
{
    [Header("UI Bileşenleri")]
    [SerializeField] private Slider canSlider;
    [SerializeField] private Image canDolgusu;
    [SerializeField] private RectTransform canDolgusuRect;
    [SerializeField] private TMP_Text canYazisi;

    // Seviye bazlı Max HP tablosu (GDD uyumlu)
    public static readonly int[] SeviyeMaksHpTablosu = {
        10000, // 1. Seviye
        11000, // 2. Seviye
        13500, // 3. Seviye
        16500, // 4. Seviye
        20000, // 5. Seviye
        24000, // 6. Seviye
        29000, // 7. Seviye
        35000, // 8. Seviye
        42000, // 9. Seviye
        50000  // 10. Seviye
    };

    [Header("Can Değerleri")]
    [SyncVar(hook = nameof(OnGuncelCanDegisti))]
    [SerializeField] private int guncelCan = 10000;

    [SyncVar(hook = nameof(OnMaksCanDegisti))]
    [SerializeField] private int maksCan = 10000;

    public int GuncelCan => guncelCan;
    public int MaksCan => maksCan;

    private void Awake()
    {
        BilesenleriOtomatikBul();
    }

    private void Start()
    {
        ArayuzuGuncelle();
    }

    private void Reset()
    {
        BilesenleriOtomatikBul();
    }

    public void BilesenleriAyarla(Slider slider, Image dolgu, RectTransform dolguRect, TMP_Text yazi)
    {
        canSlider = slider;
        canDolgusu = dolgu;
        canDolgusuRect = dolguRect;
        canYazisi = yazi;
        ArayuzuGuncelle();
    }

    private void BilesenleriOtomatikBul()
    {
        if (canSlider == null)
            canSlider = GetComponentInChildren<Slider>(true);

        // Dolgu rect ve image artık slider.fillRect'ten değil, hiyerarşiden bulunur
        if (canDolgusuRect == null)
        {
            // "Dolgu" adındaki RectTransform'u ara
            var tumRectler = GetComponentsInChildren<RectTransform>(true);
            foreach (var r in tumRectler)
            {
                if (r.name == "Dolgu" && r.GetComponent<Image>() != null)
                {
                    canDolgusuRect = r;
                    canDolgusu = r.GetComponent<Image>();
                    break;
                }
            }
        }
        if (canDolgusu == null && canDolgusuRect != null)
            canDolgusu = canDolgusuRect.GetComponent<Image>();

        if (canYazisi == null && canSlider != null)
            canYazisi = canSlider.GetComponentInChildren<TMP_Text>(true);
    }

    private void OnGuncelCanDegisti(int eskiCan, int yeniCan)
    {
        ArayuzuGuncelle();
    }

    private void OnMaksCanDegisti(int eskiMaks, int yeniMaks)
    {
        ArayuzuGuncelle();
    }

    public void ArayuzuGuncelle()
    {
        float oran = maksCan > 0 ? Mathf.Clamp01((float)guncelCan / maksCan) : 0f;

        // GDD Kuralı: Can azaldıkça bar hem soldan hem sağdan (merkeze doğru) eşit küçülür
        // Slider kullanılmıyor — RectTransform anchor'ları ile doğrudan yönetilir
        if (canDolgusuRect != null)
        {
            canDolgusuRect.anchorMin = new Vector2(0.5f - (oran / 2f), 0f);
            canDolgusuRect.anchorMax = new Vector2(0.5f + (oran / 2f), 1f);
            canDolgusuRect.offsetMin = Vector2.zero;
            canDolgusuRect.offsetMax = Vector2.zero;
        }

        // GDD Kuralı: %100 Yeşil -> %50 Sarı -> %0 Kırmızı yumuşak (Color.Lerp) geçiş
        if (canDolgusu != null)
        {
            Color barRengi;
            if (oran >= 0.5f)
            {
                float t = (oran - 0.5f) / 0.5f;
                barRengi = Color.Lerp(Color.yellow, new Color(0.18f, 0.85f, 0.25f, 1f), t);
            }
            else
            {
                float t = oran / 0.5f;
                barRengi = Color.Lerp(new Color(0.9f, 0.15f, 0.15f, 1f), Color.yellow, t);
            }
            canDolgusu.color = barRengi;
        }

        // Can değeri yazısı gizlenir
        if (canYazisi != null)
        {
            canYazisi.gameObject.SetActive(false);
        }
    }

    [Server]
    public void HasarAl(int hasarMiktari)
    {
        if (hasarMiktari <= 0) return;
        guncelCan = Mathf.Clamp(guncelCan - hasarMiktari, 0, maksCan);
    }

    [Server]
    public void CanYenile(int miktar)
    {
        if (miktar <= 0) return;
        guncelCan = Mathf.Clamp(guncelCan + miktar, 0, maksCan);
    }

    [Server]
    public void CanAyarla(int yeniCan, int yeniMaksCan)
    {
        maksCan = Mathf.Max(1, yeniMaksCan);
        guncelCan = Mathf.Clamp(yeniCan, 0, maksCan);
    }

    [Server]
    public void SeviyeyeGoreMaksCanAyarla(int seviye)
    {
        int index = Mathf.Clamp(seviye - 1, 0, SeviyeMaksHpTablosu.Length - 1);
        int yeniMaks = SeviyeMaksHpTablosu[index];
        maksCan = yeniMaks;
        guncelCan = Mathf.Min(guncelCan, maksCan);
    }

    // Yerel ve çevrimdışı testler için
    public void TestCanAyarla(int yeniCan, int yeniMaksCan)
    {
        maksCan = Mathf.Max(1, yeniMaksCan);
        guncelCan = Mathf.Clamp(yeniCan, 0, maksCan);
        ArayuzuGuncelle();
    }
}
