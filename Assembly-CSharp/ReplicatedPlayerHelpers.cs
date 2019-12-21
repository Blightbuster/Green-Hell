using System;
using UnityEngine;

public static class ReplicatedPlayerHelpers
{
	public static T GetPlayerComponent<T>(this GameObject player_object) where T : MonoBehaviour
	{
		if (player_object == Player.Get().gameObject)
		{
			return ReplicatedLogicalPlayer.s_LocalLogicalPlayer.GetPlayerComponent<T>();
		}
		P2PPeer p2PPeer = player_object.ReplGetOwner();
		DebugUtils.Assert(p2PPeer != null, true);
		foreach (ReplicatedLogicalPlayer replicatedLogicalPlayer in ReplicatedLogicalPlayer.s_AllLogicalPlayers)
		{
			if (replicatedLogicalPlayer.ReplGetOwner() == p2PPeer)
			{
				return replicatedLogicalPlayer.GetPlayerComponent<T>();
			}
		}
		return default(T);
	}

	public static T GetPlayerComponent<T>(this MonoBehaviour player_component) where T : MonoBehaviour
	{
		return player_component.gameObject.GetPlayerComponent<T>();
	}
}
