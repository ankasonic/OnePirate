#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public static class CreateChatUI
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            Debug.LogError("Lütfen Oyun_UI sahnesini açın.");
            return;
        }

        // EventSystem
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Sohbet Paneli (Sol alt)
        var chatPanel = new GameObject("SohbetPaneli");
        chatPanel.transform.SetParent(canvasObj.transform, false);
        var chatRect = chatPanel.AddComponent<RectTransform>();
        chatRect.anchorMin = new Vector2(0, 0);
        chatRect.anchorMax = new Vector2(0, 0);
        chatRect.pivot = new Vector2(0, 0);
        chatRect.anchoredPosition = new Vector2(20, 20);
        chatRect.sizeDelta = new Vector2(400, 250);

        var bg = chatPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        // Scroll View (Mesajlar için)
        var scrollView = new GameObject("MesajGecmisi");
        scrollView.transform.SetParent(chatPanel.transform, false);
        var scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.offsetMin = new Vector2(10, 50); // Alttan InputField için boşluk
        scrollRect.offsetMax = new Vector2(-10, -10);

        var scrollImg = scrollView.AddComponent<Image>();
        scrollImg.color = new Color(1, 1, 1, 0.05f); // Çok hafif arka plan
        
        var scrollMask = scrollView.AddComponent<Mask>();
        scrollMask.showMaskGraphic = false;

        var scrollComp = scrollView.AddComponent<ScrollRect>();
        scrollComp.horizontal = false;
        scrollComp.vertical = true;
        scrollComp.movementType = ScrollRect.MovementType.Clamped;
        scrollComp.scrollSensitivity = 15f;

        // Viewport
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = new Vector2(0, 0);
        vpRect.anchorMax = new Vector2(1, 1);
        vpRect.pivot = new Vector2(0, 1);
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1); // Expand yapalım sonra VerticalLayout halleder
        contentRect.pivot = new Vector2(0, 0);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollComp.viewport = vpRect;
        scrollComp.content = contentRect;

        // Input Field
        var inputObj = new GameObject("MesajGirisi");
        inputObj.transform.SetParent(chatPanel.transform, false);
        var inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(1, 0);
        inputRect.pivot = new Vector2(0.5f, 0);
        inputRect.anchoredPosition = new Vector2(0, 10);
        inputRect.sizeDelta = new Vector2(-20, 30);

        var inputImg = inputObj.AddComponent<Image>();
        inputImg.color = new Color(0, 0, 0, 0.8f);

        var inputField = inputObj.AddComponent<TMP_InputField>();
        
        // Text Area (InputField için)
        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        var taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = new Vector2(0, 0);
        taRect.anchorMax = new Vector2(1, 1);
        taRect.offsetMin = new Vector2(5, 0);
        taRect.offsetMax = new Vector2(-5, 0);
        var taMask = textArea.AddComponent<RectMask2D>();

        // Text (Girdiğimiz metin)
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.color = Color.white;
        tmpText.fontSize = 14;
        tmpText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        // Placeholder
        var phObj = new GameObject("Placeholder");
        phObj.transform.SetParent(textArea.transform, false);
        var phRect = phObj.AddComponent<RectTransform>();
        phRect.anchorMin = new Vector2(0, 0);
        phRect.anchorMax = new Vector2(1, 1);
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;
        var phText = phObj.AddComponent<TextMeshProUGUI>();
        phText.text = "Mesaj yazın...";
        phText.color = new Color(1, 1, 1, 0.5f);
        phText.fontSize = 14;
        phText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        inputField.textViewport = taRect;
        inputField.textComponent = tmpText;
        inputField.placeholder = phText;
        inputField.fontAsset = TMP_Settings.defaultFontAsset;

        // Mesaj Şablonu (Prefab gibi kullanılacak, kapatılacak)
        var msgTemplate = new GameObject("MesajSablonu");
        msgTemplate.transform.SetParent(content.transform, false);
        var msgRect = msgTemplate.AddComponent<RectTransform>();
        var msgText = msgTemplate.AddComponent<TextMeshProUGUI>();
        msgText.color = Color.white;
        msgText.fontSize = 14;
        msgText.alignment = TextAlignmentOptions.BottomLeft;
        msgTemplate.SetActive(false);

        // Sohbet Arayüzü Scripti
        var chatUI = chatPanel.AddComponent<SohbetArayuzu>();
        // Reflection ile atamalar yapacağız ki compile error olmadan çalışsın (SohbetArayuzu scripti yazıldıktan sonra)
        
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Chat UI Oluşturuldu!");
    }
}
#endif
