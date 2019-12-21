using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuInGame : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.AddMenuButton(this.m_ResumeButton, "MenuInGame_Resume");
		this.AddMenuButton(this.m_LoadButton, "MenuInGame_Load");
		this.AddMenuButton(this.m_OptionsButton, "MenuInGame_Options");
		this.AddMenuButton(this.m_QuitButton, "MenuInGame_Quit");
		this.AddMenuButton(this.m_SkipTutorialButton, "MenuInGame_SkipTutorial");
		this.AddMenuButton(this.m_OnlineButton, "MenuInGame_Online");
		MenuScreen.s_ButtonsAlpha = this.m_ResumeButton.GetComponentInChildren<Button>().colors.normalColor.a;
		MenuScreen.s_ButtonsHighlightedAlpha = this.m_ResumeButton.GetComponentInChildren<Button>().colors.highlightedColor.a;
		MenuScreen.s_InactiveButtonsAlpha = MenuScreen.s_ButtonsAlpha * 0.5f;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdatePauseText();
	}

	protected void Start()
	{
		if (this.m_MenuName)
		{
			this.m_MenuName.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Name", true);
		}
	}

	public void OnConstruction()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuConstruction));
	}

	public void OnStatistics()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuStatistics));
	}

	public void OnLoad()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(LoadGameMenu));
	}

	public void OnObjectives()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuObjectives));
	}

	public void OnResume()
	{
		MenuInGameManager.Get().HideMenu();
	}

	public void OnOptions()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnSkipTutorial()
	{
		this.m_CurrentQuestion = MenuInGame.Question.SkipTutorial;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_SkipTutorial", true), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_SkipTutorial_Question", true), false);
	}

	public void OnQuitGame()
	{
		this.m_CurrentQuestion = MenuInGame.Question.Quit;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit", true), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit_Question", true), false);
	}

	public void OnOnline()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuFindGame));
	}

	public void OnYesFromDialog()
	{
		if (this.m_CurrentQuestion == MenuInGame.Question.Quit)
		{
			this.QuitToMainMenu();
			return;
		}
		if (this.m_CurrentQuestion == MenuInGame.Question.SkipTutorial)
		{
			this.SkipTutorial();
		}
	}

	public void OnNoFromDialog()
	{
	}

	public void OnOkFromDialog()
	{
	}

	public void OnCloseDialog()
	{
	}

	private void SkipTutorial()
	{
		this.m_MenuInGameManager.HideMenu();
		ScenarioManager.Get().m_SkipTutorial = true;
	}

	public void QuitToMainMenu()
	{
		Music.Get().StopAll();
		MusicJingle.Get().StoppAll();
		GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
		this.m_MenuInGameManager.HideMenu();
		LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
		GreenHellGame.Instance.ReturnToMainMenu();
	}

	public override void OnShow()
	{
		base.OnShow();
		if (this.m_ControlsImage)
		{
			this.m_ControlsImage.SetActive(GreenHellGame.GAMESCOM_DEMO);
		}
		Button onlineButton = this.m_OnlineButton;
		if (onlineButton != null)
		{
			onlineButton.gameObject.SetActive(false);
		}
		GameObject lobbyInfo = this.m_LobbyInfo;
		if (lobbyInfo != null)
		{
			lobbyInfo.gameObject.SetActive(false);
		}
		this.m_SkipTutorialButton.gameObject.SetActive(MainLevel.Instance.m_Tutorial);
		this.SetupStatistics();
		this.UpdatePauseText();
	}

	private void SetupStatistics()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		this.m_DaysVal.text = localization.GetMixed((ivalue == 1) ? "Statistic_DaysSurvivedOne" : "Statistic_DaysSurvived", new string[]
		{
			ivalue.ToString()
		});
		this.m_DistVal.text = localization.GetMixed("Statistic_TravelledDist", new string[]
		{
			StatsManager.Get().GetStatistic(Enums.Event.TraveledDist).FValue.ToString("F1")
		});
		int ivalue2 = StatsManager.Get().GetStatistic(Enums.Event.Vomit).IValue;
		this.m_VomitVal.text = localization.GetMixed((ivalue2 == 1) ? "Statistic_VomitedTimesOnce" : "Statistic_VomitedTimes", new string[]
		{
			ivalue2.ToString()
		});
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		return (!(b == this.m_LoadButton) || !ChallengesManager.Get() || !ChallengesManager.Get().m_ChallengeMode) && (!(b == this.m_LoadButton) || !GreenHellGame.Instance.IsGamescom()) && (!(b == this.m_OptionsButton) || !GreenHellGame.TWITCH_DEMO) && !GreenHellGame.GetYesNoDialog().isActiveAndEnabled && (!(b == this.m_SkipTutorialButton) || (MainLevel.Instance.m_Tutorial && !GreenHellGame.Instance.IsGamescom())) && (!(b == this.m_LoadButton) || ReplTools.IsPlayingAlone()) && !(b == this.m_OnlineButton) && !(b.GetComponent<LobbyMemberListElement>() != null) && base.IsMenuButtonEnabled(b);
	}

	private void UpdatePauseText()
	{
		if (Time.timeScale == 0f)
		{
			this.m_PauseText.gameObject.SetActive(true);
			if (this.m_LastPausedCount != -1)
			{
				this.m_PauseText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Paused", true);
				this.m_LastPausedCount = -1;
				return;
			}
		}
		else
		{
			int num = 0;
			using (List<ReplicatedLogicalPlayer>.Enumerator enumerator = ReplicatedLogicalPlayer.s_AllLogicalPlayers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_PauseGame)
					{
						num++;
					}
				}
			}
			this.m_PauseText.gameObject.SetActive(num > 0);
			if (num > 0 && this.m_LastPausedCount != num)
			{
				this.m_LastPausedCount = num;
				this.m_PauseText.text = GreenHellGame.Instance.GetLocalization().GetMixed("MenuInGame_PauseRequested", new string[]
				{
					string.Format("{0}/{1}", num, ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count)
				});
			}
		}
	}

	public Button m_ResumeButton;

	public Button m_OptionsButton;

	public Button m_QuitButton;

	public Button m_LoadButton;

	public Button m_SkipTutorialButton;

	public Button m_OnlineButton;

	public Text m_DaysVal;

	public Text m_DistVal;

	public Text m_VomitVal;

	private float m_ButtonsYDiff;

	public Text m_MenuName;

	public GameObject m_ControlsImage;

	public Text m_PauseText;

	public GameObject m_LobbyInfo;

	private int m_LastPausedCount;

	private MenuInGame.Question m_CurrentQuestion;

	private enum Question
	{
		Quit,
		SkipTutorial
	}
}
