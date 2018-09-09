using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsControls : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
		this.m_InvertMouseYButton.SetTitle(GreenHellGame.Instance.GetLocalization().Get("OptionsControls_InvertMouse"));
		this.m_InvertMouseYButton.AddOption("Yes", GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Yes"));
		this.m_InvertMouseYButton.AddOption("No", GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_No"));
		this.m_XSensitivitySlider = this.m_XSensitivityButton.GetComponentInChildren<Slider>();
		this.m_YSensitivitySlider = this.m_YSensitivityButton.GetComponentInChildren<Slider>();
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		this.m_OptionsChanged = true;
		if (option == "Yes")
		{
			this.m_InvertMouseY = true;
		}
		else if (option == "No")
		{
			this.m_InvertMouseY = false;
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_OptionsChanged = false;
		this.m_InvertMouseYButton.SetByOption((!GreenHellGame.Instance.m_Settings.m_InvertMouseY) ? "No" : "Yes");
		this.m_InvertMouseY = GreenHellGame.Instance.m_Settings.m_InvertMouseY;
		this.m_XSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_XSensitivity;
		this.m_YSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_YSensitivity;
		this.m_StartXSensitivity = GreenHellGame.Instance.m_Settings.m_XSensitivity;
		this.m_StartYSensitivity = GreenHellGame.Instance.m_Settings.m_YSensitivity;
	}

	public override void OnBack()
	{
		if (this.m_OptionsChanged)
		{
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), false);
		}
		else
		{
			this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
		}
	}

	public void OnYesFromDialog()
	{
		this.ApplyOptions();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	private void ApplyOptions()
	{
		GreenHellGame.Instance.m_Settings.m_InvertMouseY = this.m_InvertMouseY;
		GreenHellGame.Instance.m_Settings.m_XSensitivity = this.m_XSensitivitySlider.value;
		GreenHellGame.Instance.m_Settings.m_YSensitivity = this.m_YSensitivitySlider.value;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
	}

	public void OnNoFromDialog()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnOkFromDialog()
	{
	}

	protected override void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
		if (this.m_XSensitivitySlider.value != this.m_StartXSensitivity || this.m_YSensitivitySlider.value != this.m_StartYSensitivity)
		{
			this.m_OptionsChanged = true;
		}
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_BackButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_BackButton.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		this.m_InvertMouseYButton.GetComponentInChildren<Text>().color = color;
		this.m_XSensitivityButton.GetComponentInChildren<Text>().color = color;
		this.m_YSensitivityButton.GetComponentInChildren<Text>().color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton.gameObject;
		}
		component = this.m_BackText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_BackButton.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton.gameObject)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		this.UpdateButton(this.m_InvertMouseYButton);
		this.UpdateButton(this.m_XSensitivityButton);
		this.UpdateButton(this.m_YSensitivityButton);
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	private void UpdateButton(UISelectButton button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, Input.mousePosition))
		{
			this.m_ActiveButton = button.gameObject;
		}
		component = button.m_Title.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == button.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == button.gameObject)
		{
			Color color = button.GetColor();
			color.a = 1f;
			button.SetColor(color);
		}
	}

	private void UpdateButton(GameObject button)
	{
		Transform transform = button.transform.FindDeepChild("Title");
		RectTransform component = transform.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, Input.mousePosition))
		{
			this.m_ActiveButton = button.gameObject;
		}
		component = transform.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == button.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == button.gameObject)
		{
			transform.GetComponents<Text>(this.m_TempText);
			if (this.m_TempText.Count <= 0)
			{
				return;
			}
			Text text = this.m_TempText[0];
			Color color = text.color;
			color.a = 1f;
			text.color = color;
		}
	}

	public UISelectButton m_InvertMouseYButton;

	public GameObject m_XSensitivityButton;

	public GameObject m_YSensitivityButton;

	private Slider m_XSensitivitySlider;

	private Slider m_YSensitivitySlider;

	private float m_StartXSensitivity = 1f;

	private float m_StartYSensitivity = 1f;

	public Button m_BackButton;

	private bool m_OptionsChanged;

	private GameObject m_ActiveButton;

	private bool m_InvertMouseY;

	public Text m_BackText;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private List<Text> m_TempText = new List<Text>(5);
}
