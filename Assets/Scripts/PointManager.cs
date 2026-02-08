using UnityEngine;
using UnityEngine.Events;

public class PointManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int currentScore = 0;
    
    [Header("Events")]
    public UnityEvent<int> onScoreChanged; // Passes current score
    public UnityEvent<int> onPointsAdded; // Passes points just added
    
    private static PointManager instance;
    
    void Awake()
    {
        // Singleton pattern - only one PointManager exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddPoints(int points)
    {
        currentScore += points;
        
        Debug.Log($"Points added: +{points}. Total score: {currentScore}");
        
        onPointsAdded?.Invoke(points);
        onScoreChanged?.Invoke(currentScore);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        onScoreChanged?.Invoke(currentScore);
    }
    
    public int GetScore()
    {
        return currentScore;
    }
    
    // Static method for easy access from anywhere
    public static PointManager Instance
    {
        get { return instance; }
    }
}