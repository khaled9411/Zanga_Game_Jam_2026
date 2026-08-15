using UnityEngine;

/// <summary>
/// Spawns a random starting weapon when GameEvents.OnGameStarted fires,
/// and a new random weapon whenever GameEvents.OnWeaponDepleted fires.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPickupPrefabs; // your 3 weapon prefabs
    [SerializeField] private float minSpawnDistance = 3f;
    [SerializeField] private float maxSpawnDistance = 8f;

    private void OnEnable()
    {
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnWeaponDepleted += HandleWeaponDepleted;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnWeaponDepleted -= HandleWeaponDepleted;
    }

    private void HandleGameStarted()
    {
        SpawnRandomWeapon();
    }

    private void HandleWeaponDepleted()
    {
        SpawnRandomWeapon();
    }

    private void SpawnRandomWeapon()
    {
        if (weaponPickupPrefabs == null || weaponPickupPrefabs.Length == 0) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        GameObject chosenPrefab = weaponPickupPrefabs[Random.Range(0, weaponPickupPrefabs.Length)];

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector2 spawnPos = (Vector2)playerObj.transform.position + randomDir * randomDist;

        Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
    }
}