using System;
using System.Collections.Generic;
using UnityEngine;

public class RelevanceSystem : MonoBehaviour
{
	private void Awake()
	{
		DebugUtils.Assert(RelevanceSystem.s_Instance == null, true);
		RelevanceSystem.s_Instance = this;
		P2PSession.Instance.RegisterHandler(33, new P2PNetworkMessageDelegate(this.OnPeerDisconnect));
	}

	private void OnPeerDisconnect(P2PNetworkMessage net_msg)
	{
		P2PConnection connection = net_msg.m_Connection;
		P2PPeer p2PPeer = (connection != null) ? connection.m_Peer : null;
		if (p2PPeer != null)
		{
			Dictionary<GameObject, Relevance>.Enumerator enumerator = this.m_Components.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<GameObject, Relevance> keyValuePair = enumerator.Current;
				Relevance value = keyValuePair.Value;
				if (value)
				{
					value.OnPeerDisconnected(p2PPeer);
				}
			}
			enumerator.Dispose();
		}
	}

	private void LateUpdate()
	{
		if (!RelevanceSystem.ENABLED)
		{
			return;
		}
		Dictionary<GameObject, Relevance>.Enumerator enumerator = this.m_Components.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<GameObject, Relevance> keyValuePair = enumerator.Current;
			Relevance value = keyValuePair.Value;
			if (value && value.enabled && value.LastUpdate <= Time.time - 0.5f)
			{
				value.LastUpdate = Time.time + UnityEngine.Random.Range(0f, 0.2f);
				Relevance.ERelevanceCalculationResult erelevanceCalculationResult;
				float num = value.CalculateRelevance(ReplTools.GetLocalPeer(), out erelevanceCalculationResult);
				if (num >= 0.2f && !value.gameObject.activeSelf)
				{
					value.gameObject.SetActive(true);
					value.OnRelevanceActivated(ReplTools.GetLocalPeer());
				}
				bool flag = false;
				if (value.ReplIsOwner())
				{
					bool flag2 = value.ReplCanChangeOwnership() && !value.ReplIsTransferringOwnership() && value.m_CreationTime < Time.time - 1f;
					P2PPeer p2PPeer = null;
					float num2 = 0f;
					for (int i = 0; i < ReplTools.GetRemotePeers().Count; i++)
					{
						P2PPeer p2PPeer2 = ReplTools.GetRemotePeers()[i];
						float num3 = value.CalculateRelevance(p2PPeer2, out erelevanceCalculationResult);
						if (erelevanceCalculationResult == Relevance.ERelevanceCalculationResult.ToActivate)
						{
							value.OnRelevanceActivated(p2PPeer2);
						}
						else if (erelevanceCalculationResult == Relevance.ERelevanceCalculationResult.ToDeactivate)
						{
							value.OnRelevanceDeactivated(p2PPeer2);
						}
						if (flag2 && num3 > num2 && (num <= 0f || num3 > num + 0.1f))
						{
							num2 = num3;
							p2PPeer = p2PPeer2;
						}
					}
					if (p2PPeer != null)
					{
						value.ReplGiveOwnership(p2PPeer);
					}
					if (num <= 0f && value.CanBeRemovedByRelevance(true) && !value.ReplIsTransferringOwnership())
					{
						UnityEngine.Object.Destroy(value.gameObject);
						flag = true;
					}
				}
				if (!flag && num <= 0.1f && value.gameObject.activeSelf)
				{
					value.gameObject.SetActive(false);
					value.OnRelevanceDeactivated(ReplTools.GetLocalPeer());
				}
			}
		}
		enumerator.Dispose();
	}

	public void RegisterComponent(Relevance component)
	{
		this.m_Components[component.gameObject] = component;
	}

	public void UnregisterComponent(Relevance component)
	{
		this.m_Components.Remove(component.gameObject);
	}

	public void SetRelevanceEnabled(GameObject obj, bool enabled)
	{
		Relevance relevance;
		if (this.m_Components.TryGetValue(obj, out relevance))
		{
			relevance.enabled = enabled;
		}
	}

	public void RegisterQuadTree(QuadTree quad_tree, GameObject game_object)
	{
	}

	public void UnregisterQuadTree(QuadTree quad_tree, GameObject game_object)
	{
	}

	public static readonly bool ENABLED;

	public static RelevanceSystem s_Instance;

	private Dictionary<GameObject, Relevance> m_Components = new Dictionary<GameObject, Relevance>(2000);

	private const float RELEVANCE_UPDATE_INTERVAL = 0.5f;

	private const float OWNERSHIP_CHANGE_AFTER_CREATION_MIN_TIME = 1f;
}
