using UnityEngine;
using UnityEngine.UI;

public class BtnPanel : MonoBehaviour
{
    private Animator animator;
    private bool openedPanel;
    public GameObject panel;
    private void Awake()
    {
        animator = GetComponent<Animator>();

    }
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            UiManager.Instance.OpenPanel(this);

          
        });
    }

    public void ClosePanel()
    {
        animator.Play("ClosePanel");
        openedPanel = false;
        panel.SetActive(false);
    }
    public void OpenPanel()
    {
        animator.Play("OpenPanel");
        openedPanel = true;
        panel.SetActive(true);

    }
}
