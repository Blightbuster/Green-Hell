using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DaytimeReplicator : ReplicatedBehaviour, ICustomReplicationInterval
{
	public override void OnReplicationPrepare()
	{
		this.ReplSetDirty();
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initialState)
	{
		writer.Write(MainLevel.s_GameTime);
		writer.Write(MainLevel.Instance.m_TODSky.Cycle.Day);
		writer.Write(MainLevel.Instance.m_TODSky.Cycle.Hour);
		writer.Write(MainLevel.Instance.m_TODSky.Cycle.Month);
		writer.Write(MainLevel.Instance.m_TODSky.Cycle.Year);
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initialState)
	{
		MainLevel.s_GameTime = reader.ReadFloat();
		MainLevel.Instance.m_TODSky.Cycle.Day = reader.ReadInt32();
		MainLevel.Instance.m_TODSky.Cycle.Hour = reader.ReadFloat();
		MainLevel.Instance.m_TODSky.Cycle.Month = reader.ReadInt32();
		MainLevel.Instance.m_TODSky.Cycle.Year = reader.ReadInt32();
	}

	public float GetReplicationIntervalMin()
	{
		if (Player.Get().m_SleepController.IsAllPlayersSleeping())
		{
			return 0.1f;
		}
		return -1f;
	}

	public float GetReplicationIntervalMax()
	{
		return -1f;
	}
}
