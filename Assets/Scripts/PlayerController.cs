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

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        TryInteract();
    }

    private void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);
        if (hit == null) return;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        UpdateAnimator();

        GameEvents.TriggerPlayerPositionChanged(rb.position);
    }

    private void UpdateAnimator()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (animator == null) return;

        animator.SetBool(MovingParam, isMoving);

        // Feed the blend tree X as always-positive (so it always samples the Right pose),
        // and mirror visually via flipX when actually facing left.
        float blendX = Mathf.Abs(lastMoveDir.x);
        float blendY = lastMoveDir.y;

        animator.SetFloat(XParam, blendX);
        animator.SetFloat(YParam, blendY);

        if (Mathf.Abs(lastMoveDir.x) > 0.01f && visualRenderer != null)
        {
            visualRenderer.flipX = lastMoveDir.x < 0f;
        }
    }

    public Vector2 GetFacingDirection() => lastMoveDir;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}