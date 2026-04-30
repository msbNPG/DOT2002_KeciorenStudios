using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public int health = 100;

    // YENÝ: Saldýrý animasyonunun arka arkaya kitlenmemesi için bekleme süresi
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Menzile girdi, dur
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);

            // Yüzünü oyuncuya dön
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // YENÝ: Saniyede 60 kere saldýrmasýný engellemek için zaman kontrolü
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time; // Son saldýrý zamanýný kaydet
            }
        }
        else
        {
            // Uzakta, oyuncuyu kovala
            agent.isStopped = false;
            agent.SetDestination(player.position);

            // YENÝ: Yürüme animasyonunun kesin tetiklenmesi için direkt 1 veriyoruz
            animator.SetFloat("Speed", 1f);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        animator.SetTrigger("Die");
    }
}