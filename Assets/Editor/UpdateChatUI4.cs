#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class UpdateChatUI4
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            Debug.LogError("Lütfen Oyun_UI sahnesini açın.");
            return;
        }

        var chatUI = Object.FindFirstObjectByType<SohbetArayuzu>();
        if (chatUI == null) return;

        var gonderBtn = GameObject.Find("Canvas/SohbetPaneli/GonderButonu")?.GetComponent<Button>();
        var kapatBtn = GameObject.Find("Canvas/SohbetPaneli/KapatButonu")?.GetComponent<Button>();
        var acBtn = GameObject.Find("Canvas/SohbetAcButonu")?.GetComponent<Button>();
        var panel = GameObject.Find("Canvas/SohbetPaneli");

        var serializedObject = new SerializedObject(chatUI);
        
        if (gonderBtn != null) serializedObject.FindProperty("gonderButonu").objectReferenceValue = gonderBtn;
        if (kapatBtn != null) serializedObject.FindProperty("kapatButonu").objectReferenceValue = kapatBtn;
        if (acBtn != null) serializedObject.FindProperty("acmaButonu").objectReferenceValue = acBtn;
        if (panel != null) serializedObject.FindProperty("sohbetPaneli").objectReferenceValue = panel;

        serializedObject.ApplyModifiedProperties();
        
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Sohbet script referansları güncellendi.");
    }
}
#endif