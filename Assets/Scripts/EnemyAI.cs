using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    [SerializeField] private GameObject reskinParticlePrefab;
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Big Enemy Patrol")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float patrolPointReachedThreshold = 0.3f;
    [SerializeField] private float attackRange = 1.5f; // range at which it switches from patrol to chase

    private int currentHits = 0;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isAggroed = false;
    private bool isDead = false;

    private Vector2 patrolOrigin;
    private Vector2 currentPatrolTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameEvents.OnDreamChanged += HandleDreamChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnDreamChanged -= HandleDreamChanged;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        patrolOrigin = rb.position;
        PickNewPatrolTarget();

        ApplySkinForDream(FindManagerCurrentDream());
       
    }

    private int FindManagerCurrentDream()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        return gm != null ? gm.CurrentDream : 1;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (!data.isBig)
        {
            if (!isAggroed) isAggroed = true;
            ChasePlayer();
            return;
        }

        // Big enemy: chase only within attackRange, otherwise patrol
        if (dist <= attackRange)
        {
            if (!isAggroed)
            {
                isAggroed = true;
                Debug.Log($"[EnemyAI] Big enemy {data.enemyName} switched to chase (in attack range)");
            }
            ChasePlayer();
        }
        else if (dist <= data.aggroRange)
        {
            // In aggro range but not attack range yet — still patrol, doesn't chase from far
            isAggroed = false;
            Patrol();
        }
        else
        {
            isAggroed = false;
            Patrol();
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * data.moveSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        Vector2 dir = (currentPatrolTarget - rb.position).normalized;
        rb.MovePosition(rb.position + dir * (data.moveSpeed * 0.5f) * Time.deltaTime);

        if (Vector2.Distance(rb.position, currentPatrolTarget) <= patrolPointReachedThreshold)
        {
            PickNewPatrolTarget();
        }
    }

    private void PickNewPatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolOrigin + randomOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            GameEvents.TriggerRequestTakeDamage(1);
            Debug.Log($"[EnemyAI] {data.enemyName} hit player on contact");
        }
    }

    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"[EnemyAI] {data.enemyName} took hit {currentHits}/{data.maxHits}");

        if (currentHits >= data.maxHits)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"[EnemyAI] {data.enemyName} died, playing death sequence");

        rb.simulated = false; // stop physics/collisions immediately so it can't hit the player while dying
        GetComponent<Collider2D>().enabled = false;

        // Placeholder death handling until real animations exist:
        // spawns a particle burst and destroys after a short delay so it doesn't just vanish instantly.
        // Once you have a real death animation, replace this block with:
        //   animator.SetTrigger("Die"); then Destroy(gameObject, animationClipLength);
        if (deathParticlePrefab != null)
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

        GameEvents.TriggerEnemyDied(gameObject);
        Destroy(gameObject, 0.3f); // small buffer so particle isn't cut off; swap to animation length later
    }

    private void HandleDreamChanged(int dreamIndex)
    {
        ApplySkinForDream(dreamIndex);

        if (reskinParticlePrefab != null)
            Instantiate(reskinParticlePrefab, transform.position, Quaternion.identity);

        Debug.Log($"[EnemyAI] {data.enemyName} reskinned for dream {dreamIndex}");
    }

    private void ApplySkinForDream(int dreamIndex)
    {
        if (data.skinsPerDream == null) return;

        foreach (var skin in data.skinsPerDream)
        {
            if (skin.dreamIndex == dreamIndex)
            {
                sr.sprite = skin.skinSprite;
                return;
            }
        }
    }

    public void TakeHit(Vector2 knockbackDirection, float knockbackForce = 4f)
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"[EnemyAI] {data.enemyName} took hit {currentHits}/{data.maxHits}");

        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        if (currentHits >= data.maxHits)
        {
            Die();
        }
    }
}