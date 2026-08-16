using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Mini Enemy Prefabs (index 0 = Dream 1, ... index 3 = Dream 4)")]
    [SerializeField] private GameObject[] miniEnemyPrefabsByDream = new GameObject[4];

    [Header("Big Enemy Prefabs (index 0 = Dream 1, ... index 3 = Dream 4)")]
    [SerializeField] private GameObject[] bigEnemyPrefabsByDream = new GameObject[4];

    [Header("Mini Enemy Rules")]
    [SerializeField] private int minMiniEnemies = 10;
    [SerializeField] private int maxMiniEnemies = 20;
    [SerializeField] private float spawnIntervalSeconds = 4f;
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float initialGracePeriod = 15f;

    [Header("Big Enemy Rules")]
    [SerializeField] private int bigEnemyStartDream = 3;

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
        int previousDream = currentDream;
        currentDream = dreamIndex;
      
        SwapAllMiniEnemiesToCurrentDream();
        SwapBigEnemyToCurrentDream();

        if (dreamIndex >= bigEnemyStartDream && activeBigEnemy == null)
        {
            SpawnBigEnemy();
        }
    }

    private void SwapAllMiniEnemiesToCurrentDream()
    {
        List<GameObject> oldEnemies = new List<GameObject>(activeMiniEnemies);
        activeMiniEnemies.Clear();

        foreach (var old in oldEnemies)
        {
            if (old == null) continue;

            Vector2 pos = old.transform.position;
            int carriedHits = 0;
            EnemyAI oldAI = old.GetComponent<EnemyAI>();
            if (oldAI != null) carriedHits = oldAI.CurrentHits;

            Destroy(old);

            GameObject newEnemy = SpawnMiniEnemyAt(pos, carriedHits);
            if (newEnemy != null) activeMiniEnemies.Add(newEnemy);
        }
    }

    private void SwapBigEnemyToCurrentDream()
    {
        if (activeBigEnemy == null) return;

        Vector2 pos = activeBigEnemy.transform.position;
        int carriedHits = 0;
        EnemyAI oldAI = activeBigEnemy.GetComponent<EnemyAI>();
        if (oldAI != null) carriedHits = oldAI.CurrentHits;

        Destroy(activeBigEnemy);
        activeBigEnemy = null;

        GameObject prefab = GetPrefabForDream(bigEnemyPrefabsByDream, currentDream);
        if (prefab == null) return;

        activeBigEnemy = Instantiate(prefab, pos, Quaternion.identity);
        EnemyAI newAI = activeBigEnemy.GetComponent<EnemyAI>();
        if (newAI != null) newAI.CarriedOverHits = carriedHits;

        GameEvents.TriggerEnemySpawned(activeBigEnemy);
    
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        activeMiniEnemies.RemoveAll(e => e == null);
        if (enemy == activeBigEnemy)
        {
            activeBigEnemy = null;
           
        }
    }

    private IEnumerator StaggeredMiniSpawnLoop()
    {
       
        yield return new WaitForSeconds(initialGracePeriod);

        while (gameRunning && activeMiniEnemies.Count < minMiniEnemies)
        {
            SpawnMiniEnemyRandomPos();
            yield return new WaitForSeconds(spawnIntervalSeconds);
        }

        while (gameRunning)
        {
            activeMiniEnemies.RemoveAll(e => e == null);

            if (activeMiniEnemies.Count < maxMiniEnemies)
            {
                SpawnMiniEnemyRandomPos();
            }

            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private void SpawnMiniEnemyRandomPos()
    {
        GameObject enemy = SpawnMiniEnemyAt(GetRandomSpawnPosition(), 0);
        if (enemy != null) activeMiniEnemies.Add(enemy);
    }

    private GameObject SpawnMiniEnemyAt(Vector2 pos, int carriedHits)
    {
        GameObject prefab = GetPrefabForDream(miniEnemyPrefabsByDream, currentDream);
        if (prefab == null) return null;

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.CarriedOverHits = carriedHits;

        GameEvents.TriggerEnemySpawned(enemy);
     
        return enemy;
    }

    private void SpawnBigEnemy()
    {
        GameObject prefab = GetPrefabForDream(bigEnemyPrefabsByDream, currentDream);
        if (prefab == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        activeBigEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        GameEvents.TriggerEnemySpawned(activeBigEnemy);
        
    }

    private GameObject GetPrefabForDream(GameObject[] prefabArray, int dreamIndex)
    {
        int i = dreamIndex - 1; // dreamIndex is 1-4, array is 0-3
        if (prefabArray == null || i < 0 || i >= prefabArray.Length || prefabArray[i] == null)
        {
            return null;
        }
        return prefabArray[i];
    }

    private Vector2 GetRandomSpawnPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2 origin = playerObj != null ? (Vector2)playerObj.transform.position : Vector2.zero;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return origin + randomDir * spawnRadius;
    }
}