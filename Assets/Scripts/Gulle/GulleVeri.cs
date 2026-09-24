using System;
using System.Collections.Generic;
using UnityEngine;

public enum GulleTuru
{
    Demir = 0,
    Alev = 1,
    Buz = 2,
    KuruKafa = 3,
    Asit = 4
}

[Serializable]
public struct GulleOzellikleri
{
    public GulleTuru tur;
    public string isim;
    public float npcBonusYuzde;
    public float kuleBonusYuzde;
    public float oyuncuBonusYuzde;
    public int piraPuan;
    public bool sifami;
    public bool npcYeVurabilir;
    public bool kuleyeVurabilir;
    public bool ucluVurus;
    public int birimFiyatElmas; // 100 adet için
    public Color gulleRengi;
    public Color yaziRengi;

    public GulleOzellikleri(
        GulleTuru tur,
        string isim,
        float npcBonus,
        float kuleBonus,
        float oyuncuBonus,
        int piraPuan,
        bool sifami,
        bool npcYeVurabilir,
        bool kuleyeVurabilir,
        bool ucluVurus,
        int birimFiyatElmas,
        Color gulleRengi,
        Color yaziRengi)
    {
        this.tur = tur;
        this.isim = isim;
        this.npcBonusYuzde = npcBonus;
        this.kuleBonusYuzde = kuleBonus;
        this.oyuncuBonusYuzde = oyuncuBonus;
        this.piraPuan = piraPuan;
        this.sifami = sifami;
        this.npcYeVurabilir = npcYeVurabilir;
        this.kuleyeVurabilir = kuleyeVurabilir;
        this.ucluVurus = ucluVurus;
        this.birimFiyatElmas = birimFiyatElmas;
        this.gulleRengi = gulleRengi;
        this.yaziRengi = yaziRengi;
    }
}

public static class GulleVeritabani
{
    // GDD'deki 5 Gülle tipinin kuralları ve özellikleri
    public static readonly Dictionary<GulleTuru, GulleOzellikleri> Gulleler = new()
    {
        {
            GulleTuru.Demir,
            new GulleOzellikleri(
                GulleTuru.Demir,
                "Demir Gülle",
                npcBonus: 0f,
                kuleBonus: 0f,
                oyuncuBonus: 0f,
                piraPuan: 0,
                sifami: false,
                npcYeVurabilir: true,
                kuleyeVurabilir: true,
                ucluVurus: false,
                birimFiyatElmas: 0,
                gulleRengi: new Color(0.7f, 0.7f, 0.7f, 1f),
                yaziRengi: new Color(0.9f, 0.2f, 0.2f, 1f) // Kırmızı
            )
        },
        {
            GulleTuru.Alev,
            new GulleOzellikleri(
                GulleTuru.Alev,
                "Alev Gülle",
                npcBonus: 15f,
                kuleBonus: 15f,
                oyuncuBonus: 10f,
                piraPuan: 1,
                sifami: false,
                npcYeVurabilir: true,
                kuleyeVurabilir: true,
                ucluVurus: false,
                birimFiyatElmas: 5,
                gulleRengi: new Color(1f, 0.45f, 0.1f, 1f),
                yaziRengi: new Color(1f, 0.3f, 0.1f, 1f) // Alev Kırmızısı
            )
        },
        {
            GulleTuru.Buz,
            new GulleOzellikleri(
                GulleTuru.Buz,
                "Buz Gülle",
                npcBonus: 0f,
                kuleBonus: 10f, // Kendi filo kulelerine can artışı
                oyuncuBonus: 10f, // Oyunculara can artışı
                piraPuan: 1,
                sifami: true, // Şifa güllesi — hasar vermez, can artırır
                npcYeVurabilir: true, // NPC'lere de şifa atılabilir
                kuleyeVurabilir: true,
                ucluVurus: false,
                birimFiyatElmas: 5,
                gulleRengi: new Color(0.2f, 0.8f, 1f, 1f),
                yaziRengi: new Color(0.18f, 0.85f, 0.25f, 1f) // GDD: Yeşil renk hasar/şifa yazısı
            )
        },
        {
            GulleTuru.KuruKafa,
            new GulleOzellikleri(
                GulleTuru.KuruKafa,
                "Kuru Kafa Gülle",
                npcBonus: 5f,
                kuleBonus: 10f,
                oyuncuBonus: 15f,
                piraPuan: 2,
                sifami: false,
                npcYeVurabilir: true,
                kuleyeVurabilir: true,
                ucluVurus: false,
                birimFiyatElmas: 10,
                gulleRengi: new Color(0.85f, 0.85f, 0.95f, 1f),
                yaziRengi: new Color(0.85f, 0.1f, 0.3f, 1f)
            )
        },
        {
            GulleTuru.Asit,
            new GulleOzellikleri(
                GulleTuru.Asit,
                "Asit Gülle",
                npcBonus: 0f,
                kuleBonus: 0f,
                oyuncuBonus: 15f,
                piraPuan: 3,
                sifami: false,
                npcYeVurabilir: false, // GDD: NPC'de kullanılamaz
                kuleyeVurabilir: false, // GDD: Kulede kullanılamaz
                ucluVurus: true, // GDD: 1. vuruş normal, 1s sonra yarısı, 2s sonra çeyreği
                birimFiyatElmas: 20,
                gulleRengi: new Color(0.2f, 0.95f, 0.3f, 1f),
                yaziRengi: new Color(0.6f, 0.9f, 0.1f, 1f)
            )
        }
    };

    public static GulleOzellikleri Getir(GulleTuru tur)
    {
        if (Gulleler.TryGetValue(tur, out var ozellik))
            return ozellik;
        return Gulleler[GulleTuru.Demir];
    }
}
