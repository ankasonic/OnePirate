using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TilemapSinirBul : EditorWindow
{
    [MenuItem("OnePirate/Tilemap Sinirini Hesapla")]
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
                
                Vector3 minWorld = tilemap.CellToWorld(bounds.min);
                Vector3 maxWorld = tilemap.CellToWorld(bounds.max);

                Debug.Log($"[TilemapSinirBul] Tilemap Bounds - Min Cell: {bounds.min}, Max Cell: {bounds.max}");
                Debug.Log($"[TilemapSinirBul] World Bounds - Min World: {minWorld}, Max World: {maxWorld}");
            }
            else
            {
                Debug.LogError("Deniz objesinde Tilemap yok!");
            }
        }
        else
        {
            Debug.LogError("Deniz objesi bulunamadı!");
        }
    }
}
