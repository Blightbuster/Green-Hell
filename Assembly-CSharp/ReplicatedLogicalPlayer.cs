using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ReplicatedLogicalPlayer : ReplicatedBehaviour, IPeerWorldRepresentation
{
	public static ReplicatedLogicalPlayer s_LocalLogicalPlayer { get; private set; }

	public static List<ReplicatedLogicalPlayer> s_AllLogicalPlayers { get; } = new List<ReplicatedLogicalPlayer>();

	private void Awake()
	{
		base.ReplBlockChangeOwnership(true);
		this.RegisterForPeer(base.ReplGetOwner());
		ReplicatedLogicalPlayer.s_AllLogicalPlayers.Add(this);
		if (base.ReplIsOwner())
		{
			ReplicatedLogicalPlayer.s_LocalLogicalPlayer = this;
		}
		foreach (Component component in base.GetComponents<Component>())
		{
			if (component != null)
			{
				this.m_Behaviours.Add(component.GetType(), component);
			}
		}
	}

	private void OnDestroy()
	{
		this.UnregisterForPeer(base.ReplGetOwner());
		ReplicatedLogicalPlayer.s_AllLogicalPlayers.Remove(this);
		if (base.ReplIsOwner())
		{
			ReplicatedLogicalPlayer.s_LocalLogicalPlayer = null;
		}
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		DebugUtils.Assert(false, true);
	}

	public Vector3 GetWorldPosition()
	{
		return base.transform.position;
	}

	public bool IsReplicated()
	{
		return true;
	}

	public T GetPlayerComponent<T>() where T : Component
	{
		Component component;
		if (this.m_Behaviours.TryGetValue(typeof(T), out component))
		{
			return (T)((object)component);
		}
		return default(T);
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		if (this.m_PauseGame_Repl != this.m_PauseGame)
		{
			this.m_PauseGame_Repl = this.m_PauseGame;
			this.ReplSetDirty();
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_PauseGame_Repl);
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		this.m_PauseGame_Repl = reader.ReadBoolean();
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		this.m_PauseGame = this.m_PauseGame_Repl;
	}

	private Dictionary<Type, Component> m_Behaviours = new Dictionary<Type, Component>();

	[Replicate(new string[]
	{

	})]
	[HideInInspector]
	public bool m_PauseGame;

	private bool m_PauseGame_Repl;
}
