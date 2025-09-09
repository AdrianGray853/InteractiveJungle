using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

[Serializable]
public class VoiceOverEntry
{
    public VoiceOverType type;
    public AudioClip clip;
}

[Serializable]
public class SFXEntry
{
    public SFXType type;
    public AudioClip clip;
}

public enum VoiceOverType
{
    // Jungle-specific
    WelcomeToTheJungleDecorateAndMeetNewFriends,
    RakeTheLeavesOnTheGround,
    TakeCareOfYourJungleFriendsAndFeedThemEveryDay,

    // Common
    GreatJobYouUnlockedSomethingNew,
    WellDoneYouUnlockedSomethingNew,
    LetsBuildTheObjectFromShapes,
    LetsBuildTheWordPlaceEachLetter,
    LetsColorPickYourFavoriteColors,
    LetsDoAPuzzle,
    LetsTraceThePathFollowTheLine,
    LetsPlayAndUnlockNewThings,
    SoNice,
    ThatsAmazing,
    ThatsSoFun
}

public enum SFXType
{
    // Jungle-specific
    MainMusicJungle,
    AnimalsEating,
    RakingTheLeaves,

    // Common
    Click,
    DoneActivity,
    DragAndDrop,
    GiftFromCompletedActivity,
    HomeButtonFromActivities,
    Unlock
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource clickSource;
    [SerializeField] private AudioSource sfxMusicSource;
    [SerializeField] private AudioSource voiceOverSource;

    [Header("VoiceOvers Mapping")]
    [SerializeField] private List<VoiceOverEntry> voiceOverEntries = new List<VoiceOverEntry>();

    [Header("SFX Mapping")]
    [SerializeField] private List<SFXEntry> sfxEntries = new List<SFXEntry>();

    private Dictionary<VoiceOverType, AudioClip> voiceOverDict;
    private Dictionary<SFXType, AudioClip> sfxDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Convert lists to dictionaries
        voiceOverDict = new Dictionary<VoiceOverType, AudioClip>();
        foreach (var entry in voiceOverEntries)
        {
            if (!voiceOverDict.ContainsKey(entry.type))
                voiceOverDict.Add(entry.type, entry.clip);
        }

        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (var entry in sfxEntries)
        {
            if (!sfxDict.ContainsKey(entry.type))
                sfxDict.Add(entry.type, entry.clip);
        }
    }

    #region Music
    public void PlayMusic(SFXType type, bool loop = true)
    {
        if (sfxDict.TryGetValue(type, out var clip))
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic() => musicSource.Stop();
    #endregion

    #region SFX
    public void PlaySFX(SFXType type)
    {
        if (sfxDict.TryGetValue(type, out var clip))
            sfxSource.PlayOneShot(clip);
    }
    public void PlayClickSFX(SFXType type)
    {
        if (sfxDict.TryGetValue(type, out var clip))
            clickSource.PlayOneShot(clip);
    }

    public void PlaySFXMusic(SFXType type)
    {
        if (sfxDict.TryGetValue(type, out var clip))
        {
            sfxMusicSource.clip = clip;
            sfxMusicSource.loop = true;
            sfxMusicSource.Play();
        }
    }

    public void StopSfxMusic() => sfxMusicSource.Stop();
    #endregion

    #region VoiceOvers
    public void PlayVoiceOver(VoiceOverType type)
    {
        if (voiceOverDict.TryGetValue(type, out var clip))
        {
            voiceOverSource.Stop(); // only one VO at a time
            voiceOverSource.clip = clip;
            voiceOverSource.Play();
        }
    }
    public void PlayVoiceOver(AudioClip clip)
    {
       
            voiceOverSource.Stop(); // only one VO at a time
            voiceOverSource.clip = clip;
            voiceOverSource.Play();
        
    }

    public void StopVoiceOverMusic() => voiceOverSource.Stop();
    #endregion
}
