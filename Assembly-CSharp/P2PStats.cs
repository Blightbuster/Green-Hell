using System;
using System.Collections.Generic;

public static class P2PStats
{
	public static Dictionary<short, P2PStats.PacketStat> GetPacketStats(P2PNetworkDirection dir)
	{
		if (dir == P2PNetworkDirection.Incoming)
		{
			return P2PStats.m_PacketStatsIncoming;
		}
		return P2PStats.m_PacketStatsOutgoing;
	}

	public static Dictionary<Type, P2PStats.ReplicationStat> GetReplicationStats()
	{
		return P2PStats.m_ReplicationStats;
	}

	public static void IncrementPacketStat(P2PNetworkDirection dir, short msg_type, int count, int bytes = 0)
	{
	}

	public static void IncrementReplicationStat(P2PNetworkDirection dir, Type component_type, int count, int bytes = 0)
	{
	}

	public static void SetPacketStat(P2PNetworkDirection dir, short msg_type, int count, int bytes = 0)
	{
	}

	public static void ResetStats()
	{
		P2PStats.m_PacketStatsIncoming.Clear();
		P2PStats.m_PacketStatsOutgoing.Clear();
		P2PStats.m_ReplicationStats.Clear();
	}

	private static Dictionary<short, P2PStats.PacketStat> m_PacketStatsIncoming = new Dictionary<short, P2PStats.PacketStat>();

	private static Dictionary<short, P2PStats.PacketStat> m_PacketStatsOutgoing = new Dictionary<short, P2PStats.PacketStat>();

	private static Dictionary<Type, P2PStats.ReplicationStat> m_ReplicationStats = new Dictionary<Type, P2PStats.ReplicationStat>();

	public class PacketStat
	{
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				P2PMsgType.MsgTypeToString(this.msgType),
				": count=",
				this.count,
				" bytes=",
				this.bytes
			});
		}

		public short msgType;

		public int count;

		public int bytes;
	}

	public class ReplicationStat
	{
		public Type componentType;

		public P2PStats.ReplicationStat.SValue inStats;

		public P2PStats.ReplicationStat.SValue outStats;

		public struct SValue
		{
			public int count;

			public int bytes;
		}
	}
}
