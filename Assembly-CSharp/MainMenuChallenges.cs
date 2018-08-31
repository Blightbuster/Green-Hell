using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuChallenges : MainMenuScreen
{
	public override void OnShow()
	{
		base.OnShow();
		DebugUtils.Assert(this.m_Buttons.Count >= ChallengesManager.Get().m_Challenges.Count, "Not enough buttons!", true, DebugUtils.AssertType.Info);
		for (int i = 0; i < ChallengesManager.Get().m_Challenges.Count; i++)
		{
			Text componentInChildren = this.m_Buttons[i].GetComponentInChildren<Text>();
			componentInChildren.text = GreenHellGame.Instance.GetLocalization().Get(ChallengesManager.Get().m_Challenges[i].m_NameID);
			this.m_Color = componentInChildren.color;
		}
		for (int j = ChallengesManager.Get().m_Challenges.Count; j < this.m_Buttons.Count; j++)
		{
			this.m_Buttons[j].gameObject.SetActive(false);
		}
		this.m_Loading = false;
	}

	private void Update()
	{
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_Color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		Vector2 screenPoint = Input.mousePosition;
		Button activeButton = null;
		this.m_SelectedChallenge = null;
		RectTransform component;
		for (int i = 0; i < this.m_Buttons.Count; i++)
		{
			Button button = this.m_Buttons[i];
			if (!button.gameObject.activeSelf)
			{
				break;
			}
			button.GetComponentInChildren<Text>().color = this.m_Color;
			component = button.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				activeButton = button;
				this.m_SelectedChallenge = ChallengesManager.Get().m_Challenges[i];
			}
		}
		this.m_BackButton.GetComponentInChildren<Text>().color = this.m_Color;
		component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			activeButton = this.m_BackButton;
		}
		this.SetActiveButton(activeButton);
		foreach (Button button2 in this.m_Buttons)
		{
			if (!button2.gameObject.activeSelf)
			{
				break;
			}
			component = button2.GetComponentInChildren<Text>().GetComponent<RectTransform>();
			Vector3 position = component.position;
			float num = (!(this.m_ActiveButton == button2)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX;
			float num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
			num = ((!(this.m_ActiveButton == button2)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
			num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
			position.x += num2;
			component.position = position;
			if (this.m_ActiveButton == button2)
			{
				color = button2.GetComponentInChildren<Text>().color;
				color.a = 1f;
				button2.GetComponentInChildren<Text>().color = color;
			}
		}
		if (this.m_ActiveButton == this.m_BackButton)
		{
			color = this.m_BackButton.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_BackButton.GetComponentInChildren<Text>().color = color;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
		this.UpdateDescription();
	}

	private void UpdateDescription()
	{
		if (this.m_SelectedChallenge != null && this.m_DescCanvasGroup.alpha < 1f && Time.time - this.m_SetActiveButtonTime > 0.3f)
		{
			this.m_DescCanvasGroup.alpha += Time.deltaTime * 0.5f;
		}
		else if (this.m_SelectedChallenge == null)
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
			this.m_ChallengeName.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedChallenge.m_NameID);
			this.m_Description.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedChallenge.m_DescriptionID);
			this.m_StartTime.text = ChallengesManager.Get().DateTimeToLocalizedString(this.m_SelectedChallenge.m_StartDate, true);
			this.m_EndTime.text = ChallengesManager.Get().DateTimeToLocalizedString(this.m_SelectedChallenge.m_EndDate, true);
			if (this.m_SelectedChallenge.m_BestScore > 0f)
			{
				this.m_BestTime.text = GreenHellGame.Instance.GetLocalization().Get("ChallengeResult_BestTime");
				Text bestTime = this.m_BestTime;
				bestTime.text += " - ";
				DateTime date = this.m_SelectedChallenge.m_StartDate.AddHours((double)this.m_SelectedChallenge.m_BestScore);
				Text bestTime2 = this.m_BestTime;
				bestTime2.text += ChallengesManager.Get().DateTimeToLocalizedString(date, false);
				float fillAmount = this.m_SelectedChallenge.m_BestScore / this.m_SelectedChallenge.m_Duration;
				this.m_BestResultBelt.fillAmount = fillAmount;
			}
			else
			{
				this.m_BestTime.text = string.Empty;
				this.m_BestResultBelt.fillAmount = 0f;
			}
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
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnStartGame), 2f, null);
		CursorManager.Get().ShowCursor(false);
	}

	private void OnStartGame()
	{
		GreenHellGame.Instance.m_GameDifficulty = GameDifficulty.Hard;
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = false;
		Music.Get().Stop(1f);
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnStartGameDelayed", 1f);
		MainMenuManager.Get().HideAllScreens();
	}

	private void OnStartGameDelayed()
	{
		Music.Get().PlayByName("loading_screen", false);
		Music.Get().m_Source.loop = true;
		GreenHellGame.Instance.StartGame();
	}

	public void OnBack()
	{
		if (this.m_Loading)
		{
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
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
