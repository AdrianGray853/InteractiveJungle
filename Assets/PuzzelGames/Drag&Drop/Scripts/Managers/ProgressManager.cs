using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

    public class ProgressManager : MonoBehaviour
    {
        string savePath;
    	const string header = "@LETSAVE";
    	const int version = 2;

    	private static ProgressManager _instance = null;
    	public static ProgressManager Instance
    	{
    		get
    		{
    			if (_instance == null)
    			{
    				Instantiate(Resources.Load("Managers") as GameObject);
    			}
    			return _instance;
    		}
    		private set
    		{
    			_instance = value;
    		}
    	}

    	bool initialized = false;

    	class SaveData
    	{
    		public int UpperCaseLevel = 0; // Stores the last unlocked level
    		public int LowerCaseLevel = 0; // Stores the last unlocked level
    		public int VisitedMixLevels = 0; // Binary mask for visited levels in mixed levels.
    		public int ShownTutorials = 0; // Mask based on miniGames order (from left to right), Rocket = 0, Skate = 1, LetterPuzzle = 2

    		// Subscription
    		public long ExpirationDate = 0;

    		// Onboarding
    		public bool OnboardingShown = false;
    		public int SelectedAge = -1;

    		//public int DoneLevels = 0; // BitMask based on GameData.eGameType

    		public int CurrentVersion { get; private set; } // The version we want to be at, not the loaded version
    		public int LoadedVersion { get; private set; }

    		public bool HasData { get; private set; } = false;

    		public SaveData(int currentVersion) => CurrentVersion = currentVersion;

    		public void Reset()
            {
    			UpperCaseLevel = 0;
    			LowerCaseLevel = 0;
    			VisitedMixLevels = 0;
    			ShownTutorials = 0;

    			SelectedAge = -1;
    			OnboardingShown = false;
    			ExpirationDate = 0;
    		}

    		public bool Read(BinaryReader br)
            {
    			char[] rHeader = br.ReadChars(header.Length);
    			if (new string(rHeader) == header)
    			{
    				LoadedVersion = br.ReadInt32();

    				if (CurrentVersion != LoadedVersion)
    					Debug.LogWarning("Different save version! Upgrading!");

    				// Version 1+
    				UpperCaseLevel = TryReadInt32(br, 1, 0);
    				LowerCaseLevel = TryReadInt32(br, 1, 0);
    				VisitedMixLevels = TryReadInt32(br, 1, 0);
    				ShownTutorials = TryReadInt32(br, 1, 0);

    				ExpirationDate = TryReadInt64(br, 1, 0);
    				SelectedAge = TryReadInt32(br, 1, -1);

    				// Version 2+
    				OnboardingShown = TryReadBool(br, 2, false);

    				HasData = true;
    				return true;
    			}
    			return false;
    		}

    		private int TryReadInt32(BinaryReader br, int version = 1, int defaultValue = 0)
            {
    			if (CurrentVersion >= version)
    				return br.ReadInt32();
    			else
    				return defaultValue;
            }

    		private long TryReadInt64(BinaryReader br, int version = 1, long defaultValue = 0)
    		{
    			if (CurrentVersion >= version)
    				return br.ReadInt64();
    			else
    				return defaultValue;
    		}

    		private bool TryReadBool(BinaryReader br, int version = 1, bool defaultValue = false)
    		{
    			if (CurrentVersion >= version)
    				return br.ReadBoolean();
    			else
    				return defaultValue;
    		}

    		public bool Save(BinaryWriter bw)
            {
    			bw.Write(header.ToCharArray()); // header
    			bw.Write(CurrentVersion);
    			bw.Write(UpperCaseLevel);
    			bw.Write(LowerCaseLevel);
    			bw.Write(VisitedMixLevels);
    			bw.Write(ShownTutorials);

    			bw.Write(ExpirationDate);
    			bw.Write(SelectedAge);
    			bw.Write(OnboardingShown);
    			return true;
    		}
    	}
    	SaveData saveData = new SaveData(version);

    	// Unsaved data! Accessed directly, TODO: Maybe move these to GameData???
    	public int LastUnlockedLevelBigLetter = -1; // Used to show unlock bubble animation
    	public int LastUnlockedLevelSmallLetter = -1;

    	public int LastPlayedLevelBigLetter = -1; // Used to scroll to next level
    	public int LastPlayedLevelSmallLetter = -1; // Unclamped! Can be 'Z' + 1!

    	public int ReviewFinishedLetterCounter = 0; // Used for review request for now, counts how many letters were played in this session

    	int reviewLevelShown = 0;

    	void Awake()
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

    		savePath = Application.persistentDataPath + "/letterSave.dat";
    		Reset();
            initialized = Load();
    		if (!initialized)
    		{
    			Reset();
    			initialized = Save();
    		}
    	}

    	/*
    	private bool IsEOF(BinaryReader br)
        {
    		return (br.BaseStream.Position == br.BaseStream.Length);
        }
    	*/

    	public bool Load()
    	{
    		if (!File.Exists(savePath))
    		{
    			Debug.LogWarning("Save file doesn't exist!");
    			return false;
    		}

    		bool success = false;

    		try
    		{
    			using (BinaryReader br = new BinaryReader(File.Open(savePath, FileMode.Open), System.Text.Encoding.UTF8))
    			{
    				success = saveData.Read(br);
    			}
    		}
    		catch (IOException e)
    		{
    			Debug.LogError("Couldn't load the save file! " + e.Message);
    			Reset();
    		}

    		return success;
    	}

    	public bool Save()
    	{
    		bool success = false;
    		try
    		{
    			using (BinaryWriter bw = new BinaryWriter(File.Open(savePath,
    										FileMode.Create), System.Text.Encoding.UTF8, false))
    			{
    				success = saveData.Save(bw);
    			}
    		} 
    		catch (IOException e)
    		{
    			Debug.LogError("Couldn't write the save file! " + e.Message);
    		}

    		return success;
    	}

    	public void Reset()
    	{
    		saveData.Reset();

    		LastUnlockedLevelBigLetter = -1;
    		LastUnlockedLevelSmallLetter = -1;

    		LastPlayedLevelBigLetter = -1;
    		LastPlayedLevelSmallLetter = -1;
    	}
    	//---------------------------------------------------------------------------------------------------------
    	public void SetExpireDate(long time)
        {
    		saveData.ExpirationDate = time;
    		if (initialized)
    			Save();
    	}
    	public void SetSelectedAgeRange(int age)
        {
    		saveData.SelectedAge = age;
    		if (initialized)
    			Save();
    	}
    	public void UnlockLevel(GameData.eGameType gameType, int level)
    	{
    		switch (gameType)
    		{
    			case GameData.eGameType.UpperCase:
    				saveData.UpperCaseLevel = Mathf.Max(saveData.UpperCaseLevel, level);
    				break;
    			case GameData.eGameType.LowerCase:
    				saveData.LowerCaseLevel = Mathf.Max(saveData.LowerCaseLevel, level);
    				break;
    		}

    		if (initialized)
    			Save();
    	}

    	public int GetUnlockLevel(GameData.eGameType gameType)
    	{
    		switch (gameType)
    		{
    			case GameData.eGameType.UpperCase:
    				return saveData.UpperCaseLevel;
    			case GameData.eGameType.LowerCase:
    				return saveData.LowerCaseLevel;
    		}
    		return 0;
    	}

    	public void SetMixLevelVisited(int level)
        {
    		saveData.VisitedMixLevels |= 1 << level;

    		if (initialized)
    			Save();
        }

    	public bool IsMixLevelVisited(int level)
        {
    		return (saveData.VisitedMixLevels & (1 << level)) != 0;
        }

    	public void SetTutorialShown(int idx)
    	{
    		saveData.ShownTutorials |= 1 << idx;

    		if (initialized)
    			Save();
    	}

    	public bool IsTutorialShown(int idx)
    	{
    		return (saveData.ShownTutorials & (1 << idx)) != 0;
    	}

    	// Reviews:
    	// 0 - 2 Win/Death Minigames
    	// 3 - 8 MixLetters
    	// 9 - prima activitate la letters (drag word)
    	// 10 - la finisarea a doua litera

    	public void SetReviewShow(int level)
    	{
    		reviewLevelShown |= 1 << (level - 1);
    	}

    	public bool IsReviewShown(int level)
    	{
    		return (reviewLevelShown & 1 << (level - 1)) != 0;
    	}

    	public void SetOnboardingShown(bool shown)
        {
    		saveData.OnboardingShown = true;
    		if (initialized)
    			Save();
    	}

    	public bool IsOnboardingShown() => saveData.OnboardingShown;

    	/*
    	public void SetGameDone(GameData.eGameType gameType)
        {
    		saveData.DoneLevels |= 1 << (int)gameType;
    		if (initialized)
    			Save();
    	}

    	public bool GetGameDone(GameData.eGameType gameType)
        {
    		return (saveData.DoneLevels & (1 << (int)gameType)) != 0;
        }
    	*/
    }


}