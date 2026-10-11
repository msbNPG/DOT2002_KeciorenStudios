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

    [Header("Ayarlar")]
    [Tooltip("Ayarlar panelindeki ses seviyesi slider'i (0-1)")]
    public UnityEngine.UI.Slider volumeSlider;

    [Header("Durumlar")]
    public bool isGameStarted = false;
    public bool isPaused = false;
    public bool isFailed = false;
    public bool isInventoryOpen = false;
    public bool isGameOver = false;

    // Sahne yeniden yuklenince ana menu yerine dogrudan oyunu baslat (Tekrar Dene)
    private static bool startGameOnLoad = false;

    // Ayarlar kapaninca pause menusune mi ana menuye mi donulecek
    private bool settingsOpenedFromPause = false;

    private const string VolumePrefKey = "MasterVolume";

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
        // Kayitli ses seviyesini uygula ve slider'a bagla
        AudioListener.volume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Tekrar Dene ile yuklendiysek menuyu atla
        if (startGameOnLoad)
        {
            startGameOnLoad = false;
            StartGame();
        }
        else if (!isGameStarted)
        {
            // Oyun basinda ana menuyu goster
            ShowMainMenu();
        }
    }

    private void Update()
    {
        // Olduyse veya kazandiysa hicbir tus isleme
        if (isFailed || isGameOver) return;

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

            // Ayarlar / istatistik ekranindan geri don
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }
            if (statsPanel != null && statsPanel.activeSelf)
            {
                ShowMainMenu();
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
        // Panel yoksa ekrani bos birakip oyuncuyu kilitleme
        if (settingsPanel == null)
        {
            Debug.LogWarning("[UIManager] Settings paneli atanmamis!");
            return;
        }

        settingsOpenedFromPause = isPaused;
        HideAllPanels();
        SetPanelActive(settingsPanel, true);
        Time.timeScale = 0f;
        UnlockCursor();
    }

    /// <summary>
    /// Ayarlar panelini kapatir ve acildigi yere (pause veya ana menu) doner.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsOpenedFromPause)
        {
            ShowPause();
        }
        else
        {
            ShowMainMenu();
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
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

                string rTimeStr = data.fastestWinTime > 0 ? FormatTime(data.fastestWinTime) : "—";

                statsText.text = StatLine("Öldürülen zombi", data.totalZombieKills.ToString()) +
                                 StatLine("Zombi yüzünden ölüm", data.deathByHealth.ToString()) +
                                 StatLine("Açlıktan ölüm", data.deathByHunger.ToString()) +
                                 StatLine("Susuzluktan ölüm", data.deathByThirst.ToString()) +
                                 StatLine("En iyi süre", rTimeStr).TrimEnd('\n');
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
        isGameOver = true;

        if (victoryPanel != null)
        {
            UnityEngine.UI.Text statsText = FindOrCreateChildText(victoryPanel, "WinStatsText");
            if (statsText != null)
            {
                statsText.text = "Süre: " + FormatTime(time) + "\nÖldürülen zombi: " + kills;

                if (RunManager.Instance != null && RunManager.Instance.currentData.fastestWinTime > 0)
                {
                    statsText.text += "\nEn iyi süre: " + FormatTime(RunManager.Instance.currentData.fastestWinTime);
                }
            }
        }

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ShowFailed()
    {
        ShowFailed(null);
    }

    /// <summary>
    /// Basarisiz ekranini gosterir. reason: PlayerSurvivalStats.DeathReason* sabitlerinden biri.
    /// </summary>
    public void ShowFailed(string reason)
    {
        HideAllPanels();
        SetPanelActive(failedPanel, true);

        if (failedPanel != null)
        {
            UnityEngine.UI.Text reasonText = FindOrCreateChildText(failedPanel, "FailedReasonText");
            if (reasonText != null)
            {
                reasonText.text = GetDeathMessage(reason);
            }
        }

        isFailed = true;
        Time.timeScale = 0f;
        UnlockCursor();
        DisablePlayerMovement();
    }

    private static string GetDeathMessage(string reason)
    {
        switch (reason)
        {
            case PlayerSurvivalStats.DeathReasonHealth: return "Zombiler seni yakaladı.";
            case PlayerSurvivalStats.DeathReasonHunger: return "Açlıktan öldün.";
            case PlayerSurvivalStats.DeathReasonThirst: return "Susuzluktan öldün.";
            default: return "Hayatta kalamadın.";
        }
    }

    private static string StatLine(string label, string value)
    {
        return "<color=#8C948F>" + label + "</color>    <b>" + value + "</b>\n";
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, secs);
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
        isGameOver = false;
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

    /// <summary>
    /// Bolumu sifirdan yukler ve menuyu atlayip dogrudan oyunu baslatir.
    /// </summary>
    public void RestartLevel()
    {
        startGameOnLoad = true;
        ReloadActiveScene();
    }

    /// <summary>
    /// Bolumu sifirdan yukler ve ana menuyu gosterir (envanter, sure, zombi durumu sifirlanir).
    /// </summary>
    public void GoToMainMenu()
    {
        startGameOnLoad = false;
        ReloadActiveScene();
    }

    private void ReloadActiveScene()
    {
        Time.timeScale = 1f;
        // buildIndex yerine isim: sahne build listesinde farkli sirada olabilir
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        // Panel icindeki kart/alt objelerde de ara
        foreach (UnityEngine.UI.Text existing in panel.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if (existing.name == childName)
            {
                return existing;
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
