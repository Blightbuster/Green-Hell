using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedTerrainGrass;
using AIs;
using CJTools;
using Enums;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class MainLevel : MonoBehaviour, ISaveLoad
{
	public static float s_GameTime
	{
		get
		{
			return MainLevel.Instance.m_TODSky.Cycle.GameTime;
		}
		set
		{
			MainLevel.Instance.m_TODSky.Cycle.GameTime = value;
		}
	}

	public static float s_GameTimeDelta
	{
		get
		{
			return MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
		}
	}

	public bool m_LevelStarted { get; private set; }

	private MainLevel()
	{
		MainLevel.s_Instance = this;
	}

	public static MainLevel Instance
	{
		get
		{
			return MainLevel.s_Instance;
		}
	}

	private void Awake()
	{
		new IconColors();
		this.m_LevelScene = SceneManager.GetSceneByName("Level");
		SceneLoadUnloadRequestHolder.OnSceneLoad += this.OnSceneLoad;
		SceneLoadUnloadRequestHolder.OnSceneUnload += this.OnSceneUnload;
	}

	private void Start()
	{
		if (GreenHellGame.Instance.m_LoadState == GreenHellGame.LoadState.None)
		{
			this.StartLevel();
		}
		else
		{
			base.enabled = false;
		}
		HxVolumetricCamera component = this.m_MainCamera.GetComponent<HxVolumetricCamera>();
		if (component)
		{
			component.enabled = false;
		}
	}

	public void StartLevel()
	{
		if (GreenHellGame.FORCE_SURVIVAL)
		{
			this.m_GameMode = GameMode.Survival;
		}
		else
		{
			this.m_GameMode = GreenHellGame.Instance.m_GameMode;
		}
		if (this.m_GameMode == GameMode.None)
		{
			this.ShowModeMenu();
		}
		else
		{
			this.Initialize();
		}
		this.m_LevelStartTime = Time.time;
		base.enabled = true;
		this.m_LevelStarted = true;
		if (GreenHellGame.Instance.m_Settings.m_NeverPlayed)
		{
			GreenHellGame.Instance.m_Settings.m_NeverPlayed = false;
			GreenHellGame.Instance.m_Settings.SaveSettings();
		}
	}

	private void ShowModeMenu()
	{
		ScenarioManager.Get().LoadScene("Story");
		ScenarioManager.Get().LoadScene("LootBoxes");
		MenuInGameManager.Get().ShowScreen(typeof(MenuDebugSelectMode));
	}

	public void InitObjects()
	{
		this.m_UniqueObjects.Clear();
		this.m_AllObjects.Clear();
		foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
		{
			if (gameObject.scene.IsValid() && !gameObject.transform.root.gameObject.name.StartsWith("P2PPlayer"))
			{
				this.m_AllObjects.Add(gameObject);
				if (!this.m_UniqueObjects.ContainsKey(gameObject.name))
				{
					this.m_UniqueObjects.Add(gameObject.name, gameObject);
				}
			}
		}
	}

	public void Initialize()
	{
		this.InitObjects();
		this.InitAmbientAudioSystem();
		this.TeleportPlayerOnStart();
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
		ChallengesManager.Get().OnLevelLoaded();
		ScenarioManager.Get().Initialize();
		this.SetupSceneItems();
		if (GreenHellGame.Instance.m_FromSave)
		{
			SaveGame.Load();
		}
		this.m_FogStartDistance = RenderSettings.fogStartDistance;
		this.m_FogEndDistance = RenderSettings.fogEndDistance;
		this.m_EmmisiveColorProperty = Shader.PropertyToID("_EmissionColor");
		this.m_EmissiveColor = new Color(0.29f, 0.611f, 0.219f, 1f);
		if (GreenHellGame.TWITCH_DEMO)
		{
			new TwitchDemoManager();
		}
		this.m_PostProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
		this.m_DefaultStationaryBlending = this.m_PostProcessLayer.temporalAntialiasing.stationaryBlending;
		CursorManager.Get().ResetCursorRequests();
		CursorManager.Get().ShowCursor(false, false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_NGSSDirectional = this.m_TODSky.gameObject.GetComponentInChildren<NGSS_Directional>();
		this.m_GrassManager = Terrain.activeTerrain.GetComponent<GrassManager>();
		AnimationEventsReceiver.PreParseAnimationEventScripts();
	}

	private void InitAmbientAudioSystem()
	{
		if (base.GetComponent<AmbientAudioSystem>() == null)
		{
			base.gameObject.AddComponent<AmbientAudioSystem>();
		}
	}

	private void SetupSceneItems()
	{
		foreach (Item item in Item.s_AllItems)
		{
			if (!this.m_SceneItems.ContainsKey(item.m_InfoName))
			{
				this.m_SceneItems.Add(item.m_InfoName, new List<Vector3>());
			}
			this.m_SceneItems[item.m_InfoName].Add(item.transform.position);
		}
	}

	private void ApplyGraphicsSettings()
	{
		if (this.m_NGSSDirectional)
		{
			if (this.m_NGSSDirectional.SAMPLERS_COUNT != (NGSS_Directional.SAMPLER_COUNT)GreenHellGame.Instance.m_Settings.m_ShadowsBlur)
			{
				this.m_NGSSDirectional.SAMPLERS_COUNT = (NGSS_Directional.SAMPLER_COUNT)GreenHellGame.Instance.m_Settings.m_ShadowsBlur;
			}
			this.m_NGSSDirectional.KEEP_NGSS_ONDISABLE = false;
			if (this.m_NGSSDirectional.enabled != GreenHellGame.Instance.m_Settings.m_SoftShadows)
			{
				this.m_NGSSDirectional.enabled = GreenHellGame.Instance.m_Settings.m_SoftShadows;
			}
		}
	}

	public bool IsPause()
	{
		return this.m_PauseRequestsCount > 0;
	}

	public void Pause(bool pause)
	{
		if (pause)
		{
			this.m_PauseRequestsCount++;
		}
		else
		{
			this.m_PauseRequestsCount--;
		}
		if (ReplicatedLogicalPlayer.s_LocalLogicalPlayer)
		{
			ReplicatedLogicalPlayer.s_LocalLogicalPlayer.m_PauseGame = (this.m_PauseRequestsCount > 0);
		}
		this.UpdateTimeScale();
	}

	private void Update()
	{
		int i = 0;
		while (i < this.m_SceneAsyncOperation.Count)
		{
			if (this.m_SceneAsyncOperation[i] == null || this.m_SceneAsyncOperation[i].isDone)
			{
				this.m_SceneAsyncOperation.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
		this.ApplyGraphicsSettings();
		this.UpdateSlowMotion();
		this.UpdateTimeScale();
		this.UpdateCurentTimeInMinutes();
		this.UpdateInputsDebug();
		EventsManager.OnEvent(Enums.Event.PlayTime, Time.deltaTime);
		this.UpdateLoading();
		if (GreenHellGame.ROADSHOW_DEMO && Input.GetKeyDown(KeyCode.Escape))
		{
			this.m_DebugPause = !this.m_DebugPause;
			CursorManager.Get().ShowCursor(this.m_DebugPause, false);
			this.Pause(this.m_DebugPause);
			if (this.m_DebugPause)
			{
				Player.Get().BlockMoves();
				Player.Get().BlockRotation();
			}
			else
			{
				Player.Get().UnblockMoves();
				Player.Get().UnblockRotation();
			}
		}
		this.UpdateFog();
		if (this.m_TODSky.Cycle.Hour != this.m_LastEmissionColorChangeHour)
		{
			this.UpdateEmissiveMaterials();
			this.m_LastEmissionColorChangeHour = this.m_TODSky.Cycle.Hour;
		}
		if (TwitchDemoManager.Get() != null)
		{
			TwitchDemoManager.Get().Update();
		}
		this.UpdateDebugCutscene();
		this.UpdateAA();
		ItemReplacer.UpdateByDistance();
		this.TODInterpolatorUpdate();
	}

	private void UpdateInputsDebug()
	{
		if (!GreenHellGame.DEBUG || MenuInGameManager.Get().IsAnyScreenVisible())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Numlock))
		{
			this.CycleTimeScaleSlowdown();
		}
		else if (Input.GetKeyDown(KeyCode.KeypadDivide))
		{
			this.CycleTimeScaleSpeedup();
		}
		else if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			this.m_TODTime.AddHoursDebug(1f);
			ReflectionProbeUpdater[] array = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			this.m_TODTime.AddHoursDebug(-1f);
			ReflectionProbeUpdater[] array2 = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			this.m_TODTime.AddHours(10f, true, false);
			ReflectionProbeUpdater[] array3 = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Pause))
		{
			this.m_DebugPause = !this.m_DebugPause;
			CursorManager.Get().ShowCursor(this.m_DebugPause, false);
			this.Pause(this.m_DebugPause);
			if (this.m_DebugPause)
			{
				Player.Get().BlockMoves();
				Player.Get().BlockRotation();
			}
			else
			{
				Player.Get().UnblockMoves();
				Player.Get().UnblockRotation();
			}
		}
		else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
		{
			Cheats.m_GodMode = !Cheats.m_GodMode;
		}
		else if (Input.GetKeyDown(KeyCode.KeypadPeriod))
		{
			this.m_TODTime.ProgressTime = !this.m_TODTime.ProgressTime;
		}
		else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Player.Get().TwentyFivePointsOfDamage();
			}
			else
			{
				Player.Get().Kill();
				Player.Get().UpdateDeath();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Equals))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				RainManager.Get().ToggleDebug();
			}
			else
			{
				RainManager.Get().ToggleRain();
			}
			if (Input.GetKey(KeyCode.RightShift))
			{
				RainManager.Get().TogglePeriodDebug();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Semicolon))
		{
			HUDManager.Get().ShowCredits();
		}
		else if (Input.GetKeyDown(KeyCode.Minus))
		{
			this.m_DebugShowCursor = !this.m_DebugShowCursor;
			CursorManager.Get().ShowCursor(this.m_DebugShowCursor, false);
			if (this.m_DebugShowCursor)
			{
				Vector2 zero = Vector2.zero;
				zero.Set((float)(Screen.width / 2), (float)(Screen.height / 2));
				CursorManager.Get().SetCursorPos(zero);
			}
		}
		else if (Input.GetKeyDown(KeyCode.U) && !Input.GetKey(KeyCode.LeftControl))
		{
			ParticleSystem[] array4 = Resources.FindObjectsOfTypeAll<ParticleSystem>();
			int num = 0;
			int num2 = 0;
			for (int l = 0; l < array4.Length; l++)
			{
				bool activeInHierarchy = array4[l].gameObject.activeInHierarchy;
				Debug.Log("ParticleSystem " + array4[l].gameObject.name + (activeInHierarchy ? " 1" : " 0"));
				if (activeInHierarchy)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
			Debug.Log("ParticleSystem num active " + num.ToString());
			Debug.Log("ParticleSystem num inactive " + num2.ToString());
		}
		else if (Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket))
		{
			if (this.m_CutscenePlayer == null)
			{
				GameObject original;
				if (Input.GetKeyDown(KeyCode.LeftBracket))
				{
					original = (Resources.Load("Prefabs/TempPrefabs/CutscenePlayer/CutscenePlayer") as GameObject);
				}
				else
				{
					original = (Resources.Load("Prefabs/TempPrefabs/CutscenePlayer/CutscenePlayer2") as GameObject);
				}
				this.m_CutscenePlayer = UnityEngine.Object.Instantiate<GameObject>(original);
				this.m_CutscenePlayer.transform.rotation = Player.Get().transform.rotation;
				this.m_CutscenePlayer.transform.position = Player.Get().transform.position;
				CameraManager.Get().SetTarget(this.m_CutscenePlayer.GetComponent<Being>());
				CameraManager.Get().SetMode(CameraManager.Mode.CutscenePlayer);
			}
			else
			{
				UnityEngine.Object.Destroy(this.m_CutscenePlayer);
				this.m_CutscenePlayer = null;
				CameraManager.Get().SetTarget(Player.Get());
				CameraManager.Get().SetMode(CameraManager.Mode.Normal);
			}
		}
		else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F6))
		{
			Player.Get().StartController(PlayerControllerType.BodyInspectionMiniGame);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad9))
		{
			SaveGame.Save();
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			HUDCameraPosition.Get().m_Active = true;
		}
		else
		{
			HUDCameraPosition.Get().m_Active = false;
		}
		if (Input.GetKey(KeyCode.F10))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.EnableSkyDome();
				return;
			}
			this.DisableSkyDome();
		}
	}

	private void CycleTimeScaleSpeedup()
	{
		if (this.m_TimeScaleMode == 0)
		{
			this.m_TimeScaleMode = 1;
			return;
		}
		this.m_TimeScaleMode = 0;
	}

	private void CycleTimeScaleSlowdown()
	{
		if (this.m_TimeScaleMode == 2)
		{
			this.m_TimeScaleMode = 0;
			return;
		}
		this.m_TimeScaleMode = 2;
	}

	private void UpdateSlowMotion()
	{
		if (this.m_SlowMotionFactor != this.m_WantedSlowMotionFactor)
		{
			float b = Time.unscaledTime - this.m_ChangeSlowMotionTime;
			this.m_SlowMotionFactor = CJTools.Math.GetProportionalClamp(this.m_SlowMotionFactor, this.m_WantedSlowMotionFactor, b, 0f, 1f);
		}
	}

	public void UpdateTimeScale()
	{
		bool can_pause = true;
		if (!ReplTools.IsPlayingAlone())
		{
			ReplTools.ForEachLogicalPlayer(delegate(ReplicatedLogicalPlayer player)
			{
				if (!player.m_PauseGame)
				{
					can_pause = false;
				}
			}, ReplTools.EPeerType.All);
		}
		float num = 1f;
		if (this.IsPause() & can_pause)
		{
			if (Time.time != 0f)
			{
				num = 0f;
				this.PauseAllAudio(true);
				this.m_WasPausedLastFrame = true;
			}
		}
		else
		{
			if (this.m_TimeScaleMode == 0)
			{
				num = 1f;
			}
			else if (this.m_TimeScaleMode == 1)
			{
				num *= 10f;
			}
			else if (this.m_TimeScaleMode == 2)
			{
				num *= 0.1f;
			}
			if (Time.timeScale == 0f)
			{
				this.PauseAllAudio(false);
			}
			if (this.m_WasPausedLastFrame)
			{
				this.m_LastUnpauseTime = Time.time;
			}
			this.m_WasPausedLastFrame = false;
		}
		Time.timeScale = num * this.m_SlowMotionFactor;
	}

	public void PauseAllAudio(bool pause)
	{
		if (!this.m_WasPausedLastFrame)
		{
			this.audio_sources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
		}
		for (int i = 0; i < this.audio_sources.Length; i++)
		{
			if (pause)
			{
				if (this.audio_sources[i] && !this.audio_sources[i].ignoreListenerPause)
				{
					this.audio_sources[i].Pause();
				}
			}
			else if (this.audio_sources[i])
			{
				this.audio_sources[i].UnPause();
			}
		}
	}

	public GameObject FindObject(string name, string type = "")
	{
		if (type == string.Empty)
		{
			return this.GetUniqueObject(name);
		}
		foreach (GameObject gameObject in this.m_AllObjects)
		{
			if (gameObject && gameObject.name == name)
			{
				if (type.Length <= 0)
				{
					return gameObject;
				}
				if (gameObject.GetComponent(type))
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetUniqueObject(string name)
	{
		GameObject result = null;
		this.m_UniqueObjects.TryGetValue(name, out result);
		return result;
	}

	public void EnableObject(GameObject obj)
	{
		if (obj)
		{
			obj.SetActive(true);
			Item component = obj.GetComponent<Item>();
			if (component && !component.m_Registered)
			{
				component.Initialize(true);
			}
		}
	}

	public void DisableObject(GameObject obj)
	{
		if (obj)
		{
			obj.SetActive(false);
			Item component = obj.GetComponent<Item>();
			if (component && component.m_Registered)
			{
				component.ItemsManagerUnregister();
			}
		}
	}

	public bool IsObjectEnabled(GameObject obj)
	{
		return obj && obj.activeSelf;
	}

	public bool IsObjectInRange(GameObject obj, GameObject dest, float range)
	{
		return obj && dest && Vector3.Distance(obj.transform.position, dest.transform.position) <= range;
	}

	public void AttachObject(GameObject obj, GameObject parent)
	{
		if (!obj || !parent)
		{
			return;
		}
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Collider component2 = obj.GetComponent<Collider>();
		if (component2 != null)
		{
			component2.isTrigger = true;
		}
		Item component3 = obj.GetComponent<Item>();
		if (component3)
		{
			Quaternion rhs = Quaternion.Inverse(component3.m_Holder.localRotation);
			Vector3 b = component3.m_Holder.parent.position - component3.m_Holder.position;
			component3.gameObject.transform.rotation = parent.transform.rotation;
			component3.gameObject.transform.rotation *= rhs;
			component3.gameObject.transform.position = parent.transform.position;
			component3.gameObject.transform.position -= b;
			component3.gameObject.transform.parent = parent.transform;
		}
		else
		{
			obj.transform.position = parent.transform.position;
			obj.transform.rotation = parent.transform.rotation;
			obj.transform.parent = parent.transform;
		}
		if (!this.m_AttachMap.ContainsKey(obj))
		{
			if (component3)
			{
				component3.StaticPhxRequestAdd();
			}
			this.m_AttachMap.Add(obj, 30);
		}
	}

	private void UpdateAttach()
	{
		for (int i = 0; i < this.m_AttachMap.Count; i++)
		{
			GameObject gameObject = this.m_AttachMap.Keys.ElementAt(i);
			if (!gameObject)
			{
				this.m_AttachMap.Clear();
				return;
			}
			if (gameObject.transform.parent != null)
			{
				this.AttachObject(gameObject, gameObject.transform.parent.gameObject);
			}
			Dictionary<GameObject, int> attachMap = this.m_AttachMap;
			GameObject key = gameObject;
			int value = attachMap[key] - 1;
			attachMap[key] = value;
			if (this.m_AttachMap[gameObject] <= 0)
			{
				this.m_AttachMap.Remove(gameObject);
			}
		}
	}

	public void SetSlowMotionFactor(float factor)
	{
		this.m_WantedSlowMotionFactor = factor;
		this.m_ChangeSlowMotionTime = Time.unscaledTime;
	}

	public static float GetTerrainY(Vector3 pos)
	{
		return Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
	}

	public void UpdateCurentTimeInMinutes()
	{
		this.m_CurentTimeInMinutes = Mathf.Floor((float)(this.m_TODSky.Cycle.Year - 2016)) * 12f * 30f * 24f * 60f + Mathf.Floor((float)this.m_TODSky.Cycle.Month) * 30f * 24f * 60f + Mathf.Floor((float)this.m_TODSky.Cycle.Day) * 24f * 60f + Mathf.Floor(this.m_TODSky.Cycle.Hour) * 60f + (float)this.m_TODSky.Cycle.DateTime.Minute + (float)this.m_TODSky.Cycle.DateTime.Second / 60f;
	}

	public float GetCurrentTimeMinutes()
	{
		return this.m_CurentTimeInMinutes;
	}

	public void EnableAtmosphereAndCloudsUpdate(bool enable)
	{
		this.m_TODAtmosphereAndCloudsUpdateEnabled = enable;
	}

	public void Save()
	{
		SaveGame.SaveVal("GameTime", MainLevel.s_GameTime);
		SaveGame.SaveVal("Day", MainLevel.Instance.m_TODSky.Cycle.Day);
		SaveGame.SaveVal("Hour", MainLevel.Instance.m_TODSky.Cycle.Hour);
		SaveGame.SaveVal("Month", MainLevel.Instance.m_TODSky.Cycle.Month);
		SaveGame.SaveVal("Year", MainLevel.Instance.m_TODSky.Cycle.Year);
		SaveGame.SaveVal("GameMode", (int)this.m_GameMode);
		SaveGame.SaveVal("DayTimeProgress", this.m_TODTime.ProgressTime);
		SaveGame.SaveVal("Tutorial", this.m_Tutorial);
		SaveGame.SaveVal("SkipTutorial", ScenarioManager.Get().m_SkipTutorial);
		SaveGame.SaveVal("SaveGameBlocked", this.m_SaveGameBlocked);
		SaveGame.SaveVal("RainVolume", this.m_RainVolume);
		foreach (Elevator elevator in Elevator.s_AllElevators)
		{
			elevator.Save();
		}
	}

	public void Load()
	{
		Music.Get().StopAll();
		MainLevel.s_GameTime = SaveGame.LoadFVal("GameTime");
		MainLevel.Instance.m_TODSky.Cycle.Day = SaveGame.LoadIVal("Day");
		MainLevel.Instance.m_TODSky.Cycle.Hour = SaveGame.LoadFVal("Hour");
		MainLevel.Instance.m_TODSky.Cycle.Month = SaveGame.LoadIVal("Month");
		MainLevel.Instance.m_TODSky.Cycle.Year = SaveGame.LoadIVal("Year");
		this.m_GameMode = (GameMode)SaveGame.LoadIVal("GameMode");
		this.m_TODTime.ProgressTime = SaveGame.LoadBVal("DayTimeProgress");
		this.m_Tutorial = SaveGame.LoadBVal("Tutorial");
		ScenarioManager.Get().m_SkipTutorial = SaveGame.LoadBVal("SkipTutorial");
		this.UpdateCurentTimeInMinutes();
		this.m_SaveGameBlocked = SaveGame.LoadBVal("SaveGameBlocked");
		if (SaveGame.FValExist("RainVolume"))
		{
			this.m_RainVolume = SaveGame.LoadFVal("RainVolume");
		}
		else
		{
			this.m_RainVolume = 1f;
		}
		this.MSAmbientStart(0.3f);
	}

	public void LoadDayTime()
	{
		MainLevel.Instance.m_TODSky.Cycle.Day = SaveGame.LoadIVal("Day");
		MainLevel.Instance.m_TODSky.Cycle.Hour = SaveGame.LoadFVal("Hour");
		MainLevel.Instance.m_TODSky.Cycle.Month = SaveGame.LoadIVal("Month");
		MainLevel.Instance.m_TODSky.Cycle.Year = SaveGame.LoadIVal("Year");
		this.UpdateCurentTimeInMinutes();
	}

	public void PlayMovie(string movie_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		bool flag = HUDMovie.Get().PlayMovie(movie_name);
		this.m_IsMoviePlaying = flag;
		if (flag)
		{
			this.Pause(true);
			this.UpdateTimeScale();
		}
	}

	public void PlayMovieWithFade(string movie_name, float volume)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		bool isMoviePlaying = HUDMovie.Get().PlayMovieWithFade(movie_name, volume);
		this.m_IsMoviePlaying = isMoviePlaying;
	}

	public void OnStopMovie()
	{
		this.m_IsMoviePlaying = false;
		this.Pause(false);
	}

	public bool IsMoviePlaying()
	{
		return this.m_IsMoviePlaying;
	}

	public bool IsMovieWithFadePlaying()
	{
		return HUDMovie.Get().GetMovieType() == MovieType.WithFade && HUDMovie.Get().GetState() > MovieWithFadeState.None && HUDMovie.Get().GetState() < MovieWithFadeState.PostFadeIn;
	}

	public bool IsMovieWithFadeFinished()
	{
		return HUDMovie.Get().GetMovieType() == MovieType.None || HUDMovie.Get().GetState() == MovieWithFadeState.PostFadeIn;
	}

	public void ScenarioPerformSave()
	{
		if (!ReplTools.IsPlayingAlone() && !ReplTools.AmIMaster())
		{
			return;
		}
		if (SaveGame.m_State == SaveGame.State.None)
		{
			SaveGame.Save();
		}
	}

	public bool IsNight()
	{
		return this.m_TODSky.Cycle.Hour < 5f || this.m_TODSky.Cycle.Hour > 22f;
	}

	private void UpdateLoading()
	{
		if (LoadingScreen.Get() != null && LoadingScreen.Get().m_Active && LoadingScreen.Get().m_State != LoadingScreenState.ReturnToMainMenu)
		{
			bool flag = true;
			if (Time.unscaledTime - Player.Get().GetTeleportTime() < 1f)
			{
				flag = false;
			}
			if (this.m_Streamers.Count > 0)
			{
				for (int i = 0; i < this.m_Streamers.Count; i++)
				{
					if (this.m_Streamers[i].IsSomethingToLoad())
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				this.m_HideLoadingScreenFrameCount++;
			}
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.PreloadScheduled && !fadeSystem.m_FadeIn && !fadeSystem.m_FadingIn)
			{
				SaveGame.PlayerLoad();
				if (ItemsManager.Get())
				{
					ItemsManager.Get().Preload();
				}
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.PreloadCompleted)
			{
				if (SaveGame.s_ExpectedGameMode == GameMode.Story)
				{
					GreenHellGame.Instance.m_GameMode = GameMode.Story;
					SceneLoadUnloadRequestHolder.Get().LoadScene("Story", SceneLoadUnloadRequest.Reason.SavegameLoad);
				}
				else if (SaveGame.s_ExpectedGameMode == GameMode.Survival)
				{
					GreenHellGame.Instance.m_GameMode = GameMode.Survival;
					SceneLoadUnloadRequestHolder.Get().UnloadScene("Story", SceneLoadUnloadRequest.Reason.SavegameLoad);
				}
				GreenHellGame.Instance.m_LoadGameState = LoadGameState.ScenePreparation;
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.ScenePreparation)
			{
				if (!SceneLoadUnloadRequestHolder.Get().IsAnyRequest(SceneLoadUnloadRequest.Reason.Any, SceneLoadUnloadRequest.OpType.Any, "Story"))
				{
					GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadScheduled;
				}
			}
			else if (this.m_HideLoadingScreenFrameCount > 5 && GreenHellGame.Instance.m_LoadGameState == LoadGameState.FullLoadScheduled)
			{
				if (HUDManager.Get())
				{
					HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
				}
				SaveGame.FullLoad();
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.FullLoadWaitingForScenario)
			{
				SaveGame.UpdateFullLoadWaitingForScenario();
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.FullLoadCompleted)
			{
				GreenHellGame.Instance.m_LoadGameState = LoadGameState.None;
			}
			if (this.m_HideLoadingScreenFrameCount > 30 && GreenHellGame.Instance.m_LoadGameState == LoadGameState.None && !fadeSystem.m_FadeIn && !fadeSystem.m_FadingIn && !fadeSystem.m_FadeOut && !fadeSystem.m_FadingOut)
			{
				this.ApplyFixes();
				fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnLoadingEndFadeOut), 2f, null);
			}
		}
	}

	public void OnLoadingEndFadeOut()
	{
		LoadingScreen.Get().Hide();
		this.m_HideLoadingScreenFrameCount = 0;
		AmbientAudioSystem.Instance.StartRainForestAmbienceMultisample();
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 2f);
		if (Player.Get().m_MovesBlockedOnChangeScene)
		{
			Player.Get().UnblockMoves();
			Player.Get().m_MovesBlockedOnChangeScene = false;
		}
		Player.Get().SetupActiveController();
		GreenHellGame.Instance.m_Settings.ApplyAntiAliasing();
		GreenHellGame.Instance.m_Settings.ApplyFOVChange();
	}

	public void StartMultisample(string name, float fade_in)
	{
		MSManager.Get().PlayMultiSample(this, name, fade_in);
	}

	public void StopMultisample(string ms_name, float fade_out)
	{
		MSMultiSample msmultiSample = MSManager.Get().FindMultisample(ms_name);
		if (msmultiSample != null)
		{
			msmultiSample.Stop(this, fade_out);
		}
	}

	public void StopDayTimeProgress()
	{
		this.m_TODTime.ProgressTime = false;
	}

	public void StartDayTimeProgress()
	{
		this.m_TODTime.ProgressTime = true;
	}

	public void SetDayTime(int hour, int minutes)
	{
		if (Scenario.Get().m_IsLoading)
		{
			return;
		}
		this.m_TODSky.Cycle.Hour = (float)hour + 1.66666663f * ((float)minutes / 100f);
	}

	public void OnFallenObjectsManagerInitialized()
	{
	}

	public void OnStaticObjectsManagerInitialized()
	{
	}

	private void UpdateFog()
	{
		if (FogSensor.s_NumEnters > 0)
		{
			RenderSettings.fogStartDistance += (FogSensor.s_FogDensityStart - RenderSettings.fogStartDistance) * Time.deltaTime;
			RenderSettings.fogEndDistance += (FogSensor.s_FogDensityEnd - RenderSettings.fogEndDistance) * Time.deltaTime;
			return;
		}
		float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_FogStartDistance, 10f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
		RenderSettings.fogStartDistance += (proportionalClamp - RenderSettings.fogStartDistance) * Time.deltaTime;
		proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_FogEndDistance, 150f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
		RenderSettings.fogEndDistance += (proportionalClamp - RenderSettings.fogEndDistance) * Time.deltaTime;
	}

	private void UpdateEmissiveMaterials()
	{
		bool isDay = this.m_TODSky.IsDay;
		for (int i = 0; i < this.m_EmissiveMaterials.Count; i++)
		{
			if (isDay)
			{
				this.m_EmissiveMaterials[i].SetColor(this.m_EmmisiveColorProperty, Color.black);
			}
			else
			{
				this.m_EmissiveMaterials[i].SetColor(this.m_EmmisiveColorProperty, this.m_EmissiveColor);
			}
		}
	}

	public void ScenarioFadeOutWithScreen(string prefab_name, float duration)
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			GameObject screen_prefab = Resources.Load<GameObject>("Prefabs/Systems/" + prefab_name);
			fadeSystem.FadeOut(FadeType.All, null, duration, screen_prefab);
		}
	}

	public void ScenarioFadeOut(float duration)
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, null, duration, null);
		}
	}

	public void ScenarioFadeIn(float duration)
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			if (fadeSystem.m_VisFadeLevel > 0f || fadeSystem.m_FadeOut || fadeSystem.m_FadingOut)
			{
				fadeSystem.FadeIn(FadeType.All, null, duration);
			}
		}
	}

	public bool IsFadeOut()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		return fadeSystem.m_FadeOut || fadeSystem.m_FadingOut;
	}

	public bool IsFadeIn()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		return fadeSystem.m_FadeIn || fadeSystem.m_FadingIn;
	}

	public void DreamFadeOut(float duration)
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		huddreamFade.FadeOut(FadeType.All, duration);
		huddreamFade.SetImagesColor(Color.black);
	}

	public void DreamFadeIn(float duration)
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		huddreamFade.FadeIn(FadeType.All, duration);
		huddreamFade.SetImagesColor(Color.black);
	}

	public void DreamWhiteFadeOut(float duration)
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		huddreamFade.FadeOut(FadeType.All, duration);
		huddreamFade.SetImagesColor(Color.white);
	}

	public void DreamWhiteFadeIn(float duration)
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		huddreamFade.FadeIn(FadeType.All, duration);
		huddreamFade.SetImagesColor(Color.white);
	}

	public bool IsDreamFadeOut()
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		return huddreamFade.m_FadeOut || huddreamFade.m_FadingOut;
	}

	public bool IsDreamFadeIn()
	{
		HUDDreamFade huddreamFade = HUDDreamFade.Get();
		return huddreamFade.m_FadeIn || huddreamFade.m_FadingIn;
	}

	public bool IsDebugCutscenePlaying()
	{
		return this.m_CutscenePlaying;
	}

	private void UpdateDebugCutscene()
	{
		if (this.m_CutscenePlaying && Time.realtimeSinceStartup - this.m_CutsceneStartTime > 5f)
		{
			this.EndDebugCutscene();
		}
	}

	private void EndDebugCutscene()
	{
		this.m_CutscenePlaying = false;
		this.Pause(false);
		this.m_CutsceneName = string.Empty;
	}

	public void ResetGameBeforeLoad()
	{
		if (CameraManager.Get())
		{
			CameraManager.Get().SetZoom(0f);
		}
		if (HintsManager.Get())
		{
			HintsManager.Get().HideAllHints();
		}
		if (ObjectivesManager.Get())
		{
			ObjectivesManager.Get().DeactivateAllActiveObjectives();
		}
		Player.Get().SetDreamPPActive(false);
		this.EnableSkyDome();
		Player.Get().SetSpeedMul(1f);
		HUDBase[] hudlist = HUDManager.Get().m_HUDList;
		for (int i = 0; i < hudlist.Length; i++)
		{
			hudlist[i].ScenarioUnblock();
		}
		FistFightController.Get().UnblockFight();
		Music.Get().StopAll();
		MusicJingle.Get().StoppAll();
		MSManager.Get().StopAllMultiSamples();
		this.EnableTerrainRendering(true);
		Watch watch = Watch.Get();
		if (watch == null)
		{
			return;
		}
		watch.ClearFakeDate();
	}

	private void TeleportPlayerOnStart()
	{
		if (GreenHellGame.Instance.m_GameMode == GameMode.Survival)
		{
			GameObject target = GameObject.Find("Survival_Start");
			Player.Get().Teleport(target, false);
		}
		else if (GreenHellGame.Instance.m_GameMode == GameMode.Story)
		{
			GameObject target2 = GameObject.Find("Survival_Start");
			Player.Get().Teleport(target2, false);
		}
		Player.Get().m_RespawnPosition = Player.Get().GetWorldPosition();
	}

	private void UpdateAA()
	{
		if (this.m_PostProcessLayer == null)
		{
			return;
		}
		if (Player.Get().GetComponent<NotepadController>().IsActive())
		{
			this.m_PostProcessLayer.temporalAntialiasing.stationaryBlending = 0.6f;
			return;
		}
		this.m_PostProcessLayer.temporalAntialiasing.stationaryBlending = this.m_DefaultStationaryBlending;
	}

	private void SleepFlocks()
	{
	}

	public void RegisterStreamer(Streamer streamer)
	{
		this.m_Streamers.Add(streamer);
	}

	public void UnRegisterStreamer(Streamer streamer)
	{
		this.m_Streamers.Remove(streamer);
	}

	public List<Streamer> GetStreamers()
	{
		return this.m_Streamers;
	}

	public void ScenarioSetTutorial(bool set)
	{
		this.m_Tutorial = set;
		ScenarioManager.Get().SetBoolVariable("IsTutorialFinished", !set);
	}

	public bool IsAnyStreamerLoading()
	{
		if (this.m_Streamers.Count > 0)
		{
			for (int i = 0; i < this.m_Streamers.Count; i++)
			{
				if (this.m_Streamers[i].IsSomethingToLoad())
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnDestroy()
	{
		if (TwitchDemoManager.Get() != null)
		{
			TwitchDemoManager.Get().Destroy();
		}
		FishTank.s_FishTanks.Clear();
		HumanAIGroup.s_AIGroups.Clear();
		AISoundModule.ClearCache();
		Firecamp.s_Firecamps.Clear();
		FirecampRack.s_FirecampRacks.Clear();
		Food.s_AllFoods.Clear();
		Item.s_AllItems.Clear();
		Item.s_AllItemIDs.Clear();
		ItemHold.s_AllItemHolds.Clear();
		Trigger.s_ActiveTriggers.Clear();
		Trigger.s_AllTriggers.Clear();
		ItemReplacer.s_ToreplaceByDistance.Clear();
		ItemReplacer.s_AllReplacers.Clear();
		ItemSlot.s_AllItemSlots.Clear();
		ItemSlot.s_ActiveItemSlots.Clear();
		EventsManager.m_Receivers.Clear();
		EventsManager.m_ReceiversToAdd.Clear();
		EventsManager.m_ReceiversToRemove.Clear();
		NoiseManager.s_Receivers.Clear();
		Storage.s_AllStorages.Clear();
		RainCutter.s_AllRainCutters.Clear();
		Generator.s_AllGenerators.Clear();
		LootBox.s_AllLootBoxes.Clear();
		SaveGame.m_SaveGameVersion = new GameVersion(0, 0);
		SceneLoadUnloadRequestHolder.OnSceneLoad -= this.OnSceneLoad;
		SceneLoadUnloadRequestHolder.OnSceneUnload -= this.OnSceneUnload;
	}

	public void OnFullLoadEnd()
	{
		foreach (Elevator elevator in Elevator.s_AllElevators)
		{
			elevator.Load();
		}
		FallenObjectsManager.Get().OnFullLoadEnd();
		ScenarioManager.Get().OnFullLoadEnd();
		TriggersManager.Get().Load();
		BalanceSystem20.Get().OnFullLoadEnd();
	}

	private void ApplyFixes()
	{
		if (SaveGame.m_SaveGameVersion == GreenHellGame.s_GameVersionEarlyAccessUpdate6)
		{
			GameObject gameObject = GameObject.Find("PlayerProggresion");
			if (gameObject == null)
			{
				return;
			}
			Transform transform = gameObject.transform.FindDeepChild("Refugees_Camp");
			if (transform == null)
			{
				return;
			}
			Transform transform2 = transform.FindDeepChild("refugees_items");
			Transform x = transform.FindDeepChild("shrimp_trap");
			if (transform2.childCount > 1 || x != null)
			{
				return;
			}
			for (int i = 0; i < transform.transform.childCount; i++)
			{
				if (transform.transform.GetChild(i).gameObject.name == "Unlock_shrimp_Trap")
				{
					SensorBase component = transform.transform.GetChild(i).gameObject.GetComponent<SensorBase>();
					if (component)
					{
						component.SetWasInside(false);
						component.gameObject.SetActive(true);
					}
				}
				else
				{
					UnityEngine.Object.Destroy(transform.transform.GetChild(i).gameObject);
				}
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab("WaterUpdate_CampItems"), Vector3.zero, Quaternion.identity);
			for (int j = 0; j < gameObject2.transform.childCount; j++)
			{
				gameObject2.transform.GetChild(j).SetParent(transform);
			}
		}
		if (SaveGame.m_SaveGameVersion > GreenHellGame.s_GameVersionEarlyAccessUpdate6 && SaveGame.m_SaveGameVersion <= GreenHellGame.s_GameVersionEarlyAccessUpdate7)
		{
			GameObject gameObject3 = GameObject.Find("PlayerProggresion");
			if (gameObject3 == null)
			{
				return;
			}
			Transform transform3 = gameObject3.transform.FindDeepChild("Refugees_Camp");
			if (transform3 == null)
			{
				return;
			}
			for (int k = 0; k < transform3.transform.childCount; k++)
			{
				if (transform3.transform.GetChild(k).gameObject.name == "Unlock_shrimp_Trap")
				{
					SensorBase component2 = transform3.transform.GetChild(k).gameObject.GetComponent<SensorBase>();
					if (component2)
					{
						component2.SetWasInside(false);
						component2.gameObject.SetActive(true);
					}
				}
			}
		}
	}

	public void PostProcessVolumeExDisableWithFade(GameObject obj, float fade_time)
	{
		PostProcessVolumeEx component = obj.GetComponent<PostProcessVolumeEx>();
		DebugUtils.Assert(component, true);
		if (component)
		{
			component.m_FadeOutDuration = fade_time;
			component.m_FadeOutStartTime = Time.time;
		}
	}

	public void DisableSkyDome()
	{
		this.m_TODSky.gameObject.SetActive(false);
	}

	public void EnableSkyDome()
	{
		this.m_TODSky.gameObject.SetActive(true);
	}

	public void TODInterpolatorStoreTime()
	{
		this.m_TODInterpolatorStoredHour = this.m_TODSky.Cycle.Hour;
	}

	public void TODInterpolatorInterpolate(float hour, float duration)
	{
		this.m_TODInterpolatorState = MainLevel.TODTimeInterpolatorState.Interpolating;
		this.m_TODInterpolatorWantedHour = hour;
		this.m_TODInterpolatorStartTime = Time.time;
		this.m_TODInterpolatorStartHour = this.m_TODSky.Cycle.Hour;
		this.m_TODInterpolatorInterpolationDuration = duration;
	}

	public void TODInterpolatorRestore(float duration)
	{
		this.m_TODInterpolatorState = MainLevel.TODTimeInterpolatorState.Restoring;
		this.m_TODInterpolatorWantedHour = this.m_TODInterpolatorStoredHour;
		this.m_TODInterpolatorStartTime = Time.time;
		this.m_TODInterpolatorStartHour = this.m_TODSky.Cycle.Hour;
		this.m_TODInterpolatorInterpolationDuration = duration;
	}

	private void TODInterpolatorUpdate()
	{
		if (this.m_TODInterpolatorState == MainLevel.TODTimeInterpolatorState.Interpolating || this.m_TODInterpolatorState == MainLevel.TODTimeInterpolatorState.Restoring)
		{
			float proportionalClamp;
			if (this.m_TODInterpolatorCurve != null)
			{
				float time = (this.m_TODInterpolatorInterpolationDuration != 0f) ? ((Time.time - this.m_TODInterpolatorStartTime) / this.m_TODInterpolatorInterpolationDuration) : 0f;
				float b = this.m_TODInterpolatorCurve.Evaluate(time);
				proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_TODInterpolatorStartHour, this.m_TODInterpolatorWantedHour, b, 0f, 1f);
			}
			else
			{
				proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_TODInterpolatorStartHour, this.m_TODInterpolatorWantedHour, Time.time, this.m_TODInterpolatorStartTime, this.m_TODInterpolatorStartTime + this.m_TODInterpolatorInterpolationDuration);
			}
			float hours = proportionalClamp - this.m_TODSky.Cycle.Hour;
			this.m_TODTime.AddHours(hours, true, true);
		}
		if (Time.time > this.m_TODInterpolatorStartTime + this.m_TODInterpolatorInterpolationDuration)
		{
			this.m_TODInterpolatorState = MainLevel.TODTimeInterpolatorState.None;
		}
	}

	public void MSAmbientStart(float fade_in)
	{
		AmbientAudioSystem.Instance.MSAmbientStart(fade_in);
	}

	public void MSAmbientStop(float fade_out)
	{
		AmbientAudioSystem.Instance.MSAmbientStop(fade_out);
	}

	public void EnableTerrainRendering(bool set)
	{
		if (Terrain.activeTerrain)
		{
			Terrain.activeTerrain.drawHeightmap = set;
		}
	}

	public bool IsGeneratorFull(GameObject obj)
	{
		Generator component = obj.GetComponent<Generator>();
		return component && component.IsFull();
	}

	public bool AreStreamersLoaded()
	{
		bool result = true;
		if (Time.unscaledTime - Player.Get().GetTeleportTime() < 1f)
		{
			result = false;
		}
		if (this.m_Streamers.Count > 0)
		{
			for (int i = 0; i < this.m_Streamers.Count; i++)
			{
				if (this.m_Streamers[i].IsSomethingToLoad())
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public void BlockSaveGame()
	{
		this.m_SaveGameBlocked = true;
	}

	public void UnblockSaveGame()
	{
		this.m_SaveGameBlocked = false;
	}

	public void SetRainVolume(float vol)
	{
		this.m_RainVolume = vol;
	}

	public void OnSceneLoad(Scene scene, SceneLoadUnloadRequest request)
	{
		if (request.m_SceneName.StartsWith("Dream"))
		{
			HxVolumetricCamera component = this.m_MainCamera.GetComponent<HxVolumetricCamera>();
			if (component)
			{
				component.enabled = true;
			}
		}
	}

	public void OnSceneUnload(Scene scene, SceneLoadUnloadRequest request)
	{
		if (request.m_SceneName.StartsWith("Dream"))
		{
			HxVolumetricCamera component = this.m_MainCamera.GetComponent<HxVolumetricCamera>();
			if (component)
			{
				component.enabled = false;
			}
		}
	}

	private static MainLevel s_Instance = null;

	public TOD_Sky m_TODSky;

	public TOD_Time m_TODTime;

	public Camera m_MainCamera;

	private int m_TimeScaleMode;

	private float m_WantedSlowMotionFactor = 1f;

	private float m_SlowMotionFactor = 1f;

	private float m_ChangeSlowMotionTime;

	private int m_PauseRequestsCount;

	private List<GameObject> m_AllObjects = new List<GameObject>();

	private Dictionary<string, GameObject> m_UniqueObjects = new Dictionary<string, GameObject>();

	private static string s_AnimalSoundsDayName = "jungle_day_animal_random_";

	private static string s_AnimalSoundsNightName = "jungle_night_animal_random_";

	private bool m_DebugShowCursor;

	private bool m_WasPausedLastFrame;

	private bool m_DebugPause;

	private bool m_IsMoviePlaying;

	private Dictionary<GameObject, int> m_AttachMap = new Dictionary<GameObject, int>();

	public GameObject m_CutscenePlayer;

	public List<Material> m_EmissiveMaterials = new List<Material>();

	private int m_EmmisiveColorProperty = -1;

	private float m_LastEmissionColorChangeHour = -1f;

	private Color m_EmissiveColor = Color.white;

	private float m_FogStartDistance;

	private float m_FogEndDistance;

	private int m_HideLoadingScreenFrameCount;

	[HideInInspector]
	public bool m_CutscenePlaying;

	private float m_CutsceneStartTime;

	private const float m_CutsceneDuration = 5f;

	[HideInInspector]
	public string m_CutsceneName = string.Empty;

	[HideInInspector]
	public GameMode m_GameMode;

	private PostProcessLayer m_PostProcessLayer;

	private float m_DefaultStationaryBlending;

	[HideInInspector]
	public bool m_Tutorial;

	private AudioSource[] audio_sources;

	[HideInInspector]
	public float m_LevelStartTime = float.MaxValue;

	private NGSS_Directional m_NGSSDirectional;

	[HideInInspector]
	public GrassManager m_GrassManager;

	public AsyncOperation m_UnusedAssetsAsyncOperation;

	public List<AsyncOperation> m_SceneAsyncOperation = new List<AsyncOperation>();

	private float m_CurentTimeInMinutes;

	public Scene m_LevelScene;

	public AnimationCurve m_SoundRolloffCurve;

	public Dictionary<string, List<Vector3>> m_SceneItems = new Dictionary<string, List<Vector3>>();

	public float m_LastUnpauseTime;

	[HideInInspector]
	public bool m_TODAtmosphereAndCloudsUpdateEnabled = true;

	private List<Streamer> m_Streamers = new List<Streamer>();

	public MainLevel.TODTimeInterpolatorState m_TODInterpolatorState;

	private float m_TODInterpolatorStoredHour;

	private float m_TODInterpolatorWantedHour;

	private float m_TODInterpolatorStartTime;

	private float m_TODInterpolatorStartHour;

	private float m_TODInterpolatorInterpolationDuration = 1f;

	public AnimationCurve m_TODInterpolatorCurve;

	[HideInInspector]
	public bool m_SaveGameBlocked;

	[HideInInspector]
	public float m_RainVolume = 1f;

	public enum TODTimeInterpolatorState
	{
		None,
		Interpolating,
		Restoring
	}
}
