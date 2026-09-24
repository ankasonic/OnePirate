using UnityEngine;
using TMPro;
using Mirror;

public class OyuncuFilo : NetworkBehaviour
{
    [Header("UI Bileşenleri")]
    [SerializeField] private TMP_Text filoTagiYazisi;

    [Header("Filo Bilgisi")]
    [SyncVar(hook = nameof(OnFiloTagiDegisti))]
    [SerializeField] private string filoTagi = "";

    [SerializeField] private bool koseliParantezKullan = true;

    public string FiloTagi => filoTagi;

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
        filoTagiYazisi = yazi;
        ArayuzuGuncelle();
    }

    private void BilesenleriOtomatikBul()
    {
        if (filoTagiYazisi == null)
            filoTagiYazisi = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnFiloTagiDegisti(string eskiTag, string yeniTag)
    {
        ArayuzuGuncelle();
    }

    public void ArayuzuGuncelle()
    {
        if (filoTagiYazisi != null)
        {
            if (string.IsNullOrWhiteSpace(filoTagi))
            {
                filoTagiYazisi.text = "";
                filoTagiYazisi.gameObject.SetActive(false);
            }
            else
            {
                string formatliTag = koseliParantezKullan ? $"{filoTagi}" : filoTagi;
                filoTagiYazisi.text = formatliTag;
                filoTagiYazisi.gameObject.SetActive(true);
            }
        }
    }

    // 3 karakter sınırlaması (büyük harf)
    public static string TagFormatla(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return "";
        tag = tag.Trim().ToUpper();
        if (tag.Length > 3)
            tag = tag.Substring(0, 3);
        return tag;
    }

    [Command]
    public void CmdFiloTagiAyarla(string yeniTag)
    {
        ServerFiloTagiAyarla(yeniTag);
    }

    [Server]
    public void ServerFiloTagiAyarla(string yeniTag)
    {
        filoTagi = TagFormatla(yeniTag);
    }

    // Yerel ve çevrimdışı testler için
    public void SetFiloTagi(string yeniTag)
    {
        filoTagi = TagFormatla(yeniTag);
        ArayuzuGuncelle();
    }
}
