using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

/// <summary>
/// Yerel oyuncu gemisinin harita koordinatını (ör. AA-11) UI'da gösterir.
/// Her harita değişiminde HaritaSiniri yeniden hesaplandığı için otomatik güncellenir.
/// </summary>
public class KoordinatGosterge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI koordinatText;
    [SerializeField] private float guncellemeAraligi = 0.1f;

    private float zamanlayici;
    private Transform yerelOyuncu;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Yeni haritada sınırlar ve oyuncu referansı tazelensin
        yerelOyuncu = null;
        HaritaSiniri.Hesapla();
        Guncelle();
    }

    private void Update()
    {
        zamanlayici += Time.deltaTime;
        if (zamanlayici < guncellemeAraligi) return;
        zamanlayici = 0f;
        Guncelle();
    }

    private void Guncelle()
    {
        if (koordinatText == null) return;

        if (yerelOyuncu == null)
            YerelOyuncuyuBul();

        if (yerelOyuncu == null)
        {
            koordinatText.text = "--";
            return;
        }

        if (!HaritaSiniri.SinirlarHesaplandi)
            HaritaSiniri.Hesapla();

        koordinatText.text = KoordinatSistemi.Formatla(yerelOyuncu.position);
    }

    private void YerelOyuncuyuBul()
    {
        if (NetworkClient.localPlayer != null)
        {
            yerelOyuncu = NetworkClient.localPlayer.transform;
            return;
        }

        // Editör / offline test yedegi
        var oyuncu = GameObject.FindGameObjectWithTag("Player");
        if (oyuncu != null)
            yerelOyuncu = oyuncu.transform;
    }
}
