using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private string InitialSceneToLoad;
    	public string LastScene { get; private set; }
	
    	private static SceneLoader _instance = null;
    	//private static bool _initialized = false;
    	public static SceneLoader Instance
    	{
    		get
    		{
    			if (_instance == null)// && !_initialized)
    			{
    				Instantiate(Resources.Load("Managers") as GameObject);
    				//GameObject go = new GameObject("SceneLoader");
    				//_instance = go.AddComponent<SceneLoader>();

    				//_initialized = true;
    			}
    			return _instance;
    		}
    		private set
    		{
    			_instance = value;
    			//_initialized = true;
    		}
    	}

    	bool initialSceneLoaded = false;

    	private void Awake()
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

    		LastScene = "Loader";
    	}

    	// Start is called before the first frame update
    	void Start()
        {
    		Application.targetFrameRate = 60;
        }

        // Update is called once per frame
        void Update()
        {
    		if (!SplashScreen.isFinished)
    			return;

    		if (!initialSceneLoaded)
            {
    			if (InitialSceneToLoad != null && InitialSceneToLoad != "")
    			{ // Guard for temporary managers in scenes
    				LoadScene(InitialSceneToLoad);
    			}
    			initialSceneLoaded = true;
    		}
    	}

        public void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

    	public void LoadScene(string sceneName, bool KillTweens = true)
    	{
    		if (SystemInfo.deviceModel.Contains("iPad") && !sceneName.Contains("iPad"))
    			sceneName = sceneName + "_iPad";
            Debug.Log("Loading scene " + sceneName);

    		if (KillTweens)
    			DOTween.KillAll(true, "Transition"); //DOTween.Clear(true);
    		DisableAll(); // Make sure we disable all gameobjects to avoid shit...

    		SoundManager.Instance.ClearAndStopSFX();

    		LastScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneName);
        }

    	public string GetCurrentSceneName()
    	{
    		return SceneManager.GetActiveScene().name;
    	}

    	void DisableAll()
    	{
    		List<GameObject> rootObjects = new List<GameObject>();
    		Scene scene = SceneManager.GetActiveScene();
    		scene.GetRootGameObjects(rootObjects);
    		for (int i = 0; i < rootObjects.Count; ++i)
    		{
    			GameObject gameObject = rootObjects[i];
    			//if (gameObject.scene.name == "DontDestroyOnLoad")
    			//	continue;
    			gameObject.SetActive(false);
    		}
    		//Debug.Break();
    	}
    }


}