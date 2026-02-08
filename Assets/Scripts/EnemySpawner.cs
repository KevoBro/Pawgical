using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private int maxEnemiesAlive = 10; // Prevent too many enemies
    
    [Header("Size Randomization")]
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.3f;
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 1f; // Random offset from spawner position
    
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
            // Wait random interval
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Only spawn if under the limit
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
        // Pick random enemy type
        EnemySpawnData spawnData = enemyTypes[Random.Range(0, enemyTypes.Count)];
        
        if (spawnData.enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab not assigned in spawner '{gameObject.name}'");
            return;
        }
        
        // Calculate spawn position with random offset
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0; // Keep enemies on ground level
        Vector3 spawnPosition = transform.position + randomOffset;
        
        // Spawn enemy
        GameObject enemy = Instantiate(spawnData.enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Randomize scale
        float randomScale = Random.Range(minScale, maxScale);
        enemy.transform.localScale = Vector3.one * randomScale;
        
        // Set randomized health (base health * scale)
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            float scaledHealth = spawnData.baseHealth * randomScale;
            enemyHealth.SetMaxHealth(scaledHealth);
            
            Debug.Log($"Spawned {spawnData.enemyName} - Scale: {randomScale:F2}, Health: {scaledHealth:F0}");
        }
        
        // Track enemy count
        currentEnemyCount++;
        
        // Subscribe to enemy death to update count
        EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.spawner = this;
    }
    
    public void OnEnemyDied()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount); // Prevent negative
    }
    
    // Public methods to control spawner
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
    
    // Visualize spawn radius in editor
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

// Helper class to track when enemies die
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
    public string enemyName; // For identification in inspector
    public GameObject enemyPrefab;
    public float baseHealth = 50f; // Base health before scaling
}