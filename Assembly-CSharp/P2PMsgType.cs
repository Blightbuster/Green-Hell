using System;

public class P2PMsgType
{
	public static string MsgTypeToString(short value)
	{
		if (value < 0 || value > 37)
		{
			return string.Empty;
		}
		string text = P2PMsgType.msgLabels[(int)value];
		if (string.IsNullOrEmpty(text))
		{
			text = "[" + value + "]";
		}
		return text;
	}

	public const short Replication = 1;

	public const short ObjectSpawn = 2;

	public const short ObjectDespawn = 3;

	public const short PeerInfo = 5;

	public const short RequestOwnership = 6;

	public const short GiveOwnership = 7;

	public const short NeedFullObjectInfo = 8;

	public const short ObjectSpawnAndGiveOwnership = 9;

	public const short TextChat = 10;

	internal const short UserMessage = 0;

	internal const short HLAPIMsg = 28;

	internal const short LLAPIMsg = 29;

	internal const short HLAPIResend = 30;

	internal const short HLAPIPending = 31;

	public const short InternalHighest = 31;

	public const short Connect = 32;

	public const short Disconnect = 33;

	public const short Error = 34;

	public const short Ready = 35;

	public const short NotReady = 36;

	public const short Highest = 37;

	internal static string[] msgLabels = new string[]
	{
		"none",
		"Replication",
		"ObjectSpawn",
		"ObjectDespawn",
		"",
		"PeerInfo",
		"RequestOwnership",
		"GiveOwnership",
		"NeedFullObjectInfo",
		"ObjectSpawnAndGiveOwnership",
		"TextChat",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"",
		"HLAPIMsg",
		"LLAPIMsg",
		"HLAPIResend",
		"HLAPIPending",
		"",
		"",
		"",
		"",
		""
	};
}
