using UnityEngine;

public class EyeDetection : MonoBehaviour
{
    [Header("Hedef ve Iþýk")]
    public Transform player;
    public Light eyeLight;

    [Header("Engel Ayarlarý")]
    public LayerMask obstacleMask;

    [Header("UI Ayarlarý")]
    public GameObject failPanel; // Artýk sahne adý deðil, Canvas'taki paneli buraya atayacaðýz

    private bool isCaught = false; // Yakalandýktan sonra kodun sürekli çalýþmasýný engellemek için

    void Update()
    {
        if (isCaught) return; // Eðer çoktan yakalandýysa, tarama yapmayý býrak

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 1. KONTROL: Menzil
        if (distanceToPlayer < eyeLight.range)
        {
            // 2. KONTROL: Açý
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer < eyeLight.spotAngle / 2f)
            {
                // 3. KONTROL: Engel (Raycast)
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    CatchPlayer();
                }
            }
        }
    }

    void CatchPlayer()
    {
        isCaught = true; // Yakalandý durumunu aktifleþtir
        Debug.Log("Sauron seni gördü!");

        // Fail panelini ekranda göster
        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }

        // Farenin imlecini görünür yap ve kilidini aç
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Oyunu (zamaný) durdur.
        Time.timeScale = 0f;
    }
}