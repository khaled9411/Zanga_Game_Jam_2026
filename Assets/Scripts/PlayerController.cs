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

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called automatically by PlayerInput (Send Messages) from the "Move" action
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
    }

    // Called automatically by PlayerInput (Send Messages) from the "Interact" action
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        TryInteract();
    }

    private void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);
        if (hit == null) return;

        // WeaponPickup and RewindPickup currently handle their own E-press internally
        // via Input.GetKeyDown, so they don't need anything called on them here.
        // This hook exists for any other interactable your team adds later.
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        GameEvents.TriggerPlayerPositionChanged(rb.position);
    }

    public Vector2 GetFacingDirection() => lastMoveDir;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}