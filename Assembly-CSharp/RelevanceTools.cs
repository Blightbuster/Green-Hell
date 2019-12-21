using System;
using UnityEngine;

public class RelevanceTools
{
	public static float GetDistToLocalPlayer(GameObject obj)
	{
		if (ReplTools.GetLocalPeer().m_Representation != null)
		{
			return ReplTools.GetLocalPeer().m_Representation.GetWorldPosition().Distance(obj.transform.position);
		}
		return -1f;
	}

	public static float GetDistToClosestPlayer(GameObject obj)
	{
		float num = float.MaxValue;
		if (ReplTools.GetLocalPeer().m_Representation != null)
		{
			num = ReplTools.GetLocalPeer().m_Representation.GetWorldPosition().Distance(obj.transform.position);
		}
		for (int i = 0; i < ReplTools.GetRemotePeers().Count; i++)
		{
			P2PPeer p2PPeer = ReplTools.GetRemotePeers()[i];
			if (p2PPeer != null && p2PPeer.m_Representation != null)
			{
				float num2 = p2PPeer.m_Representation.GetWorldPosition().Distance(obj.transform.position);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public static float GetDistToClosestPlayer2D(GameObject obj)
	{
		float num = float.MaxValue;
		if (ReplTools.GetLocalPeer().m_Representation != null)
		{
			num = ReplTools.GetLocalPeer().m_Representation.GetWorldPosition().Distance2D(obj.transform.position);
		}
		for (int i = 0; i < ReplTools.GetRemotePeers().Count; i++)
		{
			P2PPeer p2PPeer = ReplTools.GetRemotePeers()[i];
			if (p2PPeer != null && p2PPeer.m_Representation != null)
			{
				float num2 = p2PPeer.m_Representation.GetWorldPosition().Distance2D(obj.transform.position);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public const float RELEVANCE_MAX = 1f;

	public const float RELEVANCE_MIN = 0f;

	public const float RELEVANCE_UNKNOWN = -1f;

	public const float RELEVANCE_ACTIVATE = 0.2f;

	public const float RELEVANCE_DEACTIVATE = 0.1f;

	public const float RELEVANCE_DIFF_TO_GIVE_OWNERSHIP = 0.1f;

	public const float MAX_RELEVANCE_RANGE_DEFAULT = 5f;

	public const float MIN_RELEVANCE_RANGE_DEFAULT = 50f;
}
