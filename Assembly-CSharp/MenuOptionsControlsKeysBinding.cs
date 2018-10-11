using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsControlsKeysBinding : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
		this.m_DefaultText = this.m_DefaultButton.GetComponentInChildren<Text>();
		List<InputsManager.InputAction> list = new List<InputsManager.InputAction>();
		list.Add(InputsManager.InputAction.Forward);
		list.Add(InputsManager.InputAction.DodgeForward);
		this.m_ForwardButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.Backward);
		list.Add(InputsManager.InputAction.DodgeBackward);
		this.m_BackwardButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.Left);
		list.Add(InputsManager.InputAction.DodgeLeft);
		this.m_LeftButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.Right);
		list.Add(InputsManager.InputAction.DodgeRight);
		this.m_RightButton.SetInputActions(list);
		this.m_SprintButton.SetInputAction(InputsManager.InputAction.Sprint);
		this.m_DuckButton.SetInputAction(InputsManager.InputAction.Duck);
		this.m_JumpButton.SetInputAction(InputsManager.InputAction.Jump);
		this.m_DropButton.SetInputAction(InputsManager.InputAction.Drop);
		list.Clear();
		list.Add(InputsManager.InputAction.SpearThrowAim);
		list.Add(InputsManager.InputAction.ItemAim);
		this.m_AimButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.SpearThrow);
		list.Add(InputsManager.InputAction.ItemThrow);
		this.m_ThrowButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.SpearThrowReleaseAim);
		list.Add(InputsManager.InputAction.ItemCancelAim);
		list.Add(InputsManager.InputAction.BowCancelAim);
		this.m_CancelAimButton.SetInputActions(list);
		this.m_WaterDrinkButton.SetInputAction(InputsManager.InputAction.WaterDrink);
		list.Clear();
		list.Add(InputsManager.InputAction.ShowInventory);
		list.Add(InputsManager.InputAction.HideInventory);
		this.m_InventoryButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.ShowNotepad);
		list.Add(InputsManager.InputAction.HideNotepad);
		this.m_NotepadButton.SetInputActions(list);
		list.Clear();
		list.Add(InputsManager.InputAction.ShowWheel);
		list.Add(InputsManager.InputAction.HideWheel);
		this.m_WheelButton.SetInputActions(list);
		this.m_DialogButton.SetInputAction(InputsManager.InputAction.ShowSelectDialogNode);
		this.m_WeaponSlot1Button.SetInputAction(InputsManager.InputAction.QuickEquip0);
		this.m_WeaponSlot2Button.SetInputAction(InputsManager.InputAction.QuickEquip1);
		this.m_WeaponSlot3Button.SetInputAction(InputsManager.InputAction.QuickEquip2);
		this.m_WeaponSlot4Button.SetInputAction(InputsManager.InputAction.QuickEquip3);
		this.m_BlockButton.SetInputAction(InputsManager.InputAction.Block);
		this.m_WatchButton.SetInputAction(InputsManager.InputAction.Watch);
		this.m_ThrowStoneButton.SetInputAction(InputsManager.InputAction.ThrowStone);
		List<TriggerAction.TYPE> list2 = new List<TriggerAction.TYPE>();
		list2.Add(TriggerAction.TYPE.Take);
		list2.Add(TriggerAction.TYPE.Harvest);
		list2.Add(TriggerAction.TYPE.Eat);
		list2.Add(TriggerAction.TYPE.Drink);
		list2.Add(TriggerAction.TYPE.DrinkHold);
		list2.Add(TriggerAction.TYPE.Use);
		list2.Add(TriggerAction.TYPE.Sleep);
		list2.Add(TriggerAction.TYPE.Insert);
		list2.Add(TriggerAction.TYPE.Climb);
		list2.Add(TriggerAction.TYPE.PickUp);
		list2.Add(TriggerAction.TYPE.Arm);
		list2.Add(TriggerAction.TYPE.Exit);
		list2.Add(TriggerAction.TYPE.Pour);
		list2.Add(TriggerAction.TYPE.Read);
		list2.Add(TriggerAction.TYPE.Look);
		list2.Add(TriggerAction.TYPE.Open);
		list2.Add(TriggerAction.TYPE.Close);
		list2.Add(TriggerAction.TYPE.TakeHold);
		list2.Add(TriggerAction.TYPE.RemoveFromSnareTrap);
		list2.Add(TriggerAction.TYPE.Ignite);
		list2.Add(TriggerAction.TYPE.Fill);
		list2.Add(TriggerAction.TYPE.UseHold);
		list2.Add(TriggerAction.TYPE.ClimbHold);
		list2.Add(TriggerAction.TYPE.SwapHold);
		list2.Add(TriggerAction.TYPE.SaveGame);
		list2.Add(TriggerAction.TYPE.InsertToStand);
		this.m_ActionButton.SetTriggerActions(list2);
		this.m_QuitButton.SetInputAction(InputsManager.InputAction.AdditionalQuit);
		this.m_ReadButton.SetInputAction(InputsManager.InputAction.Read);
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.InitializeButtons();
	}

	private void InitializeButtons()
	{
		this.m_ForwardButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Forward));
		this.m_BackwardButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Backward));
		this.m_LeftButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Left));
		this.m_RightButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Right));
		this.m_SprintButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Sprint));
		this.m_DuckButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Duck));
		this.m_JumpButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Jump));
		this.m_DropButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Drop));
		this.m_AimButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.SpearThrowAim));
		this.m_ThrowButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.SpearThrow));
		this.m_CancelAimButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.SpearThrowReleaseAim));
		this.m_WaterDrinkButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.WaterDrink));
		this.m_InventoryButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.ShowInventory));
		this.m_NotepadButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.ShowNotepad));
		this.m_WheelButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.ShowWheel));
		this.m_DialogButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.ShowSelectDialogNode));
		this.m_WeaponSlot1Button.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.QuickEquip0));
		this.m_WeaponSlot2Button.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.QuickEquip1));
		this.m_WeaponSlot3Button.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.QuickEquip2));
		this.m_WeaponSlot4Button.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.QuickEquip3));
		this.m_BlockButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Block));
		this.m_WatchButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Watch));
		this.m_ThrowStoneButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.ThrowStone));
		this.m_QuitButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.AdditionalQuit));
		this.m_ReadButton.SetKeyCode(this.GetKeyCodeByInputAction(InputsManager.InputAction.Read));
		this.m_ActionButton.SetKeyCode(this.GetKeyCodeByTriggerAction(TriggerAction.TYPE.Take));
	}

	private KeyCode GetKeyCodeByInputAction(InputsManager.InputAction action)
	{
		Dictionary<int, InputActionData> actionsByInputAction = InputsManager.Get().GetActionsByInputAction();
		foreach (KeyValuePair<int, InputActionData> keyValuePair in actionsByInputAction)
		{
			if (keyValuePair.Key == (int)action)
			{
				Dictionary<int, InputActionData>.Enumerator enumerator;
				KeyValuePair<int, InputActionData> keyValuePair2 = enumerator.Current;
				return keyValuePair2.Value.m_KeyCode;
			}
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
		return KeyCode.Space;
	}

	private KeyCode GetKeyCodeByTriggerAction(TriggerAction.TYPE action)
	{
		Dictionary<int, InputActionData> actionsByTriggerAction = InputsManager.Get().GetActionsByTriggerAction();
		foreach (KeyValuePair<int, InputActionData> keyValuePair in actionsByTriggerAction)
		{
			if (keyValuePair.Key == (int)action)
			{
				Dictionary<int, InputActionData>.Enumerator enumerator;
				KeyValuePair<int, InputActionData> keyValuePair2 = enumerator.Current;
				return keyValuePair2.Value.m_KeyCode;
			}
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
		return KeyCode.Space;
	}

	private new void Update()
	{
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_BackButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_BackButton.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		this.m_DefaultButton.GetComponentInChildren<Text>().color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton.gameObject;
		}
		component = this.m_DefaultButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_DefaultButton.gameObject;
		}
		component = this.m_BackText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_BackButton.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton.gameObject)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		component = this.m_DefaultText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_DefaultButton.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_DefaultButton.gameObject)
		{
			color = this.m_DefaultText.color;
			color.a = 1f;
			this.m_DefaultText.color = color;
		}
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public override void OnBack()
	{
		this.m_Question = KeyBindingQuestion.Save;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), false);
	}

	public void OnDefault()
	{
		this.m_Question = KeyBindingQuestion.Default;
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), false);
	}

	public void OnYesFromDialog()
	{
		if (this.m_Question == KeyBindingQuestion.Save)
		{
			this.ApplyOptions();
			GreenHellGame.Instance.m_Settings.SaveSettings();
			this.m_MenuInGameManager.ShowScreen(typeof(MenuOptionsControls));
		}
		else
		{
			this.ApplyDefaultOptions();
		}
	}

	private void ApplyOptions()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		this.AddActionsByInputAction(this.m_ForwardButton, dictionary);
		this.AddActionsByInputAction(this.m_BackwardButton, dictionary);
		this.AddActionsByInputAction(this.m_LeftButton, dictionary);
		this.AddActionsByInputAction(this.m_RightButton, dictionary);
		this.AddActionsByInputAction(this.m_SprintButton, dictionary);
		this.AddActionsByInputAction(this.m_DuckButton, dictionary);
		this.AddActionsByInputAction(this.m_JumpButton, dictionary);
		this.AddActionsByInputAction(this.m_DropButton, dictionary);
		this.AddActionsByInputAction(this.m_AimButton, dictionary);
		this.AddActionsByInputAction(this.m_ThrowButton, dictionary);
		this.AddActionsByInputAction(this.m_CancelAimButton, dictionary);
		this.AddActionsByInputAction(this.m_WaterDrinkButton, dictionary);
		this.AddActionsByInputAction(this.m_InventoryButton, dictionary);
		this.AddActionsByInputAction(this.m_NotepadButton, dictionary);
		this.AddActionsByInputAction(this.m_WheelButton, dictionary);
		this.AddActionsByInputAction(this.m_DialogButton, dictionary);
		this.AddActionsByInputAction(this.m_WeaponSlot1Button, dictionary);
		this.AddActionsByInputAction(this.m_WeaponSlot2Button, dictionary);
		this.AddActionsByInputAction(this.m_WeaponSlot3Button, dictionary);
		this.AddActionsByInputAction(this.m_WeaponSlot4Button, dictionary);
		this.AddActionsByInputAction(this.m_BlockButton, dictionary);
		this.AddActionsByInputAction(this.m_WatchButton, dictionary);
		this.AddActionsByInputAction(this.m_ThrowStoneButton, dictionary);
		this.AddActionsByTriggerAction(this.m_ActionButton, dictionary2);
		this.AddActionsByInputAction(this.m_QuitButton, dictionary);
		this.AddActionsByInputAction(this.m_ReadButton, dictionary);
		InputsManager.Get().ApplyOptions(dictionary, dictionary2);
	}

	private void ApplyDefaultOptions()
	{
		InputsManager.Get().ApplyDefaultOptions();
		this.InitializeButtons();
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

	public Button m_BackButton;

	public Text m_BackText;

	public Button m_DefaultButton;

	public Text m_DefaultText;

	private GameObject m_ActiveButton;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	public UIKeyBindButton m_ForwardButton;

	public UIKeyBindButton m_BackwardButton;

	public UIKeyBindButton m_LeftButton;

	public UIKeyBindButton m_RightButton;

	public UIKeyBindButton m_SprintButton;

	public UIKeyBindButton m_DuckButton;

	public UIKeyBindButton m_JumpButton;

	public UIKeyBindButton m_DropButton;

	public UIKeyBindButton m_AimButton;

	public UIKeyBindButton m_ThrowButton;

	public UIKeyBindButton m_CancelAimButton;

	public UIKeyBindButton m_WaterDrinkButton;

	public UIKeyBindButton m_InventoryButton;

	public UIKeyBindButton m_NotepadButton;

	public UIKeyBindButton m_WheelButton;

	public UIKeyBindButton m_DialogButton;

	public UIKeyBindButton m_WeaponSlot1Button;

	public UIKeyBindButton m_WeaponSlot2Button;

	public UIKeyBindButton m_WeaponSlot3Button;

	public UIKeyBindButton m_WeaponSlot4Button;

	public UIKeyBindButton m_BlockButton;

	public UIKeyBindButton m_WatchButton;

	public UIKeyBindButton m_ThrowStoneButton;

	public UIKeyBindButton m_ActionButton;

	public UIKeyBindButton m_QuitButton;

	public UIKeyBindButton m_ReadButton;

	private KeyBindingQuestion m_Question;
}
