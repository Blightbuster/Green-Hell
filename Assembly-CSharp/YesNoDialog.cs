using System;
using Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class YesNoDialog : MonoBehaviour, IInputsReceiver
{
	private void Start()
	{
		this.m_DW.m_LabelText = base.gameObject.FindChild("Txt_Label").GetComponent<Text>();
		this.m_DW.m_DescriptionText = base.gameObject.FindChild("Txt_Description").GetComponent<Text>();
		this.m_DW.m_Button1Text = base.gameObject.FindChild("Button1").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button2Text = base.gameObject.FindChild("Button2").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button3Text = base.gameObject.FindChild("Button3").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button1 = base.gameObject.FindChild("Button1").GetComponent<Button>();
		this.m_DW.m_Button2 = base.gameObject.FindChild("Button2").GetComponent<Button>();
		this.m_DW.m_Button3 = base.gameObject.FindChild("Button3").GetComponent<Button>();
		base.gameObject.SetActive(false);
		this.m_DW.m_PadButton1 = base.gameObject.FindChild("PadYes").gameObject;
		this.m_DW.m_PadButton2 = base.gameObject.FindChild("PadNo").gameObject;
		this.m_DW.m_PadButton3 = base.gameObject.FindChild("PadOk").gameObject;
		this.m_DW.m_PadButton1Text = this.m_DW.m_PadButton1.FindChild("Text").GetComponent<Text>();
		this.m_DW.m_PadButton2Text = this.m_DW.m_PadButton2.FindChild("Text").GetComponent<Text>();
		this.m_DW.m_PadButton3Text = this.m_DW.m_PadButton3.FindChild("Text").GetComponent<Text>();
		this.ApplyLanguage();
	}

	public void ApplyLanguage()
	{
		this.m_DW.m_Button1Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Yes", true);
		this.m_DW.m_Button2Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_No", true);
		this.m_DW.m_Button3Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_OK", true);
		this.m_DW.m_PadButton1Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Yes", true);
		this.m_DW.m_PadButton2Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_No", true);
		this.m_DW.m_PadButton3Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_OK", true);
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Button_A)
		{
			if (this.m_DialogType == DialogWindowType.Ok)
			{
				this.OkButtonClicked();
				return;
			}
			this.YesButtonClicked();
		}
	}

	public bool CanReceiveAction()
	{
		return base.gameObject.activeSelf;
	}

	public bool CanReceiveActionPaused()
	{
		return base.gameObject.activeSelf;
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(InputHelpers.PadButton.Button_B.KeyFromPad()))
		{
			if (this.m_DialogType == DialogWindowType.YesNo)
			{
				this.NoButtonClicked();
			}
			if (this.m_DialogType == DialogWindowType.Ok)
			{
				this.OkButtonClicked();
				return;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			if (this.m_DialogType == DialogWindowType.YesNo)
			{
				this.YesButtonClicked();
			}
			if (this.m_DialogType == DialogWindowType.Ok)
			{
				this.OkButtonClicked();
			}
		}
	}

	public void OnButtonEnter()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Focus);
	}

	public void OnButtonExit()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
	}

	public void Show(IYesNoDialogOwner owner, DialogWindowType type, string label = "label", string description = "description", bool change_effect = true)
	{
		this.m_DialogType = type;
		this.m_DW.m_LabelText.text = label;
		this.m_DW.m_DescriptionText.text = description;
		if (GreenHellGame.IsPadControllerActive())
		{
			this.m_DW.m_Button1.gameObject.SetActive(false);
			this.m_DW.m_Button2.gameObject.SetActive(false);
			this.m_DW.m_Button3.gameObject.SetActive(false);
			if (type == DialogWindowType.YesNo)
			{
				this.m_DW.m_PadButton1.gameObject.SetActive(true);
				this.m_DW.m_PadButton2.gameObject.SetActive(true);
				this.m_DW.m_PadButton3.gameObject.SetActive(false);
			}
			else if (type == DialogWindowType.Ok)
			{
				this.m_DW.m_PadButton1.gameObject.SetActive(false);
				this.m_DW.m_PadButton2.gameObject.SetActive(false);
				this.m_DW.m_PadButton3.gameObject.SetActive(true);
			}
		}
		else
		{
			this.m_DW.m_PadButton1.gameObject.SetActive(false);
			this.m_DW.m_PadButton2.gameObject.SetActive(false);
			this.m_DW.m_PadButton3.gameObject.SetActive(false);
			if (type == DialogWindowType.YesNo)
			{
				this.m_DW.m_Button1.gameObject.SetActive(true);
				this.m_DW.m_Button2.gameObject.SetActive(true);
				this.m_DW.m_Button3.gameObject.SetActive(false);
			}
			else if (type == DialogWindowType.Ok)
			{
				this.m_DW.m_Button1.gameObject.SetActive(false);
				this.m_DW.m_Button2.gameObject.SetActive(false);
				this.m_DW.m_Button3.gameObject.SetActive(true);
			}
		}
		base.gameObject.SetActive(true);
		this.m_Screen = owner;
		this.SetCanvasActive(false);
		this.m_ChangeEffect = change_effect;
		if (this.m_ChangeEffect)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 1f);
		}
		MenuScreen menuScreen = this.m_Screen as MenuScreen;
		if (menuScreen != null && !menuScreen.m_IsIngame)
		{
			Time.timeScale = 0.5f;
		}
		InputsManager.Get().RegisterReceiver(this);
	}

	private void Close()
	{
		base.gameObject.SetActive(false);
		this.SetCanvasActive(true);
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
		if (this.m_ChangeEffect)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 0f);
		}
		MenuScreen menuScreen = this.m_Screen as MenuScreen;
		if (menuScreen != null && !menuScreen.m_IsIngame)
		{
			Time.timeScale = 1f;
		}
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			current.SetSelectedGameObject(null);
		}
		InputsManager.Get().UnregisterReceiver(this);
		IYesNoDialogOwner screen = this.m_Screen;
		if (screen == null)
		{
			return;
		}
		screen.OnCloseDialog();
	}

	public void YesButtonClicked()
	{
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
		IYesNoDialogOwner screen = this.m_Screen;
		if (screen != null)
		{
			screen.OnYesFromDialog();
		}
		this.Close();
	}

	public void NoButtonClicked()
	{
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
		IYesNoDialogOwner screen = this.m_Screen;
		if (screen != null)
		{
			screen.OnNoFromDialog();
		}
		this.Close();
	}

	public void OkButtonClicked()
	{
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
		IYesNoDialogOwner screen = this.m_Screen;
		if (screen != null)
		{
			screen.OnOkFromDialog();
		}
		this.Close();
	}

	private void SetCanvasActive(bool active)
	{
		CanvasGroup canvasGroup = null;
		if (MenuInGameManager.Get())
		{
			canvasGroup = MenuInGameManager.Get().m_CanvasGroup;
		}
		else if (MenuInGameManager.Get())
		{
			canvasGroup = MainMenuManager.Get().m_CanvasGroup;
		}
		if (canvasGroup != null)
		{
			canvasGroup.blocksRaycasts = active;
		}
	}

	private DialogueWindow m_DW = new DialogueWindow();

	private IYesNoDialogOwner m_Screen;

	private DialogWindowType m_DialogType;

	private bool m_ChangeEffect = true;
}
