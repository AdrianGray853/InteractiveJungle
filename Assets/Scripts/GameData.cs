using UnityEngine;
namespace Interactive.Jungle { 
public static class GameData
{
    public static int GetCurrentLevelNo
    {
        get => PlayerPrefs.GetInt("GetCurrentLevelNo", 0); // Default value is 0
        set => PlayerPrefs.SetInt("GetCurrentLevelNo", value);
    }
    public static int GetBackgroundPuzzel
    {
        get => PlayerPrefs.GetInt("GetBackgroundPuzzel", 0); // Default value is 0
        set => PlayerPrefs.SetInt("GetBackgroundPuzzel", value);
    }
}
}