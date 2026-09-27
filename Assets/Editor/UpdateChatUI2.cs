#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class UpdateChatUI2
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
        var acKapaBtn = GameObject.Find("Canvas/AcmaKapamaButonu")?.GetComponent<Button>();
        var panel = GameObject.Find("Canvas/SohbetPaneli");

        var serializedObject = new SerializedObject(chatUI);
        
        if (gonderBtn != null) serializedObject.FindProperty("gonderButonu").objectReferenceValue = gonderBtn;
        if (acKapaBtn != null) serializedObject.FindProperty("acmaKapamaButonu").objectReferenceValue = acKapaBtn;
        if (panel != null) serializedObject.FindProperty("sohbetPaneli").objectReferenceValue = panel;

        serializedObject.ApplyModifiedProperties();
        
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Script referansları güncellendi.");
    }
}
#endif