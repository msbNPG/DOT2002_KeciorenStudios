#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editör aracı: Tools -> Generate Advanced UI Prefab menüsünden
/// tüm oyun arayüzünü içeren bir Canvas prefabı oluşturur.
/// Oluşturulan paneller: MainMenu, Settings, Stats, Pause, Victory, Failed, Inventory, GameplayHUD
/// </summary>
public static class UISystemBuilder
{
    // ─── Renk Sabitleri ─────────────────────────────────────────────────
    private static readonly Color DarkBg = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private static readonly Color ButtonColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    private static readonly Color ButtonHover = new Color(0.3f, 0.3f, 0.35f, 1f);
    private static readonly Color ButtonPressed = new Color(0.15f, 0.15f, 0.2f, 1f);
    private static readonly Color SlotColor = new Color(0.18f, 0.18f, 0.22f, 1f);
    private static readonly Color HungerColor;   // #FF8C00
    private static readonly Color ThirstColor;   // #1E90FF
    private static readonly Color HealthColor;   // #DC143C
    private static readonly Color SliderBgColor = new Color(0.12f, 0.12f, 0.16f, 1f);
    private static readonly Color CrosshairColor = new Color(1f, 1f, 1f, 0.85f);

    // ─── Statik yapıcı – hex renkleri ayrıştır ─────────────────────────
    static UISystemBuilder()
    {
        ColorUtility.TryParseHtmlString("#FF8C00", out HungerColor);
        ColorUtility.TryParseHtmlString("#1E90FF", out ThirstColor);
        ColorUtility.TryParseHtmlString("#DC143C", out HealthColor);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ANA GİRİŞ NOKTASI
    // ═══════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Generate Advanced UI Prefab")]
    public static void Generate()
    {
        // --- Kök Canvas ---
        GameObject canvasGO = new GameObject("AdvancedUI_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();



        // --- Paneller ---
        CreateMainMenuPanel(canvasGO.transform);
        CreateSettingsPanel(canvasGO.transform);
        CreateStatsPanel(canvasGO.transform);
        CreatePausePanel(canvasGO.transform);
        CreateVictoryPanel(canvasGO.transform);
        CreateFailedPanel(canvasGO.transform);
        CreateInventoryPanel(canvasGO.transform);
        CreateGameplayHUD(canvasGO.transform);

        // --- SCRIPT REFERANSLARINI BAGLA ---
        UIManager uiManager = canvasGO.AddComponent<UIManager>();
        InventoryManager inventoryManager = canvasGO.AddComponent<InventoryManager>();
        InventoryUI inventoryUI = canvasGO.AddComponent<InventoryUI>();
        PlayerSurvivalStats survivalStats = canvasGO.AddComponent<PlayerSurvivalStats>();

        // --- EventSystem yoksa olustur (Yeni Input System uyumlu) ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Paneller
        uiManager.mainMenuPanel = canvasGO.transform.Find("MainMenuPanel").gameObject;
        uiManager.settingsPanel = canvasGO.transform.Find("SettingsPanel").gameObject;
        uiManager.statsPanel = canvasGO.transform.Find("StatsPanel").gameObject;
        uiManager.pausePanel = canvasGO.transform.Find("PausePanel").gameObject;
        uiManager.victoryPanel = canvasGO.transform.Find("VictoryPanel").gameObject;
        uiManager.failedPanel = canvasGO.transform.Find("FailedPanel").gameObject;
        uiManager.inventoryPanel = canvasGO.transform.Find("InventoryPanel").gameObject;
        uiManager.gameplayHUD = canvasGO.transform.Find("GameplayHUD").gameObject;

        // InventoryUI referanslari (recursive arama yapmak daha guvenli)
        inventoryUI.itemGridParent = uiManager.inventoryPanel.transform.Find("GridBackground/GridContent");
        inventoryUI.machinePartGridParent = uiManager.inventoryPanel.transform.Find("MachineBg/MachineContent");
        
        // Eger tam pathleri farkliysa asagidaki gibi genisletilebilir:
        if (inventoryUI.machinePartGridParent == null) 
        {
            var allTransforms = uiManager.inventoryPanel.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms) {
                if (t.name == "GridContent") inventoryUI.itemGridParent = t;
                if (t.name == "MachinePartsContent") inventoryUI.machinePartGridParent = t;
            }
        }

        // PlayerInteract referansi
        PlayerInteract playerInteract = Object.FindFirstObjectByType<PlayerInteract>();
        if (playerInteract == null)
        {
            GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (playerCam != null) playerInteract = playerCam.AddComponent<PlayerInteract>();
        }

        if (playerInteract != null)
        {
            var allTexts = uiManager.gameplayHUD.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in allTexts)
            {
                if (t.gameObject.name == "InteractPromptText")
                {
                    playerInteract.interactPromptText = t;
                    break;
                }
            }
        }

        // SurvivalStats referanslari
        var hudTransforms = uiManager.gameplayHUD.GetComponentsInChildren<Slider>(true);
        foreach (var s in hudTransforms) {
            if (s.name == "HungerSlider") survivalStats.hungerSlider = s;
            if (s.name == "ThirstSlider") survivalStats.thirstSlider = s;
            if (s.name == "HealthSlider") survivalStats.healthSlider = s;
        }

        // Event Atamalari (Butonlari UIManager'a bagla)
        var btnPlay = uiManager.mainMenuPanel.transform.Find("PlayButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.onClick, new UnityEngine.Events.UnityAction(uiManager.StartGame));

        var btnSettings = uiManager.mainMenuPanel.transform.Find("SettingsButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowSettings));

        var btnQuit = uiManager.mainMenuPanel.transform.Find("QuitButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnQuit.onClick, new UnityEngine.Events.UnityAction(uiManager.QuitGame));

        var btnSetBack = uiManager.settingsPanel.transform.Find("BackButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSetBack.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowMainMenu));

        var btnStatsBack = uiManager.statsPanel.transform.Find("BackButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStatsBack.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowMainMenu));

        var btnResume = uiManager.pausePanel.transform.Find("ResumeButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnResume.onClick, new UnityEngine.Events.UnityAction(uiManager.ResumePause));

        var btnPauseSettings = uiManager.pausePanel.transform.Find("SettingsButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPauseSettings.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowSettings));

        var btnPauseMenu = uiManager.pausePanel.transform.Find("MainMenuButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPauseMenu.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowMainMenu));

        var btnVictoryMenu = uiManager.victoryPanel.transform.Find("MainMenuButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnVictoryMenu.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowMainMenu));

        var btnFailedRetry = uiManager.failedPanel.transform.Find("RetryButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnFailedRetry.onClick, new UnityEngine.Events.UnityAction(uiManager.RestartLevel));

        var btnFailedMenu = uiManager.failedPanel.transform.Find("MainMenuButton").GetComponent<UnityEngine.UI.Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnFailedMenu.onClick, new UnityEngine.Events.UnityAction(uiManager.ShowMainMenu));

        // --- Prefab olarak kaydet ---
        string folderPath = "Assets/Prefabs/GeneratedUI";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/Prefabs", "GeneratedUI");

        string prefabPath = folderPath + "/AdvancedUI.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);

        // Sahnedeki geçici nesneyi sil, prefab'ı seç
        Object.DestroyImmediate(canvasGO);
        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = savedPrefab;
        EditorGUIUtility.PingObject(savedPrefab);

        Debug.Log($"<color=#00FF88>[UISystemBuilder]</color> Gelişmiş UI prefabı başarıyla oluşturuldu: {prefabPath}");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  1) MAIN MENU PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateMainMenuPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "MainMenuPanel", true);

        // Başlık
        CreateText(panel.transform, "TitleText", "DOT 2002",
            TextAnchor.MiddleCenter, 64, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -80f), new Vector2(600f, 80f));

        // Butonlar – dikey sıralama
        float startY = 40f;
        float spacing = 70f;
        string[] labels = { "Play", "Settings", "Quit" };
        for (int i = 0; i < labels.Length; i++)
        {
            float yPos = startY - i * spacing;
            CreateButton(panel.transform, labels[i] + "Button", labels[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, yPos), new Vector2(320f, 55f));
        }

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  2) SETTINGS PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateSettingsPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "SettingsPanel", true);

        CreateText(panel.transform, "TitleText", "AYARLAR",
            TextAnchor.MiddleCenter, 48, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(400f, 60f));

        CreateButton(panel.transform, "BackButton", "Back",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 80f), new Vector2(260f, 50f));

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  3) STATS PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateStatsPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "StatsPanel", true);

        CreateText(panel.transform, "TitleText", "İSTATİSTİKLER",
            TextAnchor.MiddleCenter, 48, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(500f, 60f));

        CreateButton(panel.transform, "BackButton", "Back",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 80f), new Vector2(260f, 50f));

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  4) PAUSE PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreatePausePanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "PausePanel", true);

        CreateText(panel.transform, "TitleText", "DURAKLAT",
            TextAnchor.MiddleCenter, 48, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(400f, 60f));

        float startY = 30f;
        float spacing = 65f;
        string[] labels = { "Resume", "Settings", "MainMenu" };
        for (int i = 0; i < labels.Length; i++)
        {
            float yPos = startY - i * spacing;
            CreateButton(panel.transform, labels[i] + "Button", labels[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, yPos), new Vector2(300f, 50f));
        }

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  5) VICTORY PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateVictoryPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "VictoryPanel", true);

        CreateText(panel.transform, "TitleText", "ZAFER!",
            TextAnchor.MiddleCenter, 56, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -80f), new Vector2(400f, 70f));

        CreateButton(panel.transform, "NextLevelButton", "NextLevel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 10f), new Vector2(300f, 50f));

        CreateButton(panel.transform, "MainMenuButton", "MainMenu",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -60f), new Vector2(300f, 50f));

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  6) FAILED PANELİ
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateFailedPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "FailedPanel", true);

        CreateText(panel.transform, "TitleText", "BAŞARISIZ!",
            TextAnchor.MiddleCenter, 56, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -80f), new Vector2(400f, 70f));

        CreateButton(panel.transform, "RetryButton", "Retry",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 10f), new Vector2(300f, 50f));

        CreateButton(panel.transform, "MainMenuButton", "MainMenu",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -60f), new Vector2(300f, 50f));

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  7) INVENTORY PANELİ (TAB ile açılır)
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateInventoryPanel(Transform parent)
    {
        // --- Panel (tam ekran deil, ortalanmis kutu) ---
        GameObject panel = new GameObject("InventoryPanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        SetAnchors(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600f, 560f); // Genisletilmis boyut

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = DarkBg;

        // CanvasGroup
        CanvasGroup cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // "ENVANTER" Basligi
        CreateText(panel.transform, "TitleText", "ENVANTER",
            TextAnchor.MiddleCenter, 48, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -40f), new Vector2(400f, 60f));

        // Layout Constants
        float pad = 10f;

        // ==================== SOL TARAF (ANA ENVANTER) ====================
        int gridCols = 3;
        int gridRows = 3;
        float cellW = 80f;
        float cellH = 80f;
        float spX = 8f;
        float spY = 8f;

        float gridW = gridCols * cellW + (gridCols - 1) * spX + pad * 2f;
        float gridH = gridRows * cellH + (gridRows - 1) * spY + pad * 2f;

        // Yiyecek/Icecek Etiketi
        CreateText(panel.transform, "GridLabel", "Yiyecek / Icecek / Mermi",
            TextAnchor.MiddleCenter, 18, FontStyle.Normal,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-120f, -90f), new Vector2(gridW, 24f));

        // Grid Background
        GameObject gridBg = new GameObject("GridBackground");
        gridBg.transform.SetParent(panel.transform, false);
        RectTransform gridBgRect = gridBg.AddComponent<RectTransform>();
        SetAnchors(gridBgRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        gridBgRect.anchoredPosition = new Vector2(-120f, -114f - gridH * 0.5f);
        gridBgRect.sizeDelta = new Vector2(gridW, gridH);
        Image gridBgImg = gridBg.AddComponent<Image>();
        gridBgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        // Grid Content
        GameObject gridContent = new GameObject("GridContent");
        gridContent.transform.SetParent(gridBg.transform, false);
        RectTransform gridContentRect = gridContent.AddComponent<RectTransform>();
        gridContentRect.anchorMin = Vector2.zero;
        gridContentRect.anchorMax = Vector2.one;
        gridContentRect.offsetMin = new Vector2(pad, pad);
        gridContentRect.offsetMax = new Vector2(-pad, -pad);

        GridLayoutGroup glg = gridContent.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(cellW, cellH);
        glg.spacing = new Vector2(spX, spY);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = gridCols;
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;

        for (int i = 0; i < gridCols * gridRows; i++)
        {
            CreateInventorySlot(gridContent.transform, "Slot_" + i, cellW, cellH);
        }

        // ==================== SAG TARAF (MAKINE PARCALARI) ====================
        int machineRows = 5;
        float mCellW = 70f;
        float mCellH = 70f;
        float mSpY = 6f;

        float machineW = 1 * mCellW + pad * 2f;
        float machineH = machineRows * mCellH + (machineRows - 1) * mSpY + pad * 2f;

        CreateText(panel.transform, "MachinePartsLabel", "Makine Parcalari",
            TextAnchor.MiddleCenter, 18, FontStyle.Normal,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(160f, -90f), new Vector2(machineW + 40f, 24f));

        GameObject machineBg = new GameObject("MachineBg");
        machineBg.transform.SetParent(panel.transform, false);
        RectTransform machineBgRect = machineBg.AddComponent<RectTransform>();
        SetAnchors(machineBgRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        machineBgRect.anchoredPosition = new Vector2(160f, -114f - machineH * 0.5f);
        machineBgRect.sizeDelta = new Vector2(machineW, machineH);
        Image machineBgImg = machineBg.AddComponent<Image>();
        machineBgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        GameObject machineContent = new GameObject("MachinePartsContent");
        machineContent.transform.SetParent(machineBg.transform, false);
        RectTransform machineContentRect = machineContent.AddComponent<RectTransform>();
        machineContentRect.anchorMin = Vector2.zero;
        machineContentRect.anchorMax = Vector2.one;
        machineContentRect.offsetMin = new Vector2(pad, pad);
        machineContentRect.offsetMax = new Vector2(-pad, -pad);

        GridLayoutGroup mGlg = machineContent.AddComponent<GridLayoutGroup>();
        mGlg.cellSize = new Vector2(mCellW, mCellH);
        mGlg.spacing = new Vector2(0, mSpY);
        mGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        mGlg.constraintCount = 1; // DIKEY DIZILIM ICIN 1 KOLON

        for (int i = 0; i < machineRows; i++)
        {
            CreateInventorySlot(machineContent.transform, "MachineSlot_" + i, mCellW, mCellH);
        }

        panel.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  8) GAMEPLAY HUD
    // ═══════════════════════════════════════════════════════════════════
    private static void CreateGameplayHUD(Transform parent)
    {
        // HUD paneli – tam ekran, arka planı yok
        GameObject hud = new GameObject("GameplayHUD");
        hud.transform.SetParent(parent, false);
        RectTransform hudRect = hud.AddComponent<RectTransform>();
        StretchFull(hudRect);

        // ─── Sol Üst: Durum çubukları ───────────────────────────────
        GameObject statusGroup = new GameObject("StatusBars");
        statusGroup.transform.SetParent(hud.transform, false);
        RectTransform statusRect = statusGroup.AddComponent<RectTransform>();
        SetAnchors(statusRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        statusRect.anchoredPosition = new Vector2(20f, -20f);
        statusRect.sizeDelta = new Vector2(260f, 100f);

        // VerticalLayoutGroup ile dikey sıralama
        VerticalLayoutGroup vlg = statusGroup.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childAlignment = TextAnchor.UpperLeft;

        CreateStatSlider(statusGroup.transform, "HungerSlider", "Açlık", HungerColor, 200f, 20f);
        CreateStatSlider(statusGroup.transform, "ThirstSlider", "Susuzluk", ThirstColor, 200f, 20f);
        CreateStatSlider(statusGroup.transform, "HealthSlider", "Sağlık", HealthColor, 200f, 20f);

        // Etkilesim Metni (Ekranin ortasinin biraz altinda)
        CreateText(hud.transform, "InteractPromptText", "[E] Esyayi Al",
            TextAnchor.MiddleCenter, 24, FontStyle.Bold,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -60f), new Vector2(600f, 40f));

        // HUD her zaman aktif
        hud.SetActive(true);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  YARDIMCI METOTLAR
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tam ekranı kaplayan, yarı saydam arka planlı panel oluşturur.
    /// </summary>
    private static GameObject CreatePanel(Transform parent, string name, bool fullscreen)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        if (fullscreen)
        {
            StretchFull(rect);
        }

        Image img = panel.AddComponent<Image>();
        img.color = DarkBg;
        img.raycastTarget = true;

        return panel;
    }

    /// <summary>
    /// Standart buton oluşturur.
    /// </summary>
    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        RectTransform rect = btnGO.AddComponent<RectTransform>();
        SetAnchors(rect, anchorMin, anchorMax, pivot);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = ButtonColor;

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = ButtonColor;
        cb.highlightedColor = ButtonHover;
        cb.pressedColor = ButtonPressed;
        cb.selectedColor = ButtonHover;
        cb.disabledColor = new Color(0.15f, 0.15f, 0.18f, 0.6f);
        cb.fadeDuration = 0.1f;
        btn.colors = cb;

        // Buton içi yazı
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        StretchFull(textRect);

        Text txt = textGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;

        return btnGO;
    }

    /// <summary>
    /// Metin nesnesi oluşturur.
    /// </summary>
    private static GameObject CreateText(Transform parent, string name, string content,
        TextAnchor alignment, int fontSize, FontStyle style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        SetAnchors(rect, anchorMin, anchorMax, pivot);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = Color.white;
        txt.alignment = alignment;
        txt.raycastTarget = false;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        return go;
    }

    /// <summary>
    /// Envanter slotu oluşturur: Buton + İkon Image + Adet/Ad Text
    /// </summary>
    private static GameObject CreateInventorySlot(Transform parent, string name, float w, float h)
    {
        GameObject slot = new GameObject(name);
        slot.transform.SetParent(parent, false);

        RectTransform slotRect = slot.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(w, h);

        Image slotImg = slot.AddComponent<Image>();
        slotImg.color = SlotColor;

        Button slotBtn = slot.AddComponent<Button>();
        ColorBlock scb = slotBtn.colors;
        scb.normalColor = SlotColor;
        scb.highlightedColor = new Color(0.28f, 0.28f, 0.34f, 1f);
        scb.pressedColor = new Color(0.14f, 0.14f, 0.18f, 1f);
        scb.selectedColor = new Color(0.28f, 0.28f, 0.34f, 1f);
        scb.fadeDuration = 0.08f;
        slotBtn.colors = scb;

        // İkon (eşya resmi)
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(slot.transform, false);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        SetAnchors(iconRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        iconRect.anchoredPosition = new Vector2(0f, 4f);
        iconRect.sizeDelta = new Vector2(w * 0.6f, h * 0.6f);

        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = new Color(1f, 1f, 1f, 0.3f);
        iconImg.raycastTarget = false;

        // Adet / İsim yazısı
        GameObject countText = new GameObject("CountText");
        countText.transform.SetParent(slot.transform, false);
        RectTransform countRect = countText.AddComponent<RectTransform>();
        SetAnchors(countRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
        countRect.anchoredPosition = new Vector2(0f, 2f);
        countRect.sizeDelta = new Vector2(0f, 18f);

        Text cTxt = countText.AddComponent<Text>();
        cTxt.text = "";
        cTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cTxt.fontSize = 12;
        cTxt.fontStyle = FontStyle.Normal;
        cTxt.color = Color.white;
        cTxt.alignment = TextAnchor.MiddleCenter;
        cTxt.raycastTarget = false;

        return slot;
    }

    /// <summary>
    /// Durum slider'ı oluşturur (Açlık / Susuzluk / Sağlık).
    /// </summary>
    private static GameObject CreateStatSlider(Transform parent, string name, string label,
        Color fillColor, float width, float height)
    {
        // Satır kapsayıcı
        GameObject row = new GameObject(name + "_Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(width + 60f, height);

        // Etiket
        GameObject lblGO = new GameObject("Label");
        lblGO.transform.SetParent(row.transform, false);
        RectTransform lblRect = lblGO.AddComponent<RectTransform>();
        SetAnchors(lblRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        lblRect.anchoredPosition = Vector2.zero;
        lblRect.sizeDelta = new Vector2(70f, 0f);

        Text lblTxt = lblGO.AddComponent<Text>();
        lblTxt.text = label;
        lblTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lblTxt.fontSize = 14;
        lblTxt.fontStyle = FontStyle.Bold;
        lblTxt.color = Color.white;
        lblTxt.alignment = TextAnchor.MiddleLeft;
        lblTxt.raycastTarget = false;
        lblTxt.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Slider nesnesi
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(row.transform, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        SetAnchors(sliderRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        sliderRect.anchoredPosition = new Vector2(72f, 0f);
        sliderRect.sizeDelta = new Vector2(width, 0f);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.interactable = false;

        // Arka plan
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        StretchFull(bgRect);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = SliderBgColor;

        // Doldurma alanı
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        StretchFull(fillAreaRect);

        // Doldurma görseli
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        StretchFull(fillRect);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = fillColor;

        slider.fillRect = fillRect;

        return row;
    }

    /// <summary>
    /// Ekran ortasına + şeklinde nişangâh oluşturur.
    /// </summary>
    private static void CreateCrosshair(Transform parent)
    {
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(parent, false);
        RectTransform chRect = crosshair.AddComponent<RectTransform>();
        SetAnchors(chRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        chRect.anchoredPosition = Vector2.zero;
        chRect.sizeDelta = new Vector2(30f, 30f);

        float thickness = 2f;
        float length = 16f;

        // Yatay çizgi
        GameObject hLine = new GameObject("HorizontalLine");
        hLine.transform.SetParent(crosshair.transform, false);
        RectTransform hRect = hLine.AddComponent<RectTransform>();
        SetAnchors(hRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        hRect.anchoredPosition = Vector2.zero;
        hRect.sizeDelta = new Vector2(length, thickness);
        Image hImg = hLine.AddComponent<Image>();
        hImg.color = CrosshairColor;
        hImg.raycastTarget = false;

        // Dikey çizgi
        GameObject vLine = new GameObject("VerticalLine");
        vLine.transform.SetParent(crosshair.transform, false);
        RectTransform vRect = vLine.AddComponent<RectTransform>();
        SetAnchors(vRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        vRect.anchoredPosition = Vector2.zero;
        vRect.sizeDelta = new Vector2(thickness, length);
        Image vImg = vLine.AddComponent<Image>();
        vImg.color = CrosshairColor;
        vImg.raycastTarget = false;
    }

    // ─── RectTransform Yardımcıları ─────────────────────────────────
    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
    }
}
#endif
