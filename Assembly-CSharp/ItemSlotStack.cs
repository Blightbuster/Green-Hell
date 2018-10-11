using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlotStack : ItemSlot
{
	protected override void Awake()
	{
		base.Awake();
		this.m_StackDummies.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			this.m_StackDummies.Add(base.transform.GetChild(i).gameObject);
		}
	}

	public override bool IsStack()
	{
		return true;
	}

	public override bool IsOccupied()
	{
		return this.m_Items.Count >= this.m_StackDummies.Count;
	}

	public override void InsertItem(Item item)
	{
		this.m_Items.Add(item);
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
		}
		if (this.m_ItemParent && this.m_ItemParent.m_Info.m_InventoryCellsGroup != null)
		{
			this.m_ItemParent.m_Info.m_InventoryCellsGroup.Insert(item, null);
		}
		this.OnInsertItem(item);
		this.SetupStack();
		if (item.m_InventorySlot)
		{
			item.m_InventorySlot.m_ActivityUpdate = false;
			item.m_InventorySlot.Deactivate();
		}
	}

	public override void RemoveItem(Item item, bool from_destroy = false)
	{
		if (LoadingScreen.Get().m_State == LoadingScreenState.ReturnToMainMenu)
		{
			return;
		}
		if (!this.m_Items.Contains(item))
		{
			if (item == this.m_ItemParent)
			{
				this.ReorganizeStack(from_destroy);
			}
			return;
		}
		this.OnRemoveItem();
		item.m_CurrentSlot = null;
		item.StaticPhxRequestRemove();
		if (!from_destroy)
		{
			item.transform.parent = null;
		}
		this.m_Items.Remove(item);
		this.SetupStack();
		if (item.m_InventorySlot)
		{
			item.m_InventorySlot.m_ActivityUpdate = true;
		}
	}

	private void ReorganizeStack(bool from_destroy = false)
	{
		if (this.m_Items.Count == 0)
		{
			return;
		}
		Item item = this.m_Items[0];
		if (!item.m_InventorySlot)
		{
			item.InitInventorySlot();
		}
		this.RemoveItem(item, from_destroy);
		item.transform.rotation = this.m_ItemParent.transform.rotation;
		item.transform.position = this.m_ItemParent.transform.position;
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			Item item2 = this.m_Items[i];
			this.RemoveItem(item2, from_destroy);
			if (SaveGame.m_State == SaveGame.State.None)
			{
				item.m_InventorySlot.InsertItem(item2);
			}
		}
	}

	private void SetupStack()
	{
		if (this.m_Items.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			this.m_Items[i].transform.localPosition = this.m_StackDummies[i].transform.localPosition;
			this.m_Items[i].transform.localRotation = ((!this.m_AdjustRotation) ? Quaternion.identity : this.m_StackDummies[i].transform.localRotation);
		}
	}

	public List<Item> m_Items = new List<Item>();

	[HideInInspector]
	public List<GameObject> m_StackDummies = new List<GameObject>();
}
