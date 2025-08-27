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
    [HideInInspector]
    public bool panelOpen;
    public void OpenPanel(BtnPanel btn)
    {
        if (GameManager.instance.currentDrag != null)
        {
            Destroy(GameManager.instance.currentDrag);
            return;
        }
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
            panelOpen = false;
            prePanel = null;
            return;
        }

        prePanel = btn;
        panelOpen = true;
        foreach (var tool in tools)
        {
            tool.gameObject.SetActive(false);
        }

        prePanel.OpenPanel();
        panelAnim.Play("OpenPanel");
        canAnim.Play("OpenPanel");



    }
    public void ClosePanel()
    {
        panelOpen = false;

        prePanel.ClosePanel();
        panelAnim.Play("ClosePanel");
        canAnim.Play("ClosePanel");
        foreach (var item in tools)
        {
            item.SetActive(true);
        }
        prePanel = null;
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
                    ClosePanel();
                }
            }
        }
    }

    public void OnClickPuzzelGame()
    {
        GameManager.instance.UnlockNextBackground();
    }
    public void OnClickTracingGame()
    {
        GameManager.instance.UnlockNextDecoration();
    }
    public void OnClickColoringGame()
    {
        GameManager.instance.UnlockNextAnimal();
    }


}