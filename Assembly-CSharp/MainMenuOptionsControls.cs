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
		this.m_KeyBindingsText = this.m_KeyBindingsButton.GetComponentInChildren<Text>();
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

	public void OnKeyBindings()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsControlsKeysBinding), true);
	}

	private void Update()
	{
		if (this.m_XSensitivitySlider.value != this.m_StartXSensitivity || this.m_YSensitivitySlider.value != this.m_StartYSensitivity)
		{
			this.m_AcceptButton.interactable = true;
		}
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_BackButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_BackButton.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_KeyBindingsButton.GetComponentInChildren<Text>().color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_KeyBindingsButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_KeyBindingsButton.gameObject;
		}
		component = this.m_KeyBindingsText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_KeyBindingsButton.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_KeyBindingsButton.gameObject)
		{
			color = this.m_KeyBindingsText.color;
			color.a = 1f;
			this.m_KeyBindingsText.color = color;
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

	public Button m_KeyBindingsButton;

	public Text m_KeyBindingsText;

	public Button m_AcceptButton;

	public Button m_BackButton;

	private OptionsControlsQuestion m_Question;

	private GameObject m_ActiveButton;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;
}
