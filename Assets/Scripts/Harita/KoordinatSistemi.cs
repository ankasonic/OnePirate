using UnityEngine;

/// <summary>
/// Harita sol üst köşesini AA-11 kabul eder.
/// Yukarıdan aşağıya 5 birimde bir AA, AB, AC...
/// Soldan sağa 5 birimde bir 11, 12, 13...
/// Her harita yüklendiğinde HaritaSiniri sınırlarına göre yeniden hesaplanır.
/// </summary>
public static class KoordinatSistemi
{
    public const float HucreBoyutu = 5f;
    public const int BaslangicSutun = 11;

    public static bool DunyaPozisyonundanHesapla(Vector3 dunyaPozisyon, out string harfKodu, out int sutun)
    {
        harfKodu = "--";
        sutun = 0;

        if (!HaritaSiniri.SinirlarHesaplandi)
            return false;

        Vector2 min = HaritaSiniri.MinSinir;
        Vector2 max = HaritaSiniri.MaxSinir;

        // Sol üst = AA-11 → x:min, y:max
        float yerelX = dunyaPozisyon.x - min.x;
        float yerelY = max.y - dunyaPozisyon.y;

        int satirIndex = Mathf.FloorToInt(yerelY / HucreBoyutu);
        int sutunIndex = Mathf.FloorToInt(yerelX / HucreBoyutu);

        // Harita dışına taşmayı sınırla
        int maxSatir = Mathf.Max(0, Mathf.FloorToInt((max.y - min.y) / HucreBoyutu));
        int maxSutun = Mathf.Max(0, Mathf.FloorToInt((max.x - min.x) / HucreBoyutu));
        satirIndex = Mathf.Clamp(satirIndex, 0, maxSatir);
        sutunIndex = Mathf.Clamp(sutunIndex, 0, maxSutun);

        harfKodu = SatirIndexiniHarfeCevir(satirIndex);
        sutun = BaslangicSutun + sutunIndex;
        return true;
    }

    public static string Formatla(Vector3 dunyaPozisyon)
    {
        if (!DunyaPozisyonundanHesapla(dunyaPozisyon, out string harf, out int sutun))
            return "--";

        return $"{harf} {sutun}";
    }

    /// <summary>0 → AA, 1 → AB, ..., 25 → AZ, 26 → BA</summary>
    public static string SatirIndexiniHarfeCevir(int index)
    {
        if (index < 0) index = 0;

        int birinci = index / 26;
        int ikinci = index % 26;

        // ZZ'yi aşarsa üç harfe geç (AAA...)
        if (birinci > 25)
        {
            int ucuncu = birinci / 26;
            birinci %= 26;
            return $"{(char)('A' + ucuncu)}{(char)('A' + birinci)}{(char)('A' + ikinci)}";
        }

        return $"{(char)('A' + birinci)}{(char)('A' + ikinci)}";
    }
}
