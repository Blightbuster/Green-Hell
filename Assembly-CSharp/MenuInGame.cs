using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuInGame : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_ResumeText = this.m_ResumeButton.GetComponentInChildren<Text>();
		this.m_ResumeText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Resume");
		this.m_LoadText = this.m_LoadButton.GetComponentInChildren<Text>();
		this.m_LoadText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Load");
		this.m_OptionsText = this.m_OptionsButton.GetComponentInChildren<Text>();
		this.m_OptionsText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Options");
		this.m_QuitText = this.m_QuitButton.GetComponentInChildren<Text>();
		this.m_QuitText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit");
		this.m_SkipTutorialText = this.m_SkipTutorialButton.GetComponentInChildren<Text>();
		this.m_SkipTutorialText.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_SkipTutorial");
		MainMenuScreen.s_ButtonsAlpha = this.m_ResumeButton.GetComponentInChildren<Text>().color.a;
		MainMenuScreen.s_InactiveButtonsAlpha = MainMenuScreen.s_ButtonsAlpha * 0.5f;
		this.m_ButtonsYDiff = ((RectTransform)this.m_LoadButton.transform).anchoredPosition.y - ((RectTransform)this.m_ResumeButton.transform).anchoredPosition.y;
	}

	protected override void Start()
	{
		base.Start();
		if (this.m_MenuName)
		{
			this.m_MenuName.text = GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Name");
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateButtons();
		this.UpdateInputs();
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
		this.m_MenuInGameManager.ShowScreen(typeof(MenuLoad));
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
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_SkipTutorial"), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_SkipTutorial_Question"), false);
	}

	public void OnQuitGame()
	{
		this.m_CurrentQuestion = MenuInGame.Question.Quit;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit"), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit_Question"), false);
	}

	public void OnYesFromDialog()
	{
		if (this.m_CurrentQuestion == MenuInGame.Question.Quit)
		{
			this.QuitToMainMenu();
		}
		else if (this.m_CurrentQuestion == MenuInGame.Question.SkipTutorial)
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

	private void SkipTutorial()
	{
		this.m_MenuInGameManager.HideMenu();
		ScenarioManager.Get().m_SkipTutorial = true;
	}

	public void QuitToMainMenu()
	{
		GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
		this.m_MenuInGameManager.HideMenu();
		LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
		GreenHellGame.Instance.ReturnToMainMenu();
	}

	private void UpdateButtons()
	{
		if (GreenHellGame.GetYesNoDialog().isActiveAndEnabled)
		{
			return;
		}
		Color color = this.m_ResumeButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_ResumeButton.GetComponentInChildren<Text>().color;
		float s_ButtonsAlpha = MainMenuScreen.s_ButtonsAlpha;
		float s_InactiveButtonsAlpha = MainMenuScreen.s_InactiveButtonsAlpha;
		color.a = s_ButtonsAlpha;
		color2.a = s_InactiveButtonsAlpha;
		this.m_ResumeText.color = color;
		this.m_OptionsText.color = ((!GreenHellGame.TWITCH_DEMO) ? color : color2);
		this.m_QuitText.color = color;
		this.m_LoadText.color = color;
		this.m_SkipTutorialText.color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_ResumeButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_ResumeButton;
		}
		component = this.m_LoadButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_LoadButton;
		}
		if (!GreenHellGame.TWITCH_DEMO)
		{
			component = this.m_OptionsButton.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				this.m_ActiveButton = this.m_OptionsButton;
			}
		}
		component = this.m_QuitButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_QuitButton;
		}
		if (this.m_SkipTutorialButton.gameObject.activeSelf)
		{
			component = this.m_SkipTutorialButton.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
			{
				this.m_ActiveButton = this.m_SkipTutorialButton;
			}
		}
		component = this.m_ResumeText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_ResumeButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_ResumeButton)
		{
			color = this.m_ResumeText.color;
			color.a = 1f;
			this.m_ResumeText.color = color;
		}
		component = this.m_LoadText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_LoadButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_LoadButton)
		{
			color = this.m_LoadText.color;
			color.a = 1f;
			this.m_LoadText.color = color;
		}
		component = this.m_OptionsText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_OptionsButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_OptionsButton)
		{
			color = this.m_OptionsText.color;
			color.a = 1f;
			this.m_OptionsText.color = color;
		}
		component = this.m_QuitText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_QuitButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_QuitButton)
		{
			color = this.m_QuitText.color;
			color.a = 1f;
			this.m_QuitText.color = color;
		}
		if (this.m_SkipTutorialButton.gameObject.activeSelf)
		{
			component = this.m_SkipTutorialText.GetComponent<RectTransform>();
			localPosition = component.localPosition;
			num = ((!(this.m_ActiveButton == this.m_SkipTutorialButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
			num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
			localPosition.x += num2;
			component.localPosition = localPosition;
			if (this.m_ActiveButton == this.m_SkipTutorialButton)
			{
				color = this.m_SkipTutorialText.color;
				color.a = 1f;
				this.m_SkipTutorialText.color = color;
			}
		}
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
	}

	private void UpdateInputs()
	{
		if (GreenHellGame.GetYesNoDialog().isActiveAndEnabled)
		{
			return;
		}
		if (Input.GetMouseButton(0) && this.m_ActiveButton != null)
		{
			this.OnButtonPressed(this.m_ActiveButton);
		}
	}

	private void OnButtonPressed(Button button)
	{
		if (button == this.m_ResumeButton)
		{
			this.OnResume();
		}
		else if (button == this.m_QuitButton)
		{
			this.OnQuitGame();
		}
		else if (button == this.m_OptionsButton)
		{
			this.OnOptions();
		}
		else if (button == this.m_SkipTutorialButton)
		{
			this.OnSkipTutorial();
		}
		else if (button == this.m_LoadButton)
		{
			this.OnLoad();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_SkipTutorialButton.gameObject.SetActive(MainLevel.Instance.m_Tutorial);
		Vector2 vector = ((RectTransform)this.m_SkipTutorialButton.transform).anchoredPosition + ((!MainLevel.Instance.m_Tutorial) ? Vector2.zero : (Vector2.up * this.m_ButtonsYDiff));
		((RectTransform)this.m_ResumeButton.transform).anchoredPosition = vector;
		((RectTransform)this.m_LoadButton.transform).anchoredPosition = vector + Vector2.up * this.m_ButtonsYDiff;
		((RectTransform)this.m_OptionsButton.transform).anchoredPosition = vector + Vector2.up * this.m_ButtonsYDiff * 2f;
		((RectTransform)this.m_QuitButton.transform).anchoredPosition = vector + Vector2.up * this.m_ButtonsYDiff * 3f;
		this.SetupStatistics();
	}

	private void SetupStatistics()
	{
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		this.m_DaysVal.text = ivalue.ToString() + ((ivalue != 1) ? " days" : " day");
		this.m_DistVal.text = StatsManager.Get().GetStatistic(Enums.Event.TraveledDist).FValue.ToString("F1") + " km";
		int ivalue2 = StatsManager.Get().GetStatistic(Enums.Event.Vomit).IValue;
		this.m_VomitVal.text = ivalue2.ToString() + ((ivalue2 != 1) ? " times" : " time");
	}

	public Button m_ResumeButton;

	public Button m_OptionsButton;

	public Button m_QuitButton;

	public Button m_LoadButton;

	public Button m_SkipTutorialButton;

	private Text m_ResumeText;

	private Text m_OptionsText;

	private Text m_QuitText;

	private Text m_LoadText;

	private Text m_SkipTutorialText;

	public Text m_DaysVal;

	public Text m_DistVal;

	public Text m_VomitVal;

	private float m_ButtonsYDiff;

	private Button m_ActiveButton;

	public Text m_MenuName;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private MenuInGame.Question m_CurrentQuestion;

	private enum Question
	{
		Quit,
		SkipTutorial
	}
}
