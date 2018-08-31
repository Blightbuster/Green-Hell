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
				KeyCodeToString.s_Dict.Add((int)enumerator.Current, enumerator.Current.ToString());
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
