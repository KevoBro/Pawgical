using UnityEngine;

public class LightningProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    private float speed;
    private float lifetime;
    private float spawnTime;
    
    [Header("AOE Settings")]
    [SerializeField] private GameObject lightningAOEPrefab;
    
    public void Initialize(float damage, float speed, float lifetime)
    {
        // We don't use damage here since the AOE deals damage
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
        // Spawn AOE on any surface hit (enemies, ground, walls)
        if (other.CompareTag("Enemy") || other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            SpawnLightningAOE(other.ClosestPoint(transform.position));
            Destroy(gameObject);
        }
    }
    
    void SpawnLightningAOE(Vector3 impactPoint)
    {
        if (lightningAOEPrefab != null)
        {
            // Spawn at impact point
            Instantiate(lightningAOEPrefab, impactPoint, Quaternion.identity);
            Debug.Log("Lightning AOE spawned at: " + impactPoint);
        }
        else
        {
            Debug.LogError("Lightning AOE Prefab not assigned!");
        }
    }
}