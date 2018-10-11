using System;
using Enums;
using UnityEngine;

public class BalanceSystemObject
{
	public GameObject m_GameObject;

	public string m_Group;

	public GameObject m_BalanceSpawner;

	public ItemID m_ItemID;

	public int m_ChildNum;

	public Vector3 m_Position;

	public int m_ActiveChildrenMask;

	public bool m_AllChildrenDestroyed;

	public float m_LastSpawnObjectTime = float.MinValue;
}
