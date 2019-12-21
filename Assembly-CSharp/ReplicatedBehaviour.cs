using System;
using UnityEngine;

[RequireComponent(typeof(ReplicationComponent))]
public class ReplicatedBehaviour : MonoBehaviour, IReplicatedBehaviour
{
	private ReplicationComponent ReplicationComponent
	{
		get
		{
			return this.m_ReplicationComponent.Get(this);
		}
	}

	public ReplicationComponent GetReplicationComponent()
	{
		return this.m_ReplicationComponent.Get(this);
	}

	public void ReplRequestOwnership()
	{
		this.ReplicationComponent.ReplRequestOwnership();
	}

	public void ReplGiveOwnership(P2PPeer peer)
	{
		this.ReplicationComponent.ReplGiveOwnership(peer);
	}

	public bool ReplCanChangeOwnership()
	{
		return this.ReplicationComponent.ReplCanChangeOwnership();
	}

	public void ReplBlockChangeOwnership(bool block)
	{
		this.ReplicationComponent.ReplBlockChangeOwnership(block);
	}

	public bool ReplIsOwner()
	{
		return this.ReplicationComponent.ReplIsOwner();
	}

	public P2PPeer ReplGetOwner()
	{
		return this.ReplicationComponent.ReplGetOwner();
	}

	public bool ReplIsTransferringOwnership()
	{
		return this.ReplicationComponent.IsTransferringOwnership();
	}

	public virtual void ReplOnChangedOwner(bool was_owner)
	{
	}

	public virtual void ReplOnSpawned()
	{
	}

	public virtual void OnReplicationPrepare()
	{
	}

	public virtual void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
	}

	public virtual void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
	}

	public virtual void OnReplicationResolve()
	{
	}

	private CachedComponent<ReplicationComponent> m_ReplicationComponent;
}
