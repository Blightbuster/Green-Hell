using System;
using System.Collections.Generic;
using UnityEngine;

public static class IReplicatedBehaviourExtension
{
	public static ReplicationComponent GetReplicationComponent(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicatedBehaviour replicatedBehaviour = repl_behaviour as ReplicatedBehaviour;
		if (replicatedBehaviour == null)
		{
			return Replicator.Singleton.GetReplComponentForGameObject(((MonoBehaviour)repl_behaviour).gameObject, true);
		}
		return replicatedBehaviour.GetReplicationComponent();
	}

	public static void ReplGiveOwnership(this IReplicatedBehaviour repl_behaviour, P2PPeer peer)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return;
		}
		replicationComponent.ReplGiveOwnership(peer);
	}

	public static bool ReplCanChangeOwnership(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		return replicationComponent != null && replicationComponent.ReplCanChangeOwnership();
	}

	public static void ReplBlockChangeOwnership(this IReplicatedBehaviour repl_behaviour, bool block)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return;
		}
		replicationComponent.ReplBlockChangeOwnership(block);
	}

	public static P2PPeer ReplGetOwner(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return null;
		}
		return replicationComponent.ReplGetOwner();
	}

	public static bool ReplIsTransferringOwnership(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		return replicationComponent != null && replicationComponent.IsTransferringOwnership();
	}

	public static float ReplGetReplicationInterval(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return 0f;
		}
		return replicationComponent.ReplGetReplicationInterval(ReplTools.GetLocalPeer());
	}

	public static void ReplSetDirty(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return;
		}
		replicationComponent.ReplSetDirty(repl_behaviour, true);
	}

	public static bool ReplIsDirty(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		return replicationComponent != null && replicationComponent.ReplIsDirty(repl_behaviour);
	}

	public static void ReplClearDirty(this IReplicatedBehaviour repl_behaviour)
	{
		ReplicationComponent replicationComponent = repl_behaviour.GetReplicationComponent();
		if (replicationComponent == null)
		{
			return;
		}
		replicationComponent.ReplSetDirty(repl_behaviour, false);
	}

	public static int GetUniqueIdForType(this IReplicatedBehaviour repl_behaviour)
	{
		int hashCode;
		if (!IReplicatedBehaviourExtension.s_ReplBehaviourUniqueIds.TryGetValue(repl_behaviour.GetType(), out hashCode))
		{
			hashCode = repl_behaviour.GetType().FullName.GetHashCode();
			IReplicatedBehaviourExtension.s_ReplBehaviourUniqueIds[repl_behaviour.GetType()] = hashCode;
		}
		return hashCode;
	}

	public static Type GetTypeFromUniqueId(int unique_id)
	{
		Type result = null;
		Dictionary<Type, int>.Enumerator enumerator = IReplicatedBehaviourExtension.s_ReplBehaviourUniqueIds.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Type, int> keyValuePair = enumerator.Current;
			if (keyValuePair.Value == unique_id)
			{
				keyValuePair = enumerator.Current;
				result = keyValuePair.Key;
				break;
			}
		}
		enumerator.Dispose();
		return result;
	}

	private static Dictionary<Type, int> s_ReplBehaviourUniqueIds = new Dictionary<Type, int>();
}
