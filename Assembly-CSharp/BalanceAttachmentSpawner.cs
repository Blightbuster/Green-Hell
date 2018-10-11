using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BalanceAttachmentSpawner : BalanceSpawner
{
	public override bool IsAttachmentSpawner()
	{
		return true;
	}

	public ItemID GetItemID(int idx)
	{
		return (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemIDNamesList[idx]);
	}

	public override GameObject TryToAttach(ItemID item_id, out int child_num)
	{
		child_num = -1;
		for (int i = 0; i < this.m_ItemIDNamesList.Count; i++)
		{
			if (item_id == this.GetItemID(i))
			{
				Item item = ItemsManager.Get().CreateItem(item_id, true, this.m_ObjectList[i].m_List[0].transform.position, this.m_ObjectList[i].m_List[0].transform.rotation);
				item.transform.position = Vector3.zero;
				item.transform.rotation = Quaternion.identity;
				item.transform.SetParent(this.m_ObjectList[i].m_List[0].transform, false);
				item.m_CanSave = false;
				for (int j = 0; j < base.transform.childCount; j++)
				{
					if (base.transform.GetChild(j) == this.m_ObjectList[i].m_List[0].transform)
					{
						child_num = j;
						return item.gameObject;
					}
				}
			}
		}
		return null;
	}

	public override Item Attach(ItemID item_id, int child_num, int active_children_mask)
	{
		Item item = ItemsManager.Get().CreateItem(item_id, true, base.transform.GetChild(child_num).position, base.transform.GetChild(child_num).rotation);
		item.transform.position = Vector3.zero;
		item.transform.rotation = Quaternion.identity;
		item.transform.SetParent(base.transform.GetChild(child_num), false);
		item.m_CanSave = false;
		this.m_GameObjectsToDestroy.Clear();
		for (int i = 0; i < item.gameObject.transform.childCount; i++)
		{
			if ((1 << i & active_children_mask) > 0)
			{
				item.gameObject.transform.GetChild(i).gameObject.SetActive(true);
			}
			else
			{
				this.m_GameObjectsToDestroy.Add(item.gameObject.transform.GetChild(i).gameObject);
			}
		}
		for (int j = 0; j < this.m_GameObjectsToDestroy.Count; j++)
		{
			UnityEngine.Object.Destroy(this.m_GameObjectsToDestroy[j]);
		}
		return item;
	}

	[HideInInspector]
	public List<string> m_ItemIDNamesList = new List<string>();

	[HideInInspector]
	public List<GameObjectListWrapper> m_ObjectList = new List<GameObjectListWrapper>();

	private List<GameObject> m_GameObjectsToDestroy = new List<GameObject>();
}
