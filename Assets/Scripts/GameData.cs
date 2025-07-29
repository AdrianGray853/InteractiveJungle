using UnityEngine;

public static class GameData 
{
    public static int GetCurrentLevelNo
    {
        get => PlayerPrefs.GetInt("GetCurrentLevelNo", 0); // Default value is 0
        set => PlayerPrefs.SetInt("GetCurrentLevelNo", value);
    }
}
