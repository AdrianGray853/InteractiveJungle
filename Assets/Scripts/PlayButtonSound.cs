using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonSound : MonoBehaviour
{
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }
    public void PlaySound()
    {
        //SoundManager.Instance.PlayClickSFX(SFXType.Click);
    }
}
