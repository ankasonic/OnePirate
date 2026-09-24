using UnityEngine;
using TMPro;
using Mirror;

public class OyuncuKullaniciAdi : NetworkBehaviour
{
    [Header("UI Bileşenleri")]
    [SerializeField] private TMP_Text kullaniciAdiYazisi;

    [Header("Kullanıcı Bilgisi")]
    [SyncVar(hook = nameof(OnKullaniciAdiDegisti))]
    [SerializeField] private string kullaniciAdi = "Korsan";

    public string KullaniciAdi => kullaniciAdi;

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

    public void BileseniAyarla(TMP_Text yazi)
    {
        kullaniciAdiYazisi = yazi;
        ArayuzuGuncelle();
    }

    private void BilesenleriOtomatikBul()
    {
        if (kullaniciAdiYazisi == null)
            kullaniciAdiYazisi = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnKullaniciAdiDegisti(string eskiAd, string yeniAd)
    {
        ArayuzuGuncelle();
    }

    public void ArayuzuGuncelle()
    {
        if (kullaniciAdiYazisi != null)
        {
            kullaniciAdiYazisi.text = string.IsNullOrWhiteSpace(kullaniciAdi) ? "Oyuncu" : kullaniciAdi;
        }
    }

    [Command]
    public void CmdKullaniciAdiAyarla(string yeniAd)
    {
        ServerKullaniciAdiAyarla(yeniAd);
    }

    [Server]
    public void ServerKullaniciAdiAyarla(string yeniAd)
    {
        kullaniciAdi = string.IsNullOrWhiteSpace(yeniAd) ? "Oyuncu" : yeniAd.Trim();
    }

    // Yerel ve çevrimdışı testler için
    public void SetKullaniciAdi(string yeniAd)
    {
        kullaniciAdi = string.IsNullOrWhiteSpace(yeniAd) ? "Oyuncu" : yeniAd.Trim();
        ArayuzuGuncelle();
    }
}
