using System;
using Enums;
using UnityEngine.UI;

public class MainMenuOptionsGame : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		if (GreenHellGame.GAMESCOM_DEMO)
		{
			this.m_Language.interactable = false;
		}
		EnumUtils<Language>.ForeachValue(new Action<Language>(this.AddLanguageOption));
		this.m_Subtitles.FillOptionsFromEnum<SubtitlesSetting>("Subtitles_Setting_");
		this.ApplyControls();
	}

	private void AddLanguageOption(Language lang)
	{
		if (Array.IndexOf<Language>(MainMenuOptionsGame.DISABLED_LANGUAGES, lang) == -1)
		{
			this.m_Language.AddOption(EnumUtils<Language>.GetName((int)lang), "Language_" + EnumUtils<Language>.GetName((int)lang));
		}
	}

	private void SetLanguageOptionText(Language lang)
	{
		if (Array.IndexOf<Language>(MainMenuOptionsGame.DISABLED_LANGUAGES, lang) == -1)
		{
			this.m_Language.SetOptionText(EnumUtils<Language>.GetName((int)lang), "Language_" + EnumUtils<Language>.GetName((int)lang));
		}
	}

	private bool SelectCurrentlyUsedLanguage(Language lang)
	{
		if (lang == GreenHellGame.Instance.m_Settings.m_Language)
		{
			this.m_Language.SetByOption(EnumUtils<Language>.GetName(lang));
			return true;
		}
		return false;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.ApplyControls();
	}

	private void ApplyControls()
	{
		EnumUtils<Language>.ForeachValueSelector(new Func<Language, bool>(this.SelectCurrentlyUsedLanguage));
		this.m_Subtitles.SetSelectedOptionEnumValue<SubtitlesSetting>(GreenHellGame.Instance.m_Settings.m_Subtitles);
		this.m_Crosshair.SetSelectedOptionBoolValue(GreenHellGame.Instance.m_Settings.m_Crosshair);
		this.m_Hints.SetSelectedOptionBoolValue(GreenHellGame.Instance.m_Settings.m_Hints);
	}

	public override void OnBack()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsGame.OptionsGameQuestion.Back;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), !this.m_IsIngame);
			return;
		}
		base.OnBack();
	}

	public override void OnAccept()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsGame.OptionsGameQuestion.Accept;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept", true), !this.m_IsIngame);
			return;
		}
		this.ShowPreviousScreen();
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == MainMenuOptionsGame.OptionsGameQuestion.Back)
		{
			this.ShowPreviousScreen();
			return;
		}
		if (this.m_Question == MainMenuOptionsGame.OptionsGameQuestion.Accept)
		{
			this.ApplySettings();
			GreenHellGame.Instance.m_Settings.SaveSettings();
			GreenHellGame.Instance.m_Settings.ApplySettings(false);
			GreenHellGame.GetYesNoDialog().ApplyLanguage();
			this.ShowPreviousScreen();
		}
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

	private void ApplySettings()
	{
		GreenHellGame.Instance.m_Settings.m_Language = this.m_Language.GetSelectedOptionEnumValue<Language>();
		GreenHellGame.Instance.m_Settings.m_Subtitles = this.m_Subtitles.GetSelectedOptionEnumValue<SubtitlesSetting>();
		GreenHellGame.Instance.m_Settings.m_Crosshair = this.m_Crosshair.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_Hints = this.m_Hints.GetSelectedOptionBoolValue();
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		if (b == this.m_AcceptButton)
		{
			return base.IsAnyOptionModified();
		}
		return base.IsMenuButtonEnabled(b);
	}

	public UISelectButton m_Language;

	public UISelectButton m_Subtitles;

	public UISelectButton m_Crosshair;

	public UISelectButton m_Hints;

	public Button m_AcceptButton;

	public Button m_BackButton;

	private readonly string LARGE_SUBTITLES = "LARGE";

	private MainMenuOptionsGame.OptionsGameQuestion m_Question;

	private static readonly Language[] DISABLED_LANGUAGES = new Language[0];

	public enum OptionsGameQuestion
	{
		None,
		Back,
		Accept
	}
}
