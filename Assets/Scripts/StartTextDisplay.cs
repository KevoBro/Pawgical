using UnityEngine;
using TMPro;
using System.Collections;

public class StartTextDisplay : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public string[] messages;
    public float displayDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 1f;
    public float delayBetweenMessages = 0.5f;
    
    void Start()
    {
        StartCoroutine(DisplayTextSequence());
    }
    
    IEnumerator DisplayTextSequence()
    {
        foreach (string message in messages)
        {
            // Set text and start invisible
            textDisplay.text = message;
            textDisplay.color = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, 0f);
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
            // Display
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Delay
            yield return new WaitForSeconds(delayBetweenMessages);
        }
        
        textDisplay.gameObject.SetActive(false);
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = textDisplay.color;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            textDisplay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }
    
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color color = textDisplay.color;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            textDisplay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }
}
