using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// InventoryManager verisini UI panelindeki slotlarla senkronize eder.
/// Genel envanter icin 9 slot, makine parcalari icin 5 slot yonetir.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI Referanslari")]
    [Tooltip("9 adet genel esya slot butonunu iceren Grid Layout parent objesi")]
    public Transform itemGridParent;

    [Tooltip("5 adet makine parcasi slot butonunu iceren Grid Layout parent objesi")]
    public Transform machinePartGridParent;

    void Start()
    {
        // Envanter degisiklik olayina abone ol
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += RefreshUI;

            // Baslangicta UI'yi bir kere guncelle
            RefreshUI();
        }
        else
        {
            Debug.LogError("InventoryUI: InventoryManager bulunamadi! Sahnede InventoryManager oldugundan emin ol.");
        }
    }

    void OnDestroy()
    {
        // Sahne degistiginde veya obje yok edildiginde aboneligi kaldir
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= RefreshUI;
        }
    }

    /// <summary>
    /// Tum envanter slotlarini InventoryManager verisine gore gunceller.
    /// Hem genel esya slotlarini hem de makine parcasi slotlarini yeniler.
    /// </summary>
    public void RefreshUI()
    {
        // Genel esya slotlarini guncelle (9 slot)
        RefreshItemGrid();

        // Makine parcasi slotlarini guncelle (5 slot)
        RefreshMachinePartGrid();
    }

    /// <summary>
    /// 9 adet genel esya slotunu gunceller.
    /// Her slot icin ikon, miktar ve tiklanabilirlik ayarlanir.
    /// </summary>
    private void RefreshItemGrid()
    {
        if (itemGridParent == null)
        {
            Debug.LogWarning("InventoryUI: itemGridParent atanmamis!");
            return;
        }

        var items = InventoryManager.Instance.items;

        for (int i = 0; i < itemGridParent.childCount; i++)
        {
            Transform slotTransform = itemGridParent.GetChild(i);
            Button slotButton = slotTransform.GetComponent<Button>();
            Image iconImage = slotTransform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI countText = slotTransform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

            // TextMeshPro bulunamazsa standart Text dene
            Text countTextLegacy = null;
            if (countText == null)
            {
                countTextLegacy = slotTransform.Find("CountText")?.GetComponent<Text>();
            }

            if (i < items.Count && items[i] != null)
            {
                // Bu slotta bir esya var, bilgileri goster
                InventoryItem item = items[i];

                // Ikonu goster
                if (iconImage != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                }

                // Miktar yazisini guncelle
                if (countText != null)
                {
                    countText.text = item.count > 1 ? item.count.ToString() : "";
                    countText.enabled = true;
                }
                else if (countTextLegacy != null)
                {
                    countTextLegacy.text = item.count > 1 ? item.count.ToString() : "";
                    countTextLegacy.enabled = true;
                }

                // Butonu tiklanabilir yap ve sol/sag tiklama destegini ekle
                if (slotButton != null)
                {
                    slotButton.interactable = true;
                    slotButton.onClick.RemoveAllListeners(); // Artik sol tigi SlotRightClick yonetecek, normal onClick kapatilsa da olur ama dursun
                    
                    SlotRightClick clickHandler = slotTransform.GetComponent<SlotRightClick>();
                    if (clickHandler == null)
                    {
                        clickHandler = slotTransform.gameObject.AddComponent<SlotRightClick>();
                    }

                    int index = i; // Closure icin yerel kopya
                    clickHandler.onLeftClick = () =>
                    {
                        InventoryManager.Instance.UseItem(index);
                        RefreshUI();
                    };

                    clickHandler.onRightClick = () =>
                    {
                        InventoryManager.Instance.DropItem(index);
                        RefreshUI();
                    };
                }
            }
            else
            {
                // Bu slot bos, temizle
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (countText != null)
                {
                    countText.text = "";
                    countText.enabled = false;
                }
                else if (countTextLegacy != null)
                {
                    countTextLegacy.text = "";
                    countTextLegacy.enabled = false;
                }

                if (slotButton != null)
                {
                    slotButton.interactable = false;
                    slotButton.onClick.RemoveAllListeners();
                }
            }
        }
    }

    /// <summary>
    /// 5 adet makine parcasi slotunu gunceller.
    /// Her slot icin ikon, miktar ve tiklanabilirlik ayarlanir.
    /// </summary>
    private void RefreshMachinePartGrid()
    {
        if (machinePartGridParent == null)
        {
            Debug.LogWarning("InventoryUI: machinePartGridParent atanmamis!");
            return;
        }

        var machineParts = InventoryManager.Instance.machineParts;

        for (int i = 0; i < machinePartGridParent.childCount; i++)
        {
            Transform slotTransform = machinePartGridParent.GetChild(i);
            Button slotButton = slotTransform.GetComponent<Button>();
            Image iconImage = slotTransform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI countText = slotTransform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

            // TextMeshPro bulunamazsa standart Text dene
            Text countTextLegacy = null;
            if (countText == null)
            {
                countTextLegacy = slotTransform.Find("CountText")?.GetComponent<Text>();
            }

            if (i < machineParts.Count && machineParts[i] != null)
            {
                // Bu slotta bir makine parcasi var
                InventoryItem part = machineParts[i];

                // Ikonu goster
                if (iconImage != null)
                {
                    iconImage.sprite = part.icon;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                }

                // Miktar yazisini guncelle
                if (countText != null)
                {
                    countText.text = part.count > 1 ? part.count.ToString() : "";
                    countText.enabled = true;
                }
                else if (countTextLegacy != null)
                {
                    countTextLegacy.text = part.count > 1 ? part.count.ToString() : "";
                    countTextLegacy.enabled = true;
                }

                // Butonu tiklanabilir yap
                if (slotButton != null)
                {
                    slotButton.interactable = true;
                    slotButton.onClick.RemoveAllListeners();
                    int index = i; // Closure icin yerel kopya
                    slotButton.onClick.AddListener(() =>
                    {
                        InventoryManager.Instance.UseMachinePart(index);
                        RefreshUI();
                    });
                }
            }
            else
            {
                // Bu slot bos, temizle
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (countText != null)
                {
                    countText.text = "";
                    countText.enabled = false;
                }
                else if (countTextLegacy != null)
                {
                    countTextLegacy.text = "";
                    countTextLegacy.enabled = false;
                }

                if (slotButton != null)
                {
                    slotButton.interactable = false;
                    slotButton.onClick.RemoveAllListeners();
                }
            }
        }
    }
}
