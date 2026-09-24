using UnityEngine;
using TMPro;

public class HasarYazisi : MonoBehaviour
{
    [SerializeField] private TMP_Text yaziText;
    [SerializeField] private float ucusHizi = 1.5f;
    [SerializeField] private float yokOlmaSuresi = 1f;

    private Color baslangicRengi;
    private float gecenSure;

    private void Awake()
    {
        if (yaziText == null)
            yaziText = GetComponentInChildren<TMP_Text>();
    }

    public void Baslat(int miktar, Color renk, bool sifaMi = false, bool kritikMi = false)
    {
        if (yaziText == null)
            yaziText = GetComponentInChildren<TMP_Text>();

        baslangicRengi = renk;
        yaziText.color = baslangicRengi;

        // 1. Eksi işareti olmadan, şifada da "+" olmadan direkt rakam
        string metin = $"{miktar}";

        // 2. Küçültülmüş kibar font boyutu
        if (kritikMi)
        {
            metin = $"KRİTİK!\n{metin}";
            yaziText.fontSize = 4.5f;
        }
        else
        {
            yaziText.fontSize = 3.2f;
        }

        yaziText.text = metin;
        Destroy(gameObject, yokOlmaSuresi);
    }

    private void Update()
    {
        transform.position += Vector3.up * (ucusHizi * Time.deltaTime);

        gecenSure += Time.deltaTime;
        float alfa = Mathf.Clamp01(1f - (gecenSure / yokOlmaSuresi));

        if (yaziText != null)
        {
            Color c = baslangicRengi;
            c.a = alfa;
            yaziText.color = c;
        }
    }
}
