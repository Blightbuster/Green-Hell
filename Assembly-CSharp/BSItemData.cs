using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BSItemData
{
	public string m_PrefabName = string.Empty;

	public GameObject m_Prefab;

	public float m_Chance;

	public ItemID m_ItemID = ItemID.None;

	public float m_LastSpawnTime = float.MinValue;

	public float m_BaseChance;

	public float m_IncRate = 0.01f;

	public float m_ChanceAccu;

	public float m_WalkRange = 20f;

	public float m_WalkRangeValue = 0.2f;

	public VDelegate m_Func;

	public Vector3 m_LastSpawnPos = Vector3.zero;

	public List<int> m_HaveItemID = new List<int>();

	public int m_HaveItemCount;

	public float m_HaveItemNegativeEffect;
}
