using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
internal class ReplicatedStaticObjects : ReplicatedBehaviour
{
	public static ReplicatedStaticObjects GetLocal()
	{
		return ReplicatedStaticObjects.s_LocalReplicatedStaticObjects;
	}

	private void Awake()
	{
		if (base.ReplIsOwner())
		{
			ReplicatedStaticObjects.s_LocalReplicatedStaticObjects = this;
		}
	}

	private void OnDestroy()
	{
		if (base.ReplIsOwner())
		{
			ReplicatedStaticObjects.s_LocalReplicatedStaticObjects = null;
		}
	}

	public void AddDestroyedObject(GameObject obj)
	{
		if (!ReplTools.IsPlayingAlone())
		{
			this.m_ReplDestroyedObject.Add(obj.transform.position);
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			writer.WritePackedUInt32((uint)this.m_ReplDestroyedObject.Count);
			for (int i = 0; i < this.m_ReplDestroyedObject.Count; i++)
			{
				writer.Write(this.m_ReplDestroyedObject[i]);
			}
			this.m_ReplDestroyedObject.Clear();
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initialState)
	{
		if (!initialState)
		{
			uint num = reader.ReadPackedUInt32();
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				StaticObjectsManager.Get().ObjectDestroyed(reader.ReadVector3());
				num2++;
			}
		}
	}

	private static ReplicatedStaticObjects s_LocalReplicatedStaticObjects;

	private List<Vector3> m_ReplDestroyedObject = new List<Vector3>();
}
