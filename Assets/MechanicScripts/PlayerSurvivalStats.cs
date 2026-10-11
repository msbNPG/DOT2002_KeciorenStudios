using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oyuncunun hayatta kalma istatistiklerini yönetir: Açlık, Susuzluk ve Sağlık.
/// Singleton yapısı ile her yerden erişilebilir.
/// </summary>
public class PlayerSurvivalStats : MonoBehaviour
{
    // Singleton erişim noktası
    public static PlayerSurvivalStats Instance { get; private set; }

    [Header("Açlık Ayarları")]
    [Tooltip("Maksimum açlık değeri")]
    public float maxHunger = 100f;
    [Tooltip("Mevcut açlık değeri")]
    public float currentHunger = 100f;
    [Tooltip("Saniyede azalan açlık miktarı")]
    public float hungerDecreaseRate = 0.5f;

    [Header("Susuzluk Ayarları")]
    [Tooltip("Maksimum susuzluk değeri")]
    public float maxThirst = 100f;
    [Tooltip("Mevcut susuzluk değeri")]
    public float currentThirst = 100f;
    [Tooltip("Saniyede azalan susuzluk miktarı")]
    public float thirstDecreaseRate = 0.7f;

    [Header("Sağlık Ayarları")]
    [Tooltip("Maksimum sağlık değeri")]
    public float maxHealth = 100f;
    [Tooltip("Mevcut sağlık değeri")]
    public float currentHealth = 100f;

    [Header("UI Slider Referansları")]
    [Tooltip("Açlık slider'ı (Inspector'dan atanabilir)")]
    public Slider hungerSlider;
    [Tooltip("Susuzluk slider'ı (Inspector'dan atanabilir)")]
    public Slider thirstSlider;
    [Tooltip("Sağlık slider'ı (Inspector'dan atanabilir)")]
    public Slider healthSlider;

    // Oyuncunun ölü olup olmadığını takip eder
    private bool isDead = false;

    /// <summary>
    /// Singleton kurulumu. Sahnede yalnızca bir tane olmasını garanti eder.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Zaten bir Instance varsa bu fazlalık nesneyi yok et
            Debug.LogWarning("[PlayerSurvivalStats] Sahnede birden fazla Instance tespit edildi. Fazlalık yok ediliyor.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Her karede açlık ve susuzluk azaltılır, değerler kontrol edilir ve UI güncellenir.
    /// </summary>
    private void Update()
    {
        // Oyuncu öldüyse hiçbir şey yapma
        if (isDead) return;

        // Açlığı zamanla azalt
        currentHunger -= hungerDecreaseRate * Time.deltaTime;

        // Susuzluğu zamanla azalt
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        // Tüm değerleri 0 ile maksimum arasında tut
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Herhangi bir değer sıfıra ulaştıysa oyuncu ölür
        if (currentHunger <= 0f || currentThirst <= 0f || currentHealth <= 0f)
        {
            // Ölüm sebebini logla ve Die() çağır
            if (currentHealth <= 0f)
            {
                Die(DeathReasonHealth);
            }
            else if (currentHunger <= 0f)
            {
                Die(DeathReasonHunger);
            }
            else
            {
                Die(DeathReasonThirst);
            }
            return;
        }

        // UI slider'larını güncelle
        UpdateUI();
    }

    // RunManager.AddDeath bu anahtarlarla eşleşir
    public const string DeathReasonHealth = "Saglik";
    public const string DeathReasonHunger = "Aclik";
    public const string DeathReasonThirst = "Susuzluk";

    // Bu oranın altındaki barlar yanıp söner
    private const float LowStatThreshold = 0.25f;

    /// <summary>
    /// UI slider'larını normalize edilmiş değerlerle günceller (mevcut / maksimum).
    /// </summary>
    private void UpdateUI()
    {
        SetBar(hungerSlider, currentHunger, maxHunger);
        SetBar(thirstSlider, currentThirst, maxThirst);
        SetBar(healthSlider, currentHealth, maxHealth);
    }

    private static void SetBar(Slider slider, float current, float max)
    {
        if (slider == null) return;

        float ratio = max > 0f ? current / max : 0f;
        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, ratio);

        // Kritik seviyede dolguyu yanıp söndür
        Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        if (fill != null)
        {
            Color c = fill.color;
            c.a = ratio < LowStatThreshold ? 0.45f + 0.55f * Mathf.PingPong(Time.unscaledTime * 2.5f, 1f) : 1f;
            fill.color = c;
        }
    }

    /// <summary>
    /// Oyuncuya hasar verir. Zombiler tarafından çağrılır.
    /// Örnek kullanım: PlayerSurvivalStats.Instance.TakeDamage(damageAmount);
    /// </summary>
    /// <param name="damage">Verilecek hasar miktarı</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("[PlayerSurvivalStats] Oyuncu " + damage + " hasar aldı. Kalan sağlık: " + currentHealth);

        // Sağlık sıfıra düştüyse öl
        if (currentHealth <= 0f)
        {
            Die(DeathReasonHealth);
        }
    }

    /// <summary>
    /// Açlık değerini belirtilen miktar kadar geri yükler, maksimum değeri aşamaz.
    /// </summary>
    /// <param name="amount">Geri yüklenecek açlık miktarı</param>
    public void RestoreHunger(float amount)
    {
        if (isDead) return;

        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        Debug.Log("[PlayerSurvivalStats] Açlık geri yüklendi: +" + amount + " | Mevcut: " + currentHunger);
    }

    /// <summary>
    /// Susuzluk değerini belirtilen miktar kadar geri yükler, maksimum değeri aşamaz.
    /// </summary>
    /// <param name="amount">Geri yüklenecek susuzluk miktarı</param>
    public void RestoreThirst(float amount)
    {
        if (isDead) return;

        currentThirst += amount;
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        Debug.Log("[PlayerSurvivalStats] Susuzluk geri yüklendi: +" + amount + " | Mevcut: " + currentThirst);
    }

    /// <summary>
    /// Sağlık değerini belirtilen miktar kadar geri yükler, maksimum değeri aşamaz.
    /// </summary>
    /// <param name="amount">Geri yüklenecek sağlık miktarı</param>
    public void RestoreHealth(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("[PlayerSurvivalStats] Sağlık geri yüklendi: +" + amount + " | Mevcut: " + currentHealth);
    }

    /// <summary>
    /// Oyuncuyu öldürür. Hareketi devre dışı bırakır ve başarısız ekranını gösterir.
    /// </summary>
    /// <param name="reason">Ölüm sebebi (Açlık, Susuzluk veya Sağlık)</param>
    private void Die(string reason)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[PlayerSurvivalStats] OYUNCU ÖLDÜ! Sebep: " + reason);

        // Son kez UI'ı güncelle (hepsi sıfır olabilir)
        UpdateUI();

        // Kalıcı istatistiklere ölüm sebebini yaz
        if (RunManager.Instance != null)
        {
            RunManager.Instance.AddDeath(reason);
        }

        // UIManager varsa başarısız ekranını göster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFailed(reason);
        }
        else
        {
            Debug.LogWarning("[PlayerSurvivalStats] UIManager bulunamadı! Başarısız ekranı gösterilemiyor.");
        }

        // Oyuncunun hareketini devre dışı bırak
        DisablePlayerMovement();
    }

    /// <summary>
    /// Oyuncunun hareket bileşenlerini devre dışı bırakır.
    /// Önce CharacterController arar, bulamazsa Infima Games Character bileşenini arar.
    /// </summary>
    private void DisablePlayerMovement()
    {
        // CharacterController bileşenini bul ve devre dışı bırak
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            // Bu obje üzerinde yoksa, Player etiketli obje üzerinde ara
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                characterController = playerObj.GetComponent<CharacterController>();
            }
        }

        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("[PlayerSurvivalStats] CharacterController devre dışı bırakıldı.");
        }

        // Infima Games Character bileşenini bul ve devre dışı bırak
        var character = GetComponent<InfimaGames.LowPolyShooterPack.Character>();
        if (character == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                character = playerObj.GetComponent<InfimaGames.LowPolyShooterPack.Character>();
            }
        }

        if (character != null)
        {
            character.enabled = false;
            Debug.Log("[PlayerSurvivalStats] Character bileşeni devre dışı bırakıldı.");
        }

        // Ek güvenlik: MonoBehaviour tabanlı hareket scriptlerini de devre dışı bırakmaya çalış
        // Eğer bu obje üzerinde başka hareket scriptleri varsa onları da kapat
        if (characterController == null && character == null)
        {
            Debug.LogWarning("[PlayerSurvivalStats] Hiçbir hareket bileşeni bulunamadı! Oyuncu hareketi durdurulamayabilir.");
        }
    }

    /// <summary>
    /// Oyuncunun ölü olup olmadığını döndürür.
    /// </summary>
    /// <returns>Oyuncu öldüyse true, yaşıyorsa false</returns>
    public bool IsDead()
    {
        return isDead;
    }
}
