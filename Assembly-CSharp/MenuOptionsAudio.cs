using System;
using System.Collections.Generic;
using CJTools;
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
		this.ApplySliders();
		this.m_StartVol = this.m_Slider.value;
		this.m_StartDialogsVol = this.m_DialogsSlider.value;
		this.m_StartMusicVol = this.m_MusicSlider.value;
		this.m_StartEnviroVol = this.m_EnviroSlider.value;
		this.m_StartGeneralVol = this.m_GeneralSlider.value;
	}

	private void ApplySliders()
	{
		this.m_Slider.value = GreenHellGame.Instance.m_Settings.m_Volume;
		this.m_DialogsSlider.value = GreenHellGame.Instance.m_Settings.m_DialogsVolume;
		this.m_MusicSlider.value = GreenHellGame.Instance.m_Settings.m_MusicVolume;
		this.m_EnviroSlider.value = GreenHellGame.Instance.m_Settings.m_EnviroVolume;
		this.m_GeneralSlider.value = GreenHellGame.Instance.m_Settings.m_GeneralVolume;
	}

	protected override void Update()
	{
		GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Master).audioMixer.SetFloat("MasterVolume", General.LinearToDecibel(this.m_Slider.value));
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
		this.UpdateButtons();
		this.UpdateButton(this.m_Slider.gameObject);
		this.UpdateButton(this.m_DialogsSlider.gameObject);
		this.UpdateButton(this.m_MusicSlider.gameObject);
		this.UpdateButton(this.m_EnviroSlider.gameObject);
		this.UpdateButton(this.m_GeneralSlider.gameObject);
	}

	private void UpdateButtons()
	{
		Color color = this.m_BackButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_BackButton.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		this.m_VolumeText.color = color;
		this.m_DialogsText.color = color;
		this.m_EnviroText.color = color;
		this.m_GeneralText.color = color;
		this.m_MusicText.color = color;
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
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public override void OnBack()
	{
		if (this.m_Slider.value != this.m_StartVol || this.m_DialogsSlider.value != this.m_StartDialogsVol || this.m_MusicSlider.value != this.m_StartMusicVol || this.m_EnviroSlider.value != this.m_StartEnviroVol || this.m_GeneralSlider.value != this.m_StartGeneralVol)
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
		GreenHellGame.Instance.m_Settings.m_DialogsVolume = this.m_DialogsSlider.value;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_MusicSlider.value;
		GreenHellGame.Instance.m_Settings.m_EnviroVolume = this.m_EnviroSlider.value;
		GreenHellGame.Instance.m_Settings.m_GeneralVolume = this.m_GeneralSlider.value;
		this.ApplySliders();
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnNoFromDialog()
	{
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_StartVol;
		GreenHellGame.Instance.m_Settings.m_DialogsVolume = this.m_StartDialogsVol;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_StartMusicVol;
		GreenHellGame.Instance.m_Settings.m_EnviroVolume = this.m_StartEnviroVol;
		GreenHellGame.Instance.m_Settings.m_GeneralVolume = this.m_StartGeneralVol;
		this.ApplySliders();
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public void OnOkFromDialog()
	{
	}

	private void UpdateButton(GameObject button)
	{
		Transform transform = button.transform.parent.FindDeepChild("Title");
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

	public Button m_BackButton;

	public Text m_BackText;

	public Slider m_Slider;

	public Text m_VolumeText;

	public Slider m_DialogsSlider;

	public Text m_DialogsText;

	public Slider m_MusicSlider;

	public Text m_MusicText;

	public Slider m_EnviroSlider;

	public Text m_EnviroText;

	public Slider m_GeneralSlider;

	public Text m_GeneralText;

	private GameObject m_ActiveButton;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private float m_StartVol;

	private float m_StartDialogsVol;

	private float m_StartMusicVol;

	private float m_StartEnviroVol;

	private float m_StartGeneralVol;

	private List<Text> m_TempText = new List<Text>(5);
}
