using System;
using UnityEngine;

namespace RootMotion
{
	public static class Warning
	{
		public static void Log(string message, Warning.Logger logger, bool logInEditMode = false)
		{
			if (!logInEditMode && !Application.isPlaying)
			{
				return;
			}
			if (Warning.logged)
			{
				return;
			}
			if (logger != null)
			{
				logger(message);
			}
			Warning.logged = true;
		}

		public static void Log(string message, Transform context, bool logInEditMode = false)
		{
			if (!logInEditMode && !Application.isPlaying)
			{
				return;
			}
			if (Warning.logged)
			{
				return;
			}
			Debug.LogWarning(message, context);
			Warning.logged = true;
		}

		public static bool logged;

		public delegate void Logger(string message);
	}
}
