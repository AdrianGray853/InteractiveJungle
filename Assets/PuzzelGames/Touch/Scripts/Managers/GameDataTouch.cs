using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GameDataTouch : MonoBehaviour
    {
        static GameDataTouch _instance = null;
        public static GameDataTouch Instance { 
            get
    		{
                if (_instance == null)
    			{
                    //_instance = FindObjectOfType<GameDataTouch>();
                    //if (_instance == null)
    				//{
                        Instantiate(Resources.Load("Managers") as GameObject);
                        /*
                        GameObject go = new GameObject("GameDataTouch");
                        _instance = go.AddComponent<GameDataTouch>();
                        */
    				//}
    			}
                return _instance;
            }
            set
    		{
                _instance = value;
    		}
    	}

    	public enum eGameType 
        { 
            TouchAndFill,
            ColorEnvironment,
            PatternAndStamps,
            AI,
            Symmetry,
            Outline,
            Tracing,

            NumModules
        }

        public readonly static string[] SceneNames =
        {
    		"TouchAndFill",
    		"ColorInEnvironment",
    		"StampAndPattern",
    		"AI",
    		"Symmetry",
    		"Outline",
    #if PROJECT_X
    		"DrawTracing"
    #else
    		"Tracing"
    #endif
        };

        public eGameType GameType;
        public int SelectedLevel;

        public float JungleScrollOffset = 0f;
        public int JungleEnableMask = 0;
        public int JungleCategory = 3;

    	// Start is called before the first frame update
    	void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
        }
    }


}