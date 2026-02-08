using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private int maxEnemiesAlive = 10;
    
    [Header("Size Randomization")]
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.3f;
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnRadius = true;
    
    private int currentEnemyCount = 0;
    private bool isSpawning = false;
    
    void Start()
    {
        if (enemyTypes.Count == 0)
        {
            Debug.LogError($"EnemySpawner '{gameObject.name}' has no enemy types assigned!");
            return;
        }
        
        StartCoroutine(SpawnLoop());
    }
    
    IEnumerator SpawnLoop()
    {
        isSpawning = true;
        
        while (isSpawning)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
            
            if (currentEnemyCount < maxEnemiesAlive)
            {
                SpawnRandomEnemy();
            }
            else
            {
                Debug.Log($"Spawner '{gameObject.name}' at max capacity ({maxEnemiesAlive} enemies)");
            }
        }
    }
    
    void SpawnRandomEnemy()
    {
        EnemySpawnData spawnData = GetWeightedRandomEnemy();
        
        if (spawnData == null || spawnData.enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not assigned in spawner '{gameObject.name}'");
            return;
        }
        
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0;
        Vector3 spawnPosition = transform.position + randomOffset;
        
        GameObject enemy = Instantiate(spawnData.enemyPrefab, spawnPosition, Quaternion.identity);
        
        float randomScale = Random.Range(minScale, maxScale);
        enemy.transform.localScale = Vector3.one * randomScale;
        
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            float scaledHealth = spawnData.baseHealth * randomScale;
            enemyHealth.SetMaxHealth(scaledHealth);
            
            // NEW: Set point value
            enemyHealth.SetPointValue(spawnData.pointValue);
            
            Debug.Log($"Spawned {spawnData.enemyName} - Scale: {randomScale:F2}, Health: {scaledHealth:F0}, Points: {spawnData.pointValue}");
        }
        
        currentEnemyCount++;
        
        EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.spawner = this;
    }
    
    EnemySpawnData GetWeightedRandomEnemy()
    {
        float totalWeight = 0f;
        foreach (EnemySpawnData enemy in enemyTypes)
        {
            totalWeight += enemy.spawnWeight;
        }
        
        if (totalWeight <= 0)
        {
            Debug.LogWarning("Total spawn weight is 0. Using equal probability.");
            return enemyTypes[Random.Range(0, enemyTypes.Count)];
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        
        float cumulativeWeight = 0f;
        foreach (EnemySpawnData enemy in enemyTypes)
        {
            cumulativeWeight += enemy.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                return enemy;
            }
        }
        
        return enemyTypes[enemyTypes.Count - 1];
    }
    
    public void OnEnemyDied()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount);
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnLoop());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    void OnDrawGizmos()
    {
        if (showSpawnRadius)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}

public class EnemyDeathTracker : MonoBehaviour
{
    public EnemySpawner spawner;
    
    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyDied();
        }
    }
}

[System.Serializable]
public class EnemySpawnData
{
    public string enemyName;
    public GameObject enemyPrefab;
    public float baseHealth = 50f;
    public int pointValue = 10; // NEW: Points awarded when killed
    
    [Header("Spawn Rate")]
    [Range(0f, 100f)]
    public float spawnWeight = 50f;
}