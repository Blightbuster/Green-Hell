using System;
using System.Collections.ObjectModel;
using UnityEngine;

public static class ReplTools
{
	public static bool AmIMaster()
	{
		return P2PSession.Instance.AmIMaster();
	}

	public static P2PPeer GetLocalPeer()
	{
		return P2PSession.Instance.LocalPeer;
	}

	public static GameObject GetLocalReplicatedPlayer()
	{
		ReplicatedLogicalPlayer s_LocalLogicalPlayer = ReplicatedLogicalPlayer.s_LocalLogicalPlayer;
		if (s_LocalLogicalPlayer == null)
		{
			return null;
		}
		return s_LocalLogicalPlayer.gameObject;
	}

	public static ReadOnlyCollection<P2PPeer> GetRemotePeers()
	{
		return P2PSession.Instance.m_RemotePeers;
	}

	public static int GetRemotePeerCount()
	{
		return P2PSession.Instance.m_RemotePeers.Count;
	}

	public static bool IsPlayingAlone()
	{
		return P2PSession.Instance.m_RemotePeers.Count == 0 || P2PSession.Instance.IsAppQuitInProgress();
	}

	public static P2PPeer GetPeerById(short id)
	{
		return P2PSession.Instance.GetPeerById(id);
	}

	public static void ForEachPeerRepresentation(Action<IPeerWorldRepresentation> action, ReplTools.EPeerType enum_method = ReplTools.EPeerType.All)
	{
	}

	public static void ForEachLogicalPlayer(Action<ReplicatedLogicalPlayer> action, ReplTools.EPeerType enum_method = ReplTools.EPeerType.All)
	{
		for (int i = 0; i < ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count; i++)
		{
			ReplicatedLogicalPlayer replicatedLogicalPlayer = ReplicatedLogicalPlayer.s_AllLogicalPlayers[i];
			if (!(replicatedLogicalPlayer == null) && (enum_method != ReplTools.EPeerType.Remote || !(replicatedLogicalPlayer == ReplicatedLogicalPlayer.s_LocalLogicalPlayer)) && (enum_method != ReplTools.EPeerType.Local || !(replicatedLogicalPlayer != ReplicatedLogicalPlayer.s_LocalLogicalPlayer)))
			{
				action(replicatedLogicalPlayer);
			}
		}
	}

	public static bool ReplIsOwner(this MonoBehaviour component)
	{
		return true;
	}

	public static bool ReplIsOwner(this GameObject game_object)
	{
		return true;
	}

	public static P2PPeer ReplGetOwner(this GameObject game_object)
	{
		ReplicationComponent replComponentForGameObject = Replicator.Singleton.GetReplComponentForGameObject(game_object, false);
		return ((replComponentForGameObject != null) ? replComponentForGameObject.ReplGetOwner() : null) ?? null;
	}

	public static void ReplRequestOwnership(this MonoBehaviour component, bool search_parent = false)
	{
	}

	public static void ReplRequestOwnership(this GameObject game_object, bool search_parent = false)
	{
	}

	private static bool ReplRequestOwnershipInternal(GameObject game_object, bool search_parent = false)
	{
		ReplicationComponent replComponentForGameObject = Replicator.Singleton.GetReplComponentForGameObject(game_object, false);
		if (replComponentForGameObject != null)
		{
			replComponentForGameObject.ReplRequestOwnership();
			return true;
		}
		return search_parent && game_object.transform.parent != null && ReplTools.ReplRequestOwnershipInternal(game_object.transform.parent.gameObject, search_parent);
	}

	public static bool ReplIsReplicable(this MonoBehaviour component)
	{
		return false;
	}

	public static bool ReplIsReplicable(this GameObject game_object)
	{
		return false;
	}

	public static float ReplGetLastReplTime(this MonoBehaviour component)
	{
		return 0f;
	}

	public static bool ReplIsBeingDeserialized(this MonoBehaviour component, bool check_parent = false)
	{
		return false;
	}

	public static bool ReplIsBeingDeserialized(this GameObject game_object, bool check_parent = false)
	{
		return false;
	}

	public static void ReplSendAsap(this MonoBehaviour component)
	{
		ReplicationComponent replComponentForGameObject = Replicator.Singleton.GetReplComponentForGameObject(component.gameObject, false);
		if (replComponentForGameObject)
		{
			replComponentForGameObject.ReplSendAsap();
		}
	}

	public static void ReplSendAsap(this GameObject game_object)
	{
		ReplicationComponent replComponentForGameObject = Replicator.Singleton.GetReplComponentForGameObject(game_object, false);
		if (replComponentForGameObject)
		{
			replComponentForGameObject.ReplSendAsap();
		}
	}

	public enum EPeerType
	{
		All,
		Remote,
		Local
	}
}
