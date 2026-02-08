using UnityEngine;
using UnityEngine.AI;

public class EnemyBerserkerNavMesh : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private EnemyHealth enemyHealth;
    
    [Header("Movement Settings")]
    public float baseSpeed = 1.5f;
    public float maxSpeed = 3f; // Speed at 0% health
    public float stoppingDistance = 1.5f;
    
    [Header("Speed Scaling")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 2);
    // X-axis: health percentage (0 = dead, 1 = full health)
    // Y-axis: speed multiplier (1 = base speed, 2 = double speed)
    
    public float updateRate = 0.1f;
    private float nextUpdateTime;
    private float initialSpeed;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        
        if (enemyHealth == null)
        {
            Debug.LogError($"{gameObject.name} needs an EnemyHealth component for berserker behavior!");
        }
        
        // Add some randomness to base speed
        initialSpeed = baseSpeed + Random.Range(-0.5f, 0.5f);
        agent.speed = initialSpeed;
        agent.stoppingDistance = stoppingDistance;
    }
    
    void Update()
    {
        // Update pathfinding
        if (Time.time >= nextUpdateTime && player != null && agent != null)
        {
            agent.SetDestination(player.position);
            nextUpdateTime = Time.time + updateRate;
        }
        
        // Update speed based on health
        UpdateSpeedBasedOnHealth();
    }
    
    void UpdateSpeedBasedOnHealth()
    {
        if (enemyHealth == null || agent == null) return;
        
        // Get current health percentage (1.0 = full health, 0.0 = dead)
        float healthPercentage = enemyHealth.GetHealthPercentage();
        
        // Invert it so lower health = higher value
        float invertedHealth = 1f - healthPercentage;
        
        // Calculate speed multiplier using curve
        float speedMultiplier = speedCurve.Evaluate(healthPercentage);
        
        // Or use simple linear interpolation (commented alternative):
        // float speedMultiplier = Mathf.Lerp(maxSpeed / baseSpeed, 1f, healthPercentage);
        
        // Apply new speed
        float newSpeed = initialSpeed * speedMultiplier;
        newSpeed = Mathf.Clamp(newSpeed, initialSpeed, maxSpeed);
        agent.speed = newSpeed;
    }
}