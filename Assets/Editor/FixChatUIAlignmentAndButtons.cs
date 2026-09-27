#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;

public static class FixChatUIAlignmentAndButtons
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            Debug.LogError("Lütfen Oyun_UI sahnesini açın.");
            return;
        }

        var canvas = GameObject.Find("Canvas");
        
        // 1. Yazı ortadan başlama sorunu çözümü (Input Field -> Text)
        var inputText = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (inputText != null)
        {
            // Yazıyı Dikey olarak ortala, Yatay olarak sola daya (Midline-Left)
            inputText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
        }
        
        var inputRect = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text")?.GetComponent<RectTransform>();
        if (inputRect != null)
        {
            // Yukarıdan/aşağıdan offset'leri sıfırla ki ortalama düzgün çalışsın
            inputRect.offsetMin = new Vector2(0, 0);
            inputRect.offsetMax = new Vector2(0, 0);
        }

        // 2. Chat açma butonunu sol alta al ve yeni ikon ata
        var acBtnObj = GameObject.Find("Canvas/SohbetPaneli/AcmaKapamaButonu");
        if (acBtnObj != null)
        {
            // Butonu SohbetPanelinden çıkarıp Canvas'ın içine koy
            acBtnObj.transform.SetParent(canvas.transform, false);
            acBtnObj.name = "SohbetAcButonu";
            
            var acRect = acBtnObj.GetComponent<RectTransform>();
            acRect.anchorMin = new Vector2(0, 0);
            acRect.anchorMax = new Vector2(0, 0);
            acRect.pivot = new Vector2(0, 0);
            acRect.anchoredPosition = new Vector2(20, 20); // Ekranın sol alt köşesine al
            acRect.sizeDelta = new Vector2(40, 40); // Boyutunu ayarla

            var img = acBtnObj.GetComponent<Image>();
            // Sohbet açmak için konuşma balonu veya benzer bir ikon (Geçici Icon_Quest_t kullanıldı)
            var sohbetIkonu = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/Buton/Icon_Quest_t.png"); 
            if(sohbetIkonu != null) img.sprite = sohbetIkonu;
        }

        // 3. Gönder Butonunun ikonunu güncelle
        var gonderBtnImg = GameObject.Find("Canvas/SohbetPaneli/GonderButonu")?.GetComponent<Image>();
        if (gonderBtnImg != null)
        {
            var okIkonu = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/Buton/Icon_ArrowRight_t.png");
            if(okIkonu != null) gonderBtnImg.sprite = okIkonu;
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Chat UI hizalama ve ikonları düzeltildi.");
    }
}
#endif