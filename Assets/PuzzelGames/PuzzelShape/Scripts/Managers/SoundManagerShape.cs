using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SoundManagerShape : MonoBehaviour
    {
    	[System.Serializable]
        public class AudioDesc
    	{
            public string Name;
            public AudioClip Clip;
    	}

    	class AudioQueue
        {
    		public AudioDesc Description;
    		public float Volume;
    		public bool IsSpatial;
    		public Vector3 Position;
        }

    	public static SoundManagerShape Instance { get; private set; }

    	[Range(0.0f, 1.0f)]
    	public float GlobalMusicVolume = 1.0f;
    	[Range(0.0f, 1.0f)]
    	public float GlobalSFXVolume = 1.0f;

    	public AudioDesc[] Sounds;

    	public string InitMusic = "";

        public AudioSource SFXSource;
        AudioSource[] musicSources = new AudioSource[2];
        int currentMusicSource;

    	Queue<AudioQueue> queue = new Queue<AudioQueue>();
    	float queueTimer;

        private void Awake()
        {
    		if (Instance != null && Instance != this)
    		{
    			Destroy(gameObject);
    		}
    		else
    		{
    			DontDestroyOnLoad(this);
    			Instance = this;
    		}

    		musicSources[0] = gameObject.AddComponent<AudioSource>();
    		musicSources[0].volume = 0f;
    		musicSources[0].loop = true;
    		musicSources[1] = gameObject.AddComponent<AudioSource>();
    		musicSources[1].volume = 0f;
    		musicSources[1].loop = true;
    	}

        void Start()
        {
    		if (InitMusic != "")
    		{
    			PlayMusic(InitMusic);
    		}
    	}

        void Update()
        {
            if (queueTimer > 0)
            {
    			queueTimer -= Time.deltaTime;
    			if (queueTimer <= 0f && queue.Count > 0)
                {
    				Debug.Log("Dequeing Item!");
    				AudioQueue audioQ = queue.Dequeue();
    				if (audioQ.IsSpatial)
    					PlaySFX(audioQ.Description.Name, audioQ.Position, audioQ.Volume);
    				else
    					PlaySFX(audioQ.Description.Name, audioQ.Volume);
    				queueTimer = audioQ.Description.Clip.length;
    				Debug.Log("Dequeing Item and adding delay: " + queueTimer);
    			}
            }
        }

    	public void ClearQueue()
        {
    		queue.Clear();
    		queueTimer = 0f;
        }

    	public void AddSFXToQueue(string name, float volume = 1.0f)
        {
    		if (queueTimer <= 0)
            { // Play instantly
    			AudioClip clip = GetClip(name);
    			PlaySFX(name, volume);
    			queueTimer = clip.length;
    			Debug.Log("Playing Instantly Queue + Delay: " + clip.length);
    		} 
    		else
            {
    			AudioDesc audioDesc = System.Array.Find(Sounds, x => x.Name == name);
    			if (audioDesc != null)
    			{
    				Debug.Log("Delaying queue");
    				queue.Enqueue(new AudioQueue() { Description = audioDesc, Volume = volume, IsSpatial = false });
    			}
            }
        }

    	public void AddSFXToQueue(string name, Vector3 position, float volume = 1.0f)
    	{
    		if (queueTimer <= 0)
    		{ // Play instantly
    			AudioClip clip = GetClip(name);
    			PlaySFX(name, position, volume);
    			queueTimer = clip.length;
    		}
    		else
    		{
    			AudioDesc audioDesc = System.Array.Find(Sounds, x => x.Name == name);
    			if (audioDesc != null)
    			{
    				queue.Enqueue(new AudioQueue() { Description = audioDesc, Volume = volume, IsSpatial = true, Position = position });
    			}
    		}
    	}

    	public void PlaySFX(string name, float volume = 1.0f)
    	{
            AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
            if (audio != null)
    		{
                SFXSource.PlayOneShot(audio.Clip, volume * GlobalSFXVolume);
    		}
            else
    		{
                Debug.LogWarning("[PlaySFX] Can't find sound " + name);
    		}
    	}

    	public void PlaySFX(string name, Vector3 position, float volume = 1.0f)
    	{
    		AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
    		if (audio != null)
    		{
    			AudioSource.PlayClipAtPoint(audio.Clip, position, volume * GlobalSFXVolume);
    		}
    		else
    		{
    			Debug.LogWarning("[PlaySFX] Can't find sound " + name);
    		}
    	}

    	public void PlayCongratsVoice(bool toQueue = false)
        {
    		string[] soundNames = { "YouDidGreat", "Alright", "Amazing", "SoCool", "Excelent", "GoodJob", "WellDone", "YouDidIt" };
    		if (toQueue)
    			AddSFXToQueue(soundNames[Random.Range(0, soundNames.Length)]);
    		else
    			PlaySFX(soundNames[Random.Range(0, soundNames.Length)]);
    	}
    	public AudioClip GetClip(string name)
    	{
    		AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
    		if (audio != null)
    		{
    			return audio.Clip;
    		}
    		return null;
    	}

        public void PlayMusic(string name, float volume = 1.0f)
    	{
    		AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
    		if (audio != null)
    		{
                AudioSource music = musicSources[currentMusicSource];
                if (music.isPlaying)
                    music.Stop();
                music.volume = volume * GlobalMusicVolume;
                music.clip = audio.Clip;
                music.Play();
    		}
    		else
    		{
    			Debug.LogWarning("[PlayMusic] Can't find sound " + name);
    		}
    	}

    	public void StopMusic()
    	{
    		if (musicSources[currentMusicSource].isPlaying)
    			musicSources[currentMusicSource].Stop();
    	}

        Coroutine crossfadeCoroutine = null;

        public void CrossFadeMusic(string name, float duration, float volume = 1.0f)
    	{
    		AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
    		if (audio != null)
    		{
    			if (crossfadeCoroutine != null)
    			{
    				StopCoroutine(crossfadeCoroutine);
    				crossfadeCoroutine = null;
    			}

    			crossfadeCoroutine = StartCoroutine(CrossFadeMusicCoroutine(audio.Clip, duration, volume * GlobalMusicVolume));
    		}
    		else
    		{
    			Debug.LogWarning("[PlayMusic] Can't find sound " + name);
    		}
    	}

        IEnumerator CrossFadeMusicCoroutine(AudioClip newMusic, float duration, float targetVolume)
    	{
    		while (fadeOutCoroutine != null || setMusicVolumeCoroutine != null)
    			yield return null; // Dangerous??? Maybe not

    		AudioSource fadeFromSource = musicSources[currentMusicSource];
    		currentMusicSource = GetOtherMusicSource();
    		AudioSource fadeToSource = musicSources[currentMusicSource];

    		if (fadeToSource.isPlaying)
    			fadeToSource.Stop();

    		fadeToSource.clip = newMusic;
    		fadeToSource.volume = 0f;
    		fadeToSource.Play();

    		if (!fadeFromSource.isPlaying)
    		{ // Nothing to crossfade just fade in
    			float multiplier = 0f;
    			while (multiplier != 1.0f)
    			{
    				multiplier = Mathf.MoveTowards(multiplier, 1.0f, Time.deltaTime / duration);
    				fadeToSource.volume = targetVolume * multiplier;
    				yield return null;
    			}
    		} 
    		else
    		{
    			float multiplier = 0f;
    			float fadeFromVolume = fadeFromSource.volume;
    			while (multiplier != 1.0f)
    			{
    				multiplier = Mathf.MoveTowards(multiplier, 1.0f, Time.deltaTime / duration);
    				fadeToSource.volume = targetVolume * multiplier;
    				fadeFromSource.volume = fadeFromVolume * (1.0f - multiplier);
    				yield return null;
    			}

    			fadeFromSource.Stop();
    		}
    	}

    	Coroutine fadeOutCoroutine = null;

    	public void FadeOutMusic(float duration)
    	{
    		if (!musicSources[currentMusicSource].isPlaying)
    			return;

    		if (fadeOutCoroutine != null)
    		{
    			StopCoroutine(fadeOutCoroutine);
    			fadeOutCoroutine = null;
    		}

    		fadeOutCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
    	}

    	IEnumerator FadeOutMusicCoroutine(float duration)
    	{
    		while (crossfadeCoroutine != null || setMusicVolumeCoroutine != null)
    			yield return null;

    		AudioSource fadeSource = musicSources[currentMusicSource];
    		float volume = fadeSource.volume;
    		float multiplier = 1.0f;
    		while (multiplier != 0f)
    		{
    			multiplier = Mathf.MoveTowards(multiplier, 0.0f, Time.deltaTime / duration);
    			fadeSource.volume = multiplier * volume;
    			yield return null;
    		}

    		fadeSource.Stop();
    	}

    	Coroutine setMusicVolumeCoroutine;

    	public void SetMusicVolume(float duration, float volume)
    	{
    		if (!musicSources[currentMusicSource].isPlaying)
    			return;

    		if (setMusicVolumeCoroutine != null)
    		{
    			StopCoroutine(setMusicVolumeCoroutine);
    			setMusicVolumeCoroutine = null;
    		}

    		setMusicVolumeCoroutine = StartCoroutine(SetMusicVolumeCoroutine(duration, volume * GlobalMusicVolume));
    	}

    	IEnumerator SetMusicVolumeCoroutine(float duration, float volume)
    	{
    		while (crossfadeCoroutine != null || fadeOutCoroutine != null)
    			yield return null;

    		AudioSource fadeSource = musicSources[currentMusicSource];
    		float sourceVolume = fadeSource.volume;
    		float interplation = 1.0f;
    		while (interplation != 0f)
    		{
    			interplation = Mathf.MoveTowards(interplation, 0.0f, Time.deltaTime / duration);
    			fadeSource.volume = Mathf.Lerp(volume, sourceVolume, interplation);
    			yield return null;
    		}
    	}

    	int GetOtherMusicSource()
    	{
    		return (currentMusicSource + 1) % 2;
    	}
    }


}