using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

    public class AddSFXFromFiles : EditorWindow
    {
    	AudioClip[] clips;

    	[MenuItem("Macro/Add SFX From Files")]
    	static void SettingsWindow()
    	{
    		AddSFXFromFiles window = CreateInstance<AddSFXFromFiles>();
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
    			SoundManager soundManager = managers.GetComponent<SoundManager>();
    			if (soundManager != null)
    			{
    				SoundManager.AudioDesc[] audioDescs = clips.Select(x => new SoundManager.AudioDesc { Clip = x, Name = x.name }).ToArray();
    				soundManager.Sounds = soundManager.Sounds.Concat(audioDescs).ToArray();
    				EditorUtility.SetDirty(soundManager);
    			}
    		}
    	}

    	void OnGUI()
    	{
    		if (GUILayout.Button("Update Selection"))
    			UpdateSFXSelection();
    		if (GUILayout.Button("Add to SoundManager"))
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