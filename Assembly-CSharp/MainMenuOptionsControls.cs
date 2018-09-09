using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptionsControls : MainMenuScreen, IYesNoDialogOwner
{
	private void Start()
	{
		this.m_InvertMouseYButton.SetTitle(GreenHellGame.Instance.GetLocalization().Get("OptionsControls_InvertMouse"));
		this.m_InvertMouseYButton.AddOption("Yes", GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Yes"));
		this.m_InvertMouseYButton.AddOption("No", GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_No"));
		this.m_XSensitivitySlider = this.m_XSensitivityButton.GetComponentInChildren<Slider>();
		this.m_YSensitivitySlider = this.m_YSensitivityButton.GetComponentInChildren<Slider>();
		this.m_AcceptButton.interactable = false;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.m_InvertMouseYButton.SetByOption((!GreenHellGame.Instance.m_Settings.m_InvertMouseY) ? "No" : "Yes");
		this.m_InvertMouseY = GreenHellGame.Instance.m_Settings.m_InvertMouseY;
		this.m_XSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_XSensitivity;
		this.m_YSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_YSensitivity;
		this.m_StartXSensitivity = GreenHellGame.Instance.m_Settings.m_XSensitivity;
		this.m_StartYSensitivity = GreenHellGame.Instance.m_Settings.m_YSensitivity;
		this.m_AcceptButton.interactable = false;
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		if (option == "Yes")
		{
			this.m_InvertMouseY = true;
		}
		else if (option == "No")
		{
			this.m_InvertMouseY = false;
		}
		this.m_AcceptButton.interactable = true;
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == OptionsControlsQuestion.Back)
		{
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptions), true);
		}
		else if (this.m_Question == OptionsControlsQuestion.Accept)
		{
			this.ApplySettings();
			GreenHellGame.Instance.m_Settings.SaveSettings();
			GreenHellGame.Instance.m_Settings.ApplySettings();
		}
	}

	public void OnNoFromDialog()
	{
	}

	public void OnOkFromDialog()
	{
	}

	private void ApplySettings()
	{
		GreenHellGame.Instance.m_Settings.m_InvertMouseY = this.m_InvertMouseY;
		GreenHellGame.Instance.m_Settings.m_XSensitivity = this.m_XSensitivitySlider.value;
		GreenHellGame.Instance.m_Settings.m_YSensitivity = this.m_YSensitivitySlider.value;
	}

	public void OnBack()
	{
		this.m_Question = OptionsControlsQuestion.Back;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back"), true);
	}

	public void OnAccept()
	{
		this.m_Question = OptionsControlsQuestion.Accept;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), true);
	}

	private void Update()
	{
		if (this.m_XSensitivitySlider.value != this.m_StartXSensitivity || this.m_YSensitivitySlider.value != this.m_StartYSensitivity)
		{
			this.m_AcceptButton.interactable = true;
		}
	}

	public UISelectButton m_InvertMouseYButton;

	public GameObject m_XSensitivityButton;

	public GameObject m_YSensitivityButton;

	private Slider m_XSensitivitySlider;

	private Slider m_YSensitivitySlider;

	private float m_StartXSensitivity = 1f;

	private float m_StartYSensitivity = 1f;

	private bool m_InvertMouseY;

	public Button m_AcceptButton;

	public Button m_BackButton;

	private OptionsControlsQuestion m_Question;
}
