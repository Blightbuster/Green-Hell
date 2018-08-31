using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class CJDebug
{
	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern void OutputDebugString(string message);

	public static void Log(string message)
	{
		CJDebug.OutputDebugString(message);
		Debug.Log(message);
		CJDebug.m_Log += "\n";
		CJDebug.m_Log += message;
	}

	public static string m_Log = string.Empty;
}
