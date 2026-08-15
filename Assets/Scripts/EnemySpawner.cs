using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private float initialGracePeriod = 15f; // seconds before any enemy spawns

    [Header("Prefabs")]
    [SerializeField] private GameObject miniEnemyPrefab;
    [SerializeField] private GameObject bigEnemyPrefab;

    [Header("Mini Enemy Rules")]
    [SerializeField] private int minMiniEnemies = 10;
    [SerializeField] private int maxMiniEnemies = 20;
    [SerializeField] private float spawnIntervalSeconds = 4f;
    [SerializeField] private float spawnRadius = 12f;

    [Header("Big Enemy Rules")]
    [SerializeField] private int bigEnemyStartDream = 3; // per doc: max 1 active starting only Dream 3/4



    private List<GameObject> activeMiniEnemies = new List<GameObject>();
    private GameObject activeBigEnemy;
    private int currentDream = 1;
    private bool gameRunning = false;

    private void OnEnable()
    {
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnDreamChanged += HandleDreamChanged;
        GameEvents.OnEnemyDied += HandleEnemyDied;
        GameEvents.OnGameWon += HandleGameEnded;
        GameEvents.OnGameLost += HandleGameEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnDreamChanged -= HandleDreamChanged;
        GameEvents.OnEnemyDied -= HandleEnemyDied;
        GameEvents.OnGameWon -= HandleGameEnded;
        GameEvents.OnGameLost -= HandleGameEnded;
    }

    private void HandleGameStarted()
    {
        gameRunning = true;
        currentDream = 1;
        activeMiniEnemies.Clear();
        activeBigEnemy = null;

       
        StopAllCoroutines();
        StartCoroutine(StaggeredMiniSpawnLoop());
    }

    private void HandleGameEnded()
    {
        gameRunning = false;
        StopAllCoroutines();
       
    }

    private void HandleDreamChanged(int dreamIndex)
    {
        currentDream = dreamIndex;
        Debug.Log($"[EnemySpawner] Dream changed to {dreamIndex}");

        if (dreamIndex >= bigEnemyStartDream && activeBigEnemy == null)
        {
            SpawnBigEnemy();
        }
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        activeMiniEnemies.RemoveAll(e => e == null);
        if (enemy == activeBigEnemy)
        {
            activeBigEnemy = null;
            Debug.Log("[EnemySpawner] BigEnemy Died");
        }
    }

    private IEnumerator StaggeredMiniSpawnLoop()
    {
        
        yield return new WaitForSeconds(initialGracePeriod);

        while (gameRunning && activeMiniEnemies.Count < minMiniEnemies)
        {
            SpawnMiniEnemy();
            yield return new WaitForSeconds(spawnIntervalSeconds);
        }

        while (gameRunning)
        {
            activeMiniEnemies.RemoveAll(e => e == null);

            if (activeMiniEnemies.Count < maxMiniEnemies)
            {
                SpawnMiniEnemy();
            }

            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private void SpawnMiniEnemy()
    {
        if (miniEnemyPrefab == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(miniEnemyPrefab, spawnPos, Quaternion.identity);
        activeMiniEnemies.Add(enemy);

        GameEvents.TriggerEnemySpawned(enemy);
      
    }

    private void SpawnBigEnemy()
    {
        if (bigEnemyPrefab == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        activeBigEnemy = Instantiate(bigEnemyPrefab, spawnPos, Quaternion.identity);

        GameEvents.TriggerEnemySpawned(activeBigEnemy);
        Debug.Log($"[EnemySpawner] Spawned BIG enemy at {spawnPos} for dream {currentDream}");
    }

    private Vector2 GetRandomSpawnPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2 origin = playerObj != null ? (Vector2)playerObj.transform.position : Vector2.zero;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return origin + randomDir * spawnRadius;
    }
}