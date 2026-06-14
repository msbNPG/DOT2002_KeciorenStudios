using UnityEngine;

public class KillScript : MonoBehaviour
{
    [Header("Ateþ Etme Ayarlarý")]
    public Camera playerCamera;
    public int damage = 25;
    public float range = 100f;

    [Header("Otomatik Atýþ Ayarý")]
    public float fireRate = 0.1f; // 0.1 saniyede bir ateþ eder (Saniyede 10 mermi)
    private float nextFireTime = 0f;

    void Update()
    {
        // GetMouseButton(0) farenin sol tuþuna BASILI TUTULDUÐU sürece çalýþýr
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate; // Bir sonraki mermi için süreyi kur
            Shoot();
        }
    }

    void Shoot()
    {
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
}