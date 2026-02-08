using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Death Settings")]
    [SerializeField] private string startMenuSceneName = "StartMenu"; // Name of your start menu scene
    [SerializeField] private float delayBeforeSceneChange = 2f; // Delay in seconds before loading menu
    [SerializeField] private bool fadeToBlack = true; // Optional fade effect
    [SerializeField] private FadeScreen fadeScreen; // Reference to your FadeScreen
    
    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged; // Passes current health percentage (0-1)
    
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Try to find FadeScreen if not assigned
        if (fadeScreen == null && fadeToBlack)
        {
            fadeScreen = FindObjectOfType<FadeScreen>();
            if (fadeScreen == null)
            {
                Debug.LogWarning("FadeScreen not found! Fading will be disabled.");
                fadeToBlack = false;
            }
        }
    }

    
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Don't take damage if already dead
        
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
        if (isDead) return; // Can't heal if dead
        
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        onHealthChanged?.Invoke(currentHealth / maxHealth);
        
        Debug.Log($"Player healed {amount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        Debug.Log("Player died!");
        onDeath?.Invoke();
        
        // Start the death sequence
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // Optional: Disable player controls here
        // GetComponent<CharacterController>()?.enabled = false;
        
        // Wait for delay
        yield return new WaitForSeconds(delayBeforeSceneChange);
        
        // Optional fade to black before scene change
        if (fadeToBlack && fadeScreen != null)
        {
            fadeScreen.FadeOut(); // Fade from 0 to 1 (transparent to black)
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }
        
        // Load the start menu
        LoadStartMenu();
    }
    
    void LoadStartMenu()
    {
        Debug.Log($"Loading scene: {startMenuSceneName}");
        SceneManager.LoadScene(startMenuSceneName);
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