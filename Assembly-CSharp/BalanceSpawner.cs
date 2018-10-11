using System;
using Enums;
using UnityEngine;

public class BalanceSpawner : MonoBehaviour
{
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
	public float m_LastCheckSpawnTime = float.MinValue;

	public float m_LastSpawnObjectTime = float.MinValue;
}
