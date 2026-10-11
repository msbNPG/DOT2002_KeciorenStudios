using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Oyun genelini yoneten ana sistem.
/// Asi toplama sayaci, zamanlayici, popup mesajlari ve kazanma mantigi.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Asi Craftlama Sistemi")]
    public int toplananEsya = 0;
    public int gerekenEsya = 5;

    [Header("Arayuz (UI) Ayarlari")]
    [Tooltip("Ekranda 1/5 Asi Parcasi yazacak olan UI Text objesi")]
    public Text asiMiktariText;

    [Tooltip("Yazinin ekranda kac saniye kalacagi")]
    public float yaziEkrandaKalmaSuresi = 2f;

    [Tooltip("HUD'da surekli gorunen 'Asi Parcalari 0/5' sayaci (opsiyonel)")]
    public Text asiSayaciText;

    [Header("Anlik Oyun (Run) Istatistikleri")]
    public int currentRunZombieKills = 0;
    public float runTimer = 0f;
    private bool isGameWon = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Inspector'dan atanmamissa otomatik olustur
        if (asiMiktariText == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject txtObj = new GameObject("AsiMiktariText");
                txtObj.transform.SetParent(canvas.transform, false);

                RectTransform rect = txtObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 100);
                rect.sizeDelta = new Vector2(600, 100);

                asiMiktariText = txtObj.AddComponent<Text>();
                asiMiktariText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                asiMiktariText.fontSize = 32;
                asiMiktariText.fontStyle = FontStyle.Bold;
                asiMiktariText.alignment = TextAnchor.MiddleCenter;
                asiMiktariText.color = Color.yellow;
                asiMiktariText.text = "";

                // Okunabilirlik icin outline ekle
                Outline outline = txtObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2, -2);
            }
        }
        else
        {
            asiMiktariText.text = "";
        }

        UpdateAsiSayaci();
    }

    private void UpdateAsiSayaci()
    {
        if (asiSayaciText == null) return;
        asiSayaciText.text = toplananEsya + " / " + gerekenEsya;
        asiSayaciText.color = toplananEsya >= gerekenEsya ? new Color(0.55f, 0.9f, 0.4f) : Color.white;
    }

    void Update()
    {
        // Oyun kazanilmadigi surece sureyi artir
        if (!isGameWon)
        {
            runTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// InventoryManager asi parcasi alindiginda bunu cagirir.
    /// </summary>
    public void EsyaToplandi()
    {
        if (toplananEsya < gerekenEsya)
        {
            toplananEsya++;
            UpdateAsiSayaci();
            Debug.Log("[GameManager] Asi Parcasi Toplandi: " + toplananEsya + " / " + gerekenEsya);

            if (toplananEsya >= gerekenEsya)
            {
                Debug.Log("[GameManager] BUTUN PARCALARI TOPLADIM ARTIK ASIYI URETEBILIRIM!");
                ShowPopupText("TÜM AŞI PARÇALARI TOPLANDI! AŞI İSTASYONUNA GİT", new Color(0.55f, 0.9f, 0.4f));
            }
            else
            {
                ShowPopupText("AŞI PARÇASI BULUNDU  " + toplananEsya + " / " + gerekenEsya, new Color(1f, 0.85f, 0.35f));
            }
        }
    }

    public void ShowPopupText(string message, Color color)
    {
        if (asiMiktariText != null)
        {
            asiMiktariText.text = message;
            asiMiktariText.color = color;

            StopAllCoroutines();
            StartCoroutine(HideTextAfterDelay());
        }
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(yaziEkrandaKalmaSuresi);
        if (asiMiktariText != null)
        {
            asiMiktariText.text = "";
        }
    }

    public void GameWon()
    {
        // Istasyona tekrar basinca ikinci kez kazanilmasin
        if (isGameWon) return;
        isGameWon = true;

        if (RunManager.Instance != null)
        {
            RunManager.Instance.CheckAndSaveWinTime(runTimer);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen(currentRunZombieKills, runTimer);
        }
    }
}