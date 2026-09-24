using UnityEditor;
using UnityEngine;

public class TagAyarla
{
    [MenuItem("OnePirate/Gerekli Taglari Ekle")]
    public static void TaglariEkle()
    {
        string[] gerekliTaglar = new string[] { "NPC", "Kule", "Admin", "Player" };

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool degisiklikYapildi = false;

        foreach (string tag in gerekliTaglar)
        {
            bool bulundu = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tag))
                {
                    bulundu = true;
                    break;
                }
            }

            if (!bulundu)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty yeniTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                yeniTag.stringValue = tag;
                degisiklikYapildi = true;
                Debug.Log($"[TagAyarla] '{tag}' tagı eklendi.");
            }
        }

        if (degisiklikYapildi)
        {
            tagManager.ApplyModifiedProperties();
            Debug.Log("[TagAyarla] Tüm gerekli taglar başarıyla ayarlandı!");
        }
        else
        {
            Debug.Log("[TagAyarla] Gerekli taglar zaten mevcut.");
        }
    }
}
