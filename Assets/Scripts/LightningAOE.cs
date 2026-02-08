using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningAOE : MonoBehaviour
{
    [Header("AOE Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float damagePerTick = 5f;
    [SerializeField] private float tickInterval = 0.5f; // Damage every 0.5 seconds
    
    [Header("Stun Settings")]
    [SerializeField] private bool stunEnemies = true;
    [SerializeField] private float stunDuration = 0.3f; // Brief pause per tick
    
    [Header("Visual Effects")]
    [SerializeField] private Color lightningColor = new Color(0.5f, 0.5f, 1f, 0.3f);

    [Header("Audio")]
    [SerializeField] private AudioClip lightningSound;
    private AudioSource audioSource;
    
    private float startTime;
    private List<GameObject> enemiesInRange = new List<GameObject>();
    private HashSet<GameObject> currentlyStunnedEnemies = new HashSet<GameObject>();
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (lightningSound != null)
        {
            audioSource.PlayOneShot(lightningSound);
        }
        startTime = Time.time;
        StartCoroutine(DamageOverTime());
        
        // Auto-destroy after duration
        Destroy(gameObject, duration);
    }
    
    void Update()
    {
        // Continuously check for enemies in range
        UpdateEnemiesInRange();
    }
    
    void UpdateEnemiesInRange()
    {
        // Clear the list
        enemiesInRange.Clear();
        
        // Find all enemies in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                enemiesInRange.Add(col.gameObject);
            }
        }
    }
    
    IEnumerator DamageOverTime()
    {
        while (Time.time - startTime < duration)
        {
            // Deal damage to all enemies in range
            foreach (GameObject enemy in enemiesInRange)
            {
                if (enemy != null) // Check if enemy still exists
                {
                    DamageEnemy(enemy);
                    
                    if (stunEnemies && !currentlyStunnedEnemies.Contains(enemy))
                    {
                        StartCoroutine(StunEnemy(enemy));
                    }
                }
            }
            
            yield return new WaitForSeconds(tickInterval);
        }
    }
    
    void DamageEnemy(GameObject enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damagePerTick);
            Debug.Log($"Lightning damaged {enemy.name} for {damagePerTick}");
        }
    }
    
    IEnumerator StunEnemy(GameObject enemy)
    {
        if (enemy == null) yield break;
        
        currentlyStunnedEnemies.Add(enemy);
        
        // Disable enemy movement
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
        }
        
        // Wait for stun duration
        yield return new WaitForSeconds(stunDuration);
        
        // Re-enable movement
        if (agent != null && enemy != null)
        {
            agent.isStopped = false;
        }
        
        currentlyStunnedEnemies.Remove(enemy);
    }
    
    // Visualize the AOE radius in editor
    void OnDrawGizmos()
    {
        Gizmos.color = lightningColor;
        Gizmos.DrawSphere(transform.position, radius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}