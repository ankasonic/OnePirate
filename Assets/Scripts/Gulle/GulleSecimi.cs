using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GulleSecimi : NetworkBehaviour
{
    [Header("Aktif Gülle")]
    [SyncVar(hook = nameof(OnGulleDegisti))]
    [SerializeField] private GulleTuru seciliGulle = GulleTuru.Demir;

    [Header("Gülle Stokları (Demir hariç sınırlı)")]
    [SerializeField] private int alevGullesi = 10000;
    [SerializeField] private int buzGullesi = 10000;
    [SerializeField] private int kuruKafaGullesi = 10000;
    [SerializeField] private int asitGullesi = 10000;

    public GulleTuru SeciliGulle => seciliGulle;
    public event Action<GulleTuru> GulleDegistiOlayi;

    private OyuncuSeviye oyuncuSeviye;

    private void Awake()
    {
        oyuncuSeviye = GetComponent<OyuncuSeviye>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Keyboard.current != null)
        {
            // Hem klavye üst sıra rakam tuşları (1-5) hem de Numpad rakam tuşları (1-5) desteklenir
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) GulleSec(GulleTuru.Demir);
            if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) GulleSec(GulleTuru.Alev);
            if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) GulleSec(GulleTuru.Buz);
            if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) GulleSec(GulleTuru.KuruKafa);
            if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) GulleSec(GulleTuru.Asit);
        }
    }

    public void GulleSec(GulleTuru yeniTur)
    {
        // İstemcide anında yerel olarak seç ve olayı tetikle (Gecikmesiz UI ve atış kontrolü)
        seciliGulle = yeniTur;
        GulleDegistiOlayi?.Invoke(seciliGulle);
        Debug.Log($"[GulleSecimi] Gülle Seçildi: {seciliGulle} | Kalan Stok: {(seciliGulle == GulleTuru.Demir ? "Sınırsız" : GulleMiktari(seciliGulle).ToString())}");

        if (isClient)
        {
            CmdGulleSec(yeniTur);
        }
    }

    [Command]
    private void CmdGulleSec(GulleTuru yeniTur)
    {
        seciliGulle = yeniTur;
    }

    private void OnGulleDegisti(GulleTuru eskiTur, GulleTuru yeniTur)
    {
        seciliGulle = yeniTur;
        GulleDegistiOlayi?.Invoke(yeniTur);
    }

    // GDD: "Gemi her bir atışta 100 gülle harcar fakat bu seviyeye göre değişir her seviye artışında +20 gülle ekstra harcar"
    public int GerekliGulleSayisi()
    {
        int seviye = oyuncuSeviye != null ? oyuncuSeviye.Seviye : 1;
        return 100 + ((seviye - 1) * 20);
    }

    // GDD: Atış yapıldığında stoktan düşme ve gerekirse demir gülleye otomatik geçiş
    [Server]
    public float GulleHarcaVeHasarKatsayisiHesapla()
    {
        if (seciliGulle == GulleTuru.Demir)
        {
            return 1f; // Demir gülle sınırsız
        }

        int gerekli = GerekliGulleSayisi();
        int mevcut = GulleMiktari(seciliGulle);

        if (mevcut >= gerekli)
        {
            GulleEksilt(seciliGulle, gerekli);
            return 1f;
        }
        else if (mevcut > 0)
        {
            // GDD: "elindeki gülle sayısına denk gelen hasar kadar atış yapar ve demir gülleye otomatik geçiş yapılır."
            float oran = (float)mevcut / gerekli;
            GulleEksilt(seciliGulle, mevcut);
            seciliGulle = GulleTuru.Demir; // Demir gülleye otomatik geç
            return oran;
        }
        else
        {
            // Stok kalmadıysa direkt demir gülleye geç
            seciliGulle = GulleTuru.Demir;
            return 1f;
        }
    }

    public int GulleMiktari(GulleTuru tur) => tur switch
    {
        GulleTuru.Demir => int.MaxValue,
        GulleTuru.Alev => alevGullesi,
        GulleTuru.Buz => buzGullesi,
        GulleTuru.KuruKafa => kuruKafaGullesi,
        GulleTuru.Asit => asitGullesi,
        _ => 0
    };

    public void GulleEkle(GulleTuru tur, int miktar)
    {
        switch (tur)
        {
            case GulleTuru.Alev: alevGullesi += miktar; break;
            case GulleTuru.Buz: buzGullesi += miktar; break;
            case GulleTuru.KuruKafa: kuruKafaGullesi += miktar; break;
            case GulleTuru.Asit: asitGullesi += miktar; break;
        }
    }

    private void GulleEksilt(GulleTuru tur, int miktar)
    {
        switch (tur)
        {
            case GulleTuru.Alev: alevGullesi = Mathf.Max(0, alevGullesi - miktar); break;
            case GulleTuru.Buz: buzGullesi = Mathf.Max(0, buzGullesi - miktar); break;
            case GulleTuru.KuruKafa: kuruKafaGullesi = Mathf.Max(0, kuruKafaGullesi - miktar); break;
            case GulleTuru.Asit: asitGullesi = Mathf.Max(0, asitGullesi - miktar); break;
        }
    }

    // GDD Sayı Formatı: 15204046 -> 15m, 214654 -> 214k
    public static string SayiFormatla(int sayi)
    {
        if (sayi >= 1_000_000)
            return $"{sayi / 1_000_000}m";
        if (sayi >= 1_000)
            return $"{sayi / 1_000}k";
        return sayi.ToString();
    }
}
