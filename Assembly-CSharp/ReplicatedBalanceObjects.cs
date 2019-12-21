using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
internal class ReplicatedBalanceObjects : ReplicatedBehaviour
{
	public static ReplicatedBalanceObjects GetLocal()
	{
		return ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects;
	}

	private void Awake()
	{
		if (base.ReplIsOwner())
		{
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects = this;
		}
	}

	private void OnDestroy()
	{
		if (base.ReplIsOwner())
		{
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects = null;
		}
	}

	public static void OnObjectChanged(BalanceSystemObject obj)
	{
		if (ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects != null)
		{
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects.StoreObjectChanged(obj);
		}
	}

	public static void OnObjectDestroyed(BalanceSystemObject obj)
	{
		if (ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects != null)
		{
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects.StoreObjectDestroyed(obj);
		}
	}

	public static void Clear()
	{
		if (ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects != null)
		{
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects.m_ReplBalanceObjects.Clear();
			ReplicatedBalanceObjects.s_LocalReplicatedBalanceObjects.m_ReplDestroyedBalanceObjects.Clear();
		}
	}

	private void StoreObjectChanged(BalanceSystemObject obj)
	{
		if (!ReplTools.IsPlayingAlone())
		{
			this.m_ReplBalanceObjects.Add(obj);
			this.ReplSetDirty();
		}
	}

	private void StoreObjectDestroyed(BalanceSystemObject obj)
	{
		if (!ReplTools.IsPlayingAlone())
		{
			this.m_ReplDestroyedBalanceObjects.Add(obj);
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		if (!initial_state)
		{
			using (P2PNetworkWriterSeekHelper p2PNetworkWriterSeekHelper = new P2PNetworkWriterSeekHelper(writer))
			{
				writer.WritePackedUInt32(0u);
				uint num = 0u;
				for (int i = 0; i < this.m_ReplBalanceObjects.Count; i++)
				{
					if (this.m_ReplBalanceObjects[i] != null)
					{
						this.m_ReplBalanceObjects[i].Serialize(writer);
						num += 1u;
					}
				}
				p2PNetworkWriterSeekHelper.SeekToStoredPos();
				writer.WritePackedUInt32(num);
			}
			this.m_ReplBalanceObjects.Clear();
			using (P2PNetworkWriterSeekHelper p2PNetworkWriterSeekHelper2 = new P2PNetworkWriterSeekHelper(writer))
			{
				writer.WritePackedUInt32(0u);
				uint num2 = 0u;
				for (int j = 0; j < this.m_ReplDestroyedBalanceObjects.Count; j++)
				{
					if (this.m_ReplDestroyedBalanceObjects[j] != null)
					{
						this.m_ReplDestroyedBalanceObjects[j].Serialize(writer);
						num2 += 1u;
					}
				}
				p2PNetworkWriterSeekHelper2.SeekToStoredPos();
				writer.WritePackedUInt32(num2);
			}
			this.m_ReplDestroyedBalanceObjects.Clear();
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		if (!initial_state)
		{
			uint num = reader.ReadPackedUInt32();
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				ReplicatedBalanceObjects.s_TmpObjHolder.Deserialize(reader);
				BalanceSystem20.Get().OnBalanceSystemObjectReplReceived(ReplicatedBalanceObjects.s_TmpObjHolder, false);
				num2++;
			}
			num = reader.ReadPackedUInt32();
			int num3 = 0;
			while ((long)num3 < (long)((ulong)num))
			{
				ReplicatedBalanceObjects.s_TmpObjHolder.Deserialize(reader);
				BalanceSystem20.Get().OnBalanceSystemObjectReplReceived(ReplicatedBalanceObjects.s_TmpObjHolder, true);
				num3++;
			}
		}
	}

	private static ReplicatedBalanceObjects s_LocalReplicatedBalanceObjects;

	private List<BalanceSystemObject> m_ReplBalanceObjects = new List<BalanceSystemObject>();

	private List<BalanceSystemObject> m_ReplDestroyedBalanceObjects = new List<BalanceSystemObject>();

	private static BalanceSystemObject s_TmpObjHolder = new BalanceSystemObject();
}
