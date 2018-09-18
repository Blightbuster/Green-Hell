using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
	public void SaveSettings()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + "/" + this.m_SettingsFileName);
		binaryFormatter.Serialize(fileStream, GreenHellGame.s_GameVersion);
		binaryFormatter.Serialize(fileStream, this.m_Language);
		binaryFormatter.Serialize(fileStream, this.m_Volume);
		binaryFormatter.Serialize(fileStream, this.m_SoftShadows);
		binaryFormatter.Serialize(fileStream, this.m_ShadowsBlur);
		binaryFormatter.Serialize(fileStream, this.m_InvertMouseY);
		binaryFormatter.Serialize(fileStream, this.m_XSensitivity);
		binaryFormatter.Serialize(fileStream, this.m_YSensitivity);
		binaryFormatter.Serialize(fileStream, this.m_DialogsVolume);
		binaryFormatter.Serialize(fileStream, this.m_MusicVolume);
		binaryFormatter.Serialize(fileStream, this.m_EnviroVolume);
		binaryFormatter.Serialize(fileStream, this.m_GeneralVolume);
		InputsManager.Get().SaveSettings(binaryFormatter, fileStream);
		fileStream.Close();
	}

	public void LoadSettings()
	{
		if (File.Exists(Application.persistentDataPath + "/" + this.m_SettingsFileName))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open(Application.persistentDataPath + "/" + this.m_SettingsFileName, FileMode.Open);
			GameVersion lhs = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
			this.m_Language = (Language)binaryFormatter.Deserialize(fileStream);
			this.m_Volume = (float)binaryFormatter.Deserialize(fileStream);
			if (lhs < GreenHellGame.s_GameVersionEarlyAccessUpdate2)
			{
				this.m_Volume = General.DecibelToLinear(this.m_Volume);
			}
			this.m_SoftShadows = (bool)binaryFormatter.Deserialize(fileStream);
			this.m_ShadowsBlur = (GameSettings.OptionLevel)binaryFormatter.Deserialize(fileStream);
			if (lhs > GreenHellGame.s_GameVersionEarlyAcces)
			{
				this.m_InvertMouseY = (bool)binaryFormatter.Deserialize(fileStream);
				this.m_XSensitivity = (float)binaryFormatter.Deserialize(fileStream);
				this.m_YSensitivity = (float)binaryFormatter.Deserialize(fileStream);
				this.m_DialogsVolume = (float)binaryFormatter.Deserialize(fileStream);
				this.m_MusicVolume = (float)binaryFormatter.Deserialize(fileStream);
				this.m_EnviroVolume = (float)binaryFormatter.Deserialize(fileStream);
				this.m_GeneralVolume = (float)binaryFormatter.Deserialize(fileStream);
			}
			if (lhs >= GreenHellGame.s_GameVersionEarlyAccessUpdate3)
			{
				InputsManager.Get().LoadSettings(binaryFormatter, fileStream);
			}
			fileStream.Close();
		}
		this.ApplySettings();
	}

	public void ApplySettings()
	{
		this.ApplyLanguage();
	}

	private void ApplyLanguage()
	{
		List<Text> list = GameSettings.FindObjectsOfTypeAllInternal<Text>();
		Dictionary<Text, string> dictionary = new Dictionary<Text, string>();
		Localization localization = GreenHellGame.Instance.GetLocalization();
		SortedDictionary<string, string> localizedtexts = localization.GetLocalizedtexts();
		for (int i = 0; i < list.Count; i++)
		{
			foreach (KeyValuePair<string, string> keyValuePair in localizedtexts)
			{
				if (keyValuePair.Value == list[i].text)
				{
					Dictionary<Text, string> dictionary2 = dictionary;
					Text key = list[i];
					SortedDictionary<string, string>.Enumerator enumerator;
					KeyValuePair<string, string> keyValuePair2 = enumerator.Current;
					dictionary2[key] = keyValuePair2.Key;
					break;
				}
			}
		}
		localization.Reload();
		foreach (KeyValuePair<Text, string> keyValuePair3 in dictionary)
		{
			Text key2 = keyValuePair3.Key;
			Localization localization2 = localization;
			Dictionary<Text, string>.Enumerator enumerator2;
			KeyValuePair<Text, string> keyValuePair4 = enumerator2.Current;
			key2.text = localization2.Get(keyValuePair4.Value);
		}
	}

	public static List<T> FindObjectsOfTypeAllInternal<T>()
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			foreach (GameObject gameObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
			{
				list.AddRange(gameObject.GetComponentsInChildren<T>(true));
			}
		}
		return list;
	}

	private void OnSettingsLoaded()
	{
	}

	[HideInInspector]
	public Language m_Language;

	[HideInInspector]
	public float m_Volume = 0.5f;

	[HideInInspector]
	public float m_DialogsVolume = 1f;

	[HideInInspector]
	public float m_MusicVolume = 1f;

	[HideInInspector]
	public float m_EnviroVolume = 1f;

	[HideInInspector]
	public float m_GeneralVolume = 1f;

	[HideInInspector]
	public string m_SettingsFileName = "Settings.sav";

	[HideInInspector]
	public bool m_SoftShadows = true;

	[HideInInspector]
	public GameSettings.OptionLevel m_ShadowsBlur = GameSettings.OptionLevel.High;

	[HideInInspector]
	public bool m_InvertMouseY;

	[HideInInspector]
	public float m_XSensitivity = 1f;

	[HideInInspector]
	public float m_YSensitivity = 1f;

	public enum OptionLevel
	{
		Low,
		Medium,
		High,
		VeryHigh
	}
}
