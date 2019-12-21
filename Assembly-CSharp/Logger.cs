using System;
using System.Diagnostics;
using UnityEngine;

public sealed class Logger
{
	[Conditional("DEBUG_ENABLED")]
	public static void Log(object message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void Log(object message, UnityEngine.Object context)
	{
		UnityEngine.Debug.Log(message, context);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogFormat(string message, params object[] args)
	{
		UnityEngine.Debug.LogFormat(message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogFormat(UnityEngine.Object context, string message, params object[] args)
	{
		UnityEngine.Debug.LogFormat(context, message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogWarning(object message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogWarning(object message, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogWarning(message, context);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogWarningFormat(string message, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogWarningFormat(UnityEngine.Object context, string message, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(context, message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogError(object message)
	{
		UnityEngine.Debug.LogError(message);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogError(object message, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogError(message, context);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogErrorFormat(string message, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogErrorFormat(UnityEngine.Object context, string message, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(context, message, args);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogException(Exception exception)
	{
		UnityEngine.Debug.LogException(exception);
	}

	[Conditional("DEBUG_ENABLED")]
	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogException(exception, context);
	}

	public const string LOGGER_SYMBOL = "DEBUG_ENABLED";
}
