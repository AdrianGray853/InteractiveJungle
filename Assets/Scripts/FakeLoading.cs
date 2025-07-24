using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FakeLoading : MonoBehaviour
{
    public Image progressBar;
    public Text loadingText;
    public float loadingTime = 3f; // Time in seconds to "fake load"
    public string nextSceneName = "GameScene"; // Change to your next scene's name

    private void Start()
    {
        StartCoroutine(LoadFakeProgress());
    }

    private IEnumerator LoadFakeProgress()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadingTime;
            progressBar.fillAmount = progress;

            UpdateLoadingText(progress);
            yield return null; // Wait until next frame
        }

        progressBar.fillAmount = 1f;
        loadingText.text = "Done!";

        yield return new WaitForSeconds(1f); // Short delay before scene switch

        SceneManager.LoadScene(nextSceneName);
    }

    private void UpdateLoadingText(float progress)
    {
        if (progress < 0.3f)
            loadingText.text = "Loading...";
        else if (progress < 0.6f)
            loadingText.text = "Almost There...";
        else if (progress < 0.9f)
            loadingText.text = "Finalizing...";
    }
}
