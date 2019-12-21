using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class StopwatchHelper
{
	public static void Start(string name)
	{
		StopwatchHelper.GetWatch(name).Start();
	}

	public static void Stop(string name)
	{
		StopwatchHelper.GetWatch(name).Stop();
	}

	public static void Reset(string name)
	{
		StopwatchHelper.GetWatch(name).Reset();
	}

	public static void LogOut()
	{
		foreach (KeyValuePair<string, Stopwatch> keyValuePair in StopwatchHelper.s_Watches)
		{
			UnityEngine.Debug.LogError(string.Format("watch {0} time {1}", keyValuePair.Key, keyValuePair.Value.ElapsedMilliseconds));
		}
	}

	public static Stopwatch GetWatch(string name)
	{
		Stopwatch stopwatch;
		if (!StopwatchHelper.s_Watches.TryGetValue(name, out stopwatch))
		{
			stopwatch = new Stopwatch();
			StopwatchHelper.s_Watches.Add(name, stopwatch);
		}
		return stopwatch;
	}

	private static Dictionary<string, Stopwatch> s_Watches = new Dictionary<string, Stopwatch>();
}
