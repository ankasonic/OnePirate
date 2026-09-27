#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class AddUISceneToBuild
{
    public static void Execute()
    {
        string scenePath = "Assets/Scenes/Oyun_UI.unity";
        
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Oyun_UI scene added to build settings.");
        }
    }
}
#endif