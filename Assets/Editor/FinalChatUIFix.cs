#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;

public static class FinalChatUIFix
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
        var chatUI = Object.FindFirstObjectByType<SohbetArayuzu>();
        
        // 1. Text hizalamasını tam olarak MidlineLeft yap
        var inputText = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (inputText != null)
        {
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            
            // Layout ayarlarını garantile
            var rt = inputText.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // 2. Chat açma butonunu Canvas içinde bul ve referans ata
        var acBtn = GameObject.Find("Canvas/SohbetAcButonu")?.GetComponent<Button>();
        if (acBtn != null && chatUI != null)
        {
            var serializedObject = new SerializedObject(chatUI);
            serializedObject.FindProperty("acmaButonu").objectReferenceValue = acBtn;
            serializedObject.ApplyModifiedProperties();
        }

        // Oyun basinda sohbet gizli olsun, sadece acma butonu gorunsun
        var panel = GameObject.Find("Canvas/SohbetPaneli");
        if (panel != null)
        {
            panel.SetActive(false);
        }
        if (acBtn != null)
        {
            acBtn.gameObject.SetActive(true);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Chat UI Fix Applied.");
    }
}
#endif