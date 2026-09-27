using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Mirror;

public class SohbetArayuzu : MonoBehaviour
{
    public static SohbetArayuzu Instance;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI mesajSablonu;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button gonderButonu;
    [SerializeField] private Button acmaButonu;
    [SerializeField] private Button kapatButonu;
    [SerializeField] private GameObject sohbetPaneli;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(MesajGonderildi);
        }

        if (gonderButonu != null)
        {
            gonderButonu.onClick.AddListener(GonderButonunaBasildi);
        }

        if (acmaButonu != null)
        {
            acmaButonu.onClick.AddListener(SohbetAc);
        }

        if (kapatButonu != null)
        {
            kapatButonu.onClick.AddListener(SohbetKapat);
        }
        
        // Oyun basinda sohbet acik olsun
        SohbetAc();
    }

    private void GonderButonunaBasildi()
    {
        if (inputField != null)
        {
            MesajGonderildi(inputField.text);
        }
    }

    private void SohbetAc()
    {
        if (sohbetPaneli != null) sohbetPaneli.SetActive(true);
        if (acmaButonu != null) acmaButonu.gameObject.SetActive(false);
    }

    private void SohbetKapat()
    {
        if (sohbetPaneli != null) sohbetPaneli.SetActive(false);
        if (acmaButonu != null) acmaButonu.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Yazı yazma alanındayken C tuşuna basıldığında sohbetin tetiklenmesini engelle
        bool yaziYaziliyor = inputField != null && inputField.isFocused;

        // C'ye basıldığında sohbet kapalıysa aç
        if (Keyboard.current.cKey.wasPressedThisFrame && !yaziYaziliyor)
        {
            if (sohbetPaneli != null && !sohbetPaneli.activeSelf)
            {
                SohbetAc();
                
                // Açılır açılmaz hemen yazı yazılabilmesi için odaklar
                if (inputField != null)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                }
            }
        }

        // Enter'a basıldıysa ve sohbet açıksa input alanına odaklan
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (sohbetPaneli != null && sohbetPaneli.activeSelf)
            {
                if (inputField != null && !inputField.isFocused)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                }
            }
        }
    }

    private void MesajGonderildi(string mesaj)
    {
        if (string.IsNullOrWhiteSpace(mesaj))
        {
            inputField.text = string.Empty;
            inputField.Select();
            inputField.ActivateInputField();
            return;
        }

        var yerelOyuncu = NetworkClient.localPlayer;
        if (yerelOyuncu != null)
        {
            var sohbet = yerelOyuncu.GetComponent<OyuncuSohbet>();
            if (sohbet != null)
            {
                sohbet.CmdMesajGonder(mesaj);
            }
            else
            {
                Debug.LogWarning("Yerel oyuncuda OyuncuSohbet bileşeni bulunamadı.");
            }
        }

        if (inputField != null)
        {
            inputField.text = string.Empty;
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public static void MesajEkle(string gonderen, string mesaj)
    {
        if (Instance == null || Instance.mesajSablonu == null || Instance.content == null) return;

        var yeniMesajObj = Instantiate(Instance.mesajSablonu.gameObject, Instance.content);
        yeniMesajObj.SetActive(true);
        var textComp = yeniMesajObj.GetComponent<TextMeshProUGUI>();
        
        textComp.text = $"<b>[{gonderen}]:</b> {mesaj}";

        Canvas.ForceUpdateCanvases();
        if (Instance.scrollRect != null)
        {
            Instance.scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
