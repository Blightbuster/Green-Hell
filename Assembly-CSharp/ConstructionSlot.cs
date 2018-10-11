using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ConstructionSlot : MonoBehaviour
{
	private void Awake()
	{
		foreach (string value in this.m_MatchingItems)
		{
			this.m_MatchingItemIDs.Add((ItemID)Enum.Parse(typeof(ItemID), value));
		}
		if (this.m_ChangeTo != string.Empty)
		{
			this.m_ChangeToID = (ItemID)Enum.Parse(typeof(ItemID), this.m_ChangeTo);
		}
	}

	public void SetConstruction(Construction construction)
	{
		this.m_Construction = construction;
		this.ConnectConstructions(this.m_ParentConstruction, construction);
	}

	private void ConnectConstructions(Construction construction1, Construction construction2)
	{
		if (construction1 == construction2)
		{
			return;
		}
		if (!construction1.m_ConnectedConstructions.Contains(construction2))
		{
			construction1.m_ConnectedConstructions.Add(construction2);
			int count = construction1.m_ConnectedConstructions.Count;
			for (int i = 0; i < count; i++)
			{
				this.ConnectConstructions(construction1.m_ConnectedConstructions[i], construction2);
			}
		}
		if (!construction2.m_ConnectedConstructions.Contains(construction1))
		{
			construction2.m_ConnectedConstructions.Add(construction1);
			int count2 = construction2.m_ConnectedConstructions.Count;
			for (int j = 0; j < count2; j++)
			{
				this.ConnectConstructions(construction2.m_ConnectedConstructions[j], construction1);
			}
		}
	}

	public List<string> m_MatchingItems = new List<string>();

	[HideInInspector]
	public List<ItemID> m_MatchingItemIDs = new List<ItemID>();

	public string m_ChangeTo = string.Empty;

	[HideInInspector]
	public ItemID m_ChangeToID = ItemID.None;

	public bool m_DestroyWithSnapParent;

	[HideInInspector]
	public Construction m_ParentConstruction;

	[HideInInspector]
	public Construction m_Construction;
}
