using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class OyuncuHareket : NetworkBehaviour
{
    [SerializeField] private float hiz = 5f;
    private Vector3 hedefPozisyon;
    private bool hareket = false;
    private Camera oyuncuKamera;

    private void Start()
    {
        hedefPozisyon = transform.position;

        // Z ekseninde dönmeyi fizik seviyesinde kilitle
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        // Sadece yerel oyuncu kendi gemisini kontrol edebilir
        if (!isLocalPlayer) return;

        // Z rotasyonunu her kare sıfırla (çarpışma vb. durumda dönmeyi engelle)
        if (transform.rotation != Quaternion.identity)
            transform.rotation = Quaternion.identity;

        if (Mouse.current == null) return;

        // Sol tık kontrolü: Hedefe tıklanmadıysa (boş denize tıklandıysa) hareket et
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 fareDunyaPos = FareDunyaPozisyonunuAl();
            Collider2D hit = Physics2D.OverlapPoint(fareDunyaPos);

            bool hedefeTiklandi = false;
            if (hit != null)
            {
                if (hit.GetComponentInParent<NpcHasar>() != null ||
                    hit.GetComponentInParent<OyuncuCanBari>() != null ||
                    hit.CompareTag("NPC") || hit.CompareTag("Player") || hit.CompareTag("Kule"))
                {
                    hedefeTiklandi = true;
                }
            }

            // Eğer hedefe tıklanmadıysa (boş denize tıklandıysa) gemiyi hareket ettir
            if (!hedefeTiklandi)
            {
                hedefPozisyon = new Vector3(fareDunyaPos.x, fareDunyaPos.y, 0f);
                hareket = true;
            }
        }

        // Sağ tık kontrolü: Sağ tık doğrudan hareket komutudur (hedef seçimiyle çakışmaz)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 fareDunyaPos = FareDunyaPozisyonunuAl();
            hedefPozisyon = new Vector3(fareDunyaPos.x, fareDunyaPos.y, 0f);
            hareket = true;
        }

        // Oyuncuyu hedefe doğru hareket ettir
        if (hareket)
        {
            OyuncuyuHareket();
        }
    }

    private Vector2 FareDunyaPozisyonunuAl()
    {
        Camera cam = KameraBul();
        if (cam == null) return transform.position;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = -cam.transform.position.z;
        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    private Camera KameraBul()
    {
        if (oyuncuKamera == null)
        {
            var ok = GetComponent<OyuncuKamera>();
            if (ok != null && ok.HedefKamera != null)
                oyuncuKamera = ok.HedefKamera;
            else
                oyuncuKamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        }
        return oyuncuKamera;
    }

    private void OyuncuyuHareket()
    {
        float mesafe = Vector3.Distance(transform.position, hedefPozisyon);

        if (mesafe > 0.1f)
        {
            Vector3 yon = (hedefPozisyon - transform.position).normalized;
            transform.position += yon * hiz * Time.deltaTime;
        }
        else
        {
            transform.position = hedefPozisyon;
            hareket = false;
        }
    }
}