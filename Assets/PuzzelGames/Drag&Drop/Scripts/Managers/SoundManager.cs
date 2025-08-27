using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
    	private static SoundManager _instance = null;
    	public static SoundManager Instance
    	{
    		get
    		{
    			if (_instance == null)
    			{
    				Instantiate(Resources.Load("DragDrop/Managers") as GameObject);
    			}
    			return _instance;
    		}
    		private set
    		{
    			_instance = value;
    		}
    	}

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
    		public int Priority;
    		public string Category;
    	}

    	[Range(0.0f, 1.0f)]
    	public float GlobalMusicVolume = 1.0f;
    	[Range(0.0f, 1.0f)]
    	public float GlobalSFXVolume = 1.0f;

    	public AudioDesc[] Sounds;

    	public string InitMusic = "";

        public AudioSource SFXSource;
        AudioSource[] musicSources = new AudioSource[2];
        int currentMusicSource = 0;
    	string currentMusic = "";

    	Queue<AudioQueue> queue = new Queue<AudioQueue>();
    	float queueTimer = 0f;

    	class AudioSpawnDesc
        {
    		public AudioSource Source;
    		public int Priority; // Sounds with priority 0 will not get overriten
    		public string Category; // Category is used with priority to replace sounds (stop and remove) so they'll not play together
        }
    	List<AudioSpawnDesc> spawnedSounds = new List<AudioSpawnDesc>();
    	List<SFXStack> spawnedStacks = new List<SFXStack>();

    	private void Awake()
        {
    		if (_instance != null && _instance != this)
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

        // Start is called before the first frame update
        void Start()
        {
    		if (InitMusic != "")
    		{
    			PlayMusic(InitMusic);
    		}
    	}

    	// Update is called once per frame
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
    					PlaySFX(audioQ.Description.Name, audioQ.Volume, audioQ.Category, audioQ.Priority);
    				queueTimer = audioQ.Description.Clip.length;
    				Debug.Log("Dequeing Item and adding delay: " + queueTimer);
    			}
    		}

    		for (int i = spawnedSounds.Count - 1; i >= 0; i--)
    		{
    			if (spawnedSounds[i] == null || spawnedSounds[i].Source == null || spawnedSounds[i].Source.gameObject == null)
    				spawnedSounds.RemoveAt(i);
    		}

    		for (int i = spawnedStacks.Count - 1; i >= 0; i--)
            {
    			if (!spawnedStacks[i].StackSequence.IsActive())
                {
    				spawnedStacks.RemoveAt(i);
                }
            }
    	}

    	public void ClearAndStopSFX()
        {
    		queue.Clear();
    		queueTimer = -1.0f;

    		foreach (var sound in spawnedSounds)
    			if (sound.Source != null && sound.Source.gameObject != null)
    				Destroy(sound.Source.gameObject);

    		spawnedSounds.Clear();

    		foreach (var stack in spawnedStacks)
    			stack.StackSequence.Kill(false);

    		spawnedStacks.Clear();
        }

    	public AudioSource SpawnSFX(AudioClip clip, float volume, float pitch = 1.0f)
        {
    		GameObject sound = new GameObject("sound_" + clip.name);
    		AudioSource audio = sound.AddComponent<AudioSource>();

    		audio.clip = clip;
    		audio.volume = volume;
    		audio.pitch = pitch;
    		audio.loop = false; //SFXSource.loop;
    		audio.playOnAwake = false; //SFXSource.playOnAwake;
    		audio.spatialBlend = SFXSource.spatialBlend;
    		audio.minDistance = SFXSource.minDistance;
    		audio.maxDistance = SFXSource.maxDistance;
    		audio.dopplerLevel = SFXSource.dopplerLevel;
    		audio.rolloffMode = SFXSource.rolloffMode;
    		audio.panStereo = SFXSource.panStereo;
    		audio.priority = SFXSource.priority;
    		audio.ignoreListenerPause = SFXSource.ignoreListenerPause;
    		audio.ignoreListenerVolume = SFXSource.ignoreListenerVolume;
    		audio.outputAudioMixerGroup = SFXSource.outputAudioMixerGroup;

    		sound.AddComponent<DestroyAudioOnFinish>();

    		audio.Play();

    		return audio;
        }

    	public AudioSource PlaySFX(string name, float volume = 1.0f, string category = "sfx", int priority = 0, SFXStack fromStack = null)
    	{
    		if (priority != 0)
    		{
    			if (spawnedSounds.Any(x => x.Category == category && x.Priority > priority))
    			{
    				Debug.Log("Found a higher priority sound! Ignoring " + name);
    				return null;
    			}

    			/*
    			var soundsToRemove = spawnedSounds.Where(x => x.Category == category && x.Priority != 0 && x.Priority <= priority).ToArray();
    			foreach (var sound in soundsToRemove)
                {
    				Destroy(sound.Source.gameObject);
    				spawnedSounds.Remove(sound);
                }
    			*/
    			for (int i = spawnedSounds.Count - 1; i >= 0; i--)
                {
    				var sound = spawnedSounds[i];
    				if (sound == null || sound.Category == category && sound.Priority != 0 && sound.Priority <= priority)
                    {
    					if (sound != null && sound.Source != null && sound.Source.gameObject != null)
    						Destroy(sound.Source.gameObject);
    					spawnedSounds.RemoveAt(i);
                    }
                }

    			for (int i = spawnedStacks.Count - 1; i >= 0; i--)
                {
    				if (fromStack != null && fromStack == spawnedStacks[i])
    					continue; // Avoid evaluating the same stack... might not be the best solution atm, but need to go FAST and FURIOUS! It's all about Family Vin!

    				if (spawnedStacks[i].Priority != 0 && spawnedStacks[i].Category == category && spawnedStacks[i].Priority <= priority)
                    {
    					spawnedStacks[i].StackSequence.Kill(false);
    					spawnedStacks.RemoveAt(i);
                    }
                }
    		}

            AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
            if (audio != null)
    		{
    			//SFXSource.PlayOneShot(audio.Clip, volume * GlobalSFXVolume);
    			AudioSource source = SpawnSFX(audio.Clip, volume * GlobalSFXVolume);
    			spawnedSounds.Add(new AudioSpawnDesc() { Source = source, Category = category, Priority = priority });
    			return source;
    		}
            else
    		{
                Debug.LogWarning("[PlaySFX] Can't find sound " + name);
    			return null;
    		}
    	}

    	public class SFXStack
        {
    		public float PreviousDuration;
    		public Sequence StackSequence;
    		public int Priority;
    		public string Category;

    		public SFXStack AddSFXStack(string name, float volume = 1.0f)
            {
    			AudioClip clip = SoundManager.Instance.GetClip(name);
    			if (clip != null)
    				PreviousDuration = clip.length;
    			StackSequence.AppendCallback(() => SoundManager.Instance.PlaySFX(name, volume, Category, Priority, this));
    			StackSequence.AppendInterval(clip.length);
    			return this;
            }

    		public void Stop()
            {
    			StackSequence.Kill(false);
            }
    	}

    	public SFXStack PlaySFXStack(string name, float volume = 1.0f, string category = "sfx", int priority = 0)
        {
    		AudioSource source = PlaySFX(name, volume, category, priority);
    		SFXStack stack;
    		if (source == null)
    		{ 
    			stack = new SFXStack() { StackSequence = DOTween.Sequence(), PreviousDuration = 0f };
    			spawnedStacks.Add(stack);
    			return stack;
    		}
    		float delay = source.clip.length;
    		Sequence s = DOTween.Sequence().AppendInterval(delay);
    		stack = new SFXStack() { StackSequence = s, PreviousDuration = delay, Category = category, Priority = priority };
    		spawnedStacks.Add(stack);
    		return stack;
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
    	public AudioClip GetClip(string name)
    	{
    		AudioDesc audio = System.Array.Find(Sounds, x => x.Name == name);
    		if (audio != null)
    		{
    			return audio.Clip;
    		}
    		return null;
    	}
    	public void ClearQueue()
    	{
    		queue.Clear();
    		queueTimer = 0f;
    	}
    	public void AddSFXToQueue(string name, float volume = 1.0f, string category = "sfx", int priority = 0)
    	{
    		if (queueTimer <= 0)
    		{ // Play instantly
    			AudioClip clip = GetClip(name);
    			if (clip == null)
                {
    				Debug.LogWarning("Can't find clip " + name + "!");
    				return;
                }
    			queueTimer = clip.length;
    			PlaySFX(name, volume, category, priority);
    			Debug.Log("Playing Instantly Queue + Delay: " + clip.length);
    		}
    		else
    		{
    			/*
    			if (priority != 0 && queue.Any(x => x.Category == category && x.Priority <= priority))
                {
    				queue = new Queue<AudioQueue>(queue.Where(x => x.Category != category || x.Priority == 0 || x.Priority > priority));
                }
    			*/
    			AudioDesc audioDesc = System.Array.Find(Sounds, x => x.Name == name);
    			if (audioDesc != null)
    			{
    				Debug.Log("Delaying queue");
    				queue.Enqueue(new AudioQueue() { Description = audioDesc, Volume = volume, IsSpatial = false, Category = category, Priority = priority });
    			}
    			else
    			{
    				Debug.LogWarning("Can't find clip " + name + "! To enquee");
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

    	public bool IsMusicPlaying()
        {
    		if (musicSources[currentMusicSource] == null)
    			return false;
    		return musicSources[currentMusicSource].isPlaying;
    	}

    	public string GetCurrentMusicName()
        {
    		return currentMusic;
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
    			currentMusic = name;
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

    			currentMusic = name;
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

    		crossfadeCoroutine = null;
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

    		fadeOutCoroutine = null;
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
    		float interpolation = 1.0f;
    		while (interpolation != 0f)
    		{
    			interpolation = Mathf.MoveTowards(interpolation, 0.0f, Time.deltaTime / duration);
    			fadeSource.volume = Mathf.Lerp(volume, sourceVolume, interpolation);
    			yield return null;
    		}

    		setMusicVolumeCoroutine = null;
    	}

    	int GetOtherMusicSource()
    	{
    		return (currentMusicSource + 1) % 2;
    	}

    #if UNITY_EDITOR
    	public void PerformTest()
        {
    		Debug.LogWarning("Doing Sound Test!");

    		Debug.LogWarning("Performing ");
    		for (int i = 0; i < Sounds.Length; i++)
            {
    			for (int j = i + 1; j < Sounds.Length; j++)
                {
    				if (Sounds[i].Name == Sounds[j].Name)
                    {
    					Debug.LogWarning("Found Duplicate " + Sounds[i].Name + " clip1: " + Sounds[i].Clip.name + " clip2: " + Sounds[j].Clip.name + " i: " + i + " j: " + j);
                    }

    				if (Sounds[i].Clip.name == Sounds[j].Clip.name)
    				{
    					Debug.LogWarning("Found Duplicate Clip " + Sounds[i].Clip.name + " clip1: " + Sounds[i].Name + " clip2: " + Sounds[j].Name + " i: " + i + " j: " + j);
    				}
    			}

    			if (!Sounds[i].Clip.name.Contains(Sounds[i].Name))
    				Debug.LogWarning("Different name and file name: " + Sounds[i].Clip.name + " : " + Sounds[i].Name + " i: " + i);
    		}
        }
    #endif
    }


}