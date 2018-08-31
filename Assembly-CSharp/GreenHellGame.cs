using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Enums;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GreenHellGame : MonoBehaviour
{
	public static GreenHellGame Instance
	{
		get
		{
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
		this.InitAudioMixer();
		this.m_Settings = base.gameObject.AddComponent<GameSettings>();
		this.m_Settings.LoadSettings();
		GreenHellGame.ROADSHOW_DEMO = File.Exists(Application.dataPath + "/Resources/scripts/Debug/ROADSHOW_DEMO");
		GreenHellGame.DEBUG = File.Exists(Application.dataPath + "/Resources/scripts/Debug/DEBUG");
		GreenHellGame.TWITCH_DEMO = File.Exists(Application.dataPath + "/Resources/scripts/Debug/TWITCH_DEMO");
		GreenHellGame.FORCE_SURVIVAL = File.Exists(Application.dataPath + "/Resources/scripts/Debug/FORCE_SURVIVAL");
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
		CursorManager.Get().ShowCursor(false);
		this.InitializeChallengesManager();
		this.m_Settings.ApplySettings();
	}

	private void OnEnable()
	{
		int num = 0;
		num++;
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
			CJDebug.Log("GreenHellGame::GetPrefab prefab doesn't exist in map");
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
	}

	public static MainLevel GetLevel()
	{
		return MainLevel.Instance;
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
		this.m_ScenesToLoad.Clear();
		this.AddScene("Level");
		this.LoadScenesAsync();
	}

	public void ReturnToMainMenu()
	{
		this.m_ScenesToLoad.Clear();
		this.AddScene("MainMenu");
		this.LoadScenesAsync();
	}

	private void LoadScenesAsync()
	{
		List<string> scenesToLoad = this.m_ScenesToLoad;
		for (int i = 0; i < scenesToLoad.Count; i++)
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Single);
			asyncOperation.allowSceneActivation = true;
			this.m_AsyncOps.Add(asyncOperation);
		}
	}

	private void AddScene(string name)
	{
		this.m_ScenesToLoad.Add(name);
	}

	private void OnDestroy()
	{
		int num = 0;
		num++;
	}

	private void Update()
	{
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
		this.m_SnapshotDefault = this.m_AudioMixer.FindSnapshot("Snapshot_Default");
		this.m_SnapshotLowSanity = this.m_AudioMixer.FindSnapshot("Snapshot_LowSanity");
		this.m_SnapshotSleep = this.m_AudioMixer.FindSnapshot("Snapshot_Sleep");
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
		default:
			return null;
		}
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
		this.m_AudioMixer.TransitionToSnapshots(array, array2, time_to_reach);
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
		foreach (KeyValuePair<int, List<string>> keyValuePair2 in this.m_SortedTextures)
		{
			List<string> value = keyValuePair2.Value;
			value.Sort();
		}
		foreach (KeyValuePair<int, List<string>> keyValuePair3 in this.m_SortedTextures)
		{
			foreach (string str in keyValuePair3.Value)
			{
				text2 = text2 + "Name: " + str;
				object arg = text2;
				object arg2 = " Mem: ";
				SortedDictionary<int, List<string>>.Enumerator enumerator;
				KeyValuePair<int, List<string>> keyValuePair4 = enumerator.Current;
				text2 = arg + arg2 + keyValuePair4.Key;
				text2 += Environment.NewLine;
			}
		}
		text2 = text2 + "TotalMem: " + num;
		byte[] bytes = Encoding.ASCII.GetBytes(text2);
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Close();
	}

	public static bool ROADSHOW_DEMO = false;

	public static bool DEBUG = false;

	public static bool TWITCH_DEMO = false;

	public static bool FORCE_SURVIVAL = false;

	public static GameVersion s_GameVersionEarlyAcces = new GameVersion(0, 9);

	public static GameVersion s_GameVersion = new GameVersion(GreenHellGame.s_GameVersionEarlyAcces);

	private static GreenHellGame s_Instance = null;

	private Localization m_Localization;

	private static FadeSystem m_FadeSystem = null;

	private static YesNoDialog m_YesNoDialog = null;

	private static LocalizationReplaceManager m_LocalReplaceManager = null;

	private Dictionary<string, GameObject> m_PrefabsMap = new Dictionary<string, GameObject>();

	public bool m_FromSave;

	public GameMode m_GameMode;

	public static Music s_Music = null;

	public LoadingScreen m_LoadingScreen;

	private List<string> m_ScenesToLoad = new List<string>();

	[HideInInspector]
	public List<AsyncOperation> m_AsyncOps = new List<AsyncOperation>();

	private GameObject m_LoadingScreenGO;

	public LoadGameState m_LoadGameState;

	[HideInInspector]
	public GameDifficulty m_GameDifficulty = GameDifficulty.Normal;

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

	private Dictionary<string, string> m_PrefabsPathMap = new Dictionary<string, string>();

	private bool m_LoadAllPrefabs;

	private Dictionary<string, GameObject> m_AllPrefabsMap = new Dictionary<string, GameObject>();

	public AudioMixer m_AudioMixer;

	private AudioMixerGroup m_AudioMixerGroupMaster;

	private AudioMixerGroup m_AudioMixerGroupPlayer;

	private AudioMixerGroup m_AudioMixerGroupEnviro;

	private AudioMixerGroup m_AudioMixerGroupMusic;

	private AudioMixerGroup m_AudioMixerGroupAI;

	private AudioMixerGroup m_AudioMixerGroupChatter;

	private AudioMixerGroup m_AudioMixerGroupSleep;

	private AudioMixerSnapshot m_SnapshotDefault;

	private AudioMixerSnapshot m_SnapshotLowSanity;

	private AudioMixerSnapshot m_SnapshotSleep;

	private AudioMixerSnapshotGame m_CurrentSnapshot;

	private static CompareArrayByDimension s_DimensionComparer = new CompareArrayByDimension();

	private SortedDictionary<int, List<string>> m_SortedTextures = new SortedDictionary<int, List<string>>(GreenHellGame.s_DimensionComparer);
}
