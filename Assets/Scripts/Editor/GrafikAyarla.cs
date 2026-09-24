using UnityEngine;
using UnityEditor;
using System.IO;

public static class GrafikAyarla
{
    [MenuItem("OnePirate/Grafik Kalitesini Iyilestir (Maksimum Kalite)")]
    public static void MaksimumKaliteyeAyarla()
    {
        Debug.Log("[GrafikAyarla] Grafik kalitesi maksimum seviyeye yukseltiyor...");

        // 1. Proje Seviyesi Kalite Ayarlari
        QualitySettings.antiAliasing = 4; // 4x MSAA Anti-Aliasing (kenar yumusatma)
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.globalTextureMipmapLimit = 0; // Kaplamalari tam cozunurlukte tut

        // 2. Texture2D Klasorundeki Tum Gorselleri Tara ve Yuksek Kaliteye Ayarla
        string textureKlasoru = "Assets/Texture2D";
        if (Directory.Exists(textureKlasoru))
        {
            string[] dosyalar = Directory.GetFiles(textureKlasoru, "*.*", SearchOption.AllDirectories);
            int guncellenenSayisi = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string dosya in dosyalar)
                {
                    string uzanti = Path.GetExtension(dosya).ToLower();
                    if (uzanti == ".png" || uzanti == ".jpg" || uzanti == ".jpeg" || uzanti == ".psd")
                    {
                        string unityYolu = dosya.Replace("\\", "/");
                        TextureImporter importer = AssetImporter.GetAtPath(unityYolu) as TextureImporter;

                        if (importer != null)
                        {
                            bool degisti = false;

                            // 2D Sprite'larda MipMap bulanikliga yol acar, kapali olmali
                            if (importer.mipmapEnabled)
                            {
                                importer.mipmapEnabled = false;
                                degisti = true;
                            }

                            // Keskinlik icin Anisotropic filtreleme
                            if (importer.anisoLevel < 16)
                            {
                                importer.anisoLevel = 16;
                                degisti = true;
                            }

                            if (importer.filterMode != FilterMode.Bilinear)
                            {
                                importer.filterMode = FilterMode.Bilinear;
                                degisti = true;
                            }

                            // Buyuk 4K gorsellerin (kk1-kk5, piratess vb.) 2048'e kirpilmasini engelle
                            if (importer.maxTextureSize < 4096)
                            {
                                importer.maxTextureSize = 4096;
                                degisti = true;
                            }

                            // Sikistirma kalitesini en yuksege cikar (Piksellesme ve artifactleri kaldir)
                            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                            {
                                importer.textureCompression = TextureImporterCompression.Uncompressed;
                                degisti = true;
                            }

                            // 4000x4000 boyutundaki dev görsellerin (kk1-kk5, piratess) ekranda devasa yayılmasını önle, gemi boyutuna (~1.6 birim) getir
                            string dosyaAdi = Path.GetFileNameWithoutExtension(dosya).ToLower();
                            if (dosyaAdi.StartsWith("kk") || dosyaAdi.Contains("pirate"))
                            {
                                if (importer.spritePixelsPerUnit != 2500f)
                                {
                                    importer.spritePixelsPerUnit = 2500f;
                                    degisti = true;
                                }
                            }

                            // Standalone platform ayarlarini da Uncompressed yap
                            TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
                            if (defaultSettings.maxTextureSize < 4096 || defaultSettings.textureCompression != TextureImporterCompression.Uncompressed)
                            {
                                defaultSettings.maxTextureSize = 4096;
                                defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
                                importer.SetPlatformTextureSettings(defaultSettings);
                                degisti = true;
                            }

                            TextureImporterPlatformSettings standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
                            if (standaloneSettings.overridden || standaloneSettings.maxTextureSize < 4096)
                            {
                                standaloneSettings.overridden = true;
                                standaloneSettings.maxTextureSize = 4096;
                                standaloneSettings.textureCompression = TextureImporterCompression.Uncompressed;
                                importer.SetPlatformTextureSettings(standaloneSettings);
                                degisti = true;
                            }

                            if (degisti)
                            {
                                EditorUtility.SetDirty(importer);
                                importer.SaveAndReimport();
                                guncellenenSayisi++;
                            }
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"[GrafikAyarla] Toplam {guncellenenSayisi} gorsel maksimum kalite (4K / Sikistirmasiz / 16x Aniso) formatina guncellendi.");
        }

        // 3. Sahnedeki Ana Kamerayi Kontrol Et
        Camera anaKamera = Camera.main;
        if (anaKamera != null)
        {
            anaKamera.allowMSAA = true;
            anaKamera.allowHDR = true;
            EditorUtility.SetDirty(anaKamera);
        }

        // 4. Prefablari da yeni CanvasScaler ayarlari ile guncelle
        OyuncuPrefabAyarla.Yapilandir();
        NpcPrefabAyarla.Yapilandir();

        AssetDatabase.SaveAssets();
        Debug.Log("[GrafikAyarla] Tum grafik kalitesi iyilestirmeleri basariyla tamamlandi!");
    }
}
