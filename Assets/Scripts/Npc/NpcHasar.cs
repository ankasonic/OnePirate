using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NpcHasar : NetworkBehaviour
{
    [Header("NPC Türü")]
    [SerializeField] private NpcTuru npcTuru = NpcTuru.BorisJuckson;

    [Header("UI Bileşenleri")]
    [SerializeField] private Slider canSlider;
    [SerializeField] private Image canDolgusu;
    [SerializeField] private RectTransform canDolgusuRect;
    [SerializeField] private TMP_Text canYazisi;
    [SerializeField] private TMP_Text npcAdiYazisi;

    [Header("Görsel Efektler (Opsiyonel)")]
    [SerializeField] private GameObject yanmaEfekti;
    [SerializeField] private GameObject patlamaEfekti;

    [Header("Can Bilgileri")]
    [SyncVar(hook = nameof(OnGuncelCanDegisti))]
    [SerializeField] private int guncelCan = 2000;

    [SyncVar(hook = nameof(OnMaksCanDegisti))]
    [SerializeField] private int maksCan = 2000;

    [SyncVar(hook = nameof(OnNpcAdiDegisti))]
    [SerializeField] private string npcAdi = "Boris Juckson";

    public int GuncelCan => guncelCan;
    public int MaksCan => maksCan;
    public string NpcAdi => npcAdi;
    public NpcTuru NpcTuru => npcTuru;

    // GDD: "Ödül Sahibi: En çok hasar veren oyuncu. Hasar Sayacı Sıfırlama: Can tam dolunca sıfırlanır."
    private readonly Dictionary<NetworkIdentity, int> oyuncuHasarlari = new();
    private float sonHasarZamani;
    private const float OTO_TAMIR_SURESI = 300f; // 5 dakika

    private NpcOdul npcOdul;
    private NpcHareket npcHareket;

    private void Awake()
    {
        npcOdul = GetComponent<NpcOdul>();
        npcHareket = GetComponent<NpcHareket>();
        BilesenleriOtomatikBul();
    }

    private void Start()
    {
        if (isServer)
        {
            NpcAyarla(npcTuru);
        }
        ArayuzuGuncelle();
    }

    private void Reset()
    {
        BilesenleriOtomatikBul();
    }

    private void Update()
    {
        if (!isServer) return;

        // GDD: "Can Dolumu: 5 dakika içerisinde hasar almazsa Max HP'ye otomatik ulaşır."
        if (guncelCan > 0 && guncelCan < maksCan && (Time.time - sonHasarZamani >= OTO_TAMIR_SURESI))
        {
            guncelCan = maksCan;
            oyuncuHasarlari.Clear(); // Can tam dolunca sayaç sıfırlanır
        }
    }

    public void BilesenleriOtomatikBul()
    {
        if (canSlider == null)
            canSlider = GetComponentInChildren<Slider>(true);

        // Dolgu rect ve image artık slider.fillRect'ten değil, hiyerarşiden bulunur
        if (canDolgusuRect == null)
        {
            var tumRectler = GetComponentsInChildren<RectTransform>(true);
            foreach (var r in tumRectler)
            {
                if (r.name == "Dolgu" && r.GetComponent<Image>() != null)
                {
                    canDolgusuRect = r;
                    canDolgusu = r.GetComponent<Image>();
                    break;
                }
            }
        }
        if (canDolgusu == null && canDolgusuRect != null)
            canDolgusu = canDolgusuRect.GetComponent<Image>();

        if (canYazisi == null && canSlider != null)
            canYazisi = canSlider.GetComponentInChildren<TMP_Text>(true);

        if (npcAdiYazisi == null)
        {
            var tmps = GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in tmps)
            {
                if (t != canYazisi)
                {
                    npcAdiYazisi = t;
                    break;
                }
            }
        }
    }

    [Server]
    public void NpcAyarla(NpcTuru tur)
    {
        npcTuru = tur;
        var ozellik = NpcVeritabani.Getir(tur);
        npcAdi = ozellik.isim;
        maksCan = ozellik.maxHp;
        guncelCan = ozellik.maxHp;
        sonHasarZamani = Time.time;
        oyuncuHasarlari.Clear();

        if (npcHareket != null)
        {
            npcHareket.Ayarla(ozellik.hiz, ozellik.hasar, ozellik.agresif);
        }

        if (npcOdul != null)
        {
            npcOdul.Ayarla(ozellik.odulAltin, ozellik.odulXP);
        }
    }

    private void OnGuncelCanDegisti(int eskiCan, int yeniCan)
    {
        ArayuzuGuncelle();
        EfektleriGuncelle();
    }

    private void OnMaksCanDegisti(int eskiMaks, int yeniMaks)
    {
        ArayuzuGuncelle();
    }

    private void OnNpcAdiDegisti(string eskiAd, string yeniAd)
    {
        if (npcAdiYazisi != null)
            npcAdiYazisi.text = yeniAd;
    }

    public void ArayuzuGuncelle()
    {
        float oran = maksCan > 0 ? Mathf.Clamp01((float)guncelCan / maksCan) : 0f;

        // GDD: Can barı her iki taraftan merkeze doğru eşit küçülür
        // Slider kullanılmıyor — RectTransform anchor'ları ile doğrudan yönetilir
        if (canDolgusuRect != null)
        {
            canDolgusuRect.anchorMin = new Vector2(0.5f - (oran / 2f), 0f);
            canDolgusuRect.anchorMax = new Vector2(0.5f + (oran / 2f), 1f);
            canDolgusuRect.offsetMin = Vector2.zero;
            canDolgusuRect.offsetMax = Vector2.zero;
        }

        // GDD: %100 Yeşil -> %50 Sarı -> %0 Kırmızı Color.Lerp akıcı geçiş
        if (canDolgusu != null)
        {
            Color barRengi;
            if (oran >= 0.5f)
            {
                float t = (oran - 0.5f) / 0.5f;
                barRengi = Color.Lerp(Color.yellow, new Color(0.18f, 0.85f, 0.25f, 1f), t);
            }
            else
            {
                float t = oran / 0.5f;
                barRengi = Color.Lerp(new Color(0.9f, 0.15f, 0.15f, 1f), Color.yellow, t);
            }
            canDolgusu.color = barRengi;
        }

        // Can değeri yazısı gizlenir
        if (canYazisi != null)
        {
            canYazisi.gameObject.SetActive(false);
        }

        if (npcAdiYazisi != null)
        {
            npcAdiYazisi.text = npcAdi;
        }
    }

    private void EfektleriGuncelle()
    {
        float oran = maksCan > 0 ? (float)guncelCan / maksCan : 0f;

        // GDD: %50'nin altına düşünce yanma animasyonu başlar, can 0 olunca durur
        if (yanmaEfekti != null)
        {
            yanmaEfekti.SetActive(oran > 0f && oran <= 0.5f);
        }
    }

    /// <summary>
    /// NPC'nin canını artırır (Buz güllesi şifası için).
    /// Sunucuda çalışır; SyncVar hook aracılığıyla tüm istemcilerde UI güncellenir.
    /// </summary>
    [Server]
    public void CanYenile(int miktar)
    {
        if (miktar <= 0 || guncelCan <= 0) return;
        guncelCan = Mathf.Clamp(guncelCan + miktar, 0, maksCan);
        sonHasarZamani = Time.time; // Oto-tamir zamanlayıcısını sıfırla
    }

    [Server]
    public void HasarAl(int hasarMiktari, NetworkIdentity saldiranOyuncu)
    {
        if (hasarMiktari <= 0 || guncelCan <= 0) return;

        sonHasarZamani = Time.time;
        guncelCan = Mathf.Clamp(guncelCan - hasarMiktari, 0, maksCan);

        // Hasar sayacını kaydet
        if (saldiranOyuncu != null)
        {
            if (!oyuncuHasarlari.ContainsKey(saldiranOyuncu))
                oyuncuHasarlari[saldiranOyuncu] = 0;
            oyuncuHasarlari[saldiranOyuncu] += hasarMiktari;

            // Agresif olmayan NPC saldırıya uğrayınca karşılık verir
            if (npcHareket != null)
            {
                npcHareket.SaldiranaKarsilikVer(saldiranOyuncu);
            }
        }

        // GDD: Can 0 olduğunda ödül verilir ve batar
        if (guncelCan <= 0)
        {
            BatmaIslemi();
        }
    }

    [Server]
    private void BatmaIslemi()
    {
        // En çok hasar veren oyuncuyu belirle
        NetworkIdentity enCokHasarVeren = null;
        int enYuksekHasar = -1;

        foreach (var pair in oyuncuHasarlari)
        {
            if (pair.Key != null && pair.Value > enYuksekHasar)
            {
                enYuksekHasar = pair.Value;
                enCokHasarVeren = pair.Key;
            }
        }

        // Ödülü dağıt
        if (npcOdul != null)
        {
            npcOdul.OdulDagit(enCokHasarVeren, npcAdi);
        }

        RpcOlumEfekti();

        // Spawner varsa yeniden doğumu bildir
        NpcSpawner spawner = FindFirstObjectByType<NpcSpawner>();
        if (spawner != null)
        {
            spawner.NpcOldu(this);
        }
        else
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void RpcOlumEfekti()
    {
        if (yanmaEfekti != null) yanmaEfekti.SetActive(false);
        if (patlamaEfekti != null) patlamaEfekti.SetActive(true);
    }
}
