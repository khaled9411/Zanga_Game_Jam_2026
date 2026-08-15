using UnityEngine;

/// <summary>
/// A floating weapon in the world. Player presses E while in range to pick it up.
/// Destroys itself after pickup.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private float interactRange = 1.5f;

    private Transform player;
    private WeaponSystem playerWeaponSystem;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerWeaponSystem = playerObj.GetComponent<WeaponSystem>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            playerWeaponSystem?.EquipWeapon(weaponData);
            Destroy(gameObject);
        }
    }
}