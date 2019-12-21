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
				item.m_CanSaveNotTriggered = false;
				Item[] componentsInChildren = item.GetComponentsInChildren<Item>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].m_CanSaveNotTriggered = false;
				}
				for (int k = 0; k < base.transform.childCount; k++)
				{
					if (base.transform.GetChild(k) == this.m_ObjectList[i].m_List[0].transform)
					{
						child_num = k;
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
		item.m_CanSaveNotTriggered = false;
		Item[] componentsInChildren = item.GetComponentsInChildren<Item>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].m_CanSaveNotTriggered = false;
		}
		this.m_GameObjectsToDestroy.Clear();
		for (int j = 0; j < item.gameObject.transform.childCount; j++)
		{
			if ((1 << j & active_children_mask) > 0)
			{
				item.gameObject.transform.GetChild(j).gameObject.SetActive(true);
			}
			else
			{
				this.m_GameObjectsToDestroy.Add(item.gameObject.transform.GetChild(j).gameObject);
			}
		}
		for (int k = 0; k < this.m_GameObjectsToDestroy.Count; k++)
		{
			UnityEngine.Object.Destroy(this.m_GameObjectsToDestroy[k]);
		}
		return item;
	}

	[HideInInspector]
	public List<string> m_ItemIDNamesList = new List<string>();

	[HideInInspector]
	public List<GameObjectListWrapper> m_ObjectList = new List<GameObjectListWrapper>();

	private List<GameObject> m_GameObjectsToDestroy = new List<GameObject>();
}
