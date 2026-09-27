using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Seçenekler ekranı: sağ üst ikonla açılır, 8 sekme arasında geçiş yapılır.
/// Veriler şimdilik görsel placeholder; ileride veritabanından doldurulacak.
/// </summary>
public class SeceneklerPaneli : MonoBehaviour
{
    [SerializeField] private GameObject panelKok;
    [SerializeField] private Button acmaButonu;
    [SerializeField] private Button kapatButonu;
    [SerializeField] private Button[] sekmeButonlari;
    [SerializeField] private GameObject[] sekmeIcerikleri;
    [SerializeField] private Button cikisOnayButonu;
    [SerializeField] private Button arkaPlanKapatButonu;
    [SerializeField] private Color sekmeNormal = new Color(0.12f, 0.18f, 0.24f, 1f);
    [SerializeField] private Color sekmeAktif = new Color(0.22f, 0.45f, 0.48f, 1f);
    [SerializeField] private Color sekmeCikis = new Color(0.45f, 0.15f, 0.15f, 1f);

    private int aktifSekme = -1;

    private void Awake()
    {
        if (acmaButonu != null)
            acmaButonu.onClick.AddListener(PaneliAc);

        if (kapatButonu != null)
            kapatButonu.onClick.AddListener(PaneliKapat);

        if (arkaPlanKapatButonu != null)
            arkaPlanKapatButonu.onClick.AddListener(PaneliKapat);

        if (cikisOnayButonu != null)
            cikisOnayButonu.onClick.AddListener(OyundanCikisIste);

        if (sekmeButonlari != null)
        {
            for (int i = 0; i < sekmeButonlari.Length; i++)
            {
                int index = i;
                if (sekmeButonlari[i] != null)
                    sekmeButonlari[i].onClick.AddListener(() => SekmeAc(index));
            }
        }

        if (panelKok != null)
            panelKok.SetActive(false);
    }

    private void Update()
    {
        if (panelKok == null || !panelKok.activeSelf) return;
        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            PaneliKapat();
    }

    public void PaneliAc()
    {
        if (panelKok == null) return;
        panelKok.SetActive(true);
        SekmeAc(aktifSekme < 0 ? 0 : aktifSekme);
    }

    public void PaneliKapat()
    {
        if (panelKok != null)
            panelKok.SetActive(false);
    }

    public void SekmeAc(int index)
    {
        if (sekmeIcerikleri == null || index < 0 || index >= sekmeIcerikleri.Length)
            return;

        aktifSekme = index;

        for (int i = 0; i < sekmeIcerikleri.Length; i++)
        {
            if (sekmeIcerikleri[i] != null)
                sekmeIcerikleri[i].SetActive(i == index);
        }

        if (sekmeButonlari == null) return;

        for (int i = 0; i < sekmeButonlari.Length; i++)
        {
            if (sekmeButonlari[i] == null) continue;
            var img = sekmeButonlari[i].GetComponent<Image>();
            if (img == null) continue;

            bool cikisSekmesi = i == sekmeButonlari.Length - 1;
            if (cikisSekmesi)
                img.color = i == index ? new Color(0.65f, 0.2f, 0.2f, 1f) : sekmeCikis;
            else
                img.color = i == index ? sekmeAktif : sekmeNormal;
        }
    }

    /// <summary>Çıkış — şimdilik placeholder. DB kaydı ve hasar kontrolü sonra eklenecek.</summary>
    public void OyundanCikisIste()
    {
        Debug.Log("[Secenekler] Oyundan çıkış istendi (görsel placeholder). Hasar altındaysa engellenecek; değilse harita+koordinat kaydedilecek.");
        PaneliKapat();
    }
}
