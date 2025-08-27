using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class SceneLoaderShape : MonoBehaviour
    {
    	public static SceneLoaderShape Instance { get; private set; }

        [SerializeField]
        private string InitialSceneToLoad;
    	public string LastScene { get; private set; }


    	private void Awake()
    	{
    		if (Instance != null && Instance != this)
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

    	void Start()
        {
    		if (InitialSceneToLoad != null && InitialSceneToLoad != "")
    		{ // Guard for temporary managers in scenes
    			Application.targetFrameRate = 60;
    			LoadScene(InitialSceneToLoad);
    		}
        }

        public void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadScene(string sceneName)
    	{
    		if (UtilsShape.IsIPad() && !sceneName.Contains("iPad"))
    			sceneName = sceneName + "_iPad";
    		Debug.Log("Loading scene " + sceneName);

    		DisableAll(); // Make sure we disable all gameobjects to avoid shit...

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