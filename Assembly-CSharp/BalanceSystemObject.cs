using System;
using Enums;
using UnityEngine;

public class BalanceSystemObject
{
	public void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.m_Position);
		writer.Write((int)this.m_ItemID);
		writer.Write(this.m_ActiveChildrenMask);
		writer.Write((short)this.m_Group.index);
	}

	public void Deserialize(P2PNetworkReader reader)
	{
		this.m_Position = reader.ReadVector3();
		this.m_ItemID = (ItemID)reader.ReadInt32();
		this.m_ActiveChildrenMask = reader.ReadInt32();
		this.m_Group = BalanceSystem20.Get().GetGroupByIndex((int)reader.ReadInt16());
	}

	public void CopyReplValues(BalanceSystemObject obj)
	{
		this.m_Position = obj.m_Position;
		this.m_ItemID = obj.m_ItemID;
		this.m_ActiveChildrenMask = obj.m_ActiveChildrenMask;
		this.m_Group = obj.m_Group;
	}

	public GameObject m_GameObject;

	public BalanceSystem20.GroupProps m_Group;

	public GameObject m_BalanceSpawner;

	public ItemID m_ItemID;

	public int m_ChildNum;

	public Vector3 m_Position;

	public int m_ActiveChildrenMask = -1;

	public bool m_AllChildrenDestroyed;

	public float m_LastSpawnObjectTime = float.MinValue;
}
