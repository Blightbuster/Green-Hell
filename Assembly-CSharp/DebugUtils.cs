using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils
{
	public static bool Assert(bool condition, bool show_stack = true)
	{
		return DebugUtils.Assert(condition, "Assertion", show_stack, DebugUtils.AssertType.Info);
	}

	public static bool Assert(UnityEngine.Object obj, bool show_stack = true)
	{
		return DebugUtils.Assert(obj != null, "Assertion", show_stack, DebugUtils.AssertType.Info);
	}

	public static bool Assert(DebugUtils.AssertType type = DebugUtils.AssertType.Info)
	{
		return DebugUtils.Assert(false, "Assertion", true, type);
	}

	public static bool Assert(string msg, bool show_stack = true, DebugUtils.AssertType type = DebugUtils.AssertType.Info)
	{
		return DebugUtils.Assert(false, msg, show_stack, type);
	}

	public static bool Assert(bool condition, string msg, bool show_stack = true, DebugUtils.AssertType type = DebugUtils.AssertType.Info)
	{
		if (condition)
		{
			return false;
		}
		CJDebug.Log(msg);
		return true;
	}

	private static List<string> m_Ignored = new List<string>();

	private static bool m_Exit = false;

	public enum AssertType
	{
		FatalError,
		Error,
		Info
	}
}
