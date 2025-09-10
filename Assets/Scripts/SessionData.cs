using UnityEngine;

public class SessionData : MonoBehaviour
{
    public int SessionID;
    public static SessionData Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
     
    }
}
