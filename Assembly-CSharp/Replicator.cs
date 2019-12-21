using System;
using System.Collections.Generic;
using UnityEngine;

public class Replicator
{
	public static bool IsAnyObjectBeingDeserialized()
	{
		return ReplicationComponent.s_DeserializedComponent != null;
	}

	public static bool IsObjectBeingDeserialized(GameObject obj, bool check_parent = false)
	{
		if (ReplicationComponent.s_DeserializedComponent == null)
		{
			return false;
		}
		while (obj)
		{
			if (ReplicationComponent.s_DeserializedComponent.gameObject == obj)
			{
				return true;
			}
			if (!check_parent)
			{
				return false;
			}
			obj = obj.transform.parent.gameObject;
		}
		return false;
	}

	public void RegisterReplicationComponent(ReplicationComponent component)
	{
		if (component)
		{
			if (this.m_GameObjectMapLock)
			{
				this.m_DelayedRegisterObjects.Add(Tuple.Create<GameObject, ReplicationComponent>(component.gameObject, component));
				return;
			}
			this.m_GameObjectMap[component.gameObject] = component;
		}
	}

	public void UnregisterReplicationComponent(ReplicationComponent component)
	{
		DebugUtils.Assert(!this.m_GameObjectMapLock, "Removing replicated object inside internal update!", true, DebugUtils.AssertType.Info);
		if (!this.m_GameObjectMapLock)
		{
			this.m_GameObjectMap.Remove(component.gameObject);
		}
	}

	public ReplicationComponent GetReplComponentForGameObject(GameObject obj, bool expect_component = true)
	{
		ReplicationComponent component;
		if (!this.m_GameObjectMap.TryGetValue(obj, out component))
		{
			component = obj.GetComponent<ReplicationComponent>();
			if (component != null)
			{
				this.RegisterReplicationComponent(component);
			}
			else if (this.m_GameObjectMapLock)
			{
				this.m_DelayedRegisterObjects.Add(Tuple.Create<GameObject, ReplicationComponent>(obj, null));
			}
			else
			{
				this.m_GameObjectMap[obj] = null;
			}
			if (expect_component && P2PLogFilter.logError && component == null)
			{
				Debug.LogWarning("Could not find ReplicationComponent in " + obj.name, obj);
			}
		}
		return component;
	}

	public void Update()
	{
		this.m_GameObjectMapLock = true;
		Dictionary<GameObject, ReplicationComponent>.Enumerator enumerator = this.m_GameObjectMap.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<GameObject, ReplicationComponent> keyValuePair = enumerator.Current;
			ReplicationComponent value = keyValuePair.Value;
			if (value != null && !this.m_DelayedSpawnObjects.Contains(value.gameObject))
			{
				value.OnReplicatorUpdate(false);
			}
		}
		enumerator.Dispose();
		this.m_GameObjectMapLock = false;
		if (this.m_DelayedRegisterObjects.Count > 0)
		{
			foreach (GameObject key in this.m_DelayedUnregisterObjects)
			{
				this.m_GameObjectMap.Remove(key);
			}
			foreach (Tuple<GameObject, ReplicationComponent> tuple in this.m_DelayedRegisterObjects)
			{
				this.m_GameObjectMap[tuple.Item1] = tuple.Item2;
			}
			this.m_DelayedUnregisterObjects.Clear();
			this.m_DelayedRegisterObjects.Clear();
		}
	}

	public void OnReplicationMessage(P2PNetworkMessage net_msg)
	{
		byte[] array = net_msg.m_Reader.ReadGuidBytesTemporary();
		GameObject gameObject = GuidManager.ResolveGuid(array);
		if (gameObject)
		{
			this.GetReplComponentForGameObject(gameObject, true).ReplicationReceive(net_msg.m_Reader, false);
			return;
		}
		if (P2PLogFilter.logWarn)
		{
			Debug.LogWarning("Did not find target for replication update message for " + new Guid(array));
		}
		P2PNetworkWriter p2PNetworkWriter = new P2PNetworkWriter();
		p2PNetworkWriter.StartMessage(8);
		p2PNetworkWriter.Write(array, GuidComponent.GUID_BYTES_CNT);
		p2PNetworkWriter.FinishMessage();
		P2PSession.Instance.SendWriterTo(net_msg.m_Connection.m_Peer, p2PNetworkWriter, 0);
	}

	private bool Spawn(GameObject obj)
	{
		P2PNetworkWriter p2PNetworkWriter = this.BuildSpawnWriter(obj, false);
		if (p2PNetworkWriter == null)
		{
			return false;
		}
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("Spawn called for {0} with guid {1}", obj.name, obj.GetComponent<GuidComponent>().GetGuid()), obj);
		}
		P2PSession.Instance.SendWriterToAll(p2PNetworkWriter, 0);
		return true;
	}

	public void SpawnDelayed(GameObject obj)
	{
		if (!ReplTools.IsPlayingAlone())
		{
			this.m_DelayedSpawnObjects.Add(obj);
		}
	}

	public void UpdateDelayedSpawnObjects()
	{
		if (!ReplTools.IsPlayingAlone())
		{
			for (int i = 0; i < this.m_DelayedSpawnObjects.Count; i++)
			{
				GameObject gameObject = this.m_DelayedSpawnObjects[i];
				if (gameObject != null)
				{
					this.Spawn(gameObject);
				}
			}
		}
		this.m_DelayedSpawnObjects.Clear();
	}

	public bool SpawnForPeer(GameObject obj, P2PPeer peer)
	{
		P2PNetworkWriter p2PNetworkWriter = this.BuildSpawnWriter(obj, false);
		if (p2PNetworkWriter == null)
		{
			return false;
		}
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("SpawnForPeer called for {0} with guid {1}", obj.name, obj.GetComponent<GuidComponent>().GetGuid()), obj);
		}
		P2PSession.Instance.SendWriterTo(peer, p2PNetworkWriter, 0);
		return true;
	}

	public bool SpawnForPeerAndGiveOwnership(GameObject obj, P2PPeer peer)
	{
		P2PNetworkWriter p2PNetworkWriter = this.BuildSpawnWriter(obj, true);
		if (p2PNetworkWriter == null)
		{
			return false;
		}
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("SpawnForPeerAndGiveOwnership called for {0} with guid {1}", obj.name, obj.GetComponent<GuidComponent>().GetGuid()), obj);
		}
		P2PSession.Instance.SendWriterTo(peer, p2PNetworkWriter, 0);
		return true;
	}

	private P2PNetworkWriter BuildSpawnWriter(GameObject obj, bool give_ownership)
	{
		ReplicationComponent replComponentForGameObject = this.GetReplComponentForGameObject(obj, true);
		if (replComponentForGameObject == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Replicator::Spawn called for non-replicable object " + obj.name + "!");
			}
			return null;
		}
		if (!replComponentForGameObject.enabled)
		{
			return null;
		}
		GuidComponent component = obj.GetComponent<GuidComponent>();
		if (component == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Replicator::Spawn called for object " + obj.name + " without guid!", obj);
			}
			return null;
		}
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Concat(new string[]
			{
				"Replicator::BuildSpawnWriter called for ",
				obj.name,
				" with asset id ",
				replComponentForGameObject.AssetId.ToString(),
				"!"
			}), obj);
		}
		Replicator.s_ObjectSpawnMessage.assetId = replComponentForGameObject.AssetId;
		Replicator.s_ObjectSpawnMessage.guid_bytes = component.GetGuidBytes();
		Replicator.s_ObjectSpawnMessage.position = obj.transform.position;
		Replicator.s_ObjectSpawnMessage.payload = replComponentForGameObject.Serialize(true);
		P2PNetworkWriter p2PNetworkWriter = new P2PNetworkWriter();
		p2PNetworkWriter.StartMessage(give_ownership ? 9 : 2);
		Replicator.s_ObjectSpawnMessage.Serialize(p2PNetworkWriter);
		p2PNetworkWriter.FinishMessage();
		return p2PNetworkWriter;
	}

	public void OnSpawnMessage(P2PNetworkMessage net_msg, bool take_ownership)
	{
		net_msg.ReadMessage<P2PObjectSpawnMessage>(Replicator.s_ObjectSpawnMessage);
		GameObject gameObject = GuidManager.ResolveGuid(Replicator.s_ObjectSpawnMessage.guid_bytes);
		if (P2PLogFilter.logDev)
		{
			Debug.Log(string.Format("OnSpawnMessage {0} channel: {1} take_ownership: {2}", new Guid(Replicator.s_ObjectSpawnMessage.guid_bytes), net_msg.m_ChannelId, take_ownership));
		}
		if (!gameObject)
		{
			GameObject prefabById = P2PAssetIdCache.Instance.GetPrefabById(Replicator.s_ObjectSpawnMessage.assetId);
			if (prefabById)
			{
				bool activeSelf = prefabById.activeSelf;
				prefabById.SetActive(false);
				gameObject = UnityEngine.Object.Instantiate<GameObject>(prefabById, Replicator.s_ObjectSpawnMessage.position, Quaternion.identity);
				ReplicationComponent component = gameObject.GetComponent<ReplicationComponent>();
				component.ReplOnChangedOwner(net_msg.m_Connection.m_Peer);
				gameObject.GetComponent<GuidComponent>().ForceGuid(Replicator.s_ObjectSpawnMessage.guid_bytes);
				prefabById.SetActive(activeSelf);
				gameObject.SetActive(activeSelf);
				component.ReplOnSpawned();
				component.Deserialize(Replicator.s_ObjectSpawnMessage.payload, true);
				if (take_ownership)
				{
					component.ReplRequestOwnership();
					return;
				}
			}
			else if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Format("OnSpawnMessage no asset found {0}", Replicator.s_ObjectSpawnMessage.assetId));
				return;
			}
		}
		else
		{
			if (P2PLogFilter.logWarn)
			{
				Debug.LogWarning(string.Format("OnSpawnMessage found target for supposedly new object to spawn {0} {1}", gameObject.name, new Guid(Replicator.s_ObjectSpawnMessage.guid_bytes)));
			}
			ReplicationComponent replComponentForGameObject = this.GetReplComponentForGameObject(gameObject, true);
			replComponentForGameObject.Deserialize(Replicator.s_ObjectSpawnMessage.payload, true);
			if (take_ownership)
			{
				replComponentForGameObject.ReplRequestOwnership();
			}
		}
	}

	public bool Despawn(ReplicationComponent obj)
	{
		if (ReplTools.IsPlayingAlone())
		{
			return false;
		}
		P2PNetworkWriter p2PNetworkWriter = this.BuildDespawnWriter(obj);
		if (p2PNetworkWriter != null)
		{
			P2PSession.Instance.SendWriterToAll(p2PNetworkWriter, 0);
		}
		return p2PNetworkWriter != null;
	}

	public bool DespawnForPeer(ReplicationComponent obj, P2PPeer peer)
	{
		P2PNetworkWriter p2PNetworkWriter = this.BuildDespawnWriter(obj);
		if (p2PNetworkWriter != null)
		{
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("DespawnForPeer [{0}] called for {1} guid: {2}", peer.GetHostId(), obj.name, obj.GetGuidComponent.GetGuid()), obj);
			}
			P2PSession.Instance.SendWriterTo(peer, p2PNetworkWriter, 0);
		}
		return p2PNetworkWriter != null;
	}

	private P2PNetworkWriter BuildDespawnWriter(ReplicationComponent obj)
	{
		if (obj.GetGuidComponent == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Replicator::BuildDespawnWriter called for object without guid!");
			}
			return null;
		}
		P2PNetworkWriter p2PNetworkWriter = new P2PNetworkWriter();
		p2PNetworkWriter.StartMessage(3);
		p2PNetworkWriter.Write(obj.gameObject);
		p2PNetworkWriter.FinishMessage();
		return p2PNetworkWriter;
	}

	public void OnDespawnMessage(P2PNetworkMessage netMsg)
	{
		GameObject gameObject = netMsg.m_Reader.ReadGameObject();
		if (gameObject)
		{
			if (this.GetReplComponentForGameObject(gameObject, true).ReplGetOwner() == netMsg.m_Connection.m_Peer)
			{
				if (P2PLogFilter.logPedantic)
				{
					Debug.Log(string.Format("OnDespawnMessage called for {0} guid: {1}", gameObject.name, gameObject.GetComponent<GuidComponent>().GetGuid()), gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
			if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Format("Removing object {0} guid: {1} not owned by peer ordering the removal!!!", gameObject.name, gameObject.GetComponent<GuidComponent>().GetGuid()));
			}
		}
	}

	public void OnRequestOwnership(P2PNetworkMessage netMsg)
	{
		GameObject gameObject = netMsg.m_Reader.ReadGameObject();
		if (gameObject)
		{
			ReplicationComponent replComponentForGameObject = this.GetReplComponentForGameObject(gameObject, true);
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("OnRequestOwnership called for {0} guid: {1}", replComponentForGameObject.name, replComponentForGameObject.GetGuidComponent.GetGuid()), gameObject);
			}
			ReplicationComponent.s_DeserializedComponent = replComponentForGameObject;
			P2PPeer peerById = ReplTools.GetPeerById(netMsg.m_Reader.ReadInt16());
			replComponentForGameObject.ReplOnChangedOwner(peerById);
			ReplicationComponent.s_DeserializedComponent = null;
		}
	}

	public void OnGiveOwnership(P2PNetworkMessage netMsg)
	{
		byte[] array;
		GameObject gameObject = netMsg.m_Reader.ReadGameObjectAndGuid(out array);
		if (gameObject)
		{
			ReplicationComponent replComponentForGameObject = this.GetReplComponentForGameObject(gameObject, true);
			if (P2PLogFilter.logPedantic)
			{
				Debug.Log(string.Format("OnGiveOwnership called for {0} guid: {1}", replComponentForGameObject.name, replComponentForGameObject.GetGuidComponent.GetGuid()), gameObject);
			}
			ReplicationComponent.s_DeserializedComponent = replComponentForGameObject;
			P2PPeer peerById = ReplTools.GetPeerById(netMsg.m_Reader.ReadInt16());
			if (peerById == ReplTools.GetLocalPeer())
			{
				replComponentForGameObject.ReplRequestOwnership();
			}
			else
			{
				replComponentForGameObject.ReplOnChangedOwner(peerById);
			}
			ReplicationComponent.s_DeserializedComponent = null;
			return;
		}
		P2PNetworkWriter p2PNetworkWriter = new P2PNetworkWriter();
		p2PNetworkWriter.StartMessage(8);
		p2PNetworkWriter.Write(array, GuidComponent.GUID_BYTES_CNT);
		p2PNetworkWriter.FinishMessage();
		if (P2PLogFilter.logWarn)
		{
			Debug.LogWarning("Did not find target for give ownership message for " + new Guid(array));
		}
		P2PSession.Instance.SendWriterTo(netMsg.m_Connection.m_Peer, p2PNetworkWriter, 0);
	}

	public void OnNeedFullObjectInfo(P2PNetworkMessage netMsg)
	{
		byte[] array = netMsg.m_Reader.ReadGuidBytesTemporary();
		GameObject gameObject = GuidManager.ResolveGuid(array);
		if (!gameObject)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Cant send full object info - no object! " + new Guid(array));
			}
			return;
		}
		ReplicationComponent replComponentForGameObject = this.GetReplComponentForGameObject(gameObject, true);
		if (P2PLogFilter.logPedantic)
		{
			Debug.Log(string.Format("OnNeedFullObjectInfo received for {0} guid: {1}", gameObject.name, replComponentForGameObject.GetGuidComponent.GetGuid()));
		}
		if (replComponentForGameObject != null && replComponentForGameObject.IsTransferringOwnershipTo(netMsg.m_Connection.m_Peer))
		{
			this.SpawnForPeerAndGiveOwnership(gameObject, netMsg.m_Connection.m_Peer);
			return;
		}
		this.SpawnForPeer(gameObject, netMsg.m_Connection.m_Peer);
	}

	public void OnPeerConnected(P2PPeer peer)
	{
		if (peer.GetHostId() == P2PPeer.s_InvalidId)
		{
			return;
		}
		this.m_GameObjectMapLock = true;
		Dictionary<GameObject, ReplicationComponent>.Enumerator enumerator = this.m_GameObjectMap.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<GameObject, ReplicationComponent> keyValuePair = enumerator.Current;
			ReplicationComponent value = keyValuePair.Value;
			if (!(value == null) && !(value.GetRelevanceComponent != null) && (value.ReplIsOwner() || (ReplTools.AmIMaster() && value.ReplGetOwner() == P2PPeer.s_Invalid)))
			{
				this.SpawnForPeer(value.gameObject, peer);
			}
		}
		enumerator.Dispose();
		this.m_GameObjectMapLock = false;
	}

	public void OnPeerDisconnected(P2PPeer peer)
	{
		if (!ReplTools.AmIMaster())
		{
			return;
		}
		this.DestroyPeerRepresentationRelatedObjects(peer);
		this.m_GameObjectMapLock = true;
		Dictionary<GameObject, ReplicationComponent>.Enumerator enumerator = this.m_GameObjectMap.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<GameObject, ReplicationComponent> keyValuePair = enumerator.Current;
			ReplicationComponent value = keyValuePair.Value;
			if (!(value == null) && value.ReplGetOwner() == peer)
			{
				if (value.ReplCanChangeOwnership())
				{
					value.ReplRequestOwnership();
				}
				else
				{
					UnityEngine.Object.Destroy(value.gameObject);
				}
			}
		}
		enumerator.Dispose();
		this.m_GameObjectMapLock = false;
	}

	private void DestroyPeerRepresentationRelatedObjects(P2PPeer peer)
	{
		IReplicatedBehaviour[] componentsInChildren = (peer.m_Representation as Component).gameObject.GetComponentsInChildren<IReplicatedBehaviour>();
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		IReplicatedBehaviour[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = (array[i] as Component).gameObject;
			if (!hashSet.Contains(gameObject))
			{
				hashSet.Add(gameObject);
			}
		}
		foreach (GameObject obj in hashSet)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	public static readonly Replicator Singleton = new Replicator();

	private Dictionary<GameObject, ReplicationComponent> m_GameObjectMap = new Dictionary<GameObject, ReplicationComponent>(2000);

	private List<GameObject> m_DelayedSpawnObjects = new List<GameObject>();

	private bool m_GameObjectMapLock;

	private List<Tuple<GameObject, ReplicationComponent>> m_DelayedRegisterObjects = new List<Tuple<GameObject, ReplicationComponent>>();

	private List<GameObject> m_DelayedUnregisterObjects = new List<GameObject>();

	private static P2PObjectSpawnMessage s_ObjectSpawnMessage = new P2PObjectSpawnMessage();
}
