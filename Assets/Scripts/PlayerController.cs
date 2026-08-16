using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Animator animator;

    private static readonly int MovingParam = Animator.StringToHash("Moving");
    private static readonly int XParam = Animator.StringToHash("X");
    private static readonly int YParam = Animator.StringToHash("Y");
    private static readonly int HurtParam = Animator.StringToHash("Hurt");
    private static readonly int DeathParam = Animator.StringToHash("Death");

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameEvents.OnRequestTakeDamage += HandleTookDamage;
        GameEvents.OnHeartsChanged += HandleHeartsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestTakeDamage -= HandleTookDamage;
        GameEvents.OnHeartsChanged -= HandleHeartsChanged;
    }


    private void HandleHeartsChanged(int hearts)
    {
        if (hearts <= 0 && !isDead)
        {
            HandlePlayerDeath();
        }
    }

    public void OnMove(InputValue value)
    {
        if (isDead) return;

        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
    }

    public void OnInteract(InputValue value)
    {
        if (isDead || !value.isPressed) return;
        TryInteract();
    }

    private void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);
        if (hit == null) return;
    }

    private void Update()
    {
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        GameEvents.TriggerPlayerPositionChanged(rb.position);
    }

    private void UpdateAnimator()
    {
        if (animator == null || isDead) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool(MovingParam, isMoving);

        float blendX = Mathf.Abs(lastMoveDir.x);
        float blendY = lastMoveDir.y;

        animator.SetFloat(XParam, blendX);
        animator.SetFloat(YParam, blendY);

        if (Mathf.Abs(lastMoveDir.x) > 0.01f && visualRenderer != null)
        {
            visualRenderer.flipX = lastMoveDir.x < 0f;
        }
    }

    private void HandleTookDamage(int amount)
    {
        if (isDead || animator == null) return;
        animator.SetTrigger(HurtParam);
        Debug.Log("Player hurt animation triggered");
    }

    private void HandlePlayerDeath()
    {
        if (isDead) return;
        isDead = true;
        moveInput = Vector2.zero;

        if (animator != null)
            animator.SetTrigger(DeathParam);

        Debug.Log("Player death animation triggered");
    }

    public Vector2 GetFacingDirection() => lastMoveDir;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}