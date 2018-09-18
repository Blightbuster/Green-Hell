using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedTerrainGrass;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MainLevel : MonoBehaviour, ISaveLoad
{
	private MainLevel()
	{
		MainLevel.s_Instance = this;
	}

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
	}

	private void Start()
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
	}

	private void ShowModeMenu()
	{
		MenuInGameManager.Get().ShowScreen(typeof(MenuDebugSelectMode));
	}

	public void Initialize()
	{
		this.m_AllObjects.Clear();
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(GameObject));
		foreach (GameObject gameObject in array)
		{
			if (gameObject.scene.IsValid())
			{
				this.m_AllObjects.Add(gameObject);
				if (!this.m_UniqueObjects.ContainsKey(gameObject.name))
				{
					this.m_UniqueObjects.Add(gameObject.name, gameObject);
				}
			}
		}
		if (this.m_AnimalSoundsAudioSource == null)
		{
			this.m_AnimalSoundsAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AnimalSoundsAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			this.m_AnimalSounds.Clear();
			List<AudioClip> value = new List<AudioClip>();
			List<AudioClip> value2 = new List<AudioClip>();
			this.m_AnimalSounds.Add(true, value2);
			this.m_AnimalSounds.Add(false, value);
			for (int j = 1; j < 24; j++)
			{
				string text = MainLevel.s_AnimalSoundsDayName;
				if (j < 10)
				{
					text += "0";
				}
				text += j.ToString();
				AudioClip item = Resources.Load(MSSample.s_SamplesPath + text) as AudioClip;
				this.m_AnimalSounds[true].Add(item);
			}
			for (int k = 1; k < 22; k++)
			{
				string text2 = MainLevel.s_AnimalSoundsNightName;
				if (k < 10)
				{
					text2 += "0";
				}
				text2 += k.ToString();
				AudioClip item2 = Resources.Load(MSSample.s_SamplesPath + text2) as AudioClip;
				this.m_AnimalSounds[false].Add(item2);
			}
		}
		this.TeleportPlayerOnStart();
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
		ChallengesManager.Get().OnLevelLoaded();
		ScenarioManager.Get().Initialize();
		if (GreenHellGame.Instance.m_FromSave)
		{
			SaveGame.Load();
		}
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			Cheats.m_GodMode = true;
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
		CursorManager.Get().ShowCursor(false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_NGSSDirectional = this.m_TODSky.gameObject.GetComponentInChildren<NGSS_Directional>();
		this.m_GrassManager = Terrain.activeTerrain.GetComponent<GrassManager>();
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
		this.UpdateAnimalsSound();
		this.UpdateLoading();
		if (GreenHellGame.ROADSHOW_DEMO && Input.GetKeyDown(KeyCode.Escape))
		{
			this.m_DebugPause = !this.m_DebugPause;
			CursorManager.Get().ShowCursor(this.m_DebugPause);
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
			this.m_TODTime.AddHours(1f, true);
			ReflectionProbeUpdater[] array = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			this.m_TODTime.AddHours(-1f, true);
			ReflectionProbeUpdater[] array2 = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			this.m_TODTime.AddHours(10f, true);
			ReflectionProbeUpdater[] array3 = UnityEngine.Object.FindObjectsOfType<ReflectionProbeUpdater>();
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k].RenderProbe();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Pause))
		{
			this.m_DebugPause = !this.m_DebugPause;
			CursorManager.Get().ShowCursor(this.m_DebugPause);
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
		}
		else if (Input.GetKeyDown(KeyCode.Semicolon))
		{
			this.PlayMovieWithFade("Tutorial_End");
		}
		else if (Input.GetKeyDown(KeyCode.Minus))
		{
			this.m_DebugShowCursor = !this.m_DebugShowCursor;
			CursorManager.Get().ShowCursor(this.m_DebugShowCursor);
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
				Debug.Log("ParticleSystem " + array4[l].gameObject.name + ((!activeInHierarchy) ? " 0" : " 1"));
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
	}

	private void CycleTimeScaleSpeedup()
	{
		if (this.m_TimeScaleMode == 0)
		{
			this.m_TimeScaleMode = 1;
		}
		else
		{
			this.m_TimeScaleMode = 0;
		}
	}

	private void CycleTimeScaleSlowdown()
	{
		if (this.m_TimeScaleMode == 2)
		{
			this.m_TimeScaleMode = 0;
		}
		else
		{
			this.m_TimeScaleMode = 2;
		}
	}

	private void UpdateSlowMotion()
	{
		if (this.m_SlowMotionFactor != this.m_WantedSlowMotionFactor)
		{
			float b = Time.unscaledTime - this.m_ChangeSlowMotionTime;
			this.m_SlowMotionFactor = CJTools.Math.GetProportionalClamp(this.m_SlowMotionFactor, this.m_WantedSlowMotionFactor, b, 0f, 1f);
		}
	}

	private void UpdateTimeScale()
	{
		float num = 1f;
		if (this.IsPause())
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
		}
	}

	public void DisableObject(GameObject obj)
	{
		if (obj)
		{
			obj.SetActive(false);
		}
	}

	public bool IsObjectInRange(GameObject obj, GameObject dest, float range)
	{
		if (!obj || !dest)
		{
			return false;
		}
		float num = Vector3.Distance(obj.transform.position, dest.transform.position);
		return num <= range;
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
				break;
			}
			if (gameObject.transform.parent != null)
			{
				this.AttachObject(gameObject, gameObject.transform.parent.gameObject);
			}
			Dictionary<GameObject, int> attachMap;
			GameObject key;
			(attachMap = this.m_AttachMap)[key = gameObject] = attachMap[key] - 1;
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

	private void UpdateCurentTimeInMinutes()
	{
		this.m_CurentTimeInMinutes = Mathf.Floor((float)(this.m_TODSky.Cycle.Year - 2016)) * 12f * 30f * 24f * 60f + Mathf.Floor((float)this.m_TODSky.Cycle.Month) * 30f * 24f * 60f + Mathf.Floor((float)this.m_TODSky.Cycle.Day) * 24f * 60f + Mathf.Floor(this.m_TODSky.Cycle.Hour) * 60f + (float)this.m_TODSky.Cycle.DateTime.Minute + (float)this.m_TODSky.Cycle.DateTime.Second / 60f;
	}

	public float GetCurrentTimeMinutes()
	{
		return this.m_CurentTimeInMinutes;
	}

	private void UpdateAnimalsSound()
	{
		if (this.m_AnimalSoundsAudioSource == null)
		{
			return;
		}
		if (Time.time < this.m_AnimalSoundNextTime)
		{
			return;
		}
		bool key = !this.IsNight();
		AudioClip audioClip = this.m_AnimalSounds[key][UnityEngine.Random.Range(0, this.m_AnimalSounds[key].Count)];
		if (audioClip)
		{
			this.m_AnimalSoundsAudioSource.clip = audioClip;
			this.m_AnimalSoundsAudioSource.loop = false;
			this.m_AnimalSoundsAudioSource.volume = 0.7f;
			this.m_AnimalSoundsAudioSource.Play();
			this.m_AnimalSoundNextTime = Time.time + audioClip.length + UnityEngine.Random.Range(35f, 45f);
		}
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
	}

	public void Load()
	{
		MainLevel.s_GameTime = SaveGame.LoadFVal("GameTime");
		MainLevel.Instance.m_TODSky.Cycle.Day = SaveGame.LoadIVal("Day");
		MainLevel.Instance.m_TODSky.Cycle.Hour = SaveGame.LoadFVal("Hour");
		MainLevel.Instance.m_TODSky.Cycle.Month = SaveGame.LoadIVal("Month");
		MainLevel.Instance.m_TODSky.Cycle.Year = SaveGame.LoadIVal("Year");
		this.m_GameMode = (GameMode)SaveGame.LoadIVal("GameMode");
		this.m_TODTime.ProgressTime = SaveGame.LoadBVal("DayTimeProgress");
		this.m_Tutorial = SaveGame.LoadBVal("Tutorial");
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

	public void PlayMovieWithFade(string movie_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		bool flag = HUDMovie.Get().PlayMovieWithFade(movie_name);
		this.m_IsMoviePlaying = flag;
		if (flag)
		{
			this.Pause(true);
			this.UpdateTimeScale();
		}
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
		return HUDMovie.Get().GetMovieType() == MovieType.WithFade && HUDMovie.Get().GetState() > MovieWithFadeState.None && HUDMovie.Get().GetState() < MovieWithFadeState.PostFadeOut;
	}

	public bool IsMovieWithFadeFinished()
	{
		return HUDMovie.Get().GetMovieType() == MovieType.None && HUDMovie.Get().GetState() == MovieWithFadeState.None;
	}

	public void ScenarioPerformSave()
	{
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
			if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.PreloadScheduled)
			{
				SaveGame.PlayerLoad();
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.PreloadCompleted)
			{
				GreenHellGame.Instance.m_LoadGameState = LoadGameState.FullLoadScheduled;
			}
			else if (this.m_HideLoadingScreenFrameCount > 5 && GreenHellGame.Instance.m_LoadGameState == LoadGameState.FullLoadScheduled)
			{
				SaveGame.FullLoad();
			}
			else if (GreenHellGame.Instance.m_LoadGameState == LoadGameState.FullLoadCompleted)
			{
				GreenHellGame.Instance.m_LoadGameState = LoadGameState.None;
			}
			if (this.m_HideLoadingScreenFrameCount > 5 && GreenHellGame.Instance.m_LoadGameState == LoadGameState.None)
			{
				FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
				if (!fadeSystem.m_FadeIn && !fadeSystem.m_FadingIn)
				{
					fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnLoadingEndFadeOut), 2f, null);
				}
			}
		}
	}

	public void OnLoadingEndFadeOut()
	{
		LoadingScreen.Get().Hide();
		this.m_HideLoadingScreenFrameCount = 0;
		this.StartRainForestAmbienceMultisample();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 2f);
		if (Player.Get().m_MovesBlockedOnChangeScene)
		{
			Player.Get().UnblockMoves();
			Player.Get().m_MovesBlockedOnChangeScene = false;
		}
		Player.Get().SetupActiveController();
	}

	public void StartRainForestAmbienceMultisample()
	{
		if (!this.m_RainforestMSStarted)
		{
			this.m_AmbientMS = MSManager.Get().PlayMultiSample("Rainforest_Ambience", 1f);
		}
		this.m_RainforestMSStarted = true;
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
		}
		else
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_FogStartDistance, 10f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
			RenderSettings.fogStartDistance += (proportionalClamp - RenderSettings.fogStartDistance) * Time.deltaTime;
			proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_FogEndDistance, 150f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
			RenderSettings.fogEndDistance += (proportionalClamp - RenderSettings.fogEndDistance) * Time.deltaTime;
		}
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

	public void FadeOutWithScreen(string prefab_name)
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			GameObject screen_prefab = Resources.Load<GameObject>("Prefabs/Systems/" + prefab_name);
			fadeSystem.FadeOut(FadeType.All, null, 1.5f, screen_prefab);
		}
	}

	public void FadeOut()
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			fadeSystem.FadeOut(FadeType.All, null, 1.5f, null);
		}
	}

	public void FadeIn()
	{
		if (SaveGame.m_State == SaveGame.State.None)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			fadeSystem.FadeIn(FadeType.All, null, 1.5f);
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
			GameObject target2 = GameObject.Find("StartTutorialPosition");
			Player.Get().Teleport(target2, false);
		}
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
		}
		else
		{
			this.m_PostProcessLayer.temporalAntialiasing.stationaryBlending = this.m_DefaultStationaryBlending;
		}
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

	public void ScenarioSetTutorial(bool set)
	{
		this.m_Tutorial = set;
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
		Firecamp.s_Firecamps.Clear();
		FirecampRack.s_FirecampRacks.Clear();
		Food.s_AllFoods.Clear();
		Item.s_AllItems.Clear();
		Item.s_AllItemIDs.Clear();
		Trigger.s_ActiveTriggers.Clear();
		Trigger.s_AllTriggers.Clear();
		ItemReplacer.s_ToreplaceByDistance.Clear();
		ItemSlot.s_AllItemSlots.Clear();
		ItemSlot.s_ActiveItemSlots.Clear();
		EventsManager.m_Receivers.Clear();
		EventsManager.m_ReceiversToAdd.Clear();
		EventsManager.m_ReceiversToRemove.Clear();
		NoiseManager.s_Receivers.Clear();
	}

	private static MainLevel s_Instance;

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

	private AudioSource m_AnimalSoundsAudioSource;

	private float m_AnimalSoundNextTime;

	private bool m_DebugShowCursor;

	private bool m_WasPausedLastFrame;

	private bool m_DebugPause;

	private bool m_IsMoviePlaying;

	public MSMultiSample m_AmbientMS;

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

	private GrassManager m_GrassManager;

	public AsyncOperation m_UnusedAssetsAsyncOperation;

	public List<AsyncOperation> m_SceneAsyncOperation = new List<AsyncOperation>();

	private float m_CurentTimeInMinutes;

	private Dictionary<bool, List<AudioClip>> m_AnimalSounds = new Dictionary<bool, List<AudioClip>>();

	private bool m_RainforestMSStarted;

	private List<Streamer> m_Streamers = new List<Streamer>();
}
