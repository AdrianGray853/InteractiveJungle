using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

    public class SFXQuery : EditorWindow
    {
    	SoundManagerTouch soundManager;

    	[MenuItem("Macro/SFX Query")]
    	static void SettingsWindow()
    	{
    		SFXQuery window = CreateInstance<SFXQuery>();
    		window.ShowUtility();
    	}

    	void UpdateSounds()
        {
    		GameObject managers = Resources.Load<GameObject>("Managers");
    		if (managers != null)
    		{
    			soundManager = managers.GetComponent<SoundManagerTouch>();
    		}
    	}

    	string searchText = "";
    	bool searchInName = true;
    	bool searchInClip = true;
    	bool searchStartOnly = false;

    	Vector2 scrollPos;

    	void OnGUI()
    	{
    		/*
    		if (GUILayout.Button("Update Sounds"))
    			UpdateSounds();
    		*/
    		if (soundManager == null)
    			UpdateSounds();

    		//EditorGUIUtility.labelWidth = 75;

    		searchText = EditorGUILayout.TextField("Search: ", searchText);
    		searchInName = EditorGUILayout.Toggle("Search in Name: ", searchInName);
    		searchInClip = EditorGUILayout.Toggle("Search in Clip: ", searchInClip);
    		searchStartOnly = EditorGUILayout.Toggle("From start only: ", searchStartOnly);
    		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    		EditorGUIUtility.labelWidth = 75;
    		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
    		if (soundManager != null)
    		{
    			bool changed = false;
    			foreach (var clip in soundManager.Sounds)
    			{
    				string clipPath = AssetDatabase.GetAssetPath(clip.Clip.GetInstanceID());

    				if ((searchStartOnly && !clip.Clip.name.ToLower().StartsWith(searchText.ToLower()) || 
    					!searchStartOnly && !clip.Clip.name.ToLower().Contains(searchText.ToLower())) && searchInClip)
    					continue;
    				if ((searchStartOnly && !clip.Name.ToLower().StartsWith(searchText.ToLower()) ||
    					!searchStartOnly && !clip.Name.ToLower().Contains(searchText.ToLower())) && searchInName)
    					continue;

    				EditorGUI.BeginChangeCheck();
    				clip.Name = EditorGUILayout.TextField("Name: ", clip.Name);
    				//EditorGUILayout.LabelField("� " + clip.Name, EditorStyles.boldLabel);
    				//EditorGUILayout.LabelField(clipPath);
    				/*
    				if (GUILayout.Button(clipPath))
                    {
    					Object obj = AssetDatabase.LoadAssetAtPath(clipPath, typeof(Object));
    					Selection.activeObject = obj;
    					EditorGUIUtility.PingObject(obj);
    				}
    				*/
    				clip.Clip = EditorGUILayout.ObjectField("Clip: ", clip.Clip, typeof(AudioClip), false) as AudioClip;
    				if (EditorGUI.EndChangeCheck())
    				{
    					changed = true;
    				}
    				EditorGUILayout.Space();
    			}
    			if (changed)
    				EditorUtility.SetDirty(soundManager);
    		}
    		EditorGUILayout.EndScrollView();
    	}

    	void OnInspectorUpdate()
    	{
    		Repaint();
    	}
    }

}