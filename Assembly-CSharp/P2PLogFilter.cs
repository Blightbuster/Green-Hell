using System;

public class P2PLogFilter
{
	public static int currentLogLevel
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel;
		}
		set
		{
			P2PLogFilter.s_CurrentLogLevel = value;
		}
	}

	public static bool logPedantic
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 0;
		}
	}

	public static bool logDev
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 1;
		}
	}

	public static bool logDebug
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 2;
		}
	}

	public static bool logInfo
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 3;
		}
	}

	public static bool logWarn
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 4;
		}
	}

	public static bool logError
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 5;
		}
	}

	public static bool logFatal
	{
		get
		{
			return P2PLogFilter.s_CurrentLogLevel <= 6;
		}
	}

	public const int Pedantic = 0;

	public const int Developer = 1;

	public const int Debug = 2;

	public const int Info = 3;

	public const int Warn = 4;

	public const int Error = 5;

	public const int Fatal = 6;

	private static int s_CurrentLogLevel = 6;

	public enum FilterLevel
	{
		Pedantic,
		Developer,
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}
}
