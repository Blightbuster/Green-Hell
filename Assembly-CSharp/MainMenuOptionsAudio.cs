using System;
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
		this.m_Slider.value = GreenHellGame.Instance.m_Settings.m_Volume;
		this.m_StartVol = this.m_Slider.value;
	}

	private void Update()
	{
		if (!this.m_AcceptButton.interactable && this.m_Slider.value != this.m_StartVol)
		{
			this.m_AcceptButton.interactable = true;
		}
		GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Master).audioMixer.SetFloat("MasterVolume", this.m_Slider.value);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
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
		}
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
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

	private OptionsGameQuestion m_Question;

	private float m_StartVol;
}
