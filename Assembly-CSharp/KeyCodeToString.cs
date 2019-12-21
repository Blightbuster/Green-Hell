using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class KeyCodeToString
{
	private static void Initialize()
	{
		IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!KeyCodeToString.s_Dict.ContainsKey((int)enumerator.Current))
			{
				string text = enumerator.Current.ToString();
				if (text == "Alpha0")
				{
					text = "0";
				}
				else if (text == "Alpha1")
				{
					text = "1";
				}
				else if (text == "Alpha2")
				{
					text = "2";
				}
				else if (text == "Alpha3")
				{
					text = "3";
				}
				else if (text == "Alpha4")
				{
					text = "4";
				}
				else if (text == "Alpha5")
				{
					text = "5";
				}
				else if (text == "Alpha6")
				{
					text = "6";
				}
				else if (text == "Alpha7")
				{
					text = "7";
				}
				else if (text == "Alpha8")
				{
					text = "8";
				}
				else if (text == "Alpha9")
				{
					text = "9";
				}
				else if (text == "Mouse0")
				{
					text = "LMB";
				}
				else if (text == "Mouse1")
				{
					text = "RMB";
				}
				else if (text == "Mouse2")
				{
					text = "MMB";
				}
				else if (text == "JoystickButton0")
				{
					text = "A";
				}
				else if (text == "JoystickButton1")
				{
					text = "B";
				}
				else if (text == "JoystickButton2")
				{
					text = "X";
				}
				else if (text == "JoystickButton3")
				{
					text = "Y";
				}
				else if (text == "JoystickButton4")
				{
					text = "LB";
				}
				else if (text == "JoystickButton5")
				{
					text = "RB";
				}
				else if (text == "JoystickButton6")
				{
					text = "Back";
				}
				else if (text == "JoystickButton7")
				{
					text = "Start";
				}
				else if (text == "JoystickButton8")
				{
					text = "L3";
				}
				else if (text == "JoystickButton9")
				{
					text = "R3";
				}
				KeyCodeToString.s_Dict.Add((int)enumerator.Current, text);
			}
		}
		KeyCodeToString.s_Initialized = true;
	}

	public static string GetString(KeyCode key_code)
	{
		if (!KeyCodeToString.s_Initialized)
		{
			KeyCodeToString.Initialize();
		}
		return KeyCodeToString.s_Dict[(int)key_code];
	}

	private static bool s_Initialized = false;

	private static Dictionary<int, string> s_Dict = new Dictionary<int, string>();
}
