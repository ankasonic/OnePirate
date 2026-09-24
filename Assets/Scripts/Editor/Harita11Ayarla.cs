using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Harita11Ayarla : EditorWindow
{
    [MenuItem("OnePirate/1-1 Sahnesi NpcSpawner Ekle")]
    public static void Ayarla()
    {
        string haritaYolu = "Assets/Scenes/Haritalar/1-1.unity";

        if (EditorSceneManager.GetActiveScene().path != haritaYolu)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(haritaYolu);
            }
            else
            {
                return;
            }
        }

        GameObject spawnerObj = GameObject.Find("NpcSpawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("NpcSpawner");
        }

        NpcSpawner spawner = spawnerObj.GetComponent<NpcSpawner>();
        if (spawner == null)
        {
            spawner = spawnerObj.AddComponent<NpcSpawner>();
        }

        GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Npc.prefab");
        if (npcPrefab != null)
        {
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("npcPrefab").objectReferenceValue = npcPrefab;
            so.FindProperty("toplamNpcSayisi").intValue = 40;
            so.FindProperty("haritaNpcTuru").enumValueIndex = 0; // Boris Juckson

            // Sınırlar
            so.FindProperty("spawnAlaniMin").vector2Value = new Vector2(-50f, -70f);
            so.FindProperty("spawnAlaniMax").vector2Value = new Vector2(80f, 60f);

            so.ApplyModifiedProperties();
            Debug.Log("[1-1 Ayarı] NpcSpawner başarıyla eklendi ve ayarlandı!");
        }
        else
        {
            Debug.LogError("Npc prefabı bulunamadı!");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
