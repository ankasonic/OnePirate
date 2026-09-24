using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public static class GullePrefabAyarla
{
    private const string GULLE_PREFAB_KLASORU = "Assets/Prefabs/Gulle";
    private const string HASAR_YAZISI_PREFAB_YOLU = "Assets/Prefabs/Gulle/HasarYazisi.prefab";

    [MenuItem("OnePirate/Gulle Prefablarini Yapilandir")]
    public static void Yapilandir()
    {
        if (!Directory.Exists(GULLE_PREFAB_KLASORU))
        {
            Directory.CreateDirectory(GULLE_PREFAB_KLASORU);
            AssetDatabase.Refresh();
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // 1. HasarYazisi Prefabını Oluştur / Güncelle
        GameObject hasarYazisiPrefab = HasarYazisiPrefabiniOlustur(fontAsset);

        // 2. 5 Adet Gülle Prefabını Oluştur / Güncelle
        GullePrefabiOlustur("DemirGulle", GulleTuru.Demir, "Assets/Texture2D/Oyun/Market/Gulle/DemirGulle.png", hasarYazisiPrefab);
        GullePrefabiOlustur("AlevGulle", GulleTuru.Alev, "Assets/Texture2D/Oyun/Market/Gulle/AlevGulle.png", hasarYazisiPrefab);
        GullePrefabiOlustur("BuzGulle", GulleTuru.Buz, "Assets/Texture2D/Oyun/Market/Gulle/Buz.png", hasarYazisiPrefab);
        GullePrefabiOlustur("KuruKafaGulle", GulleTuru.KuruKafa, "Assets/Texture2D/Oyun/Market/Gulle/kurukafa.png", hasarYazisiPrefab);
        GullePrefabiOlustur("AsitGulle", GulleTuru.Asit, "Assets/Texture2D/Oyun/Market/Gulle/Asit.png", hasarYazisiPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GullePrefabAyarla] Tüm gülle prefabları ve HasarYazisi prefabı başarıyla oluşturuldu!");
    }

    private static GameObject HasarYazisiPrefabiniOlustur(TMP_FontAsset fontAsset)
    {
        GameObject root = new GameObject("HasarYazisi");

        HasarYazisi yaziComp = root.AddComponent<HasarYazisi>();
        TextMeshPro tmp = root.AddComponent<TextMeshPro>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.fontSize = 10;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 25; // Gemilerin ve suyun üstünde net görünsün

        SerializedObject so = new SerializedObject(yaziComp);
        so.FindProperty("yaziText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HASAR_YAZISI_PREFAB_YOLU);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void GullePrefabiOlustur(string prefabAdi, GulleTuru tur, string spriteYolu, GameObject hasarYazisiPrefab)
    {
        string hedefYol = $"{GULLE_PREFAB_KLASORU}/{prefabAdi}.prefab";
        GameObject gulleRoot = new GameObject(prefabAdi);

        // SpriteRenderer
        SpriteRenderer sr = gulleRoot.AddComponent<SpriteRenderer>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteYolu);
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        sr.sortingOrder = 20; // Deniz ve gemilerin üstünde uçar
        // 512x512 sprite'lar için 0.05f, 128x128 olanlar için 0.2f
        float scale = (sprite != null && sprite.rect.width > 200) ? 0.05f : 0.2f;
        gulleRoot.transform.localScale = new Vector3(scale, scale, 1f);

        // Gülle Mantık Bileşeni
        Gulle gulleComp = gulleRoot.AddComponent<Gulle>();
        SerializedObject so = new SerializedObject(gulleComp);
        so.FindProperty("gulleTuru").enumValueIndex = (int)tur;
        so.FindProperty("hiz").floatValue = 18f;
        so.FindProperty("hasarYazisiPrefab").objectReferenceValue = hasarYazisiPrefab;
        so.ApplyModifiedProperties();

        // TrailRenderer (Arkada hoş bir iz bırakması için)
        TrailRenderer tr = gulleRoot.AddComponent<TrailRenderer>();
        tr.time = 0.2f;
        tr.startWidth = 0.07f;
        tr.endWidth = 0.01f;
        tr.sortingOrder = 19;
        Material defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
        if (defaultMat != null) tr.material = defaultMat;

        GulleOzellikleri ozellik = GulleVeritabani.Getir(tur);
        tr.startColor = ozellik.gulleRengi;
        Color bitis = ozellik.gulleRengi;
        bitis.a = 0f;
        tr.endColor = bitis;

        PrefabUtility.SaveAsPrefabAsset(gulleRoot, hedefYol);
        Object.DestroyImmediate(gulleRoot);
        Debug.Log($"[GullePrefabAyarla] {prefabAdi}.prefab oluşturuldu: {hedefYol}");
    }
}
