using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Mirror;
using kcp2k;

public class BaslangicAyarla : EditorWindow
{
    [MenuItem("OnePirate/Baslangic Sahnesini Ayarla (NetworkManager)")]
    public static void Ayarla()
    {
        string baslangicYolu = "Assets/Scenes/Baslangic.unity";
        
        // Eğer şu an Başlangıç sahnesinde değilsek, kaydet ve oraya geç
        if (EditorSceneManager.GetActiveScene().path != baslangicYolu)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(baslangicYolu);
            }
            else
            {
                return;
            }
        }

        GameObject nmObj = GameObject.Find("NetworkManager");
        if (nmObj == null)
        {
            nmObj = new GameObject("NetworkManager");
        }

        // KcpTransport ekle
        KcpTransport transport = nmObj.GetComponent<KcpTransport>();
        if (transport == null) transport = nmObj.AddComponent<KcpTransport>();

        // NetworkManager ekle
        NetworkManager nm = nmObj.GetComponent<NetworkManager>();
        if (nm == null) nm = nmObj.AddComponent<NetworkManager>();

        // Transport'u bağla
        nm.transport = transport;

        // Player Prefab'ı yükle ve bağla
        GameObject oyuncuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Oyuncu.prefab");
        if (oyuncuPrefab != null)
        {
            nm.playerPrefab = oyuncuPrefab;
        }
        else
        {
            Debug.LogError("Oyuncu prefabı bulunamadı! Lütfen Assets/Prefabs/Oyuncu.prefab yolunu kontrol edin.");
        }

        // Online ve Offline Sahneleri ayarla (Build Settings'de olmaları gerekir!)
        nm.offlineScene = "Assets/Scenes/Baslangic.unity";
        nm.onlineScene = "Assets/Scenes/Haritalar/1-1.unity";

        // Spawn metodunu ayarla
        nm.playerSpawnMethod = PlayerSpawnMethod.Random;

        // Geliştirme aşamasında kolaylık olması için NetworkManagerHUD ekle
        NetworkManagerHUD hud = nmObj.GetComponent<NetworkManagerHUD>();
        if (hud == null) hud = nmObj.AddComponent<NetworkManagerHUD>();

        // Mirror Spawnable Prefabs Listesini Doldur (SADECE NetworkIdentity olan prefablari ekle!)
        nm.spawnPrefabs.Clear();
        GameObject npcSpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Npc.prefab");
        if (npcSpPrefab != null && npcSpPrefab.GetComponent<NetworkIdentity>() != null)
        {
            nm.spawnPrefabs.Add(npcSpPrefab);
        }

        // Baslangic sahnesine NpcSpawner objesini ekle / yapilandir
        GameObject spawnerObj = GameObject.Find("NpcSpawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("NpcSpawner");
        }

        if (!spawnerObj.TryGetComponent<NetworkIdentity>(out var netId))
            spawnerObj.AddComponent<NetworkIdentity>();

        if (!spawnerObj.TryGetComponent<NpcSpawner>(out var spawnerComp))
            spawnerComp = spawnerObj.AddComponent<NpcSpawner>();

        GameObject npcPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Npc.prefab");
        if (npcPrefabObj != null)
        {
            SerializedObject soSpawner = new SerializedObject(spawnerComp);
            soSpawner.FindProperty("npcPrefab").objectReferenceValue = npcPrefabObj;
            soSpawner.FindProperty("toplamNpcSayisi").intValue = 40;
            soSpawner.FindProperty("haritaNpcTuru").enumValueIndex = 0; // Boris Juckson
            soSpawner.FindProperty("spawnAlaniMin").vector2Value = new Vector2(-50f, -70f);
            soSpawner.FindProperty("spawnAlaniMax").vector2Value = new Vector2(80f, 60f);
            soSpawner.ApplyModifiedProperties();
        }

        // Sahneyi kaydet
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        // Build Settings'e sahneleri ekle
        ListeleSahneleriBuildAyarlarinaEkle();

        Debug.Log("[Başlangıç Ayarı] NetworkManager başarıyla yapılandırıldı! Lobi pas geçilip doğrudan 1-1 sahnesine girilecek şekilde (Online Scene) ayarlandı.");
    }

    private static void ListeleSahneleriBuildAyarlarinaEkle()
    {
        EditorBuildSettingsScene[] orjinalSahneler = EditorBuildSettings.scenes;
        var yeniSahneler = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        bool baslangicVar = false;
        bool harita11Var = false;

        foreach (var s in orjinalSahneler)
        {
            yeniSahneler.Add(s);
            if (s.path == "Assets/Scenes/Baslangic.unity") baslangicVar = true;
            if (s.path == "Assets/Scenes/Haritalar/1-1.unity") harita11Var = true;
        }

        if (!baslangicVar) yeniSahneler.Insert(0, new EditorBuildSettingsScene("Assets/Scenes/Baslangic.unity", true));
        if (!harita11Var) yeniSahneler.Add(new EditorBuildSettingsScene("Assets/Scenes/Haritalar/1-1.unity", true));

        EditorBuildSettings.scenes = yeniSahneler.ToArray();
    }
}
