using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Image musicImage;
    public Image soundImage;

    public Sprite[] musicImages;
    public Sprite[] soundImages;

    //public string enableAnimation;
    //public string disableAnimation;

    private void OnEnable()
    {
        //GetComponent<Animator>().Play(enableAnimation);
        setSoundSettings();
    }

    public void onMusicButton()
    {
        if (PlayerPrefs.GetInt("music") == 1)
        {
            PlayerPrefs.SetInt("music", 0);
            //SoundManager.instance.stopBGSound();
        }
        else
        {
            PlayerPrefs.SetInt("music", 1);
            //SoundManager.instance.playBGSound();
        }
        setSoundSettings();
    }

    public void onSoundButton ()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            PlayerPrefs.SetInt("sound", 0);
            //SoundManager.instance.stopSound();
        }
        else
        {
            PlayerPrefs.SetInt("sound", 1);
            //SoundManager.instance.enableSound();
        }
        setSoundSettings();
    }

    public void onBackButton ()
    {
        //SoundManager.instance.PlayButtonSound();
        //GetComponent<Animator>().Play(disableAnimation);
        Invoke("onExit", 0.4f);
    }

    private void onExit()
    {
        gameObject.SetActive(false);
    }

    void setSoundSettings ()
    {
        if(PlayerPrefs.GetInt ("music") == 1)
        {
            musicImage.sprite = musicImages[0];
        }
        else
        {
            musicImage.sprite = musicImages[1];
        }

        if (PlayerPrefs.GetInt("sound") == 1)
        {
            soundImage.sprite = soundImages[0];
        }
        else
        {
            soundImage.sprite = soundImages[1];
        }
    }

    private void OnDisable()
    {
        //SoundManager.instance.PlayButtonSound();
       // GetComponent<Animator>().Play(disableAnimation);
    }
}
