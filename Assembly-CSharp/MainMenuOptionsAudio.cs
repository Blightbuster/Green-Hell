using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptionsAudio : MainMenuScreen, IYesNoDialogOwner
{
	private void Start()
	{
		this.m_AcceptButton.interactable = false;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.m_AcceptButton.interactable = false;
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

	private void Update()
	{
		if (!this.m_AcceptButton.interactable && (this.m_Slider.value != this.m_StartVol || this.m_DialogsSlider.value != this.m_StartDialogsVol || this.m_MusicSlider.value != this.m_StartMusicVol || this.m_EnviroSlider.value != this.m_StartEnviroVol || this.m_GeneralSlider.value != this.m_StartGeneralVol))
		{
			this.m_AcceptButton.interactable = true;
		}
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_MusicSlider.value;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
		GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Master).audioMixer.SetFloat("MasterVolume", General.LinearToDecibel(this.m_Slider.value));
	}

	public void OnBack()
	{
		this.m_Question = OptionsGameQuestion.Back;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back"), true);
	}

	public void OnAccept()
	{
		this.m_Question = OptionsGameQuestion.Accept;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), true);
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == OptionsGameQuestion.Back)
		{
			this.m_Slider.value = this.m_StartVol;
			this.m_DialogsSlider.value = this.m_StartDialogsVol;
			this.m_MusicSlider.value = this.m_StartMusicVol;
			this.m_EnviroSlider.value = this.m_StartEnviroVol;
			this.m_GeneralSlider.value = this.m_StartGeneralVol;
		}
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
		GreenHellGame.Instance.m_Settings.m_DialogsVolume = this.m_DialogsSlider.value;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_MusicSlider.value;
		GreenHellGame.Instance.m_Settings.m_EnviroVolume = this.m_EnviroSlider.value;
		GreenHellGame.Instance.m_Settings.m_GeneralVolume = this.m_GeneralSlider.value;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptions), true);
	}

	public void OnNoFromDialog()
	{
	}

	public void OnOkFromDialog()
	{
	}

	public Button m_AcceptButton;

	public Button m_BackButton;

	public Slider m_Slider;

	public Slider m_DialogsSlider;

	public Slider m_MusicSlider;

	public Slider m_EnviroSlider;

	public Slider m_GeneralSlider;

	private OptionsGameQuestion m_Question;

	private float m_StartVol;

	private float m_StartDialogsVol;

	private float m_StartMusicVol;

	private float m_StartEnviroVol;

	private float m_StartGeneralVol;
}
