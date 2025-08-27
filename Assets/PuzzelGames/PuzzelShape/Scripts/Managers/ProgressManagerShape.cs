using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using EModule = GameDataShape.eGameType;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EModule = GameDataShape.eGameType;

    public class ProgressManagerShape : MonoBehaviour
    {
    	public int maxUnlockedLevel = 200;
    	const string header = "@SHASAVE";
    	const int version = 3;

    	class SaveData
    	{
    		public int ScreenshotId = 0;
    		public int ColoringLevel = 0;
    		public int MemoryLevel = 0;
    		public int PuzzleLevel = 0;
    		public int EnvironmentLevel = 0;
    		public int ShapesLevel = 0;
    		public int ShapePuzzleLevel = 0;

    		public int DoneLevels = 0; // BitMask based on GameDataShape.eGameType

    		// Subscription
    		public long ExpirationDate = 0;

    		// Onboarding
    		public bool OnboardingShown = false;
    		public int SelectedAge = -1;
    	}


    	public static ProgressManagerShape Instance { get; private set; }


    	bool initialized;
    	string savePath;
    	int reviewLevelShown;
    	SaveData saveData = new();
    	Dictionary<EModule, int> totalLevelsMap = new();


    	void Awake()
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

    		savePath = Application.persistentDataPath + "/shapesSave.dat";
    		Reset();
    		initialized = Load();
    		if (!initialized)
    		{
    			Reset();
    			initialized = Save();
    		}
    	}

    	bool Load()
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
    				char[] rHeader = br.ReadChars(header.Length);
    				if (new string(rHeader) == header)
    				{
    					int rVersion = br.ReadInt32();
    					if (rVersion >= 2)
    					{
    						saveData.ScreenshotId = br.ReadInt32();
    						saveData.ColoringLevel = br.ReadInt32();
    						saveData.MemoryLevel = br.ReadInt32();
    						saveData.PuzzleLevel = br.ReadInt32();
    						saveData.EnvironmentLevel = br.ReadInt32();
    						saveData.ShapesLevel = br.ReadInt32();
    						saveData.ShapePuzzleLevel = br.ReadInt32();
    						saveData.DoneLevels = br.ReadInt32();
    					}
    					if (rVersion >= 3)
    					{
    						saveData.ExpirationDate = br.ReadInt64();
    						saveData.SelectedAge = br.ReadInt32();
    						saveData.OnboardingShown = br.ReadBoolean();
    					}

    					success = true;
    				}
    			}
    		}
    		catch (IOException e)
    		{
    			Debug.LogError("Couldn't load the save file! " + e.Message);
    			Reset();
    		}

    		return success;
    	}

    	bool Save()
    	{
    		bool success = false;
    		try
    		{
    			using (BinaryWriter bw = new BinaryWriter(File.Open(savePath,
    										FileMode.Create), System.Text.Encoding.UTF8, false))
    			{
    				bw.Write(header.ToCharArray()); // header
    				bw.Write(version);
    				bw.Write(saveData.ScreenshotId);
    				bw.Write(saveData.ColoringLevel);
    				bw.Write(saveData.MemoryLevel);
    				bw.Write(saveData.PuzzleLevel);
    				bw.Write(saveData.EnvironmentLevel);
    				bw.Write(saveData.ShapesLevel);
    				bw.Write(saveData.ShapePuzzleLevel);
    				bw.Write(saveData.DoneLevels);

    				// Version 3
    				bw.Write(saveData.ExpirationDate);
    				bw.Write(saveData.SelectedAge);
    				bw.Write(saveData.OnboardingShown);
    				success = true;
    			}
    		} 
    		catch (IOException e)
    		{
    			Debug.LogError("Couldn't write the save file! " + e.Message);
    		}

    		return success;
    	}

    	void Reset()
    	{
    		saveData.ScreenshotId = 0;
    		saveData.ColoringLevel = 0;
    		saveData.MemoryLevel = 0;
    		saveData.PuzzleLevel = 0;
    		saveData.EnvironmentLevel = 0;
    		saveData.ShapesLevel = 0;
    		saveData.ShapePuzzleLevel = 0;
    		saveData.DoneLevels = 0;

    		saveData.SelectedAge = -1;
    		saveData.OnboardingShown = false;
    		saveData.ExpirationDate = 0;
    	}

    	public int GetScreenShotId() => saveData.ScreenshotId;

    	public void SetScreenshotId(int id)
    	{
    		saveData.ScreenshotId = id;
    		if (initialized)
    			Save();
    	}

    	public void UnlockLevel(GameDataShape.eGameType gameType, int level)
    	{
    		switch (gameType)
    		{
    			case GameDataShape.eGameType.Shapes:
    				saveData.ShapesLevel = Mathf.Max(saveData.ShapesLevel, level); // Never go down!
    				break;
    			case GameDataShape.eGameType.Coloring:
    				saveData.ColoringLevel = Mathf.Max(saveData.ColoringLevel, level);
    				break;
    			case GameDataShape.eGameType.Puzzle:
    				saveData.PuzzleLevel = Mathf.Max(saveData.PuzzleLevel, level);
    				break;
    			case GameDataShape.eGameType.Environment:
    				saveData.EnvironmentLevel = Mathf.Max(saveData.EnvironmentLevel, level);
    				break;
    			case GameDataShape.eGameType.Memory:
    				saveData.MemoryLevel = Mathf.Max(saveData.MemoryLevel, level);
    				break;
    			case GameDataShape.eGameType.ShapePuzzle:
    				saveData.ShapePuzzleLevel = Mathf.Max(saveData.ShapePuzzleLevel, level);
    				break;
    		}

    		if (initialized)
    			Save();
    	}

    	public int GetUnlockLevel(GameDataShape.eGameType gameType)
    	{
    		switch (gameType)
    		{
    			case GameDataShape.eGameType.Shapes:
    				return maxUnlockedLevel;
    			case GameDataShape.eGameType.Coloring:
    				return maxUnlockedLevel;
    			case GameDataShape.eGameType.Puzzle:
    				return maxUnlockedLevel;
    			case GameDataShape.eGameType.Environment:
    				return maxUnlockedLevel;
    			case GameDataShape.eGameType.Memory:
    				return maxUnlockedLevel;
    			case GameDataShape.eGameType.ShapePuzzle:
    				return maxUnlockedLevel;
    		}
    		return 0;
    	}

    	public void SetGameDone(GameDataShape.eGameType gameType)
    	{
    		saveData.DoneLevels |= 1 << (int)gameType;
    		if (initialized)
    			Save();
    	}

    	public bool GetGameDone(GameDataShape.eGameType gameType)
    	{
    		return (saveData.DoneLevels & (1 << (int)gameType)) != 0;
    	}

    	public void SetReviewShow(GameDataShape.eGameType gameType)
    	{
    		reviewLevelShown |= 1 << (int)gameType;
    	}

    	public bool IsReviewShown(GameDataShape.eGameType gameType)
    	{
    		return (reviewLevelShown & 1 << (int)gameType) != 0;
    	}

    	public void SetOnboardingShown(bool shown)
    	{
    		saveData.OnboardingShown = shown;
    		if (initialized)
    			Save();
    	}

    	public bool IsOnboardingShown() => saveData.OnboardingShown;

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

    	public void SetModuleLevelsCount(EModule type, int count) => totalLevelsMap[type] = count;

    	public float GetModuleLevelsCompletionPercent(EModule type) => GetUnlockLevel(type) * 100f / totalLevelsMap[type];
    }


}