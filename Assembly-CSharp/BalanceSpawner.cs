using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BalanceSpawner : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.layer = LayerMask.NameToLayer("BalanceSpawner");
	}

	public virtual bool IsAttachmentSpawner()
	{
		return false;
	}

	public virtual GameObject TryToAttach(ItemID item_id, out int child_num)
	{
		child_num = -1;
		return null;
	}

	public virtual Item Attach(ItemID item_id, int child_num, int active_children_mask)
	{
		return null;
	}

	public bool m_StaticSystem;

	[HideInInspector]
	private static HashSet<BalanceSpawner> s_AllSpawners = new HashSet<BalanceSpawner>();

	private static GUIStyle m_GUIStyleNormal = null;

	private static GUIStyle m_GUIStyleCooldown = null;

	public float m_LastSpawnObjectTime = float.MinValue;

	public float m_LastNoSpawnObjectTime = float.MinValue;
}
