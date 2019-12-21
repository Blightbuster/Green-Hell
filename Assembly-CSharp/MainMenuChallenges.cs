using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuChallenges : MenuScreen
{
	public override void OnShow()
	{
		base.OnShow();
		DebugUtils.Assert(this.m_Buttons.Count >= ChallengesManager.Get().m_Challenges.Count, "Not enough buttons!", true, DebugUtils.AssertType.Info);
		for (int i = 0; i < ChallengesManager.Get().m_Challenges.Count; i++)
		{
			Text componentInChildren = this.m_Buttons[i].GetComponentInChildren<Text>();
			componentInChildren.text = GreenHellGame.Instance.GetLocalization().Get(ChallengesManager.Get().m_Challenges[i].m_NameID, true);
			this.m_Color = componentInChildren.color;
		}
		for (int j = ChallengesManager.Get().m_Challenges.Count; j < this.m_Buttons.Count; j++)
		{
			this.m_Buttons[j].gameObject.SetActive(false);
		}
		this.m_Loading = false;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateButtons();
		this.UpdateDescription();
	}

	private void UpdateButtons()
	{
		this.m_SelectedChallenge = null;
		for (int i = 0; i < this.m_Buttons.Count; i++)
		{
			Button button = this.m_Buttons[i];
			if (!button.gameObject.activeSelf)
			{
				break;
			}
			UnityEngine.Object gameObject = button.gameObject;
			MenuBase.MenuOptionData activeMenuOption = this.m_ActiveMenuOption;
			if (gameObject == ((activeMenuOption != null) ? activeMenuOption.m_Object : null))
			{
				this.m_SelectedChallenge = ChallengesManager.Get().m_Challenges[i];
				this.SetActiveButton(button);
			}
		}
	}

	private void UpdateDescription()
	{
		if (this.m_SelectedChallenge != null && this.m_DescCanvasGroup.alpha < 1f && Time.time - this.m_SetActiveButtonTime > 0.3f)
		{
			this.m_DescCanvasGroup.alpha += Time.deltaTime * 0.5f;
			return;
		}
		if (this.m_SelectedChallenge == null)
		{
			this.m_DescCanvasGroup.alpha = 0f;
		}
	}

	private void SetActiveButton(Button button)
	{
		if (this.m_ActiveButton == button)
		{
			return;
		}
		this.m_ActiveButton = button;
		this.m_SetActiveButtonTime = Time.time;
		this.SetupChallengeDescription();
	}

	private void SetupChallengeDescription()
	{
		this.m_DescCanvasGroup.alpha = 0f;
		if (this.m_SelectedChallenge != null)
		{
			this.m_Icon.sprite = this.m_SelectedChallenge.m_Icon;
			this.m_ChallengeName.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedChallenge.m_NameID, true);
			this.m_Description.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedChallenge.m_DescriptionID, true);
			this.m_StartTime.text = ChallengesManager.Get().DateTimeToLocalizedString(this.m_SelectedChallenge.m_StartDate, true);
			this.m_EndTime.text = ChallengesManager.Get().DateTimeToLocalizedString(this.m_SelectedChallenge.m_EndDate, true);
			if (this.m_SelectedChallenge.m_BestScore > 0f)
			{
				this.m_BestTime.text = GreenHellGame.Instance.GetLocalization().Get("ChallengeResult_BestTime", true);
				Text bestTime = this.m_BestTime;
				bestTime.text += " - ";
				DateTime date = this.m_SelectedChallenge.m_StartDate.AddHours((double)this.m_SelectedChallenge.m_BestScore);
				Text bestTime2 = this.m_BestTime;
				bestTime2.text += ChallengesManager.Get().DateTimeToLocalizedString(date, false);
				float fillAmount = this.m_SelectedChallenge.m_BestScore / this.m_SelectedChallenge.m_Duration;
				this.m_BestResultBelt.fillAmount = fillAmount;
				return;
			}
			this.m_BestTime.text = string.Empty;
			this.m_BestResultBelt.fillAmount = 0f;
		}
	}

	public void OnButton(int index)
	{
		ChallengesManager.Get().m_ChallengeToActivate = ChallengesManager.Get().m_Challenges[index].m_Name;
		base.Invoke("OnPreStartGame", 0.3f);
		this.m_Loading = true;
	}

	private void OnPreStartGame()
	{
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.OnStartGame), 2f, null);
		CursorManager.Get().ShowCursor(false, false);
	}

	private void OnStartGame()
	{
		DifficultySettings.SetActivePresetType(DifficultySettings.PresetType.Hard);
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = false;
		Music.Get().Stop(1f, 0);
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnStartGameDelayed", 1f);
		MainMenuManager.Get().HideAllScreens();
	}

	private void OnStartGameDelayed()
	{
		Music.Get().PlayByName("loading_screen", false, 1f, 0);
		Music.Get().m_Source[0].loop = true;
		GreenHellGame.Instance.StartGame();
	}

	public override void OnBack()
	{
		if (this.m_Loading)
		{
			return;
		}
		this.ShowPreviousScreen();
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Button_A)
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Button && !GreenHellGame.IsYesNoDialogActive())
			{
				this.m_ActiveMenuOption.m_Button.onClick.Invoke();
				return;
			}
		}
		else
		{
			base.OnInputAction(action_data);
		}
	}

	public List<Button> m_Buttons = new List<Button>();

	public Button m_BackButton;

	private Button m_ActiveButton;

	private float m_SetActiveButtonTime;

	private Challenge m_SelectedChallenge;

	public CanvasGroup m_DescCanvasGroup;

	public Image m_Icon;

	public Text m_ChallengeName;

	public Text m_Description;

	public Image m_BestResultBelt;

	public Text m_StartTime;

	public Text m_EndTime;

	public Text m_BestTime;

	private Color m_Color = Color.white;

	private bool m_Loading;
}
