using UnityEngine;
using UnityEngine.AI;

public class EnemyZigZagNavMesh : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    
    [Header("Zigzag Settings")]
    public float zigzagFrequency = 2f; // How fast it zigzags
    public float zigzagAmplitude = 1.5f; // How wide the zigzag is
    
    private Vector3 targetOffset;
    private float zigzagTime;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        agent.speed = moveSpeed + Random.Range(-0.5f, 0.5f);
        agent.stoppingDistance = stoppingDistance;
        
        // Random starting point in the zigzag pattern
        zigzagTime = Random.Range(0f, 100f);
    }
    
    public float updateRate = 0.1f;
    private float nextUpdateTime;
    
    void Update()
    {
        if (Time.time >= nextUpdateTime && player != null && agent != null)
        {
            // Calculate zigzag offset
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Get perpendicular direction (for zigzag movement)
            Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
            
            // Calculate zigzag using sine wave
            zigzagTime += Time.deltaTime * zigzagFrequency;
            float zigzagOffset = Mathf.Sin(zigzagTime) * zigzagAmplitude;
            
            // Apply offset perpendicular to direction
            Vector3 zigzagPosition = player.position + (perpendicular * zigzagOffset);
            
            agent.SetDestination(zigzagPosition);
            nextUpdateTime = Time.time + updateRate;
        }
    }
}