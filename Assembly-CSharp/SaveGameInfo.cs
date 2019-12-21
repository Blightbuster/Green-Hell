using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Enums;

public struct SaveGameInfo
{
	public string GetInfoText()
	{
		string text = "";
		Localization localization = GreenHellGame.Instance.GetLocalization();
		if (this.game_mode == GameMode.Story)
		{
			text = localization.Get("GameMode_Story", true) + " - ";
		}
		else if (this.game_mode == GameMode.Survival)
		{
			text = localization.Get("GameMode_Survival", true) + " - ";
		}
		if (this.perma_death)
		{
			text += localization.Get("DifficultyLevel_PermaDeath", true);
		}
		else
		{
			switch (this.game_difficulty)
			{
			case DifficultySettings.PresetType.Tourist:
				text += localization.Get("DifficultyLevel_Tourist", true);
				break;
			case DifficultySettings.PresetType.Easy:
				text += localization.Get("DifficultyLevel_Easy", true);
				break;
			case DifficultySettings.PresetType.Normal:
				text += localization.Get("DifficultyLevel_Normal", true);
				break;
			case DifficultySettings.PresetType.Hard:
				text += localization.Get("DifficultyLevel_Hard", true);
				break;
			case DifficultySettings.PresetType.PermaDeath:
				text += localization.Get("DifficultyLevel_PermaDeath", true);
				break;
			case DifficultySettings.PresetType.Custom:
				text += localization.Get("DifficultyLevel_Custom", true);
				break;
			}
		}
		text += " - ";
		if (this.tutorial)
		{
			text += localization.Get("Tutorial", true);
		}
		else
		{
			text += localization.Get("SaveGame_Day", true);
			text += ": ";
			text += this.days_survived.ToString();
		}
		if (this.perma_death && this.player_is_dead)
		{
			text += " - ";
			text += localization.Get("PermaDeath_Dead", true);
		}
		return text;
	}

	public static SaveGameInfo? ReadSaveSlot(int slot_id)
	{
		SaveGameInfo? result = SaveGameInfo.ReadSaveFile(SaveGame.SLOT_NAME + slot_id.ToString() + ".sav");
		if (result == null)
		{
			result = SaveGameInfo.ReadSaveFile(SaveGame.OLD_SLOT_NAME + slot_id.ToString() + ".sav");
		}
		return result;
	}

	public static SaveGameInfo? ReadSaveFile(string file_name)
	{
		if (!GreenHellGame.Instance.m_RemoteStorage.FileExistsInRemoteStorage(file_name))
		{
			return null;
		}
		int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(file_name);
		byte[] array = new byte[fileSize];
		if (GreenHellGame.Instance.m_RemoteStorage.FileRead(file_name, array, fileSize) == 0)
		{
			return null;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		SaveGameInfo saveGameInfo = default(SaveGameInfo);
		saveGameInfo.file_name = file_name;
		MemoryStream memoryStream = new MemoryStream(array);
		GameVersion lhs = new GameVersion((GameVersion)binaryFormatter.Deserialize(memoryStream));
		saveGameInfo.game_version = lhs;
		GameMode gameMode = (GameMode)binaryFormatter.Deserialize(memoryStream);
		saveGameInfo.game_mode = gameMode;
		long dateData = (long)binaryFormatter.Deserialize(memoryStream);
		saveGameInfo.date = DateTime.FromBinary(dateData);
		saveGameInfo.days_survived = (int)binaryFormatter.Deserialize(memoryStream) + 1;
		saveGameInfo.game_difficulty = (DifficultySettings.PresetType)binaryFormatter.Deserialize(memoryStream);
		saveGameInfo.tutorial = false;
		if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate8)
		{
			saveGameInfo.tutorial = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		saveGameInfo.perma_death = (saveGameInfo.game_difficulty == DifficultySettings.PresetType.PermaDeath);
		if (lhs == GreenHellGame.s_GameVersionEarlyAccessUpdate12)
		{
			saveGameInfo.perma_death = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		saveGameInfo.player_is_dead = false;
		if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
		{
			saveGameInfo.player_is_dead = (bool)binaryFormatter.Deserialize(memoryStream);
		}
		try
		{
			if (lhs >= GreenHellGame.s_GameVersionMasterShelters1_3)
			{
				saveGameInfo.played_coop = (bool)binaryFormatter.Deserialize(memoryStream);
			}
		}
		catch
		{
		}
		saveGameInfo.loadable = (!saveGameInfo.perma_death || !saveGameInfo.player_is_dead);
		memoryStream.Close();
		return new SaveGameInfo?(saveGameInfo);
	}

	public GameVersion game_version;

	public GameMode game_mode;

	public DateTime date;

	public int days_survived;

	public DifficultySettings.PresetType game_difficulty;

	public bool tutorial;

	public bool perma_death;

	public bool player_is_dead;

	public string file_name;

	public bool played_coop;

	public bool loadable;
}
