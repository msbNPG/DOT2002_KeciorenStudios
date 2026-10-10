using UnityEngine;
using UnityEngine.AI;
using InfimaGames.LowPolyShooterPack;

public enum ZombieType
{
    Low,
    Medium,
    High
}

public enum ZombieState
{
    Patrol,
    Chase,
    Attack
}

public class ZombieAI : MonoBehaviour
{
    [Header("Zombie Settings")]
    public ZombieType zombieType = ZombieType.Medium;

    [Header("Current Stats (Auto-assigned)")]
    public int maxHealth;
    public int currentHealth;
    public int attackDamage;
    public float moveSpeed;
    public float hearingRange;

    [Header("Ranges & Timers")]
    public float attackRange = 2f;
    public float patrolRadius = 15f;
    public float patrolWaitTime = 3f;
    public float attackCooldown = 2f;

    private float lastAttackTime = 0f;
    private float patrolWaitTimer = 0f;

    private ZombieState currentState = ZombieState.Patrol;
    private Vector3 startPosition;
    private Vector3 currentPatrolDestination;

    private Transform player;
    private CharacterController playerController;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false;

    void Awake()
    {
        UpdateStats();
        currentHealth = maxHealth;
    }

    // Inspector'da bir deger degistiginde otomatik calisir (Sadece Editor icinde)
    void OnValidate()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        // Turune gore ozellikleri belirle
        switch (zombieType)
        {
            case ZombieType.Low:
                maxHealth = 50;
                attackDamage = 10;
                moveSpeed = 2.5f;
                hearingRange = 10f;
                break;
            case ZombieType.Medium:
                maxHealth = 100;
                attackDamage = 20;
                moveSpeed = 4.0f;
                hearingRange = 20f;
                break;
            case ZombieType.High:
                maxHealth = 200;
                attackDamage = 35;
                moveSpeed = 5.5f;
                hearingRange = 35f;
                break;
        }
        
        // Editor'de degerleri test ederken veya ilk atamada currentHealth'in sifir kalmamasi icin
        if (!Application.isPlaying)
        {
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        agent.speed = moveSpeed;
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<CharacterController>();
        }

        // Ilk devriye noktasini sec
        SetRandomPatrolDestination();
    }

    void OnEnable()
    {
        Weapon.OnGunshot += HearGunshot;
    }

    void OnDisable()
    {
        Weapon.OnGunshot -= HearGunshot;
    }

    private void HearGunshot(Vector3 shotPosition, float soundRadius)
    {
        if (isDead) return;

        float distance = Vector3.Distance(transform.position, shotPosition);
        if (distance <= soundRadius)
        {
            // Silah sesini duydu! Direk kovalamaya basla
            currentState = ZombieState.Chase;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Ayak sesi algilama (Karakter hareket ediyorsa ve duyma mesafesindeyse)
        if (currentState == ZombieState.Patrol)
        {
            if (distanceToPlayer <= hearingRange)
            {
                // Player'in CharacterController'i varsa ve velocity buyuklugu 0.1'den fazlaysa hareket ediyordur.
                if (playerController != null && playerController.velocity.magnitude > 0.1f)
                {
                    currentState = ZombieState.Chase;
                }
            }
        }

        // Saldiri Menzili Kontrolu
        if (distanceToPlayer <= attackRange)
        {
            currentState = ZombieState.Attack;
        }
        else if (currentState == ZombieState.Attack && distanceToPlayer > attackRange)
        {
            currentState = ZombieState.Chase;
        }

        // State'lere gore davranis
        switch (currentState)
        {
            case ZombieState.Patrol:
                UpdatePatrol();
                break;
            case ZombieState.Chase:
                UpdateChase();
                break;
            case ZombieState.Attack:
                UpdateAttack();
                break;
        }
    }

    void UpdatePatrol()
    {
        agent.isStopped = false;
        // Yavas yurume hizi
        agent.speed = moveSpeed * 0.5f; 
        animator.SetFloat("Speed", 0.5f);

        // Hedefe vardik mi?
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            animator.SetFloat("Speed", 0f);
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                SetRandomPatrolDestination();
                patrolWaitTimer = 0f;
            }
        }
    }

    void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;
        NavMeshHit hit;
        
        // NavMesh uzerinde uygun bir nokta bul
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            currentPatrolDestination = hit.position;
            agent.SetDestination(currentPatrolDestination);
        }
    }

    void UpdateChase()
    {
        agent.isStopped = false;
        // Tam kosma hizi
        agent.speed = moveSpeed;
        agent.SetDestination(player.position);
        animator.SetFloat("Speed", 1f);
    }

    void UpdateAttack()
    {
        agent.isStopped = true;
        animator.SetFloat("Speed", 0f);

        // Yuzunu oyuncuya don
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Saniyede 60 kere saldirmasini engellemek icin zaman kontrolu
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;

            if (PlayerSurvivalStats.Instance != null)
            {
                PlayerSurvivalStats.Instance.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        // Hasar alinca kimin vurdugunu anlar ve kovalamaya baslar
        currentState = ZombieState.Chase;

        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

        void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        animator.SetTrigger("Die");
        
        // Istatistikleri guncelle
        if (GameManager.instance != null)
        {
            GameManager.instance.currentRunZombieKills++;
        }
        if (RunManager.Instance != null)
        {
            RunManager.Instance.AddZombieKill();
        }
    }

    // GIZMO CIZIMLERI (Sahnede gorsellestirmek icin)
    void OnDrawGizmosSelected()
    {
        // Devriye alani (Mavi)
        Gizmos.color = new Color(0, 0, 1, 0.3f); // Yari saydam mavi
        // Eger startPosition 0 ise baslangic yeri henuz belli degildir, kendi pozisyonunu ciz.
        Vector3 center = (Application.isPlaying && startPosition != Vector3.zero) ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        // Duyma mesafesi (Sari) - Awake henuz calismadiysa default hearingRange gosterir.
        // Ama biz yine de turlerine gore yaklasik cizdirebiliriz ya da o anki degere bakariz.
        Gizmos.color = new Color(1, 1, 0, 0.5f); // Yari saydam sari
        float currentHearing = Application.isPlaying ? hearingRange : GetHearingRangeByZombieType();
        Gizmos.DrawWireSphere(transform.position, currentHearing);

        // Saldiri mesafesi (Kirmizi)
        Gizmos.color = new Color(1, 0, 0, 0.7f); // Kirmizi
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // Sadece editoRde cizerken dogru deger gostermesi icin kucuk yardimci
    private float GetHearingRangeByZombieType()
    {
        switch (zombieType)
        {
            case ZombieType.Low: return 10f;
            case ZombieType.Medium: return 20f;
            case ZombieType.High: return 35f;
        }
        return 20f;
    }
}