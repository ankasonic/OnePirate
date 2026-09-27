#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEditor.SceneManagement;

public static class ExtremeChatFix
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI") return;

        var textObj = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text");
        if (textObj != null)
        {
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                // Kesinlikle Sola hizala
                tmp.alignment = TextAlignmentOptions.TopLeft; 
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.extraPadding = true;
                tmp.margin = new Vector4(5, 0, 0, 0); // Soldan 5 pixel boşluk
            }
            
            var rt = textObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                // TMP_InputField Text varsayılan RectTransform ayarları:
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.offsetMin = new Vector2(0, 0);
                rt.offsetMax = new Vector2(0, 0);
                rt.localPosition = new Vector3(0, 0, 0);
            }
        }

        var textAreaObj = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area");
        if (textAreaObj != null)
        {
            var rt = textAreaObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(10, 5); // Soldan 10px, Alttan 5px
                rt.offsetMax = new Vector2(-10, -5); // Sağdan -10px, Üstten -5px
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Chat Yazı Alanı Kesin Olarak Sola Hizalandı.");
    }
}
#endif