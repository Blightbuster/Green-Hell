using System;
using Enums;
using UnityEngine;

public class BSItemData
{
	public string m_PrefabName = string.Empty;

	public GameObject m_Prefab;

	public float m_Chance = 1f;

	public BSCondition m_Condition = BSCondition.None;

	public float m_ConditionValue;

	public float m_ConditionChance;

	public float m_Cooldown = 10f;

	public float m_CooldownChance = 0.1f;

	public ItemID m_ItemID = ItemID.None;

	public float m_LastSpawnTime = float.MinValue;
}
