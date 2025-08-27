using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

    public class AddSFXFromFilesShape : EditorWindow
    {
    	AudioClip[] clips;

    	[MenuItem("Macro/Add SFX From Files")]
    	static void SettingsWindow()
    	{
    		AddSFXFromFilesShape window = CreateInstance<AddSFXFromFilesShape>();
    		window.ShowUtility();
    	}

    	void UpdateSFXSelection()
    	{
    		clips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);
    		//Repaint();
    	}

    	void AddToSoundManager()
    	{
    		if (clips == null || clips.Length == 0)
    		{
    			Debug.Log("Select some clips first!");
    			return;
    		}

    		GameObject managers = Resources.Load<GameObject>("Managers");
    		if (managers != null)
    		{
    			SoundManagerShape soundManager = managers.GetComponent<SoundManagerShape>();
    			if (soundManager != null)
    			{
    				SoundManagerShape.AudioDesc[] audioDescs = clips.Select(x => new SoundManagerShape.AudioDesc { Clip = x, Name = x.name }).ToArray();
    				soundManager.Sounds = soundManager.Sounds.Concat(audioDescs).ToArray();
    				EditorUtility.SetDirty(soundManager);
    			}
    		}
    	}

    	void OnGUI()
    	{
    		if (GUILayout.Button("Update Selection"))
    			UpdateSFXSelection();
    		if (GUILayout.Button("Add to SoundManagerShape"))
    			AddToSoundManager();
    		//if (GUILayout.Button("Close"))
    		//	Close();

    		if (clips != null)
    		{
    			foreach (var clip in clips)
    			{
    				EditorGUILayout.LabelField(clip.name);
    			}
    		}
    	}

    	void OnInspectorUpdate()
    	{
    		Repaint();
    	}
    }

}