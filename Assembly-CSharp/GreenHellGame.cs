using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CJTools;
using Enums;
using Steamworks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GreenHellGame : MonoBehaviour, IYesNoDialogOwner
{
	public static bool GAMESCOM_DEMO { get; private set; } = false;

	public static string GetBuildVersionFile()
	{
		return "Scripts/BuildVersion";
	}

	public static string GetBuildVersion()
	{
		TextAsset textAsset = Resources.Load<TextAsset>(GreenHellGame.GetBuildVersionFile());
		if (textAsset)
		{
			return textAsset.text;
		}
		return "DEV";
	}

	public event GreenHellGame.OnAudioSnapshotChangedDel OnAudioSnapshotChangedEvent;

	public static GreenHellGame Instance
	{
		get
		{
			if (GreenHellGame.s_AppQuitting)
			{
				return null;
			}
			if (GreenHellGame.s_Instance == null)
			{
				GreenHellGame.s_Instance = new GameObject("GreenHellGame").AddComponent<GreenHellGame>();
				GreenHellGame.s_Instance.Initialize();
			}
			return GreenHellGame.s_Instance;
		}
	}

	private void Initialize()
	{
		GreenHellGame.ROADSHOW_DEMO = File.Exists(Application.dataPath + "/Resources/scripts/Debug/ROADSHOW_DEMO");
		GreenHellGame.DEBUG = File.Exists(Application.dataPath + "/Resources/scripts/Debug/DEBUG");
		GreenHellGame.TWITCH_DEMO = File.Exists(Application.dataPath + "/Resources/scripts/Debug/TWITCH_DEMO");
		GreenHellGame.FORCE_SURVIVAL = File.Exists(Application.dataPath + "/Resources/scripts/Debug/FORCE_SURVIVAL");
		GreenHellGame.GAMESCOM_DEMO = File.Exists(Application.dataPath + "/Resources/scripts/Debug/GAMESCOM_DEMO");
		this.InitializeSteam();
		this.InitializeRemoteStorage();
		this.TryToMoveSavesToRemoteStorage();
		this.TryToMoveSettingsToRemoteStorage();
		this.InitInputsManager();
		this.InitAudioMixer();
		this.m_Settings = base.gameObject.AddComponent<GameSettings>();
		this.m_Settings.LoadSettings();
		this.SetMaxDeltaTime();
		this.InitMusic();
		this.InitPrefabsMap();
		this.CreateLocalization();
		this.InitializeLoadingScreen();
		if (GreenHellGame.m_FadeSystem == null)
		{
			GreenHellGame.CreateFadeSystem();
		}
		if (GreenHellGame.m_YesNoDialog == null)
		{
			GreenHellGame.CreateYesNoDialog();
		}
		if (GreenHellGame.m_LocalReplaceManager == null)
		{
			GreenHellGame.CreateLocalizationReplaceManager();
		}
		if (base.gameObject.scene.IsValid())
		{
			SceneManager.MoveGameObjectToScene(base.gameObject, SceneManager.GetActiveScene());
		}
		CursorManager.Initialize();
		CursorManager.Get().ShowCursor(false, false);
		this.InitializeChallengesManager();
		this.m_Settings.ApplySettings(false);
		this.InitScenarioScenes();
		this.m_SessionJoinHelper = base.gameObject.AddComponent<SessionJoinHelper>();
	}

	private void InitScenarioScenes()
	{
		base.gameObject.AddComponent<SceneLoadUnloadRequestHolder>();
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("ScenarioScenes", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Scene")
			{
				this.m_ScenarioScenes.Add(key.GetVariable(0).SValue);
			}
		}
	}

	private void InitPrefabsMap()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("PrefabsMap.txt", true);
		if (File.Exists(Application.dataPath + "/Resources/scripts/Debug/LoadAllPrefabs"))
		{
			this.m_LoadAllPrefabs = true;
			Debug.Log("All prefabs true");
		}
		else
		{
			Debug.Log("All prefabs false");
		}
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			this.m_PrefabsPathMap[key.GetVariable(0).SValue] = key.GetVariable(1).SValue;
			if (this.m_LoadAllPrefabs)
			{
				this.m_AllPrefabsMap[key.GetVariable(0).SValue] = (Resources.Load(key.GetVariable(1).SValue) as GameObject);
			}
		}
	}

	public GameObject GetPrefab(string name)
	{
		if (!this.m_PrefabsPathMap.ContainsKey(name.ToLower()))
		{
			CJDebug.Log("GreenHellGame::GetPrefab prefab '" + name + "' doesn't exist in map");
			return null;
		}
		GameObject result = null;
		if (this.m_LoadAllPrefabs)
		{
			this.m_AllPrefabsMap.TryGetValue(name.ToLower(), out result);
		}
		else
		{
			result = (Resources.Load(this.m_PrefabsPathMap[name.ToLower()]) as GameObject);
		}
		return result;
	}

	public void OnApplicationQuit()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		GreenHellGame.s_Instance = null;
		GreenHellGame.s_AppQuitting = true;
	}

	public bool IsGamescom()
	{
		return GreenHellGame.GAMESCOM_DEMO;
	}

	public bool IsYoutubeDemo()
	{
		return GreenHellGame.GAMESCOM_DEMO && false;
	}

	public static MainLevel GetLevel()
	{
		return MainLevel.Instance;
	}

	public static bool IsPadControllerConnected()
	{
		return GreenHellGame.Instance.m_PadControllerConnected;
	}

	public static bool IsPCControllerActive()
	{
		return GreenHellGame.Instance.m_Settings.m_ControllerType == ControllerType.PC;
	}

	public static bool IsPadControllerActive()
	{
		return GreenHellGame.Instance.m_Settings && GreenHellGame.Instance.m_Settings.m_ControllerType == ControllerType.Pad;
	}

	private void CreateLocalization()
	{
		this.m_Localization = new Localization();
	}

	public Localization GetLocalization()
	{
		if (this.m_Localization == null)
		{
			this.CreateLocalization();
		}
		return this.m_Localization;
	}

	private void InitMusic()
	{
		GreenHellGame.s_Music = base.gameObject.AddComponent<Music>();
		GreenHellGame.s_MusicJingle = base.gameObject.AddComponent<MusicJingle>();
	}

	private static void CreateFadeSystem()
	{
		GameObject gameObject = Resources.Load("Prefabs/Systems/FadeSystem") as GameObject;
		if (gameObject)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			gameObject2.name = "FadeSystem";
			GreenHellGame.m_FadeSystem = gameObject2.GetComponent<FadeSystem>();
			GreenHellGame.m_FadeSystem.gameObject.transform.parent = GreenHellGame.Instance.gameObject.transform;
		}
	}

	private static void CreateYesNoDialog()
	{
		GameObject gameObject = Resources.Load("Prefabs/Systems/YesNodialog") as GameObject;
		if (gameObject)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			gameObject2.name = "YesNoDialog";
			GreenHellGame.m_YesNoDialog = gameObject2.GetComponent<YesNoDialog>();
			GreenHellGame.m_YesNoDialog.gameObject.transform.parent = GreenHellGame.Instance.gameObject.transform;
		}
	}

	private static void CreateLocalizationReplaceManager()
	{
		GameObject gameObject = Resources.Load("Prefabs/Systems/LocalizationReplaceManager") as GameObject;
		if (gameObject)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			gameObject2.name = "LocalizationReplaceManager";
			GreenHellGame.m_LocalReplaceManager = gameObject2.GetComponent<LocalizationReplaceManager>();
			GreenHellGame.m_LocalReplaceManager.gameObject.transform.parent = GreenHellGame.Instance.gameObject.transform;
		}
	}

	public static FadeSystem GetFadeSystem()
	{
		if (GreenHellGame.m_FadeSystem == null)
		{
			GreenHellGame.CreateFadeSystem();
		}
		return GreenHellGame.m_FadeSystem;
	}

	public static bool IsYesNoDialogActive()
	{
		return GreenHellGame.m_YesNoDialog != null && GreenHellGame.m_YesNoDialog.gameObject.activeSelf;
	}

	public static YesNoDialog GetYesNoDialog()
	{
		if (GreenHellGame.m_YesNoDialog == null)
		{
			GreenHellGame.CreateYesNoDialog();
		}
		return GreenHellGame.m_YesNoDialog;
	}

	public static LocalizationReplaceManager GetLocalizationReplaceManager()
	{
		if (GreenHellGame.m_LocalReplaceManager == null)
		{
			GreenHellGame.CreateLocalizationReplaceManager();
		}
		return GreenHellGame.m_LocalReplaceManager;
	}

	private void InitializeLoadingScreen()
	{
		GameObject original = Resources.Load("Prefabs/Items/Menu/LoadingScreen") as GameObject;
		this.m_LoadingScreenGO = UnityEngine.Object.Instantiate<GameObject>(original);
		this.m_LoadingScreen = this.m_LoadingScreenGO.GetComponent<LoadingScreen>();
		this.m_LoadingScreenGO.transform.parent = base.gameObject.transform;
	}

	private void InitializeChallengesManager()
	{
		new GameObject("ChallengesManager")
		{
			transform = 
			{
				parent = base.gameObject.transform
			}
		}.AddComponent<ChallengesManager>();
	}

	public void StartGame()
	{
		this.LoadScenesAsync("Level", LoadSceneMode.Single);
		if (ChallengesManager.Get().m_ChallengeToActivate.Length == 0)
		{
			if (GreenHellGame.Instance.m_GameMode == GameMode.Story)
			{
				this.LoadScenesAsync("Story", LoadSceneMode.Additive);
			}
			this.LoadScenesAsync("LootBoxes", LoadSceneMode.Additive);
		}
		this.m_LoadState = GreenHellGame.LoadState.GameLoading;
	}

	public void ReturnToMainMenu()
	{
		MainLevel instance = MainLevel.Instance;
		if (instance != null)
		{
			instance.EnableTerrainRendering(true);
		}
		this.m_LoadState = GreenHellGame.LoadState.ReturnToMainMenuRequest;
	}

	private void LoadScenesAsync(string scene_name, LoadSceneMode mode)
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene_name, mode);
		if (asyncOperation != null)
		{
			asyncOperation.allowSceneActivation = true;
			this.m_AsyncOps.Add(asyncOperation);
		}
	}

	private void UpdateLoadState()
	{
		switch (this.m_LoadState)
		{
		case GreenHellGame.LoadState.ReturnToMainMenuRequest:
			if (MainLevel.Instance.m_SceneAsyncOperation.Count == 0)
			{
				this.LoadScenesAsync("MainMenu", LoadSceneMode.Single);
				this.m_LoadState = GreenHellGame.LoadState.MenuLoading;
				return;
			}
			break;
		case GreenHellGame.LoadState.MenuLoading:
		{
			bool flag = true;
			using (List<AsyncOperation>.Enumerator enumerator = this.m_AsyncOps.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.isDone)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				this.m_AsyncOps.Clear();
				this.m_LoadState = GreenHellGame.LoadState.None;
				return;
			}
			break;
		}
		case GreenHellGame.LoadState.GameLoading:
		{
			bool flag2 = true;
			using (List<AsyncOperation>.Enumerator enumerator = this.m_AsyncOps.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.isDone)
					{
						flag2 = false;
					}
				}
			}
			if (flag2)
			{
				MainLevel.Instance.StartLevel();
				this.m_LoadState = GreenHellGame.LoadState.None;
			}
			break;
		}
		default:
			return;
		}
	}

	private void Update()
	{
		this.UpdateLoadState();
		this.UpdateAudio();
		this.UpdateResolution();
		if (GreenHellGame.DEBUG)
		{
			this.UpdateDebugInputs();
		}
		if (GreenHellGame.m_FadeSystem != null)
		{
			GreenHellGame.m_FadeSystem.UpdateInternal();
		}
		CursorManager.Get().Update();
		this.UpdateController();
	}

	public void OnChangeControllerOption()
	{
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings(false);
		MainMenuManager mainMenuManager = MainMenuManager.Get();
		if (mainMenuManager != null)
		{
			mainMenuManager.SetupController();
		}
		MenuInGameManager menuInGameManager = MenuInGameManager.Get();
		if (menuInGameManager != null)
		{
			menuInGameManager.SetupController();
		}
		HUDManager hudmanager = HUDManager.Get();
		if (hudmanager != null)
		{
			hudmanager.SetuController();
		}
		if (MainMenuManager.Get())
		{
			if (GreenHellGame.IsPadControllerActive())
			{
				CursorManager.Get().ShowCursor(false, false);
				return;
			}
			if (GreenHellGame.IsPCControllerActive())
			{
				CursorManager.Get().ShowCursor(true, false);
			}
		}
	}

	private void UpdateController()
	{
		if (this.m_BlockControllerUpdate)
		{
			return;
		}
		this.m_PadControllerConnected = false;
		string[] joystickNames = Input.GetJoystickNames();
		if (joystickNames.Length != 0)
		{
			int i = 0;
			while (i < joystickNames.Length)
			{
				if (!string.IsNullOrEmpty(joystickNames[i]))
				{
					this.m_PadControllerConnected = true;
					if (joystickNames[i].Contains("Wireless") && !joystickNames[i].Contains("Xbox"))
					{
						InputsManager.Get().m_PadControllerType = InputsManager.PadControllerType.Ps4;
						break;
					}
					InputsManager.Get().m_PadControllerType = InputsManager.PadControllerType.Xbox;
					break;
				}
				else
				{
					i++;
				}
			}
		}
		if (GreenHellGame.IsPadControllerActive() && !this.m_PadControllerConnected)
		{
			GreenHellGame.Instance.m_Settings.m_ControllerType = ControllerType.PC;
			this.OnChangeControllerOption();
		}
		if (this.m_PadControllerConnected && GreenHellGame.IsPCControllerActive() && !this.m_WasPadConstrollerInfo)
		{
			if (MainLevel.Instance && !MainLevel.Instance.IsPause())
			{
				MenuInGameManager.Get().ShowScreen(typeof(MenuInGame));
				this.m_PadConstrollerInfoPause = true;
			}
			this.m_WasPadConstrollerInfo = true;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YesNoDialog_Info", true), GreenHellGame.Instance.GetLocalization().Get("ControllerDetected_Info", true), false);
		}
		if (this.m_WasPadConstrollerInfo && !this.m_PadControllerConnected)
		{
			this.m_WasPadConstrollerInfo = false;
			return;
		}
		if (this.m_PadControllerConnected && GreenHellGame.IsPadControllerActive())
		{
			this.m_WasPadConstrollerInfo = true;
		}
	}

	public void OnYesFromDialog()
	{
		if (MenuInGameManager.Get())
		{
			MenuInGameManager.Get().ShowScreen(typeof(MainMenuOptionsControls));
		}
		else if (MainMenuManager.Get())
		{
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsControls), true);
		}
		this.m_PadConstrollerInfoPause = false;
	}

	public void OnNoFromDialog()
	{
		if (MenuInGameManager.Get() && this.m_PadConstrollerInfoPause)
		{
			MenuInGameManager.Get().HideMenu();
		}
		this.m_PadConstrollerInfoPause = false;
	}

	public void OnOkFromDialog()
	{
	}

	public void OnCloseDialog()
	{
	}

	private void UpdateAudio()
	{
		int num = 0;
		switch (this.m_CurrentSnapshot)
		{
		case AudioMixerSnapshotGame.Default:
			num = 0;
			break;
		case AudioMixerSnapshotGame.LowSanity:
			num = 1;
			break;
		case AudioMixerSnapshotGame.Sleep:
			num = 2;
			break;
		case AudioMixerSnapshotGame.Underwater:
			num = 3;
			break;
		}
		GameSettings settings = GreenHellGame.Instance.m_Settings;
		this.m_VolumeMultiplier.target = ((LoadingScreen.Get() != null && LoadingScreen.Get().m_Active && LoadingScreen.Get().m_State != LoadingScreenState.None) ? 0f : 1f);
		this.m_VolumeMultiplier.Update(Time.unscaledDeltaTime);
		float value = General.LinearToDecibel(settings.m_Volume);
		this.m_AudioMixer.SetFloat("MasterVolume", value);
		float dialogsVolume = settings.m_DialogsVolume;
		float value2 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 4] * dialogsVolume);
		this.m_AudioMixer.SetFloat("DialogsVolume", value2);
		float musicVolume = settings.m_MusicVolume;
		float value3 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 2] * musicVolume);
		this.m_AudioMixer.SetFloat("MusicVolume", value3);
		float enviroVolume = settings.m_EnviroVolume;
		float value4 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 1] * enviroVolume * this.m_VolumeMultiplier);
		this.m_AudioMixer.SetFloat("EnviroVolume", value4);
		float generalVolume = settings.m_GeneralVolume;
		float value5 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 0] * generalVolume * this.m_VolumeMultiplier);
		this.m_AudioMixer.SetFloat("PlayerVolume", value5);
		float value6 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 3] * generalVolume * this.m_VolumeMultiplier);
		this.m_AudioMixer.SetFloat("AIVolume", value6);
		float value7 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 6] * generalVolume * this.m_VolumeMultiplier);
		this.m_AudioMixer.SetFloat("EnviroAmplifiedVolume", value7);
		float value8 = General.LinearToDecibel(this.m_SnapshotMatrix[num, 5] * generalVolume * this.m_VolumeMultiplier);
		this.m_AudioMixer.SetFloat("SleepVolume", value8);
	}

	private void LateUpdate()
	{
		CursorManager.Get().LateUpdate();
	}

	private void SetMaxDeltaTime()
	{
		Time.maximumDeltaTime = 0.15f;
	}

	private void InitAudioMixer()
	{
		this.m_AudioMixer = (Resources.Load("PlayerSoundsAudioMixer") as AudioMixer);
		AudioMixerGroup[] array = this.m_AudioMixer.FindMatchingGroups("Master");
		this.m_AudioMixerGroupMaster = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("Player");
		this.m_AudioMixerGroupPlayer = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("Enviro");
		this.m_AudioMixerGroupEnviro = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("Music");
		this.m_AudioMixerGroupMusic = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("AI");
		this.m_AudioMixerGroupAI = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("Chatter");
		this.m_AudioMixerGroupChatter = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("Sleep");
		this.m_AudioMixerGroupSleep = array[0];
		array = this.m_AudioMixer.FindMatchingGroups("EnviroAmplified");
		this.m_AudioMixerGroupEnviroAmplified = array[0];
		this.m_SnapshotDefault = this.m_AudioMixer.FindSnapshot("Snapshot_Default");
		this.m_SnapshotLowSanity = this.m_AudioMixer.FindSnapshot("Snapshot_LowSanity");
		this.m_SnapshotSleep = this.m_AudioMixer.FindSnapshot("Snapshot_Sleep");
		this.m_SnapshotUnderwater = this.m_AudioMixer.FindSnapshot("Snapshot_Underwater");
		this.m_VolumeMultiplier.Init(1f, 5f);
	}

	public AudioMixerGroup GetAudioMixerGroup(AudioMixerGroupGame group)
	{
		switch (group)
		{
		case AudioMixerGroupGame.Master:
			return this.m_AudioMixerGroupMaster;
		case AudioMixerGroupGame.Player:
			return this.m_AudioMixerGroupPlayer;
		case AudioMixerGroupGame.Enviro:
			return this.m_AudioMixerGroupEnviro;
		case AudioMixerGroupGame.Music:
			return this.m_AudioMixerGroupMusic;
		case AudioMixerGroupGame.AI:
			return this.m_AudioMixerGroupAI;
		case AudioMixerGroupGame.Chatter:
			return this.m_AudioMixerGroupChatter;
		case AudioMixerGroupGame.Sleep:
			return this.m_AudioMixerGroupSleep;
		case AudioMixerGroupGame.EnviroAmplified:
			return this.m_AudioMixerGroupEnviroAmplified;
		default:
			return null;
		}
	}

	private float GetAudioMixerGroupVolume(AudioMixerGroupGame mixer)
	{
		float result;
		switch (mixer)
		{
		case AudioMixerGroupGame.Master:
			this.m_AudioMixer.GetFloat("MasterVolume", out result);
			break;
		case AudioMixerGroupGame.Player:
			this.m_AudioMixer.GetFloat("PlayerVolume", out result);
			break;
		case AudioMixerGroupGame.Enviro:
			this.m_AudioMixer.GetFloat("EnviroVolume", out result);
			break;
		case AudioMixerGroupGame.Music:
			this.m_AudioMixer.GetFloat("MusicVolume", out result);
			break;
		case AudioMixerGroupGame.AI:
			this.m_AudioMixer.GetFloat("AIVolume", out result);
			break;
		case AudioMixerGroupGame.Chatter:
			this.m_AudioMixer.GetFloat("DialogsVolume", out result);
			break;
		case AudioMixerGroupGame.Sleep:
			this.m_AudioMixer.GetFloat("SleepVolume", out result);
			break;
		case AudioMixerGroupGame.EnviroAmplified:
			this.m_AudioMixer.GetFloat("EnviroAmplifiedVolume", out result);
			break;
		default:
			result = 0f;
			break;
		}
		return result;
	}

	public void SetSnapshot(AudioMixerSnapshotGame snapshot, float time_to_reach = 0.5f)
	{
		if (snapshot == this.m_CurrentSnapshot)
		{
			return;
		}
		AudioMixerSnapshot[] array = null;
		float[] array2 = null;
		if (snapshot == AudioMixerSnapshotGame.Default)
		{
			array = new AudioMixerSnapshot[2];
			array2 = new float[2];
			array[0] = this.m_SnapshotDefault;
			array2[0] = 1f;
			array[1] = this.m_SnapshotLowSanity;
			array2[1] = 0f;
		}
		else if (snapshot == AudioMixerSnapshotGame.LowSanity)
		{
			array = new AudioMixerSnapshot[2];
			array2 = new float[2];
			array[0] = this.m_SnapshotDefault;
			array2[0] = 0f;
			array[1] = this.m_SnapshotLowSanity;
			array2[1] = 1f;
		}
		else if (snapshot == AudioMixerSnapshotGame.Sleep)
		{
			array = new AudioMixerSnapshot[2];
			array2 = new float[2];
			array[0] = this.m_SnapshotDefault;
			array2[0] = 0f;
			array[1] = this.m_SnapshotSleep;
			array2[1] = 1f;
		}
		else if (snapshot == AudioMixerSnapshotGame.Underwater)
		{
			array = new AudioMixerSnapshot[2];
			array2 = new float[2];
			array[0] = this.m_SnapshotDefault;
			array2[0] = 0f;
			array[1] = this.m_SnapshotUnderwater;
			array2[1] = 1f;
		}
		this.m_AudioMixer.TransitionToSnapshots(array, array2, time_to_reach);
		GreenHellGame.OnAudioSnapshotChangedDel onAudioSnapshotChangedEvent = this.OnAudioSnapshotChangedEvent;
		if (onAudioSnapshotChangedEvent != null)
		{
			onAudioSnapshotChangedEvent(this.m_CurrentSnapshot, snapshot);
		}
		this.m_CurrentSnapshot = snapshot;
	}

	public AudioMixerSnapshotGame GetCurrentSnapshot()
	{
		return this.m_CurrentSnapshot;
	}

	private void UpdateResolution()
	{
		if (Screen.fullScreen && this.m_MenuResolutionX > 0 && this.m_MenuResolutionY > 0 && (Screen.width != GreenHellGame.Instance.m_MenuResolutionX || Screen.height != GreenHellGame.Instance.m_MenuResolutionY))
		{
			Screen.SetResolution(GreenHellGame.Instance.m_MenuResolutionX, GreenHellGame.Instance.m_MenuResolutionY, true);
		}
	}

	private void UpdateDebugInputs()
	{
		if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.T))
		{
			this.DumpTextures();
		}
	}

	private void DumpTextures()
	{
		string text = DateTime.Now.ToLongTimeString();
		text = text.Replace(':', '_');
		FileStream fileStream = File.Create(Application.persistentDataPath + "/textures_dump" + text + ".txt");
		string text2 = string.Empty;
		this.m_SortedTextures.Clear();
		Texture[] array = Resources.FindObjectsOfTypeAll<Texture>();
		int num = 0;
		foreach (Texture texture in array)
		{
			int num2 = texture.width * texture.height;
			if (!this.m_SortedTextures.ContainsKey(num2))
			{
				this.m_SortedTextures.Add(num2, new List<string>());
			}
			else
			{
				this.m_SortedTextures[num2].Add(texture.name);
			}
			num += num2;
		}
		foreach (KeyValuePair<int, List<string>> keyValuePair in this.m_SortedTextures)
		{
			keyValuePair.Value.Sort();
		}
		foreach (KeyValuePair<int, List<string>> keyValuePair in this.m_SortedTextures)
		{
			keyValuePair.Value.Sort();
		}
		foreach (KeyValuePair<int, List<string>> keyValuePair in this.m_SortedTextures)
		{
			foreach (string str in keyValuePair.Value)
			{
				text2 = text2 + "Name: " + str;
				object arg = text2;
				object arg2 = " Mem: ";
				SortedDictionary<int, List<string>>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				text2 = arg + arg2 + keyValuePair.Key;
				text2 += Environment.NewLine;
			}
		}
		text2 = text2 + "TotalMem: " + num;
		byte[] bytes = Encoding.ASCII.GetBytes(text2);
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Close();
	}

	private void InitInputsManager()
	{
		base.gameObject.AddComponent<InputsManager>();
	}

	private void InitializeSteam()
	{
		this.m_SteamManager = SteamManager.Instance;
		if (!SteamManager.Initialized)
		{
			Application.Quit();
		}
	}

	private void InitializeRemoteStorage()
	{
		if (this.IsGamescom())
		{
			this.m_RemoteStorage = new DebugStorage();
			return;
		}
		this.m_RemoteStorage = new RemoteStorageSteam();
	}

	private void TryToMoveSavesToRemoteStorage()
	{
		if (GreenHellGame.GAMESCOM_DEMO)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			string text = SaveGame.SLOT_NAME + i.ToString() + ".sav";
			if (File.Exists(Application.persistentDataPath + "/" + text) && !this.FileExistsInRemoteStorage(text))
			{
				FileStream fileStream = File.Open(Application.persistentDataPath + "/" + text, FileMode.Open);
				int num = (int)fileStream.Length;
				byte[] array = new byte[fileStream.Length];
				int num2 = fileStream.Read(array, 0, (int)fileStream.Length);
				fileStream.Close();
				if (num2 == num)
				{
					if (this.m_RemoteStorage.FileWrite(text, array))
					{
						if (!File.Exists(Application.persistentDataPath + "/" + text + "_backup"))
						{
							File.Move(Application.persistentDataPath + "/" + text, Application.persistentDataPath + "/" + text + "_backup");
						}
					}
					else
					{
						DebugUtils.Assert("GreenHellgame::TryToMoveSavesToRemoteStorage - m_RemoteStorage.FileWrite failed", true, DebugUtils.AssertType.Info);
					}
				}
				else
				{
					DebugUtils.Assert("GreenHellGame::TryToMoveSavesToRemoteStorage - Problem reading save game file " + text, true, DebugUtils.AssertType.Info);
				}
			}
		}
	}

	private void TryToMoveSettingsToRemoteStorage()
	{
		if (GreenHellGame.GAMESCOM_DEMO)
		{
			return;
		}
		if (File.Exists(Application.persistentDataPath + "/" + GameSettings.s_SettingsFileName))
		{
			if (this.FileExistsInRemoteStorage(GameSettings.s_SettingsFileName))
			{
				return;
			}
			FileStream fileStream = File.Open(Application.persistentDataPath + "/" + GameSettings.s_SettingsFileName, FileMode.Open);
			int num = (int)fileStream.Length;
			byte[] array = new byte[fileStream.Length];
			int num2 = fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			if (num2 == num)
			{
				if (!this.m_RemoteStorage.FileWrite(GameSettings.s_SettingsFileName, array))
				{
					DebugUtils.Assert("GreenHellgame::TryToMoveSettingsToRemoteStorage - m_RemoteStorage.FileWrite failed", true, DebugUtils.AssertType.Info);
					return;
				}
				if (!File.Exists(Application.persistentDataPath + "/" + GameSettings.s_SettingsFileName + "_backup"))
				{
					File.Move(Application.persistentDataPath + "/" + GameSettings.s_SettingsFileName, Application.persistentDataPath + "/" + GameSettings.s_SettingsFileName + "_backup");
					return;
				}
			}
			else
			{
				DebugUtils.Assert("GreenHellGame::TryToMoveSettingsToRemoteStorage - Problem reading file " + GameSettings.s_SettingsFileName, true, DebugUtils.AssertType.Info);
			}
		}
	}

	public bool FileExistsInRemoteStorage(string save_name)
	{
		return this.m_RemoteStorage.FileExistsInRemoteStorage(save_name);
	}

	private void DeleteAllFilesFromRemoteStorage()
	{
		int fileCount = this.m_RemoteStorage.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			string text = string.Empty;
			int num = 0;
			text = this.m_RemoteStorage.GetFileNameAndSize(i, out num);
			DebugUtils.Assert(this.m_RemoteStorage.FileDelete(text), "GreenHellGame::DeleteAllFilesFromRemoteStorage - failed to delete file " + text, true, DebugUtils.AssertType.Info);
		}
	}

	public void DisablePICommanderWithFade(string commander_name, float time)
	{
		PICommander.GetCommander(commander_name).DisableWithFade(time);
	}

	public void ParametersInterpolatorSetupData(string interp_name)
	{
		ParametersInterpolator.GetInterpolator(interp_name).SetupData();
	}

	public static bool ROADSHOW_DEMO = false;

	public static bool DEBUG = false;

	public static bool TWITCH_DEMO = false;

	public static bool FORCE_SURVIVAL = false;

	public const bool YOUTUBERS_DEMO = false;

	public static GameVersion s_GameVersionEarlyAcces = new GameVersion(0, 9);

	public static GameVersion s_GameVersionEarlyAccessUpdate2 = new GameVersion(0, 10);

	public static GameVersion s_GameVersionEarlyAccessUpdate3 = new GameVersion(0, 11);

	public static GameVersion s_GameVersionEarlyAccessUpdate4 = new GameVersion(0, 12);

	public static GameVersion s_GameVersionEarlyAccessUpdate5 = new GameVersion(0, 13);

	public static GameVersion s_GameVersionEarlyAccessUpdate6 = new GameVersion(0, 14);

	public static GameVersion s_GameVersionEarlyAccessUpdate7 = new GameVersion(0, 15);

	public static GameVersion s_GameVersionEarlyAccessUpdate8 = new GameVersion(0, 16);

	public static GameVersion s_GameVersionEarlyAccessUpdate9 = new GameVersion(0, 17);

	public static GameVersion s_GameVersionEarlyAccessUpdate10 = new GameVersion(0, 18);

	public static GameVersion s_GameVersionEarlyAccessUpdate11 = new GameVersion(0, 19);

	public static GameVersion s_GameVersionEarlyAccessUpdate12 = new GameVersion(0, 20);

	public static GameVersion s_GameVersionEarlyAccessUpdate13 = new GameVersion(0, 50);

	public static GameVersion s_GameVersionReleaseCandidate = new GameVersion(0, 90);

	public static GameVersion s_GameVersionMaster = new GameVersion(1, 0);

	public static GameVersion s_GameVersionMasterHotfix1 = new GameVersion(1, 1);

	public static GameVersion s_GameVersionMasterHotfix2 = new GameVersion(1, 2);

	public static GameVersion s_GameVersionMasterHotfix3 = new GameVersion(1, 3);

	public static GameVersion s_GameVersionMasterHotfix4 = new GameVersion(1, 4);

	public static GameVersion s_GameVersionMasterFarming1_1 = new GameVersion(1, 10);

	public static GameVersion s_GameVersionMasterFarming1_1_1 = new GameVersion(1, 11);

	public static GameVersion s_GameVersionMasterFarming1_1_2 = new GameVersion(1, 12);

	public static GameVersion s_GameVersionMasterController1_2 = new GameVersion(1, 20);

	public static GameVersion s_GameVersionMasterController1_21 = new GameVersion(1, 21);

	public static GameVersion s_GameVersionMasterShelters1_3 = new GameVersion(1, 30);

	public static GameVersion s_GameVersionCoop1 = new GameVersion(1, 50);

	public static GameVersion s_GameVersion = new GameVersion(GreenHellGame.s_GameVersionMasterShelters1_3);

	public static AppId_t s_SteamAppId = new AppId_t(815370u);

	private static GreenHellGame s_Instance = null;

	private static bool s_AppQuitting = false;

	private Localization m_Localization;

	private static FadeSystem m_FadeSystem = null;

	private static YesNoDialog m_YesNoDialog = null;

	private static LocalizationReplaceManager m_LocalReplaceManager = null;

	private Dictionary<string, GameObject> m_PrefabsMap = new Dictionary<string, GameObject>();

	public bool m_FromSave;

	public GameMode m_GameMode;

	public static Music s_Music = null;

	public static MusicJingle s_MusicJingle = null;

	public LoadingScreen m_LoadingScreen;

	[HideInInspector]
	public List<AsyncOperation> m_AsyncOps = new List<AsyncOperation>();

	private GameObject m_LoadingScreenGO;

	public LoadGameState m_LoadGameState;

	[HideInInspector]
	public GameSettings m_Settings;

	public bool m_WasCompanyLogo;

	[HideInInspector]
	public int m_MenuResolutionX = -1;

	[HideInInspector]
	public int m_MenuResolutionY = -1;

	[HideInInspector]
	public bool m_MenuFullscreen;

	public static float s_NormalModeLossMul = 0.7f;

	public static float s_EasyModeLossMul = 0.5f;

	public bool m_ShowSounds3D;

	public bool m_ShowSoundsCurrentlyPlaying;

	public GreenHellGame.LoadState m_LoadState;

	[HideInInspector]
	public HashSet<string> m_ScenarioScenes = new HashSet<string>();

	private SpringFloat m_VolumeMultiplier;

	private SessionJoinHelper m_SessionJoinHelper;

	public P2PNetworkManager m_NetworkManager;

	private bool m_PadControllerConnected;

	private bool m_WasPadConstrollerInfo;

	private bool m_PadConstrollerInfoPause;

	public IRemoteStorage m_RemoteStorage;

	private Dictionary<string, string> m_PrefabsPathMap = new Dictionary<string, string>();

	private bool m_LoadAllPrefabs;

	private Dictionary<string, GameObject> m_AllPrefabsMap = new Dictionary<string, GameObject>();

	public bool m_BlockControllerUpdate;

	private float[,] m_SnapshotMatrix = new float[,]
	{
		{
			1f,
			1f,
			1f,
			1f,
			1f,
			0f,
			2.3f
		},
		{
			0.8f,
			1f,
			1f,
			1f,
			1f,
			0f,
			1.5f
		},
		{
			0f,
			0f,
			1f,
			0f,
			0f,
			1f,
			0f
		},
		{
			0f,
			1f,
			0f,
			0.1f,
			0f,
			0f,
			0.1f
		}
	};

	public AudioMixer m_AudioMixer;

	private AudioMixerGroup m_AudioMixerGroupMaster;

	private AudioMixerGroup m_AudioMixerGroupPlayer;

	private AudioMixerGroup m_AudioMixerGroupEnviro;

	private AudioMixerGroup m_AudioMixerGroupMusic;

	private AudioMixerGroup m_AudioMixerGroupAI;

	private AudioMixerGroup m_AudioMixerGroupChatter;

	private AudioMixerGroup m_AudioMixerGroupSleep;

	private AudioMixerGroup m_AudioMixerGroupEnviroAmplified;

	private AudioMixerSnapshot m_SnapshotDefault;

	private AudioMixerSnapshot m_SnapshotLowSanity;

	private AudioMixerSnapshot m_SnapshotSleep;

	private AudioMixerSnapshot m_SnapshotUnderwater;

	private AudioMixerSnapshotGame m_CurrentSnapshot;

	private static CompareArrayByDimension s_DimensionComparer = new CompareArrayByDimension();

	private SortedDictionary<int, List<string>> m_SortedTextures = new SortedDictionary<int, List<string>>(GreenHellGame.s_DimensionComparer);

	public SteamManager m_SteamManager;

	public enum LoadState
	{
		None,
		ReturnToMainMenuRequest,
		MenuLoading,
		GameLoading
	}

	public delegate void OnAudioSnapshotChangedDel(AudioMixerSnapshotGame prev_snapshot, AudioMixerSnapshotGame new_snapshot);
}
