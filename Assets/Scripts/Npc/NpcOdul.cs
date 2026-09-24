using UnityEngine;
using Mirror;

public class NpcOdul : NetworkBehaviour
{
    [Header("Ödül Değerleri")]
    [SerializeField] private int odulAltin = 50;
    [SerializeField] private int odulXP = 100;

    public int OdulAltin => odulAltin;
    public int OdulXP => odulXP;

    public void Ayarla(int altin, int xp)
    {
        odulAltin = altin;
        odulXP = xp;
    }

    [Server]
    public void OdulDagit(NetworkIdentity kazananOyuncu, string npcAdi)
    {
        if (kazananOyuncu == null) return;

        Debug.Log($"[NpcOdul] {npcAdi} batırıldı! Kazanan: {kazananOyuncu.name}. Ödül: {odulAltin} Altın, {odulXP} XP.");

        // İleride oyuncu ekonomi / envanter / seyir defteri ve bildirim sistemi bağlandığında:
        // Bildirim: $"{npcAdi} yok ettin ve ödül olarak {odulAltin} Altın, {odulXP} XP kazandın."
        TargetOdulBildir(kazananOyuncu.connectionToClient, npcAdi, odulAltin, odulXP);
    }

    [TargetRpc]
    private void TargetOdulBildir(NetworkConnection target, string npcAdi, int altin, int xp)
    {
        Debug.Log($"[Tebrikler!] {npcAdi} yok ettin ve ödül olarak {altin} Altın, {xp} XP kazandın.");
    }
}
