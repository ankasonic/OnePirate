using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoader : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ana oyun haritaları yüklendiğinde Oyun_UI sahnesini de Additive olarak yükle
        if (mode == LoadSceneMode.Single && (scene.name == "1-1" || scene.name == "2-1"))
        {
            bool uiYuklu = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == "Oyun_UI")
                {
                    uiYuklu = true;
                    break;
                }
            }

            if (!uiYuklu)
            {
                SceneManager.LoadSceneAsync("Oyun_UI", LoadSceneMode.Additive);
            }
        }
    }
}
