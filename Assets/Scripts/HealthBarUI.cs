using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage; // The colored bar
    [SerializeField] private TextMeshProUGUI healthText; // Optional: shows "75/100"
    
    [Header("Visual Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30%
    
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    
    void Start()
    {
        // Find PlayerHealth if not assigned
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("HealthBarUI: No PlayerHealth found!");
            return;
        }
        
        // Subscribe to health change events
        playerHealth.onHealthChanged.AddListener(UpdateHealthBar);
        
        // Initialize health bar
        UpdateHealthBar(playerHealth.GetHealthPercentage());
    }
    
    void UpdateHealthBar(float healthPercentage)
    {
        // Update slider value (0 to 1)
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
        }
        
        // Update color based on health
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
        }
        
        // Update text (optional)
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(playerHealth.currentHealth)}/{playerHealth.maxHealth}";
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}