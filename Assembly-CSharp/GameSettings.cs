using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CJTools;
using Enums;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
	[HideInInspector]
	public float m_BrightnessMul { get; private set; } = 1f;

	public P2PGameVisibility m_GameVisibility
	{
		get
		{
			return this.m_GameVisibilityInternal;
		}
		set
		{
			this.m_GameVisibilityInternal = value;
		}
	}

	public void SaveSettings()
	{
		if (GreenHellGame.Instance.IsGamescom())
		{
			return;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, GreenHellGame.s_GameVersion);
		binaryFormatter.Serialize(memoryStream, this.m_Language);
		binaryFormatter.Serialize(memoryStream, this.m_Volume);
		binaryFormatter.Serialize(memoryStream, this.m_SoftShadows);
		binaryFormatter.Serialize(memoryStream, this.m_ShadowsBlur);
		binaryFormatter.Serialize(memoryStream, this.m_InvertMouseY);
		binaryFormatter.Serialize(memoryStream, this.m_XSensitivity);
		binaryFormatter.Serialize(memoryStream, this.m_YSensitivity);
		binaryFormatter.Serialize(memoryStream, this.m_DialogsVolume);
		binaryFormatter.Serialize(memoryStream, this.m_MusicVolume);
		binaryFormatter.Serialize(memoryStream, this.m_EnviroVolume);
		binaryFormatter.Serialize(memoryStream, this.m_GeneralVolume);
		InputsManager.Get().SaveSettings(binaryFormatter, memoryStream);
		binaryFormatter.Serialize(memoryStream, this.m_Subtitles);
		binaryFormatter.Serialize(memoryStream, this.m_Crosshair);
		binaryFormatter.Serialize(memoryStream, this.m_Hints);
		binaryFormatter.Serialize(memoryStream, this.m_Resolution);
		binaryFormatter.Serialize(memoryStream, this.m_Fullscreen);
		binaryFormatter.Serialize(memoryStream, this.m_VSync);
		binaryFormatter.Serialize(memoryStream, this.m_ShadowDistance);
		binaryFormatter.Serialize(memoryStream, this.m_AntiAliasing);
		binaryFormatter.Serialize(memoryStream, this.m_FOVChange);
		binaryFormatter.Serialize(memoryStream, this.m_ObjectDrawDistance);
		binaryFormatter.Serialize(memoryStream, this.m_TextureQuality);
		binaryFormatter.Serialize(memoryStream, this.m_LastPlatformLanguage);
		binaryFormatter.Serialize(memoryStream, this.m_BrightnessMul);
		binaryFormatter.Serialize(memoryStream, this.m_NeverPlayed);
		binaryFormatter.Serialize(memoryStream, this.m_LookRotationSpeed);
		binaryFormatter.Serialize(memoryStream, this.m_GameVisibilityInternal);
		binaryFormatter.Serialize(memoryStream, this.m_ControllerType);
		binaryFormatter.Serialize(memoryStream, this.m_ToggleRunOption);
		binaryFormatter.Serialize(memoryStream, this.m_ToggleWatch);
		binaryFormatter.Serialize(memoryStream, this.m_ToggleCrouch);
		DebugUtils.Assert(GreenHellGame.Instance.m_RemoteStorage.FileWrite(GameSettings.s_SettingsFileName, memoryStream.GetBuffer()), "GameSettings::SaveSettings failed to save file.", true, DebugUtils.AssertType.Info);
		memoryStream.Close();
	}

	public void LoadSettings()
	{
		string text = GameSettings.s_SettingsFileName;
		if (!GreenHellGame.Instance.FileExistsInRemoteStorage(text))
		{
			text = GameSettings.s_SettingsOldFileName;
		}
		if (GreenHellGame.Instance.IsGamescom())
		{
			text = "GAMESCOM_SETTINGS.sav";
		}
		if (GreenHellGame.Instance.FileExistsInRemoteStorage(text))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			int fileSize = GreenHellGame.Instance.m_RemoteStorage.GetFileSize(text);
			byte[] array = new byte[fileSize];
			int num = GreenHellGame.Instance.m_RemoteStorage.FileRead(text, array, fileSize);
			if (num != fileSize)
			{
				if (num == 0)
				{
					Debug.LogError("Local file " + text + " is missing!!! Skipping reading data.");
				}
				else
				{
					Debug.LogError("Local file " + text + " size mismatch!!! Skipping reading data.");
				}
				GreenHellGame.Instance.m_RemoteStorage.FileForget(text);
			}
			else
			{
				MemoryStream memoryStream = new MemoryStream(array);
				GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(memoryStream));
				this.m_Language = (Language)binaryFormatter.Deserialize(memoryStream);
				this.m_Volume = (float)binaryFormatter.Deserialize(memoryStream);
				if (gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate2)
				{
					this.m_Volume = General.DecibelToLinear(this.m_Volume);
				}
				this.m_SoftShadows = (bool)binaryFormatter.Deserialize(memoryStream);
				this.m_ShadowsBlur = (GameSettings.OptionLevel)binaryFormatter.Deserialize(memoryStream);
				if (gameVersion > GreenHellGame.s_GameVersionEarlyAcces)
				{
					this.m_InvertMouseY = (bool)binaryFormatter.Deserialize(memoryStream);
					this.m_XSensitivity = (float)binaryFormatter.Deserialize(memoryStream);
					this.m_YSensitivity = (float)binaryFormatter.Deserialize(memoryStream);
					this.m_DialogsVolume = (float)binaryFormatter.Deserialize(memoryStream);
					this.m_MusicVolume = (float)binaryFormatter.Deserialize(memoryStream);
					this.m_EnviroVolume = (float)binaryFormatter.Deserialize(memoryStream);
					this.m_GeneralVolume = (float)binaryFormatter.Deserialize(memoryStream);
				}
				if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate3)
				{
					InputsManager.Get().LoadSettings(binaryFormatter, memoryStream, gameVersion);
				}
				if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate7 && gameVersion < GreenHellGame.s_GameVersionEarlyAccessUpdate13)
				{
					binaryFormatter.Deserialize(memoryStream);
					binaryFormatter.Deserialize(memoryStream);
				}
				if (gameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate13)
				{
					try
					{
						this.m_Subtitles = (SubtitlesSetting)binaryFormatter.Deserialize(memoryStream);
						this.m_Crosshair = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_Hints = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_Resolution = (string)binaryFormatter.Deserialize(memoryStream);
						this.m_Fullscreen = (string)binaryFormatter.Deserialize(memoryStream);
						this.m_VSync = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_ShadowDistance = (float)binaryFormatter.Deserialize(memoryStream);
						this.m_AntiAliasing = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_FOVChange = (float)binaryFormatter.Deserialize(memoryStream);
						this.m_ObjectDrawDistance = (float)binaryFormatter.Deserialize(memoryStream);
						this.m_TextureQuality = (string)binaryFormatter.Deserialize(memoryStream);
						this.m_LastPlatformLanguage = (Language)binaryFormatter.Deserialize(memoryStream);
						this.m_BrightnessMul = (float)binaryFormatter.Deserialize(memoryStream);
						this.m_NeverPlayed = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_LookRotationSpeed = (float)binaryFormatter.Deserialize(memoryStream);
					}
					catch
					{
					}
				}
				if (gameVersion >= GreenHellGame.s_GameVersionMasterHotfix3)
				{
					try
					{
						this.m_GameVisibilityInternal = (P2PGameVisibility)binaryFormatter.Deserialize(memoryStream);
					}
					catch
					{
					}
					try
					{
						this.m_ControllerType = (ControllerType)binaryFormatter.Deserialize(memoryStream);
					}
					catch
					{
					}
				}
				if (gameVersion >= GreenHellGame.s_GameVersionMasterController1_2)
				{
					try
					{
						this.m_ToggleRunOption = (GameSettings.ToggleRunOption)binaryFormatter.Deserialize(memoryStream);
						this.m_ToggleWatch = (bool)binaryFormatter.Deserialize(memoryStream);
						this.m_ToggleCrouch = (bool)binaryFormatter.Deserialize(memoryStream);
					}
					catch
					{
					}
				}
				memoryStream.Close();
			}
		}
		this.SetLanguage();
		this.ApplySettings(false);
	}

	private void SetLanguage()
	{
		if (GreenHellGame.GAMESCOM_DEMO)
		{
			this.m_Language = Language.English;
			this.m_LastPlatformLanguage = Language.English;
			return;
		}
		string steamUILanguage = SteamUtils.GetSteamUILanguage();
		Language language;
		if (steamUILanguage == "english")
		{
			language = Language.English;
		}
		else if (steamUILanguage == "french")
		{
			language = Language.French;
		}
		else if (steamUILanguage == "italian")
		{
			language = Language.Italian;
		}
		else if (steamUILanguage == "german")
		{
			language = Language.German;
		}
		else if (steamUILanguage == "spanish")
		{
			language = Language.Spanish;
		}
		else if (steamUILanguage == "tchinese")
		{
			language = Language.ChineseTraditional;
		}
		else if (steamUILanguage == "schinese")
		{
			language = Language.ChineseSimplyfied;
		}
		else if (steamUILanguage == "portuguese")
		{
			language = Language.Portuguese;
		}
		else if (steamUILanguage == "brazilian")
		{
			language = Language.PortugueseBrazilian;
		}
		else if (steamUILanguage == "russian")
		{
			language = Language.Russian;
		}
		else if (steamUILanguage == "polish")
		{
			language = Language.Polish;
		}
		else if (steamUILanguage == "japanese")
		{
			language = Language.Japanese;
		}
		else if (steamUILanguage == "koreana")
		{
			language = Language.Korean;
		}
		else if (steamUILanguage == "vietnamese")
		{
			language = Language.Vietnamese;
		}
		else if (steamUILanguage == "thai")
		{
			language = Language.Thai;
		}
		else if (steamUILanguage == "czech")
		{
			language = Language.Czech;
		}
		else if (steamUILanguage == "swedish")
		{
			language = Language.Swedish;
		}
		else if (steamUILanguage == "hungarian")
		{
			language = Language.Hungarian;
		}
		else
		{
			language = Language.English;
		}
		if (language != this.m_LastPlatformLanguage)
		{
			this.m_Language = language;
			this.m_LastPlatformLanguage = language;
		}
	}

	public void ApplySettings(bool apply_resolution = true)
	{
		if (this.m_LastAppliedLanguage != null)
		{
			Language? lastAppliedLanguage = this.m_LastAppliedLanguage;
			Language language = this.m_Language;
			if (lastAppliedLanguage.GetValueOrDefault() == language & lastAppliedLanguage != null)
			{
				goto IL_46;
			}
		}
		this.m_LastAppliedLanguage = new Language?(this.m_Language);
		this.ApplyLanguage();
		IL_46:
		QualitySettings.shadowDistance = this.m_ShadowDistance * this.m_MaxShadowDistance;
		this.ApplyAntiAliasing();
		this.ApplyFOVChange();
		this.ApplyObjectDrawDistance();
		if (this.m_TextureQuality == "High")
		{
			QualitySettings.masterTextureLimit = 0;
		}
		else if (this.m_TextureQuality == "Medium")
		{
			QualitySettings.masterTextureLimit = 1;
		}
		else if (this.m_TextureQuality == "Low")
		{
			QualitySettings.masterTextureLimit = 2;
		}
		this.SetBrightnessMul(this.m_BrightnessMul);
		this.ApplyTerrainSettings(-1);
		if (apply_resolution)
		{
			base.StartCoroutine("DelayedScreenSettingsChange", Time.unscaledTime);
		}
	}

	private IEnumerator DelayedScreenSettingsChange(float exec_time)
	{
		if (this.m_ApplyingSettings)
		{
			yield break;
		}
		this.m_ApplyingSettings = true;
		this.m_SettingsChangeTime = Time.realtimeSinceStartup;
		FullScreenMode full_screen;
		if (!Enum.TryParse<FullScreenMode>(this.m_Fullscreen, out full_screen))
		{
			full_screen = ((this.m_Fullscreen == "On") ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
		}
		FullScreenMode fullScreenMode = Screen.fullScreenMode;
		Resolution res = ResolutionExtension.SelectResolution(this.m_Resolution);
		if (!res.Equals(Screen.currentResolution, false))
		{
			Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) ? res.refreshRate : Screen.currentResolution.refreshRate);
			while (!res.Equals(Screen.currentResolution, false) && this.m_SettingsChangeTime > Time.realtimeSinceStartup - 1f)
			{
				yield return new WaitForEndOfFrame();
			}
			GreenHellGame.Instance.m_MenuResolutionX = res.width;
			GreenHellGame.Instance.m_MenuResolutionY = res.height;
			this.OnResolutionChanged(res.width, res.height);
			yield return new WaitForEndOfFrame();
		}
		this.m_SettingsChangeTime = Time.realtimeSinceStartup;
		if (Screen.fullScreenMode != full_screen)
		{
			Screen.fullScreenMode = full_screen;
			while (Screen.fullScreenMode != full_screen && this.m_SettingsChangeTime > Time.realtimeSinceStartup - 1f)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		this.m_SettingsChangeTime = Time.realtimeSinceStartup;
		if (QualitySettings.vSyncCount != 0 != this.m_VSync)
		{
			if (this.m_VSync)
			{
				QualitySettings.vSyncCount = 1;
			}
			else
			{
				QualitySettings.vSyncCount = 0;
			}
			while (QualitySettings.vSyncCount != 0 != this.m_VSync && this.m_SettingsChangeTime > Time.realtimeSinceStartup - 1f)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		this.m_ApplyingSettings = false;
		string text = "";
		if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen && Screen.currentResolution.refreshRate != res.refreshRate)
		{
			text += "\n";
			text += GreenHellGame.Instance.GetLocalization().Get("MenuOptions_Graphics_RefreshRate", true);
		}
		if (Screen.fullScreenMode != full_screen)
		{
			text += "\n";
			text += GreenHellGame.Instance.GetLocalization().Get("MenuOptions_Graphics_Fullscreen", true);
		}
		if (text.Length > 0 && !GreenHellGame.IsYesNoDialogActive())
		{
			text += "\n";
			GreenHellGame.GetYesNoDialog().Show(null, DialogWindowType.Ok, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().GetMixed("MenuGraphics_SettingsChangeFailed", new string[]
			{
				text
			}), MainLevel.Instance == null);
		}
		yield break;
		yield break;
	}

	public void ApplyObjectDrawDistance()
	{
		QualitySettings.lodBias = this.m_ObjectDrawDistance;
	}

	public void ApplyFOVChange()
	{
		Camera main = Camera.main;
		if (main)
		{
			CameraManager cameraManager = CameraManager.Get();
			Camera[] componentsInChildren = main.GetComponentsInChildren<Camera>();
			if (cameraManager)
			{
				float fieldOfView = cameraManager.GetDefaultFOV() + this.m_FOVChange * this.m_FOVMaxChange;
				main.fieldOfView = fieldOfView;
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i] != null)
					{
						componentsInChildren[i].fieldOfView = fieldOfView;
					}
				}
			}
		}
	}

	public void ApplyAntiAliasing()
	{
		Camera main = Camera.main;
		if (main)
		{
			PostProcessLayer component = main.GetComponent<PostProcessLayer>();
			if (component)
			{
				component.antialiasingMode = (this.m_AntiAliasing ? PostProcessLayer.Antialiasing.TemporalAntialiasing : PostProcessLayer.Antialiasing.None);
			}
		}
	}

	private void OnResolutionChanged(int width, int height)
	{
		MenuBase[] array = Resources.FindObjectsOfTypeAll<MenuBase>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnScreenResolutionChange(width, height);
		}
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
					keyValuePair = enumerator.Current;
					dictionary2[key] = keyValuePair.Key;
					break;
				}
			}
		}
		localization.Reload();
		foreach (KeyValuePair<Text, string> keyValuePair2 in dictionary)
		{
			Text key2 = keyValuePair2.Key;
			Localization localization2 = localization;
			Dictionary<Text, string>.Enumerator enumerator2;
			keyValuePair2 = enumerator2.Current;
			key2.text = localization2.Get(keyValuePair2.Value, true);
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

	public static event GameSettings.OnBrightnessChangedDel OnBrightnessChanged;

	public void SetBrightnessMul(float mul)
	{
		this.m_BrightnessMul = mul;
		GameSettings.OnBrightnessChangedDel onBrightnessChanged = GameSettings.OnBrightnessChanged;
		if (onBrightnessChanged == null)
		{
			return;
		}
		onBrightnessChanged(mul);
	}

	public void ApplyTerrainSettings(int val = -1)
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		int num = (val == -1) ? QualitySettings.GetQualityLevel() : val;
		if (activeTerrain)
		{
			activeTerrain.heightmapPixelError = CJTools.Math.GetProportionalClamp(70f, 20f, (float)num, 0f, 4f);
			if (num == 0)
			{
				activeTerrain.basemapDistance = 0f;
			}
			else
			{
				activeTerrain.basemapDistance = CJTools.Math.GetProportionalClamp(10f, 25f, (float)num, 1f, 4f);
			}
			activeTerrain.castShadows = false;
			ReliefTerrain component = activeTerrain.gameObject.GetComponent<ReliefTerrain>();
			ReliefTerrainGlobalSettingsHolder reliefTerrainGlobalSettingsHolder = (component != null) ? component.globalSettingsHolder : null;
			if (reliefTerrainGlobalSettingsHolder != null)
			{
				reliefTerrainGlobalSettingsHolder.DIST_STEPS = CJTools.Math.GetProportionalClamp(4f, 25f, (float)num, 0f, 4f);
				reliefTerrainGlobalSettingsHolder.WAVELENGTH = CJTools.Math.GetProportionalClamp(16f, 2.5f, (float)num, 0f, 4f);
				reliefTerrainGlobalSettingsHolder.SHADOW_STEPS = CJTools.Math.GetProportionalClamp(0f, 25f, (float)num, 0f, 4f);
				reliefTerrainGlobalSettingsHolder.WAVELENGTH_SHADOWS = CJTools.Math.GetProportionalClamp(16f, 0.5f, (float)num, 0f, 4f);
				reliefTerrainGlobalSettingsHolder.distance_start = CJTools.Math.GetProportionalClamp(5f, 15f, (float)num, 0f, 4f);
				reliefTerrainGlobalSettingsHolder.distance_transition = CJTools.Math.GetProportionalClamp(5f, 10f, (float)num, 0f, 4f);
				if (reliefTerrainGlobalSettingsHolder.distance_start_bumpglobal < reliefTerrainGlobalSettingsHolder.distance_start)
				{
					reliefTerrainGlobalSettingsHolder.distance_start_bumpglobal = reliefTerrainGlobalSettingsHolder.distance_start;
				}
				RTP_LODmanager rtp_LODmanagerScript = reliefTerrainGlobalSettingsHolder.Get_RTP_LODmanagerScript();
				if (rtp_LODmanagerScript)
				{
					TerrainShaderLod rtp_LODlevel = rtp_LODmanagerScript.RTP_LODlevel;
					if (num == 0)
					{
						rtp_LODmanagerScript.RTP_LODlevel = TerrainShaderLod.SIMPLE;
					}
					else if (num == 1)
					{
						rtp_LODmanagerScript.RTP_LODlevel = TerrainShaderLod.PM;
					}
					else
					{
						rtp_LODmanagerScript.RTP_LODlevel = TerrainShaderLod.POM;
					}
					if (rtp_LODlevel != rtp_LODmanagerScript.RTP_LODlevel)
					{
						rtp_LODmanagerScript.RefreshLODlevel();
					}
				}
				reliefTerrainGlobalSettingsHolder.RefreshAll();
			}
		}
	}

	public void ApplyTerrainLODSettings(int val = -1)
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		RTP_LODmanager rtp_LODmanager;
		if (activeTerrain == null)
		{
			rtp_LODmanager = null;
		}
		else
		{
			ReliefTerrain component = activeTerrain.gameObject.GetComponent<ReliefTerrain>();
			rtp_LODmanager = ((component != null) ? component.globalSettingsHolder.Get_RTP_LODmanagerScript() : null);
		}
		RTP_LODmanager rtp_LODmanager2 = rtp_LODmanager;
		if (rtp_LODmanager2)
		{
			switch (val)
			{
			case 0:
				rtp_LODmanager2.RTP_LODlevel = TerrainShaderLod.SIMPLE;
				break;
			case 1:
				rtp_LODmanager2.RTP_LODlevel = TerrainShaderLod.PM;
				break;
			case 2:
				rtp_LODmanager2.RTP_LODlevel = TerrainShaderLod.POM;
				break;
			}
			rtp_LODmanager2.RefreshLODlevel();
		}
	}

	[HideInInspector]
	public Language m_Language;

	private Language m_LastPlatformLanguage;

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
	public static readonly string s_SettingsFileName = "Settings2.sav";

	[HideInInspector]
	public static readonly string s_SettingsOldFileName = "Settings.sav";

	[HideInInspector]
	public bool m_SoftShadows = true;

	[HideInInspector]
	public GameSettings.OptionLevel m_ShadowsBlur = GameSettings.OptionLevel.High;

	[HideInInspector]
	public GameSettings.ToggleRunOption m_ToggleRunOption;

	[HideInInspector]
	public bool m_ToggleCrouch;

	[HideInInspector]
	public bool m_ToggleWatch;

	[HideInInspector]
	public bool m_InvertMouseY;

	[HideInInspector]
	public float m_XSensitivity = 1f;

	[HideInInspector]
	public float m_YSensitivity = 1f;

	[HideInInspector]
	public SubtitlesSetting m_Subtitles = SubtitlesSetting.On;

	[HideInInspector]
	public bool m_Crosshair = true;

	[HideInInspector]
	public bool m_Hints = true;

	[HideInInspector]
	public string m_Resolution = "None";

	[HideInInspector]
	public string m_Fullscreen = "On";

	[HideInInspector]
	public bool m_VSync;

	[HideInInspector]
	public float m_ShadowDistance = 0.75f;

	[HideInInspector]
	public float m_MaxShadowDistance = 125f;

	[HideInInspector]
	public bool m_AntiAliasing = true;

	[HideInInspector]
	public float m_FOVChange;

	[HideInInspector]
	public float m_FOVMaxChange = 10f;

	[HideInInspector]
	public float m_ObjectDrawDistance = 1.1f;

	[HideInInspector]
	public string m_TextureQuality = "High";

	[HideInInspector]
	public float m_LookRotationSpeed = 50f;

	[HideInInspector]
	public bool m_NeverPlayed = true;

	private P2PGameVisibility m_GameVisibilityInternal;

	[HideInInspector]
	public ControllerType m_ControllerType;

	private Language? m_LastAppliedLanguage;

	private bool m_ApplyingSettings;

	private float m_SettingsChangeTime;

	public enum OptionLevel
	{
		Low,
		Medium,
		High,
		VeryHigh
	}

	public enum ToggleRunOption
	{
		No,
		Yes,
		Always
	}

	public delegate void OnBrightnessChangedDel(float mul);
}
