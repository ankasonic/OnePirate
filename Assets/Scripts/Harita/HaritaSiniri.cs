using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Gemi ve NPC'lerin "Deniz" isimli Tilemap sınırlarına göre hareketini kısıtlar.
/// Sınırları otomatik olarak haritadan okur.
/// </summary>
public class HaritaSiniri : MonoBehaviour
{
    private static bool sinirlarHesaplandi = false;
    private static Vector2 minSinir;
    private static Vector2 maxSinir;

    public static Vector2 MinSinir => minSinir;
    public static Vector2 MaxSinir => maxSinir;
    public static bool SinirlarHesaplandi => sinirlarHesaplandi;

    private void Start()
    {
        Hesapla();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Hesapla();
    }

    public static void Hesapla()
    {
        GameObject denizObj = GameObject.Find("Deniz");
        if (denizObj != null)
        {
            Tilemap tilemap = denizObj.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                tilemap.CompressBounds();
                BoundsInt bounds = tilemap.cellBounds;
                
                // Hücre (Cell) koordinatlarını dünya (World) koordinatlarına çeviriyoruz.
                // Not: Sağ üst köşe için bounds.max'i alıyoruz.
                Vector3 minW = tilemap.CellToWorld(bounds.min);
                Vector3 maxW = tilemap.CellToWorld(bounds.max);

                minSinir = new Vector2(minW.x, minW.y);
                maxSinir = new Vector2(maxW.x, maxW.y);
                sinirlarHesaplandi = true;
                
                Debug.Log($"[HaritaSiniri] Harita sınırları otomatik hesaplandı: Min {minSinir}, Max {maxSinir}");
            }
        }
    }

    private void LateUpdate()
    {
        if (!sinirlarHesaplandi) return;

        Vector3 pos = transform.position;
        bool degisti = false;

        // Geminin yarıçapı kadar pay bırakarak dışarı taşmasını önle
        float pay = 0.5f;

        if (pos.x < minSinir.x + pay) { pos.x = minSinir.x + pay; degisti = true; }
        else if (pos.x > maxSinir.x - pay) { pos.x = maxSinir.x - pay; degisti = true; }

        if (pos.y < minSinir.y + pay) { pos.y = minSinir.y + pay; degisti = true; }
        else if (pos.y > maxSinir.y - pay) { pos.y = maxSinir.y - pay; degisti = true; }

        if (degisti)
        {
            transform.position = pos;
        }
    }
}
