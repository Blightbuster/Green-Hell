using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptionsControlsKeysBinding : MenuScreen, IYesNoDialogOwner, IInputsReceiver
{
	protected override void Awake()
	{
		base.Awake();
		this.m_KeyBindButtons = base.GetComponentsInChildren<UIKeyBindButton>();
	}

	public override void OnShow()
	{
		base.OnShow();
		this.InitializeButtons();
		InputsManager.Get().RegisterReceiver(this);
	}

	public override void OnHide()
	{
		base.OnHide();
		InputsManager.Get().UnregisterReceiver(this);
	}

	private bool InitializeButtons()
	{
		bool result = false;
		foreach (UIKeyBindButton uikeyBindButton in this.m_KeyBindButtons)
		{
			uikeyBindButton.SetMenuScreen(this);
			if (uikeyBindButton.m_Actions.Count > 0)
			{
				if (uikeyBindButton.SetupKeyCode(this.GetKeyCodeByInputAction(uikeyBindButton.m_Actions[0])))
				{
					result = true;
				}
			}
			else if (uikeyBindButton.m_TriggerActions.Count > 0 && uikeyBindButton.SetupKeyCode(this.GetKeyCodeByTriggerAction(uikeyBindButton.m_TriggerActions[0])))
			{
				result = true;
			}
		}
		return result;
	}

	private KeyCode GetKeyCodeByInputAction(InputsManager.InputAction action)
	{
		InputActionData actionDataByInputAction = InputsManager.Get().GetActionDataByInputAction(action, ControllerType._Count);
		if (actionDataByInputAction != null)
		{
			return actionDataByInputAction.m_KeyCode;
		}
		return KeyCode.Space;
	}

	private KeyCode GetKeyCodeByTriggerAction(TriggerAction.TYPE action)
	{
		InputActionData actionDataByTriggerAction = InputsManager.Get().GetActionDataByTriggerAction(action, ControllerType._Count);
		if (actionDataByTriggerAction != null)
		{
			return actionDataByTriggerAction.m_KeyCode;
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
		return KeyCode.Space;
	}

	public override void OnBack()
	{
		if (this.m_AcceptButton.interactable)
		{
			this.m_Question = MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Back;
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), false);
			return;
		}
		this.ShowPreviousScreen();
	}

	public void OnDefault()
	{
		this.m_Question = MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Default;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_BackTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Back", true), false);
	}

	public override void OnAccept()
	{
		this.m_Question = MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Save;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle", true), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept", true), false);
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Save)
		{
			this.ApplyOptions();
			GreenHellGame.Instance.m_Settings.SaveSettings();
			this.ShowPreviousScreen();
			return;
		}
		if (this.m_Question == MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Back)
		{
			this.ShowPreviousScreen();
			return;
		}
		if (this.m_Question == MainMenuOptionsControlsKeysBinding.KeyBindingQuestion.Default)
		{
			this.ApplyDefaultOptions();
		}
	}

	private void ApplyOptions()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		foreach (UIKeyBindButton uikeyBindButton in this.m_KeyBindButtons)
		{
			if (uikeyBindButton.m_Actions.Count > 0)
			{
				this.AddActionsByInputAction(uikeyBindButton, dictionary);
			}
			if (uikeyBindButton.m_TriggerActions.Count > 0)
			{
				this.AddActionsByTriggerAction(uikeyBindButton, dictionary2);
			}
		}
		InputsManager.Get().ApplyOptions(dictionary, dictionary2, ControllerType.PC);
		if (HintsManager.Get() != null)
		{
			HintsManager.Get().ReloadScript();
		}
	}

	private void ApplyDefaultOptions()
	{
		InputsManager.Get().ApplyDefaultOptions();
		if (this.InitializeButtons())
		{
			GreenHellGame.Instance.m_Settings.SaveSettings();
		}
		if (HintsManager.Get() != null)
		{
			HintsManager.Get().ReloadScript();
		}
	}

	private void AddActionsByInputAction(UIKeyBindButton button, Dictionary<int, int> map)
	{
		for (int i = 0; i < button.GetInputActions().Count; i++)
		{
			map.Add((int)button.GetInputActions()[i], (int)button.GetKeyCode());
		}
	}

	private void AddActionsByTriggerAction(UIKeyBindButton button, Dictionary<int, int> map)
	{
		for (int i = 0; i < button.GetTriggerActions().Count; i++)
		{
			map.Add((int)button.GetTriggerActions()[i], (int)button.GetKeyCode());
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

	public override void OnInputAction(InputActionData action_data)
	{
	}

	public override bool CanReceiveAction()
	{
		return base.gameObject.activeInHierarchy;
	}

	public override bool CanReceiveActionPaused()
	{
		return false;
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		if (b == this.m_AcceptButton)
		{
			return this.m_KeyBindButtons.Any((UIKeyBindButton bind) => bind.m_BindingChanged);
		}
		return base.IsMenuButtonEnabled(b);
	}

	public override void OnBindingChanged(UIKeyBindButton button)
	{
		foreach (UIKeyBindButton uikeyBindButton in this.m_KeyBindButtons)
		{
			if (!(uikeyBindButton == button) && button.GetKeyCode() == uikeyBindButton.GetKeyCode())
			{
				uikeyBindButton.StartBlinking();
				button.StartBlinking();
			}
		}
	}

	public Button m_BackButton;

	public Text m_BackText;

	public Button m_AcceptButton;

	public Text m_AcceptText;

	public Button m_DefaultButton;

	public Text m_DefaultText;

	private UIKeyBindButton[] m_KeyBindButtons;

	private MainMenuOptionsControlsKeysBinding.KeyBindingQuestion m_Question;

	private enum KeyBindingQuestion
	{
		Save,
		Default,
		Back
	}
}
