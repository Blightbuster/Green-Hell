using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UberLogger
{
	public static class Logger
	{
		static Logger()
		{
			Application.logMessageReceivedThreaded += UberLogger.Logger.UnityLogHandler;
			UberLogger.Logger.StartTick = DateTime.Now.Ticks;
			UberLogger.Logger.UnityMessageRegex = new Regex("(.*)\\((\\d+).*\\)");
		}

		[StackTraceIgnore]
		private static void UnityLogHandler(string logString, string stackTrace, LogType logType)
		{
			UberLogger.Logger.UnityLogInternal(logString, stackTrace, logType);
		}

		public static double GetRelativeTime()
		{
			return TimeSpan.FromTicks(DateTime.Now.Ticks - UberLogger.Logger.StartTick).TotalSeconds;
		}

		public static void AddLogger(ILogger logger, bool populateWithExistingMessages = true)
		{
			List<ILogger> loggers = UberLogger.Logger.Loggers;
			lock (loggers)
			{
				if (populateWithExistingMessages)
				{
					foreach (LogInfo logInfo in UberLogger.Logger.RecentMessages)
					{
						logger.Log(logInfo);
					}
				}
				if (!UberLogger.Logger.Loggers.Contains(logger))
				{
					UberLogger.Logger.Loggers.Add(logger);
				}
			}
		}

		public static void AddFilter(IFilter filter)
		{
			List<ILogger> loggers = UberLogger.Logger.Loggers;
			lock (loggers)
			{
				UberLogger.Logger.Filters.Add(filter);
			}
		}

		public static string ConvertDirectorySeparatorsFromUnityToOS(string unityFileName)
		{
			return unityFileName.Replace(UberLogger.Logger.UnityInternalDirectorySeparator, Path.DirectorySeparatorChar);
		}

		public static bool ExtractInfoFromUnityMessage(string log, ref string filename, ref int lineNumber)
		{
			MatchCollection matchCollection = UberLogger.Logger.UnityMessageRegex.Matches(log);
			if (matchCollection.Count > 0)
			{
				filename = matchCollection[0].Groups[1].Value;
				lineNumber = Convert.ToInt32(matchCollection[0].Groups[2].Value);
				return true;
			}
			return false;
		}

		public static bool ExtractInfoFromUnityStackInfo(string log, ref string declaringType, ref string methodName, ref string filename, ref int lineNumber)
		{
			MatchCollection matchCollection = Regex.Matches(log, "(.*)\\.(.*)\\s*\\(.*\\(at (.*):(\\d+)");
			if (matchCollection.Count > 0)
			{
				declaringType = matchCollection[0].Groups[1].Value;
				methodName = matchCollection[0].Groups[2].Value;
				filename = matchCollection[0].Groups[3].Value;
				lineNumber = Convert.ToInt32(matchCollection[0].Groups[4].Value);
				return true;
			}
			return false;
		}

		private static UberLogger.Logger.IgnoredUnityMethod.Mode ShowOrHideMethod(MethodBase method)
		{
			foreach (UberLogger.Logger.IgnoredUnityMethod ignoredUnityMethod in UberLogger.Logger.IgnoredUnityMethods)
			{
				if (method.DeclaringType.Name == ignoredUnityMethod.DeclaringTypeName && (ignoredUnityMethod.MethodName == null || method.Name == ignoredUnityMethod.MethodName))
				{
					return ignoredUnityMethod.ShowHideMode;
				}
			}
			return UberLogger.Logger.IgnoredUnityMethod.Mode.Show;
		}

		[StackTraceIgnore]
		private static bool GetCallstack(ref List<LogStackFrame> callstack, out LogStackFrame originatingSourceLocation)
		{
			callstack.Clear();
			StackFrame[] frames = new StackTrace(true).GetFrames();
			bool flag = false;
			originatingSourceLocation = null;
			for (int i = frames.Length - 1; i >= 0; i--)
			{
				StackFrame stackFrame = frames[i];
				MethodBase method = stackFrame.GetMethod();
				if (method.IsDefined(typeof(LogUnityOnly), true))
				{
					return true;
				}
				if (!method.IsDefined(typeof(StackTraceIgnore), true))
				{
					UberLogger.Logger.IgnoredUnityMethod.Mode mode = UberLogger.Logger.ShowOrHideMethod(method);
					bool flag2 = mode == UberLogger.Logger.IgnoredUnityMethod.Mode.Show;
					if (mode == UberLogger.Logger.IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod)
					{
						if (!flag)
						{
							flag = true;
							mode = UberLogger.Logger.IgnoredUnityMethod.Mode.Show;
						}
						else
						{
							mode = UberLogger.Logger.IgnoredUnityMethod.Mode.Hide;
						}
					}
					if (mode == UberLogger.Logger.IgnoredUnityMethod.Mode.Show)
					{
						LogStackFrame logStackFrame = new LogStackFrame(stackFrame);
						callstack.Add(logStackFrame);
						if (flag2)
						{
							originatingSourceLocation = logStackFrame;
						}
					}
				}
			}
			callstack.Reverse();
			return false;
		}

		private static List<LogStackFrame> GetCallstackFromUnityLog(string unityCallstack, out LogStackFrame originatingSourceLocation)
		{
			string[] array = Regex.Split(unityCallstack, UberLogger.Logger.UnityInternalNewLine);
			List<LogStackFrame> list = new List<LogStackFrame>();
			foreach (string unityStackFrame in array)
			{
				if (!string.IsNullOrEmpty(new LogStackFrame(unityStackFrame).GetFormattedMethodNameWithFileName()))
				{
					list.Add(new LogStackFrame(unityStackFrame));
				}
			}
			if (list.Count > 0)
			{
				originatingSourceLocation = list[0];
			}
			else
			{
				originatingSourceLocation = null;
			}
			return list;
		}

		[StackTraceIgnore]
		private static void UnityLogInternal(string unityMessage, string unityCallStack, LogType logType)
		{
			List<ILogger> loggers = UberLogger.Logger.Loggers;
			lock (loggers)
			{
				if (!UberLogger.Logger.AlreadyLogging)
				{
					try
					{
						UberLogger.Logger.AlreadyLogging = true;
						List<LogStackFrame> list = new List<LogStackFrame>();
						LogStackFrame originatingSourceLocation;
						if (!UberLogger.Logger.GetCallstack(ref list, out originatingSourceLocation))
						{
							if (list.Count == 0)
							{
								list = UberLogger.Logger.GetCallstackFromUnityLog(unityCallStack, out originatingSourceLocation);
							}
							LogSeverity severity;
							switch (logType)
							{
							case LogType.Error:
								severity = LogSeverity.Error;
								goto IL_80;
							case LogType.Assert:
								severity = LogSeverity.Error;
								goto IL_80;
							case LogType.Warning:
								severity = LogSeverity.Warning;
								goto IL_80;
							case LogType.Exception:
								severity = LogSeverity.Error;
								goto IL_80;
							}
							severity = LogSeverity.Message;
							IL_80:
							string filename = "";
							int lineNumber = 0;
							if (UberLogger.Logger.ExtractInfoFromUnityMessage(unityMessage, ref filename, ref lineNumber))
							{
								list.Insert(0, new LogStackFrame(unityMessage, filename, lineNumber));
							}
							LogInfo logInfo = new LogInfo(null, "", severity, list, originatingSourceLocation, unityMessage, Array.Empty<object>());
							UberLogger.Logger.RecentMessages.AddLast(logInfo);
							UberLogger.Logger.TrimOldMessages();
							UberLogger.Logger.Loggers.RemoveAll((ILogger l) => l == null);
							UberLogger.Logger.Loggers.ForEach(delegate(ILogger l)
							{
								l.Log(logInfo);
							});
						}
					}
					finally
					{
						UberLogger.Logger.AlreadyLogging = false;
					}
				}
			}
		}

		[StackTraceIgnore]
		public static void Log(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
		{
			List<ILogger> loggers = UberLogger.Logger.Loggers;
			lock (loggers)
			{
				if (!UberLogger.Logger.AlreadyLogging)
				{
					try
					{
						UberLogger.Logger.AlreadyLogging = true;
						using (List<IFilter>.Enumerator enumerator = UberLogger.Logger.Filters.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (!enumerator.Current.ApplyFilter(channel, source, severity, message, par))
								{
									return;
								}
							}
						}
						List<LogStackFrame> callstack = new List<LogStackFrame>();
						LogStackFrame originatingSourceLocation;
						if (!UberLogger.Logger.GetCallstack(ref callstack, out originatingSourceLocation))
						{
							LogInfo logInfo = new LogInfo(source, channel, severity, callstack, originatingSourceLocation, message, par);
							UberLogger.Logger.RecentMessages.AddLast(logInfo);
							UberLogger.Logger.TrimOldMessages();
							UberLogger.Logger.Loggers.RemoveAll((ILogger l) => l == null);
							UberLogger.Logger.Loggers.ForEach(delegate(ILogger l)
							{
								l.Log(logInfo);
							});
							if (UberLogger.Logger.ForwardMessages)
							{
								UberLogger.Logger.ForwardToUnity(source, severity, message, par);
							}
						}
					}
					finally
					{
						UberLogger.Logger.AlreadyLogging = false;
					}
				}
			}
		}

		[LogUnityOnly]
		private static void ForwardToUnity(UnityEngine.Object source, LogSeverity severity, object message, params object[] par)
		{
			object message2 = null;
			if (message != null)
			{
				string text = message as string;
				if (text != null)
				{
					if (par.Length != 0)
					{
						message2 = string.Format(text, par);
					}
					else
					{
						message2 = message;
					}
				}
				else
				{
					message2 = message;
				}
			}
			if (source == null)
			{
				if (severity == LogSeverity.Message)
				{
					UnityEngine.Debug.Log(message2);
					return;
				}
				if (severity == LogSeverity.Warning)
				{
					UnityEngine.Debug.LogWarning(message2);
					return;
				}
				if (severity == LogSeverity.Error)
				{
					UnityEngine.Debug.LogError(message2);
					return;
				}
			}
			else
			{
				if (severity == LogSeverity.Message)
				{
					UnityEngine.Debug.Log(message2, source);
					return;
				}
				if (severity == LogSeverity.Warning)
				{
					UnityEngine.Debug.LogWarning(message2, source);
					return;
				}
				if (severity == LogSeverity.Error)
				{
					UnityEngine.Debug.LogError(message2, source);
				}
			}
		}

		public static T GetLogger<T>() where T : class
		{
			foreach (ILogger logger in UberLogger.Logger.Loggers)
			{
				if (logger is T)
				{
					return logger as T;
				}
			}
			return default(T);
		}

		private static void TrimOldMessages()
		{
			while (UberLogger.Logger.RecentMessages.Count > UberLogger.Logger.MaxMessagesToKeep)
			{
				UberLogger.Logger.RecentMessages.RemoveFirst();
			}
		}

		public static int MaxMessagesToKeep = 1000;

		public static bool ForwardMessages = true;

		public static string UnityInternalNewLine = "\n";

		public static char UnityInternalDirectorySeparator = '/';

		private static List<ILogger> Loggers = new List<ILogger>();

		private static LinkedList<LogInfo> RecentMessages = new LinkedList<LogInfo>();

		private static long StartTick;

		private static bool AlreadyLogging = false;

		private static Regex UnityMessageRegex;

		private static List<IFilter> Filters = new List<IFilter>();

		private static UberLogger.Logger.IgnoredUnityMethod[] IgnoredUnityMethods = new UberLogger.Logger.IgnoredUnityMethod[]
		{
			new UberLogger.Logger.IgnoredUnityMethod
			{
				DeclaringTypeName = "Application",
				MethodName = "CallLogCallback",
				ShowHideMode = UberLogger.Logger.IgnoredUnityMethod.Mode.Hide
			},
			new UberLogger.Logger.IgnoredUnityMethod
			{
				DeclaringTypeName = "DebugLogHandler",
				MethodName = null,
				ShowHideMode = UberLogger.Logger.IgnoredUnityMethod.Mode.Hide
			},
			new UberLogger.Logger.IgnoredUnityMethod
			{
				DeclaringTypeName = "Logger",
				MethodName = null,
				ShowHideMode = UberLogger.Logger.IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
			},
			new UberLogger.Logger.IgnoredUnityMethod
			{
				DeclaringTypeName = "Debug",
				MethodName = null,
				ShowHideMode = UberLogger.Logger.IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
			},
			new UberLogger.Logger.IgnoredUnityMethod
			{
				DeclaringTypeName = "Assert",
				MethodName = null,
				ShowHideMode = UberLogger.Logger.IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
			}
		};

		private struct IgnoredUnityMethod
		{
			public string DeclaringTypeName;

			public string MethodName;

			public UberLogger.Logger.IgnoredUnityMethod.Mode ShowHideMode;

			public enum Mode
			{
				Show,
				ShowIfFirstIgnoredMethod,
				Hide
			}
		}
	}
}
