using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Animator animator;

    [Header("Big Enemy Patrol")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float patrolPointReachedThreshold = 0.3f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Chase")]
    [SerializeField] private float stopChaseDistance = 0.6f;

    [Header("Contact Damage")]
    [SerializeField] private float damageCooldown = 1f;

    private static readonly int MovingParam = Animator.StringToHash("Moving");
    private static readonly int HurtParam = Animator.StringToHash("Hurt");
    private static readonly int DeathParam = Animator.StringToHash("Death");

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
        float dist = Vector2.Distance(rb.position, player.position);
        Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;

        // Always face the player, even when close enough to stop moving
        UpdateFacing(dirToPlayer);

        if (dist <= stopChaseDistance)
        {
            SetMoving(false);
            return;
        }

        rb.MovePosition(rb.position + dirToPlayer * data.moveSpeed * Time.deltaTime);
        SetMoving(true);
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f && visualRenderer != null)
        {
            visualRenderer.flipX = direction.x < 0f;
        }
    }

    private void Patrol()
    {
        Vector2 dir = (currentPatrolTarget - rb.position).normalized;
        MoveAndAnimate(dir, data.moveSpeed * 0.5f);

        if (Vector2.Distance(rb.position, currentPatrolTarget) <= patrolPointReachedThreshold)
        {
            PickNewPatrolTarget();
        }
    }

    private void MoveAndAnimate(Vector2 direction, float speed)
    {
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
        SetMoving(true);

        if (Mathf.Abs(direction.x) > 0.01f && visualRenderer != null)
        {
            visualRenderer.flipX = direction.x < 0f;
        }
    }

    private void SetMoving(bool isMoving)
    {
        if (animator != null)
            animator.SetBool(MovingParam, isMoving);
    }

    private void PickNewPatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolOrigin + randomOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

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

        if (HasParameter(HurtParam) && currentHits < data.maxHits)
        {
            animator.SetTrigger(HurtParam);
        }

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

        if (HasParameter(DeathParam))
        {
            animator.SetTrigger(DeathParam);
            // No fixed particle/destroy here — clip is non-looping and holds on last frame,
            // matching the Player death setup. Destroy after a generous delay so it doesn't vanish mid-animation.
            Destroy(gameObject, 1.5f);
        }
        else
        {
            // No death animation available on this enemy — fallback to instant particle + short delay
            if (deathParticlePrefab != null)
                Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject, 0.3f);
        }

        GameEvents.TriggerEnemyDied(gameObject);
    }

    private bool HasParameter(int paramHash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;

        foreach (var param in animator.parameters)
        {
            if (param.nameHash == paramHash) return true;
        }
        return false;
    }
}