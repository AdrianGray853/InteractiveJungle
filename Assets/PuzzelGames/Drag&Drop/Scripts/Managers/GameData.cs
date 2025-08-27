using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GameData : MonoBehaviour
    {
        static GameData _instance = null;
        public static GameData Instance { 
            get
    		{
                if (_instance == null)
    			{
                    //_instance = FindObjectOfType<GameData>();
                    //if (_instance == null)
    				//{
                        Instantiate(Resources.Load("Managers") as GameObject);
                        /*
                        GameObject go = new GameObject("GameData");
                        _instance = go.AddComponent<GameData>();
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
            None = 0,
            LowerCase = 1,
            UpperCase = 2,
            MixedCase = 3,
            MiniGames = 4,

            NumModules
        }

        public eGameType GameType;
        public int SelectedLevel;

        // Flags per session (non persistent)
        private Dictionary<string, bool> Flags = new Dictionary<string, bool>();

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

        public void SetFlag(string flag, bool value = true)
        {
            Flags[flag] = value;
        }

        // Gets the flag, in case that the flag doesn't exist it will return the default value
        public bool GetFlag(string flag, bool defaultValue = false)
        {
            return Flags.GetValueOrDefault(flag, defaultValue);
        }
    }


}