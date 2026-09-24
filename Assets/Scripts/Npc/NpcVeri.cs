using System;
using System.Collections.Generic;

public enum NpcTuru
{
    BorisJuckson,
    CaptainCao,
    VictorKane,
    MarcusDrake,
    IvanCross,
    AdrianVolker,
    LeonKruger,
    NotviGrave,
    AntonWolfe,
    DimitriKane,
    OscarStale,
    KizilEjder,
    MaviOfke
}

[Serializable]
public struct NpcOzellikleri
{
    public string isim;
    public int maxHp;
    public int hasar;
    public float hiz;
    public int odulAltin;
    public int odulXP;
    public bool agresif;

    public NpcOzellikleri(string isim, int maxHp, int hasar, float hiz, int odulAltin, int odulXP, bool agresif = false)
    {
        this.isim = isim;
        this.maxHp = maxHp;
        this.hasar = hasar;
        this.hiz = hiz;
        this.odulAltin = odulAltin;
        this.odulXP = odulXP;
        this.agresif = agresif;
    }
}

public static class NpcVeritabani
{
    // GDD'deki birebir NPC listesi
    public static readonly Dictionary<NpcTuru, NpcOzellikleri> Npcler = new()
    {
        { NpcTuru.BorisJuckson,  new NpcOzellikleri("Boris Juckson",  2000,   100, 2f, 50,    100,   false) },
        { NpcTuru.CaptainCao,    new NpcOzellikleri("Captain Cao",    3750,   150, 2f, 75,    125,   false) },
        { NpcTuru.VictorKane,    new NpcOzellikleri("Victor Kane",    7000,   200, 2f, 100,   200,   false) },
        { NpcTuru.MarcusDrake,   new NpcOzellikleri("Marcus Drake",   10000,  250, 2f, 130,   240,   false) },
        { NpcTuru.IvanCross,     new NpcOzellikleri("Ivan Cross",     20000,  300, 2f, 165,   355,   false) },
        { NpcTuru.AdrianVolker,  new NpcOzellikleri("Adrian Volker",  30500,  360, 3f, 205,   370,   true)  },
        { NpcTuru.LeonKruger,    new NpcOzellikleri("Leon Kruger",    65000,  420, 3f, 250,   490,   true)  },
        { NpcTuru.NotviGrave,    new NpcOzellikleri("Notvi Grave",    70000,  500, 3f, 300,   520,   true)  },
        { NpcTuru.AntonWolfe,    new NpcOzellikleri("Anton Wolfe",    70500,  600, 3f, 360,   660,   true)  },
        { NpcTuru.DimitriKane,   new NpcOzellikleri("Dimitri Kane",   85000,  720, 4f, 430,   690,   true)  },
        { NpcTuru.OscarStale,    new NpcOzellikleri("Oscar Stale",    105000, 850, 4f, 600,   700,   true)  },
        { NpcTuru.KizilEjder,    new NpcOzellikleri("Kızıl Ejder",    150000, 1850, 4f, 6000, 7000,  true)  },
        { NpcTuru.MaviOfke,      new NpcOzellikleri("Mavi Öfke",      150000, 1850, 4f, 6000, 7000,  true)  }
    };

    public static NpcOzellikleri Getir(NpcTuru tur)
    {
        if (Npcler.TryGetValue(tur, out var ozellik))
            return ozellik;
        return Npcler[NpcTuru.BorisJuckson];
    }
}
