using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{

    public Image fillImage; // Reference to the UI Image for the fill
    public float fillDuration = 2f; // Duration for the fill to complete

    private void Start()
    {
        StartCoroutine(FillImage());
    }

    // Coroutine to fill the image over time
    private IEnumerator FillImage()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            // Calculate the fill amount based on the elapsed time
            float fillAmount = elapsedTime / fillDuration;

            // Update the fill amount of the image
            fillImage.fillAmount = fillAmount;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the fill amount is set to 1 (fully filled)
        fillImage.fillAmount = 1f;
    }
}
