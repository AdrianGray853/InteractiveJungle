using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

    public class ProgressManagerTouch : MonoBehaviour
    {
        string savePath;
    	const string header = "@DRASAVE";
    	const int version = 1;

    	private static ProgressManagerTouch _instance = null;
    	public static ProgressManagerTouch Instance
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
    		public int ScreenshotId = 0;

    		public Dictionary<GameDataTouch.eGameType, HashSet<int>> UnlockedStickers;
    		public Dictionary<GameDataTouch.eGameType, HashSet<int>> BookStickers;

    		// Subscription
    		public long ExpirationDate = 0;

    		// Onboarding
    		public bool OnboardingShown = false;
    		public int SelectedAge = -1;

    		//public int DoneLevels = 0; // BitMask based on GameDataTouch.eGameType

    		public int CurrentVersion { get; private set; } // The version we want to be at, not the loaded version
    		public int LoadedVersion { get; private set; }

    		public bool HasData { get; private set; } = false;

    		public SaveData(int currentVersion) => CurrentVersion = currentVersion;

    		public void Reset()
            {
    			ScreenshotId = 0;

    			UnlockedStickers = new Dictionary<GameDataTouch.eGameType, HashSet<int>>();
    			BookStickers = new Dictionary<GameDataTouch.eGameType, HashSet<int>>();

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

    				ScreenshotId = TryReadInt32(br);

    				UnlockedStickers.Clear();
    				int UnlockedStickersCount = TryReadInt32(br);
    				for (int i = 0; i < UnlockedStickersCount; i++)
    				{
    					GameDataTouch.eGameType gameType = (GameDataTouch.eGameType)TryReadInt32(br);
    					int count = TryReadInt32(br);
    					HashSet<int> stickerIndices = new HashSet<int>(count);
    					for (int j = 0; j < count; j++)
    					{
    						int stickerIdx = TryReadInt32(br);
    						stickerIndices.Add(stickerIdx);
    					}
    					UnlockedStickers.Add(gameType, stickerIndices);
    				}

    				BookStickers.Clear();
    				int BookStickersCount = TryReadInt32(br);
    				for (int i = 0; i < BookStickersCount; i++)
    				{
    					GameDataTouch.eGameType gameType = (GameDataTouch.eGameType)TryReadInt32(br);
    					int count = TryReadInt32(br);
    					HashSet<int> stickerIndices = new HashSet<int>(count);
    					for (int j = 0; j < count; j++)
    					{
    						int stickerIdx = TryReadInt32(br);
    						stickerIndices.Add(stickerIdx);
    					}
    					BookStickers.Add(gameType, stickerIndices);
    				}

    				ExpirationDate = TryReadInt64(br, 1, 0);
    				SelectedAge = TryReadInt32(br, 1, -1);
    				OnboardingShown = TryReadBool(br, 1, false);

    				HasData = true;
    				return true;
    			}
    			return false;
    		}

    		public bool Write(BinaryWriter bw)
    		{
    			bw.Write(header.ToCharArray()); // header
    			bw.Write(CurrentVersion);

    			bw.Write(ScreenshotId);

    			// Clean the dic from empty values
    			UnlockedStickers = UnlockedStickers.Where(x => x.Value != null && x.Value.Count > 0).ToDictionary(x => x.Key, y => y.Value);
    			bw.Write(UnlockedStickers.Count);
    			foreach (var stickerList in UnlockedStickers)
    			{
    				bw.Write((int)stickerList.Key);
    				bw.Write(stickerList.Value.Count);
    				foreach (var sticker in stickerList.Value)
    					bw.Write(sticker);
    			}
    			// Clean the dic from empty values
    			BookStickers = BookStickers.Where(x => x.Value != null && x.Value.Count > 0).ToDictionary(x => x.Key, y => y.Value);
    			bw.Write(BookStickers.Count);
    			foreach (var stickerList in BookStickers)
    			{
    				bw.Write((int)stickerList.Key);
    				bw.Write(stickerList.Value.Count);
    				foreach (var sticker in stickerList.Value)
    					bw.Write(sticker);
    			}

    			bw.Write(ExpirationDate);
    			bw.Write(SelectedAge);
    			bw.Write(OnboardingShown);
    			return true;
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
    	}
    	SaveData saveData = new SaveData(version);

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

    		savePath = Application.persistentDataPath + "/drawingSave.dat";
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
    				success = saveData.Write(bw);
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
    	public void UnlockSticker(GameDataTouch.eGameType gameType, int sticker)
    	{
    		if (saveData.UnlockedStickers.ContainsKey(gameType))
    			saveData.UnlockedStickers[gameType].Add(sticker);
    		else
    			saveData.UnlockedStickers[gameType] = new HashSet<int>() { sticker };

    		if (initialized)
    			Save();
    	}

    	public bool IsStickerUnlocked(GameDataTouch.eGameType gameType, int sticker)
    	{
    		if (!saveData.UnlockedStickers.ContainsKey(gameType))
    			return false;

    		return saveData.UnlockedStickers[gameType].Contains(sticker);
    	}

    	public void AddToBookSticker(GameDataTouch.eGameType gameType, int sticker)
    	{
    		if (saveData.BookStickers.ContainsKey(gameType))
    			saveData.BookStickers[gameType].Add(sticker);
    		else
    			saveData.BookStickers[gameType] = new HashSet<int>() { sticker };

    		// Remove from Unlocked Stickers
    		if (saveData.UnlockedStickers.ContainsKey(gameType))
    			saveData.UnlockedStickers[gameType].Remove(sticker);

    		if (initialized)
    			Save();
    	}

    	public bool IsOnBookSticker(GameDataTouch.eGameType gameType, int sticker)
    	{
    		if (!saveData.BookStickers.ContainsKey(gameType))
    			return false;

    		return saveData.BookStickers[gameType].Contains(sticker);
    	}

    	/*
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
    	*/

    	public void SetOnboardingShown(bool shown)
        {
    		saveData.OnboardingShown = shown;
    		if (initialized)
    			Save();
    	}

    	public bool IsOnboardingShown() => saveData.OnboardingShown;

    	/*
    	public void SetGameDone(GameDataTouch.eGameType gameType)
        {
    		saveData.DoneLevels |= 1 << (int)gameType;
    		if (initialized)
    			Save();
    	}

    	public bool GetGameDone(GameDataTouch.eGameType gameType)
        {
    		return (saveData.DoneLevels & (1 << (int)gameType)) != 0;
        }
    	*/

    	public int GetScreenShotId() => saveData.ScreenshotId;
    	public void SetScreenshotId(int id)
    	{
    		saveData.ScreenshotId = id;
    		if (initialized)
    			Save();
    	}
    }


}