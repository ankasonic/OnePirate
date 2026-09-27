using UnityEngine;
using Mirror;

public class OyuncuSohbet : NetworkBehaviour
{
    // Sunucuya mesaj gönderme komutu
    [Command]
    public void CmdMesajGonder(string mesaj)
    {
        // Mesaj boşsa veya çok uzunsa reddet
        if (string.IsNullOrWhiteSpace(mesaj)) return;
        if (mesaj.Length > 200) mesaj = mesaj.Substring(0, 200);

        string gonderen = "Oyuncu";
        
        // Gönderenin adını al
        var kullaniciAdiBileseni = GetComponent<OyuncuKullaniciAdi>();
        if (kullaniciAdiBileseni != null)
        {
            gonderen = kullaniciAdiBileseni.KullaniciAdi;
        }

        // Tüm istemcilere mesajı dağıt
        RpcMesajAl(gonderen, mesaj);
    }

    // Sunucudan tüm istemcilere mesajı iletme
    [ClientRpc]
    private void RpcMesajAl(string gonderen, string mesaj)
    {
        // UI'a mesajı ilet
        SohbetArayuzu.MesajEkle(gonderen, mesaj);
    }
}
