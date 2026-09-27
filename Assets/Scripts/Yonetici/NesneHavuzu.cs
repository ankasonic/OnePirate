using System.Collections.Generic;
using UnityEngine;

public static class NesneHavuzu
{
    private static Dictionary<string, Queue<GameObject>> havuzlar = new Dictionary<string, Queue<GameObject>>();
    private static Dictionary<GameObject, string> nesneKimlikleri = new Dictionary<GameObject, string>();

    public static GameObject Al(GameObject prefab, Vector3 pozisyon, Quaternion rotasyon)
    {
        string anahtar = prefab.name;

        if (!havuzlar.ContainsKey(anahtar))
        {
            havuzlar[anahtar] = new Queue<GameObject>();
        }

        GameObject obj = null;

        // Geçerli bir nesne bulana kadar kuyruktan çek (scene'de yok edilmiş objeleri atla)
        while (havuzlar[anahtar].Count > 0)
        {
            obj = havuzlar[anahtar].Dequeue();
            if (obj != null)
            {
                break;
            }
        }

        if (obj != null)
        {
            obj.transform.position = pozisyon;
            obj.transform.rotation = rotasyon;
            obj.SetActive(true);
            
            // Eğer nesnede TrailRenderer varsa, izin kalıntısını temizlemek için sıfırla
            TrailRenderer trail = obj.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
            }
            
            return obj;
        }

        // Eğer havuzda hiç aktif nesne kalmadıysa yeni üret
        GameObject yeniObj = Object.Instantiate(prefab, pozisyon, rotasyon);
        yeniObj.name = prefab.name; // İsimden eşleşme için
        nesneKimlikleri[yeniObj] = anahtar;
        return yeniObj;
    }

    public static void Birak(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        
        string anahtar;
        if (nesneKimlikleri.TryGetValue(obj, out anahtar))
        {
            if (!havuzlar.ContainsKey(anahtar))
            {
                havuzlar[anahtar] = new Queue<GameObject>();
            }
            havuzlar[anahtar].Enqueue(obj);
        }
        else
        {
            // Eğer havuz sistemi dışından Instantiate edildiyse ama buraya gönderildiyse:
            anahtar = obj.name.Replace("(Clone)", "").Trim();
            if (!havuzlar.ContainsKey(anahtar))
            {
                havuzlar[anahtar] = new Queue<GameObject>();
            }
            nesneKimlikleri[obj] = anahtar;
            havuzlar[anahtar].Enqueue(obj);
        }
    }
}