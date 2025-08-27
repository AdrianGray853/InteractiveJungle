using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class DebugLogCollector : MonoBehaviour
    {
    	[System.Flags]
    	public enum eLogType
    	{
    		Critical = 1 << 0,  // Errors, asserts, exceptions, ...
    		Errors = 1 << 1,
    		Warnings = 1 << 2,
    		Logs = 1 << 3
    	}

    	public eLogType LogType;
    	public int MaxLogLength = 100;

    	public class LogRecord
    	{
    		public LogType type;
    		public string message;
    		public string stack;
    		public System.DateTime time;
    	}

    	public List<LogRecord> Records = new List<LogRecord>();

        UnityEngine.UI.RawImage backgroundBlocker;

    	void OnEnable()
    	{
    #if DEVELOPMENT_BUILD
    		Application.logMessageReceived += HandleLog;
    #endif
    	}

    	void OnDisable()
    	{
    #if DEVELOPMENT_BUILD
    		Application.logMessageReceived -= HandleLog;
    #endif
    	}

    #if DEVELOPMENT_BUILD
        private void Start()
        {
    		Canvas canvas = GetComponentInChildren<Canvas>();
    		if (canvas)
            {
    			// Create a new RawImage GameObject
    			GameObject rawImageGO = new GameObject("CanvasBlocker");
    			RectTransform rawImageRectTransform = rawImageGO.AddComponent<RectTransform>();
    			backgroundBlocker = rawImageGO.AddComponent<UnityEngine.UI.RawImage>();

    			// Set the RawImage as a child of the canvas
    			rawImageRectTransform.SetParent(canvas.transform, false);

    			// Set the RawImage position and size to match the screen dimensions
    			rawImageRectTransform.anchorMin = Vector2.zero;
    			rawImageRectTransform.anchorMax = Vector2.one;
    			rawImageRectTransform.anchoredPosition = Vector2.zero;
    			rawImageRectTransform.sizeDelta = Vector2.zero;

    			backgroundBlocker.color = new Color(0f, 0f, 0f, 0.5f);
    			rawImageGO.SetActive(false);
    		}
        }
    #endif

        void HandleLog(string logString, string stackTrace, LogType type)
    	{
    		if ((type == UnityEngine.LogType.Assert || type == UnityEngine.LogType.Exception) && (LogType & eLogType.Critical) == 0)
    			return;
    		if (type == UnityEngine.LogType.Error && (LogType & eLogType.Errors) == 0)
    			return;
    		if (type == UnityEngine.LogType.Warning && (LogType & eLogType.Warnings) == 0)
    			return;
    		if (type == UnityEngine.LogType.Log && (LogType & eLogType.Logs) == 0)
    			return;

    		Records.Add(new LogRecord() { type = type, message = logString, stack = stackTrace, time = System.DateTime.Now });

    		while (Records.Count > MaxLogLength)
    		{
    			Records.RemoveAt(0);
    		}
    	}

    #if DEVELOPMENT_BUILD
    	bool IsShown = false;    
    	Vector2 scrollPosition = Vector2.zero;
    	Vector2 scrollPositionContent = Vector2.zero;
    	int selectedIndex = -1;
    	private void OnGUI()
    	{
    		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    		buttonStyle.fontSize = 50;

    		GUIStyle wrapTextSyle = new GUIStyle(GUI.skin.label);
    		wrapTextSyle.wordWrap = true;
    		wrapTextSyle.fontSize = 50;
    		//wrapTextSyle.normal.background = buttonStyle.normal.background;
    		GUI.skin.verticalScrollbar.fixedWidth = Screen.width * 0.02f;
    		GUI.skin.verticalScrollbarThumb.fixedWidth = Screen.width * 0.02f;
    		GUI.skin.horizontalScrollbar.fixedHeight = Screen.width * 0.02f;
    		GUI.skin.horizontalScrollbarThumb.fixedHeight = Screen.width * 0.02f;

    		if (GUI.Button(new Rect(Screen.width * 0.5f - 100.0f, Screen.height - 100.0f, 300.0f, 100.0f), IsShown ? "Close Debug" : "Open Debug", buttonStyle))
    		{
    			IsShown = !IsShown;
    			backgroundBlocker.gameObject.SetActive(IsShown);
    		}


    		if (IsShown)
    		{
    			scrollPosition = GUI.BeginScrollView(new Rect(0f, 0f, Screen.width * 0.5f, Screen.height - 100.0f), scrollPosition, new Rect(0f, 0f, Screen.width * 0.5f, Records.Count * 50.0f));
    			for (int i = 0; i < Records.Count; i++)
    			{
    				LogRecord record = Records[Records.Count - i - 1];
    				if (record.type == UnityEngine.LogType.Error || record.type == UnityEngine.LogType.Assert || record.type == UnityEngine.LogType.Exception)
    					buttonStyle.normal.textColor = Color.red;
    				else if (record.type == UnityEngine.LogType.Warning)
    					buttonStyle.normal.textColor = Color.yellow;
    				else
    					buttonStyle.normal.textColor = Color.white;

    				if (GUI.Button(new Rect(0f, i * 50.0f, Screen.width * 0.5f, 50.0f), record.message, buttonStyle))
    				{
    					if (selectedIndex == i)
    						selectedIndex = -1;
    					else
    						selectedIndex = Records.Count - i - 1;
    				}
    			}
    			GUI.EndScrollView();

    			if (selectedIndex >= 0 && selectedIndex < Records.Count)
    			{
    				string text = Records[selectedIndex].message + "\n\n" + Records[selectedIndex].stack.Replace("\n", "\n> ");
    				float textHeight = wrapTextSyle.CalcHeight(new GUIContent(text), Screen.width * 0.5f);
    				scrollPositionContent = GUI.BeginScrollView(new Rect(Screen.width * 0.5f, 0f, Screen.width * 0.5f, Screen.height - 100.0f), scrollPositionContent, new Rect(0f, 0f, Screen.width * 0.5f, textHeight));
    				GUI.Box(new Rect(0f, 0f, Screen.width * 0.5f, textHeight), text, wrapTextSyle);
    				//GUI.Label(new Rect(0f, 0f, Screen.width * 0.5f, textHeight), text, wrapTextSyle);
    				GUI.EndScrollView();
    			}
    		}

    	}
    #endif
    }


}