using System;
using CJTools;
using Enums;
using UnityEngine.UI;

public class MainMenuOptionsControls : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_InvertMouseYButton.AddOption("No", "MenuYesNoDialog_No");
		this.m_InvertMouseYButton.AddOption("Yes", "MenuYesNoDialog_Yes");
		this.m_ToggleRunButton.AddOption("No", "MenuYesNoDialog_No");
		this.m_ToggleRunButton.AddOption("Yes", "MenuYesNoDialog_Yes");
		this.m_ToggleRunButton.AddOption("Always", "MenuOptionsControls_AlwaysRun");
		this.m_ToggleCrouchButton.AddOption("No", "MenuYesNoDialog_No");
		this.m_ToggleCrouchButton.AddOption("Yes", "MenuYesNoDialog_Yes");
		this.m_ToggleWatchButton.AddOption("No", "MenuYesNoDialog_No");
		this.m_ToggleWatchButton.AddOption("Yes", "MenuYesNoDialog_Yes");
		this.m_ControllerButton.AddOption("No", "MenuOptionsControls_Controller_Keyboard");
		this.m_ControllerButton.AddOption("Yes", "MenuOptionsControls_Controller_Pad");
		this.m_KeyBindingsText = this.m_KeyBindingsButton.GetComponentInChildren<Text>();
	}

	public override void OnShow()
	{
		base.OnShow();
		this.m_ControllerButton.SetByOption((GreenHellGame.Instance.m_Settings.m_ControllerType == ControllerType.PC) ? "No" : "Yes");
		switch (GreenHellGame.Instance.m_Settings.m_ToggleRunOption)
		{
		case GameSettings.ToggleRunOption.No:
			this.m_ToggleRunButton.SetByOption("No");
			break;
		case GameSettings.ToggleRunOption.Yes:
			this.m_ToggleRunButton.SetByOption("Yes");
			break;
		case GameSettings.ToggleRunOption.Always:
			this.m_ToggleRunButton.SetByOption("On");
			break;
		}
		this.m_ToggleCrouchButton.SetByOption(GreenHellGame.Instance.m_Settings.m_ToggleCrouch ? "Yes" : "No");
		this.m_ToggleWatchButton.SetByOption(GreenHellGame.Instance.m_Settings.m_ToggleWatch ? "Yes" : "No");
		this.m_InvertMouseYButton.SetByOption(GreenHellGame.Instance.m_Settings.m_InvertMouseY ? "Yes" : "No");
		this.m_InvertMouseY = GreenHellGame.Instance.m_Settings.m_InvertMouseY;
		this.m_XSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_XSensitivity;
		this.m_YSensitivitySlider.value = GreenHellGame.Instance.m_Settings.m_YSensitivity;
		if (GreenHellGame.Instance.m_Settings.m_LookRotationSpeed > 50f)
		{
			this.m_MouseSmoothing.value = CJTools.Math.GetProportionalClamp(0f, 0.5f, GreenHellGame.Instance.m_Settings.m_LookRotationSpeed, 200f, 50f);
		}
		else
		{
			this.m_MouseSmoothing.value = CJTools.Math.GetProportionalClamp(0.5f, 1f, GreenHellGame.Instance.m_Settings.m_LookRotationSpeed, 50f, 12f);
		}
		this.m_CursorVisible = GreenHellGame.IsPCControllerActive();
		this.m_KeyBindingsButton.gameObject.SetActive(GreenHellGame.IsPCControllerActive());
		this.m_PadControllerConnected = GreenHellGame.IsPadControllerConnected();
		this.m_ControllerButton.gameObject.SetActive(this.m_PadControllerConnected);
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		if (button == this.m_InvertMouseYButton)
		{
			if (option == "Yes")
			{
				this.m_InvertMouseY = true;
				return;
			}
			if (option == "No")
			{
				this.m_InvertMouseY = false;
			}
		}
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == MainMenuOptionsControls.OptionsControlsQuestion.Back)
		{
			this.ShowPreviousScreen();
			return;
		}
		if (this.m_Question == MainMenuOptionsControls.OptionsControlsQuestion.Accept)
		{
			this.ChangeControllerOption();
			this.ShowPreviousScreen();
		}
	}

	public void ChangeControllerOption()
	{
		this.ApplySettings();
		GreenHellGame.Instance.OnChangeControllerOption();
		this.m_KeyBindingsButton.gameObject.SetActive(GreenHellGame.IsPCControllerActive());
		this.OnChangeControllerOption();
	}

	private void ShowCursor()
	{
		CursorManager.Get().ShowCursor(true, false);
		this.m_CursorVisible = true;
		MenuInGameManager menuInGameManager = MenuInGameManager.Get();
		if (menuInGameManager == null)
		{
			return;
		}
		menuInGameManager.SetCursorVisible(true);
	}

	private void HideCursor()
	{
		CursorManager.Get().ShowCursor(false, false);
		this.m_CursorVisible = false;
		MenuInGameManager menuInGameManager = MenuInGameManager.Get();
		if (menuInGameManager == null)
		{
			return;
		}
		menuInGameManager.SetCursorVisible(false);
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
		GreenHellGame.Instance.m_Settings.m_ToggleRunOption = this.m_ToggleRunButton.GetSelectedOptionEnumValue<GameSettings.ToggleRunOption>();
		GreenHellGame.Instance.m_Settings.m_ToggleCrouch = this.m_ToggleCrouchButton.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_ToggleWatch = this.m_ToggleWatchButton.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_InvertMouseY = this.m_InvertMouseYButton.GetSelectedOptionBoolValue();
		GreenHellGame.Instance.m_Settings.m_XSensitivity = this.m_XSensitivitySlider.value;
		GreenHellGame.Instance.m_Settings.m_YSensitivity = this.m_YSensitivitySlider.value;
		GreenHellGame.Instance.m_Settings.m_ControllerType = (ControllerType)this.m_ControllerButton.m_SelectionIdx;
		if (this.m_MouseSmoothing.value > 0.5f)
		{
			GreenHellGame.Instance.m_Settings.m_LookRotationSpeed = CJTools.Math.GetProportionalClamp(50f, 12f, this.m_MouseSmoothing.value, 0.5f, 1f);
			return;
		}
		GreenHellGame.Instance.m_Settings.m_LookRotationSpeed = CJTools.Math.GetProportionalClamp(200f, 50f, this.m_MouseSmoothing.value, 0f, 0.5f);
	}

	public override void OnBack()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsControls.OptionsControlsQuestion.Back;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), !this.m_IsIngame);
			return;
		}
		base.OnBack();
	}

	public override void OnAccept()
	{
		if (base.IsAnyOptionModified())
		{
			this.m_Question = MainMenuOptionsControls.OptionsControlsQuestion.Accept;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept", true), !this.m_IsIngame);
			return;
		}
		this.ShowPreviousScreen();
	}

	public void OnKeyBindings()
	{
		if (this.m_IsIngame)
		{
			this.m_MenuInGameManager.ShowScreen(typeof(MainMenuOptionsControlsKeysBinding));
			return;
		}
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsControlsKeysBinding), true);
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		if (b == this.m_AcceptButton)
		{
			return base.IsAnyOptionModified();
		}
		return base.IsMenuButtonEnabled(b);
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateController();
	}

	private void UpdateController()
	{
		if (GreenHellGame.IsPadControllerConnected() != this.m_PadControllerConnected)
		{
			this.OnChangeControllerOption();
		}
	}

	private void OnChangeControllerOption()
	{
		base.SetupController();
		bool flag = GreenHellGame.IsPadControllerConnected();
		this.m_ControllerButton.gameObject.SetActive(flag);
		this.m_ControllerButton.SetByOption(GreenHellGame.IsPCControllerActive() ? "No" : "Yes");
		if (GreenHellGame.IsPadControllerActive() && this.m_CursorVisible)
		{
			this.HideCursor();
		}
		else if (GreenHellGame.IsPCControllerActive() && !this.m_CursorVisible)
		{
			this.ShowCursor();
		}
		this.m_PadControllerConnected = flag;
	}

	public UISelectButton m_ControllerButton;

	public UISelectButton m_InvertMouseYButton;

	public UISelectButton m_ToggleRunButton;

	public UISelectButton m_ToggleCrouchButton;

	public UISelectButton m_ToggleWatchButton;

	public Slider m_XSensitivitySlider;

	public Slider m_YSensitivitySlider;

	public Slider m_MouseSmoothing;

	private bool m_InvertMouseY;

	public Button m_KeyBindingsButton;

	public Text m_KeyBindingsText;

	public Button m_AcceptButton;

	public Button m_BackButton;

	private const float SPEED_SMOOTHING_MAX = 12f;

	private const float SPEED_SMOOTHING_MIN = 200f;

	private const float SPEED_SMOOTHING_DEFAULT = 50f;

	private MainMenuOptionsControls.OptionsControlsQuestion m_Question;

	private bool m_CursorVisible;

	private bool m_PadControllerConnected;

	public enum OptionsControlsQuestion
	{
		None,
		Back,
		Accept
	}
}
