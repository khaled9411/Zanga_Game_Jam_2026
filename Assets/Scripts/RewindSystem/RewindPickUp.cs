using UnityEngine;

/// <summary>
/// Spawned when GameEvents.OnRewindAvailable fires. Player presses E while in range
/// to restore a heart. Destroys itself after use.
/// </summary>
public class RewindPickup : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            GameEvents.TriggerRequestRestoreHeart();
            Destroy(gameObject);
        }
    }
}