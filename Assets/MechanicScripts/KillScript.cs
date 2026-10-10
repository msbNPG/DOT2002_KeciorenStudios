using UnityEngine;

public class KillScript : MonoBehaviour
{
    [Header("Ates Etme Ayarlari")]
    public Camera playerCamera;
    public int damage = 25;
    public float range = 100f;

    [Header("Otomatik Atis Ayari")]
    public float fireRate = 0.1f; // 0.1 saniyede bir ates eder (Saniyede 10 mermi)
    private float nextFireTime = 0f;

    void Awake()
    {
        FindCameraIfNull();
    }

    void Start()
    {
        FindCameraIfNull();
    }

    void Update()
    {
        // GetMouseButton(0) farenin sol tusuna BASILI TUTULDUGU surece calisir
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate; // Bir sonraki mermi icin sureyi kur
            Shoot();
        }
    }

    void Shoot()
    {
        FindCameraIfNull();

        if (playerCamera == null)
        {
            Debug.LogWarning("[KillScript] playerCamera bulunamadi! Atis yapilamiyor.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            ZombieAI target = hit.transform.GetComponent<ZombieAI>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }

    private void FindCameraIfNull()
    {
        if (playerCamera != null) return;

        // 1. Ana kamerayi dene
        playerCamera = Camera.main;

        // 2. Kendi uzerinde veya alt objelerinde ara
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        // 3. Ust objelerde ara (ornek: eger silah objesindeyse)
        if (playerCamera == null)
        {
            playerCamera = GetComponentInParent<Camera>();
        }

        // 4. Sahnedeki ilk gecerli kamerayi bul
        if (playerCamera == null)
        {
            playerCamera = Object.FindFirstObjectByType<Camera>();
        }
    }
}