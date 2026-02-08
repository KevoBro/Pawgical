using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;
    
    [Header("Points")]
    public int pointValue = 10; // Points awarded when killed
    
    [Header("Damage Visual Feedback")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color damageColor = Color.red;
    
    [Header("Death Effects")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float effectDuration = 2f;
    [SerializeField] private bool fadeOutOnDeath = true;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private Renderer enemyRenderer;
    private Color originalColor;
    private Material enemyMaterial;
    private bool isFlashing = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        enemyRenderer = GetComponent<Renderer>();
        
        if (enemyRenderer != null)
        {
            enemyMaterial = enemyRenderer.material;
            originalColor = enemyMaterial.color;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no Renderer component!");
        }
    }
    
    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }
    
    // NEW: Allow spawner to set point value
    public void SetPointValue(int points)
    {
        pointValue = points;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (!isFlashing)
        {
            StartCoroutine(FlashRed());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator FlashRed()
    {
        if (enemyMaterial == null) yield break;
        
        isFlashing = true;
        
        enemyMaterial.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        enemyMaterial.color = originalColor;
        
        isFlashing = false;
    }
    
    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Award points to player
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoints(pointValue);
        }
        
        // Spawn smoke effect
        if (deathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, effectDuration);
        }
        
        // Optional fade out before destroying
        if (fadeOutOnDeath && enemyMaterial != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    IEnumerator FadeOutAndDestroy()
    {
        // Disable enemy AI/movement
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        // Disable colliders so it doesn't block
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Fade out
        float elapsed = 0f;
        Color startColor = enemyMaterial.color;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            
            Color newColor = startColor;
            newColor.a = alpha;
            enemyMaterial.color = newColor;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}