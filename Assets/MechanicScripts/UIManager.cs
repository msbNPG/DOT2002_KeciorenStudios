using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Tum oyun arayuzunu yoneten ana sinif.
/// Panel gecisleri, cursor kontrolu, zaman yonetimi.
/// ESC ile pause, TAB ile envanter acilir.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneller")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject statsPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public GameObject failedPanel;
    public GameObject inventoryPanel;
    public GameObject gameplayHUD;

    [Header("Durumlar")]
    public bool isGameStarted = false;
    public bool isPaused = false;
    public bool isFailed = false;
    public bool isInventoryOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Oyun baþýnda eðer doðrudan bir oynanýþ sahnesinde deðilsek ana menüyü göster
        if (!isGameStarted)
        {
            ShowMainMenu();
        }
    }

    private void Update()
    {
        // Oldu ise hicbir tus isleme
        if (isFailed) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // ESC tusu: Pause menusunu ac/kapat
        if (kb.escapeKey.wasPressedThisFrame)
        {
            // Envanter aciksa once onu kapat
            if (isInventoryOpen)
            {
                ToggleInventory();
                return;
            }

            if (isPaused)
            {
                ResumePause();
            }
            else if (isGameStarted)
            {
                ShowPause();
            }
        }

        // TAB tusu: Envanter ac/kapat
        if (kb.tabKey.wasPressedThisFrame)
        {
            if (isGameStarted && !isPaused && !isFailed)
            {
                ToggleInventory();
            }
        }
    }

    // ========================
    //  MENU PANEL METOTLARI
    // ========================

    public void ShowMainMenu()
    {
        HideAllPanels();
        SetPanelActive(mainMenuPanel, true);
        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ShowSettings()
    {
        HideAllPanels();
        SetPanelActive(settingsPanel, true);
        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ShowStats()
    {
        HideAllPanels();
        SetPanelActive(statsPanel, true);
        Time.timeScale = 0f;
        UnlockCursor();

        if (statsPanel != null && RunManager.Instance != null && RunManager.Instance.currentData != null)
        {
            UnityEngine.UI.Text statsText = FindOrCreateChildText(statsPanel, "GlobalStatsText");
            if (statsText != null)
            {
                RunData data = RunManager.Instance.currentData;

                string rTimeStr = "Hic Bitirilmedi";
                if (data.fastestWinTime > 0)
                {
                    int rMin = Mathf.FloorToInt(data.fastestWinTime / 60F);
                    int rSec = Mathf.FloorToInt(data.fastestWinTime - rMin * 60);
                    rTimeStr = string.Format("{0:00}:{1:00}", rMin, rSec);
                }

                statsText.text = "=== KURESEL ISTATISTIKLER ===\n\n" +
                                 "Toplam Zombi: " + data.totalZombieKills + "\n" +
                                 "Zombiden Olme (Can): " + data.deathByHealth + "\n" +
                                 "Acliktan Olme: " + data.deathByHunger + "\n" +
                                 "Susuzluktan Olme: " + data.deathByThirst + "\n" +
                                 "En Iyi Sure (Rekor): " + rTimeStr;
            }
        }
    }

    public void ShowPause()
    {
        HideAllPanels();
        SetPanelActive(pausePanel, true);
        SetPanelActive(gameplayHUD, false);

        isPaused = true;
        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ResumePause()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameplayHUD, true);

        isPaused = false;
        Time.timeScale = 1f;
        LockCursor();
    }

    /// <summary>
    /// Zafer panelini gosterir, istatistikleri yazar, zamani durdurur.
    /// </summary>
    public void ShowWinScreen(int kills, float time)
    {
        HideAllPanels();
        SetPanelActive(victoryPanel, true);

        if (victoryPanel != null)
        {
            UnityEngine.UI.Text statsText = FindOrCreateChildText(victoryPanel, "WinStatsText");
            if (statsText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60F);
                int seconds = Mathf.FloorToInt(time - minutes * 60);
                string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);

                statsText.text = "SURE: " + timeStr + "\nZOMBI SKORU: " + kills;

                if (RunManager.Instance != null && RunManager.Instance.currentData.fastestWinTime > 0)
                {
                    int rMin = Mathf.FloorToInt(RunManager.Instance.currentData.fastestWinTime / 60F);
                    int rSec = Mathf.FloorToInt(RunManager.Instance.currentData.fastestWinTime - rMin * 60);
                    string rTimeStr = string.Format("{0:00}:{1:00}", rMin, rSec);
                    statsText.text += "\nEN IYI SURE: " + rTimeStr;
                }
            }
        }

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ShowFailed()
    {
        HideAllPanels();
        SetPanelActive(failedPanel, true);

        isFailed = true;
        Time.timeScale = 0f;
        UnlockCursor();
        DisablePlayerMovement();
    }

    // ========================
    //  OYUN AKISI METOTLARI
    // ========================

    public void StartGame()
    {
        HideAllPanels();
        SetPanelActive(gameplayHUD, true);

        isGameStarted = true;
        isPaused = false;
        isFailed = false;
        isInventoryOpen = false;

        Time.timeScale = 1f;
        LockCursor();
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        SetPanelActive(inventoryPanel, isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f;
            UnlockCursor();
        }
        else
        {
            Time.timeScale = 1f;
            LockCursor();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnPlayerDeath()
    {
        ShowFailed();
    }

    // ========================
    //  YARDIMCI METOTLAR
    // ========================

    public void HideAllPanels()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(statsPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(victoryPanel, false);
        SetPanelActive(failedPanel, false);
        SetPanelActive(inventoryPanel, false);
        SetPanelActive(gameplayHUD, false);

        isInventoryOpen = false;
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var character = player.GetComponent<InfimaGames.LowPolyShooterPack.Character>();
            if (character != null) character.SetCursorLock(true);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var character = player.GetComponent<InfimaGames.LowPolyShooterPack.Character>();
            if (character != null) character.SetCursorLock(false);
        }
    }

    private void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        MonoBehaviour[] behaviours = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != null && behaviour != this)
            {
                string typeName = behaviour.GetType().Name.ToLower();
                if (typeName.Contains("movement") || typeName.Contains("controller") ||
                    typeName.Contains("motor") || typeName.Contains("player") ||
                    typeName.Contains("input") || typeName.Contains("hareket"))
                {
                    behaviour.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Panel icinde isimle Text arar, yoksa yeni olusturur.
    /// </summary>
    private UnityEngine.UI.Text FindOrCreateChildText(GameObject panel, string childName)
    {
        foreach (Transform child in panel.transform)
        {
            if (child.name == childName)
            {
                return child.GetComponent<UnityEngine.UI.Text>();
            }
        }

        GameObject txtObj = new GameObject(childName);
        txtObj.transform.SetParent(panel.transform, false);

        RectTransform rect = txtObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -50);
        rect.sizeDelta = new Vector2(500, 300);

        UnityEngine.UI.Text statsText = txtObj.AddComponent<UnityEngine.UI.Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsText.fontSize = 24;
        statsText.alignment = TextAnchor.MiddleCenter;
        statsText.color = Color.white;

        return statsText;
    }
}
