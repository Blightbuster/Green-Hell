using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AIs;
using Enums;
using UnityEngine;

public static class SaveGame
{
	public static void SetupObjects()
	{
		SaveGame.m_Objects.Clear();
		SaveGame.m_Objects.Add(MainLevel.Instance);
		SaveGame.m_Objects.Add(Scenario.Get());
		SaveGame.m_Objects.Add(AIManager.Get());
		SaveGame.m_Objects.Add(TriggersManager.Get());
		SaveGame.m_Objects.Add(ItemsManager.Get());
		SaveGame.m_Objects.Add(SensorManager.Get());
		SaveGame.m_Objects.Add(ConstructionGhostManager.Get());
		SaveGame.m_Objects.Add(StaticObjectsManager.Get());
		SaveGame.m_Objects.Add(Player.Get());
		SaveGame.m_Objects.Add(PlayerConditionModule.Get());
		SaveGame.m_Objects.Add(PlayerInjuryModule.Get());
		SaveGame.m_Objects.Add(PlayerDiseasesModule.Get());
		SaveGame.m_Objects.Add(StatsManager.Get());
		SaveGame.m_Objects.Add(HintsManager.Get());
		SaveGame.m_Objects.Add(ObjectivesManager.Get());
		SaveGame.m_Objects.Add(HUDObjectives.Get());
		SaveGame.m_Objects.Add(MenuNotepad.Get());
		SaveGame.m_Objects.Add(MapTab.Get());
		SaveGame.m_Objects.Add(Music.Get());
		SaveGame.m_Objects.Add(RainManager.Get());
		SaveGame.m_Objects.Add(BalanceSystem.Get());
	}

	public static bool LoadVal(string name, out int val, bool warn = false)
	{
		if (!SaveGame.m_IVals.TryGetValue(name, out val))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static int LoadIVal(string name)
	{
		int result = 0;
		SaveGame.m_IVals.TryGetValue(name, out result);
		return result;
	}

	public static bool LoadVal(string name, out float val, bool warn = false)
	{
		if (!SaveGame.m_FVals.TryGetValue(name, out val))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static float LoadFVal(string name)
	{
		float result = 0f;
		SaveGame.m_FVals.TryGetValue(name, out result);
		return result;
	}

	public static bool LoadVal(string name, out string val, bool warn = false)
	{
		if (!SaveGame.m_SVals.TryGetValue(name, out val))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static string LoadSVal(string name)
	{
		string empty = string.Empty;
		SaveGame.m_SVals.TryGetValue(name, out empty);
		return empty;
	}

	public static bool LoadVal(string name, out bool val, bool warn = false)
	{
		if (!SaveGame.m_BVals.TryGetValue(name, out val))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static bool LoadBVal(string name)
	{
		bool result = false;
		SaveGame.m_BVals.TryGetValue(name, out result);
		return result;
	}

	public static bool LoadVal(string name, out Vector2 val, bool warn = false)
	{
		val = Vector3.zero;
		if (!SaveGame.m_FVals.TryGetValue(name + "x", out val.x))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "y", out val.y))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static Vector2 LoadV2Val(string name)
	{
		Vector2 zero = Vector2.zero;
		zero.x = SaveGame.LoadFVal(name + "x");
		zero.y = SaveGame.LoadFVal(name + "y");
		return zero;
	}

	public static bool LoadVal(string name, out Vector3 val, bool warn = false)
	{
		val = Vector3.zero;
		if (!SaveGame.m_FVals.TryGetValue(name + "x", out val.x))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "y", out val.y))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "z", out val.z))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static Vector3 LoadV3Val(string name)
	{
		Vector3 zero = Vector3.zero;
		zero.x = SaveGame.LoadFVal(name + "x");
		zero.y = SaveGame.LoadFVal(name + "y");
		zero.z = SaveGame.LoadFVal(name + "z");
		return zero;
	}

	public static bool LoadVal(string name, out Quaternion val, bool warn = false)
	{
		val = Quaternion.identity;
		if (!SaveGame.m_FVals.TryGetValue(name + "x", out val.x))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "y", out val.y))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "z", out val.z))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		if (!SaveGame.m_FVals.TryGetValue(name + "w", out val.w))
		{
			if (warn)
			{
				DebugUtils.Assert("[SaveGame::LoadVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
			}
			return false;
		}
		return true;
	}

	public static Quaternion LoadQVal(string name)
	{
		Quaternion identity = Quaternion.identity;
		identity.x = SaveGame.LoadFVal(name + "x");
		identity.y = SaveGame.LoadFVal(name + "y");
		identity.z = SaveGame.LoadFVal(name + "z");
		identity.w = SaveGame.LoadFVal(name + "w");
		return identity;
	}

	public static void SaveVal(string name, int val)
	{
		if (SaveGame.m_IVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_IVals.Add(name, val);
		}
	}

	public static void SaveVal(string name, float val)
	{
		if (SaveGame.m_FVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_FVals.Add(name, val);
		}
	}

	public static void SaveVal(string name, bool val)
	{
		if (SaveGame.m_BVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_BVals.Add(name, val);
		}
	}

	public static void SaveVal(string name, string val)
	{
		if (SaveGame.m_SVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_SVals.Add(name, val);
		}
	}

	public static void SaveVal(string name, Vector2 val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_FVals.Add(name + "x", val.x);
			SaveGame.m_FVals.Add(name + "y", val.y);
		}
	}

	public static void SaveVal(string name, Vector3 val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "z"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_FVals.Add(name + "x", val.x);
			SaveGame.m_FVals.Add(name + "y", val.y);
			SaveGame.m_FVals.Add(name + "z", val.z);
		}
	}

	public static void SaveVal(string name, Quaternion val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "w"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			SaveGame.m_FVals.Add(name + "x", val.x);
			SaveGame.m_FVals.Add(name + "y", val.y);
			SaveGame.m_FVals.Add(name + "z", val.z);
			SaveGame.m_FVals.Add(name + "w", val.w);
		}
	}

	public static void Save()
	{
		SaveGame.Save(SaveGame.s_MainSaveName);
	}

	public static void Save(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't save, state = " + SaveGame.m_State.ToString());
			return;
		}
		SaveGame.m_State = SaveGame.State.Save;
		HUDSaving.Get().Activate();
		SaveGame.SetupObjects();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		foreach (ISaveLoad saveLoad in SaveGame.m_Objects)
		{
			saveLoad.Save();
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + "/" + save_name);
		binaryFormatter.Serialize(fileStream, GreenHellGame.s_GameVersion);
		binaryFormatter.Serialize(fileStream, GreenHellGame.Instance.m_GameMode);
		long num = DateTime.Now.ToBinary();
		binaryFormatter.Serialize(fileStream, num);
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		binaryFormatter.Serialize(fileStream, ivalue);
		int gameDifficulty = (int)GreenHellGame.Instance.m_GameDifficulty;
		binaryFormatter.Serialize(fileStream, gameDifficulty);
		binaryFormatter.Serialize(fileStream, SaveGame.m_IVals);
		binaryFormatter.Serialize(fileStream, SaveGame.m_SVals);
		binaryFormatter.Serialize(fileStream, SaveGame.m_FVals);
		binaryFormatter.Serialize(fileStream, SaveGame.m_BVals);
		fileStream.Close();
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void Load()
	{
		SaveGame.Load(SaveGame.s_MainSaveName);
	}

	public static void Load(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!File.Exists(Application.persistentDataPath + "/" + save_name))
		{
			return;
		}
		GreenHellGame.Instance.m_LoadingScreen.Show(LoadingScreenState.LoadSaveGame);
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.PreloadScheduled;
		SaveGame.s_MainSaveName = save_name;
	}

	public static void PreLoad()
	{
		SaveGame.PreLoad(SaveGame.s_MainSaveName);
	}

	private static void PreLoad(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + "/" + save_name, FileMode.Open);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
		long num = (long)binaryFormatter.Deserialize(fileStream);
		int num2 = (int)binaryFormatter.Deserialize(fileStream);
		int num3 = (int)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(fileStream);
		fileStream.Close();
		Player.Get().Load();
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.PreloadCompleted;
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void FullLoad()
	{
		SaveGame.FullLoad(SaveGame.s_MainSaveName);
	}

	public static void FullLoad(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!File.Exists(Application.persistentDataPath + "/" + save_name))
		{
			return;
		}
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.SetupObjects();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + "/" + save_name, FileMode.Open);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
		long num = (long)binaryFormatter.Deserialize(fileStream);
		int num2 = (int)binaryFormatter.Deserialize(fileStream);
		int num3 = (int)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(fileStream);
		fileStream.Close();
		foreach (ISaveLoad saveLoad in SaveGame.m_Objects)
		{
			saveLoad.Load();
		}
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadCompleted;
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void LoadPlayer()
	{
		SaveGame.LoadPlayer(SaveGame.s_DebugPlayerSaveName);
	}

	public static void LoadPlayer(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load player, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!File.Exists(Application.persistentDataPath + "/" + save_name))
		{
			return;
		}
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.SetupObjects();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + "/" + save_name, FileMode.Open);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
		long num = (long)binaryFormatter.Deserialize(fileStream);
		int num2 = (int)binaryFormatter.Deserialize(fileStream);
		int num3 = (int)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(fileStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(fileStream);
		fileStream.Close();
		Player.Get().Load();
		PlayerInjuryModule.Get().Load();
		PlayerConditionModule.Get().Load();
		PlayerDiseasesModule.Get().Load();
		ItemsManager.Get().Load();
		MainLevel.Instance.Load();
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadCompleted;
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void LoadInfo(string save_name, ref int days_survived, ref int difficulty)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load player, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!File.Exists(Application.persistentDataPath + "/" + save_name))
		{
			return;
		}
		SaveGame.m_State = SaveGame.State.Load;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + "/" + save_name, FileMode.Open);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
		long num = (long)binaryFormatter.Deserialize(fileStream);
		days_survived = (int)binaryFormatter.Deserialize(fileStream);
		difficulty = (int)binaryFormatter.Deserialize(fileStream);
		fileStream.Close();
		SaveGame.m_State = SaveGame.State.None;
	}

	public static SaveGame.State m_State = SaveGame.State.None;

	public static List<ISaveLoad> m_Objects = new List<ISaveLoad>();

	public static Dictionary<string, int> m_IVals = new Dictionary<string, int>();

	public static Dictionary<string, string> m_SVals = new Dictionary<string, string>();

	public static Dictionary<string, float> m_FVals = new Dictionary<string, float>();

	public static Dictionary<string, bool> m_BVals = new Dictionary<string, bool>();

	public static string s_MainSaveName = "GreenHell.sav";

	private static string s_DebugPlayerSaveName = "GreenHellDebugPlayer.sav";

	public enum State
	{
		None,
		Save,
		Load
	}
}
