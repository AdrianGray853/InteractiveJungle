using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.Touch
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class ColorPicker : MonoBehaviour
    {
    	public GameObject RefColorObject;
    	public GameObject RefPatternObject;
    	public GameObject RefStampObject;
    	public GameObject RainbowColor;
    	public Color[] Colors;
    	public Sprite[] Patterns;
    	public Sprite[] Stamps;
    	// See GameManagerTouch.eTool for the order!
    	public Transform[] Tools;
    	public Transform Highlighter;

    	public Transform PatternPanel;
    	public Transform StampPanel;

    	Vector3 PatternPanelOriginalPosition;
    	Vector3 StampPanelOriginalPosition;
    	bool isStampPanelShowing = true;

    	List<Transform> colorSelections = new List<Transform>();
    	List<Transform> patternSelections = new List<Transform>();
    	List<Transform> stampSelections = new List<Transform>();
    	int colorSelectionIdx = 0;
    	int patternSelectionIdx = 0;
    	int stampSelectionIdx = 0;
    	GameManagerTouch.eTool toolSelection = GameManagerTouch.eTool.Fill;
    	Tween HighlighterTween = null;

    	Tween ToolSwapTween = null;

    	float StampsReminderCountdown = -1.0f;

    	void Start()
    	{
    		if (PatternPanel != null)
    			PatternPanelOriginalPosition = PatternPanel.position;
    		if (StampPanel != null)
    		{
    			StampPanelOriginalPosition = StampPanel.position;
    			StampPanel.position += Vector3.right * 500.0f;
    		}

    		SelectMainTool(true);
    		if (GameDataTouch.Instance.GameType != GameDataTouch.eGameType.PatternAndStamps)
    			InitColors();
    		else
    			InitPatternAndStamps();

    		StampsReminderCountdown = 10.0f;
    	}

    	private void Update()
    	{
    		if (StampsReminderCountdown > 0 && StampPanel != null)
    		{
    			StampsReminderCountdown -= Time.deltaTime;
    			if (StampsReminderCountdown < 0 && GameManagerTouch.Instance.IsFirstLevelLoaded)
    			{
    				SoundManagerTouch.Instance.AddSFXToQueue("add_stamps", 1.0f, "voiceover", 2, 0.5f);
    			}
    		}
    	}

    	void InitColors()
        {
    		for (int i = 0; i < Colors.Length; i++)
    		{
    			GameObject newColor = Instantiate(RefColorObject, RefColorObject.transform.parent);
    			newColor.SetActive(true);
    			Transform baseGO = newColor.transform.Find("Padding/Base"); // Bad, don't learn this Igor!
    			baseGO.GetComponent<Image>().color = Colors[i];

    			Transform selected = newColor.transform.Find("Padding/Selected");
    			if (i == 0)
    				selected.gameObject.SetActive(true);
    			Button button = newColor.GetComponent<Button>();
    			int idx = i;
    			button.onClick.AddListener(() => OnColorPick(selected, idx));
    			colorSelections.Add(selected);
    		}
    		if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.TouchAndFill)
    		{
    			RainbowColor.SetActive(false);
    			GameManagerTouch.Instance.OnColorSelection(Colors[0]);
    		}
    		else
    		{
    			OnRainbowColorPick();
    		}
    	}

    	void InitPatternAndStamps()
        {
    		// Init Patterns
    		GameManagerTouch.Instance.OnPatternSelection(Patterns[0]);
    		GameManagerTouch.Instance.OnStampSelection(Stamps[0]);
    		for (int i = 0; i < Patterns.Length; i++)
    		{
    			GameObject newPattern = Instantiate(RefPatternObject, RefPatternObject.transform.parent);
    			newPattern.SetActive(true);
    			Transform baseGO = newPattern.transform.Find("Padding/Base"); // Bad, don't learn this Igor!
    			baseGO.GetComponent<Image>().sprite = Patterns[i];

    			Transform selected = newPattern.transform.Find("Padding/Selected");
    			if (i == 0)
    				selected.gameObject.SetActive(true);
    			Button button = newPattern.GetComponent<Button>();
    			int idx = i;
    			button.onClick.AddListener(() => OnPatternPick(selected, idx));
    			patternSelections.Add(selected);
    		}

    		// Init Stamps
    		for (int i = 0; i < Stamps.Length; i++)
    		{
    			GameObject newStamp = Instantiate(RefStampObject, RefStampObject.transform.parent);
    			newStamp.SetActive(true);
    			Transform baseGO = newStamp.transform.Find("Padding/Base"); // Bad, don't learn this Igor!
    			baseGO.GetComponent<Image>().sprite = Stamps[i];

    			Transform selected = newStamp.transform.Find("Padding/Selected");
    			if (i == 0)
    				selected.gameObject.SetActive(true);
    			Button button = newStamp.GetComponent<Button>();
    			int idx = i;
    			button.onClick.AddListener(() => OnStampPick(selected, idx));
    			stampSelections.Add(selected);
    		}
    	}

    	void UpdateSelection(GameManagerTouch.eTool tool, bool snap = false)
    	{
    		toolSelection = tool;
    		GameManagerTouch.Instance.OnToolSelection(toolSelection);
    		if (HighlighterTween != null)
    			HighlighterTween.Kill();
    		HighlighterTween = null;

    		if (snap)
    			Highlighter.transform.position = Tools[(int)toolSelection].position;
    		else
    			HighlighterTween = Highlighter.DOMove(Tools[(int)toolSelection].position, 0.1f).SetEase(Ease.OutExpo);

    		if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.PatternAndStamps)
            {
    			if (tool == GameManagerTouch.eTool.Pattern && isStampPanelShowing)
                {
    				isStampPanelShowing = false;
    				if (ToolSwapTween != null)
    					ToolSwapTween.Kill();
    				ToolSwapTween = DOTween.Sequence()
    					.Append(StampPanel.DOMove(StampPanelOriginalPosition + Vector3.right * 500.0f, 0.2f))
    					.Append(PatternPanel.DOMove(PatternPanelOriginalPosition, 0.2f));
                }
    			else if (tool == GameManagerTouch.eTool.Stamp && !isStampPanelShowing)
                {
    				isStampPanelShowing = true;
    				if (ToolSwapTween != null)
    					ToolSwapTween.Kill();
    				ToolSwapTween = DOTween.Sequence()
    					.Append(PatternPanel.DOMove(PatternPanelOriginalPosition + Vector3.right * 500.0f, 0.2f))
    					.Append(StampPanel.DOMove(StampPanelOriginalPosition, 0.2f));

    				StampsReminderCountdown = -1.0f; // Don't say the hint any longer
    			}
            }
    	}

    	void OnColorPick(Transform selected, int index)
    	{
    		colorSelectionIdx = index;
    		foreach (var selection in colorSelections)
    		{
    			selection.gameObject.SetActive(selected == selection);
    		}
    		RainbowColor.transform.Find("Padding/Selected").gameObject.SetActive(false);
    		GameManagerTouch.Instance.OnColorSelection(Colors[index]);
    		if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser)
    			SelectMainTool();

    		if (StampPanel == null && Random.value < 0.005f)
    		{
    			SoundManagerTouch.Instance.PlaySFX("ability_colors_amazing", 1.0f, "voiceover", 1);
    		}
    	}

    	public void OnRainbowColorPick()
    	{
    		foreach (var selection in colorSelections)
    		{
    			selection.gameObject.SetActive(false);
    		}
    		RainbowColor.transform.Find("Padding/Selected").gameObject.SetActive(true);
    		GameManagerTouch.Instance.OnRainbowColorSelection();
    		if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser)
    			SelectMainTool();
    	}
	
    	void OnPatternPick(Transform selected, int index)
    	{
    		patternSelectionIdx = index;
    		foreach (var selection in patternSelections)
    		{
    			selection.gameObject.SetActive(selected == selection);
    		}
    		GameManagerTouch.Instance.OnPatternSelection(Patterns[index]);
    		if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser)
    			SelectMainTool();
    	}
	
    	void OnStampPick(Transform selected, int index)
    	{
    		stampSelectionIdx = index;
    		foreach (var selection in stampSelections)
    		{
    			selection.gameObject.SetActive(selected == selection);
    		}
    		GameManagerTouch.Instance.OnStampSelection(Stamps[index]);
    		if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser)
    			SelectMainTool();
    	}

    	void SelectMainTool(bool snap = false)
    	{
    		if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.TouchAndFill)
    		{
    			UpdateSelection(GameManagerTouch.eTool.Fill, snap);
    		}
    		else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.ColorEnvironment || GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Outline)
    		{
    			UpdateSelection(GameManagerTouch.eTool.Brush, snap);
    		}
    		else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.PatternAndStamps)
    		{
    			UpdateSelection(GameManagerTouch.eTool.Pattern, snap);
    		}
    		else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Symmetry)
            {
    			UpdateSelection(GameManagerTouch.eTool.Symmetry2x, snap);
            }
    	}

    	public void OnBucketFillPress()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Fill);
    	}

    	public void OnEraserPress()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Eraser);
    	}

    	public void OnPencilPress()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Brush);
    	}

    	public void OnStampPress()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Stamp);
    	}
    	public void OnPattenPress()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Pattern);
    	}

    	public void OnSymmetryX2Press()
        {
    		UpdateSelection(GameManagerTouch.eTool.Symmetry2x);
        }

    	public void OnSymmetryX6Press()
    	{
    		UpdateSelection(GameManagerTouch.eTool.Symmetry6x);
    	}
    }


}