using System;
using System.Collections.Generic;
using UnityEngine;

public class P2PAssetIdCache : ScriptableObject
{
	public static P2PAssetIdCache Instance
	{
		get
		{
			return P2PAssetIdCache.s_Instance;
		}
	}

	private void OnEnable()
	{
		DebugUtils.Assert(P2PAssetIdCache.s_Instance == null || P2PAssetIdCache.s_Instance == this, true);
		P2PAssetIdCache.s_Instance = this;
		this.ProcessPrefabs();
	}

	private void OnDestroy()
	{
		DebugUtils.Assert(P2PAssetIdCache.s_Instance == this, true);
		P2PAssetIdCache.s_Instance = null;
	}

	private void ProcessPrefabs()
	{
		if (this.m_IdToPrefab.Count > 0)
		{
			return;
		}
		int i = 0;
		while (i < this.m_Prefabs.Count)
		{
			GameObject gameObject = this.m_Prefabs[i];
			if (gameObject)
			{
				foreach (GuidComponent guidComponent in gameObject.GetComponents<GuidComponent>())
				{
					if (guidComponent.GetGuid() != Guid.Empty)
					{
						guidComponent.ForceGuid(Guid.Empty);
					}
				}
				ReplicationComponent[] components2 = gameObject.GetComponents<ReplicationComponent>();
				if (components2.Length != 0)
				{
					foreach (ReplicationComponent replicationComponent in components2)
					{
						if (!this.m_IdToPrefab.ContainsValue(replicationComponent.gameObject))
						{
							this.m_IdToPrefab[replicationComponent.AssetId] = replicationComponent.gameObject;
						}
						else
						{
							Debug.LogError("[P2PAssetIdCache.ProcessPrefabs] duplicated prefab");
						}
						i++;
					}
				}
				else
				{
					this.m_Prefabs.RemoveAt(i);
					Debug.LogError("[P2PAssetIdCache.ProcessPrefabs] no ReplicationComponent in object: " + gameObject.name);
				}
			}
			else
			{
				this.m_Prefabs.RemoveAt(i);
			}
		}
	}

	public GameObject GetPrefabById(P2PNetworkHash128 id)
	{
		GameObject result = null;
		this.m_IdToPrefab.TryGetValue(id, out result);
		return result;
	}

	private static P2PAssetIdCache s_Instance;

	private const string CACHE_PATH = "Scripts/Network/P2PAssetIdCache";

	private Dictionary<P2PNetworkHash128, GameObject> m_IdToPrefab = new Dictionary<P2PNetworkHash128, GameObject>();

	[SerializeField]
	private List<GameObject> m_Prefabs = new List<GameObject>();
}
