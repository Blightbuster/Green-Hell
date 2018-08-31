using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptionsGame : MainMenuScreen, IYesNoDialogOwner
{
	private void Start()
	{
		this.m_Language.SetTitle(GreenHellGame.Instance.GetLocalization().Get("MainMenu_Options_SelectLanguage"));
		this.m_Language.AddOption("English", GreenHellGame.Instance.GetLocalization().Get("Language_English"));
		this.m_Language.AddOption("French", GreenHellGame.Instance.GetLocalization().Get("Language_French"));
		this.m_Language.AddOption("Italian", GreenHellGame.Instance.GetLocalization().Get("Language_Italian"));
		this.m_Language.AddOption("German", GreenHellGame.Instance.GetLocalization().Get("Language_German"));
		this.m_Language.AddOption("Spanish", GreenHellGame.Instance.GetLocalization().Get("Language_Spanish"));
		this.m_Language.AddOption("ChineseTraditional", GreenHellGame.Instance.GetLocalization().Get("Language_ChineseTraditional"));
		this.m_Language.AddOption("ChineseSimplyfied", GreenHellGame.Instance.GetLocalization().Get("Language_ChineseSimplyfied"));
		this.m_Language.AddOption("Portuguese", GreenHellGame.Instance.GetLocalization().Get("Language_Portuguese"));
		this.m_Language.AddOption("PortugueseBrazilian", GreenHellGame.Instance.GetLocalization().Get("Language_PortugueseBrazilian"));
		this.m_Language.AddOption("Russian", GreenHellGame.Instance.GetLocalization().Get("Language_Russian"));
		switch (GreenHellGame.Instance.m_Settings.m_Language)
		{
		case Language.English:
			this.m_Language.SetByOption("English");
			this.m_SelectedLanguage = Language.English;
			break;
		case Language.French:
			this.m_Language.SetByOption("French");
			this.m_SelectedLanguage = Language.French;
			break;
		case Language.Italian:
			this.m_Language.SetByOption("Italian");
			this.m_SelectedLanguage = Language.Italian;
			break;
		case Language.German:
			this.m_Language.SetByOption("German");
			this.m_SelectedLanguage = Language.German;
			break;
		case Language.Spanish:
			this.m_Language.SetByOption("Spanish");
			this.m_SelectedLanguage = Language.Spanish;
			break;
		case Language.ChineseTraditional:
			this.m_Language.SetByOption("ChineseTraditional");
			this.m_SelectedLanguage = Language.ChineseTraditional;
			break;
		case Language.ChineseSimplyfied:
			this.m_Language.SetByOption("ChineseSimplyfied");
			this.m_SelectedLanguage = Language.ChineseSimplyfied;
			break;
		case Language.Portuguese:
			this.m_Language.SetByOption("Portuguese");
			this.m_SelectedLanguage = Language.Portuguese;
			break;
		case Language.PortugueseBrazilian:
			this.m_Language.SetByOption("PortugueseBrazilian");
			this.m_SelectedLanguage = Language.PortugueseBrazilian;
			break;
		case Language.Russian:
			this.m_Language.SetByOption("Russian");
			this.m_SelectedLanguage = Language.Russian;
			break;
		}
		this.m_AcceptButton.interactable = false;
	}

	private void ApplyLocalization()
	{
		this.m_Language.SetOptionText("English", GreenHellGame.Instance.GetLocalization().Get("Language_English"));
		this.m_Language.SetOptionText("French", GreenHellGame.Instance.GetLocalization().Get("Language_French"));
		this.m_Language.SetOptionText("Italian", GreenHellGame.Instance.GetLocalization().Get("Language_Italian"));
		this.m_Language.SetOptionText("German", GreenHellGame.Instance.GetLocalization().Get("Language_German"));
		this.m_Language.SetOptionText("Spanish", GreenHellGame.Instance.GetLocalization().Get("Language_Spanish"));
		this.m_Language.SetOptionText("ChineseTraditional", GreenHellGame.Instance.GetLocalization().Get("Language_ChineseTraditional"));
		this.m_Language.SetOptionText("ChineseSimplyfied", GreenHellGame.Instance.GetLocalization().Get("Language_ChineseSimplyfied"));
		this.m_Language.SetOptionText("Portuguese", GreenHellGame.Instance.GetLocalization().Get("Language_Portuguese"));
		this.m_Language.SetOptionText("PortugueseBrazilian", GreenHellGame.Instance.GetLocalization().Get("Language_PortugueseBrazilian"));
		this.m_Language.SetOptionText("Russian", GreenHellGame.Instance.GetLocalization().Get("Language_Russian"));
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		this.m_AcceptButton.interactable = true;
		if (button == this.m_Language)
		{
			if (option == "English")
			{
				this.m_SelectedLanguage = Language.English;
			}
			else if (option == "French")
			{
				this.m_SelectedLanguage = Language.French;
			}
			else if (option == "Italian")
			{
				this.m_SelectedLanguage = Language.Italian;
			}
			else if (option == "German")
			{
				this.m_SelectedLanguage = Language.German;
			}
			else if (option == "Spanish")
			{
				this.m_SelectedLanguage = Language.Spanish;
			}
			else if (option == "ChineseTraditional")
			{
				this.m_SelectedLanguage = Language.ChineseTraditional;
			}
			else if (option == "ChineseSimplyfied")
			{
				this.m_SelectedLanguage = Language.ChineseSimplyfied;
			}
			else if (option == "Portuguese")
			{
				this.m_SelectedLanguage = Language.Portuguese;
			}
			else if (option == "PortugueseBrazilian")
			{
				this.m_SelectedLanguage = Language.PortugueseBrazilian;
			}
			else if (option == "Russian")
			{
				this.m_SelectedLanguage = Language.Russian;
			}
		}
	}

	private void Update()
	{
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
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptions), true);
		}
		else if (this.m_Question == OptionsGameQuestion.Accept)
		{
			this.ApplySettings();
			GreenHellGame.Instance.m_Settings.SaveSettings();
			GreenHellGame.Instance.m_Settings.ApplySettings();
			this.ApplyLocalization();
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
		GreenHellGame.Instance.m_Settings.m_Language = this.m_SelectedLanguage;
	}

	private Language m_SelectedLanguage;

	public UISelectButton m_Language;

	public Button m_AcceptButton;

	public Button m_BackButton;

	private OptionsGameQuestion m_Question;
}
