using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 10f;
    public float attackInterval = 1f; // Time between attacks
    public float attackRange = 2f; // Distance to start attacking
    
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttackTime;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogError("Player doesn't have PlayerHealth component!");
            }
        }
        else
        {
            Debug.LogError("No GameObject with 'Player' tag found!");
        }
        
        nextAttackTime = Time.time;
    }
    
    void Update()
    {
        if (player == null || playerHealth == null) return;
        
        // Check if player is in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackInterval;
        }
    }
    
    void Attack()
    {
        if (playerHealth != null && playerHealth.IsAlive())
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
            
            // Optional: Add attack animation, sound, or visual effect here
        }
    }
    
    // Optional: Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}