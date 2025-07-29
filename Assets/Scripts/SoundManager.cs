using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource bgAudioSource;
    public AudioSource gameAudioSource;



    public AudioClip bgSound;
    public AudioClip gameOver;
    public AudioClip buttonSound;
    // public AudioClip runningSound;
    //public AudioClip shootSound;

    public AudioClip CollectCoin;
    bool mapIsPlaying, encounterIsPlaying, bossIsPlaying;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        InitializeValues();
    }
    void InitializeValues()
    {
        if (!PlayerPrefs.HasKey("music"))
        {
            PlayerPrefs.SetInt("music", 1);
        }

        if (!PlayerPrefs.HasKey("sound"))
        {
            PlayerPrefs.SetInt("sound", 1);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("music") == 1)
        {
            playBGSound();
        }
        else
        {
            stopBGSound();
        }

        if (PlayerPrefs.GetInt("sound") == 1)
        {
            gameAudioSource.volume = 1;

        }
        else
        {
            gameAudioSource.volume = 0;

        }
    }

    // Update is called once per frame
    private void TurnSceneDependentSoundsOFF()
    {

        mapIsPlaying = false;
        encounterIsPlaying = false;
        bossIsPlaying = false;
    }
    void Update()
    {

    }

    public void playBGSound()
    {
        bgAudioSource.clip = bgSound;
        bgAudioSource.Play();
        TurnSceneDependentSoundsOFF();
    }

    public void stopBGSound()
    {
        bgAudioSource.Stop();
    }

    public void stopSound()
    {
        gameAudioSource.volume = 0;

    }

    public void enableSound()
    {
        gameAudioSource.volume = 1;

    }

    public void PlayButtonSound()
    {
        gameAudioSource.PlayOneShot(buttonSound);
    }

    // public void playRunningSound ()
    // {
    //     runningAudioSource.clip = runningSound;
    //     runningAudioSource.Play();
    // }


    public void PlayGameOver()
    {
        gameAudioSource.PlayOneShot(gameOver);
    }

}
