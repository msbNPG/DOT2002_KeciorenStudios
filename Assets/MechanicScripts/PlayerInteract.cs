using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Oyuncunun kamerasindan cikan Raycast ile InteractableItem ve CraftingStation
/// objelerini tespit eder ve etkilesime girmesini saglar.
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    [Header("Etkilesim Ayarlari")]
    public float defaultInteractRange = 3f;
    public LayerMask interactLayerMask = ~0;

    [Header("UI Referanslari")]
    public Text interactPromptText;

    [Tooltip("Atanirsa yazi yerine bu kutu (tus simgesi + yazi) acilip kapanir")]
    public GameObject interactPromptRoot;

    private Camera mainCamera;

    void Start()
    {
        // Ana kamerayi bul
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>();
        }

        // Inspector'dan atanmamissa sahnede ara
        if (interactPromptText == null)
        {
            var allTexts = FindObjectsByType<Text>(FindObjectsInactive.Include);
            foreach (var t in allTexts)
            {
                if (t.gameObject.name == "InteractPromptText")
                {
                    interactPromptText = t;
                    break;
                }
            }
        }

        // Hala bulunamadiysa otomatik olustur
        if (interactPromptText == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject txtObj = new GameObject("InteractPromptText");
                txtObj.transform.SetParent(canvas.transform, false);

                RectTransform rect = txtObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -60);
                rect.sizeDelta = new Vector2(600, 40);

                interactPromptText = txtObj.AddComponent<Text>();
                interactPromptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                interactPromptText.fontSize = 24;
                interactPromptText.fontStyle = FontStyle.Bold;
                interactPromptText.alignment = TextAnchor.MiddleCenter;
                interactPromptText.color = Color.white;

                Outline outline = txtObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
            }
        }

        HidePrompt();
    }

    // Kutunun kendi [E] tus simgesi varsa yaziya ayrica [E] ekleme
    private string KeyPrefix => interactPromptRoot != null ? "" : "[E] ";

    void Update()
    {
        // Sadece menu/envanter/pause acikken engelle
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.isPaused || UIManager.Instance.isFailed || UIManager.Instance.isInventoryOpen)
            {
                HidePrompt();
                return;
            }
        }

        // Time.timeScale 0 ise (dondurulmus) etkilesimi engelle
        if (Time.timeScale == 0f)
        {
            HidePrompt();
            return;
        }

        if (mainCamera == null) return;

        // Ekranin tam ortasindan ileriye dogru isin yolla
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, defaultInteractRange, interactLayerMask))
        {
            // InteractableItem kontrolu
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null && hit.distance <= item.interactDistance)
            {
                ShowPrompt(KeyPrefix + "Topla: " + item.itemName);

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    item.PickUp();
                }
                return;
            }

            // CraftingStation kontrolu
            CraftingStation station = hit.collider.GetComponent<CraftingStation>();
            if (station != null && hit.distance <= defaultInteractRange)
            {
                ShowPrompt(KeyPrefix + "Kullan: " + station.stationName);

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    station.Interact();
                }
                return;
            }
        }

        HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = text;
            interactPromptText.enabled = true;
        }
        if (interactPromptRoot != null && !interactPromptRoot.activeSelf)
        {
            interactPromptRoot.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (interactPromptText != null)
        {
            interactPromptText.enabled = false;
        }
        if (interactPromptRoot != null && interactPromptRoot.activeSelf)
        {
            interactPromptRoot.SetActive(false);
        }
    }
}