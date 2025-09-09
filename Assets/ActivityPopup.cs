using UnityEngine;
using System.Collections;

public class ActivityPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public Vector2 nextPopupDelay = new Vector2(15f, 30f);

    private bool popupActive = false;

    private void Start()
    {
        popupPrefab.SetActive(false);
        StartCoroutine(PopupLoop());
    }

    private IEnumerator PopupLoop()
    {
        while (true)
        {
            // Wait random seconds before showing popup
            float delay = Random.Range(nextPopupDelay.x, nextPopupDelay.y);
            yield return new WaitForSeconds(delay);

            ShowPopup();

            // Wait until popup is closed by user
            yield return new WaitUntil(() => popupActive == false);
        }
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

        // Hide popup
        popupPrefab.SetActive(false);
        popupActive = false;
        SoundManager.Instance.PlaySFX(SFXType.Click);
        // TODO: Load your activity scene here
        // SceneManager.LoadScene("ActivityScene");
    }
}
