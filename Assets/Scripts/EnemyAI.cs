using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visualRenderer; // drag the CHILD's SpriteRenderer here in Inspector

    [Header("Big Enemy Patrol")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float patrolPointReachedThreshold = 0.3f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Contact Damage")]
    [SerializeField] private float damageCooldown = 1f; // seconds between contact hits while touching

    private int currentHits = 0;
    private Transform player;
    private Rigidbody2D rb;
    private bool isAggroed = false;
    private bool isDead = false;
    private float lastDamageTime = -999f;

    private Vector2 patrolOrigin;
    private Vector2 currentPatrolTarget;

    public int CarriedOverHits { get; set; } = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        patrolOrigin = rb.position;
        PickNewPatrolTarget();

        currentHits = CarriedOverHits;

        if (visualRenderer == null)
            Debug.LogWarning($"[EnemyAI] {data.enemyName} has no Visual Renderer assigned — flipping won't work.");
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

        if (dist <= attackRange)
        {
            isAggroed = true;
            ChasePlayer();
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
        MoveAndRotate(dir, data.moveSpeed);
    }

    private void Patrol()
    {
        Vector2 dir = (currentPatrolTarget - rb.position).normalized;
        MoveAndRotate(dir, data.moveSpeed * 0.5f);

        if (Vector2.Distance(rb.position, currentPatrolTarget) <= patrolPointReachedThreshold)
        {
            PickNewPatrolTarget();
        }
    }

    private void MoveAndRotate(Vector2 direction, float speed)
    {
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);

        if (Mathf.Abs(direction.x) > 0.01f && visualRenderer != null)
        {
            // If this looks backwards on your sprite, swap to: direction.x > 0f
            visualRenderer.flipX = direction.x < 0f;
        }
    }

    private void PickNewPatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolOrigin + randomOffset;
    }

    // Fires once on first overlap
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    // Fires every physics frame while still overlapping — fixes "standing on it = no damage"
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void TryDealContactDamage(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        GameEvents.TriggerRequestTakeDamage(1);
        Debug.Log($"{data.enemyName} damaged player");
    }

    public void TakeHit(Vector2 knockbackDirection, float knockbackForce = 4f)
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"{data.enemyName} hit {currentHits}/{data.maxHits}");

        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

        if (currentHits >= data.maxHits)
        {
            Die();
        }
    }

    public int CurrentHits => currentHits;
    public bool IsBig => data.isBig;

    private void Die()
    {
        isDead = true;
        Debug.Log($"{data.enemyName} died");

        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;

        if (deathParticlePrefab != null)
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

        GameEvents.TriggerEnemyDied(gameObject);
        Destroy(gameObject, 0.3f);
    }
}