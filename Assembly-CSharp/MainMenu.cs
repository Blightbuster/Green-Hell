using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MainMenuScreen
{
	protected void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(GreenHellGame.Instance);
		this.SetState((!GreenHellGame.Instance.m_WasCompanyLogo) ? MainMenuState.CompanyLogo : MainMenuState.MainMenu);
		GreenHellGame.Instance.m_WasCompanyLogo = true;
	}

	private void Start()
	{
		this.m_StartStory.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_StartStory");
		this.m_StartChallenge.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_StartChallenge");
		this.m_StartSurvival.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_StartSurvival");
		this.m_LoadGame.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_LoadGame");
		this.m_Options.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_Options");
		this.m_Credits.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_Credits");
		this.m_Quit.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_Quit");
		MainMenuScreen.s_ButtonsAlpha = this.m_StartStory.GetComponentInChildren<Text>().color.a;
		MainMenuScreen.s_InactiveButtonsAlpha = MainMenuScreen.s_ButtonsAlpha * 0.3f;
		MainMenu.s_ButtonTextStartX = this.m_StartStory.GetComponentInChildren<Text>().rectTransform.position.x;
		MainMenu.s_SelectedButtonX = MainMenu.s_ButtonShiftX + MainMenu.s_ButtonTextStartX;
		CursorManager.Get().ResetCursorRequests();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_StartTime = Time.time;
		this.m_WasButtonsActive = false;
		this.StoreMenuResolution();
	}

	private void SetState(MainMenuState state)
	{
		this.m_State = state;
		this.m_StateStartTime = Time.time;
		this.OnEnterState(state);
	}

	private void OnEnterState(MainMenuState state)
	{
		Color color = Color.black;
		if (state != MainMenuState.CompanyLogo)
		{
			if (state != MainMenuState.GameLogo)
			{
				if (state == MainMenuState.MainMenu)
				{
					this.m_BG.gameObject.SetActive(true);
					this.m_CompanyLogo.gameObject.SetActive(false);
					this.m_GameLogo.gameObject.SetActive(false);
					color = this.m_BG.color;
					color.a = 1f;
					this.m_BG.color = color;
				}
			}
			else
			{
				this.m_BG.gameObject.SetActive(true);
				this.m_CompanyLogo.gameObject.SetActive(false);
				this.m_GameLogo.gameObject.SetActive(true);
				color = this.m_GameLogo.color;
				color.a = 0f;
				this.m_GameLogo.color = color;
				this.m_ButtonsEnabled = false;
			}
		}
		else
		{
			Resources.UnloadUnusedAssets();
			this.m_BG.gameObject.SetActive(true);
			this.m_CompanyLogo.gameObject.SetActive(true);
			color = this.m_CompanyLogo.color;
			color.a = 0f;
			this.m_CompanyLogo.color = color;
			this.m_GameLogo.gameObject.SetActive(false);
			this.m_ButtonsEnabled = false;
		}
	}

	private void Update()
	{
		this.UpdateInputs();
		this.UpdateState();
		this.UpdateButtons();
		this.UpdateMusic();
		this.UpdateReturnToMainMenuLoadingScreen();
	}

	private void UpdateReturnToMainMenuLoadingScreen()
	{
		if (LoadingScreen.Get() != null && LoadingScreen.Get().m_Active && LoadingScreen.Get().m_State == LoadingScreenState.ReturnToMainMenu)
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			if (!fadeSystem.m_FadeOut && !fadeSystem.m_FadingOut)
			{
				fadeSystem.FadeOut(FadeType.Vis, new VDelegate(this.OnLoadingEndFadeOut), 1.5f, null);
			}
		}
	}

	public void OnLoadingEndFadeOut()
	{
		LoadingScreen.Get().Hide();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.Vis, null, 1.5f);
	}

	private void UpdateButtons()
	{
		Color color = this.m_StartSurvival.GetComponentInChildren<Text>().color;
		Color color2 = this.m_StartSurvival.GetComponentInChildren<Text>().color;
		this.m_ButtonsActive = (this.m_State == MainMenuState.MainMenu && this.m_ButtonActivationTime > 0f && Time.time - this.m_ButtonActivationTime > this.m_ButtonsFadeInDuration);
		if (this.m_ButtonsActive && !this.m_WasButtonsActive)
		{
			CursorManager.Get().ShowCursor(true);
			this.m_WasButtonsActive = true;
		}
		float num = Mathf.Clamp01((Time.time - this.m_ButtonActivationTime) / this.m_ButtonsFadeInDuration) * MainMenuScreen.s_ButtonsAlpha;
		float num2 = Mathf.Clamp01((Time.time - this.m_ButtonActivationTime) / this.m_ButtonsFadeInDuration) * MainMenuScreen.s_InactiveButtonsAlpha;
		color.a = ((!this.m_ButtonsEnabled) ? 0f : num);
		color2.a = ((!this.m_ButtonsEnabled) ? 0f : num2);
		this.m_StartStory.GetComponentInChildren<Text>().color = color;
		this.m_StartChallenge.GetComponentInChildren<Text>().color = color;
		this.m_StartSurvival.GetComponentInChildren<Text>().color = color;
		this.m_LoadGame.GetComponentInChildren<Text>().color = color;
		this.m_Options.GetComponentInChildren<Text>().color = color;
		this.m_Credits.GetComponentInChildren<Text>().color = color;
		this.m_Quit.GetComponentInChildren<Text>().color = color;
		this.m_VLine.color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_StartStory.GetComponent<RectTransform>();
		if (!this.m_EarlyAccess || MainMenuScreen.s_DebugUnlock)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				this.m_ActiveButton = this.m_StartStory;
			}
		}
		else
		{
			this.m_StartStory.GetComponentInChildren<Text>().color = color2;
		}
		component = this.m_StartChallenge.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_StartChallenge;
		}
		component = this.m_StartSurvival.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_StartSurvival;
		}
		if (LoadSaveGameMenuCommon.IsAnySave())
		{
			component = this.m_LoadGame.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				this.m_ActiveButton = this.m_LoadGame;
			}
		}
		else
		{
			this.m_LoadGame.GetComponentInChildren<Text>().color = color2;
		}
		if (!GreenHellGame.TWITCH_DEMO)
		{
			component = this.m_Options.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				this.m_ActiveButton = this.m_Options;
			}
		}
		component = this.m_Credits.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Credits;
		}
		component = this.m_Quit.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Quit;
		}
		if (!this.m_ButtonsActive)
		{
			return;
		}
		component = this.m_StartStory.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		Vector3 position = component.position;
		float num3 = (!(this.m_ActiveButton == this.m_StartStory)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX;
		float num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_StartStory)
		{
			color = this.m_StartStory.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_StartStory.GetComponentInChildren<Text>().color = color;
		}
		if (this.m_EarlyAccess && !MainMenuScreen.s_DebugUnlock)
		{
			this.m_StartStory.GetComponentInChildren<Text>().color = color2;
		}
		component = this.m_StartChallenge.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_StartChallenge)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_StartChallenge)
		{
			color = this.m_StartChallenge.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_StartChallenge.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_StartSurvival.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_StartSurvival)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_StartSurvival)
		{
			color = this.m_StartSurvival.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_StartSurvival.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_LoadGame.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_LoadGame)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_LoadGame)
		{
			color = this.m_LoadGame.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_LoadGame.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Options.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_Options)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_Options)
		{
			color = this.m_Options.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Options.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Credits.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_Credits)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_Credits)
		{
			color = this.m_Credits.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Credits.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Quit.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num3 = ((!(this.m_ActiveButton == this.m_Quit)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num4 = Mathf.Ceil(num3 - position.x) * Time.deltaTime * 10f;
		position.x += num4;
		component.position = position;
		if (this.m_ActiveButton == this.m_Quit)
		{
			color = this.m_Quit.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Quit.GetComponentInChildren<Text>().color = color;
		}
	}

	private void UpdateMusic()
	{
		if (!this.m_MusicPlaying && Time.time - this.m_StartTime > 1.5f)
		{
			GreenHellGame.Instance.m_Settings.ApplySettings();
			Music.Get().PlayByName("main_menu", true);
			Music.Get().m_Source.volume = 0f;
			base.StartCoroutine(AudioFadeOut.FadeIn(Music.Get().m_Source, 2f, 1f));
			this.m_MusicPlaying = true;
		}
	}

	private void UpdateState()
	{
		MainMenuState state = this.m_State;
		if (state != MainMenuState.CompanyLogo)
		{
			if (state != MainMenuState.GameLogo)
			{
				if (state == MainMenuState.MainMenu)
				{
					if (Time.time - this.m_StateStartTime <= this.m_FadeOutSceneDuration)
					{
						Color color = this.m_BG.color;
						color.a = 1f - Mathf.Clamp01((Time.time - this.m_StateStartTime) / this.m_FadeOutSceneDuration);
						this.m_BG.color = color;
					}
					else
					{
						if (this.m_BG.gameObject.activeSelf)
						{
							this.m_BG.gameObject.SetActive(false);
						}
						if (!this.m_ButtonsEnabled)
						{
							this.m_ButtonsEnabled = true;
							this.m_ButtonActivationTime = Time.time;
						}
					}
				}
			}
			else if (Time.time - this.m_StateStartTime <= this.m_FadeInDuration)
			{
				Color color2 = this.m_GameLogo.color;
				color2.a = Mathf.Clamp01((Time.time - this.m_StateStartTime) / this.m_FadeInDuration);
				this.m_GameLogo.color = color2;
			}
			else if (Time.time - this.m_StateStartTime >= this.m_GameLogoDuration - this.m_FadeOutDuration)
			{
				Color color3 = this.m_GameLogo.color;
				color3.a = 1f - Mathf.Clamp01((Time.time - this.m_StateStartTime - (this.m_GameLogoDuration - this.m_FadeOutDuration)) / this.m_FadeOutDuration);
				this.m_GameLogo.color = color3;
				if (Time.time - this.m_StateStartTime > this.m_GameLogoDuration + this.m_BlackScreenDuration)
				{
					this.SetState(MainMenuState.MainMenu);
				}
			}
			else
			{
				Color color4 = this.m_GameLogo.color;
				color4.a = 1f;
				this.m_GameLogo.color = color4;
			}
		}
		else if (Time.time - this.m_StateStartTime <= this.m_FadeInDuration)
		{
			Color color5 = this.m_CompanyLogo.color;
			color5.a = Mathf.Clamp01((Time.time - this.m_StateStartTime) / this.m_FadeInDuration);
			this.m_CompanyLogo.color = color5;
		}
		else if (Time.time - this.m_StateStartTime >= this.m_CompanyLogoDuration - this.m_FadeOutDuration)
		{
			Color color6 = this.m_CompanyLogo.color;
			color6.a = 1f - Mathf.Clamp01((Time.time - this.m_StateStartTime - (this.m_CompanyLogoDuration - this.m_FadeOutDuration)) / this.m_FadeOutDuration);
			this.m_CompanyLogo.color = color6;
			if (Time.time - this.m_StateStartTime > this.m_CompanyLogoDuration + this.m_BlackScreenDuration)
			{
				this.SetState(MainMenuState.GameLogo);
			}
		}
		else
		{
			Color color7 = this.m_CompanyLogo.color;
			color7.a = 1f;
			this.m_CompanyLogo.color = color7;
		}
	}

	public void OnStartStory()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		if (this.m_EarlyAccess && !MainMenuScreen.s_DebugUnlock)
		{
			return;
		}
		GreenHellGame.Instance.m_GameMode = GameMode.Story;
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuDifficultyLevel), true);
	}

	public void OnStartChallenge()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		GreenHellGame.Instance.m_GameMode = GameMode.Survival;
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuChallenges), true);
	}

	public void OnStartSurvival()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		GreenHellGame.Instance.m_GameMode = GameMode.Survival;
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuDifficultyLevel), true);
	}

	public void OnLoadGame()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		if (!LoadSaveGameMenuCommon.IsAnySave())
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(LoadGameMenu), true);
	}

	public void OnOptions()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptions), true);
	}

	public void OnCredits()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuCredits), true);
	}

	public void OnQuit()
	{
		if (!this.m_ButtonsActive)
		{
			return;
		}
		Application.Quit();
	}

	private void UpdateInputs()
	{
		if (GreenHellGame.DEBUG)
		{
			if (Input.GetKey(KeyCode.Space) && this.m_State != MainMenuState.MainMenu)
			{
				this.SetState(MainMenuState.MainMenu);
			}
			else if (Input.GetKey(KeyCode.U))
			{
				MainMenuScreen.s_DebugUnlock = true;
			}
			else if (Input.GetKey(KeyCode.L))
			{
				LoadingScreen.Get().Show(LoadingScreenState.StartGame);
			}
			else if (Input.GetKey(KeyCode.K))
			{
				LoadingScreen.Get().Hide();
			}
		}
	}

	private void StoreMenuResolution()
	{
		GreenHellGame.Instance.m_MenuResolutionX = Screen.width;
		GreenHellGame.Instance.m_MenuResolutionY = Screen.height;
		GreenHellGame.Instance.m_MenuFullscreen = Screen.fullScreen;
	}

	public RawImage m_CompanyLogo;

	public RawImage m_GameLogo;

	public RawImage m_BG;

	private MainMenuState m_State;

	private bool m_ButtonsEnabled;

	private float m_FadeInDuration = 1f;

	private float m_FadeOutDuration = 1.7f;

	private float m_FadeOutSceneDuration = 3f;

	private float m_StateStartTime = -1f;

	private float m_CompanyLogoDuration = 5f;

	private float m_GameLogoDuration = 7f;

	private float m_BlackScreenDuration = 1f;

	private bool m_MusicPlaying;

	public Button m_StartStory;

	public Button m_StartChallenge;

	public Button m_StartSurvival;

	public Button m_LoadGame;

	public Button m_Options;

	public Button m_Credits;

	public Button m_Quit;

	public RawImage m_VLine;

	private float m_ButtonActivationTime = -1f;

	private float m_ButtonsFadeInDuration = 2f;

	private bool m_ButtonsActive;

	private bool m_WasButtonsActive;

	private Button m_ActiveButton;

	public static float s_ButtonTextStartX;

	public static float s_SelectedButtonX = 10f;

	private static float s_ButtonShiftX = 10f;

	[HideInInspector]
	public GameMode m_GameMode;

	private bool m_EarlyAccess = GreenHellGame.s_GameVersion <= GreenHellGame.s_GameVersionEarlyAcces;

	private float m_StartTime;
}
