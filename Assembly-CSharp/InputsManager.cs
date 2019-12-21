using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
		InputHelpers.InitPadIcons();
		this.LoadScript();
		if (this.m_ActionsByKeyCode.Count == 0)
		{
			Debug.LogWarning("[InputsManager:Awake] Input script is empty!");
			UnityEngine.Object.Destroy(this);
		}
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("RightStickXPs4"));
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("RightStickYPs4"));
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("DPadXPs4"));
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("DPadYPs4"));
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("LeftTriggerPs4"));
		CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis("RightTriggerPs4"));
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
				this.LoadAction(key, false, true);
			}
			else if (key.GetName() == "TriggerAction")
			{
				this.LoadAction(key, true, true);
			}
			else if (key.GetName() == "AxisAnalog")
			{
				this.LoadAxis(key, true);
			}
			else if (key.GetName() == "AxisDigital")
			{
				this.LoadAxis(key, false);
			}
		}
	}

	private void LoadAction(Key key, bool trigger, bool show_asserts = true)
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
		InputHelpers.PadButton pad_button;
		KeyCode keyCode;
		if (Enum.TryParse<InputHelpers.PadButton>(svalue, out pad_button))
		{
			keyCode = pad_button.KeyFromPad();
			inputActionData.m_Ps4KeyCode = pad_button.KeyFromPad(InputsManager.PadControllerType.Ps4);
			if (this.m_Ps4ActionsByKeyCode.ContainsKey((int)inputActionData.m_Ps4KeyCode))
			{
				this.m_Ps4ActionsByKeyCode[(int)inputActionData.m_Ps4KeyCode].Add(inputActionData);
			}
			else
			{
				List<InputActionData> list = new List<InputActionData>();
				list.Add(inputActionData);
				this.m_Ps4ActionsByKeyCode.Add((int)inputActionData.m_Ps4KeyCode, list);
			}
		}
		else
		{
			keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), svalue);
		}
		inputActionData.m_KeyCode = keyCode;
		inputActionData.m_ControllerType = keyCode.GetControllerType();
		if (inputActionData.m_ControllerType == ControllerType.Pad)
		{
			inputActionData.m_PadIcon = InputHelpers.GetPadIcon(inputActionData);
		}
		if (this.m_ActionsByKeyCode.ContainsKey((int)keyCode))
		{
			this.m_ActionsByKeyCode[(int)keyCode].Add(inputActionData);
		}
		else
		{
			List<InputActionData> list2 = new List<InputActionData>();
			list2.Add(inputActionData);
			this.m_ActionsByKeyCode.Add((int)keyCode, list2);
		}
		int actionId = inputActionData.m_Action.GetActionId(inputActionData.m_TriggerAction);
		InputActionData[] array;
		if (!this.m_ActionsByInputAction.TryGetValue(actionId, out array))
		{
			array = new InputActionData[2];
			this.m_ActionsByInputAction.Add(actionId, array);
		}
		array[(int)keyCode.GetControllerType()] = inputActionData;
	}

	private void LoadAxis(Key key, bool analog)
	{
		InputActionData inputActionData = new InputActionData();
		ControllerType controllerType = (ControllerType)Enum.Parse(typeof(ControllerType), key.GetVariable(2).SValue);
		inputActionData.m_Action = (InputsManager.InputAction)Enum.Parse(typeof(InputsManager.InputAction), key.GetVariable(0).SValue);
		inputActionData.m_AxisName = key.GetVariable(1).SValue;
		inputActionData.m_Inverted = key.GetVariable(3).BValue;
		inputActionData.m_Analog = analog;
		inputActionData.m_ControllerType = controllerType;
		if (inputActionData.m_ControllerType == ControllerType.Pad)
		{
			inputActionData.m_PadIcon = InputHelpers.GetPadIcon(inputActionData);
		}
		if (!analog)
		{
			inputActionData.m_Histeresis.x = ((key.GetVariable(4) != null) ? Mathf.Min(0.99f, key.GetVariable(4).FValue) : 0.5f);
			inputActionData.m_Histeresis.y = ((key.GetVariable(5) != null) ? Mathf.Min(0.99f, key.GetVariable(5).FValue) : 0.5f);
			inputActionData.m_Type = ((key.GetVariable(6) != null) ? ((InputsManager.InputActionType)Enum.Parse(typeof(InputsManager.InputActionType), key.GetVariable(6).SValue)) : InputsManager.InputActionType.Down);
			inputActionData.m_Hold = ((key.GetVariable(7) != null) ? key.GetVariable(7).FValue : 0f);
		}
		InputActionData[] array;
		if (!this.m_ActionsByInputAction.TryGetValue((int)inputActionData.m_Action, out array))
		{
			array = new InputActionData[2];
			this.m_ActionsByInputAction.Add((int)inputActionData.m_Action, array);
		}
		array[(int)controllerType] = inputActionData;
		List<InputActionData> list;
		if (!this.m_ActionsByAxis.TryGetValue(inputActionData.m_AxisName, out list))
		{
			list = new List<InputActionData>();
			this.m_ActionsByAxis.Add(inputActionData.m_AxisName, list);
		}
		list.Add(inputActionData);
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

	private bool CanActivateHold(InputActionData action)
	{
		return action.m_Action == InputsManager.InputAction.SpearUpAim || action.m_Action == InputsManager.InputAction.SpearThrowAim || action.m_Action == InputsManager.InputAction.ItemAim || action.m_Hold > 0f || (TriggerController.Get() != null && ((TriggerController.Get().GetBestTrigger() != null && TriggerController.Get().IsValidBestTriggerAction(action.m_TriggerAction)) || (TriggerController.Get().GetAdditionalTrigger() != null && TriggerController.Get().IsValidAdditionalTriggerAction(action.m_TriggerAction))));
	}

	private Dictionary<int, List<InputActionData>> GetActionsByKeyCode()
	{
		if (!GreenHellGame.IsPadControllerActive() || this.m_PadControllerType != InputsManager.PadControllerType.Ps4)
		{
			return this.m_ActionsByKeyCode;
		}
		return this.m_Ps4ActionsByKeyCode;
	}

	private void UpdateActions()
	{
		bool pause = Time.timeScale == 0f;
		foreach (KeyValuePair<int, List<InputActionData>> keyValuePair in this.GetActionsByKeyCode())
		{
			int key = keyValuePair.Key;
			if (Input.GetKeyDown((KeyCode)key))
			{
				foreach (InputActionData inputActionData in this.GetActionsByKeyCode()[key])
				{
					if (inputActionData.m_ControllerType == GreenHellGame.Instance.m_Settings.m_ControllerType)
					{
						if (inputActionData.m_Type == InputsManager.InputActionType.Down && inputActionData.m_Hold == 0f)
						{
							this.OnAction(inputActionData, pause);
							if (inputActionData.m_TriggerAction != TriggerAction.TYPE.None)
							{
								if (!this.m_ActiveActionsByInputAction.ContainsKey(InputHelpers.GetActionId(inputActionData.m_TriggerAction)))
								{
									this.m_ActiveActionsByInputAction.Add(InputHelpers.GetActionId(inputActionData.m_TriggerAction), (KeyCode)key);
								}
							}
							else if (!this.m_ActiveActionsByInputAction.ContainsKey(inputActionData.m_Action.GetActionId(TriggerAction.TYPE.None)))
							{
								this.m_ActiveActionsByInputAction.Add(inputActionData.m_Action.GetActionId(TriggerAction.TYPE.None), (KeyCode)key);
							}
						}
						else if (this.CanActivateHold(inputActionData))
						{
							inputActionData.m_StartHoldTime = Time.time;
							if (!this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData.m_TriggerAction))
							{
								this.m_ActiveHoldActionsByTriggerAction.Add((int)inputActionData.m_TriggerAction, inputActionData);
							}
						}
						if (inputActionData.m_Type == InputsManager.InputActionType.Down)
						{
							inputActionData.m_LastPressTime = Time.time;
						}
					}
				}
			}
			else if (Input.GetKeyUp((KeyCode)key))
			{
				if (key == 323 && this.m_OmitMouseUp)
				{
					this.m_OmitMouseUp = false;
				}
				else
				{
					foreach (InputActionData inputActionData2 in this.GetActionsByKeyCode()[key])
					{
						if (inputActionData2.m_ControllerType == GreenHellGame.Instance.m_Settings.m_ControllerType)
						{
							if (inputActionData2.m_Type == InputsManager.InputActionType.Up && inputActionData2.m_Hold == 0f)
							{
								this.OnAction(inputActionData2, pause);
							}
							else if (inputActionData2.m_Type == InputsManager.InputActionType.Up && inputActionData2.m_Hold > 0f && inputActionData2.m_StartHoldTime > 0f && Time.time - inputActionData2.m_StartHoldTime >= inputActionData2.m_Hold)
							{
								this.OnAction(inputActionData2, pause);
							}
							inputActionData2.m_StartHoldTime = 0f;
							if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData2.m_TriggerAction))
							{
								this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData2.m_TriggerAction);
							}
							if (inputActionData2.m_Type == InputsManager.InputActionType.Up)
							{
								inputActionData2.m_LastReleaseTime = Time.time;
							}
						}
					}
				}
			}
			foreach (InputActionData inputActionData3 in this.GetActionsByKeyCode()[key])
			{
				if (inputActionData3.m_ControllerType == GreenHellGame.Instance.m_Settings.m_ControllerType && inputActionData3.m_Type == InputsManager.InputActionType.Down && inputActionData3.m_StartHoldTime > 0f && Time.time - inputActionData3.m_StartHoldTime >= inputActionData3.m_Hold)
				{
					this.OnAction(inputActionData3, pause);
					if (inputActionData3.m_TriggerAction != TriggerAction.TYPE.None)
					{
						this.m_ActiveActionsByInputAction.Remove(InputHelpers.GetActionId(inputActionData3.m_TriggerAction));
					}
					else
					{
						this.m_ActiveActionsByInputAction.Remove(inputActionData3.m_Action.GetActionId(TriggerAction.TYPE.None));
					}
					inputActionData3.m_StartHoldTime = 0f;
					if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData3.m_TriggerAction))
					{
						this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData3.m_TriggerAction);
					}
				}
			}
			Dictionary<string, List<InputActionData>>.Enumerator enumerator5;
			foreach (KeyValuePair<string, List<InputActionData>> keyValuePair2 in this.m_ActionsByAxis)
			{
				float axis = this.GetAxis(keyValuePair2.Key);
				keyValuePair2 = enumerator5.Current;
				foreach (InputActionData inputActionData4 in keyValuePair2.Value)
				{
					if (inputActionData4.m_ControllerType == GreenHellGame.Instance.m_Settings.m_ControllerType)
					{
						float num = Mathf.Max(0f, axis * (inputActionData4.m_Inverted ? -1f : 1f));
						if (!inputActionData4.m_Analog)
						{
							if (inputActionData4.m_Value == 0f)
							{
								if (inputActionData4.m_AxisValue <= inputActionData4.m_Histeresis.x && num > inputActionData4.m_Histeresis.x)
								{
									if (inputActionData4.m_Type == InputsManager.InputActionType.Down)
									{
										if (inputActionData4.m_Hold == 0f)
										{
											this.OnAction(inputActionData4, pause);
										}
										else if (this.CanActivateHold(inputActionData4))
										{
											inputActionData4.m_StartHoldTime = Time.time;
											if (!this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData4.m_TriggerAction))
											{
												this.m_ActiveHoldActionsByTriggerAction.Add((int)inputActionData4.m_TriggerAction, inputActionData4);
											}
										}
										inputActionData4.m_LastPressTime = Time.time;
									}
									inputActionData4.m_Value = 1f;
								}
							}
							else if (inputActionData4.m_AxisValue > inputActionData4.m_Histeresis.y && num <= inputActionData4.m_Histeresis.y)
							{
								if (inputActionData4.m_Type == InputsManager.InputActionType.Up)
								{
									this.OnAction(inputActionData4, pause);
								}
								inputActionData4.m_LastReleaseTime = Time.time;
								inputActionData4.m_StartHoldTime = 0f;
								inputActionData4.m_Value = 0f;
							}
						}
						if (num != inputActionData4.m_AxisValue)
						{
							inputActionData4.m_AxisValue = num;
							if (inputActionData4.m_Analog)
							{
								inputActionData4.m_Value = num;
							}
						}
					}
				}
			}
			foreach (KeyValuePair<string, List<InputActionData>> keyValuePair2 in this.m_ActionsByAxis)
			{
				this.GetAxis(keyValuePair2.Key);
				keyValuePair2 = enumerator5.Current;
				foreach (InputActionData inputActionData5 in keyValuePair2.Value)
				{
					if (inputActionData5.m_ControllerType == GreenHellGame.Instance.m_Settings.m_ControllerType && !inputActionData5.m_Analog && inputActionData5.m_Type == InputsManager.InputActionType.Down && inputActionData5.m_StartHoldTime > 0f && Time.time - inputActionData5.m_StartHoldTime >= inputActionData5.m_Hold)
					{
						this.OnAction(inputActionData5, pause);
						if (inputActionData5.m_TriggerAction != TriggerAction.TYPE.None)
						{
							this.m_ActiveActionsByInputAction.Remove(InputHelpers.GetActionId(inputActionData5.m_TriggerAction));
						}
						else
						{
							this.m_ActiveActionsByInputAction.Remove(inputActionData5.m_Action.GetActionId(TriggerAction.TYPE.None));
						}
						inputActionData5.m_StartHoldTime = 0f;
						if (this.m_ActiveHoldActionsByTriggerAction.ContainsKey((int)inputActionData5.m_TriggerAction))
						{
							this.m_ActiveHoldActionsByTriggerAction.Remove((int)inputActionData5.m_TriggerAction);
						}
					}
				}
			}
		}
	}

	public float GetAxis(string axis_name)
	{
		if (this.m_PadControllerType == InputsManager.PadControllerType.Ps4 && GreenHellGame.IsPadControllerActive() && CrossPlatformInputManager.AxisExists(axis_name + "Ps4"))
		{
			return CrossPlatformInputManager.GetAxis(axis_name + "Ps4");
		}
		return CrossPlatformInputManager.GetAxis(axis_name);
	}

	private void UpdatActiveActions()
	{
		if (this.m_ActiveActionsByInputAction.Count > 0)
		{
			this.m_InputActionsToRemove.Clear();
			foreach (KeyValuePair<int, KeyCode> keyValuePair in this.m_ActiveActionsByInputAction)
			{
				int key = keyValuePair.Key;
				if (!Input.GetKey(this.m_ActiveActionsByInputAction[key]))
				{
					this.m_InputActionsToRemove.Add(key);
				}
			}
			foreach (int key2 in this.m_InputActionsToRemove)
			{
				this.m_ActiveActionsByInputAction.Remove(key2);
			}
		}
	}

	private void OnAction(InputActionData action, bool pause)
	{
		if (GreenHellGame.Instance.m_Settings.m_ControllerType != action.m_ControllerType)
		{
			return;
		}
		if (this.m_TextInputActive)
		{
			return;
		}
		if (action.m_TriggerAction != TriggerAction.TYPE.None)
		{
			if (TriggerController.Get() != null && !pause)
			{
				TriggerController.Get().OnTriggerAction(action.m_TriggerAction);
				return;
			}
		}
		else
		{
			foreach (IInputsReceiver inputsReceiver in this.m_Receivers)
			{
				if (inputsReceiver.CanReceiveAction() && (!pause || inputsReceiver.CanReceiveActionPaused()))
				{
					inputsReceiver.OnInputAction(action);
				}
			}
		}
	}

	public bool IsActionActive(InputsManager.InputAction action)
	{
		return this.GetActionValue(action) > 0f;
	}

	public bool IsActionActive(TriggerAction.TYPE action)
	{
		return this.GetActionValue(action) > 0f;
	}

	public float GetActionValue(InputsManager.InputAction action)
	{
		return this.GetActionValue(action.GetActionId(TriggerAction.TYPE.None));
	}

	public float GetActionValue(TriggerAction.TYPE action)
	{
		return this.GetActionValue(InputHelpers.GetActionId(action));
	}

	private float GetActionValue(int action)
	{
		InputActionData[] array;
		if (this.m_ActionsByInputAction.TryGetValue(action, out array))
		{
			InputActionData inputActionData = array[(int)GreenHellGame.Instance.m_Settings.m_ControllerType];
			if (inputActionData != null && inputActionData.m_AxisName != null)
			{
				return inputActionData.m_Value;
			}
			if (Time.timeScale == 0f && inputActionData != null && inputActionData.m_Type == InputsManager.InputActionType.Down && inputActionData.m_Hold == 0f)
			{
				if (!Input.GetKeyDown(inputActionData.m_KeyCode))
				{
					return 0f;
				}
				return 1f;
			}
			else if (this.m_ActiveActionsByInputAction.ContainsKey(action))
			{
				return 1f;
			}
		}
		return 0f;
	}

	public InputActionData GetInputActionData(InputsManager.InputAction action)
	{
		InputActionData[] array = null;
		this.m_ActionsByInputAction.TryGetValue((int)action, out array);
		return array[(int)GreenHellGame.Instance.m_Settings.m_ControllerType];
	}

	public InputActionData GetInputActionData(TriggerAction.TYPE trigger)
	{
		InputActionData[] array = null;
		int actionId = InputHelpers.GetActionId(trigger);
		this.m_ActionsByInputAction.TryGetValue(actionId, out array);
		return array[(int)GreenHellGame.Instance.m_Settings.m_ControllerType];
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
		return Mathf.Clamp01((Time.time - inputActionData.m_StartHoldTime) / inputActionData.m_Hold);
	}

	public InputActionData GetActionDataByInputAction(InputsManager.InputAction input_action, ControllerType controller_type = ControllerType._Count)
	{
		InputActionData[] array;
		if (this.m_ActionsByInputAction.TryGetValue(input_action.GetActionId(TriggerAction.TYPE.None), out array))
		{
			return array[(int)((controller_type == ControllerType._Count) ? GreenHellGame.Instance.m_Settings.m_ControllerType : controller_type)];
		}
		return null;
	}

	public InputActionData GetActionDataByTriggerAction(TriggerAction.TYPE trigger_action, ControllerType controller_type = ControllerType._Count)
	{
		InputActionData[] array;
		if (this.m_ActionsByInputAction.TryGetValue(InputHelpers.GetActionId(trigger_action), out array))
		{
			return array[(int)((controller_type == ControllerType._Count) ? GreenHellGame.Instance.m_Settings.m_ControllerType : controller_type)];
		}
		return null;
	}

	public void ApplyOptions(Dictionary<int, int> actions_by_input_action, Dictionary<int, int> actions_by_trigger_action, ControllerType controller_type)
	{
		foreach (KeyValuePair<int, int> keyValuePair in actions_by_input_action)
		{
			int actionId = ((InputsManager.InputAction)keyValuePair.Key).GetActionId(TriggerAction.TYPE.None);
			InputActionData[] array;
			if (!this.m_ActionsByInputAction.TryGetValue(actionId, out array))
			{
				array = new InputActionData[2];
				this.m_ActionsByInputAction.Add(actionId, array);
			}
			if (array[(int)controller_type] != null)
			{
				array[(int)controller_type].m_KeyCode = (KeyCode)keyValuePair.Value;
			}
			else
			{
				Debug.Log("");
			}
		}
		foreach (KeyValuePair<int, int> keyValuePair2 in actions_by_trigger_action)
		{
			int actionId2 = InputHelpers.GetActionId((TriggerAction.TYPE)keyValuePair2.Key);
			InputActionData[] array2;
			if (!this.m_ActionsByInputAction.TryGetValue(actionId2, out array2))
			{
				array2 = new InputActionData[2];
				this.m_ActionsByInputAction.Add(actionId2, array2);
			}
			if (array2[(int)controller_type] != null)
			{
				array2[(int)controller_type].m_KeyCode = (KeyCode)keyValuePair2.Value;
			}
			else
			{
				Debug.Log("");
			}
		}
		foreach (KeyValuePair<int, InputActionData[]> keyValuePair3 in this.m_ActionsByInputAction)
		{
			InputActionData[] value = keyValuePair3.Value;
			for (int i = 0; i < value.Length; i++)
			{
				InputActionData act = value[i];
				if (act != null && act.m_KeyCode != KeyCode.None)
				{
					KeyCode keyCode = act.m_KeyCode;
					List<InputActionData> list;
					if (!this.m_ActionsByKeyCode.TryGetValue((int)keyCode, out list))
					{
						list = new List<InputActionData>();
						this.m_ActionsByKeyCode.Add((int)keyCode, list);
					}
					if (!list.Any((InputActionData a) => a.m_Action == act.m_Action && a.m_TriggerAction == act.m_TriggerAction))
					{
						if (keyValuePair3.Value[(int)controller_type] != null)
						{
							InputActionData inputActionData = keyValuePair3.Value[(int)controller_type].ShallowCopy();
							foreach (int key in this.m_ActionsByKeyCode.Keys)
							{
								List<InputActionData> list2 = this.m_ActionsByKeyCode[key];
								bool flag = false;
								foreach (InputActionData inputActionData2 in list2)
								{
									if ((inputActionData2.m_Action != InputsManager.InputAction.None && inputActionData2.m_Action == inputActionData.m_Action) || (inputActionData2.m_TriggerAction != TriggerAction.TYPE.None && inputActionData2.m_TriggerAction == inputActionData.m_TriggerAction))
									{
										this.m_ActionsByKeyCode[key].Remove(inputActionData2);
										flag = true;
										break;
									}
								}
								if (flag)
								{
									break;
								}
							}
							list.Add(inputActionData);
						}
						else
						{
							Debug.Log("");
						}
					}
				}
			}
		}
		this.m_ActiveActionsByInputAction.Clear();
		this.m_ActiveHoldActionsByTriggerAction.Clear();
		this.m_InputActionsToRemove.Clear();
		GC.Collect();
	}

	public void SaveSettings(BinaryFormatter bf, Stream file)
	{
		ControllerType controllerType = ControllerType.PC;
		bf.Serialize(file, 1);
		bf.Serialize(file, this.m_ActionsByInputAction.Count);
		foreach (KeyValuePair<int, InputActionData[]> keyValuePair in this.m_ActionsByInputAction)
		{
			int key = keyValuePair.Key;
			if (key < 117)
			{
				string name = EnumUtils<InputsManager.InputAction>.GetName((InputsManager.InputAction)key);
				bf.Serialize(file, name);
				InputActionData[] value = keyValuePair.Value;
				string name2 = EnumUtils<KeyCode>.GetName((value != null && value[(int)controllerType] != null) ? value[(int)controllerType].m_KeyCode : KeyCode.None);
				bf.Serialize(file, name2);
			}
		}
		foreach (KeyValuePair<int, InputActionData[]> keyValuePair2 in this.m_ActionsByInputAction)
		{
			if (keyValuePair2.Key >= 117)
			{
				string name3 = EnumUtils<TriggerAction.TYPE>.GetName(InputHelpers.GetTriggerAction(keyValuePair2.Key));
				bf.Serialize(file, name3);
				InputActionData[] value2 = keyValuePair2.Value;
				string name4 = EnumUtils<KeyCode>.GetName((value2 != null && value2[(int)controllerType] != null) ? value2[(int)controllerType].m_KeyCode : KeyCode.None);
				bf.Serialize(file, name4);
			}
		}
	}

	public void LoadSettings(BinaryFormatter bf, Stream file, GameVersion gv)
	{
		int controller_type = 0;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = (int)bf.Deserialize(file);
		if (num != 1)
		{
			int num2 = num;
			for (int i = 0; i < num2; i++)
			{
				bf.Deserialize(file);
				bf.Deserialize(file);
			}
			num2 = (int)bf.Deserialize(file);
			for (int j = 0; j < num2; j++)
			{
				bf.Deserialize(file);
				bf.Deserialize(file);
			}
			return;
		}
		int num3 = (int)bf.Deserialize(file);
		for (int k = 0; k < num3; k++)
		{
			InputsManager.InputAction inputAction = InputsManager.InputAction.None;
			TriggerAction.TYPE type = TriggerAction.TYPE.None;
			string value = (string)bf.Deserialize(file);
			bool flag = Enum.TryParse<InputsManager.InputAction>(value, out inputAction);
			bool flag2 = !flag && Enum.TryParse<TriggerAction.TYPE>(value, out type);
			int actionId = inputAction.GetActionId(type);
			string value2 = (string)bf.Deserialize(file);
			KeyCode value3 = (KeyCode)Enum.Parse(typeof(KeyCode), value2);
			InputActionData[] array;
			if (this.m_ActionsByInputAction.TryGetValue(actionId, out array))
			{
				if (flag)
				{
					dictionary[(int)inputAction] = (int)value3;
				}
				if (flag2)
				{
					dictionary2[(int)type] = (int)value3;
				}
			}
		}
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse(this.m_InputsScript, true);
		if (GreenHellGame.s_GameVersionEarlyAccessUpdate6 > gv)
		{
			for (int l = 0; l < scriptParser.GetKeysCount(); l++)
			{
				Key key = scriptParser.GetKey(l);
				if (key.GetName() == "Action" && (key.GetVariable(0).SValue == EnumUtils<InputsManager.InputAction>.GetName(InputsManager.InputAction.FishingAim) || key.GetVariable(0).SValue == EnumUtils<InputsManager.InputAction>.GetName(InputsManager.InputAction.FishingCast) || key.GetVariable(0).SValue == EnumUtils<InputsManager.InputAction>.GetName(InputsManager.InputAction.FishingReel) || key.GetVariable(0).SValue == EnumUtils<InputsManager.InputAction>.GetName(InputsManager.InputAction.FishingStrike)))
				{
					this.LoadAction(key, false, false);
				}
			}
		}
		if (num3 == this.m_ActionsByInputAction.Count && gv >= GreenHellGame.s_GameVersionReleaseCandidate)
		{
			this.ApplyOptions(dictionary, dictionary2, (ControllerType)controller_type);
		}
	}

	public void ApplyDefaultOptions()
	{
		this.m_ActionsByInputAction.Clear();
		this.m_ActionsByKeyCode.Clear();
		this.m_Ps4ActionsByKeyCode.Clear();
		this.m_ActiveActionsByInputAction.Clear();
		this.m_ActiveHoldActionsByTriggerAction.Clear();
		this.m_InputActionsToRemove.Clear();
		this.LoadScript();
	}

	private string m_InputsScript = "Inputs.txt";

	public const ControllerType STORED_CONTROLLER = ControllerType.PC;

	private List<IInputsReceiver> m_Receivers = new List<IInputsReceiver>();

	private List<IInputsReceiver> m_RegisterReceiverRequests = new List<IInputsReceiver>(20);

	private List<IInputsReceiver> m_UnregisterReceiversRequests = new List<IInputsReceiver>();

	private Dictionary<int, List<InputActionData>> m_ActionsByKeyCode = new Dictionary<int, List<InputActionData>>();

	private Dictionary<int, List<InputActionData>> m_Ps4ActionsByKeyCode = new Dictionary<int, List<InputActionData>>();

	private Dictionary<string, List<InputActionData>> m_ActionsByAxis = new Dictionary<string, List<InputActionData>>();

	private Dictionary<int, InputActionData[]> m_ActionsByInputAction = new Dictionary<int, InputActionData[]>();

	private Dictionary<int, KeyCode> m_ActiveActionsByInputAction = new Dictionary<int, KeyCode>();

	private Dictionary<int, InputActionData> m_ActiveHoldActionsByTriggerAction = new Dictionary<int, InputActionData>();

	[HideInInspector]
	public bool m_OmitMouseUp = true;

	[HideInInspector]
	public bool m_TextInputActive;

	[HideInInspector]
	public InputsManager.PadControllerType m_PadControllerType;

	private static InputsManager s_Instance;

	private List<int> m_InputActionsToRemove = new List<int>(20);

	private const int NEW_SETTINGS = 1;

	public enum PadControllerType
	{
		None,
		Xbox,
		Ps4
	}

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
		BowMaxAim,
		BowStopMaxAim,
		BowShot,
		BowlSpil,
		BowlDrink,
		WaterDrink,
		FishingAim,
		FishingCancelAim,
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
		ReadCollectable,
		LookUp,
		LookDown,
		LookLeft,
		LookRight,
		HideSelectDialog,
		PrevItemOrPage,
		NextItemOrPage,
		WatchNext,
		WatchPrev,
		WheelSelect,
		HUDItemNext,
		HUDItemPrev,
		HUDItemSelect,
		HUDItemCancel,
		InventoryNextTab,
		InventoryPrevTab,
		BIShowHideArmor,
		NotepadNextTab,
		NotepadPrevTab,
		Button_A,
		Button_B,
		Button_X,
		Button_Y,
		LSForward,
		LSBackward,
		LSRight,
		LSLeft,
		RSForward,
		RSBackward,
		RSRight,
		RSLeft,
		ZoomMap,
		StartCrafting,
		SortItemsInBackpack,
		ThrowStonePad,
		TextChat,
		DuckStop,
		DPadLeft,
		DPadRight,
		DPadUp,
		DPadDown,
		R3,
		L3,
		LeftStickRot,
		RightStickRot,
		Count,
		_Trigger_First = 117
	}

	public enum InputActionType
	{
		Down,
		Up
	}
}
