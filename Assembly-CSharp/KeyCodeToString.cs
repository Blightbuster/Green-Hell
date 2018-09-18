using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class KeyCodeToString
{
	private static void Initialize()
	{
		Array values = Enum.GetValues(typeof(KeyCode));
		IEnumerator enumerator = values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!KeyCodeToString.s_Dict.ContainsKey((int)enumerator.Current))
			{
				string text = enumerator.Current.ToString();
				if (text == "Alpha0")
				{
					text = "0";
				}
				if (text == "Alpha1")
				{
					text = "1";
				}
				if (text == "Alpha2")
				{
					text = "2";
				}
				if (text == "Alpha3")
				{
					text = "3";
				}
				if (text == "Alpha4")
				{
					text = "4";
				}
				if (text == "Alpha5")
				{
					text = "5";
				}
				if (text == "Alpha6")
				{
					text = "6";
				}
				if (text == "Alpha7")
				{
					text = "7";
				}
				if (text == "Alpha8")
				{
					text = "8";
				}
				if (text == "Alpha9")
				{
					text = "9";
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
