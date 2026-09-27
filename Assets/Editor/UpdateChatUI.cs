#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public static class UpdateChatUI
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            Debug.LogError("Lütfen Oyun_UI sahnesini açın.");
            return;
        }

        var sohbetPaneli = GameObject.Find("Canvas/SohbetPaneli");
        if (sohbetPaneli == null) return;
        
        var rectTransform = sohbetPaneli.GetComponent<RectTransform>();
        
        // Gönderme Butonu (Enter)
        var btnEnterObj = new GameObject("GonderButonu");
        btnEnterObj.transform.SetParent(sohbetPaneli.transform, false);
        var btnEnterRect = btnEnterObj.AddComponent<RectTransform>();
        btnEnterRect.anchorMin = new Vector2(1, 0);
        btnEnterRect.anchorMax = new Vector2(1, 0);
        btnEnterRect.pivot = new Vector2(1, 0);
        btnEnterRect.anchoredPosition = new Vector2(-10, 10);
        btnEnterRect.sizeDelta = new Vector2(30, 30);
        
        var btnEnterImg = btnEnterObj.AddComponent<Image>();
        var spriteGonder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/UI_Canvas/AcmaKapama.png"); // Örnek olarak bu ikonu koyuyoruz, değiştirebiliriz.
        if(spriteGonder != null) btnEnterImg.sprite = spriteGonder;
        var btnEnter = btnEnterObj.AddComponent<Button>();

        // MesajGirisinin sağını biraz kısaltıp butona yer açalım
        var mesajGirisi = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi");
        if(mesajGirisi != null)
        {
            var mRect = mesajGirisi.GetComponent<RectTransform>();
            mRect.offsetMax = new Vector2(-45, mRect.offsetMax.y);
        }

        // Açma / Kapama Butonu (Sohbet panelinin sağ üst köşesi)
        var btnToggleObj = new GameObject("AcmaKapamaButonu");
        btnToggleObj.transform.SetParent(GameObject.Find("Canvas").transform, false);
        var btnToggleRect = btnToggleObj.AddComponent<RectTransform>();
        btnToggleRect.anchorMin = new Vector2(0, 0);
        btnToggleRect.anchorMax = new Vector2(0, 0);
        btnToggleRect.pivot = new Vector2(0, 0);
        btnToggleRect.anchoredPosition = new Vector2(420, 240); // Panelin sağ üstüne denk gelecek şekilde
        btnToggleRect.sizeDelta = new Vector2(30, 30);

        var btnToggleImg = btnToggleObj.AddComponent<Image>();
        var spriteToggle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/UI_Canvas/AcmaKapama.png");
        if(spriteToggle != null) btnToggleImg.sprite = spriteToggle;
        
        var btnToggle = btnToggleObj.AddComponent<Button>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Butonlar eklendi.");
    }
}
#endif