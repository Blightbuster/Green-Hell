using System;
using CJTools;
using Enums;
using UnityEngine.UI;

public class MainMenuOptionsAudio : MenuScreen, IYesNoDialogOwner
{
	public override void OnShow()
	{
		base.OnShow();
		this.ApplySliders();
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
		base.Update();
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_MusicSlider.value;
		GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Master).audioMixer.SetFloat("MasterVolume", General.LinearToDecibel(this.m_Slider.value));
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		if (b == this.m_AcceptButton)
		{
			return base.IsAnyOptionModified();
		}
		return base.IsMenuButtonEnabled(b);
	}

	public override void OnBack()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsAudio.OptionsAudioQuestion.Back;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), !this.m_IsIngame);
			return;
		}
		base.OnBack();
	}

	public override void OnAccept()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsAudio.OptionsAudioQuestion.Accept;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept", true), !this.m_IsIngame);
			return;
		}
		this.ShowPreviousScreen();
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == MainMenuOptionsAudio.OptionsAudioQuestion.Back)
		{
			base.RevertOptionValues();
		}
		GreenHellGame.Instance.m_Settings.m_Volume = this.m_Slider.value;
		GreenHellGame.Instance.m_Settings.m_DialogsVolume = this.m_DialogsSlider.value;
		GreenHellGame.Instance.m_Settings.m_MusicVolume = this.m_MusicSlider.value;
		GreenHellGame.Instance.m_Settings.m_EnviroVolume = this.m_EnviroSlider.value;
		GreenHellGame.Instance.m_Settings.m_GeneralVolume = this.m_GeneralSlider.value;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings(false);
		this.ShowPreviousScreen();
	}

	public void OnNoFromDialog()
	{
	}

	public void OnOkFromDialog()
	{
	}

	public void OnCloseDialog()
	{
	}

	public Button m_AcceptButton;

	public Button m_BackButton;

	public Slider m_Slider;

	public Slider m_DialogsSlider;

	public Slider m_MusicSlider;

	public Slider m_EnviroSlider;

	public Slider m_GeneralSlider;

	private MainMenuOptionsAudio.OptionsAudioQuestion m_Question;

	public enum OptionsAudioQuestion
	{
		None,
		Back,
		Accept
	}
}
