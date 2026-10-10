using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Belirlenen alan icinde rastgele konumlara nesne dagitan sistem.
/// Zombiler NavMesh uzerinde, esyalar serbest alanda spawn olur.
/// Inspector'dan prefab listelerini ve adetlerini ayarlayin.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Alani Ayarlari")]
    [Tooltip("Spawn alaninin X eksenindeki genisligi")]
    public float areaWidth = 50f;
    [Tooltip("Spawn alaninin Z eksenindeki derinligi")]
    public float areaDepth = 50f;
    [Tooltip("Nesnelerin yerlestirilecegi maksimum yukseklik (Raycast icin)")]
    public float maxHeight = 50f;
    [Tooltip("Oyuncuya bu mesafeden yakin spawn olmasin")]
    public float playerSafeRadius = 8f;

    [Header("Zombi Ayarlari")]
    [Tooltip("Sahneye cikarilacak zombi prefablari (rastgele secilir)")]
    public List<GameObject> zombiePrefabs = new List<GameObject>();
    [Tooltip("Toplam kac zombi spawn olacak")]
    public int zombieCount = 10;
    [Tooltip("NavMesh uzerinde arama yaricapi")]
    public float navMeshSampleRadius = 10f;

    [Header("Yiyecek Ayarlari")]
    [Tooltip("Yiyecek prefablari (InteractableItem icermeli)")]
    public List<GameObject> foodPrefabs = new List<GameObject>();
    [Tooltip("Toplam kac yiyecek spawn olacak")]
    public int foodCount = 5;

    [Header("Su / Icecek Ayarlari")]
    [Tooltip("Icecek prefablari (InteractableItem icermeli)")]
    public List<GameObject> waterPrefabs = new List<GameObject>();
    [Tooltip("Toplam kac icecek spawn olacak")]
    public int waterCount = 5;

    [Header("Mermi Ayarlari")]
    [Tooltip("Mermi prefablari (InteractableItem icermeli)")]
    public List<GameObject> ammoPrefabs = new List<GameObject>();
    [Tooltip("Toplam kac mermi kutusu spawn olacak")]
    public int ammoCount = 8;

    [Header("Saglik Esyasi Ayarlari")]
    [Tooltip("Medikal prefablari (InteractableItem icermeli)")]
    public List<GameObject> medicalPrefabs = new List<GameObject>();
    [Tooltip("Toplam kac saglik esyasi spawn olacak")]
    public int medicalCount = 3;

    [Header("Asi Parcasi Ayarlari")]
    [Tooltip("Asi parcasi prefablari (InteractableItem, ItemType=MachinePart)")]
    public List<GameObject> machinePartPrefabs = new List<GameObject>();
    [Tooltip("Toplam kac asi parcasi spawn olacak (genellikle 5)")]
    public int machinePartCount = 5;

    [Header("Gelismis Ayarlar")]
    [Tooltip("Nesneler arasi minimum mesafe")]
    public float minDistanceBetweenObjects = 3f;
    [Tooltip("Spawn islemi oyun basladiginda otomatik calissin mi?")]
    public bool spawnOnStart = true;
    [Tooltip("Spawn edilen nesneler bu objenin altina gruplansin (temiz hierarchy)")]
    public bool groupUnderThis = true;

    // Spawn edilen tum pozisyonlari takip et (ust uste gelmesin)
    private List<Vector3> usedPositions = new List<Vector3>();
    private Transform playerTransform;

    void Start()
    {
        // Oyuncu referansini bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (spawnOnStart)
        {
            SpawnAll();
        }
    }

    /// <summary>
    /// Tum kategorilerdeki nesneleri spawn eder.
    /// </summary>
    [ContextMenu("Hepsini Spawn Et")]
    public void SpawnAll()
    {
        usedPositions.Clear();

        // Zombileri NavMesh uzerine spawn et
        SpawnCategory(zombiePrefabs, zombieCount, true, "Zombiler");

        // Esyalari serbest alana spawn et
        SpawnCategory(foodPrefabs, foodCount, false, "Yiyecekler");
        SpawnCategory(waterPrefabs, waterCount, false, "Icecekler");
        SpawnCategory(ammoPrefabs, ammoCount, false, "Mermiler");
        SpawnCategory(medicalPrefabs, medicalCount, false, "Saglik");
        SpawnCategory(machinePartPrefabs, machinePartCount, false, "AsiParcalari");

        Debug.Log("[RandomSpawner] Tum nesneler basariyla spawn edildi!");
    }

    /// <summary>
    /// Belirli bir kategoriyi spawn eder.
    /// </summary>
    private void SpawnCategory(List<GameObject> prefabs, int count, bool useNavMesh, string groupName)
    {
        if (prefabs == null || prefabs.Count == 0 || count <= 0) return;

        // Gruplama icin parent olustur
        Transform parent = null;
        if (groupUnderThis)
        {
            GameObject groupObj = new GameObject("Spawned_" + groupName);
            groupObj.transform.SetParent(this.transform);
            groupObj.transform.localPosition = Vector3.zero;
            parent = groupObj.transform;
        }

        int spawned = 0;
        int maxAttempts = count * 20; // Sonsuz donguye girmemesi icin
        int attempts = 0;

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 spawnPos;

            if (useNavMesh)
            {
                // NavMesh uzerinde rastgele pozisyon bul
                if (!TryGetNavMeshPosition(out spawnPos))
                {
                    continue;
                }
            }
            else
            {
                // Serbest alanda rastgele pozisyon bul (yere oturt)
                if (!TryGetGroundPosition(out spawnPos))
                {
                    continue;
                }
            }

            // Oyuncuya cok yakin mi?
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(spawnPos, playerTransform.position);
                if (distToPlayer < playerSafeRadius)
                {
                    continue;
                }
            }

            // Diger nesnelere cok yakin mi?
            if (IsTooCloseToExisting(spawnPos))
            {
                continue;
            }

            // Prefab sec (listeden rastgele)
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

            // Spawn et
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            usedPositions.Add(spawnPos);
            spawned++;
        }

        if (spawned < count)
        {
            Debug.LogWarning($"[RandomSpawner] '{groupName}' icin {count} hedeflendi ama sadece {spawned} spawn edilebildi. Alani genisletin veya minDistanceBetweenObjects degerini azaltin.");
        }
        else
        {
            Debug.Log($"[RandomSpawner] {spawned}x {groupName} basariyla spawn edildi.");
        }
    }

    /// <summary>
    /// NavMesh uzerinde gecerli bir pozisyon bulmaya calisir.
    /// </summary>
    private bool TryGetNavMeshPosition(out Vector3 result)
    {
        result = Vector3.zero;

        // Rastgele bir nokta sec
        Vector3 center = transform.position;
        float randomX = Random.Range(-areaWidth / 2f, areaWidth / 2f);
        float randomZ = Random.Range(-areaDepth / 2f, areaDepth / 2f);
        Vector3 randomPoint = center + new Vector3(randomX, maxHeight, randomZ);

        // Bu noktanin en yakin NavMesh pozisyonunu bul
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomPoint, out navHit, navMeshSampleRadius + maxHeight, NavMesh.AllAreas))
        {
            result = navHit.position;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Yere oturtulmus bir pozisyon bulmaya calisir (Raycast ile).
    /// </summary>
    private bool TryGetGroundPosition(out Vector3 result)
    {
        result = Vector3.zero;

        Vector3 center = transform.position;
        float randomX = Random.Range(-areaWidth / 2f, areaWidth / 2f);
        float randomZ = Random.Range(-areaDepth / 2f, areaDepth / 2f);
        Vector3 rayOrigin = center + new Vector3(randomX, maxHeight, randomZ);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, maxHeight * 2f))
        {
            result = hit.point + Vector3.up * 0.1f; // Yerin biraz ustune koy
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pozisyonun daha once spawn edilen nesnelere cok yakin olup olmadigini kontrol eder.
    /// </summary>
    private bool IsTooCloseToExisting(Vector3 pos)
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if (Vector3.Distance(pos, usedPositions[i]) < minDistanceBetweenObjects)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Spawn alanini Scene gorunumunde yesil kutu olarak cizer.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Vector3 center = transform.position + Vector3.up * (maxHeight / 2f);
        Vector3 size = new Vector3(areaWidth, maxHeight, areaDepth);
        Gizmos.DrawCube(center, size);

        // Disaridan da cizgi olarak goster
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireCube(center, size);

        // Oyuncu guvenli alanini kirmizi daire olarak goster
        if (playerSafeRadius > 0)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, playerSafeRadius);
        }
    }
}
