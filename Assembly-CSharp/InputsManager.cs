using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputsManager : MonoBehaviour
{
	public static InputsManager Get()
	{
		return InputsManager.s_Instance;
	}

	private void Awake()
	{
		InputsManager.s_Instance = this;
		this.LoadScript();
		if (this.m_ActionsByKeyCode.Count == 0)
		{
			Debug.LogWarning("[InputsManager:Awake] Input script is empty!");
			UnityEngine.Object.Destroy(this);
		}
	}

	private void LoadScript()
	{
		TextAssetParser textAssetParser = new TextAssetParser(this.m_InputsScript);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Action")
			{
				this.LoadAction(key, false);
			}
			else if (key.GetName() == "TriggerAction")
			{
				this.LoadAction(key, true);
			}
		}
	}

	private void LoadAction(Key key, bool trigger)
	{
		InputActionData inputActionData = new InputActionData();
		if (trigger)
		{
			inputActionData.m_TriggerAction = (TriggerAction.TYPE)Enum.Parse(typeof(TriggerAction.TYPE), key.GetVariable(0).SValue);
		}
		else
		{
			inputActionData.m_Action = (InputsManager.InputAction)Enum.Parse(typeof(InputsManager.InputAction), key.GetVariable(0).SValue);
		}
		string svalue = key.GetVariable(1).SValue;
		inputActionData.m_Type = (InputsManager.InputActionType)Enum.Parse(typeof(InputsManager.InputActionType), key.GetVariable(2).SValue);
		inputActionData.m_Hold = key.GetVariable(3).FValue;
		if (key.GetVariablesCount() > 4)
		{
			inputActionData.m_CrossPlatformInput = key.GetVariable(4).SValue;
		}
		string[] array = svalue.Split(new char[]
		{
			';'
		});
		for (int i = 0; i < array.Length; i++)
		{
			KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), array[i]);
			inputActionData.m_KeyCodes.Add(keyCode);
			if (this.m_ActionsByKeyCode.ContainsKey((int)keyCode))
			{
				this.m_ActionsByKeyCode[(int)keyCode].Add(inputActionData);
			}
			else
			{
				List<InputActionData> list = new List<InputActionData>();
				list.Add(inputActionData);
				this.m_ActionsByKeyCode.Add((int)keyCode, list);
			}
		}
		if (inputActionData.m_CrossPlatformInput != string.Empty)
		{
			if (this.m_ActionsByCrossPlatformInput.ContainsKey(inputActionData.m_CrossPlatformInput))
			{
				this.m_ActionsByCrossPlatformInput[inputActionData.m_CrossPlatformInput].Add(inputActionData);
			}
			else
			{
				List<InputActionData> list2 = new List<InputActionData>();
				list2.Add(inputActionData);
				this.m_ActionsByCrossPlatformInput.Add(inputActionData.m_CrossPlatformInput, list2);
			}
		}
		if (trigger)
		{
			if (this.m_ActionsByTriggerAction.ContainsKey((int)inputActionData.m_TriggerAction))
			{
				DebugUtils.Assert("[InputsManager:LoadScript] Error! Duplicated trigger action names - " + inputActionData.m_TriggerAction.ToString(), true, DebugUtils.AssertType.Info);
			}
			else
			{
				this.m_ActionsByTriggerAction.Add((int)inputActionData.m_TriggerAction, inputActionData);
			}
		}
		else if (this.m_ActionsByInputAction.ContainsKey((int)inputActionData.m_Action))
		{
			DebugUtils.Assert("[InputsManager:LoadScript] Error! Duplicated input action names - " + inputActionData.m_Action.ToString(), true, DebugUtils.AssertType.Info);
		}
		else
		{
			this.m_ActionsByInputAction.Add((int)inputActionData.m_Action, inputActionData);
		}
	}

	public void RegisterReceiver(IInputsReceiver receiver)
	{
		this.m_RegisterReceiverRequests.Add(receiver);
	}

	public void UnregisterReceiver(IInputsReceiver receiver)
	{
		this.m_UnregisterReceiversRequests.Remove(receiver);
	}

	private void LateUpdate()
	{
		this.UpdateReceiverRequests();
		if (this.m_Receivers.Count == 0)
		{
			return;
		}
		this.UpdateActions();
		this.UpdatActiveActions();
	}

	private void UpdateReceiverRequests()
	{
		if (this.m_RegisterReceiverRequests.Count > 0)
		{
			foreach (IInputsReceiver item in this.m_RegisterReceiverRequests)
			{
				this.m_Receivers.Add(item);
			}
			this.m_RegisterReceiverRequests.Clear();
		}
		if (this.m_UnregisterReceiversRequests.Count > 0)
		{
			foreach (IInputsReceiver item2 in this.m_UnregisterReceiversRequests)
			{
				this.m_Receivers.Remove(item2);
			}
			this.m_UnregisterReceiversRequests.Clear();
		}
	}

	private void UpdateActions()
	{
		foreach (KeyValuePair<string, List<InputActionData>> keyValuePair in this.m_ActionsByCrossPlatformInput)
		{
			string key = keyValuePair.Key;
			float axis = CrossPlatformInputManager.GetAxis(key);
			if (axis >= InputsManager.MIN_AXIS_VAL)
			{
				foreach (InputActionData inputActionData in this.m_ActionsByCrossPlatformInput[key])
				{
					if (inputActionData.m_Type == InputsManager.InputActionType.Down && inputActionData.m_Hold == 0f)
					{
						this.OnAction(inputActionData);
						if (!this.m_ActiveActionsByInputActionCPI.ContainsKey((int)inputActionData.m_Action))
						{
							this.m_ActiveActionsByInputActionCPI.Add((int)inputActionData.m_Action, key);
						}
					}
				}
			}
		}
		foreach (KeyValuePair<int, List<InputActionData>> keyValuePair2 in this.m_ActionsByKeyCode)
		{
			int key2 = keyValuePair2.Key;
			if (Input.GetKeyDown((KeyCode)key2))
			{
				foreach (InputActionData inputActionData2 in this.m_ActionsByKeyCode[key2])
				{
					if (inputActionData2.m_Type == InputsManager.InputActionType.Down && inputActionData2.m_Hold == 0f)
					{
						this.OnAction(inputActionData2);
						if (inputActionData2.m_TriggerAction != TriggerAction.TYPE.None)
						{
							if (!this.m_ActiveActionsByTriggerAction.ContainsKey((int)inputActionData2.m_TriggerAction))
							{
								this.m_ActiveActionsByTriggerAction.Add((int)inputActionData2.m_TriggerAction, (KeyCode)key2);
							}
						}
						else if (!this.m_ActiveActionsByInputAction.ContainsKey((int)inputActionData2.m_Action))
						{
							this.m_ActiveActionsByInputAction.Add((int)inputActionData2.m_Action, (KeyCode)key2);
						}
					}
					else
					{
						inputActionData2.m_PressTime = Time.time;
						if (!this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData2.m_TriggerAction))
						{
							this.m_ActiveHoldActionsByTriggerAction.Add((int)inputActionData2.m_TriggerAction, inputActionData2);
						}
					}
				}
			}
			else if (Input.GetKeyUp((KeyCode)key2))
			{
				foreach (InputActionData inputActionData3 in this.m_ActionsByKeyCode[key2])
				{
					if (inputActionData3.m_Type == InputsManager.InputActionType.Up && inputActionData3.m_Hold == 0f)
					{
						this.OnAction(inputActionData3);
					}
					else if (inputActionData3.m_Type == InputsManager.InputActionType.Up && inputActionData3.m_Hold > 0f && inputActionData3.m_PressTime > 0f && Time.time - inputActionData3.m_PressTime >= inputActionData3.m_Hold)
					{
						this.OnAction(inputActionData3);
					}
					inputActionData3.m_PressTime = 0f;
					if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData3.m_TriggerAction))
					{
						this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData3.m_TriggerAction);
					}
				}
			}
			foreach (InputActionData inputActionData4 in this.m_ActionsByKeyCode[key2])
			{
				if (inputActionData4.m_Type == InputsManager.InputActionType.Down && inputActionData4.m_PressTime > 0f && Time.time - inputActionData4.m_PressTime >= inputActionData4.m_Hold)
				{
					this.OnAction(inputActionData4);
					if (inputActionData4.m_TriggerAction != TriggerAction.TYPE.None)
					{
						this.m_ActiveActionsByTriggerAction.Remove((int)inputActionData4.m_TriggerAction);
					}
					else
					{
						this.m_ActiveActionsByInputAction.Remove((int)inputActionData4.m_Action);
					}
					inputActionData4.m_PressTime = 0f;
					if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData4.m_TriggerAction))
					{
						this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData4.m_TriggerAction);
					}
				}
			}
		}
	}

	private void UpdatActiveActions()
	{
		if (this.m_ActiveActionsByInputActionCPI.Count > 0)
		{
			this.m_InputActionsToRemove.Clear();
			foreach (KeyValuePair<int, string> keyValuePair in this.m_ActiveActionsByInputActionCPI)
			{
				int key = keyValuePair.Key;
				float axis = CrossPlatformInputManager.GetAxis(this.m_ActiveActionsByInputActionCPI[key]);
				if (axis < InputsManager.MIN_AXIS_VAL)
				{
					this.m_InputActionsToRemove.Add((InputsManager.InputAction)key);
				}
			}
			foreach (InputsManager.InputAction key2 in this.m_InputActionsToRemove)
			{
				this.m_ActiveActionsByInputActionCPI.Remove((int)key2);
			}
		}
		if (this.m_ActiveActionsByInputAction.Count > 0)
		{
			this.m_InputActionsToRemove.Clear();
			foreach (KeyValuePair<int, KeyCode> keyValuePair2 in this.m_ActiveActionsByInputAction)
			{
				InputsManager.InputAction key3 = (InputsManager.InputAction)keyValuePair2.Key;
				if (!Input.GetKey(this.m_ActiveActionsByInputAction[(int)key3]))
				{
					this.m_InputActionsToRemove.Add(key3);
				}
			}
			foreach (InputsManager.InputAction key4 in this.m_InputActionsToRemove)
			{
				this.m_ActiveActionsByInputAction.Remove((int)key4);
			}
		}
		if (this.m_ActiveActionsByTriggerAction.Count > 0)
		{
			this.m_TriggerActionsToRemove.Clear();
			foreach (KeyValuePair<int, KeyCode> keyValuePair3 in this.m_ActiveActionsByTriggerAction)
			{
				int key5 = keyValuePair3.Key;
				if (!Input.GetKey(this.m_ActiveActionsByTriggerAction[key5]))
				{
					this.m_TriggerActionsToRemove.Add((TriggerAction.TYPE)key5);
				}
			}
			foreach (TriggerAction.TYPE key6 in this.m_TriggerActionsToRemove)
			{
				this.m_ActiveActionsByTriggerAction.Remove((int)key6);
			}
		}
	}

	private void OnAction(InputActionData action)
	{
		if (action.m_TriggerAction != TriggerAction.TYPE.None)
		{
			TriggerController.Get().OnTriggerAction(action.m_TriggerAction);
		}
		else
		{
			foreach (IInputsReceiver inputsReceiver in this.m_Receivers)
			{
				if (inputsReceiver.CanReceiveAction())
				{
					inputsReceiver.OnInputAction(action.m_Action);
				}
			}
		}
	}

	public bool IsActionActive(InputsManager.InputAction action)
	{
		foreach (KeyValuePair<int, KeyCode> keyValuePair in this.m_ActiveActionsByInputAction)
		{
			InputsManager.InputAction key = (InputsManager.InputAction)keyValuePair.Key;
			if (key == action)
			{
				return true;
			}
		}
		foreach (KeyValuePair<int, string> keyValuePair2 in this.m_ActiveActionsByInputActionCPI)
		{
			InputsManager.InputAction key2 = (InputsManager.InputAction)keyValuePair2.Key;
			if (key2 == action)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsActionActive(TriggerAction.TYPE action)
	{
		foreach (KeyValuePair<int, KeyCode> keyValuePair in this.m_ActiveActionsByTriggerAction)
		{
			TriggerAction.TYPE key = (TriggerAction.TYPE)keyValuePair.Key;
			if (key == action)
			{
				return true;
			}
		}
		return false;
	}

	public InputActionData GetInputActionData(InputsManager.InputAction action)
	{
		InputActionData result = null;
		this.m_ActionsByInputAction.TryGetValue((int)action, out result);
		return result;
	}

	public InputActionData GetInputActionData(TriggerAction.TYPE action)
	{
		InputActionData result = null;
		this.m_ActionsByTriggerAction.TryGetValue((int)action, out result);
		return result;
	}

	public float GetActionHoldProgress(TriggerAction.TYPE action)
	{
		if (!this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)action))
		{
			return 0f;
		}
		InputActionData inputActionData = null;
		this.m_ActiveHoldActionsByTriggerAction.TryGetValue((int)action, out inputActionData);
		if (inputActionData.m_Hold == 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01((Time.time - inputActionData.m_PressTime) / inputActionData.m_Hold);
	}

	public TextAsset m_InputsScript;

	private List<IInputsReceiver> m_Receivers = new List<IInputsReceiver>();

	private List<IInputsReceiver> m_RegisterReceiverRequests = new List<IInputsReceiver>(20);

	private List<IInputsReceiver> m_UnregisterReceiversRequests = new List<IInputsReceiver>();

	private Dictionary<int, List<InputActionData>> m_ActionsByKeyCode = new Dictionary<int, List<InputActionData>>();

	private Dictionary<int, InputActionData> m_ActionsByInputAction = new Dictionary<int, InputActionData>();

	private Dictionary<int, KeyCode> m_ActiveActionsByInputAction = new Dictionary<int, KeyCode>();

	private Dictionary<int, InputActionData> m_ActionsByTriggerAction = new Dictionary<int, InputActionData>();

	private Dictionary<int, KeyCode> m_ActiveActionsByTriggerAction = new Dictionary<int, KeyCode>();

	private Dictionary<int, InputActionData> m_ActiveHoldActionsByTriggerAction = new Dictionary<int, InputActionData>();

	private Dictionary<string, List<InputActionData>> m_ActionsByCrossPlatformInput = new Dictionary<string, List<InputActionData>>();

	private Dictionary<int, string> m_ActiveActionsByInputActionCPI = new Dictionary<int, string>();

	private static float MIN_AXIS_VAL = 0.5f;

	private static InputsManager s_Instance;

	private List<InputsManager.InputAction> m_InputActionsToRemove = new List<InputsManager.InputAction>(20);

	private List<TriggerAction.TYPE> m_TriggerActionsToRemove = new List<TriggerAction.TYPE>(20);

	public enum InputAction
	{
		None = -1,
		Drop,
		MeleeAttack,
		ItemSwing,
		ItemAim,
		ItemCancelAim,
		ItemThrow,
		SpearAttack,
		SpearUpAim,
		SpearAttackUp,
		SpearThrowAim,
		SpearThrowReleaseAim,
		SpearThrow,
		BlowpipeAim,
		BlowpipeShot,
		BowAim,
		BowCancelAim,
		BowShot,
		BowlSpil,
		BowlDrink,
		WaterSpill,
		WaterDrink,
		FishingAim,
		FishingCast,
		FishingStrike,
		FishingReel,
		FishingTakeFish,
		StopFireMinigame,
		CreateConstruction,
		CancelConstruction,
		ConstructionRotate,
		ShowWheel,
		HideWheel,
		Crafting,
		ShowInventory,
		HideInventory,
		HideInventoryHold,
		ShowNotepad,
		HideNotepad,
		ShowMap,
		HideMap,
		LMB,
		RMB,
		MMB,
		BISelectItem,
		BIRotateLimb,
		BISelectLimb,
		SkipDialogNode,
		ShowSelectDialogNode,
		QuickEquip0,
		QuickEquip1,
		QuickEquip2,
		QuickEquip3,
		ThrowStone,
		SkipCutscene,
		QuitBodyInspection,
		SkipMovie,
		FistFight,
		FistFightHard,
		Block,
		Jump,
		DodgeLeft,
		DodgeRight,
		DodgeForward,
		DodgeBackward,
		Duck,
		Sprint,
		Quit,
		Watch,
		Count
	}

	public enum InputActionType
	{
		Down,
		Up
	}
}
