using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using DG.Tweening;
    public static class HelperFunctions
{
    public static void Refresh(this List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
              
            if(list[i]==null)
            {
                list.Remove(list[i]);
            }
        }
    }
    public static void SetY(this Transform transform, float y)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        Vector3 position = transform.position;
        position.y = y;
        transform.position = position;
    }

    public static Tweener DOTextFloat(this TextMeshProUGUI text, float initialValue, float finalValue, float duration)

    {

        return DOTextFloat(text, initialValue, finalValue, duration, it => it.ToString());

    }

     static Tweener DOTextFloat(this TextMeshProUGUI text, float initialValue, float finalValue, float duration, Func<float, string> convertor)

    {

        return DOTween.To(

             () => initialValue,

             it => text.text = convertor(it),

             finalValue,

             duration

         );

    }
    public static void SetX(this ref Vector3 transform, float x)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        transform.x = x;
    } public static void SetY(this ref Vector3 transform, float y)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        transform.y = y;
    } public static void SetZ(this ref Vector3 transform, float z)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        transform.z = z;
    }
    public static void SetX(this Transform transform, float x)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
    public static void SizeY(this Transform transform, float y)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        Vector3 position = transform.localScale;
        position.y = y;
        transform.localScale = position;
    }
    public static void SizeX(this Transform transform, float x)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        Vector3 position = transform.localScale;
        position.x = x;
        transform.localScale = position;
    }
    public static void SetZ(this Transform transform, float z)
    {
        if (transform == null) { Debug.LogError("transform is null"); return; }
        Vector3 position = transform.position;
        position.z = z;
        transform.position = position;
    }
    /// <summary>Invokes an action after a given time.</summary>
    /// <param name="action">The action to invoke.</param>
    /// <param name="time">The time in seconds.</param>
    /// <param name="useCachedYields">Whether cached yield values should be used. Defaults to true.</param>
    public static Coroutine Invoke(this MonoBehaviour monoBehaviour, Action action, float time, bool useCachedYields = true)
    {
        return monoBehaviour.StartCoroutine(InvokeImplementation(action, time, useCachedYields));
    }

    /// <summary>Coroutine which waits time seconds and then invokes the given action.</summary>
    private static IEnumerator InvokeImplementation(Action action, float time, bool useCachedYields)
    {
        //wait for time seconds then invoke the action. if useCachedYields is true, uses a cached WaitForSeconds, otherwise creates a new one
        yield return (useCachedYields ? WaitFor.Seconds(time) : new WaitForSeconds(time));
        action();
    }

    /// <summary>Invokes an action after a given time, then repeatedly every repeateRate seconds.</summary>
    /// <param name="action">The action to invoke.</param>
    /// <param name="time">The time in seconds.</param>
    /// <param name="repeatRate">The repeat rate in seconds.</param>
    /// <param name="useCachedYields">Whether cached yield values should be used. Defaults to true.</param>
    public static Coroutine InvokeRepeating(this MonoBehaviour monoBehaviour, Action action, float time, float repeatRate, bool useCachedYields = true)
    {
        return monoBehaviour.StartCoroutine(InvokeRepeatingImplementation(action, time, repeatRate, useCachedYields));
    }

    /// <summary>The coroutine implementation of InvokeRepeating.
    private static IEnumerator InvokeRepeatingImplementation(Action action, float time, float repeatRate, bool useCachedYields)
    {
        //wait for a given time then indefiently loop - if useCachedYields is true, uses a cached WaitForSeconds, otherwise creates a new one
        yield return (useCachedYields ? WaitFor.Seconds(time) : new WaitForSeconds(time));
        while (true)
        {
            //invokes the action then waits repeatRate seconds - if useCachedYields is true, uses a cached WaitForSeconds, otherwise creates a new one
            action();
            yield return (useCachedYields ? WaitFor.Seconds(repeatRate) : new WaitForSeconds(repeatRate));
        }
    }


}



public static class WaitFor
{
    /// <summary>A Float comparer used in the waitForSecondsDictionary.</summary>
    private class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }
    /// <summary>A dictionary of WaitForSeconds whose keys are the wait time.</summary>
    private static Dictionary<float, WaitForSeconds> waitForSecondsDictionary = new Dictionary<float, WaitForSeconds>(0, new FloatComparer());
    /// <summary>Suspends the coroutine execution for the given amount of seconds using scaled time.</summary>
    public static WaitForSeconds Seconds(float seconds)
    {
        //test if a WaitForSeconds with this wait time exists - if not, create one
        WaitForSeconds waitForSeconds;
        if (!waitForSecondsDictionary.TryGetValue(seconds, out waitForSeconds))
        {
            waitForSecondsDictionary.Add(seconds, waitForSeconds = new WaitForSeconds(seconds));
        }
        return waitForSeconds;
    }

    /// <summary>A backing variable for FixedUpdate.</summary>
    static WaitForFixedUpdate _FixedUpdate;
    /// <summary>Waits until next fixed frame rate update function.</summary>
    public static WaitForFixedUpdate FixedUpdate
    {
        get { return _FixedUpdate ?? (_FixedUpdate = new WaitForFixedUpdate()); }
    }

    /// <summary>A backing variable for EndOfFrame.</summary>
    private static WaitForEndOfFrame _EndOfFrame;
    /// <summary>Waits until the end of the frame after all cameras and GUI is rendered, just before displaying the frame on screen.</summary>
    public static WaitForEndOfFrame EndOfFrame
    {
        get { return _EndOfFrame ?? (_EndOfFrame = new WaitForEndOfFrame()); }
    }
}
