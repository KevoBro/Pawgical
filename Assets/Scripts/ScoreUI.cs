using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI pointsPopupText; // Optional: shows "+50" when you earn points
    
    [Header("References")]
    [SerializeField] private PointManager pointManager;
    
    [Header("Popup Settings")]
    [SerializeField] private bool enablePopup = true;
    [SerializeField] private float popupDuration = 1f;
    
    private float popupTimer = 0f;
    
    void Start()
    {
        // Find PointManager if not assigned
        if (pointManager == null)
        {
            pointManager = FindObjectOfType<PointManager>();
        }
        
        if (pointManager == null)
        {
            Debug.LogError("ScoreUI: No PointManager found!");
            return;
        }
        
        // Subscribe to score events
        pointManager.onScoreChanged.AddListener(UpdateScoreDisplay);
        pointManager.onPointsAdded.AddListener(ShowPointsPopup);
        
        // Initialize display
        UpdateScoreDisplay(pointManager.GetScore());
        
        // Hide popup initially
        if (pointsPopupText != null)
        {
            pointsPopupText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // Handle popup fade out
        if (popupTimer > 0)
        {
            popupTimer -= Time.deltaTime;
            
            if (popupTimer <= 0 && pointsPopupText != null)
            {
                pointsPopupText.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    void ShowPointsPopup(int pointsAdded)
    {
        if (!enablePopup || pointsPopupText == null) return;
        
        pointsPopupText.text = $"+{pointsAdded}";
        pointsPopupText.gameObject.SetActive(true);
        popupTimer = popupDuration;
    }
    
    void OnDestroy()
    {
        if (pointManager != null)
        {
            pointManager.onScoreChanged.RemoveListener(UpdateScoreDisplay);
            pointManager.onPointsAdded.RemoveListener(ShowPointsPopup);
        }
    }
}