using System;
using System.Collections.Generic;
using UnityEngine;

internal static class InputHelpers
{
	public static KeyCode KeyFromPad(this InputHelpers.PadButton pad_button)
	{
		return pad_button.KeyFromPad(GreenHellGame.IsPadControllerActive() ? InputsManager.Get().m_PadControllerType : InputsManager.PadControllerType.None);
	}

	public static KeyCode KeyFromPad(this InputHelpers.PadButton pad_button, InputsManager.PadControllerType controller_type)
	{
		if (controller_type == InputsManager.PadControllerType.Ps4)
		{
			switch (pad_button)
			{
			case InputHelpers.PadButton.Button_X:
				return KeyCode.JoystickButton0;
			case InputHelpers.PadButton.Button_Y:
				return KeyCode.JoystickButton3;
			case InputHelpers.PadButton.Button_A:
				return KeyCode.JoystickButton1;
			case InputHelpers.PadButton.Button_B:
				return KeyCode.JoystickButton2;
			case InputHelpers.PadButton.LB:
				return KeyCode.JoystickButton4;
			case InputHelpers.PadButton.RB:
				return KeyCode.JoystickButton5;
			case InputHelpers.PadButton.Back:
				return KeyCode.JoystickButton8;
			case InputHelpers.PadButton.Start:
				return KeyCode.JoystickButton9;
			case InputHelpers.PadButton.R3:
				return KeyCode.JoystickButton11;
			case InputHelpers.PadButton.L3:
				return KeyCode.JoystickButton10;
			case InputHelpers.PadButton.LeftStickRot:
				return KeyCode.JoystickButton10;
			case InputHelpers.PadButton.RightStickRot:
				return KeyCode.JoystickButton11;
			default:
				return KeyCode.None;
			}
		}
		else
		{
			switch (pad_button)
			{
			case InputHelpers.PadButton.Button_X:
				return KeyCode.JoystickButton2;
			case InputHelpers.PadButton.Button_Y:
				return KeyCode.JoystickButton3;
			case InputHelpers.PadButton.Button_A:
				return KeyCode.JoystickButton0;
			case InputHelpers.PadButton.Button_B:
				return KeyCode.JoystickButton1;
			case InputHelpers.PadButton.LB:
				return KeyCode.JoystickButton4;
			case InputHelpers.PadButton.RB:
				return KeyCode.JoystickButton5;
			case InputHelpers.PadButton.Back:
				return KeyCode.JoystickButton6;
			case InputHelpers.PadButton.Start:
				return KeyCode.JoystickButton7;
			case InputHelpers.PadButton.R3:
				return KeyCode.JoystickButton9;
			case InputHelpers.PadButton.L3:
				return KeyCode.JoystickButton8;
			case InputHelpers.PadButton.LeftStickRot:
				return KeyCode.JoystickButton10;
			case InputHelpers.PadButton.RightStickRot:
				return KeyCode.JoystickButton11;
			default:
				return KeyCode.None;
			}
		}
	}

	public static InputHelpers.PadButton PadButtonFromKey(this KeyCode key)
	{
		switch (key)
		{
		case KeyCode.JoystickButton0:
			return InputHelpers.PadButton.Button_A;
		case KeyCode.JoystickButton1:
			return InputHelpers.PadButton.Button_B;
		case KeyCode.JoystickButton2:
			return InputHelpers.PadButton.Button_X;
		case KeyCode.JoystickButton3:
			return InputHelpers.PadButton.Button_Y;
		case KeyCode.JoystickButton4:
			return InputHelpers.PadButton.LB;
		case KeyCode.JoystickButton5:
			return InputHelpers.PadButton.RB;
		case KeyCode.JoystickButton6:
			return InputHelpers.PadButton.Back;
		case KeyCode.JoystickButton7:
			return InputHelpers.PadButton.Start;
		case KeyCode.JoystickButton8:
			return InputHelpers.PadButton.L3;
		case KeyCode.JoystickButton9:
			return InputHelpers.PadButton.R3;
		case KeyCode.JoystickButton10:
			return InputHelpers.PadButton.LeftStickRot;
		case KeyCode.JoystickButton11:
			return InputHelpers.PadButton.RightStickRot;
		default:
			return InputHelpers.PadButton.None;
		}
	}

	public static void InitPadIcons()
	{
		foreach (Sprite sprite in Resources.LoadAll<Sprite>("HUD/Icon_Pad"))
		{
			if (Enum.IsDefined(typeof(InputHelpers.PadButton), sprite.name))
			{
				InputHelpers.PadButton value = EnumUtils<InputHelpers.PadButton>.GetValue(sprite.name);
				InputHelpers.m_PadButtonIconsMap.Add((int)value, sprite);
			}
			InputHelpers.m_PadIconsMap.Add(sprite.name, sprite);
		}
	}

	public static Sprite GetIconFromPadButton(InputHelpers.PadButton button)
	{
		Sprite result = null;
		InputHelpers.m_PadButtonIconsMap.TryGetValue((int)button, out result);
		return result;
	}

	public static Sprite GetPadIcon(InputActionData action)
	{
		Sprite sprite = null;
		if (action.m_KeyCode != KeyCode.None)
		{
			sprite = InputHelpers.GetIconFromPadButton(action.m_KeyCode.PadButtonFromKey());
		}
		else
		{
			string key = string.Empty;
			string axisName = action.m_AxisName;
			if (!(axisName == "DPadY"))
			{
				if (!(axisName == "DPadX"))
				{
					if (!(axisName == "LeftStickY"))
					{
						if (!(axisName == "LeftStickX"))
						{
							if (!(axisName == "RightStickY"))
							{
								if (!(axisName == "RightStickX"))
								{
									key = action.m_AxisName;
								}
								else
								{
									key = (action.m_Inverted ? "RightStickLeft" : "RightStickRight");
								}
							}
							else
							{
								key = (action.m_Inverted ? "RightStickBackward" : "RightStickForward");
							}
						}
						else
						{
							key = (action.m_Inverted ? "LeftStickLeft" : "LeftStickRight");
						}
					}
					else
					{
						key = (action.m_Inverted ? "LeftStickBackward" : "LeftStickForward");
					}
				}
				else
				{
					key = (action.m_Inverted ? "DPadLeft" : "DPadRight");
				}
			}
			else
			{
				key = (action.m_Inverted ? "DPadDown" : "DPadUp");
			}
			InputHelpers.m_PadIconsMap.TryGetValue(key, out sprite);
		}
		sprite == null;
		return sprite;
	}

	public static ControllerType GetControllerType(this KeyCode key_code)
	{
		if (key_code >= KeyCode.JoystickButton0)
		{
			return ControllerType.Pad;
		}
		return ControllerType.PC;
	}

	public static int GetActionId(this InputsManager.InputAction action, TriggerAction.TYPE trigger = TriggerAction.TYPE.None)
	{
		if (trigger != TriggerAction.TYPE.None)
		{
			return (int)(117 + trigger);
		}
		return (int)action;
	}

	public static int GetActionId(TriggerAction.TYPE trigger)
	{
		return (int)(117 + trigger);
	}

	public static InputsManager.InputAction GetInputAction(int action_id)
	{
		if (action_id < 117)
		{
			return (InputsManager.InputAction)action_id;
		}
		return InputsManager.InputAction.None;
	}

	public static TriggerAction.TYPE GetTriggerAction(int action_id)
	{
		if (action_id >= 117)
		{
			return (TriggerAction.TYPE)(action_id - 117);
		}
		return TriggerAction.TYPE.None;
	}

	public static Vector2 GetLookInput(float mouse_mul_x = 1f, float mouse_mul_y = 1f, float pad_multiplier = 150f)
	{
		Vector2 vector = new Vector2(InputsManager.Get().GetActionValue(InputsManager.InputAction.LookRight) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookLeft), InputsManager.Get().GetActionValue(InputsManager.InputAction.LookUp) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookDown));
		if (GreenHellGame.Instance.m_Settings.m_InvertMouseY)
		{
			vector.y *= -1f;
		}
		if (GreenHellGame.Instance.m_Settings.m_ControllerType != ControllerType.PC)
		{
			float magnitude = vector.magnitude;
			if (magnitude > 1f)
			{
				vector /= magnitude;
			}
			vector *= Mathf.Pow(vector.magnitude, 2f);
			vector *= Time.deltaTime * pad_multiplier;
		}
		else
		{
			vector.x *= mouse_mul_x;
			vector.y *= mouse_mul_y;
		}
		return vector;
	}

	private static Dictionary<int, Sprite> m_PadButtonIconsMap = new Dictionary<int, Sprite>();

	private static Dictionary<string, Sprite> m_PadIconsMap = new Dictionary<string, Sprite>();

	private const float PAD_MULTIPLIER_DEFAULT = 150f;

	public enum PadButton
	{
		None = -1,
		Button_X,
		Button_Y,
		Button_A,
		Button_B,
		LB,
		RB,
		Back,
		Start,
		R3,
		L3,
		LeftStickRot,
		RightStickRot
	}
}
