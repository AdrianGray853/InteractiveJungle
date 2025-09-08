using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActivityPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public float firstDelay = 5f;
    public Vector2 nextPopupDelay = new Vector2(15f, 30f);

    private bool popupActive = false;

    private void Start()
    {
        popupPrefab.SetActive(false);

        // Check if popup was just played before scene reload
        if (PlayerPrefs.GetInt("PopupJustPlayed", 0) == 1)
        {
            // Reset flag
            PlayerPrefs.SetInt("PopupJustPlayed", 0);

            // Skip firstDelay, use normal random interval
            float nextDelay = Random.Range(nextPopupDelay.x, nextPopupDelay.y);
            StartCoroutine(ShowPopupAfterDelay(nextDelay));
        }
        else
        {
            // First popup after standard firstDelay
            StartCoroutine(ShowPopupAfterDelay(firstDelay));
        }
    }

    private IEnumerator ShowPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowPopup();
    }

    private void ShowPopup()
    {
        if (popupActive) return;
        SoundManager.Instance.PlayVoiceOver(VoiceOverType.LetsPlayAndUnlockNewThings);
        popupPrefab.SetActive(true);
        popupActive = true;


    }

    public void OnPopupClicked()
    {
        Debug.Log("Popup clicked → Load random activity here!");

        // Mark popup as recently played
        PlayerPrefs.SetInt("PopupJustPlayed", 1);

        // Hide popup
        popupPrefab.SetActive(false);
        popupActive = false;

        // TODO: Load your activity scene here
        // SceneManager.LoadScene("ActivityScene");
    }
}
