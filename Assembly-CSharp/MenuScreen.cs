using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuScreen : MenuBase, IInputsReceiver
{
	protected override void Awake()
	{
		base.Awake();
		this.SetupController();
	}

	public virtual bool IsDebugScreen()
	{
		return false;
	}

	protected override void Update()
	{
		if (GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
		{
			return;
		}
		base.Update();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
			return;
		}
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Right))
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Slider)
			{
				float actionValue = InputsManager.Get().GetActionValue(InputsManager.InputAction.Right);
				if (actionValue > 0.3f)
				{
					this.m_ActiveMenuOption.m_Slider.value += (this.m_ActiveMenuOption.m_Slider.maxValue - this.m_ActiveMenuOption.m_Slider.minValue) * actionValue * Time.unscaledDeltaTime;
					return;
				}
			}
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Left))
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Slider)
			{
				float actionValue2 = InputsManager.Get().GetActionValue(InputsManager.InputAction.Left);
				if (actionValue2 > 0.3f)
				{
					this.m_ActiveMenuOption.m_Slider.value -= (this.m_ActiveMenuOption.m_Slider.maxValue - this.m_ActiveMenuOption.m_Slider.minValue) * actionValue2 * Time.unscaledDeltaTime;
					return;
				}
			}
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.DPadRight))
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Slider)
			{
				this.m_ActiveMenuOption.m_Slider.value += (this.m_ActiveMenuOption.m_Slider.maxValue - this.m_ActiveMenuOption.m_Slider.minValue) * Time.unscaledDeltaTime;
				return;
			}
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.DPadLeft) && this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Slider)
		{
			this.m_ActiveMenuOption.m_Slider.value -= (this.m_ActiveMenuOption.m_Slider.maxValue - this.m_ActiveMenuOption.m_Slider.minValue) * Time.unscaledDeltaTime;
		}
	}

	public virtual KeyCode GetShortcutKey()
	{
		return KeyCode.None;
	}

	public virtual bool CanReceiveAction()
	{
		return true;
	}

	public virtual bool CanReceiveActionPaused()
	{
		return true;
	}

	public virtual void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Button_B)
		{
			if (!GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
			{
				this.OnBack();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Button_X)
		{
			if (!GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
			{
				this.OnAccept();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Button_A)
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Button && !GreenHellGame.IsYesNoDialogActive())
			{
				string persistentMethodName = this.m_ActiveMenuOption.m_Button.onClick.GetPersistentMethodName(0);
				base.SendMessage(persistentMethodName);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.LSRight || action_data.m_Action == InputsManager.InputAction.DPadRight)
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_SelectButton)
			{
				this.m_ActiveMenuOption.m_SelectButton.PressRightArrow();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.LSLeft || action_data.m_Action == InputsManager.InputAction.DPadLeft)
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_SelectButton)
			{
				this.m_ActiveMenuOption.m_SelectButton.PressLeftArrow();
				return;
			}
		}
		else
		{
			if (action_data.m_Action == InputsManager.InputAction.LSBackward || action_data.m_Action == InputsManager.InputAction.DPadDown)
			{
				for (int i = 0; i < this.m_OptionsObjects.Values.Count; i++)
				{
					MenuBase.MenuOptionData menuOptionData = this.m_OptionsObjects.Values.ElementAt(i);
					if (this.m_ActiveMenuOption == menuOptionData)
					{
						for (int j = i + 1; j < this.m_OptionsObjects.Values.Count; j++)
						{
							menuOptionData = this.m_OptionsObjects.Values.ElementAt(j);
							if (this.IsMenuButtonEnabled(menuOptionData.m_Button))
							{
								UIButtonEx component = menuOptionData.m_Button.GetComponent<UIButtonEx>();
								if (!component || component.m_MoveWhenFocused)
								{
									this.m_ActiveMenuOption = menuOptionData;
									return;
								}
							}
							if (this.IsMenuSliderEnabled(menuOptionData.m_Slider))
							{
								this.m_ActiveMenuOption = menuOptionData;
								return;
							}
							if (this.IsMenuSelectButtonEnabled(menuOptionData.m_SelectButton))
							{
								this.m_ActiveMenuOption = menuOptionData;
								return;
							}
						}
						return;
					}
				}
				return;
			}
			if (action_data.m_Action == InputsManager.InputAction.LSForward || action_data.m_Action == InputsManager.InputAction.DPadUp)
			{
				for (int k = 0; k < this.m_OptionsObjects.Values.Count; k++)
				{
					MenuBase.MenuOptionData menuOptionData2 = this.m_OptionsObjects.Values.ElementAt(k);
					if (this.m_ActiveMenuOption == menuOptionData2)
					{
						for (int l = k - 1; l >= 0; l--)
						{
							menuOptionData2 = this.m_OptionsObjects.Values.ElementAt(l);
							if (this.IsMenuButtonEnabled(menuOptionData2.m_Button))
							{
								UIButtonEx component2 = menuOptionData2.m_Button.GetComponent<UIButtonEx>();
								if (!component2 || component2.m_MoveWhenFocused)
								{
									this.m_ActiveMenuOption = menuOptionData2;
									return;
								}
							}
							if (this.IsMenuSliderEnabled(menuOptionData2.m_Slider))
							{
								this.m_ActiveMenuOption = menuOptionData2;
								return;
							}
							if (this.IsMenuSelectButtonEnabled(menuOptionData2.m_SelectButton))
							{
								this.m_ActiveMenuOption = menuOptionData2;
								return;
							}
						}
						return;
					}
				}
			}
		}
	}

	public virtual bool ShouldPauseGame()
	{
		return true;
	}

	public virtual void OnClose()
	{
		if (this.m_IsIngame)
		{
			this.m_MenuInGameManager.HideMenu();
		}
	}

	public override void OnBack()
	{
		this.ShowPreviousScreen();
	}

	public virtual void OnAccept()
	{
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
		this.OnShow();
		base.OnPostShow();
		if (this.m_IsIngame)
		{
			Player.Get().OnMenuScreenShow(base.GetType());
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		this.OnHide();
		if (this.m_IsIngame)
		{
			Player.Get().OnMenuScreenHide();
		}
	}

	protected void SetVisibleHUD(HUDManager.HUDGroup group)
	{
		if (this.m_IsIngame)
		{
			this.m_VisibleHUD = group;
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		this.UpdateItemsVisibility();
		if (this.m_IsIngame && HUDManager.Get().m_ActiveGroup != HUDManager.HUDGroup.Credits)
		{
			HUDManager.Get().SetActiveGroup(this.m_VisibleHUD);
		}
		InputsManager.Get().RegisterReceiver(this);
		this.SetupController();
	}

	public override void OnHide()
	{
		base.OnHide();
		if (this.m_IsIngame && HUDManager.Get().m_ActiveGroup != HUDManager.HUDGroup.Credits)
		{
			HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
		}
		InputsManager.Get().UnregisterReceiver(this);
	}

	public virtual void ShowPreviousScreen()
	{
		if (this.m_IsIngame)
		{
			this.m_MenuInGameManager.ShowPrevoiusScreen();
			return;
		}
		MainMenuManager.Get().ShowPreviousScreen();
	}

	private void UpdateItemsVisibility()
	{
		foreach (MenuBase.MenuOptionData menuOptionData in this.m_OptionsObjects.Values)
		{
			menuOptionData.SetBackgroundVisible(!this.m_IsIngame);
		}
	}

	public virtual void OnBindingChanged(UIKeyBindButton button)
	{
	}

	public void SetupController()
	{
		bool flag = GreenHellGame.IsPadControllerActive();
		foreach (GameObject gameObject in this.m_PadDisableElements)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(!flag);
			}
		}
		foreach (GameObject gameObject2 in this.m_PadEnableElements)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(flag);
			}
		}
	}

	public static float s_ButtonsAlpha = 1f;

	public static float s_ButtonsHighlightedAlpha = 1f;

	public static float s_InactiveButtonsAlpha = 0.3f;

	[HideInInspector]
	public bool m_IsIngame;

	private HUDManager.HUDGroup m_VisibleHUD;

	[HideInInspector]
	public MenuInGameManager m_MenuInGameManager;

	public List<GameObject> m_PadEnableElements = new List<GameObject>();

	public List<GameObject> m_PadDisableElements = new List<GameObject>();
}
