using UnityEditor;
using UnityEngine;
using System.Linq;

public static class AddSceneToBuild
{
    public static void Execute()
    {
        string scenePath = "Assets/Scenes/Haritalar/2-1.unity";
        
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Scene added to build settings.");
        }
        else
        {
            Debug.Log("Scene already in build settings.");
        }
    }
}