using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;

public static class OyuncuPrefabAyarla
{
    private const string PREFAB_YOLU = "Assets/Prefabs/Oyuncu.prefab";

    [MenuItem("OnePirate/Oyuncu Prefabini Yapilandir")]
    public static void Yapilandir()
    {
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isStageOpen = stage != null && stage.prefabAssetPath == PREFAB_YOLU;
        GameObject prefabRoot = isStageOpen ? stage.prefabContentsRoot : PrefabUtility.LoadPrefabContents(PREFAB_YOLU);

        if (prefabRoot == null)
        {
            Debug.LogError($"[OyuncuPrefabAyarla] Prefab yüklenemedi: {PREFAB_YOLU}");
            return;
        }

        try
        {
            Debug.Log("[OyuncuPrefabAyarla] Oyuncu prefabı GDD'ye göre (gemi altına) yapılandırılıyor...");

            // 1. Root Bileşenlerini Ekle / Bul
            if (!prefabRoot.TryGetComponent<NetworkIdentity>(out var netIdentity))
                netIdentity = prefabRoot.AddComponent<NetworkIdentity>();

            if (!prefabRoot.TryGetComponent<OyuncuCanBari>(out var canBariComp))
                canBariComp = prefabRoot.AddComponent<OyuncuCanBari>();

            if (!prefabRoot.TryGetComponent<OyuncuKullaniciAdi>(out var kullaniciAdiComp))
                kullaniciAdiComp = prefabRoot.AddComponent<OyuncuKullaniciAdi>();

            if (!prefabRoot.TryGetComponent<OyuncuSeviye>(out var seviyeComp))
                seviyeComp = prefabRoot.AddComponent<OyuncuSeviye>();

            if (!prefabRoot.TryGetComponent<OyuncuFilo>(out var filoComp))
                filoComp = prefabRoot.AddComponent<OyuncuFilo>();

            if (!prefabRoot.TryGetComponent<GulleSecimi>(out var gulleSecimiComp))
                gulleSecimiComp = prefabRoot.AddComponent<GulleSecimi>();

            if (!prefabRoot.TryGetComponent<OyuncuKamera>(out var kameraComp))
                kameraComp = prefabRoot.AddComponent<OyuncuKamera>();

            if (!prefabRoot.TryGetComponent<OyuncuHedefSecimi>(out var hedefSecimiComp))
                hedefSecimiComp = prefabRoot.AddComponent<OyuncuHedefSecimi>();

            if (!prefabRoot.TryGetComponent<OyuncuAtes>(out var atesComp))
                atesComp = prefabRoot.AddComponent<OyuncuAtes>();

            if (!prefabRoot.TryGetComponent<HaritaSiniri>(out var haritaSiniriComp))
                haritaSiniriComp = prefabRoot.AddComponent<HaritaSiniri>();

            // Rigidbody2D: Kinematic (fizik motorunu kullanır ama yerçekimi/kuvvet yok)
            // FreezeRotation: Karaya çarptığında Z rotasyonunu engelle
            Rigidbody2D rb = prefabRoot.GetComponent<Rigidbody2D>();
            if (rb == null) rb = prefabRoot.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // BoxCollider2D: Hedef seçimi için tıklama alanı
            BoxCollider2D col = prefabRoot.GetComponent<BoxCollider2D>();
            if (col == null) col = prefabRoot.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);
            col.isTrigger = false;



            Sprite hedefSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/BeyazIcon/Icon_PictoIcon_Target.Png");
            if (hedefSprite != null)
            {
                SerializedObject soHedef = new SerializedObject(hedefSecimiComp);
                soHedef.FindProperty("hedefGostergesiSprite").objectReferenceValue = hedefSprite;
                soHedef.ApplyModifiedProperties();
            }

            SerializedObject soKamera = new SerializedObject(kameraComp);
            soKamera.FindProperty("sinirlarAktif").boolValue = true;
            soKamera.FindProperty("minSinirlar").vector2Value = new Vector2(-50f, -70f);
            soKamera.FindProperty("maxSinirlar").vector2Value = new Vector2(80f, 60f);
            soKamera.ApplyModifiedProperties();

            // Eski "BasUstuUI" varsa temizle veya yeniden adlandır
            Transform eskiBasUstu = prefabRoot.transform.Find("BasUstuUI");
            if (eskiBasUstu != null)
            {
                Object.DestroyImmediate(eskiBasUstu.gameObject);
            }

            // 2. GemiAltiUI Canvas Oluştur veya Bul (Gemi deseninin altında yer alır)
            Transform gemiAltiTransform = prefabRoot.transform.Find("GemiAltiUI");
            GameObject gemiAltiObj;
            if (gemiAltiTransform == null)
            {
                gemiAltiObj = new GameObject("GemiAltiUI");
                gemiAltiObj.transform.SetParent(prefabRoot.transform, false);
            }
            else
            {
                gemiAltiObj = gemiAltiTransform.gameObject;
            }

            Canvas canvas = gemiAltiObj.GetComponent<Canvas>();
            if (canvas == null) canvas = gemiAltiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = gemiAltiObj.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gemiAltiObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            RectTransform canvasRect = gemiAltiObj.GetComponent<RectTransform>();
            // Gemi görselinin altına konumlandırılır (Y = -0.65f)
            canvasRect.localPosition = new Vector3(0f, -0.65f, 0f);
            canvasRect.sizeDelta = new Vector2(160f, 70f);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 1f);

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            // -------------------------------------------------------------
            // 3. Gemi Deseni Altında: CAN BARI (En üstte)
            // -------------------------------------------------------------
            Transform canBariTransform = gemiAltiObj.transform.Find("CanBari");
            GameObject canBariObj;
            if (canBariTransform == null)
            {
                canBariObj = new GameObject("CanBari");
                canBariObj.transform.SetParent(gemiAltiObj.transform, false);
            }
            else
            {
                canBariObj = canBariTransform.gameObject;
            }

            RectTransform canBariRect = canBariObj.GetComponent<RectTransform>();
            if (canBariRect == null) canBariRect = canBariObj.AddComponent<RectTransform>();
            canBariRect.anchoredPosition = new Vector2(0f, 22f);
            canBariRect.sizeDelta = new Vector2(100f, 10f);

            // Slider component gerekli değil — can barı tamamen RectTransform anchor'larıyla yönetiliyor
            // Slider varsa kaldır (siyah arka plan artefaktına neden olur)
            Slider eskiSlider = canBariObj.GetComponent<Slider>();
            if (eskiSlider != null) Object.DestroyImmediate(eskiSlider);

            // Arka Plan
            Transform arkaPlanTrans = canBariObj.transform.Find("ArkaPlan");
            GameObject arkaPlanObj;
            if (arkaPlanTrans == null)
            {
                arkaPlanObj = new GameObject("ArkaPlan");
                arkaPlanObj.transform.SetParent(canBariObj.transform, false);
            }
            else
            {
                arkaPlanObj = arkaPlanTrans.gameObject;
            }
            RectTransform arkaPlanRect = arkaPlanObj.GetComponent<RectTransform>();
            if (arkaPlanRect == null) arkaPlanRect = arkaPlanObj.AddComponent<RectTransform>();
            arkaPlanRect.anchorMin = Vector2.zero;
            arkaPlanRect.anchorMax = Vector2.one;
            arkaPlanRect.sizeDelta = Vector2.zero;
            Image arkaPlanImg = arkaPlanObj.GetComponent<Image>();
            if (arkaPlanImg == null) arkaPlanImg = arkaPlanObj.AddComponent<Image>();
            arkaPlanImg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            // Dolgu Alanı & Dolgu (GDD: Hem soldan hem sağdan küçülür)
            Transform dolguAlaniTrans = canBariObj.transform.Find("DolguAlani");
            GameObject dolguAlaniObj;
            if (dolguAlaniTrans == null)
            {
                dolguAlaniObj = new GameObject("DolguAlani");
                dolguAlaniObj.transform.SetParent(canBariObj.transform, false);
            }
            else
            {
                dolguAlaniObj = dolguAlaniTrans.gameObject;
            }
            RectTransform dolguAlaniRect = dolguAlaniObj.GetComponent<RectTransform>();
            if (dolguAlaniRect == null) dolguAlaniRect = dolguAlaniObj.AddComponent<RectTransform>();
            dolguAlaniRect.anchorMin = Vector2.zero;
            dolguAlaniRect.anchorMax = Vector2.one;
            dolguAlaniRect.sizeDelta = Vector2.zero;

            Transform dolguTrans = dolguAlaniObj.transform.Find("Dolgu");
            GameObject dolguObj;
            if (dolguTrans == null)
            {
                dolguObj = new GameObject("Dolgu");
                dolguObj.transform.SetParent(dolguAlaniObj.transform, false);
            }
            else
            {
                dolguObj = dolguTrans.gameObject;
            }
            RectTransform dolguRect = dolguObj.GetComponent<RectTransform>();
            if (dolguRect == null) dolguRect = dolguObj.AddComponent<RectTransform>();
            dolguRect.anchorMin = Vector2.zero;
            dolguRect.anchorMax = Vector2.one;
            dolguRect.sizeDelta = Vector2.zero;
            Image dolguImg = dolguObj.GetComponent<Image>();
            if (dolguImg == null) dolguImg = dolguObj.AddComponent<Image>();
            dolguImg.color = new Color(0.18f, 0.85f, 0.25f, 1f); // Yeşil

            // Slider kullanılmıyor — dolgu RectTransform anchor'larıyla OyuncuCanBari tarafından yönetilir

            // Can Metni ("10000/10000")
            Transform canYaziTrans = canBariObj.transform.Find("CanMetni");
            GameObject canYaziObj;
            if (canYaziTrans == null)
            {
                canYaziObj = new GameObject("CanMetni");
                canYaziObj.transform.SetParent(canBariObj.transform, false);
            }
            else
            {
                canYaziObj = canYaziTrans.gameObject;
            }
            RectTransform canYaziRect = canYaziObj.GetComponent<RectTransform>();
            if (canYaziRect == null) canYaziRect = canYaziObj.AddComponent<RectTransform>();
            canYaziRect.anchorMin = Vector2.zero;
            canYaziRect.anchorMax = Vector2.one;
            canYaziRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI canTmp = canYaziObj.GetComponent<TextMeshProUGUI>();
            if (canTmp == null) canTmp = canYaziObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) canTmp.font = fontAsset;
            canTmp.fontSize = 8;
            canTmp.alignment = TextAlignmentOptions.Center;
            canTmp.color = Color.white;
            canTmp.text = "";
            canYaziObj.SetActive(false); // Baştan gizle — OyuncuCanBari.ArayuzuGuncelle içinde de gizli kalır

            // -------------------------------------------------------------
            // 4. Can Barının Altında: KULLANICI ADI
            // -------------------------------------------------------------
            Transform adTrans = gemiAltiObj.transform.Find("KullaniciAdi");
            GameObject adObj;
            if (adTrans == null)
            {
                adObj = new GameObject("KullaniciAdi");
                adObj.transform.SetParent(gemiAltiObj.transform, false);
            }
            else
            {
                adObj = adTrans.gameObject;
            }
            RectTransform adRect = adObj.GetComponent<RectTransform>();
            if (adRect == null) adRect = adObj.AddComponent<RectTransform>();
            adRect.anchoredPosition = new Vector2(0f, 9f);
            adRect.sizeDelta = new Vector2(120f, 14f);

            TextMeshProUGUI adTmp = adObj.GetComponent<TextMeshProUGUI>();
            if (adTmp == null) adTmp = adObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) adTmp.font = fontAsset;
            adTmp.fontSize = 10;
            adTmp.fontStyle = FontStyles.Bold;
            adTmp.alignment = TextAlignmentOptions.Center;
            adTmp.color = Color.white;
            adTmp.text = "Korsan";

            // -------------------------------------------------------------
            // 5. Kullanıcı Adının Altında: FİLO VE SEVİYE İKONU
            // GDD: "onun altında Filo ve yanında Seviye ikonu bulunur."
            // -------------------------------------------------------------
            Transform filoSeviyeTrans = gemiAltiObj.transform.Find("FiloVeSeviye");
            GameObject filoSeviyeObj;
            if (filoSeviyeTrans == null)
            {
                filoSeviyeObj = new GameObject("FiloVeSeviye");
                filoSeviyeObj.transform.SetParent(gemiAltiObj.transform, false);
            }
            else
            {
                filoSeviyeObj = filoSeviyeTrans.gameObject;
            }
            RectTransform filoSeviyeRect = filoSeviyeObj.GetComponent<RectTransform>();
            if (filoSeviyeRect == null) filoSeviyeRect = filoSeviyeObj.AddComponent<RectTransform>();
            filoSeviyeRect.anchoredPosition = new Vector2(0f, -4f);
            filoSeviyeRect.sizeDelta = new Vector2(100f, 14f);

            // Seviye İkonu
            Transform seviyeTrans = filoSeviyeObj.transform.Find("SeviyeIkonu");
            GameObject seviyeObj;
            if (seviyeTrans == null)
            {
                seviyeObj = new GameObject("SeviyeIkonu");
                seviyeObj.transform.SetParent(filoSeviyeObj.transform, false);
            }
            else
            {
                seviyeObj = seviyeTrans.gameObject;
            }
            RectTransform seviyeRect = seviyeObj.GetComponent<RectTransform>();
            if (seviyeRect == null) seviyeRect = seviyeObj.AddComponent<RectTransform>();
            seviyeRect.anchoredPosition = new Vector2(-22f, 0f);
            seviyeRect.sizeDelta = new Vector2(14f, 14f);

            Image seviyeImg = seviyeObj.GetComponent<Image>();
            if (seviyeImg == null) seviyeImg = seviyeObj.AddComponent<Image>();
            Sprite seviye1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/Seviye/1.png");
            if (seviye1Sprite != null) seviyeImg.sprite = seviye1Sprite;

            // Filo Tagı (3 karakter, Seviye ikonunun yanında)
            Transform filoTrans = filoSeviyeObj.transform.Find("FiloTagi");
            GameObject filoObj;
            if (filoTrans == null)
            {
                filoObj = new GameObject("FiloTagi");
                filoObj.transform.SetParent(filoSeviyeObj.transform, false);
            }
            else
            {
                filoObj = filoTrans.gameObject;
            }
            RectTransform filoRect = filoObj.GetComponent<RectTransform>();
            if (filoRect == null) filoRect = filoObj.AddComponent<RectTransform>();
            filoRect.anchoredPosition = new Vector2(12f, 0f);
            filoRect.sizeDelta = new Vector2(50f, 14f);

            TextMeshProUGUI filoTmp = filoObj.GetComponent<TextMeshProUGUI>();
            if (filoTmp == null) filoTmp = filoObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) filoTmp.font = fontAsset;
            filoTmp.fontSize = 10;
            filoTmp.fontStyle = FontStyles.Bold;
            filoTmp.alignment = TextAlignmentOptions.MidlineLeft;
            filoTmp.color = new Color(1f, 0.82f, 0.15f, 1f); // Altın sarısı
            filoTmp.text = "[FIL]";

            // -------------------------------------------------------------
            // 6. En Altta: MADALYALAR (Varsa)
            // GDD: "En alt da (varsa) Madalyalar yer alır. Birden fazla madalya varsa yan yana gösterilir."
            // -------------------------------------------------------------
            Transform madalyaTrans = gemiAltiObj.transform.Find("Madalyalar");
            GameObject madalyaObj;
            if (madalyaTrans == null)
            {
                madalyaObj = new GameObject("Madalyalar");
                madalyaObj.transform.SetParent(gemiAltiObj.transform, false);
            }
            else
            {
                madalyaObj = madalyaTrans.gameObject;
            }
            RectTransform madalyaRect = madalyaObj.GetComponent<RectTransform>();
            if (madalyaRect == null) madalyaRect = madalyaObj.AddComponent<RectTransform>();
            madalyaRect.anchoredPosition = new Vector2(0f, -17f);
            madalyaRect.sizeDelta = new Vector2(100f, 12f);

            HorizontalLayoutGroup layoutGroup = madalyaObj.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null) layoutGroup = madalyaObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.spacing = 3f;

            // 7. Seviye İkonlarını Listele
            List<Sprite> ikonListesi = new List<Sprite>();
            for (int i = 1; i <= 10; i++)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Texture2D/Oyun/Seviye/{i}.png");
                if (s != null) ikonListesi.Add(s);
            }

            // 8. Script Referanslarını Bağla (SerializedObject kullanarak)
            SerializedObject soCan = new SerializedObject(canBariComp);
            soCan.FindProperty("canSlider").objectReferenceValue = null; // Slider kullanılmıyor
            soCan.FindProperty("canDolgusu").objectReferenceValue = dolguImg;
            soCan.FindProperty("canDolgusuRect").objectReferenceValue = dolguRect;
            soCan.FindProperty("canYazisi").objectReferenceValue = canTmp;
            soCan.FindProperty("guncelCan").intValue = 10000;
            soCan.FindProperty("maksCan").intValue = 10000;
            soCan.ApplyModifiedProperties();

            SerializedObject soAd = new SerializedObject(kullaniciAdiComp);
            soAd.FindProperty("kullaniciAdiYazisi").objectReferenceValue = adTmp;
            soAd.ApplyModifiedProperties();

            SerializedObject soSeviye = new SerializedObject(seviyeComp);
            soSeviye.FindProperty("seviyeIkonu").objectReferenceValue = seviyeImg;
            SerializedProperty propIkonlar = soSeviye.FindProperty("seviyeIkonlari");
            propIkonlar.arraySize = ikonListesi.Count;
            for (int i = 0; i < ikonListesi.Count; i++)
            {
                propIkonlar.GetArrayElementAtIndex(i).objectReferenceValue = ikonListesi[i];
            }
            soSeviye.ApplyModifiedProperties();

            SerializedObject soFilo = new SerializedObject(filoComp);
            soFilo.FindProperty("filoTagiYazisi").objectReferenceValue = filoTmp;
            soFilo.ApplyModifiedProperties();

            SerializedObject soGulleSecimi = new SerializedObject(gulleSecimiComp);
            soGulleSecimi.FindProperty("alevGullesi").intValue = 10000;
            soGulleSecimi.FindProperty("buzGullesi").intValue = 10000;
            soGulleSecimi.FindProperty("kuruKafaGullesi").intValue = 10000;
            soGulleSecimi.FindProperty("asitGullesi").intValue = 10000;
            soGulleSecimi.ApplyModifiedProperties();

            SerializedObject soAtes = new SerializedObject(atesComp);
            soAtes.FindProperty("demirGullePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gulle/DemirGulle.prefab");
            soAtes.FindProperty("alevGullePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gulle/AlevGulle.prefab");
            soAtes.FindProperty("buzGullePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gulle/BuzGulle.prefab");
            soAtes.FindProperty("kuruKafaGullePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gulle/KuruKafaGulle.prefab");
            soAtes.FindProperty("asitGullePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gulle/AsitGulle.prefab");
            soAtes.ApplyModifiedProperties();

            // 9. Prefabı Kaydet
            if (isStageOpen)
            {
                EditorUtility.SetDirty(prefabRoot);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(stage.scene);
                Debug.Log("[OyuncuPrefabAyarla] Açık olan Prefab Stage başarıyla güncellendi!");
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_YOLU);
                Debug.Log("[OyuncuPrefabAyarla] Oyuncu prefabı GDD'ye göre (gemi altına) başarıyla güncellendi ve kaydedildi!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OyuncuPrefabAyarla] Hata oluştu: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            if (!isStageOpen && prefabRoot != null)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
