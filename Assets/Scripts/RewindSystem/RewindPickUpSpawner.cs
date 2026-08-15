using UnityEngine;

/// <summary>
/// Listens for GameEvents.OnRewindAvailable and spawns a RewindPickup near the player.
/// Attach to an empty GameObject in the scene — needs a reference to the pickup prefab.
/// </summary>
public class RewindPickupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject rewindPickupPrefab;
    [SerializeField] private float minSpawnDistance = 1.5f;
    [SerializeField] private float maxSpawnDistance = 3f;

    private void OnEnable()
    {
        GameEvents.OnRewindAvailable += HandleRewindAvailable;
    }

    private void OnDisable()
    {
        GameEvents.OnRewindAvailable -= HandleRewindAvailable;
    }

    private void HandleRewindAvailable()
    {
        if (rewindPickupPrefab == null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector2 spawnPos = (Vector2)playerObj.transform.position + randomDir * randomDist;

        Instantiate(rewindPickupPrefab, spawnPos, Quaternion.identity);
    }
}