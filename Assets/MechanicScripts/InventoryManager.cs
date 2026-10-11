using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Envanterdeki esya turlerini tanimlar.
/// </summary>
public enum ItemType
{
    Food,
    Water,
    PistolAmmo,
    RifleAmmo,
    MachinePart,
    Medical // Can yenileme esyalari (Medkit, Bandaj vb.)
}

/// <summary>
/// Oyuncunun eşya envanterini yönetir (yiyecek, içecek, mermi, makine parçası).
/// LPSP silah envanteri ile karıştırılmamalıdır.
/// </summary>
/// <summary>
/// Envanterdeki tek bir esya yiginini temsil eder.
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public ItemType type;
    public string itemName;
    public Sprite icon;
    public int count;
    public int restoreValue;

    public InventoryItem(ItemType type, string itemName, Sprite icon, int count, int restoreValue = 25)
    {
        this.type = type;
        this.itemName = itemName;
        this.icon = icon;
        this.count = count;
        this.restoreValue = restoreValue;
    }
}

public class InventoryManager : MonoBehaviour
{
    // ==================== Singleton ====================
    public static InventoryManager Instance { get; private set; }


    // ==================== Veri Sınıfları ====================

    /// <summary>
    /// Envanterdeki tek bir eşya yığınını temsil eder.
    /// </summary>


    // ==================== Envanter Verileri ====================

    [Header("Yere Atma Sistemi")]
    [Tooltip("Envanterden yere esya atildiginda sahneye cikarilacak 3D objeler (Prefablar)")]
    public List<GameObject> dropPrefabs = new List<GameObject>();

    [Header("Ana Envanter (Yiyecek, Icecek, Mermi)")]
    [Tooltip("Tuketilebilir ve mermi esyalari icin ana izgara. Maksimum 9 slot.")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Asi Parcalari (Kazanma Kosulu)")]
    [Tooltip("Makine parçaları için ayrı liste. Maksimum 5 slot.")]
    public List<InventoryItem> machineParts = new List<InventoryItem>();

    // ==================== Kapasite Sabitleri ====================

    /// <summary>
    /// Sag tiklandiginda esyayi yere atar
    /// </summary>
    public void DropItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        InventoryItem item = items[index];

        // 1. Sahneye prefab firlat. Prefab yoksa esyayi silme, envanterde kalsin.
        if (!SpawnDropPrefab(item)) return;

        // 2. Envanterden 1 adet dusur
        item.count--;
        if (item.count <= 0)
        {
            items.RemoveAt(index);
        }

        onInventoryChanged?.Invoke();
    }

    private bool SpawnDropPrefab(InventoryItem item)
    {
        GameObject prefabToSpawn = null;
        foreach (var prefab in dropPrefabs)
        {
            if (prefab != null)
            {
                InteractableItem ii = prefab.GetComponent<InteractableItem>();
                if (ii != null && ii.itemName == item.itemName)
                {
                    prefabToSpawn = prefab;
                    break;
                }
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Transform playerTr = playerObj.transform;
                // Oyuncunun biraz onunde ve yukarisinda spawn et
                Vector3 spawnPos = playerTr.position + playerTr.forward * 1.5f + Vector3.up * 1.2f;
                GameObject dropped = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                
                Rigidbody rb = dropped.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Ileri dogru hafifce firlat
                    rb.AddForce(playerTr.forward * 4f, ForceMode.Impulse);
                }
                else
                {
                    // Rigidbody yoksa ekle ki yere dussun
                    rb = dropped.AddComponent<Rigidbody>();
                    rb.AddForce(playerTr.forward * 4f, ForceMode.Impulse);
                }
                return true;
            }
        }
        else
        {
            Debug.LogWarning("[InventoryManager] Drop prefab bulunamadi: " + item.itemName + ". Lutfen Inspector'dan 'Drop Prefabs' listesine bu esyanin 3D modelini ekleyin!");
        }
        return false;
    }

    /// <summary>
    /// Ana envanter ızgarasının maksimum slot sayısı.
    /// </summary>
    private const int MAX_ITEMS_CAPACITY = 9;

    /// <summary>
    /// Makine parçaları listesinin maksimum slot sayısı.
    /// </summary>
    private const int MAX_MACHINE_PARTS_CAPACITY = 5;

    // ==================== Olaylar ====================

    /// <summary>
    /// Envanter her değiştiğinde tetiklenir. UI güncelleme için kullanılır.
    /// </summary>
    public Action onInventoryChanged;

    // ==================== Yaşam Döngüsü ====================

    private void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[InventoryManager] Birden fazla ornek tespit edildi. Fazlalik yok ediliyor.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Başlangıç eşyaları: 1 tabanca mermisi ve 1 tüfek mermisi
        AddItem(ItemType.PistolAmmo, "Tabanca Mermisi", null, 1);
        AddItem(ItemType.RifleAmmo, "Tüfek Mermisi", null, 1);
    }

    // ==================== Genel Yöntemler ====================

    /// <summary>
    /// Envantere eşya ekler. Aynı türde eşya varsa sayısını artırır.
    /// Yeni eşya ise listeye ekler. Envanter doluysa false döner.
    /// </summary>
    /// <param name="type">Eklenecek eşya türü.</param>
    /// <param name="name">Eşyanın görünen adı.</param>
    /// <param name="icon">Eşyanın UI ikonu.</param>
    /// <param name="amount">Eklenecek miktar.</param>
    /// <returns>Ekleme başarılıysa true, envanter doluysa false.</returns>
    public bool AddItem(ItemType type, string name, Sprite icon, int amount = 1, int restoreValue = 25)
    {
        // Makine parçaları ayrı listeye yönlendirilir
        if (type == ItemType.MachinePart)
        {
            return AddMachinePart(name, icon, amount);
        }

        // Ana envanterde aynı türde eşya var mı kontrol et
        InventoryItem existing = FindItemByType(items, type);

        if (existing != null)
        {
            // Mevcut eşyanın sayısını artır
            existing.count += amount;
            // Baslangic esyalari ikonsuz eklenir; ilk ikonlu esya geldiginde ikonu devral
            if (existing.icon == null && icon != null) existing.icon = icon;
            Debug.Log($"[InventoryManager] '{existing.itemName}' sayisi artirildi: {existing.count}");
            onInventoryChanged?.Invoke();
            return true;
        }

        // Yeni eşya eklenecek, kapasite kontrolü
        if (items.Count >= MAX_ITEMS_CAPACITY)
        {
            Debug.LogWarning("[InventoryManager] Ana envanter dolu! Esya eklenemedi: " + name);
            return false;
        }

        // Yeni eşya için InventoryItem oluştur
        InventoryItem newItem = new InventoryItem(type, name, icon, amount, restoreValue);
        items.Add(newItem);
        Debug.Log($"[InventoryManager] Yeni esya eklendi: '{name}' x{amount}");
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Envanterden belirtilen türde eşya çıkarır.
    /// Yeterli eşya yoksa false döner.
    /// </summary>
    /// <param name="type">Çıkarılacak eşya türü.</param>
    /// <param name="amount">Çıkarılacak miktar.</param>
    /// <returns>Çıkarma başarılıysa true, yeterli eşya yoksa false.</returns>
    public bool RemoveItem(ItemType type, int amount = 1)
    {
        // Makine parçaları ayrı listeden çıkarılır
        if (type == ItemType.MachinePart)
        {
            return RemoveMachinePart(amount);
        }

        InventoryItem existing = FindItemByType(items, type);

        if (existing == null || existing.count < amount)
        {
            Debug.LogWarning($"[InventoryManager] Yeterli esya yok. Tur: {type}, Istenen: {amount}");
            return false;
        }

        existing.count -= amount;

        // Sayı sıfıra düşerse eşyayı listeden kaldır
        if (existing.count <= 0)
        {
            items.Remove(existing);
            Debug.Log($"[InventoryManager] Esya tamamen tukendi ve listeden kaldirildi: {type}");
        }
        else
        {
            Debug.Log($"[InventoryManager] Esya cikarildi. Tur: {type}, Kalan: {existing.count}");
        }

        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Belirtilen türdeki eşyanın toplam sayısını döndürür.
    /// </summary>
    /// <param name="type">Sorgulanacak eşya türü.</param>
    /// <returns>Eşya sayısı. Eşya yoksa 0.</returns>
    public int GetItemCount(ItemType type)
    {
        // Makine parçaları ayrı listeden sorgulanır
        if (type == ItemType.MachinePart)
        {
            return GetMachinePartCount();
        }

        InventoryItem existing = FindItemByType(items, type);
        return existing != null ? existing.count : 0;
    }

    /// <summary>
    /// Oyuncunun belirtilen türde yeterli eşyası olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="type">Kontrol edilecek eşya türü.</param>
    /// <param name="amount">Gereken minimum miktar.</param>
    /// <returns>Yeterli eşya varsa true.</returns>
    public bool HasItem(ItemType type, int amount = 1)
    {
        return GetItemCount(type) >= amount;
    }

    /// <summary>
    /// Oyuncu bir slota tıkladığında çağrılır.
    /// Yiyecek: 25 açlık geri kazandırır. İçecek: 25 susuzluk geri kazandırır.
    /// Mermi ve makine parçası tıklamayla kullanılamaz.
    /// </summary>
    /// <param name="slotIndex">Tıklanan slotun indeksi.</param>
        public void UseMachinePart(int slotIndex)
    {
        // Makine parcalarina tiklayinca ozel bir eylem yapilmiyorsa bos birakilabilir.
        Debug.Log("Makine parcasi secildi: " + slotIndex);
    }

    public void UseItem(int slotIndex)
    {
        // Geçersiz indeks kontrolü
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            Debug.LogWarning($"[InventoryManager] Geçersiz slot indeksi: {slotIndex}");
            return;
        }

        InventoryItem item = items[slotIndex];

        switch (item.type)
        {
            case ItemType.Food:
                if (PlayerSurvivalStats.Instance != null)
                {
                    PlayerSurvivalStats.Instance.RestoreHunger((float)item.restoreValue);
                    Debug.Log($"[InventoryManager] '{item.itemName}' yendi. Aclik {item.restoreValue} birim geri kazanildi.");
                }
                else
                {
                    Debug.LogError("[InventoryManager] PlayerSurvivalStats bulunamadi!");
                }
                item.count--;
                if (item.count <= 0) items.RemoveAt(slotIndex);
                onInventoryChanged?.Invoke();
                break;

            case ItemType.Water:
                if (PlayerSurvivalStats.Instance != null)
                {
                    PlayerSurvivalStats.Instance.RestoreThirst((float)item.restoreValue);
                    Debug.Log($"[InventoryManager] '{item.itemName}' icildi. Susuzluk {item.restoreValue} birim geri kazanildi.");
                }
                else
                {
                    Debug.LogError("[InventoryManager] PlayerSurvivalStats bulunamadi!");
                }
                item.count--;
                if (item.count <= 0) items.RemoveAt(slotIndex);
                onInventoryChanged?.Invoke();
                break;

            case ItemType.Medical:
                if (PlayerSurvivalStats.Instance != null)
                {
                    PlayerSurvivalStats.Instance.RestoreHealth((float)item.restoreValue);
                    Debug.Log($"[InventoryManager] '{item.itemName}' kullanildi. Saglik {item.restoreValue} birim geri kazanildi.");
                }
                else
                {
                    Debug.LogError("[InventoryManager] PlayerSurvivalStats bulunamadi!");
                }
                item.count--;
                if (item.count <= 0) items.RemoveAt(slotIndex);
                onInventoryChanged?.Invoke();
                break;

            case ItemType.PistolAmmo:
            case ItemType.RifleAmmo:
                // Mermi tıklamayla kullanılamaz. Sadece R tuşuyla yeniden doldurma sistemi üzerinden tüketilir.
                Debug.Log("[InventoryManager] Mermi tiklamayla kullanilamaz. Yeniden doldurmak icin R tusunu kullanin.");
                break;

            case ItemType.MachinePart:
                // Makine parçası tıklamayla kullanılamaz.
                Debug.Log("[InventoryManager] Asi parcasi dogrudan kullanilamaz.");
                break;
        }
    }

    /// <summary>
    /// Yeniden doldurma sistemi için mermi tüketir.
    /// Silah yeniden doldurulurken bu yöntem çağrılır.
    /// </summary>
    /// <param name="ammoType">Tüketilecek mermi türü (PistolAmmo veya RifleAmmo).</param>
    /// <param name="amount">Tüketilecek mermi miktarı.</param>
    /// <returns>Yeterli mermi varsa ve tüketim başarılıysa true.</returns>
    public bool ConsumeAmmo(ItemType ammoType, int amount)
    {
        // Sadece mermi türlerinin tüketilmesine izin ver
        if (ammoType != ItemType.PistolAmmo && ammoType != ItemType.RifleAmmo)
        {
            Debug.LogWarning($"[InventoryManager] ConsumeAmmo sadece mermi turleri icin kullanilabilir. Gecersiz tur: {ammoType}");
            return false;
        }

        return RemoveItem(ammoType, amount);
    }

    // ==================== Makine Parçası Yöntemleri ====================

    /// <summary>
    /// Makine parçasını ayrı listeye ekler.
    /// Ekleme başarılı olursa GameManager üzerinden EsyaToplandi() çağrılır.
    /// </summary>
    /// <param name="name">Parçanın adı.</param>
    /// <param name="icon">Parçanın UI ikonu.</param>
    /// <param name="amount">Eklenecek miktar.</param>
    /// <returns>Ekleme başarılıysa true.</returns>
    private bool AddMachinePart(string name, Sprite icon, int amount)
    {
        // Aynı isimde makine parçası var mı kontrol et
        InventoryItem existing = null;
        for (int i = 0; i < machineParts.Count; i++)
        {
            if (machineParts[i].itemName == name)
            {
                existing = machineParts[i];
                break;
            }
        }

        if (existing != null)
        {
            // Mevcut parçanın sayısını artır
            existing.count += amount;
            Debug.Log($"[InventoryManager] Makine parcasi sayisi artirildi: '{existing.itemName}' x{existing.count}");
        }
        else
        {
            // Kapasite kontrolü
            if (machineParts.Count >= MAX_MACHINE_PARTS_CAPACITY)
            {
                Debug.LogWarning("[InventoryManager] Asi parcalari listesi dolu! Parca eklenemedi: " + name);
                return false;
            }

            // Yeni makine parçası oluştur
            InventoryItem newPart = new InventoryItem(ItemType.MachinePart, name, icon, amount);
            machineParts.Add(newPart);
            Debug.Log($"[InventoryManager] Yeni makine parcasi eklendi: '{name}' x{amount}");
        }

        // Mevcut zanaat sistemi ile uyumluluk için GameManager'a bildir
        if (GameManager.instance != null)
        {
            GameManager.instance.EsyaToplandi();
            Debug.Log("[InventoryManager] GameManager.EsyaToplandi() cagirildi.");
        }
        else
        {
            Debug.LogWarning("[InventoryManager] GameManager bulunamadi! EsyaToplandi() cagirilmadi.");
        }

        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Makine parçası listesinden parça çıkarır (ilk bulunan parçadan).
    /// </summary>
    /// <param name="amount">Çıkarılacak miktar.</param>
    /// <returns>Çıkarma başarılıysa true.</returns>
    private bool RemoveMachinePart(int amount)
    {
        // Toplam makine parçası sayısını hesapla
        int totalParts = GetMachinePartCount();

        if (totalParts < amount)
        {
            Debug.LogWarning($"[InventoryManager] Yeterli makine parcasi yok. Mevcut: {totalParts}, Istenen: {amount}");
            return false;
        }

        int remaining = amount;

        // İlk parçalardan başlayarak çıkar
        for (int i = machineParts.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (machineParts[i].count <= remaining)
            {
                remaining -= machineParts[i].count;
                machineParts.RemoveAt(i);
            }
            else
            {
                machineParts[i].count -= remaining;
                remaining = 0;
            }
        }

        Debug.Log($"[InventoryManager] {amount} makine parcasi cikarildi.");
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Toplam makine parçası sayısını döndürür.
    /// </summary>
    /// <returns>Tüm makine parçalarının toplam sayısı.</returns>
    private int GetMachinePartCount()
    {
        int total = 0;
        for (int i = 0; i < machineParts.Count; i++)
        {
            total += machineParts[i].count;
        }
        return total;
    }

    // ==================== Yardımcı Yöntemler ====================

    /// <summary>
    /// Verilen listede belirtilen türde eşya arar.
    /// </summary>
    /// <param name="list">Aranacak eşya listesi.</param>
    /// <param name="type">Aranan eşya türü.</param>
    /// <returns>Bulunan eşya veya null.</returns>
    private InventoryItem FindItemByType(List<InventoryItem> list, ItemType type)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].type == type)
            {
                return list[i];
            }
        }
        return null;
    }
}
