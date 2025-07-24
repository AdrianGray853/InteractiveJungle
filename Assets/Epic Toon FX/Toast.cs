using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toast : MonoBehaviour
{
    private void OnEnable()
    {
        ShowToast("Hi Bilu");
    }
    public static void ShowToast(string text)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(
              () =>
              {
                  AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
                  AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", text);
                  AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                  AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
                  toast.Call("show");
              }
              ));
        }
    }
}
