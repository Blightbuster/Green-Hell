using System;
using System.Collections;
using System.Collections.Generic;

internal class InputActionToString
{
	private static void Initialize()
	{
		Array values = Enum.GetValues(typeof(InputsManager.InputAction));
		IEnumerator enumerator = values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!InputActionToString.s_Dict.ContainsKey((int)enumerator.Current))
			{
				InputActionToString.s_Dict.Add((int)enumerator.Current, enumerator.Current.ToString());
			}
		}
		InputActionToString.s_Initialized = true;
	}

	public static string GetString(InputsManager.InputAction input_action)
	{
		if (!InputActionToString.s_Initialized)
		{
			InputActionToString.Initialize();
		}
		return InputActionToString.s_Dict[(int)input_action];
	}

	private static bool s_Initialized = false;

	private static Dictionary<int, string> s_Dict = new Dictionary<int, string>();
}
