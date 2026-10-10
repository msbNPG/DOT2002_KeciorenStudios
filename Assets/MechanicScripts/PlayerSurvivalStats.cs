using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oyuncunun hayatta kalma istatistiklerini yÃ¶netir: AÃ§lÄ±k, Susuzluk ve SaÄŸlÄ±k.
/// Singleton yapÄ±sÄ± ile her yerden eriÅŸilebilir.
/// </summary>
public class PlayerSurvivalStats : MonoBehaviour
{
    // Singleton eriÅŸim noktasÄ±
    public static PlayerSurvivalStats Instance { get; private set; }

    [Header("AÃ§lÄ±k AyarlarÄ±")]
    [Tooltip("Maksimum aÃ§lÄ±k deÄŸeri")]
    public float maxHunger = 100f;
    [Tooltip("Mevcut aÃ§lÄ±k deÄŸeri")]
    public float currentHunger = 100f;
    [Tooltip("Saniyede azalan aÃ§lÄ±k miktarÄ±")]
    public float hungerDecreaseRate = 0.5f;

    [Header("Susuzluk AyarlarÄ±")]
    [Tooltip("Maksimum susuzluk deÄŸeri")]
    public float maxThirst = 100f;
    [Tooltip("Mevcut susuzluk deÄŸeri")]
    public float currentThirst = 100f;
    [Tooltip("Saniyede azalan susuzluk miktarÄ±")]
    public float thirstDecreaseRate = 0.7f;

    [Header("SaÄŸlÄ±k AyarlarÄ±")]
    [Tooltip("Maksimum saÄŸlÄ±k deÄŸeri")]
    public float maxHealth = 100f;
    [Tooltip("Mevcut saÄŸlÄ±k deÄŸeri")]
    public float currentHealth = 100f;

    [Header("UI Slider ReferanslarÄ±")]
    [Tooltip("AÃ§lÄ±k slider'Ä± (Inspector'dan atanabilir)")]
    public Slider hungerSlider;
    [Tooltip("Susuzluk slider'Ä± (Inspector'dan atanabilir)")]
    public Slider thirstSlider;
    [Tooltip("SaÄŸlÄ±k slider'Ä± (Inspector'dan atanabilir)")]
    public Slider healthSlider;

    // Oyuncunun Ã¶lÃ¼ olup olmadÄ±ÄŸÄ±nÄ± takip eder
    private bool isDead = false;

    /// <summary>
    /// Singleton kurulumu. Sahnede yalnÄ±zca bir tane olmasÄ±nÄ± garanti eder.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Zaten bir Instance varsa bu fazlalÄ±k nesneyi yok et
            Debug.LogWarning("[PlayerSurvivalStats] Sahnede birden fazla Instance tespit edildi. FazlalÄ±k yok ediliyor.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Her karede aÃ§lÄ±k ve susuzluk azaltÄ±lÄ±r, deÄŸerler kontrol edilir ve UI gÃ¼ncellenir.
    /// </summary>
    private void Update()
    {
        // Oyuncu Ã¶ldÃ¼yse hiÃ§bir ÅŸey yapma
        if (isDead) return;

        // AÃ§lÄ±ÄŸÄ± zamanla azalt
        currentHunger -= hungerDecreaseRate * Time.deltaTime;

        // SusuzluÄŸu zamanla azalt
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        // TÃ¼m deÄŸerleri 0 ile maksimum arasÄ±nda tut
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Herhangi bir deÄŸer sÄ±fÄ±ra ulaÅŸtÄ±ysa oyuncu Ã¶lÃ¼r
        if (currentHunger <= 0f || currentThirst <= 0f || currentHealth <= 0f)
        {
            // Ã–lÃ¼m sebebini logla ve Die() Ã§aÄŸÄ±r
            if (currentHealth <= 0f)
            {
                Die("SaÄŸlÄ±k");
            }
            else if (currentHunger <= 0f)
            {
                Die("AÃ§lÄ±k");
            }
            else
            {
                Die("Susuzluk");
            }
            return;
        }

        // UI slider'larÄ±nÄ± gÃ¼ncelle
        UpdateUI();
    }

    /// <summary>
    /// UI slider'larÄ±nÄ± normalize edilmiÅŸ deÄŸerlerle gÃ¼nceller (mevcut / maksimum).
    /// </summary>
    private void UpdateUI()
    {
        // AÃ§lÄ±k slider'Ä± atanmÄ±ÅŸsa gÃ¼ncelle
        if (hungerSlider != null)
        {
            hungerSlider.value = currentHunger;
        }

        // Susuzluk slider'Ä± atanmÄ±ÅŸsa gÃ¼ncelle
        if (thirstSlider != null)
        {
            thirstSlider.value = currentThirst;
        }

        // SaÄŸlÄ±k slider'Ä± atanmÄ±ÅŸsa gÃ¼ncelle
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    /// <summary>
    /// Oyuncuya hasar verir. Zombiler tarafÄ±ndan Ã§aÄŸrÄ±lÄ±r.
    /// Ã–rnek kullanÄ±m: PlayerSurvivalStats.Instance.TakeDamage(damageAmount);
    /// </summary>
    /// <param name="damage">Verilecek hasar miktarÄ±</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("[PlayerSurvivalStats] Oyuncu " + damage + " hasar aldÄ±. Kalan saÄŸlÄ±k: " + currentHealth);

        // SaÄŸlÄ±k sÄ±fÄ±ra dÃ¼ÅŸtÃ¼yse Ã¶l
        if (currentHealth <= 0f)
        {
            Die("SaÄŸlÄ±k");
        }
    }

    /// <summary>
    /// AÃ§lÄ±k deÄŸerini belirtilen miktar kadar geri yÃ¼kler, maksimum deÄŸeri aÅŸamaz.
    /// </summary>
    /// <param name="amount">Geri yÃ¼klenecek aÃ§lÄ±k miktarÄ±</param>
    public void RestoreHunger(float amount)
    {
        if (isDead) return;

        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

        Debug.Log("[PlayerSurvivalStats] AÃ§lÄ±k geri yÃ¼klendi: +" + amount + " | Mevcut: " + currentHunger);
    }

    /// <summary>
    /// Susuzluk deÄŸerini belirtilen miktar kadar geri yÃ¼kler, maksimum deÄŸeri aÅŸamaz.
    /// </summary>
    /// <param name="amount">Geri yÃ¼klenecek susuzluk miktarÄ±</param>
    public void RestoreThirst(float amount)
    {
        if (isDead) return;

        currentThirst += amount;
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        Debug.Log("[PlayerSurvivalStats] Susuzluk geri yÃ¼klendi: +" + amount + " | Mevcut: " + currentThirst);
    }

    /// <summary>
    /// SaÄŸlÄ±k deÄŸerini belirtilen miktar kadar geri yÃ¼kler, maksimum deÄŸeri aÅŸamaz.
    /// </summary>
    /// <param name="amount">Geri yÃ¼klenecek saÄŸlÄ±k miktarÄ±</param>
    public void RestoreHealth(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("[PlayerSurvivalStats] SaÄŸlÄ±k geri yÃ¼klendi: +" + amount + " | Mevcut: " + currentHealth);
    }

    /// <summary>
    /// Oyuncuyu Ã¶ldÃ¼rÃ¼r. Hareketi devre dÄ±ÅŸÄ± bÄ±rakÄ±r ve baÅŸarÄ±sÄ±z ekranÄ±nÄ± gÃ¶sterir.
    /// </summary>
    /// <param name="reason">Ã–lÃ¼m sebebi (AÃ§lÄ±k, Susuzluk veya SaÄŸlÄ±k)</param>
    private void Die(string reason)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[PlayerSurvivalStats] OYUNCU Ã–LDÃœ! Sebep: " + reason);

        // Son kez UI'Ä± gÃ¼ncelle (hepsi sÄ±fÄ±r olabilir)
        UpdateUI();

        // UIManager varsa baÅŸarÄ±sÄ±z ekranÄ±nÄ± gÃ¶ster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFailed();
        }
        else
        {
            Debug.LogWarning("[PlayerSurvivalStats] UIManager bulunamadÄ±! BaÅŸarÄ±sÄ±z ekranÄ± gÃ¶sterilemiyor.");
        }

        // Oyuncunun hareketini devre dÄ±ÅŸÄ± bÄ±rak
        DisablePlayerMovement();
    }

    /// <summary>
    /// Oyuncunun hareket bileÅŸenlerini devre dÄ±ÅŸÄ± bÄ±rakÄ±r.
    /// Ã–nce CharacterController arar, bulamazsa Infima Games Character bileÅŸenini arar.
    /// </summary>
    private void DisablePlayerMovement()
    {
        // CharacterController bileÅŸenini bul ve devre dÄ±ÅŸÄ± bÄ±rak
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            // Bu obje Ã¼zerinde yoksa, Player etiketli obje Ã¼zerinde ara
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                characterController = playerObj.GetComponent<CharacterController>();
            }
        }

        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("[PlayerSurvivalStats] CharacterController devre dÄ±ÅŸÄ± bÄ±rakÄ±ldÄ±.");
        }

        // Infima Games Character bileÅŸenini bul ve devre dÄ±ÅŸÄ± bÄ±rak
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
            Debug.Log("[PlayerSurvivalStats] Character bileÅŸeni devre dÄ±ÅŸÄ± bÄ±rakÄ±ldÄ±.");
        }

        // Ek gÃ¼venlik: MonoBehaviour tabanlÄ± hareket scriptlerini de devre dÄ±ÅŸÄ± bÄ±rakmaya Ã§alÄ±ÅŸ
        // EÄŸer bu obje Ã¼zerinde baÅŸka hareket scriptleri varsa onlarÄ± da kapat
        if (characterController == null && character == null)
        {
            Debug.LogWarning("[PlayerSurvivalStats] HiÃ§bir hareket bileÅŸeni bulunamadÄ±! Oyuncu hareketi durdurulamayabilir.");
        }
    }

    /// <summary>
    /// Oyuncunun Ã¶lÃ¼ olup olmadÄ±ÄŸÄ±nÄ± dÃ¶ndÃ¼rÃ¼r.
    /// </summary>
    /// <returns>Oyuncu Ã¶ldÃ¼yse true, yaÅŸÄ±yorsa false</returns>
    public bool IsDead()
    {
        return isDead;
    }
}
