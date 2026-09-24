using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.IO;

public static class NpcPrefabAyarla
{
    private const string PREFAB_YOLU = "Assets/Prefabs/Npc.prefab";

    [MenuItem("OnePirate/Npc Prefabini Yapilandir")]
    public static void Yapilandir()
    {
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isStageOpen = stage != null && stage.prefabAssetPath == PREFAB_YOLU;
        GameObject prefabRoot;

        if (isStageOpen)
        {
            prefabRoot = stage.prefabContentsRoot;
        }
        else if (File.Exists(PREFAB_YOLU))
        {
            prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_YOLU);
        }
        else
        {
            prefabRoot = new GameObject("Npc");
        }

        if (prefabRoot == null)
        {
            Debug.LogError($"[NpcPrefabAyarla] Prefab oluşturulamadı/yüklenemedi: {PREFAB_YOLU}");
            return;
        }

        try
        {
            Debug.Log("[NpcPrefabAyarla] NPC prefabı GDD'ye göre yapılandırılıyor...");

            // 1. SpriteRenderer Ekle / Ayarla
            SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
            if (sr == null) sr = prefabRoot.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            // NPC için Korsan gemi sprite'ını ata (kk1 veya korsan)
            Sprite npcSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/Oyun/Korsan/kk1.png");
            if (npcSprite != null)
            {
                sr.sprite = npcSprite;
            }

            // 2. Rigidbody2D (Gemi ve NPC'lerin içinden geçebilmesi için)
            Rigidbody2D rb = prefabRoot.GetComponent<Rigidbody2D>();
            if (rb == null) rb = prefabRoot.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Z rotasyonunu kilitle

            // 3. BoxCollider2D (Hedef seçimi / tıklama için)
            BoxCollider2D col = prefabRoot.GetComponent<BoxCollider2D>();
            if (col == null) col = prefabRoot.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);
            col.isTrigger = false;

            // 4. Mirror NetworkIdentity
            if (!prefabRoot.TryGetComponent<NetworkIdentity>(out var netIdentity))
                netIdentity = prefabRoot.AddComponent<NetworkIdentity>();

            // 5. NPC Scriptleri
            if (!prefabRoot.TryGetComponent<NpcHareket>(out var harekeComp))
                harekeComp = prefabRoot.AddComponent<NpcHareket>();

            if (!prefabRoot.TryGetComponent<NpcHasar>(out var hasarComp))
                hasarComp = prefabRoot.AddComponent<NpcHasar>();

            if (!prefabRoot.TryGetComponent<NpcOdul>(out var odulComp))
                odulComp = prefabRoot.AddComponent<NpcOdul>();

            if (!prefabRoot.TryGetComponent<HaritaSiniri>(out var haritaSiniriComp))
                haritaSiniriComp = prefabRoot.AddComponent<HaritaSiniri>();


            // 6. GemiAltiUI Canvas (Gemi deseninin altında yer alır)
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
            canvasRect.localPosition = new Vector3(0f, -0.65f, 0f);
            canvasRect.sizeDelta = new Vector2(160f, 50f);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 1f);

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            // 7. Can Barı (En üstte)
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
            canBariRect.anchoredPosition = new Vector2(0f, 14f);
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

            // Dolgu Alanı & Dolgu
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
            dolguImg.color = new Color(0.18f, 0.85f, 0.25f, 1f);

            // Slider kullanılmıyor — dolgu RectTransform anchor'larıyla NpcHasar tarafından yönetilir

            // Can Metni — gizli kalacak (NpcHasar.ArayuzuGuncelle içinde SetActive(false))
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
            canYaziObj.SetActive(false); // Baştan gizle

            // 8. NPC Adı (Can barının altında)
            Transform adTrans = gemiAltiObj.transform.Find("NpcAdi");
            GameObject adObj;
            if (adTrans == null)
            {
                adObj = new GameObject("NpcAdi");
                adObj.transform.SetParent(gemiAltiObj.transform, false);
            }
            else
            {
                adObj = adTrans.gameObject;
            }
            RectTransform adRect = adObj.GetComponent<RectTransform>();
            if (adRect == null) adRect = adObj.AddComponent<RectTransform>();
            adRect.anchoredPosition = new Vector2(0f, 2f);
            adRect.sizeDelta = new Vector2(120f, 14f);

            TextMeshProUGUI adTmp = adObj.GetComponent<TextMeshProUGUI>();
            if (adTmp == null) adTmp = adObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) adTmp.font = fontAsset;
            adTmp.fontSize = 10;
            adTmp.fontStyle = FontStyles.Bold;
            adTmp.alignment = TextAlignmentOptions.Center;
            adTmp.color = Color.white;
            adTmp.text = "Boris Juckson";

            // 9. SerializedObject ile Referansları Bağla
            SerializedObject soHasar = new SerializedObject(hasarComp);
            soHasar.FindProperty("canSlider").objectReferenceValue = null; // Slider kullanılmıyor
            soHasar.FindProperty("canDolgusu").objectReferenceValue = dolguImg;
            soHasar.FindProperty("canDolgusuRect").objectReferenceValue = dolguRect;
            soHasar.FindProperty("canYazisi").objectReferenceValue = canTmp;
            soHasar.FindProperty("npcAdiYazisi").objectReferenceValue = adTmp;
            soHasar.FindProperty("guncelCan").intValue = 2000;
            soHasar.FindProperty("maksCan").intValue = 2000;
            soHasar.FindProperty("npcAdi").stringValue = "Boris Juckson";
            soHasar.ApplyModifiedProperties();

            // 10. Prefabı Kaydet
            if (isStageOpen)
            {
                EditorUtility.SetDirty(prefabRoot);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(stage.scene);
                Debug.Log("[NpcPrefabAyarla] Açık olan NPC Prefab Stage başarıyla güncellendi!");
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_YOLU);
                Debug.Log("[NpcPrefabAyarla] NPC prefabı başarıyla kaydedildi: " + PREFAB_YOLU);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NpcPrefabAyarla] Hata oluştu: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            if (!isStageOpen && prefabRoot != null && File.Exists(PREFAB_YOLU))
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
            else if (!isStageOpen && prefabRoot != null)
            {
                Object.DestroyImmediate(prefabRoot);
            }
        }
    }
}
