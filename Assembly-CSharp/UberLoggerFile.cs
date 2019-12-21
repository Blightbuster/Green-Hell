using System;
using System.IO;
using UberLogger;
using UnityEngine;

public class UberLoggerFile : UberLogger.ILogger
{
	public UberLoggerFile(string filename, bool includeCallStacks = true)
	{
		this.IncludeCallStacks = includeCallStacks;
		string text = Path.Combine(Application.persistentDataPath, filename);
		Debug.Log("Initialising file logging to " + text);
		this.LogFileWriter = new StreamWriter(text, false);
		this.LogFileWriter.AutoFlush = true;
	}

	public void Log(LogInfo logInfo)
	{
		lock (this)
		{
			this.LogFileWriter.WriteLine(logInfo.Message);
			if (this.IncludeCallStacks && logInfo.Callstack.Count > 0)
			{
				foreach (LogStackFrame logStackFrame in logInfo.Callstack)
				{
					this.LogFileWriter.WriteLine(logStackFrame.GetFormattedMethodNameWithFileName());
				}
				this.LogFileWriter.WriteLine();
			}
		}
	}

	private StreamWriter LogFileWriter;

	private bool IncludeCallStacks;
}
