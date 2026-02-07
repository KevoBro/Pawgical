using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float lifetime;
    private float spawnTime;
    
    public void Initialize(float damage, float speed, float lifetime)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.spawnTime = Time.time;
    }
    
    void Update()
    {
        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;
        
        // Destroy after lifetime expires
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // Destroy projectile
            Destroy(gameObject);
        }
        
        // Destroy on hitting walls/ground
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}