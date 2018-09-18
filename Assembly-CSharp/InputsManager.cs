using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse(this.m_InputsScript, true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
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
		KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), svalue);
		inputActionData.m_KeyCode = keyCode;
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
		this.m_UnregisterReceiversRequests.Add(receiver);
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
		foreach (KeyValuePair<int, List<InputActionData>> keyValuePair in this.m_ActionsByKeyCode)
		{
			int key = keyValuePair.Key;
			if (Input.GetKeyDown((KeyCode)key))
			{
				foreach (InputActionData inputActionData in this.m_ActionsByKeyCode[key])
				{
					if (inputActionData.m_Type == InputsManager.InputActionType.Down && inputActionData.m_Hold == 0f)
					{
						this.OnAction(inputActionData);
						if (inputActionData.m_TriggerAction != TriggerAction.TYPE.None)
						{
							if (!this.m_ActiveActionsByTriggerAction.ContainsKey((int)inputActionData.m_TriggerAction))
							{
								this.m_ActiveActionsByTriggerAction.Add((int)inputActionData.m_TriggerAction, (KeyCode)key);
							}
						}
						else if (!this.m_ActiveActionsByInputAction.ContainsKey((int)inputActionData.m_Action))
						{
							this.m_ActiveActionsByInputAction.Add((int)inputActionData.m_Action, (KeyCode)key);
						}
					}
					else
					{
						inputActionData.m_PressTime = Time.time;
						if (!this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData.m_TriggerAction))
						{
							this.m_ActiveHoldActionsByTriggerAction.Add((int)inputActionData.m_TriggerAction, inputActionData);
						}
					}
				}
			}
			else if (Input.GetKeyUp((KeyCode)key))
			{
				foreach (InputActionData inputActionData2 in this.m_ActionsByKeyCode[key])
				{
					if (inputActionData2.m_Type == InputsManager.InputActionType.Up && inputActionData2.m_Hold == 0f)
					{
						this.OnAction(inputActionData2);
					}
					else if (inputActionData2.m_Type == InputsManager.InputActionType.Up && inputActionData2.m_Hold > 0f && inputActionData2.m_PressTime > 0f && Time.time - inputActionData2.m_PressTime >= inputActionData2.m_Hold)
					{
						this.OnAction(inputActionData2);
					}
					inputActionData2.m_PressTime = 0f;
					if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData2.m_TriggerAction))
					{
						this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData2.m_TriggerAction);
					}
				}
			}
			foreach (InputActionData inputActionData3 in this.m_ActionsByKeyCode[key])
			{
				if (inputActionData3.m_Type == InputsManager.InputActionType.Down && inputActionData3.m_PressTime > 0f && Time.time - inputActionData3.m_PressTime >= inputActionData3.m_Hold)
				{
					this.OnAction(inputActionData3);
					if (inputActionData3.m_TriggerAction != TriggerAction.TYPE.None)
					{
						this.m_ActiveActionsByTriggerAction.Remove((int)inputActionData3.m_TriggerAction);
					}
					else
					{
						this.m_ActiveActionsByInputAction.Remove((int)inputActionData3.m_Action);
					}
					inputActionData3.m_PressTime = 0f;
					if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData3.m_TriggerAction))
					{
						this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData3.m_TriggerAction);
					}
				}
			}
		}
	}

	private void UpdatActiveActions()
	{
		if (this.m_ActiveActionsByInputAction.Count > 0)
		{
			this.m_InputActionsToRemove.Clear();
			foreach (KeyValuePair<int, KeyCode> keyValuePair in this.m_ActiveActionsByInputAction)
			{
				InputsManager.InputAction key = (InputsManager.InputAction)keyValuePair.Key;
				if (!Input.GetKey(this.m_ActiveActionsByInputAction[(int)key]))
				{
					this.m_InputActionsToRemove.Add(key);
				}
			}
			foreach (InputsManager.InputAction key2 in this.m_InputActionsToRemove)
			{
				this.m_ActiveActionsByInputAction.Remove((int)key2);
			}
		}
		if (this.m_ActiveActionsByTriggerAction.Count > 0)
		{
			this.m_TriggerActionsToRemove.Clear();
			foreach (KeyValuePair<int, KeyCode> keyValuePair2 in this.m_ActiveActionsByTriggerAction)
			{
				int key3 = keyValuePair2.Key;
				if (!Input.GetKey(this.m_ActiveActionsByTriggerAction[key3]))
				{
					this.m_TriggerActionsToRemove.Add((TriggerAction.TYPE)key3);
				}
			}
			foreach (TriggerAction.TYPE key4 in this.m_TriggerActionsToRemove)
			{
				this.m_ActiveActionsByTriggerAction.Remove((int)key4);
			}
		}
	}

	private void OnAction(InputActionData action)
	{
		if (action.m_TriggerAction != TriggerAction.TYPE.None)
		{
			if (TriggerController.Get() != null)
			{
				TriggerController.Get().OnTriggerAction(action.m_TriggerAction);
			}
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

	public Dictionary<int, List<InputActionData>> GetActionsByKeyCode()
	{
		return this.m_ActionsByKeyCode;
	}

	public Dictionary<int, InputActionData> GetActionsByInputAction()
	{
		return this.m_ActionsByInputAction;
	}

	public Dictionary<int, InputActionData> GetActionsByTriggerAction()
	{
		return this.m_ActionsByTriggerAction;
	}

	public void ApplyOptions(Dictionary<int, int> actions_by_input_action, Dictionary<int, int> actions_by_trigger_action)
	{
		for (int i = 0; i < actions_by_input_action.Keys.Count; i++)
		{
			for (int j = 0; j < this.m_ActionsByInputAction.Keys.Count; j++)
			{
				if (actions_by_input_action.Keys.ElementAt(i) == this.m_ActionsByInputAction.Keys.ElementAt(j))
				{
					this.m_ActionsByInputAction[this.m_ActionsByInputAction.Keys.ElementAt(j)].m_KeyCode = (KeyCode)actions_by_input_action.Values.ElementAt(i);
					break;
				}
			}
		}
		for (int k = 0; k < actions_by_trigger_action.Keys.Count; k++)
		{
			for (int l = 0; l < this.m_ActionsByTriggerAction.Keys.Count; l++)
			{
				if (actions_by_trigger_action.Keys.ElementAt(k) == this.m_ActionsByTriggerAction.Keys.ElementAt(l))
				{
					this.m_ActionsByTriggerAction[this.m_ActionsByTriggerAction.Keys.ElementAt(l)].m_KeyCode = (KeyCode)actions_by_trigger_action.Values.ElementAt(k);
					break;
				}
			}
		}
		this.m_ActionsByKeyCode.Clear();
		for (int m = 0; m < this.m_ActionsByInputAction.Keys.Count; m++)
		{
			KeyCode keyCode = this.m_ActionsByInputAction.Values.ElementAt(m).m_KeyCode;
			if (this.m_ActionsByKeyCode.ContainsKey((int)keyCode))
			{
				this.m_ActionsByKeyCode[(int)keyCode].Add(new InputActionData(this.m_ActionsByInputAction.Values.ElementAt(m)));
			}
			else
			{
				List<InputActionData> list = new List<InputActionData>();
				list.Add(new InputActionData(this.m_ActionsByInputAction.Values.ElementAt(m)));
				this.m_ActionsByKeyCode.Add((int)keyCode, list);
			}
		}
		for (int n = 0; n < this.m_ActionsByTriggerAction.Keys.Count; n++)
		{
			KeyCode keyCode2 = this.m_ActionsByTriggerAction.Values.ElementAt(n).m_KeyCode;
			if (this.m_ActionsByKeyCode.ContainsKey((int)keyCode2))
			{
				this.m_ActionsByKeyCode[(int)keyCode2].Add(new InputActionData(this.m_ActionsByTriggerAction.Values.ElementAt(n)));
			}
			else
			{
				List<InputActionData> list2 = new List<InputActionData>();
				list2.Add(new InputActionData(this.m_ActionsByTriggerAction.Values.ElementAt(n)));
				this.m_ActionsByKeyCode.Add((int)keyCode2, list2);
			}
		}
		this.m_ActiveActionsByInputAction.Clear();
		this.m_ActiveActionsByTriggerAction.Clear();
		this.m_ActiveHoldActionsByTriggerAction.Clear();
		this.m_InputActionsToRemove.Clear();
		this.m_TriggerActionsToRemove.Clear();
		GC.Collect();
	}

	public void SaveSettings(BinaryFormatter bf, FileStream file)
	{
		bf.Serialize(file, this.m_ActionsByInputAction.Keys.Count);
		for (int i = 0; i < this.m_ActionsByInputAction.Keys.Count; i++)
		{
			int num = this.m_ActionsByInputAction.Keys.ElementAt(i);
			InputsManager.InputAction inputAction = (InputsManager.InputAction)num;
			string graph = inputAction.ToString();
			bf.Serialize(file, graph);
			KeyCode keyCode = this.m_ActionsByInputAction.Values.ElementAt(i).m_KeyCode;
			string graph2 = keyCode.ToString();
			bf.Serialize(file, graph2);
		}
		bf.Serialize(file, this.m_ActionsByTriggerAction.Keys.Count);
		for (int j = 0; j < this.m_ActionsByTriggerAction.Keys.Count; j++)
		{
			int num2 = this.m_ActionsByTriggerAction.Keys.ElementAt(j);
			TriggerAction.TYPE type = (TriggerAction.TYPE)num2;
			string graph3 = type.ToString();
			bf.Serialize(file, graph3);
			KeyCode keyCode2 = this.m_ActionsByTriggerAction.Values.ElementAt(j).m_KeyCode;
			string graph4 = keyCode2.ToString();
			bf.Serialize(file, graph4);
		}
	}

	public void LoadSettings(BinaryFormatter bf, FileStream file)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = (int)bf.Deserialize(file);
		for (int i = 0; i < num; i++)
		{
			string value = (string)bf.Deserialize(file);
			InputsManager.InputAction key = (InputsManager.InputAction)Enum.Parse(typeof(InputsManager.InputAction), value);
			string value2 = (string)bf.Deserialize(file);
			KeyCode value3 = (KeyCode)Enum.Parse(typeof(KeyCode), value2);
			InputActionData inputActionData = null;
			if (this.m_ActionsByInputAction.TryGetValue((int)key, out inputActionData))
			{
				dictionary.Add((int)key, (int)value3);
			}
		}
		int num2 = (int)bf.Deserialize(file);
		for (int j = 0; j < num2; j++)
		{
			string value4 = (string)bf.Deserialize(file);
			TriggerAction.TYPE key2 = (TriggerAction.TYPE)Enum.Parse(typeof(TriggerAction.TYPE), value4);
			string value5 = (string)bf.Deserialize(file);
			KeyCode value6 = (KeyCode)Enum.Parse(typeof(KeyCode), value5);
			InputActionData inputActionData2 = null;
			if (this.m_ActionsByTriggerAction.TryGetValue((int)key2, out inputActionData2))
			{
				dictionary2.Add((int)key2, (int)value6);
			}
		}
		this.ApplyOptions(dictionary, dictionary2);
	}

	public void ApplyDefaultOptions()
	{
		this.m_ActionsByInputAction.Clear();
		this.m_ActionsByKeyCode.Clear();
		this.m_ActionsByTriggerAction.Clear();
		this.m_ActiveActionsByInputAction.Clear();
		this.m_ActiveActionsByTriggerAction.Clear();
		this.m_ActiveHoldActionsByTriggerAction.Clear();
		this.m_InputActionsToRemove.Clear();
		this.m_TriggerActionsToRemove.Clear();
		this.LoadScript();
	}

	private string m_InputsScript = "Inputs.txt";

	private List<IInputsReceiver> m_Receivers = new List<IInputsReceiver>();

	private List<IInputsReceiver> m_RegisterReceiverRequests = new List<IInputsReceiver>(20);

	private List<IInputsReceiver> m_UnregisterReceiversRequests = new List<IInputsReceiver>();

	private Dictionary<int, List<InputActionData>> m_ActionsByKeyCode = new Dictionary<int, List<InputActionData>>();

	private Dictionary<int, InputActionData> m_ActionsByInputAction = new Dictionary<int, InputActionData>();

	private Dictionary<int, KeyCode> m_ActiveActionsByInputAction = new Dictionary<int, KeyCode>();

	private Dictionary<int, InputActionData> m_ActionsByTriggerAction = new Dictionary<int, InputActionData>();

	private Dictionary<int, KeyCode> m_ActiveActionsByTriggerAction = new Dictionary<int, KeyCode>();

	private Dictionary<int, InputActionData> m_ActiveHoldActionsByTriggerAction = new Dictionary<int, InputActionData>();

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
		WaterDrink,
		FishingAim,
		FishingCast,
		FishingStrike,
		FishingReel,
		FishingTakeFish,
		CreateConstruction,
		ConstructionRotate,
		ShowWheel,
		HideWheel,
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
		Forward,
		Backward,
		Left,
		Right,
		AdditionalQuit,
		Read,
		Count
	}

	public enum InputActionType
	{
		Down,
		Up
	}
}
