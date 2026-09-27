using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;

public class HaritaDegistir : NetworkBehaviour
{
    [SerializeField] private float sinirMesafesi = 2f; // Harita sınırına ne kadar yaklaşıldığında geçiş yapılabilir

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            string aktifSahne = SceneManager.GetActiveScene().name;

            // Haritanın sağ sınırına yakın mı?
            if (transform.position.x >= HaritaSiniri.MaxSinir.x - sinirMesafesi)
            {
                if (aktifSahne == "1-1")
                {
                    CmdHaritaDegistir("2-1");
                }
            }
            // Haritanın sol sınırına yakın mı?
            else if (transform.position.x <= HaritaSiniri.MinSinir.x + sinirMesafesi)
            {
                if (aktifSahne == "2-1")
                {
                    CmdHaritaDegistir("1-1");
                }
            }
        }
    }

    [Command]
    private void CmdHaritaDegistir(string yeniHarita)
    {
        NetworkManager.singleton.ServerChangeScene(yeniHarita);
    }
}
