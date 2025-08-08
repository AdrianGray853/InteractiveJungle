using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConsentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject consentPanel;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;
    [SerializeField] private RectTransform rect;

    private UnityAction yesAction;
    private UnityAction noAction;

    public static ConsentManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        yesBtn.onClick.AddListener(OnClickYes);
        noBtn.onClick.AddListener(OnClickNo);
    }

    /// <summary>
    /// Shows the consent panel at a specific position with yes/no callbacks.
    /// </summary>
    public void ShowConsent(Vector3 spawnPos, UnityAction onYes, UnityAction onNo)
    {
        //rect.position = spawnPos;
        yesAction = onYes;
        noAction = onNo;
        consentPanel.SetActive(true);
    }

    /// <summary>
    /// Called when user clicks "Yes".
    /// </summary>
    public void OnClickYes()
    {
        Debug.Log("ClickYes");
        yesAction?.Invoke();
        CloseConsent();

    }

    /// <summary>
    /// Called when user clicks "No".
    /// </summary>
    public void OnClickNo()
    {
        noAction?.Invoke();
        CloseConsent();

    }

    /// <summary>
    /// Hides the panel and clears the actions.
    /// </summary>
    public void CloseConsent()
    {
        consentPanel.SetActive(false);
        yesAction = null;
        noAction = null;
    }
}
