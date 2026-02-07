using UnityEngine;
using UnityEngine.AI;

public class EnemyHoppingNavMeshSimple : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    
    [Header("Hopping Settings")]
    public float minHopHeight = 0.3f;
    public float maxHopHeight = 0.7f;
    public float hopDuration = 0.3f;
    public float minHopInterval = 0.5f;
    public float maxHopInterval = 2f;
    
    private float nextHopTime;
    private bool isHopping = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        agent.speed = moveSpeed + Random.Range(-0.1f, 0.1f);
        agent.stoppingDistance = stoppingDistance;
        
        nextHopTime = Time.time + Random.Range(minHopInterval, maxHopInterval);
    }
    
    public float updateRate = 0.1f;
    private float nextUpdateTime;
    
    void Update()
    {
        if (Time.time >= nextUpdateTime && player != null && agent != null)
        {
            agent.SetDestination(player.position);
            nextUpdateTime = Time.time + updateRate;
        }
        
        if (Time.time >= nextHopTime && !isHopping)
        {
            StartCoroutine(PerformHop());
            nextHopTime = Time.time + Random.Range(minHopInterval, maxHopInterval);
        }
    }
    
    System.Collections.IEnumerator PerformHop()
    {
        isHopping = true;
        float elapsedTime = 0f;
        
        // Randomize hop height for this hop
        float currentHopHeight = Random.Range(minHopHeight, maxHopHeight);
        
        while (elapsedTime < hopDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / hopDuration;
            
            // Create a smooth arc (parabola)
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * currentHopHeight;
            
            Vector3 newPos = agent.nextPosition;
            newPos.y += height;
            transform.position = newPos;
            
            yield return null;
        }
        
        isHopping = false;
    }
}