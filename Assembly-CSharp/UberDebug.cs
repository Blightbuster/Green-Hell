using System;
using UberLogger;
using UnityEngine;

public static class UberDebug
{
	[StackTraceIgnore]
	public static void Log(UnityEngine.Object context, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void Log(string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogChannel(UnityEngine.Object context, string channel, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogChannel(string channel, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogWarning(UnityEngine.Object context, object message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogWarning(object message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogWarningChannel(UnityEngine.Object context, string channel, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogWarningChannel(string channel, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogError(UnityEngine.Object context, object message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogError(object message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogErrorChannel(UnityEngine.Object context, string channel, string message, params object[] par)
	{
	}

	[StackTraceIgnore]
	public static void LogErrorChannel(string channel, string message, params object[] par)
	{
	}

	[LogUnityOnly]
	public static void UnityLog(object message)
	{
	}

	[LogUnityOnly]
	public static void UnityLogWarning(object message)
	{
	}

	[LogUnityOnly]
	public static void UnityLogError(object message)
	{
	}
}
