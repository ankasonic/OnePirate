#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Oyun_UI sahnesine Seçenekler ikonu + 8 sekmeli paneli kurar (görsel placeholder).
/// Menü: Tools/OnePirate/Secenekler UI Olustur
/// </summary>
public static class CreateSeceneklerUI
{
    private static readonly Color PanelBg = new Color(0.06f, 0.09f, 0.12f, 0.96f);
    private static readonly Color KartBg = new Color(0.1f, 0.14f, 0.18f, 1f);
    private static readonly Color Altin = new Color(0.95f, 0.82f, 0.45f, 1f);
    private static readonly Color Krem = new Color(1f, 0.95f, 0.82f, 1f);
    private static readonly Color Turkuaz = new Color(0.25f, 0.7f, 0.72f, 1f);
    private static readonly Color KoyuKenar = new Color(0.15f, 0.22f, 0.28f, 1f);
    private static readonly Color Kirmizi = new Color(0.9f, 0.3f, 0.3f, 1f);

    private static TMP_FontAsset _font;

    [MenuItem("Tools/OnePirate/Secenekler UI Olustur")]
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Oyun_UI")
        {
            Debug.LogError("Lütfen Oyun_UI sahnesini açın.");
            return;
        }

        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas bulunamadı.");
            return;
        }

        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Righteous-Regular SDF.asset");

        DestroyIfExists("Canvas/SeceneklerAcButonu");
        DestroyIfExists("Canvas/SeceneklerPaneli");

        // ---- Açma butonu (sağ üst) ----
        var acBtn = CreatePanel("SeceneklerAcButonu", canvas.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-16f, -16f), new Vector2(56f, 56f), new Color(0.12f, 0.2f, 0.26f, 0.95f));
        var acButton = acBtn.AddComponent<Button>();
        var acText = CreateText(acBtn.transform, "Ikon", "☰", 28, TextAlignmentOptions.Center, Altin);
        Stretch(acText.rectTransform);

        // ---- Ana panel kökü ----
        var kok = CreatePanel("SeceneklerPaneli", canvas.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.55f));
        StretchFull(kok.GetComponent<RectTransform>());
        kok.SetActive(false);

        // Arka plana tıklayınca kapanır (AnaPanel raycast yutar)
        var overlayBtn = kok.GetComponent<Button>();
        if (overlayBtn == null) overlayBtn = kok.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;

        var ana = CreatePanel("AnaPanel", kok.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(1080f, 720f), PanelBg);
        // Ana panele tıklama yayılmasın
        var anaImg = ana.GetComponent<Image>();
        anaImg.raycastTarget = true;

        // Başlık çubuğu
        var baslikCubugu = CreatePanel("BaslikCubugu", ana.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, 0), new Vector2(0, 56f), new Color(0.08f, 0.12f, 0.16f, 1f));
        SetOffsets(baslikCubugu.GetComponent<RectTransform>(), 0, -56, 0, 0);

        CreateText(baslikCubugu.transform, "Baslik", "SEÇENEKLER", 26, TextAlignmentOptions.Center, Altin);

        var kapatObj = CreatePanel("KapatButonu", baslikCubugu.transform,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-12f, 0), new Vector2(40f, 40f), new Color(0.4f, 0.15f, 0.15f, 1f));
        var kapatBtn = kapatObj.AddComponent<Button>();
        CreateText(kapatObj.transform, "X", "X", 22, TextAlignmentOptions.Center, Color.white);

        // Sekme şeridi
        var sekmeSeridi = CreatePanel("SekmeSeridi", ana.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, -56f), new Vector2(0, 88f), new Color(0.07f, 0.1f, 0.13f, 1f));
        SetOffsets(sekmeSeridi.GetComponent<RectTransform>(), 12, -144, -12, -64);

        string[] sekmeAdlari =
        {
            "Profil", "İstatistik", "Ayarlar", "Hazinenin\nŞifresi",
            "Günlük\nÇark", "Seyir\nDefteri", "Filo", "Oyundan\nÇıkış"
        };
        string[] sekmeKisalt =
        {
            "PR", "İS", "AY", "HŞ", "ÇR", "SD", "FL", "ÇK"
        };

        var sekmeButonlari = new Button[8];
        float btnW = 118f;
        float gap = 8f;
        float totalW = 8 * btnW + 7 * gap;
        float startX = -totalW * 0.5f + btnW * 0.5f;

        for (int i = 0; i < 8; i++)
        {
            float x = startX + i * (btnW + gap);
            Color bg = i == 7 ? new Color(0.45f, 0.15f, 0.15f, 1f) : new Color(0.12f, 0.18f, 0.24f, 1f);
            var btnGo = CreatePanel($"Sekme_{i}_{sekmeKisalt[i]}", sekmeSeridi.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x, 0), new Vector2(btnW, 72f), bg);
            var btn = btnGo.AddComponent<Button>();
            sekmeButonlari[i] = btn;

            var ikon = CreateText(btnGo.transform, "Ikon", sekmeKisalt[i], 18, TextAlignmentOptions.Center, Altin);
            ikon.rectTransform.anchorMin = new Vector2(0, 0.45f);
            ikon.rectTransform.anchorMax = new Vector2(1, 1);
            ikon.rectTransform.offsetMin = Vector2.zero;
            ikon.rectTransform.offsetMax = Vector2.zero;

            var etiket = CreateText(btnGo.transform, "Etiket", sekmeAdlari[i], 11, TextAlignmentOptions.Center, Krem);
            etiket.rectTransform.anchorMin = new Vector2(0, 0);
            etiket.rectTransform.anchorMax = new Vector2(1, 0.48f);
            etiket.rectTransform.offsetMin = new Vector2(2, 2);
            etiket.rectTransform.offsetMax = new Vector2(-2, -2);
            etiket.textWrappingMode = TextWrappingModes.Normal;
            etiket.lineSpacing = -20f;
        }

        // İçerik alanı
        var icerikKok = CreatePanel("IcerikAlani", ana.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
        SetOffsets(icerikKok.GetComponent<RectTransform>(), 16, 16, -16, -152);
        icerikKok.GetComponent<Image>().raycastTarget = false;

        var icerikler = new GameObject[8];
        icerikler[0] = BuildProfil(icerikKok.transform);
        icerikler[1] = BuildIstatistik(icerikKok.transform);
        icerikler[2] = BuildAyarlar(icerikKok.transform);
        icerikler[3] = BuildHazineSifresi(icerikKok.transform);
        icerikler[4] = BuildGunlukCark(icerikKok.transform);
        icerikler[5] = BuildSeyirDefteri(icerikKok.transform);
        icerikler[6] = BuildFilo(icerikKok.transform);
        icerikler[7] = BuildCikis(icerikKok.transform);

        for (int i = 0; i < icerikler.Length; i++)
            icerikler[i].SetActive(i == 0);

        // Controller
        var ctrlGo = canvas;
        var eski = canvas.GetComponent<SeceneklerPaneli>();
        if (eski != null) Object.DestroyImmediate(eski);
        var ctrl = canvas.AddComponent<SeceneklerPaneli>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("panelKok").objectReferenceValue = kok;
        so.FindProperty("acmaButonu").objectReferenceValue = acButton;
        so.FindProperty("kapatButonu").objectReferenceValue = kapatBtn;
        so.FindProperty("arkaPlanKapatButonu").objectReferenceValue = overlayBtn;

        var cikisBtn = icerikler[7].GetComponentsInChildren<Button>(true);
        Button cikisOnay = null;
        foreach (var b in cikisBtn)
        {
            if (b.gameObject.name == "CikisOnayButonu")
            {
                cikisOnay = b;
                break;
            }
        }
        so.FindProperty("cikisOnayButonu").objectReferenceValue = cikisOnay;

        var sekmeProp = so.FindProperty("sekmeButonlari");
        sekmeProp.arraySize = 8;
        for (int i = 0; i < 8; i++)
            sekmeProp.GetArrayElementAtIndex(i).objectReferenceValue = sekmeButonlari[i];

        var icerikProp = so.FindProperty("sekmeIcerikleri");
        icerikProp.arraySize = 8;
        for (int i = 0; i < 8; i++)
            icerikProp.GetArrayElementAtIndex(i).objectReferenceValue = icerikler[i];

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Seçenekler UI oluşturuldu ve kaydedildi.");
    }

    // ----------------- İçerik builders -----------------

    private static GameObject BuildProfil(Transform parent)
    {
        var root = CreateScrollIcerik("ProfilIcerik", parent, out Transform content);

        string[] satirlar =
        {
            "Kullanıcı Adı", "KaptanSafa",
            "Seviye", "12",
            "ID Numarası", "#104829",
            "Filo Adı / Tag", "Kara Bayrak  [KB]",
            "Max HP", "4.500",
            "Tecrübe Puanı", "128.450",
            "Aldığı Madalyalar", "Cesaret • Deniz Kurdu • Avcı",
            "TP Sıralaması (Tüm Zamanlar)", "47.",
            "Batırılan NPC", "312",
            "Batırılan Oyuncu", "58",
            "NPC'lere Toplam Hasar", "1.245.000",
            "Oyunculara Toplam Hasar", "386.200",
            "Kayıt Tarihi", "1 Mayıs 2026",
            "Oyunda Geçen Süre", "142 sa 35 dk",
            "Tamamlanan Görev", "27",
            "Toplanan Hazine Sandığı", "94"
        };

        CreateBaslik(content, "PROFİL ALANI");
        CreateNot(content, "Bu bilgileri diğer tüm oyuncular görebilir. (Veriler yakında veritabanından gelecek.)");

        for (int i = 0; i < satirlar.Length; i += 2)
            CreateBilgiSatiri(content, satirlar[i], satirlar[i + 1]);

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildIstatistik(Transform parent)
    {
        var root = CreateScrollIcerik("IstatistikIcerik", parent, out Transform content);
        CreateBaslik(content, "İSTATİSTİK ALANI");
        CreateNot(content, "En iyi 100 oyuncu listelenir. Sizin sıranız kırmızı ile işaretlenir. İlk 100 dışındaysanız listenin altında ayrıca gösterilir. Aylık sıralamalar ayın son günü 23:59 (sunucu) sıfırlanır; tüm zamanlar asla sıfırlanmaz.");

        string[] kategoriler =
        {
            "Tecrübe Puanı (Tüm Zamanlar)",
            "Tecrübe Puanı (Aylık)",
            "Oyuncu Batırma (Tüm Zamanlar)",
            "Oyuncu Batırma (Aylık)",
            "NPC Batırma (Tüm Zamanlar)",
            "NPC Batırma (Aylık)"
        };

        foreach (var kat in kategoriler)
        {
            CreateAltBaslik(content, kat);
            CreateSiralamaOrnek(content);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildAyarlar(Transform parent)
    {
        var root = CreateScrollIcerik("AyarlarIcerik", parent, out Transform content);
        CreateBaslik(content, "AYARLAR ALANI");

        CreateAltBaslik(content, "Hesap / Güvenlik");
        CreateNot(content, "Kullanıcı adı değiştirme: 500 elmas • 7 günde 1 kez. Eski isim geçmişi tutulur (admin görür). Şifre / e-posta değiştirme, oturum yönetimi.");
        CreatePlaceholderButon(content, "Kullanıcı Adı Değiştir (500 Elmas)");
        CreatePlaceholderButon(content, "Şifre Değiştir");
        CreatePlaceholderButon(content, "E-posta Değiştir");
        CreatePlaceholderButon(content, "Oturum Yönetimi");

        CreateAltBaslik(content, "Ses Ayarları");
        CreateSliderSatiri(content, "Ana Ses", 0.8f);
        CreateSliderSatiri(content, "Müzik", 0.6f);
        CreateSliderSatiri(content, "Efekt", 0.9f);

        CreateAltBaslik(content, "Grafik Ayarları");
        CreateBilgiSatiri(content, "FPS Limiti", "60");
        CreateBilgiSatiri(content, "Çözünürlük / Ölçek", "1920×1080 • %100");
        CreateBilgiSatiri(content, "Efektler", "Açık");

        CreateAltBaslik(content, "Kontrol Ayarları");
        CreateBilgiSatiri(content, "Saldırı Tuşu", "Boşluk");
        CreateBilgiSatiri(content, "Gülle Seçimi", "1 – 5");
        CreateBilgiSatiri(content, "Zoom Hassasiyeti", "Orta");
        CreateBilgiSatiri(content, "Mouse Hızı", "Orta");

        CreateAltBaslik(content, "Yardım / Destek");
        CreatePlaceholderButon(content, "Sıkça Sorulan Sorular");
        CreatePlaceholderButon(content, "Hata Bildir");
        CreatePlaceholderButon(content, "Destek Talebi");
        CreatePlaceholderButon(content, "Oyun Rehberi");

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildHazineSifresi(Transform parent)
    {
        var root = CreateScrollIcerik("HazineSifresiIcerik", parent, out Transform content);
        CreateBaslik(content, "HAZİNENİN ŞİFRESİ");
        CreateNot(content, "Her kod hesap başına 1 kez. Süresi geçmiş / kullanılmış kod: \"Hazinenin şifresi daha önce kullanılmış veya süresi dolmuş\". 10 dk içinde en fazla 3 deneme; sonra 10 dk kilit.");

        var satir = CreatePanel("KodSatiri", content, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 48), new Color(0, 0, 0, 0));
        var le = satir.AddComponent<LayoutElement>();
        le.minHeight = 48; le.preferredHeight = 48;
        satir.GetComponent<Image>().raycastTarget = false;

        var hlg = satir.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.padding = new RectOffset(0, 0, 0, 0);

        var inputGo = new GameObject("KodInput", typeof(RectTransform));
        inputGo.transform.SetParent(satir.transform, false);
        var inputLe = inputGo.AddComponent<LayoutElement>();
        inputLe.flexibleWidth = 1; inputLe.minWidth = 200;
        var inputBg = inputGo.AddComponent<Image>();
        inputBg.color = new Color(0.08f, 0.1f, 0.14f, 1f);
        var input = inputGo.AddComponent<TMP_InputField>();
        var placeholder = CreateText(inputGo.transform, "Placeholder", "Kodunuzu girin...", 16, TextAlignmentOptions.Left, new Color(1, 1, 1, 0.35f));
        Stretch(placeholder.rectTransform);
        placeholder.margin = new Vector4(12, 0, 12, 0);
        var text = CreateText(inputGo.transform, "Text", "", 16, TextAlignmentOptions.Left, Krem);
        Stretch(text.rectTransform);
        text.margin = new Vector4(12, 0, 12, 0);
        input.textViewport = inputGo.GetComponent<RectTransform>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.fontAsset = _font;

        var kullan = CreatePanel("KullanButonu", satir.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 48), Turkuaz);
        var kullanLe = kullan.AddComponent<LayoutElement>();
        kullanLe.minWidth = 120; kullanLe.preferredWidth = 120;
        kullan.AddComponent<Button>();
        CreateText(kullan.transform, "Lbl", "Kullan", 18, TextAlignmentOptions.Center, Color.white);

        CreateAltBaslik(content, "Kullanım Geçmişi");
        CreateBilgiSatiri(content, "BAHAR2026", "12 May 2026 • 500 Altın + 50 Elmas");
        CreateBilgiSatiri(content, "DENIZATI", "3 May 2026 • 10 Alev Güllesi");
        CreateNot(content, "Deneme hakkı: 3 / 3  •  Kilit: yok (placeholder)");

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildGunlukCark(Transform parent)
    {
        var root = CreateScrollIcerik("GunlukCarkIcerik", parent, out Transform content);
        CreateBaslik(content, "GÜNLÜK ÇARK");
        CreateNot(content, "24 saatte 1 çevirme. Hak birikmez, ertesi güne devretmez. Ödüller: Altın, Elmas, Gülle, Barut, Kalkan, Fosfor, Tasarım. Oranlar admin tarafından belirlenir.");

        var cark = CreatePanel("CarkGorsel", content, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 200), new Color(0.08f, 0.14f, 0.18f, 1f));
        var cle = cark.AddComponent<LayoutElement>();
        cle.minHeight = 200; cle.preferredHeight = 200;
        CreateText(cark.transform, "CarkEmoji", "✦  ÇARK  ✦", 36, TextAlignmentOptions.Center, Altin);
        CreateText(cark.transform, "OdulOrnek", "Ödül dilimleri (placeholder)", 14, TextAlignmentOptions.Bottom, new Color(1, 1, 1, 0.5f));

        CreatePlaceholderButon(content, "Çarkı Çevir");
        CreateBilgiSatiri(content, "Sonraki Çevirme", "14:32:08");
        CreateAltBaslik(content, "Son 7 Gün Geçmişi");
        CreateBilgiSatiri(content, "Bugün", "250 Altın");
        CreateBilgiSatiri(content, "Dün", "1 Elmas");
        CreateBilgiSatiri(content, "2 gün önce", "5 Barut");

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildSeyirDefteri(Transform parent)
    {
        var root = CreateScrollIcerik("SeyirDefteriIcerik", parent, out Transform content);
        CreateBaslik(content, "SEYİR DEFTERİ");
        CreateNot(content, "Son 30 eylem. Kayıtlar istemcide 1 ay tutulur, otomatik silinir. Oyuncu silemez.");

        string[] kayitlar =
        {
            "İhale kazandın", "İhaleyi kazandın. 10000 Alev Güllesi hesabına eklendi.",
            "Seviye atladın", "Tebrikler! 3. Seviye'ye ulaştın.",
            "NPC batırdın", "Kızıl Ejder yok edildi. +1200 XP, +350 Altın.",
            "Sandık topladın", "Hazine sandığı: 2 Elmas + Tuzak! (-200 HP)",
            "Kule yıktın", "Düşman kulesi yıkıldı. Filo puanı +50.",
            "Görev tamamlandı", "Görev: 'Kıyı Devriyesi' tamamlandı."
        };

        for (int i = 0; i < kayitlar.Length; i += 2)
        {
            CreateAltBaslik(content, kayitlar[i]);
            CreateNot(content, kayitlar[i + 1]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildFilo(Transform parent)
    {
        var root = CreateScrollIcerik("FiloIcerik", parent, out Transform content);
        CreateBaslik(content, "FİLO ALANI");

        CreateBilgiSatiri(content, "Filo Adı / Tag", "Kara Bayrak  [KB]");
        CreateBilgiSatiri(content, "Kuruluş Tarihi", "12 Mart 2026");
        CreateBilgiSatiri(content, "Filo Lideri", "KaptanSafa");
        CreateBilgiSatiri(content, "Lider Yardımcısı", "DenizKurt");
        CreateBilgiSatiri(content, "Üye Sayısı", "8 / 20");
        CreateBilgiSatiri(content, "Filo Puanı / Sıra", "842.100 TP  •  12.");
        CreateBilgiSatiri(content, "Sahip Olunan Haritalar", "1-1, 2-1");

        CreateAltBaslik(content, "Filo Üyeleri");
        CreateBilgiSatiri(content, "1. KaptanSafa", "128.450 TP  •  Lider");
        CreateBilgiSatiri(content, "2. DenizKurt", "97.200 TP  •  Yardımcı");
        CreateBilgiSatiri(content, "3. MaviOfke", "64.800 TP");
        CreateBilgiSatiri(content, "4. KaraYelken", "51.100 TP");

        CreatePlaceholderButon(content, "Filodan Çık");
        CreateNot(content, "Filoda değilseniz burada \"Filo Oluştur\" butonu görünür. (placeholder)");
        CreatePlaceholderButon(content, "Filo Oluştur");

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    private static GameObject BuildCikis(Transform parent)
    {
        var root = CreateScrollIcerik("CikisIcerik", parent, out Transform content);
        CreateBaslik(content, "OYUNDAN ÇIKIŞ");
        CreateNot(content, "5 saniye hasar almayan gemiler için son harita ve koordinat veritabanına kaydedilir, oturum kapatılır. Hasar altındayken çıkış yapılamaz.");

        var uyari = CreatePanel("Uyari", content, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 80), new Color(0.35f, 0.12f, 0.12f, 1f));
        var ule = uyari.AddComponent<LayoutElement>();
        ule.minHeight = 80; ule.preferredHeight = 80;
        CreateText(uyari.transform, "Msg", "Çıkış yapmak istediğinize emin misiniz?\nİlerleme kaydı sunucuya yazılacak.", 16, TextAlignmentOptions.Center, Krem);

        var cikisBtn = CreatePanel("CikisOnayButonu", content, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 56), new Color(0.7f, 0.18f, 0.18f, 1f));
        var cle = cikisBtn.AddComponent<LayoutElement>();
        cle.minHeight = 56; cle.preferredHeight = 56;
        cikisBtn.AddComponent<Button>();
        CreateText(cikisBtn.transform, "Lbl", "Oyundan Çıkış Yap", 20, TextAlignmentOptions.Center, Color.white);

        CreateBilgiSatiri(content, "Durum", "Hasar yok • Çıkışa hazır (placeholder)");
        CreateBilgiSatiri(content, "Kayıt edilecek", "Harita 1-1  •  AA 11");

        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        return root;
    }

    // ----------------- UI helpers -----------------

    private static GameObject CreateScrollIcerik(string name, Transform parent, out Transform content)
    {
        var root = CreatePanel(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, KartBg);
        StretchFull(root.GetComponent<RectTransform>());

        var scroll = root.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        var viewport = CreatePanel("Viewport", root.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, Color.white);
        StretchFull(viewport.GetComponent<RectTransform>());
        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(viewport.transform, false);
        var crt = contentGo.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 24);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = crt;

        content = contentGo.transform;
        return root;
    }

    private static void CreateBaslik(Transform parent, string text)
    {
        var t = CreateText(parent, "Baslik", text, 22, TextAlignmentOptions.Center, Altin);
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 36; le.preferredHeight = 36;
    }

    private static void CreateAltBaslik(Transform parent, string text)
    {
        var t = CreateText(parent, "AltBaslik", text, 16, TextAlignmentOptions.Left, Turkuaz);
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 28; le.preferredHeight = 28;
        t.margin = new Vector4(4, 8, 4, 0);
    }

    private static void CreateNot(Transform parent, string text)
    {
        var t = CreateText(parent, "Not", text, 13, TextAlignmentOptions.Left, new Color(0.85f, 0.85f, 0.8f, 0.85f));
        t.textWrappingMode = TextWrappingModes.Normal;
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 40; le.preferredHeight = 48;
        t.margin = new Vector4(4, 0, 4, 4);
    }

    private static void CreateBilgiSatiri(Transform parent, string etiket, string deger)
    {
        var row = CreatePanel("Satir", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 34), new Color(0.08f, 0.11f, 0.14f, 1f));
        var le = row.AddComponent<LayoutElement>();
        le.minHeight = 34; le.preferredHeight = 34;

        var e = CreateText(row.transform, "Etiket", etiket, 13, TextAlignmentOptions.Left, new Color(0.7f, 0.75f, 0.7f, 1f));
        e.rectTransform.anchorMin = new Vector2(0, 0);
        e.rectTransform.anchorMax = new Vector2(0.48f, 1);
        e.rectTransform.offsetMin = new Vector2(10, 0);
        e.rectTransform.offsetMax = Vector2.zero;

        var d = CreateText(row.transform, "Deger", deger, 13, TextAlignmentOptions.Right, Krem);
        d.rectTransform.anchorMin = new Vector2(0.48f, 0);
        d.rectTransform.anchorMax = new Vector2(1, 1);
        d.rectTransform.offsetMin = Vector2.zero;
        d.rectTransform.offsetMax = new Vector2(-10, 0);
    }

    private static void CreateSiralamaOrnek(Transform parent)
    {
        string[] satir =
        {
            "1. DenizLordu", "245.000",
            "2. KaraFilo", "231.400",
            "3. MaviFırtına", "198.200",
            "47. KaptanSafa", "128.450  ← SİZ"
        };
        for (int i = 0; i < satir.Length; i += 2)
        {
            bool siz = satir[i].Contains("KaptanSafa");
            var row = CreatePanel("Rank", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
                Vector2.zero, new Vector2(0, 28), siz ? new Color(0.35f, 0.1f, 0.1f, 1f) : new Color(0.08f, 0.11f, 0.14f, 0.8f));
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 28; le.preferredHeight = 28;
            var col = siz ? Kirmizi : Krem;
            var t = CreateText(row.transform, "T", $"{satir[i]}    {satir[i + 1]}", 12, TextAlignmentOptions.Left, col);
            Stretch(t.rectTransform);
            t.margin = new Vector4(12, 0, 12, 0);
        }
        CreateNot(parent, "İlk 100 dışındaysanız: Güncel sıranız #247 burada gösterilir. (placeholder)");
    }

    private static void CreatePlaceholderButon(Transform parent, string label)
    {
        var btn = CreatePanel("Btn", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 40), KoyuKenar);
        var le = btn.AddComponent<LayoutElement>();
        le.minHeight = 40; le.preferredHeight = 40;
        btn.AddComponent<Button>();
        CreateText(btn.transform, "Lbl", label, 14, TextAlignmentOptions.Center, Altin);
    }

    private static void CreateSliderSatiri(Transform parent, string etiket, float deger)
    {
        CreateBilgiSatiri(parent, etiket, $"{Mathf.RoundToInt(deger * 100)}%");
        var row = CreatePanel("Slider", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 1),
            Vector2.zero, new Vector2(0, 24), new Color(0, 0, 0, 0));
        var le = row.AddComponent<LayoutElement>();
        le.minHeight = 24; le.preferredHeight = 24;
        row.GetComponent<Image>().raycastTarget = false;

        var bg = CreatePanel("Track", row.transform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(-20, 8), new Color(0.15f, 0.2f, 0.25f, 1f));
        Stretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<RectTransform>().offsetMin = new Vector2(10, 8);
        bg.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -8);

        var fill = CreatePanel("Fill", bg.transform, Vector2.zero, new Vector2(deger, 1), new Vector2(0, 0.5f),
            Vector2.zero, Vector2.zero, Turkuaz);
        StretchFull(fill.GetComponent<RectTransform>());
        fill.GetComponent<RectTransform>().anchorMax = new Vector2(deger, 1);
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 amin, Vector2 amax, Vector2 pivot,
        Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
        TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        Stretch(tmp.rectTransform);
        return tmp;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static void SetOffsets(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
    }

    private static void DestroyIfExists(string path)
    {
        var go = GameObject.Find(path);
        if (go != null) Object.DestroyImmediate(go);
    }
}
#endif
