using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(GuidComponent))]
public sealed class ReplicationComponent : MonoBehaviour
{
	public GuidComponent GetGuidComponent
	{
		get
		{
			return this.m_GuidComponentInternal.Get(this);
		}
	}

	public Relevance GetRelevanceComponent
	{
		get
		{
			return this.m_RelevanceComponentInternal.Get(this);
		}
	}

	private IReplicatedBehaviour[] GetReplBehaviours
	{
		get
		{
			if (this.m_ReplBehavioursInternal == null)
			{
				this.m_ReplBehavioursInternal = base.GetComponents<IReplicatedBehaviour>();
				this.m_ReplBehavioursFlag = new bool[this.m_ReplBehavioursInternal.Length];
				this.m_DirtyBit = new bool[this.m_ReplBehavioursInternal.Length];
			}
			return this.m_ReplBehavioursInternal;
		}
	}

	public bool IsTransferringOwnership()
	{
		return this.ReplIsOwner() && this.m_TransferringOwnershipTo != null;
	}

	public bool IsTransferringOwnershipTo(P2PPeer peer)
	{
		return this.ReplIsOwner() && this.m_TransferringOwnershipTo == peer;
	}

	public float GetReplicationIntervalMin()
	{
		float num = this.m_ReplicationIntervalMin;
		ICustomReplicationInterval[] customReplicationIntervalComponents = this.m_CustomReplicationIntervalComponents;
		for (int i = 0; i < customReplicationIntervalComponents.Length; i++)
		{
			float replicationIntervalMin = customReplicationIntervalComponents[i].GetReplicationIntervalMin();
			if (replicationIntervalMin > 0f && replicationIntervalMin < num)
			{
				num = replicationIntervalMin;
			}
		}
		return num;
	}

	public float GetReplicationIntervalMax()
	{
		float num = this.m_ReplicationIntervalMax;
		ICustomReplicationInterval[] customReplicationIntervalComponents = this.m_CustomReplicationIntervalComponents;
		for (int i = 0; i < customReplicationIntervalComponents.Length; i++)
		{
			float replicationIntervalMax = customReplicationIntervalComponents[i].GetReplicationIntervalMax();
			if (replicationIntervalMax > 0f && replicationIntervalMax < num)
			{
				num = replicationIntervalMax;
			}
		}
		return num;
	}

	public bool ReplCanChangeOwnership()
	{
		return !this.m_IsOwnershipBlocked;
	}

	public void ReplBlockChangeOwnership(bool block)
	{
		this.m_IsOwnershipBlocked = block;
	}

	public bool ReplIsOwner()
	{
		return (this.m_OwnerPeer == null && ReplTools.IsPlayingAlone()) || this.m_OwnerPeer == ReplTools.GetLocalPeer();
	}

	public bool ReplIsReplicable()
	{
		return true;
	}

	public P2PPeer ReplGetOwner()
	{
		return this.m_OwnerPeer;
	}

	public bool ReplWasSpawned()
	{
		return this.AssetId.IsValid();
	}

	public void ReplRequestOwnership()
	{
		if (this.ReplIsOwner() || (!this.ReplCanChangeOwnership() && this.ReplGetOwner() != P2PPeer.s_Invalid))
		{
			return;
		}
		this.m_TransferringOwnershipTo = null;
		this.ReplOnChangedOwner(ReplTools.GetLocalPeer());
		if (!ReplTools.IsPlayingAlone())
		{
			ReplicationComponent.s_Writer.StartMessage(6);
			ReplicationComponent.s_Writer.Write(base.gameObject);
			ReplicationComponent.s_Writer.Write(this.m_OwnerPeer.GetHostId());
			ReplicationComponent.s_Writer.FinishMessage();
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("ReplRequestOwnership called for {0} guid: {1}", base.name, this.GetGuidComponent.GetGuid()), base.gameObject);
			}
			P2PSession.Instance.SendWriterToAll(ReplicationComponent.s_Writer, 0);
		}
	}

	public void ReplGiveOwnership(P2PPeer peer)
	{
		if (!this.ReplIsOwner() && this.ReplGetOwner() != P2PPeer.s_Invalid)
		{
			return;
		}
		if (ReplTools.IsPlayingAlone())
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Trying to give ownership ower " + base.gameObject.name + " while playing alone!", this);
			}
			return;
		}
		ReplicationComponent.s_Writer.StartMessage(7);
		ReplicationComponent.s_Writer.Write(base.gameObject);
		ReplicationComponent.s_Writer.Write(peer.GetHostId());
		ReplicationComponent.s_Writer.FinishMessage();
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("ReplGiveOwnership to {0} called for {1} guid: {2}", peer.GetHostId(), base.name, this.GetGuidComponent.GetGuid()), base.gameObject);
		}
		P2PSession.Instance.SendWriterTo(peer, ReplicationComponent.s_Writer, 0);
		this.m_TransferringOwnershipTo = peer;
	}

	public void ReplOnChangedOwner(P2PPeer peer)
	{
		if (this.m_OwnerPeer != peer)
		{
			bool flag = this.m_OwnerPeer.IsValid();
			bool was_owner = this.m_OwnerPeer.IsLocalPeer();
			this.m_OwnerPeer = peer;
			if (this.m_TransferringOwnershipTo != null)
			{
				this.m_TransferringOwnershipTo = null;
			}
			if (flag)
			{
				IReplicatedBehaviour[] getReplBehaviours = this.GetReplBehaviours;
				for (int i = 0; i < getReplBehaviours.Length; i++)
				{
					getReplBehaviours[i].ReplOnChangedOwner(was_owner);
				}
			}
		}
	}

	public void ReplOnSpawned()
	{
		IReplicatedBehaviour[] getReplBehaviours = this.GetReplBehaviours;
		for (int i = 0; i < getReplBehaviours.Length; i++)
		{
			getReplBehaviours[i].ReplOnSpawned();
		}
	}

	public float GetLastReplicationTime()
	{
		return this.m_LastReplicationTime;
	}

	private float CalculateReplicationInterval(P2PPeer peer)
	{
		return this.CalculateReplicationInterval(this.GetRelevanceComponent.GetRelevance(peer));
	}

	private float CalculateReplicationInterval(float relevance)
	{
		return CJTools.Math.GetProportionalClamp(this.GetReplicationIntervalMax(), this.GetReplicationIntervalMin(), relevance, 0f, 1f);
	}

	private float CalculateMinReplicationInterval()
	{
		if (this.GetRelevanceComponent)
		{
			return this.CalculateReplicationInterval(this.GetRelevanceComponent.GetMaxRemoteRelevance());
		}
		return this.GetReplicationIntervalMin();
	}

	public float ReplGetReplicationInterval(P2PPeer peer)
	{
		if (!(this.GetRelevanceComponent != null))
		{
			return this.GetReplicationIntervalMin();
		}
		if (peer == ReplTools.GetLocalPeer())
		{
			if (this.m_LocalReplicationIntervalUpdateTime < Time.time)
			{
				this.m_LocalReplicationIntervalNow = this.CalculateReplicationInterval(peer);
				this.m_LocalReplicationIntervalUpdateTime = Time.time;
			}
			return this.m_LocalReplicationIntervalNow;
		}
		return this.CalculateReplicationInterval(peer);
	}

	public void ReplSetDirty(IReplicatedBehaviour repl_behaviour, bool dirty)
	{
		if (this.m_ReplBehavioursInternal == null || this.m_ReplBehavioursInternal.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this.GetReplBehaviours.Length; i++)
		{
			if (this.GetReplBehaviours[i] == repl_behaviour)
			{
				this.m_DirtyBit[i] = dirty;
				return;
			}
		}
	}

	public bool ReplIsDirty(IReplicatedBehaviour repl_behaviour)
	{
		if (this.GetReplBehaviours == null)
		{
			return false;
		}
		for (int i = 0; i < this.GetReplBehaviours.Length; i++)
		{
			if (this.GetReplBehaviours[i] == repl_behaviour)
			{
				return this.m_DirtyBit[i];
			}
		}
		return false;
	}

	public int GetComponentIndex(IReplicatedBehaviour repl_behaviour)
	{
		for (int i = 0; i < this.GetReplBehaviours.Length; i++)
		{
			if (this.GetReplBehaviours[i] == repl_behaviour)
			{
				return i;
			}
		}
		DebugUtils.Assert(false, true);
		return -1;
	}

	public IReplicatedBehaviour GetComponentFromIndex(int repl_behaviour_idx)
	{
		if (repl_behaviour_idx >= 0 && repl_behaviour_idx < this.GetReplBehaviours.Length)
		{
			return this.GetReplBehaviours[repl_behaviour_idx];
		}
		DebugUtils.Assert(false, true);
		return null;
	}

	public byte[] Serialize(bool initial_state)
	{
		ReplicationComponent.s_Writer.SeekZero();
		ReplicationComponent.s_Writer.Write(this.m_Revision);
		ReplicationComponent.s_Writer.Write((this.m_OwnerPeer != null) ? this.m_OwnerPeer.GetHostId() : P2PPeer.s_InvalidId);
		ReplicationComponent.s_Writer.Write(this.ReplCanChangeOwnership());
		foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
		{
			ReplicationComponent.s_Writer.Write(true);
			ReplicationComponent.s_Writer.Write(replicatedBehaviour.GetUniqueIdForType());
			replicatedBehaviour.OnReplicationPrepare();
			this.CallReplicationPrepare_Gen(replicatedBehaviour);
			replicatedBehaviour.OnReplicationSerialize(ReplicationComponent.s_Writer, initial_state);
			this.CallReplicationSerialize_Gen(replicatedBehaviour, ReplicationComponent.s_Writer, initial_state);
		}
		return ReplicationComponent.s_Writer.AsArray();
	}

	public void Deserialize(byte[] payload, bool initialState)
	{
		byte[] buffer = this.m_Reader.Replace(payload);
		this.ReplicationReceive(this.m_Reader, initialState);
		this.m_Reader.Replace(buffer);
	}

	private bool ReplicationSend(bool initial_state, ReplicationComponent.SendToPeers peers = null)
	{
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log("ReplicationSend called for " + base.name, this);
		}
		int num = 0;
		float time = Time.time;
		ReplicationComponent.s_Writer.StartMessage(1);
		ReplicationComponent.s_Writer.Write(base.gameObject);
		ReplicationComponent.s_Writer.Write(this.m_Revision + 1);
		ReplicationComponent.s_Writer.Write(this.m_OwnerPeer.GetHostId());
		ReplicationComponent.s_Writer.Write(this.ReplCanChangeOwnership());
		short position = ReplicationComponent.s_Writer.Position;
		foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
		{
			if (replicatedBehaviour as UnityEngine.Object == null)
			{
				ReplicationComponent.s_Writer.Write(false);
			}
			else
			{
				replicatedBehaviour.OnReplicationPrepare();
				this.CallReplicationPrepare_Gen(replicatedBehaviour);
				if (initial_state || this.ReplIsDirty(replicatedBehaviour))
				{
					ReplicationComponent.s_Writer.Write(true);
					ReplicationComponent.s_Writer.Write(replicatedBehaviour.GetUniqueIdForType());
					replicatedBehaviour.OnReplicationSerialize(ReplicationComponent.s_Writer, initial_state);
					this.CallReplicationSerialize_Gen(replicatedBehaviour, ReplicationComponent.s_Writer, initial_state);
					if (!initial_state)
					{
						this.ReplSetDirty(replicatedBehaviour, false);
					}
				}
				else
				{
					ReplicationComponent.s_Writer.Write(false);
				}
			}
		}
		bool flag = (int)(ReplicationComponent.s_Writer.Position - position) > this.GetReplBehaviours.Length;
		if (flag)
		{
			this.m_Revision++;
		}
		ReplicationComponent.s_Writer.FinishMessage();
		if (flag)
		{
			if (peers == null)
			{
				P2PSession.Instance.SendWriterToAll(ReplicationComponent.s_Writer, num);
			}
			else
			{
				for (int j = 0; j < peers.count; j++)
				{
					P2PSession.Instance.SendWriterTo(peers.peers[j], ReplicationComponent.s_Writer, num);
				}
			}
		}
		return true;
	}

	public void ReplicationReceive(P2PNetworkReader reader, bool initial_state)
	{
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("ReplicationReceive called for {0} with guid {1}", base.name, this.GetGuidComponent.GetGuid()), this);
		}
		int num = reader.ReadInt32();
		if (num <= this.m_Revision && !initial_state)
		{
			return;
		}
		ReplicationComponent.s_DeserializedComponent = this;
		this.m_Revision = num;
		this.m_LastReplicationTime = Time.time;
		this.m_LastReplicationTimeReal = Time.realtimeSinceStartup;
		short num2 = reader.ReadInt16();
		if (this.m_OwnerPeer.GetHostId() != num2)
		{
			this.ReplOnChangedOwner(ReplTools.GetPeerById(num2));
			if (this.m_OwnerPeer == ReplTools.GetLocalPeer())
			{
				this.ReplRequestOwnership();
			}
		}
		this.ReplBlockChangeOwnership(!reader.ReadBoolean());
		for (int i = 0; i < this.GetReplBehaviours.Length; i++)
		{
			IReplicatedBehaviour replicatedBehaviour = this.GetReplBehaviours[i];
			if (replicatedBehaviour as UnityEngine.Object == null)
			{
				if (reader.ReadBoolean())
				{
					ReplicationComponent.s_DeserializedComponent = null;
					return;
				}
				this.m_ReplBehavioursFlag[i] = false;
			}
			else if (reader.ReadBoolean())
			{
				int uniqueIdForType = replicatedBehaviour.GetUniqueIdForType();
				int num3 = reader.ReadInt32();
				if (uniqueIdForType != num3)
				{
					ReplicationComponent.s_DeserializedComponent = null;
					return;
				}
				replicatedBehaviour.OnReplicationDeserialize(reader, initial_state);
				this.CallReplicationDeserialize_Gen(replicatedBehaviour, reader, initial_state);
				this.m_ReplBehavioursFlag[i] = true;
			}
			else
			{
				this.m_ReplBehavioursFlag[i] = false;
			}
		}
		for (int j = 0; j < this.GetReplBehaviours.Length; j++)
		{
			if (this.m_ReplBehavioursFlag[j])
			{
				IReplicatedBehaviour replicatedBehaviour2 = this.GetReplBehaviours[j];
				replicatedBehaviour2.OnReplicationResolve();
				this.CallReplicationResolve_Gen(replicatedBehaviour2);
			}
		}
		if (P2PConnection.s_Size > reader.Position && !initial_state)
		{
			Debug.LogError(string.Format("Didn't read whole buffer! (number of bytes unread: {0})", P2PConnection.s_Size - reader.Position), this);
		}
		ReplicationComponent.s_DeserializedComponent = null;
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Replicator.Singleton.RegisterReplicationComponent(this);
			if (!this.ReplGetOwner().IsValid() && this.ReplWasSpawned())
			{
				this.m_OwnerPeer = ReplTools.GetLocalPeer();
			}
			this.m_CustomReplicationIntervalComponents = base.GetComponents<ICustomReplicationInterval>();
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			if (this.ReplWasSpawned() && this.ReplIsOwner() && !this.GetRelevanceComponent && base.enabled)
			{
				Replicator.Singleton.SpawnDelayed(base.gameObject);
			}
			if (this.GetRelevanceComponent)
			{
				this.GetRelevanceComponent.OnRelevanceActivatedEvent += this.OnRelevanceActivated;
				this.GetRelevanceComponent.OnRelevanceDeactivatedEvent += this.OnRelevanceDeactivated;
			}
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("OnDestroy called for {0} with guid {1} is owner: {2}", base.name, this.GetGuidComponent.GetGuid(), this.ReplIsOwner()));
			}
			if (this.ReplIsOwner())
			{
				if (!this.GetRelevanceComponent)
				{
					Replicator.Singleton.Despawn(this);
				}
				else
				{
					if (P2PLogFilter.logPedantic)
					{
						Debug.Log(string.Format("OnDestroy info {0}", this.GetRelevanceComponent.m_Relevance.Count));
					}
					foreach (KeyValuePair<P2PPeer, float> keyValuePair in this.GetRelevanceComponent.m_Relevance)
					{
						if (P2PLogFilter.logPedantic)
						{
							Debug.Log(string.Format("OnDestroy {0} info {1} {2}", this.GetGuidComponent.GetGuid(), keyValuePair.Key.GetHostId(), keyValuePair.Value));
						}
						if (!keyValuePair.Key.IsLocalPeer() && keyValuePair.Value > 0f)
						{
							Replicator.Singleton.DespawnForPeer(this, keyValuePair.Key);
						}
					}
				}
			}
			Replicator.Singleton.UnregisterReplicationComponent(this);
			if (this.GetRelevanceComponent)
			{
				this.GetRelevanceComponent.OnRelevanceActivatedEvent -= this.OnRelevanceActivated;
				this.GetRelevanceComponent.OnRelevanceDeactivatedEvent -= this.OnRelevanceDeactivated;
			}
		}
	}

	public void Update()
	{
	}

	public void OnReplicatorUpdate(bool force_update = false)
	{
		if (!base.enabled || !base.gameObject.activeSelf)
		{
			return;
		}
		if (!this.ReplIsOwner() && (this.ReplGetOwner().IsValid() || !ReplTools.AmIMaster()))
		{
			return;
		}
		if (force_update || this.m_LastReplicationTimeReal < Time.realtimeSinceStartup - this.GetReplicationIntervalMin())
		{
			this.m_LastReplicationTime = Time.time;
			this.m_LastReplicationTimeReal = Time.realtimeSinceStartup;
			if (this.GetRelevanceComponent)
			{
				ReplicationComponent.s_SendToPeers.Reset();
				foreach (KeyValuePair<P2PPeer, float> keyValuePair in this.GetRelevanceComponent.m_Relevance)
				{
					P2PPeer key = keyValuePair.Key;
					if (!key.IsLocalPeer() && keyValuePair.Value > 0.1f)
					{
						float num = 0f;
						this.m_LastPeerReplicationTime.TryGetValue(key, out num);
						if (num + this.CalculateReplicationInterval(keyValuePair.Key) < Time.time)
						{
							this.m_LastPeerReplicationTime[key] = Time.time;
							ReplicationComponent.s_SendToPeers.Add(key);
						}
					}
				}
				if (ReplicationComponent.s_SendToPeers.count > 0)
				{
					this.ReplicationSend(false, ReplicationComponent.s_SendToPeers);
					return;
				}
			}
			else
			{
				this.ReplicationSend(false, null);
			}
		}
	}

	private void OnRelevanceActivated(P2PPeer peer)
	{
		if (this.ReplIsOwner() && peer != ReplTools.GetLocalPeer())
		{
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("OnRelevanceActivated called for peer {0} {1} guid {2}", peer.GetHostId(), base.name, this.GetGuidComponent.GetGuid()));
			}
			Replicator.Singleton.SpawnForPeer(base.gameObject, peer);
		}
	}

	private void OnRelevanceDeactivated(P2PPeer peer)
	{
		if (this.ReplIsOwner() && peer != ReplTools.GetLocalPeer())
		{
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("OnRelevanceDeactivated called for peer {0} {1} guid {2} CanBeRemovedByRelevance {3}", new object[]
				{
					peer.GetHostId(),
					base.name,
					this.GetGuidComponent.GetGuid(),
					this.GetRelevanceComponent.CanBeRemovedByRelevance(false)
				}));
			}
			if (this.GetRelevanceComponent.CanBeRemovedByRelevance(false))
			{
				Replicator.Singleton.DespawnForPeer(this, peer);
				return;
			}
			ReplicationComponent.s_SendToPeers.Reset();
			ReplicationComponent.s_SendToPeers.Add(peer);
			this.ReplicationSend(false, ReplicationComponent.s_SendToPeers);
		}
	}

	public void ReplSendAsap()
	{
		this.m_LastReplicationTimeReal = -1f;
		this.m_LastPeerReplicationTime.Clear();
	}

	public P2PNetworkHash128 AssetId
	{
		get
		{
			return this.m_AssetId;
		}
	}

	private void CallReplicationPrepare_Gen(IReplicatedBehaviour repl_behaviour)
	{
		if (this.m_ReplicationPrepareDel == null)
		{
			foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
			{
				Action reflectedMethod = replicatedBehaviour.GetReflectedMethod(ReplicationComponentReflection.ReflectedMethodType.OnReplicationPrepare_CJGenerated);
				if (reflectedMethod != null)
				{
					if (this.m_ReplicationPrepareDel == null)
					{
						this.m_ReplicationPrepareDel = new Dictionary<IReplicatedBehaviour, Action>();
					}
					this.m_ReplicationPrepareDel[replicatedBehaviour] = reflectedMethod;
				}
			}
		}
		Action action;
		if (this.m_ReplicationPrepareDel != null && this.m_ReplicationPrepareDel.TryGetValue(repl_behaviour, out action))
		{
			action();
		}
	}

	private void CallReplicationSerialize_Gen(IReplicatedBehaviour repl_behaviour, P2PNetworkWriter writer, bool initial_state)
	{
		if (this.m_ReplicationSerializeDel == null)
		{
			foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
			{
				Action<P2PNetworkWriter, bool> reflectedMethod = replicatedBehaviour.GetReflectedMethod(ReplicationComponentReflection.ReflectedMethodType.OnReplicationSerialize_CJGenerated);
				if (reflectedMethod != null)
				{
					if (this.m_ReplicationSerializeDel == null)
					{
						this.m_ReplicationSerializeDel = new Dictionary<IReplicatedBehaviour, Action<P2PNetworkWriter, bool>>();
					}
					this.m_ReplicationSerializeDel[replicatedBehaviour] = reflectedMethod;
				}
			}
		}
		Action<P2PNetworkWriter, bool> action;
		if (this.m_ReplicationSerializeDel != null && this.m_ReplicationSerializeDel.TryGetValue(repl_behaviour, out action))
		{
			action(writer, initial_state);
		}
	}

	private void CallReplicationDeserialize_Gen(IReplicatedBehaviour repl_behaviour, P2PNetworkReader reader, bool initial_state)
	{
		if (this.m_ReplicationDeserializeDel == null)
		{
			foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
			{
				Action<P2PNetworkReader, bool> reflectedMethod = replicatedBehaviour.GetReflectedMethod(ReplicationComponentReflection.ReflectedMethodType.OnReplicationDeserialize_CJGenerated);
				if (reflectedMethod != null)
				{
					if (this.m_ReplicationDeserializeDel == null)
					{
						this.m_ReplicationDeserializeDel = new Dictionary<IReplicatedBehaviour, Action<P2PNetworkReader, bool>>();
					}
					this.m_ReplicationDeserializeDel[replicatedBehaviour] = reflectedMethod;
				}
			}
		}
		Action<P2PNetworkReader, bool> action;
		if (this.m_ReplicationDeserializeDel != null && this.m_ReplicationDeserializeDel.TryGetValue(repl_behaviour, out action))
		{
			action(reader, initial_state);
		}
	}

	private void CallReplicationResolve_Gen(IReplicatedBehaviour repl_behaviour)
	{
		if (this.m_ReplicationResolveDel == null)
		{
			foreach (IReplicatedBehaviour replicatedBehaviour in this.GetReplBehaviours)
			{
				Action reflectedMethod = replicatedBehaviour.GetReflectedMethod(ReplicationComponentReflection.ReflectedMethodType.OnReplicationResolve_CJGenerated);
				if (reflectedMethod != null)
				{
					if (this.m_ReplicationResolveDel == null)
					{
						this.m_ReplicationResolveDel = new Dictionary<IReplicatedBehaviour, Action>();
					}
					this.m_ReplicationResolveDel[replicatedBehaviour] = reflectedMethod;
				}
			}
		}
		Action action;
		if (this.m_ReplicationResolveDel != null && this.m_ReplicationResolveDel.TryGetValue(repl_behaviour, out action))
		{
			action();
		}
	}

	private static P2PNetworkWriter s_Writer = new P2PNetworkWriter();

	public static ReplicationComponent s_DeserializedComponent = null;

	private P2PNetworkReader m_Reader = new P2PNetworkReader();

	[SerializeField]
	private P2PNetworkHash128 m_AssetId;

	private CachedComponent<GuidComponent> m_GuidComponentInternal;

	private CachedComponent<Relevance> m_RelevanceComponentInternal;

	private IReplicatedBehaviour[] m_ReplBehavioursInternal;

	private bool[] m_DirtyBit;

	private bool[] m_ReplBehavioursFlag;

	private P2PPeer m_OwnerPeer = P2PPeer.s_Invalid;

	private P2PPeer m_TransferringOwnershipTo;

	private float m_LocalReplicationIntervalUpdateTime = -1f;

	private float m_LocalReplicationIntervalNow;

	[SerializeField]
	public float m_ReplicationIntervalMin = 0.5f;

	[SerializeField]
	public float m_ReplicationIntervalMax = 2f;

	private ICustomReplicationInterval[] m_CustomReplicationIntervalComponents;

	[SerializeField]
	public bool m_IsOwnershipBlocked;

	private float m_LastReplicationTime = -1f;

	private float m_LastReplicationTimeReal = -1f;

	private Dictionary<P2PPeer, float> m_LastPeerReplicationTime = new Dictionary<P2PPeer, float>();

	private int m_Revision;

	private static ReplicationComponent.SendToPeers s_SendToPeers = new ReplicationComponent.SendToPeers();

	private Dictionary<IReplicatedBehaviour, Action> m_ReplicationPrepareDel;

	private Dictionary<IReplicatedBehaviour, Action<P2PNetworkWriter, bool>> m_ReplicationSerializeDel;

	private Dictionary<IReplicatedBehaviour, Action<P2PNetworkReader, bool>> m_ReplicationDeserializeDel;

	private Dictionary<IReplicatedBehaviour, Action> m_ReplicationResolveDel;

	private class SendToPeers
	{
		public void Reset()
		{
			this.count = 0;
		}

		public void Add(P2PPeer peer)
		{
			DebugUtils.Assert(this.count < this.peers.Length, true);
			P2PPeer[] array = this.peers;
			int num = this.count;
			this.count = num + 1;
			array[num] = peer;
		}

		public int count;

		public P2PPeer[] peers = new P2PPeer[P2PSession.MAX_PLAYERS];
	}
}
