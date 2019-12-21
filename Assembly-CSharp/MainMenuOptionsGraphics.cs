using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptionsGraphics : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
		this.m_Quality.AddOption("Very Low", "MenuGraphics_VeryLow");
		this.m_Quality.AddOption("Low", "MenuGraphics_Low");
		this.m_Quality.AddOption("Medium", "MenuGraphics_Medium");
		this.m_Quality.AddOption("High", "MenuGraphics_High");
		this.m_Quality.AddOption("Very High", "MenuGraphics_VeryHigh");
		this.m_TextureQuality.AddOption("Low", "MenuGraphics_Low");
		this.m_TextureQuality.AddOption("Medium", "MenuGraphics_Medium");
		this.m_TextureQuality.AddOption("High", "MenuGraphics_High");
		this.m_Fullscreen.ClearOptions();
		this.m_Fullscreen.m_OnOff = false;
		this.m_Fullscreen.AddOption("Windowed", "MenuOptions_Graphics_Windowed");
		this.m_Fullscreen.AddOption("FullScreenWindow", "MenuOptions_Graphics_FullScreenWindow");
		this.m_Fullscreen.AddOption("ExclusiveFullScreen", "MenuOptions_Graphics_ExclusiveFullScreen");
	}

	private void FillAvailableResolutions()
	{
		this.m_Resolution.ClearOptions();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Resolution res = Screen.resolutions[i];
			if (res.IsValid())
			{
				string text;
				string text2;
				res.ToString2(out text, out text2);
				if (!this.m_Resolution.HasOption(text))
				{
					this.m_Resolution.AddOption(text, text);
				}
			}
		}
	}

	private void FillAvailableRefreshRate(Resolution current_res)
	{
		this.m_RefreshRate.ClearOptions();
		if (this.m_Fullscreen.GetSelectedOptionEnumValue<FullScreenMode>() != FullScreenMode.ExclusiveFullScreen)
		{
			string text;
			string option;
			Screen.currentResolution.ToString2(out text, out option);
			this.m_RefreshRate.AddOption(option, "-");
			this.m_RefreshRate.interactable = false;
			return;
		}
		string byOption = "";
		int num = 0;
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Resolution res = Screen.resolutions[i];
			if (res.Equals(current_res, true))
			{
				string text2;
				string text3;
				res.ToString2(out text2, out text3);
				if (!this.m_RefreshRate.HasOption(text3))
				{
					this.m_RefreshRate.AddOption(text3, text3);
				}
				if (num == 0 || Mathf.Abs(current_res.refreshRate - res.refreshRate) < Mathf.Abs(current_res.refreshRate - num))
				{
					num = res.refreshRate;
					byOption = text3;
				}
			}
		}
		this.m_RefreshRate.interactable = true;
		this.m_RefreshRate.SetByOption(byOption);
	}

	public override void OnShow()
	{
		base.OnShow();
		this.FillAvailableResolutions();
		this.m_Quality.SetByOption(QualitySettings.names[QualitySettings.GetQualityLevel()]);
		Resolution resolution = ResolutionExtension.SelectResolution((Screen.fullScreenMode == FullScreenMode.Windowed) ? ResolutionExtension.ResolutionFromString(GreenHellGame.Instance.m_Settings.m_Resolution) : Screen.currentResolution);
		string byOption;
		string text;
		resolution.ToString2(out byOption, out text);
		if (!this.m_Resolution.SetByOption(byOption))
		{
			Screen.currentResolution.ToString2(out byOption, out text);
			this.m_Resolution.SetByOption(byOption);
		}
		FullScreenMode fullScreenMode = Screen.fullScreenMode;
		this.m_Fullscreen.SetSelectedOptionEnumValue<FullScreenMode>(fullScreenMode);
		this.FillAvailableRefreshRate(resolution);
		this.m_VSync.SetSelectedOptionBoolValue(QualitySettings.vSyncCount != 0);
		this.m_ShadowDistance.value = GreenHellGame.Instance.m_Settings.m_ShadowDistance;
		this.m_AntiAliasing.SetSelectedOptionBoolValue(GreenHellGame.Instance.m_Settings.m_AntiAliasing);
		this.m_TextureQuality.SetByOption(GreenHellGame.Instance.m_Settings.m_TextureQuality);
		this.m_FOV.value = GreenHellGame.Instance.m_Settings.m_FOVChange;
		this.m_ObjectDrawDistance.value = GreenHellGame.Instance.m_Settings.m_ObjectDrawDistance;
		this.m_FovTitlePrefix = GreenHellGame.Instance.GetLocalization().Get("MenuOptions_Graphics_FOV", true);
		this.OnSliderMoved(this.m_FOV);
		this.m_Brightness.value = GreenHellGame.Instance.m_Settings.m_BrightnessMul;
	}

	public override void OnBack()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsGraphics.OptionsGraphicsQuestion.Back;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), !this.m_IsIngame);
			return;
		}
		base.OnBack();
	}

	public override void OnAccept()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsGraphics.OptionsGraphicsQuestion.Accept;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept", true), !this.m_IsIngame);
			return;
		}
		this.ShowPreviousScreen();
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == MainMenuOptionsGraphics.OptionsGraphicsQuestion.Back)
		{
			this.m_FOV.RevertValue();
			GreenHellGame.Instance.m_Settings.m_FOVChange = this.m_FOV.value;
			GreenHellGame.Instance.m_Settings.ApplyFOVChange();
			this.m_ObjectDrawDistance.RevertValue();
			GreenHellGame.Instance.m_Settings.m_ObjectDrawDistance = this.m_ObjectDrawDistance.value;
			GreenHellGame.Instance.m_Settings.ApplyObjectDrawDistance();
			this.m_Brightness.RevertValue();
			GreenHellGame.Instance.m_Settings.SetBrightnessMul(this.m_Brightness.value);
			this.ShowPreviousScreen();
			return;
		}
		if (this.m_Question == MainMenuOptionsGraphics.OptionsGraphicsQuestion.Accept)
		{
			this.ApplyOptions();
			this.ShowPreviousScreen();
		}
	}

	private void ApplyOptions()
	{
		QualitySettings.SetQualityLevel(this.m_Quality.m_SelectionIdx);
		GreenHellGame.Instance.m_Settings.m_Resolution = ResolutionExtension.ToString(this.m_Resolution.GetSelectedOption(), this.m_RefreshRate.GetSelectedOption());
		GreenHellGame.Instance.m_Settings.m_Fullscreen = this.m_Fullscreen.GetSelectedOptionEnumValue<FullScreenMode>().ToString();
		GreenHellGame.Instance.m_Settings.m_VSync = this.m_VSync.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_ShadowDistance = this.m_ShadowDistance.value;
		GreenHellGame.Instance.m_Settings.m_AntiAliasing = this.m_AntiAliasing.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_FOVChange = this.m_FOV.value;
		GreenHellGame.Instance.m_Settings.m_ObjectDrawDistance = this.m_ObjectDrawDistance.value;
		GreenHellGame.Instance.m_Settings.m_TextureQuality = this.m_TextureQuality.GetSelectedOption();
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings(true);
	}

	public void OnNoFromDialog()
	{
	}

	public void OnSliderMoved(Slider slider)
	{
		if (slider == this.m_FOV)
		{
			GreenHellGame.Instance.m_Settings.m_FOVChange = this.m_FOV.value;
			GreenHellGame.Instance.m_Settings.ApplyFOVChange();
			CameraManager cameraManager = CameraManager.Get();
			if (this.m_FovTitle)
			{
				if (cameraManager)
				{
					float num = cameraManager.GetDefaultFOV() + this.m_FOV.value * GreenHellGame.Instance.m_Settings.m_FOVMaxChange;
					this.m_FovTitle.text = this.m_FovTitlePrefix + " (" + num.ToString("F1") + ")";
				}
				else
				{
					float num2 = 75f + this.m_FOV.value * GreenHellGame.Instance.m_Settings.m_FOVMaxChange;
					this.m_FovTitle.text = this.m_FovTitlePrefix + " (" + num2.ToString("F1") + ")";
				}
			}
		}
		if (slider == this.m_ObjectDrawDistance)
		{
			GreenHellGame.Instance.m_Settings.m_ObjectDrawDistance = this.m_ObjectDrawDistance.value;
			GreenHellGame.Instance.m_Settings.ApplyObjectDrawDistance();
		}
		if (slider == this.m_Brightness)
		{
			GreenHellGame.Instance.m_Settings.SetBrightnessMul(slider.value);
		}
	}

	public void OnOkFromDialog()
	{
	}

	public void OnCloseDialog()
	{
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		if (b == this.m_AcceptButton)
		{
			return base.IsAnyOptionModified();
		}
		return base.IsMenuButtonEnabled(b);
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		if (button == this.m_Resolution || button == this.m_Fullscreen)
		{
			Resolution current_res = ResolutionExtension.ResolutionFromString(ResolutionExtension.ToString(this.m_Resolution.GetSelectedOption(), this.m_RefreshRate.GetSelectedOption()));
			this.FillAvailableRefreshRate(current_res);
			return;
		}
		if (button == this.m_Quality)
		{
			Streamer[] array = Resources.FindObjectsOfTypeAll<Streamer>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					array[i].SetQualitySettingsRanges(button.m_SelectionIdx);
				}
			}
		}
	}

	public Button m_BackButton;

	public Button m_AcceptButton;

	public Text m_BackText;

	public UISelectButton m_Quality;

	public UISelectButton m_Resolution;

	public UISelectButton m_RefreshRate;

	public UISelectButton m_Fullscreen;

	public UISelectButton m_VSync;

	public UISliderEx m_ShadowDistance;

	public UISelectButton m_AntiAliasing;

	public UISliderEx m_FOV;

	public UISliderEx m_ObjectDrawDistance;

	public UISelectButton m_TextureQuality;

	public UISliderEx m_Brightness;

	private bool m_OptionsChanged;

	private MainMenuOptionsGraphics.OptionsGraphicsQuestion m_Question;

	public Text m_FovTitle;

	private string m_FovTitlePrefix = string.Empty;

	public enum OptionsGraphicsQuestion
	{
		None,
		Back,
		Accept
	}
}
