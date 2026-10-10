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
/// Oyuncunun eÃƒâ€¦Ã…Â¸ya envanterini yÃƒÆ’Ã‚Â¶netir (yiyecek, iÃƒÆ’Ã‚Â§ecek, mermi, makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â±).
/// LPSP silah envanteri ile karÃƒâ€Ã‚Â±Ãƒâ€¦Ã…Â¸tÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â±lmamalÃƒâ€Ã‚Â±dÃƒâ€Ã‚Â±r.
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


    // ==================== Veri SÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â±flarÃƒâ€Ã‚Â± ====================

    /// <summary>
    /// Envanterdeki tek bir eÃƒâ€¦Ã…Â¸ya yÃƒâ€Ã‚Â±Ãƒâ€Ã…Â¸Ãƒâ€Ã‚Â±nÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± temsil eder.
    /// </summary>


    // ==================== Envanter Verileri ====================

    [Header("Yere Atma Sistemi")]
    [Tooltip("Envanterden yere esya atildiginda sahneye cikarilacak 3D objeler (Prefablar)")]
    public List<GameObject> dropPrefabs = new List<GameObject>();

    [Header("Ana Envanter (Yiyecek, Icecek, Mermi)")]
    [Tooltip("Tuketilebilir ve mermi esyalari icin ana izgara. Maksimum 9 slot.")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Asi Parcalari (Kazanma Kosulu)")]
    [Tooltip("Makine parÃƒÆ’Ã‚Â§alarÃƒâ€Ã‚Â± iÃƒÆ’Ã‚Â§in ayrÃƒâ€Ã‚Â± liste. Maksimum 5 slot.")]
    public List<InventoryItem> machineParts = new List<InventoryItem>();

    // ==================== Kapasite Sabitleri ====================

    /// <summary>
    /// Sag tiklandiginda esyayi yere atar
    /// </summary>
    public void DropItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        InventoryItem item = items[index];

        // 1. Sahneye prefab firlat
        SpawnDropPrefab(item);

        // 2. Envanterden 1 adet dusur
        item.count--;
        if (item.count <= 0)
        {
            items.RemoveAt(index);
        }

        onInventoryChanged?.Invoke();
    }

    private void SpawnDropPrefab(InventoryItem item)
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
            }
        }
        else
        {
            Debug.LogWarning("[InventoryManager] Drop prefab bulunamadi: " + item.itemName + ". Lutfen Inspector'dan 'Drop Prefabs' listesine bu esyanin 3D modelini ekleyin!");
        }
    }

    /// <summary>
    /// Ana envanter Ãƒâ€Ã‚Â±zgarasÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â±n maksimum slot sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±.
    /// </summary>
    private const int MAX_ITEMS_CAPACITY = 9;

    /// <summary>
    /// Makine parÃƒÆ’Ã‚Â§alarÃƒâ€Ã‚Â± listesinin maksimum slot sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±.
    /// </summary>
    private const int MAX_MACHINE_PARTS_CAPACITY = 5;

    // ==================== Olaylar ====================

    /// <summary>
    /// Envanter her deÃƒâ€Ã…Â¸iÃƒâ€¦Ã…Â¸tiÃƒâ€Ã…Â¸inde tetiklenir. UI gÃƒÆ’Ã‚Â¼ncelleme iÃƒÆ’Ã‚Â§in kullanÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±r.
    /// </summary>
    public Action onInventoryChanged;

    // ==================== YaÃƒâ€¦Ã…Â¸am DÃƒÆ’Ã‚Â¶ngÃƒÆ’Ã‚Â¼sÃƒÆ’Ã‚Â¼ ====================

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
        // BaÃƒâ€¦Ã…Â¸langÃƒâ€Ã‚Â±ÃƒÆ’Ã‚Â§ eÃƒâ€¦Ã…Â¸yalarÃƒâ€Ã‚Â±: 1 tabanca mermisi ve 1 tÃƒÆ’Ã‚Â¼fek mermisi
        AddItem(ItemType.PistolAmmo, "Tabanca Mermisi", null, 1);
        AddItem(ItemType.RifleAmmo, "TÃƒÆ’Ã‚Â¼fek Mermisi", null, 1);
    }

    // ==================== Genel YÃƒÆ’Ã‚Â¶ntemler ====================

    /// <summary>
    /// Envantere eÃƒâ€¦Ã…Â¸ya ekler. AynÃƒâ€Ã‚Â± tÃƒÆ’Ã‚Â¼rde eÃƒâ€¦Ã…Â¸ya varsa sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± artÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â±r.
    /// Yeni eÃƒâ€¦Ã…Â¸ya ise listeye ekler. Envanter doluysa false dÃƒÆ’Ã‚Â¶ner.
    /// </summary>
    /// <param name="type">Eklenecek eÃƒâ€¦Ã…Â¸ya tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼.</param>
    /// <param name="name">EÃƒâ€¦Ã…Â¸yanÃƒâ€Ã‚Â±n gÃƒÆ’Ã‚Â¶rÃƒÆ’Ã‚Â¼nen adÃƒâ€Ã‚Â±.</param>
    /// <param name="icon">EÃƒâ€¦Ã…Â¸yanÃƒâ€Ã‚Â±n UI ikonu.</param>
    /// <param name="amount">Eklenecek miktar.</param>
    /// <returns>Ekleme baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±ysa true, envanter doluysa false.</returns>
    public bool AddItem(ItemType type, string name, Sprite icon, int amount = 1, int restoreValue = 25)
    {
        // Makine parÃƒÂ§alarÃ„Â± ayrÃ„Â± listeye yÃƒÂ¶nlendirilir
        if (type == ItemType.MachinePart)
        {
            return AddMachinePart(name, icon, amount);
        }

        // Ana envanterde aynÃƒâ€Ã‚Â± tÃƒÆ’Ã‚Â¼rde eÃƒâ€¦Ã…Â¸ya var mÃƒâ€Ã‚Â± kontrol et
        InventoryItem existing = FindItemByType(items, type);

        if (existing != null)
        {
            // Mevcut eÃƒâ€¦Ã…Â¸yanÃƒâ€Ã‚Â±n sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± artÃƒâ€Ã‚Â±r
            existing.count += amount;
            Debug.Log($"[InventoryManager] '{existing.itemName}' sayisi artirildi: {existing.count}");
            onInventoryChanged?.Invoke();
            return true;
        }

        // Yeni eÃƒâ€¦Ã…Â¸ya eklenecek, kapasite kontrolÃƒÆ’Ã‚Â¼
        if (items.Count >= MAX_ITEMS_CAPACITY)
        {
            Debug.LogWarning("[InventoryManager] Ana envanter dolu! Esya eklenemedi: " + name);
            return false;
        }

        // Yeni eÃƒâ€¦Ã…Â¸ya iÃƒÆ’Ã‚Â§in InventoryItem oluÃƒâ€¦Ã…Â¸tur
        InventoryItem newItem = new InventoryItem(type, name, icon, amount, restoreValue);
        items.Add(newItem);
        Debug.Log($"[InventoryManager] Yeni esya eklendi: '{name}' x{amount}");
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Envanterden belirtilen tÃƒÆ’Ã‚Â¼rde eÃƒâ€¦Ã…Â¸ya ÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±r.
    /// Yeterli eÃƒâ€¦Ã…Â¸ya yoksa false dÃƒÆ’Ã‚Â¶ner.
    /// </summary>
    /// <param name="type">ÃƒÆ’Ã¢â‚¬Â¡Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±lacak eÃƒâ€¦Ã…Â¸ya tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼.</param>
    /// <param name="amount">ÃƒÆ’Ã¢â‚¬Â¡Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±lacak miktar.</param>
    /// <returns>ÃƒÆ’Ã¢â‚¬Â¡Ãƒâ€Ã‚Â±karma baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±ysa true, yeterli eÃƒâ€¦Ã…Â¸ya yoksa false.</returns>
    public bool RemoveItem(ItemType type, int amount = 1)
    {
        // Makine parÃƒÆ’Ã‚Â§alarÃƒâ€Ã‚Â± ayrÃƒâ€Ã‚Â± listeden ÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±r
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

        // SayÃƒâ€Ã‚Â± sÃƒâ€Ã‚Â±fÃƒâ€Ã‚Â±ra dÃƒÆ’Ã‚Â¼Ãƒâ€¦Ã…Â¸erse eÃƒâ€¦Ã…Â¸yayÃƒâ€Ã‚Â± listeden kaldÃƒâ€Ã‚Â±r
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
    /// Belirtilen tÃƒÆ’Ã‚Â¼rdeki eÃƒâ€¦Ã…Â¸yanÃƒâ€Ã‚Â±n toplam sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± dÃƒÆ’Ã‚Â¶ndÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼r.
    /// </summary>
    /// <param name="type">Sorgulanacak eÃƒâ€¦Ã…Â¸ya tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼.</param>
    /// <returns>EÃƒâ€¦Ã…Â¸ya sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±. EÃƒâ€¦Ã…Â¸ya yoksa 0.</returns>
    public int GetItemCount(ItemType type)
    {
        // Makine parÃƒÆ’Ã‚Â§alarÃƒâ€Ã‚Â± ayrÃƒâ€Ã‚Â± listeden sorgulanÃƒâ€Ã‚Â±r
        if (type == ItemType.MachinePart)
        {
            return GetMachinePartCount();
        }

        InventoryItem existing = FindItemByType(items, type);
        return existing != null ? existing.count : 0;
    }

    /// <summary>
    /// Oyuncunun belirtilen tÃƒÆ’Ã‚Â¼rde yeterli eÃƒâ€¦Ã…Â¸yasÃƒâ€Ã‚Â± olup olmadÃƒâ€Ã‚Â±Ãƒâ€Ã…Â¸Ãƒâ€Ã‚Â±nÃƒâ€Ã‚Â± kontrol eder.
    /// </summary>
    /// <param name="type">Kontrol edilecek eÃƒâ€¦Ã…Â¸ya tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼.</param>
    /// <param name="amount">Gereken minimum miktar.</param>
    /// <returns>Yeterli eÃƒâ€¦Ã…Â¸ya varsa true.</returns>
    public bool HasItem(ItemType type, int amount = 1)
    {
        return GetItemCount(type) >= amount;
    }

    /// <summary>
    /// Oyuncu bir slota tÃƒâ€Ã‚Â±kladÃƒâ€Ã‚Â±Ãƒâ€Ã…Â¸Ãƒâ€Ã‚Â±nda ÃƒÆ’Ã‚Â§aÃƒâ€Ã…Â¸rÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±r.
    /// Yiyecek: 25 aÃƒÆ’Ã‚Â§lÃƒâ€Ã‚Â±k geri kazandÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â±r. Ãƒâ€Ã‚Â°ÃƒÆ’Ã‚Â§ecek: 25 susuzluk geri kazandÃƒâ€Ã‚Â±rÃƒâ€Ã‚Â±r.
    /// Mermi ve makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± tÃƒâ€Ã‚Â±klamayla kullanÃƒâ€Ã‚Â±lamaz.
    /// </summary>
    /// <param name="slotIndex">TÃƒâ€Ã‚Â±klanan slotun indeksi.</param>
        public void UseMachinePart(int slotIndex)
    {
        // Makine parcalarina tiklayinca ozel bir eylem yapilmiyorsa bos birakilabilir.
        Debug.Log("Makine parcasi secildi: " + slotIndex);
    }

    public void UseItem(int slotIndex)
    {
        // GeÃƒÆ’Ã‚Â§ersiz indeks kontrolÃƒÆ’Ã‚Â¼
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            Debug.LogWarning($"[InventoryManager] GeÃƒÆ’Ã‚Â§ersiz slot indeksi: {slotIndex}");
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
                // Mermi tÃƒâ€Ã‚Â±klamayla kullanÃƒâ€Ã‚Â±lamaz. Sadece R tuÃƒâ€¦Ã…Â¸uyla yeniden doldurma sistemi ÃƒÆ’Ã‚Â¼zerinden tÃƒÆ’Ã‚Â¼ketilir.
                Debug.Log("[InventoryManager] Mermi tiklamayla kullanilamaz. Yeniden doldurmak icin R tusunu kullanin.");
                break;

            case ItemType.MachinePart:
                // Makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± tÃƒâ€Ã‚Â±klamayla kullanÃƒâ€Ã‚Â±lamaz.
                Debug.Log("[InventoryManager] Asi parcasi dogrudan kullanilamaz.");
                break;
        }
    }

    /// <summary>
    /// Yeniden doldurma sistemi iÃƒÆ’Ã‚Â§in mermi tÃƒÆ’Ã‚Â¼ketir.
    /// Silah yeniden doldurulurken bu yÃƒÆ’Ã‚Â¶ntem ÃƒÆ’Ã‚Â§aÃƒâ€Ã…Â¸rÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±r.
    /// </summary>
    /// <param name="ammoType">TÃƒÆ’Ã‚Â¼ketilecek mermi tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼ (PistolAmmo veya RifleAmmo).</param>
    /// <param name="amount">TÃƒÆ’Ã‚Â¼ketilecek mermi miktarÃƒâ€Ã‚Â±.</param>
    /// <returns>Yeterli mermi varsa ve tÃƒÆ’Ã‚Â¼ketim baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±ysa true.</returns>
    public bool ConsumeAmmo(ItemType ammoType, int amount)
    {
        // Sadece mermi tÃƒÆ’Ã‚Â¼rlerinin tÃƒÆ’Ã‚Â¼ketilmesine izin ver
        if (ammoType != ItemType.PistolAmmo && ammoType != ItemType.RifleAmmo)
        {
            Debug.LogWarning($"[InventoryManager] ConsumeAmmo sadece mermi turleri icin kullanilabilir. Gecersiz tur: {ammoType}");
            return false;
        }

        return RemoveItem(ammoType, amount);
    }

    // ==================== Makine ParÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± YÃƒÆ’Ã‚Â¶ntemleri ====================

    /// <summary>
    /// Makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± ayrÃƒâ€Ã‚Â± listeye ekler.
    /// Ekleme baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â± olursa GameManager ÃƒÆ’Ã‚Â¼zerinden EsyaToplandi() ÃƒÆ’Ã‚Â§aÃƒâ€Ã…Â¸rÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±r.
    /// </summary>
    /// <param name="name">ParÃƒÆ’Ã‚Â§anÃƒâ€Ã‚Â±n adÃƒâ€Ã‚Â±.</param>
    /// <param name="icon">ParÃƒÆ’Ã‚Â§anÃƒâ€Ã‚Â±n UI ikonu.</param>
    /// <param name="amount">Eklenecek miktar.</param>
    /// <returns>Ekleme baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±ysa true.</returns>
    private bool AddMachinePart(string name, Sprite icon, int amount)
    {
        // AynÃƒâ€Ã‚Â± isimde makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± var mÃƒâ€Ã‚Â± kontrol et
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
            // Mevcut parÃƒÆ’Ã‚Â§anÃƒâ€Ã‚Â±n sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± artÃƒâ€Ã‚Â±r
            existing.count += amount;
            Debug.Log($"[InventoryManager] Makine parcasi sayisi artirildi: '{existing.itemName}' x{existing.count}");
        }
        else
        {
            // Kapasite kontrolÃƒÆ’Ã‚Â¼
            if (machineParts.Count >= MAX_MACHINE_PARTS_CAPACITY)
            {
                Debug.LogWarning("[InventoryManager] Asi parcalari listesi dolu! Parca eklenemedi: " + name);
                return false;
            }

            // Yeni makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± oluÃƒâ€¦Ã…Â¸tur
            InventoryItem newPart = new InventoryItem(ItemType.MachinePart, name, icon, amount);
            machineParts.Add(newPart);
            Debug.Log($"[InventoryManager] Yeni makine parcasi eklendi: '{name}' x{amount}");
        }

        // Mevcut zanaat sistemi ile uyumluluk iÃƒÆ’Ã‚Â§in GameManager'a bildir
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
    /// Makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± listesinden parÃƒÆ’Ã‚Â§a ÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±r (ilk bulunan parÃƒÆ’Ã‚Â§adan).
    /// </summary>
    /// <param name="amount">ÃƒÆ’Ã¢â‚¬Â¡Ãƒâ€Ã‚Â±karÃƒâ€Ã‚Â±lacak miktar.</param>
    /// <returns>ÃƒÆ’Ã¢â‚¬Â¡Ãƒâ€Ã‚Â±karma baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±lÃƒâ€Ã‚Â±ysa true.</returns>
    private bool RemoveMachinePart(int amount)
    {
        // Toplam makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± hesapla
        int totalParts = GetMachinePartCount();

        if (totalParts < amount)
        {
            Debug.LogWarning($"[InventoryManager] Yeterli makine parcasi yok. Mevcut: {totalParts}, Istenen: {amount}");
            return false;
        }

        int remaining = amount;

        // Ãƒâ€Ã‚Â°lk parÃƒÆ’Ã‚Â§alardan baÃƒâ€¦Ã…Â¸layarak ÃƒÆ’Ã‚Â§Ãƒâ€Ã‚Â±kar
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
    /// Toplam makine parÃƒÆ’Ã‚Â§asÃƒâ€Ã‚Â± sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â± dÃƒÆ’Ã‚Â¶ndÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼r.
    /// </summary>
    /// <returns>TÃƒÆ’Ã‚Â¼m makine parÃƒÆ’Ã‚Â§alarÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â±n toplam sayÃƒâ€Ã‚Â±sÃƒâ€Ã‚Â±.</returns>
    private int GetMachinePartCount()
    {
        int total = 0;
        for (int i = 0; i < machineParts.Count; i++)
        {
            total += machineParts[i].count;
        }
        return total;
    }

    // ==================== YardÃƒâ€Ã‚Â±mcÃƒâ€Ã‚Â± YÃƒÆ’Ã‚Â¶ntemler ====================

    /// <summary>
    /// Verilen listede belirtilen tÃƒÆ’Ã‚Â¼rde eÃƒâ€¦Ã…Â¸ya arar.
    /// </summary>
    /// <param name="list">Aranacak eÃƒâ€¦Ã…Â¸ya listesi.</param>
    /// <param name="type">Aranan eÃƒâ€¦Ã…Â¸ya tÃƒÆ’Ã‚Â¼rÃƒÆ’Ã‚Â¼.</param>
    /// <returns>Bulunan eÃƒâ€¦Ã…Â¸ya veya null.</returns>
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
