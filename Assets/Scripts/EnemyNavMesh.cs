
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMesh : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    
 void Start()
{
    agent = GetComponent<NavMeshAgent>();
    player = GameObject.FindGameObjectWithTag("Player").transform;
    
    // Add some randomness to speed
    agent.speed = moveSpeed + Random.Range(-0.5f, 0.5f);
    agent.stoppingDistance = stoppingDistance;
}
    
public float updateRate = 0.1f; // Update every 0.1 seconds
private float nextUpdateTime;

void Update()
{
    if (Time.time >= nextUpdateTime && player != null && agent != null)
    {
        agent.SetDestination(player.position);
        nextUpdateTime = Time.time + updateRate;
    }
}
}