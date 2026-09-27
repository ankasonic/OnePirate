#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public static class UpdateChatUI3
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
        var sohbetPaneli = GameObject.Find("Canvas/SohbetPaneli");
        if (sohbetPaneli == null) return;

        // 1. Text hizalamasını düzeltme (Input Field Text'inin Left olması)
        var inputText = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (inputText != null)
        {
            inputText.alignment = TextAlignmentOptions.TopLeft;
        }

        // 2. Kapat Butonu (Sohbet panelinin içinde)
        var kapatBtnObj = GameObject.Find("Canvas/SohbetPaneli/KapatButonu");
        if (kapatBtnObj == null)
        {
            kapatBtnObj = new GameObject("KapatButonu");
            kapatBtnObj.transform.SetParent(sohbetPaneli.transform, false);
            var kapatRect = kapatBtnObj.AddComponent<RectTransform>();
            kapatRect.anchorMin = new Vector2(1, 1);
            kapatRect.anchorMax = new Vector2(1, 1);
            kapatRect.pivot = new Vector2(1, 1);
            kapatRect.anchoredPosition = new Vector2(-5, -5);
            kapatRect.sizeDelta = new Vector2(25, 25);
            
            var kapatImg = kapatBtnObj.AddComponent<Image>();
            kapatImg.color = Color.red;
            var kapatBtn = kapatBtnObj.AddComponent<Button>();
            
            // X yazısı
            var xObj = new GameObject("Text");
            xObj.transform.SetParent(kapatBtnObj.transform, false);
            var xRect = xObj.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.sizeDelta = Vector2.zero;
            var xText = xObj.AddComponent<TextMeshProUGUI>();
            xText.text = "X";
            xText.color = Color.white;
            xText.alignment = TextAlignmentOptions.Center;
            xText.fontSize = 16;
        }

        // 3. Sohbet Aç Butonu (Sol altta)
        var acBtnObj = GameObject.Find("Canvas/AcmaKapamaButonu");
        if (acBtnObj != null)
        {
            acBtnObj.name = "SohbetAcButonu";
            var acRect = acBtnObj.GetComponent<RectTransform>();
            acRect.anchorMin = new Vector2(0, 0);
            acRect.anchorMax = new Vector2(0, 0);
            acRect.pivot = new Vector2(0, 0);
            acRect.anchoredPosition = new Vector2(20, 20); // Sol alt
            acRect.sizeDelta = new Vector2(50, 50); // Biraz daha büyük ikon
            
            var img = acBtnObj.GetComponent<Image>();
            img.color = Color.white; // normal renk
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Chat UI güncellendi.");
    }
}
#endif