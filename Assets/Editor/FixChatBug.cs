#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using TMPro;

public static class FixChatBug
{
    public static void Execute()
    {
        // 1. Oyuncu Prefab'ına OyuncuSohbet bileşeninin tam olarak eklendiğinden ve kaydedildiğinden emin ol
        var prefabPath = "Assets/Prefabs/Oyuncu.prefab";
        var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot != null)
        {
            var sohbetComp = prefabRoot.GetComponent<OyuncuSohbet>();
            if (sohbetComp == null)
            {
                prefabRoot.AddComponent<OyuncuSohbet>();
                Debug.Log("OyuncuSohbet bileşeni prefab'a başarıyla eklendi.");
            }
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        // 2. Oyun_UI Sahnesindeki UI Mantık Hatalarını Çöz
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Oyun_UI.unity");
        }

        var canvas = GameObject.Find("Canvas");
        var panel = GameObject.Find("Canvas/SohbetPaneli");
        var acBtnObj = GameObject.Find("Canvas/SohbetAcButonu");

        if (canvas != null && panel != null)
        {
            // SohbetPaneli kapalı olduğunda Start() çalışmadığı için butonu dinleyemiyordu.
            // Bu yüzden SohbetArayuzu scriptini sürekli açık olan Canvas'a taşıyoruz.
            var oldArayuz = panel.GetComponent<SohbetArayuzu>();
            if (oldArayuz != null)
            {
                Object.DestroyImmediate(oldArayuz, true);
            }

            var newArayuz = canvas.GetComponent<SohbetArayuzu>();
            if (newArayuz == null)
            {
                newArayuz = canvas.AddComponent<SohbetArayuzu>();
            }

            // Script'in tüm referanslarını yeniden Canvas'taki yeni yerine bağlıyoruz.
            var so = new SerializedObject(newArayuz);
            so.FindProperty("inputField").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi")?.GetComponent<TMP_InputField>();
            so.FindProperty("content").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/MesajGecmisi/Viewport/Content")?.GetComponent<RectTransform>();
            so.FindProperty("mesajSablonu").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/MesajGecmisi/Viewport/Content/MesajSablonu")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("scrollRect").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/MesajGecmisi")?.GetComponent<ScrollRect>();
            so.FindProperty("gonderButonu").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/GonderButonu")?.GetComponent<Button>();
            so.FindProperty("acmaButonu").objectReferenceValue = acBtnObj?.GetComponent<Button>();
            so.FindProperty("kapatButonu").objectReferenceValue = GameObject.Find("Canvas/SohbetPaneli/KapatButonu")?.GetComponent<Button>();
            so.FindProperty("sohbetPaneli").objectReferenceValue = panel;
            so.ApplyModifiedProperties();

            // 3. Yazı Hizalaması (Ortadan Başlama Sorunu)
            // TMP Input Field altındaki metin objesinin hizalaması ve pivot değerleri düzeltiliyor.
            var textObj = GameObject.Find("Canvas/SohbetPaneli/MesajGirisi/Text Area/Text");
            if (textObj != null)
            {
                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.MidlineLeft; // Dikey orta, Yatay sol
                }
                
                var rt = textObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.pivot = new Vector2(0, 0.5f); // Pivot sola dayalı
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = new Vector2(5, 0); // Soldan 5 piksel boşluk bırak
                    rt.offsetMax = new Vector2(-5, 0);
                }
            }

            // Başlangıç durumlarını ayarla
            panel.SetActive(false); // Sohbet paneli başlangıçta kapalı
            if (acBtnObj != null) acBtnObj.SetActive(true); // Açma butonu her zaman görünür

            EditorSceneManager.SaveScene(scene);
            Debug.Log("Chat paneli acilmama ve ortadan baslama sorunlari duzeltildi.");
        }
    }
}
#endif