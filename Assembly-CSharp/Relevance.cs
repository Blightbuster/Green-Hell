using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Relevance : ReplicatedBehaviour
{
	public bool CanBeRemovedByRelevance(bool on_owner)
	{
		if (!this.m_CanBeRemovedByRelevance)
		{
			return false;
		}
		if (this.m_CustomCalculators != null)
		{
			for (int i = 0; i < this.m_CustomCalculators.Length; i++)
			{
				if (!this.m_CustomCalculators[i].CanBeRemovedByRelevance(on_owner))
				{
					return false;
				}
			}
		}
		return true;
	}

	public float LastUpdate { get; set; } = float.MinValue;

	public event Relevance.OnRelevanceActivatedDelegate OnRelevanceActivatedEvent;

	public event Relevance.OnRelevanceDeactivatedDelegate OnRelevanceDeactivatedEvent;

	public void OnRelevanceActivated(P2PPeer peer)
	{
		Relevance.OnRelevanceActivatedDelegate onRelevanceActivatedEvent = this.OnRelevanceActivatedEvent;
		if (onRelevanceActivatedEvent == null)
		{
			return;
		}
		onRelevanceActivatedEvent(peer);
	}

	public void OnRelevanceDeactivated(P2PPeer peer)
	{
		Relevance.OnRelevanceDeactivatedDelegate onRelevanceDeactivatedEvent = this.OnRelevanceDeactivatedEvent;
		if (onRelevanceDeactivatedEvent == null)
		{
			return;
		}
		onRelevanceDeactivatedEvent(peer);
	}

	public void OnPeerDisconnected(P2PPeer peer)
	{
		this.m_Relevance.Remove(peer);
	}

	private void Awake()
	{
		this.m_CustomCalculators = base.GetComponents<IRelevanceCalculator>();
		this.m_CreationTime = Time.time;
	}

	private void Start()
	{
		RelevanceSystem s_Instance = RelevanceSystem.s_Instance;
		if (s_Instance == null)
		{
			return;
		}
		s_Instance.RegisterComponent(this);
	}

	public override void ReplOnSpawned()
	{
		RelevanceSystem s_Instance = RelevanceSystem.s_Instance;
		if (s_Instance == null)
		{
			return;
		}
		s_Instance.RegisterComponent(this);
	}

	private void OnDestroy()
	{
		RelevanceSystem s_Instance = RelevanceSystem.s_Instance;
		if (s_Instance == null)
		{
			return;
		}
		s_Instance.UnregisterComponent(this);
	}

	public Vector3 GetRelevanceReferencePosition()
	{
		return base.transform.position;
	}

	public float GetMaxRelevanceRange()
	{
		return this.m_MaxRelevanceRange;
	}

	public float GetMinRelevanceRange()
	{
		return this.m_MinRelevanceRange;
	}

	public float GetRelevance(P2PPeer peer)
	{
		float result = 0f;
		this.m_Relevance.TryGetValue(peer, out result);
		return result;
	}

	public float GetMaxRemoteRelevance()
	{
		float num = 0f;
		foreach (KeyValuePair<P2PPeer, float> keyValuePair in this.m_Relevance)
		{
			if (keyValuePair.Key != ReplTools.GetLocalPeer() && keyValuePair.Value > num)
			{
				num = keyValuePair.Value;
			}
		}
		return num;
	}

	public float CalculateRelevance(P2PPeer peer, out Relevance.ERelevanceCalculationResult result)
	{
		if (peer.m_Representation == null)
		{
			result = Relevance.ERelevanceCalculationResult.Inactive;
			return 0f;
		}
		if (!base.GetReplicationComponent().enabled && !peer.IsLocalPeer())
		{
			result = Relevance.ERelevanceCalculationResult.Inactive;
			return 0f;
		}
		float b = peer.m_Representation.GetWorldPosition().Distance(this.GetRelevanceReferencePosition());
		float num = this.m_Relevance.ContainsKey(peer) ? this.m_Relevance[peer] : 0f;
		float num2 = -1f;
		for (int i = 0; i < this.m_CustomCalculators.Length; i++)
		{
			float num3 = this.m_CustomCalculators[i].CalculateRelevance(peer.m_Representation, peer == base.ReplGetOwner());
			if (num3 != -1f)
			{
				num2 = System.Math.Max(num3, num2);
			}
		}
		if (num2 == -1f)
		{
			num2 = CJTools.Math.GetProportionalClamp(1f, 0f, b, this.GetMaxRelevanceRange(), this.GetMinRelevanceRange());
		}
		if (num2 <= 0.1f)
		{
			if (num > 0.1f)
			{
				result = Relevance.ERelevanceCalculationResult.ToDeactivate;
			}
			else
			{
				result = Relevance.ERelevanceCalculationResult.Inactive;
			}
		}
		else if (num2 >= 0.2f)
		{
			if (num < 0.2f)
			{
				result = Relevance.ERelevanceCalculationResult.ToActivate;
			}
			else
			{
				result = Relevance.ERelevanceCalculationResult.Active;
			}
		}
		else
		{
			result = Relevance.ERelevanceCalculationResult.NoChange;
		}
		this.m_Relevance[peer] = num2;
		return num2;
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		if (was_owner)
		{
			float relevance = this.GetRelevance(ReplTools.GetLocalPeer());
			this.m_Relevance.Clear();
			this.m_Relevance[ReplTools.GetLocalPeer()] = relevance;
		}
		else
		{
			foreach (P2PPeer key in ReplTools.GetRemotePeers())
			{
				this.m_Relevance[key] = 0.2f;
			}
		}
		foreach (P2PPeer p2PPeer in ReplTools.GetRemotePeers())
		{
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("ReplOnChangedOwner is owner {0} {1} info {2} {3}", new object[]
				{
					!was_owner,
					base.GetComponent<GuidComponent>().GetGuid(),
					p2PPeer.GetHostId(),
					this.GetRelevance(p2PPeer)
				}));
			}
		}
	}

	private IRelevanceCalculator[] m_CustomCalculators;

	[NonSerialized]
	public Dictionary<P2PPeer, float> m_Relevance = new Dictionary<P2PPeer, float>();

	[SerializeField]
	private bool m_CanBeRemovedByRelevance = true;

	[SerializeField]
	private float m_MaxRelevanceRange = 5f;

	[SerializeField]
	private float m_MinRelevanceRange = 50f;

	[NonSerialized]
	public float m_CreationTime;

	public enum ERelevanceCalculationResult
	{
		NotNeeded,
		ToActivate,
		ToDeactivate,
		Active,
		Inactive,
		NoChange
	}

	public delegate void OnRelevanceActivatedDelegate(P2PPeer peer);

	public delegate void OnRelevanceDeactivatedDelegate(P2PPeer peer);
}
