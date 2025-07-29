using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Blink : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.0f; // Adjust the duration of the fade effect
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField]private Color originalColor;

    private void OnEnable()
    {
        spriteRenderer.color = new Color32(255,255,255,0);
        StartCoroutine(FadeOutIn());
    }
    
    private IEnumerator FadeOutIn()
    {
        float startTime = Time.time;
        
        // // Fade out
        // while (Time.time - startTime < fadeDuration)
        // {
        //     float t = (Time.time - startTime) / fadeDuration;
        //     spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0f, t));
        //     yield return null;
        // }
        //
        // // Ensure the sprite is completely transparent before fading it back in
        // spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        //
        // // Wait for a short delay before fading back in (you can adjust this duration)
        // yield return new WaitForSeconds(1.0f);

        // Fade in
        startTime = Time.time;
        while (Time.time - startTime < fadeDuration)
        {
            float t = (Time.time - startTime) / fadeDuration;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, originalColor.a, t));
            yield return null;
        }

        // Ensure the sprite is fully visible at the end
        spriteRenderer.color = originalColor;
        
        yield return new WaitForSeconds(fadeDuration);
        
        startTime = Time.time;
        while (Time.time - startTime < fadeDuration)
        {
            var t = (Time.time - startTime) / fadeDuration;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalColor.a, 0f, t));
            yield return null;
        }
        
        // Ensure the sprite is completely transparent
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
