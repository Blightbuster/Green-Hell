using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public static class SaveGame
{
	public static void SetupObjects()
	{
		SaveGame.m_Objects.Clear();
		SaveGame.m_Objects.Add(DifficultySettings.Get());
		SaveGame.m_Objects.Add(DialogsManager.Get());
		SaveGame.m_Objects.Add(AIManager.Get());
		SaveGame.m_Objects.Add(EnemyAISpawnManager.Get());
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
		SaveGame.m_Objects.Add(StoryObjectivesManager.Get());
		SaveGame.m_Objects.Add(HUDObjectives.Get());
		SaveGame.m_Objects.Add(MenuNotepad.Get());
		SaveGame.m_Objects.Add(MapTab.Get());
		SaveGame.m_Objects.Add(Music.Get());
		SaveGame.m_Objects.Add(RainManager.Get());
		SaveGame.m_Objects.Add(SleepController.Get());
		SaveGame.m_Objects.Add(MainLevel.Instance);
		SaveGame.m_Objects.Add(ScenarioManager.Get());
		SaveGame.m_Objects.Add(InventoryBackpack.Get());
		SaveGame.m_Objects.Add(ReplicatedSessionState.Get());
	}

	public static void SetupObjectsCoop()
	{
		SaveGame.m_Objects.Clear();
		SaveGame.m_Objects.Add(ItemsManager.Get());
		SaveGame.m_Objects.Add(Player.Get());
		SaveGame.m_Objects.Add(PlayerConditionModule.Get());
		SaveGame.m_Objects.Add(PlayerInjuryModule.Get());
		SaveGame.m_Objects.Add(PlayerDiseasesModule.Get());
		SaveGame.m_Objects.Add(StatsManager.Get());
		SaveGame.m_Objects.Add(HintsManager.Get());
		SaveGame.m_Objects.Add(MenuNotepad.Get());
		SaveGame.m_Objects.Add(MapTab.Get());
		SaveGame.m_Objects.Add(SleepController.Get());
		SaveGame.m_Objects.Add(DifficultySettings.Get());
		SaveGame.m_Objects.Add(InventoryBackpack.Get());
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

	public static bool FValExist(string name)
	{
		float num = 0f;
		return SaveGame.m_FVals.TryGetValue(name, out num);
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

	public static bool BValExist(string name)
	{
		bool flag = false;
		return SaveGame.m_BVals.TryGetValue(name, out flag);
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

	public static bool LoadEnumVal<T>(string name, out T val, bool warn = false) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		int value;
		if (SaveGame.m_IVals.TryGetValue(name, out value))
		{
			val = (T)((object)Enum.ToObject(typeof(T), value));
			return true;
		}
		if (warn)
		{
			DebugUtils.Assert("[SaveGame::LoadEnumVal] Can't load value " + name + "!!!", true, DebugUtils.AssertType.Info);
		}
		val = default(T);
		return false;
	}

	public static void SaveVal(string name, int val)
	{
		if (SaveGame.m_IVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_IVals.Add(name, val);
	}

	public static void SaveVal(string name, float val)
	{
		if (SaveGame.m_FVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_FVals.Add(name, val);
	}

	public static void SaveVal(string name, bool val)
	{
		if (SaveGame.m_BVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_BVals.Add(name, val);
	}

	public static void SaveVal(string name, string val)
	{
		if (SaveGame.m_SVals.ContainsKey(name))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_SVals.Add(name, val);
	}

	public static void SaveVal(string name, Vector2 val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_FVals.Add(name + "x", val.x);
		SaveGame.m_FVals.Add(name + "y", val.y);
	}

	public static void SaveVal(string name, Vector3 val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "z"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_FVals.Add(name + "x", val.x);
		SaveGame.m_FVals.Add(name + "y", val.y);
		SaveGame.m_FVals.Add(name + "z", val.z);
	}

	public static void SaveVal(string name, Quaternion val)
	{
		if (SaveGame.m_FVals.ContainsKey(name + "x") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "y") || SaveGame.m_FVals.ContainsKey(name + "w"))
		{
			DebugUtils.Assert("[SaveGame::SaveVal] Can't save more than one value with name " + name + "!!!", true, DebugUtils.AssertType.Info);
			return;
		}
		SaveGame.m_FVals.Add(name + "x", val.x);
		SaveGame.m_FVals.Add(name + "y", val.y);
		SaveGame.m_FVals.Add(name + "z", val.z);
		SaveGame.m_FVals.Add(name + "w", val.w);
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
		if (GreenHellGame.Instance.IsGamescom())
		{
			return;
		}
		if (!ReplTools.IsPlayingAlone() && !ReplTools.AmIMaster())
		{
			SaveGame.SaveCoop();
			return;
		}
		Debug.Log("SAVE - " + save_name);
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
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, GreenHellGame.s_GameVersion);
		binaryFormatter.Serialize(memoryStream, GreenHellGame.Instance.m_GameMode);
		long num = DateTime.Now.ToBinary();
		binaryFormatter.Serialize(memoryStream, num);
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		binaryFormatter.Serialize(memoryStream, ivalue);
		int activePresetType = (int)DifficultySettings.GetActivePresetType();
		binaryFormatter.Serialize(memoryStream, activePresetType);
		binaryFormatter.Serialize(memoryStream, MainLevel.Instance.m_Tutorial);
		bool flag = Player.Get().IsDead();
		binaryFormatter.Serialize(memoryStream, flag);
		binaryFormatter.Serialize(memoryStream, ReplicatedSessionState.Get() != null && ReplicatedSessionState.Get().m_PlayedCoop);
		Stream serializationStream = memoryStream;
		P2PSession instance = P2PSession.Instance;
		binaryFormatter.Serialize(serializationStream, ((instance != null) ? instance.GetSessionId() : null) ?? "");
		binaryFormatter.Serialize(memoryStream, SaveGame.m_IVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_SVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_FVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_BVals);
		DebugUtils.Assert(GreenHellGame.Instance.m_RemoteStorage.FileWrite(save_name, memoryStream.GetBuffer()), "SaveGame - remote storage write failed", true, DebugUtils.AssertType.Info);
		memoryStream.Close();
		SaveGame.SaveScreenshot(save_name);
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void SaveCoop()
	{
		DebugUtils.Assert(P2PSession.Instance.GetSessionId().Length > 0, true);
		SaveGame.SaveCoop(P2PSession.Instance.GetSessionId());
	}

	public static void SaveCoop(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't save, state = " + SaveGame.m_State.ToString());
			return;
		}
		Debug.Log("SAVE - " + save_name);
		SaveGame.m_State = SaveGame.State.SaveCoop;
		HUDSaving.Get().Activate();
		SaveGame.SetupObjectsCoop();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		foreach (ISaveLoad saveLoad in SaveGame.m_Objects)
		{
			saveLoad.Save();
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, GreenHellGame.s_GameVersion);
		binaryFormatter.Serialize(memoryStream, GreenHellGame.Instance.m_GameMode);
		long num = DateTime.Now.ToBinary();
		binaryFormatter.Serialize(memoryStream, num);
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		binaryFormatter.Serialize(memoryStream, ivalue);
		int activePresetType = (int)DifficultySettings.GetActivePresetType();
		binaryFormatter.Serialize(memoryStream, activePresetType);
		binaryFormatter.Serialize(memoryStream, MainLevel.Instance.m_Tutorial);
		bool flag = Player.Get().IsDead();
		binaryFormatter.Serialize(memoryStream, flag);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_IVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_SVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_FVals);
		binaryFormatter.Serialize(memoryStream, SaveGame.m_BVals);
		DebugUtils.Assert(GreenHellGame.Instance.m_RemoteStorage.FileWrite(save_name, memoryStream.GetBuffer()), "SaveGame - remote storage write failed", true, DebugUtils.AssertType.Info);
		memoryStream.Close();
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void Load()
	{
		SaveGame.Load(SaveGame.s_MainSaveName, GameMode.None);
	}

	public static void Load(string save_name, GameMode game_mode = GameMode.None)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!GreenHellGame.Instance.FileExistsInRemoteStorage(save_name))
		{
			DebugUtils.Assert("SaveGame::Load - file doesn't exists in remote storage " + save_name, true, DebugUtils.AssertType.Info);
			return;
		}
		GreenHellGame.Instance.m_LoadingScreen.Show(LoadingScreenState.LoadSaveGame);
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.PreloadScheduled;
		SaveGame.s_MainSaveName = save_name;
		SaveGame.s_ExpectedGameMode = game_mode;
	}

	public static void PlayerLoad()
	{
		SaveGame.PlayerLoad(SaveGame.s_MainSaveName);
	}

	private static void PlayerLoad(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		Debug.Log("PLAYER_LOAD - " + save_name);
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(save_name);
		byte[] array = new byte[fileSize];
		GreenHellGame.Instance.m_RemoteStorage.FileRead(save_name, array, fileSize);
		MemoryStream memoryStream = new MemoryStream(array);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(memoryStream));
		SaveGame.m_SaveGameVersion = gameVersion;
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(memoryStream);
		long num = (long)binaryFormatter.Deserialize(memoryStream);
		int num2 = (int)binaryFormatter.Deserialize(memoryStream);
		int num3 = (int)binaryFormatter.Deserialize(memoryStream);
		if (gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && num3 >= 0 && num3 < 6)
		{
			DifficultySettings.SetActivePresetType((DifficultySettings.PresetType)num3);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate8)
		{
			bool flag = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate12 && gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && (bool)binaryFormatter.Deserialize(memoryStream))
		{
			DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.PermaDeath);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			bool flag2 = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		try
		{
			if (gameVersion >= GreenHellGame.s_GameVersionMasterShelters1_3)
			{
				bool flag3 = (bool)binaryFormatter.Deserialize(memoryStream);
				P2PSession.Instance.SetSessionId((string)binaryFormatter.Deserialize(memoryStream));
			}
		}
		catch
		{
		}
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(memoryStream);
		memoryStream.Close();
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
		if (!GreenHellGame.Instance.FileExistsInRemoteStorage(save_name))
		{
			DebugUtils.Assert("SaveGame::FullLoad - file doesn't exists in remote storage " + save_name, true, DebugUtils.AssertType.Info);
			return;
		}
		Debug.Log("FULL_LOAD - " + save_name);
		MainLevel.Instance.ResetGameBeforeLoad();
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.SetupObjects();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(save_name);
		byte[] array = new byte[fileSize];
		GreenHellGame.Instance.m_RemoteStorage.FileRead(save_name, array, fileSize);
		MemoryStream memoryStream = new MemoryStream(array);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(memoryStream));
		SaveGame.m_SaveGameVersion = gameVersion;
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(memoryStream);
		long num = (long)binaryFormatter.Deserialize(memoryStream);
		int num2 = (int)binaryFormatter.Deserialize(memoryStream);
		int num3 = (int)binaryFormatter.Deserialize(memoryStream);
		if (gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && num3 >= 0 && num3 < 6)
		{
			DifficultySettings.SetActivePresetType((DifficultySettings.PresetType)num3);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate8)
		{
			bool flag = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate12 && gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && (bool)binaryFormatter.Deserialize(memoryStream))
		{
			DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.PermaDeath);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			bool flag2 = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		try
		{
			if (gameVersion >= GreenHellGame.s_GameVersionMasterShelters1_3)
			{
				bool flag3 = (bool)binaryFormatter.Deserialize(memoryStream);
				P2PSession.Instance.SetSessionId((string)binaryFormatter.Deserialize(memoryStream));
			}
		}
		catch
		{
		}
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(memoryStream);
		memoryStream.Close();
		foreach (ISaveLoad saveLoad in SaveGame.m_Objects)
		{
			saveLoad.Load();
		}
		GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadWaitingForScenario;
	}

	public static void LoadCoop()
	{
		if (!ReplTools.IsPlayingAlone() && !ReplTools.AmIMaster())
		{
			SaveGame.LoadCoop(P2PSession.Instance.GetSessionId());
		}
	}

	public static void LoadCoop(string save_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			Debug.LogWarning("Can't load, state = " + SaveGame.m_State.ToString());
			return;
		}
		if (!GreenHellGame.Instance.FileExistsInRemoteStorage(save_name))
		{
			return;
		}
		Debug.Log("FULL_LOAD - " + save_name);
		MainLevel.Instance.ResetGameBeforeLoad();
		SaveGame.m_State = SaveGame.State.LoadCoop;
		SaveGame.SetupObjectsCoop();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(save_name);
		byte[] array = new byte[fileSize];
		GreenHellGame.Instance.m_RemoteStorage.FileRead(save_name, array, fileSize);
		MemoryStream memoryStream = new MemoryStream(array);
		GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(memoryStream));
		SaveGame.m_SaveGameVersion = gameVersion;
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(memoryStream);
		long num = (long)binaryFormatter.Deserialize(memoryStream);
		int num2 = (int)binaryFormatter.Deserialize(memoryStream);
		int num3 = (int)binaryFormatter.Deserialize(memoryStream);
		if (gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && num3 >= 0 && num3 < 6)
		{
			DifficultySettings.SetActivePresetType((DifficultySettings.PresetType)num3);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate8)
		{
			bool flag = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate12 && gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && (bool)binaryFormatter.Deserialize(memoryStream))
		{
			DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.PermaDeath);
		}
		if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			bool flag2 = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		SaveGame.m_IVals = (Dictionary<string, int>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_SVals = (Dictionary<string, string>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_FVals = (Dictionary<string, float>)binaryFormatter.Deserialize(memoryStream);
		SaveGame.m_BVals = (Dictionary<string, bool>)binaryFormatter.Deserialize(memoryStream);
		memoryStream.Close();
		foreach (ISaveLoad saveLoad in SaveGame.m_Objects)
		{
			saveLoad.Load();
		}
		SaveGame.m_State = SaveGame.State.None;
	}

	public static void UpdateFullLoadWaitingForScenario()
	{
		if (ScenarioManager.Get().LoadingCompleted())
		{
			GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadCompleted;
			SaveGame.m_State = SaveGame.State.None;
			MainLevel.Instance.OnFullLoadEnd();
		}
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
		Debug.Log("LOAD_PLAYER - " + save_name);
		SaveGame.m_State = SaveGame.State.Load;
		SaveGame.SetupObjects();
		SaveGame.m_IVals.Clear();
		SaveGame.m_SVals.Clear();
		SaveGame.m_FVals.Clear();
		SaveGame.m_BVals.Clear();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(Application.persistentDataPath + "/" + save_name, FileMode.Open);
		GameVersion lhs = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
		long num = (long)binaryFormatter.Deserialize(fileStream);
		int num2 = (int)binaryFormatter.Deserialize(fileStream);
		int num3 = (int)binaryFormatter.Deserialize(fileStream);
		if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate8)
		{
			bool flag = (bool)binaryFormatter.Deserialize(fileStream);
		}
		if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate12 && lhs < GreenHellGame.s_GameVersionEarlyAccessUpdate13 && (bool)binaryFormatter.Deserialize(fileStream))
		{
			DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.PermaDeath);
		}
		if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			bool flag2 = (bool)binaryFormatter.Deserialize(fileStream);
		}
		if (GreenHellGame.s_GameVersion >= GreenHellGame.s_GameVersionMasterShelters1_3)
		{
			BinaryFormatter binaryFormatter2 = binaryFormatter;
			Stream serializationStream = fileStream;
			P2PSession instance = P2PSession.Instance;
			binaryFormatter2.Serialize(serializationStream, ((instance != null) ? instance.GetSessionId() : null) ?? "");
		}
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

	public static void DeleteSave()
	{
		GreenHellGame.Instance.m_RemoteStorage.FileDelete(SaveGame.s_MainSaveName);
	}

	public static void SaveScreenshot(string save_name)
	{
		string text = save_name + ".png";
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.InGameMenu);
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 0f);
		Camera main = Camera.main;
		Transform transform = Camera.main.gameObject.transform.FindDeepChild("HorizonCamera");
		Camera camera = (transform != null) ? transform.GetComponent<Camera>() : null;
		Texture2D texture2D = new Texture2D(SaveGame.s_ScreenshotWidth, SaveGame.s_ScreenshotHeight, TextureFormat.RGB24, false);
		RenderTexture temporary = RenderTexture.GetTemporary(SaveGame.s_ScreenshotWidth, SaveGame.s_ScreenshotHeight, 24, RenderTextureFormat.ARGB32);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Camera camera2 = GreenHellGame.Instance.gameObject.AddComponent<Camera>();
		if (camera2 == null || camera == null)
		{
			if (camera2)
			{
				UnityEngine.Object.Destroy(camera2);
			}
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			return;
		}
		camera2.CopyFrom(camera);
		SaveGame.RenderCamera(camera2, temporary);
		camera2.CopyFrom(main);
		SaveGame.RenderCamera(camera2, temporary);
		UnityEngine.Object.Destroy(camera2);
		texture2D.ReadPixels(new Rect(0f, 0f, (float)SaveGame.s_ScreenshotWidth, (float)SaveGame.s_ScreenshotHeight), 0, 0);
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] data = texture2D.EncodeToPNG();
		Debug.Log(string.Format("Took screenshot to: {0}", text));
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, weight);
		GreenHellGame.Instance.m_RemoteStorage.FileWrite(text, data);
	}

	public static void LoadScreenshot(string save_name, RawImage image)
	{
		if (image == null)
		{
			return;
		}
		string text = save_name + ".png";
		if (GreenHellGame.Instance.m_RemoteStorage.FileExistsInRemoteStorage(text))
		{
			new BinaryFormatter();
			int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(text);
			byte[] data = new byte[fileSize];
			GreenHellGame.Instance.m_RemoteStorage.FileRead(text, data, fileSize);
			Texture2D texture2D = new Texture2D(SaveGame.s_ScreenshotWidth, SaveGame.s_ScreenshotHeight);
			texture2D.LoadImage(data);
			image.texture = texture2D;
		}
	}

	private static void RenderCamera(Camera camera, RenderTexture rt)
	{
		if (camera)
		{
			camera.enabled = false;
			camera.targetTexture = rt;
			camera.Render();
		}
	}

	public static SaveGame.State m_State = SaveGame.State.None;

	public static List<ISaveLoad> m_Objects = new List<ISaveLoad>();

	public static Dictionary<string, int> m_IVals = new Dictionary<string, int>();

	public static Dictionary<string, string> m_SVals = new Dictionary<string, string>();

	public static Dictionary<string, float> m_FVals = new Dictionary<string, float>();

	public static Dictionary<string, bool> m_BVals = new Dictionary<string, bool>();

	public static string s_MainSaveName = "GreenHell.sav";

	private static string s_DebugPlayerSaveName = "GreenHellDebugPlayer.sav";

	public static GameMode s_ExpectedGameMode = GameMode.None;

	public static readonly string OLD_SLOT_NAME = "Slot";

	public static readonly string SLOT_NAME = "NewSlot";

	public static GameVersion m_SaveGameVersion = new GameVersion(0, 0);

	public static readonly int s_ScreenshotWidth = 480;

	public static readonly int s_ScreenshotHeight = 290;

	public enum State
	{
		None,
		Save,
		SaveCoop,
		Load,
		LoadCoop
	}
}
