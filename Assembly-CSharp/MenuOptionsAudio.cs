using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsAudio : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_Slider.value = GreenHellGame.Instance.m_Settings.m_Volume;
		this.m_StartVol = this.m_Slider.value;
	}

	protected override void Update()
	{
		GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Master).audioMixer.SetFloat("MasterVolume", this.m_Slider.value);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
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
		this.m_VolumeText.color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton;
		}
		component = this.m_BackText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_BackButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public override void OnBack()
	{
		if (this.m_Slider.value != this.m_StartVol)
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
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnNoFromDialog()
	{
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_StartVol;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnOkFromDialog()
	{
	}

	public Button m_BackButton;

	public Text m_BackText;

	public Slider m_Slider;

	public Text m_VolumeText;

	private Button m_ActiveButton;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private float m_StartVol;
}
