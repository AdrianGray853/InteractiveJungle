using UnityEngine;

public class SessionData : MonoBehaviour
{
    public int SessionID;
    public static SessionData Instance;
    private void Awake()
    {
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
