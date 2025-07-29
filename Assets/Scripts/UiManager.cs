using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public Animator panelAnim;
    public Animator canAnim;
    public GameObject[] tools;
    public Transform camTransform;
    private BtnPanel prePanel;
    public bool isPanelOpen;
    public void OpenPanel(BtnPanel btn)
    {
        if (prePanel != null)
            prePanel.ClosePanel();
        if (prePanel == btn)
        {
            panelAnim.Play("ClosePanel");
            canAnim.Play("ClosePanel");
            foreach (var tool in tools)
            {
                tool.gameObject.SetActive(true);
            }
            isPanelOpen = false;
            prePanel = null;
            return;
        }

        prePanel = btn;
        isPanelOpen = true;

        foreach (var tool in tools)
        {
            tool.gameObject.SetActive(false);
        }

        prePanel.OpenPanel();
        panelAnim.Play("OpenPanel");
        canAnim.Play("OpenPanel");



    }
    private void Update()
    {
        // Check for left mouse click (or first finger tap on mobile)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if pointer is NOT over a UI element
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (prePanel != null)
                {
                    ClosePanel(); // 👈 Call your panel close method
                }
            }
        }
    }
    public void ClosePanel()
    {
        prePanel.ClosePanel();
        panelAnim.Play("ClosePanel");
        canAnim.Play("ClosePanel");
        foreach (var item in tools)
        {
            item.SetActive(true);
        }
        prePanel = null;
    }
}