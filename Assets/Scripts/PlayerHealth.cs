using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged; // Passes current health percentage (0-1)
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        // Notify listeners of health change (useful for UI updates)
        onHealthChanged?.Invoke(currentHealth / maxHealth);
        
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        onHealthChanged?.Invoke(currentHealth / maxHealth);
        
        Debug.Log($"Player healed {amount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        onDeath?.Invoke();
        
        // Add your death logic here (reload scene, show game over screen, etc.)
        // For now, just disable the player
        // gameObject.SetActive(false);
    }
    
    // Useful getter methods
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}