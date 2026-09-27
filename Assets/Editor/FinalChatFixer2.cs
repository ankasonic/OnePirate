#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;

public static class FinalChatFixer2
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI") return;

        // Buton Graphic fix
        var acBtn = GameObject.Find("Canvas/SohbetAcButonu")?.GetComponent<Button>();
        if (acBtn != null && acBtn.targetGraphic == null)
        {
            acBtn.targetGraphic = acBtn.GetComponent<Image>();
        }

        var kapatBtn = GameObject.Find("Canvas/SohbetPaneli/KapatButonu")?.GetComponent<Button>();
        if (kapatBtn != null && kapatBtn.targetGraphic == null)
        {
            kapatBtn.targetGraphic = kapatBtn.GetComponent<Image>();
        }

        var gonderBtn = GameObject.Find("Canvas/SohbetPaneli/GonderButonu")?.GetComponent<Button>();
        if (gonderBtn != null && gonderBtn.targetGraphic == null)
        {
            gonderBtn.targetGraphic = gonderBtn.GetComponent<Image>();
        }

        // Text Alignment fix for TMP_InputField Text
        var inputText = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text")?.GetComponent<TextMeshProUGUI>();
        if (inputText != null)
        {
            // Center - Left alignment equivalent in TMPro
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
        }
        
        // Ensure Input Field is correctly set up for Single Line
        var inputField = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi")?.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.lineType = TMP_InputField.LineType.SingleLine;
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Buton hedefleri ve text hizalamaları güncellendi.");
    }
}
#endif