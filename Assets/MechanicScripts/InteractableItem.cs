using UnityEngine;

public enum ConsumableEffect
{
    None = 0,
    Tiny_5 = 5,
    Small_10 = 10,
    Medium_25 = 25,
    Large_50 = 50,
    Full_100 = 100
}

/// <summary>
/// Sahnedeki toplanabilir esyalari temsil eder.
/// Yeni sistemde bu script, fiziksel collider'lara (isTrigger = false) eklenebilir.
/// PlayerInteract scripti tarafindan Raycast ile tespit edilip toplanir.
/// </summary>
public class InteractableItem : MonoBehaviour
{
    [Header("Esya Ayarlari")]
    public ItemType itemType = ItemType.Food;
    
    [Tooltip("Eger bu bir Yiyecek, Icecek veya Saglik esyasiysa ne kadar deger verecegini buradan secin (Orn: Small_10)")]
    public ConsumableEffect effectSize = ConsumableEffect.Medium_25;

    public string itemName = "Yiyecek";
    public Sprite itemIcon; // Envanterde gorunecek ikon
    public int amount = 1; // Oyuncuya verilecek miktar

    [Header("Etkilesim")]
    public float interactDistance = 3f;

    /// <summary>
    /// PlayerInteract scripti tarafindan cagrilarak esyayi envantere ekler.
    /// </summary>
    public void PickUp()
    {
        // InventoryManager uzerinden envantere eklemeyi dene
        if (InventoryManager.Instance != null)
        {
            bool eklendi = InventoryManager.Instance.AddItem(itemType, itemName, itemIcon, amount, (int)effectSize);

            if (eklendi)
            {
                // Basariyla eklendiyse esyayi sahneden kaldir
                Debug.Log(itemName + " envantere eklendi! (x" + amount + ")");
                Destroy(gameObject);
            }
            else
            {
                // Envanter doluysa oyuncuyu bilgilendir
                Debug.Log("Envanter dolu, esya alinamadi: " + itemName);
            }
        }
    }
}