using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class LoadScene : MonoBehaviour
{
    public Image fadeImage;       // assign FadePanel image here in inspector
    public float fadeDuration = 1f;
    public TMP_Text textLoading;
    private void Start()
    {
        // Start with fade-in effect
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    public void LoadScen(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(FadeOut());

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Wait a frame so new scene loads UI
        yield return null;

        // Fade back in
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        textLoading.gameObject.SetActive
          (false);
        float t = fadeDuration;
        Color c = fadeImage.color;

        while (t > 0)
        {
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
      
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        textLoading.gameObject.SetActive
        (true);
    }
}