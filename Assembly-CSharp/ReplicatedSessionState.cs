using System;
using UnityEngine;

public class ReplicatedSessionState : ReplicatedBehaviour, ISaveLoad
{
	public bool m_PlayedCoop { get; private set; }

	public static ReplicatedSessionState Get()
	{
		return ReplicatedSessionState.s_Instance;
	}

	private void Awake()
	{
		ReplicatedSessionState.s_Instance = this;
	}

	private void OnDestroy()
	{
		ReplicatedSessionState.s_Instance = null;
	}

	public void Save()
	{
		if (ReplTools.AmIMaster())
		{
			this.m_SaveRequests++;
		}
		this.m_PlayedCoop |= !ReplTools.IsPlayingAlone();
		SaveGame.SaveVal("PlayedCoop", this.m_PlayedCoop);
		this.ReplSendAsap();
	}

	public void Load()
	{
		this.m_PlayedCoop = SaveGame.LoadBVal("PlayedCoop");
	}

	public override void OnReplicationResolve()
	{
		SaveGame.SaveCoop();
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		if (this.m_SaveRequests_Repl != this.m_SaveRequests)
		{
			this.m_SaveRequests_Repl = this.m_SaveRequests;
			this.ReplSetDirty();
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_SaveRequests_Repl);
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		this.m_SaveRequests_Repl = reader.ReadInt32();
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		this.m_SaveRequests = this.m_SaveRequests_Repl;
	}

	[Replicate(new string[]
	{

	})]
	[HideInInspector]
	private int m_SaveRequests;

	private static ReplicatedSessionState s_Instance;

	private int m_SaveRequests_Repl;
}
