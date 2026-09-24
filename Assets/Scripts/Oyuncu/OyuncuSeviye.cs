using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class OyuncuSeviye : NetworkBehaviour
{
    [Header("UI Bileşenleri")]
    [SerializeField] private Image seviyeIkonu;
    [SerializeField] private TMP_Text seviyeYazisi;

    [Header("Seviye Bilgisi")]
    [SyncVar(hook = nameof(OnSeviyeDegisti))]
    [SerializeField] private int seviye = 1;

    [Header("Seviye İkonları")]
    [SerializeField] private Sprite[] seviyeIkonlari;

    private OyuncuCanBari canBari;

    public int Seviye => seviye;

    private void Awake()
    {
        canBari = GetComponent<OyuncuCanBari>();
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

    public void BilesenleriAyarla(Image ikon, TMP_Text yazi, Sprite[] ikonlar = null)
    {
        seviyeIkonu = ikon;
        seviyeYazisi = yazi;
        if (ikonlar != null)
            seviyeIkonlari = ikonlar;
        ArayuzuGuncelle();
    }

    private void BilesenleriOtomatikBul()
    {
        if (seviyeIkonu == null)
            seviyeIkonu = GetComponentInChildren<Image>(true);
        if (seviyeYazisi == null)
            seviyeYazisi = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnSeviyeDegisti(int eskiSeviye, int yeniSeviye)
    {
        ArayuzuGuncelle();
        if (canBari != null && isServer)
        {
            canBari.SeviyeyeGoreMaksCanAyarla(yeniSeviye);
        }
    }

    public void ArayuzuGuncelle()
    {
        if (seviyeYazisi != null)
        {
            seviyeYazisi.text = seviye.ToString();
        }

        if (seviyeIkonu != null && seviyeIkonlari != null && seviyeIkonlari.Length > 0)
        {
            int index = Mathf.Clamp(seviye - 1, 0, seviyeIkonlari.Length - 1);
            if (seviyeIkonlari[index] != null)
            {
                seviyeIkonu.sprite = seviyeIkonlari[index];
                seviyeIkonu.enabled = true;
            }
        }
    }

    [Server]
    public void SetSeviye(int yeniSeviye)
    {
        seviye = Mathf.Clamp(yeniSeviye, 1, 10);
        if (canBari != null)
        {
            canBari.SeviyeyeGoreMaksCanAyarla(seviye);
        }
    }

    [Server]
    public void SeviyeArttir()
    {
        if (seviye < 10)
        {
            SetSeviye(seviye + 1);
        }
    }

    // Yerel ve çevrimdışı testler için
    public void SeviyeAyarla(int yeniSeviye)
    {
        seviye = Mathf.Clamp(yeniSeviye, 1, 10);
        if (canBari != null)
        {
            canBari.SeviyeyeGoreMaksCanAyarla(seviye);
        }
        ArayuzuGuncelle();
    }
}
