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
		if (!item.m_InStorage)
		{
			item.StaticPhxRequestRemove();
		}
		if (!from_destroy && !item.m_IsBeingDestroyed)
		{
			if (item.m_InStorage)
			{
				item.transform.parent = Storage3D.Get().transform;
			}
			else if (item.m_InInventory)
			{
				item.transform.parent = InventoryBackpack.Get().transform;
			}
			else
			{
				item.transform.parent = null;
			}
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
		int i = 0;
		while (i < this.m_Items.Count)
		{
			if (this.m_Items[i] == null)
			{
				this.m_Items.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
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
		for (int j = 0; j < this.m_Items.Count; j++)
		{
			Item item2 = this.m_Items[j];
			this.RemoveItem(item2, from_destroy);
			if (SaveGame.m_State == SaveGame.State.None && item.m_InventorySlot)
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
		int num = 0;
		while (num < this.m_Items.Count && num < this.m_StackDummies.Count)
		{
			this.m_Items[num].transform.localPosition = this.m_StackDummies[num].transform.localPosition;
			this.m_Items[num].transform.localRotation = (this.m_AdjustRotation ? this.m_StackDummies[num].transform.localRotation : Quaternion.identity);
			num++;
		}
	}

	public void ReplaceItem(Item old_item, Item new_item)
	{
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			if (this.m_Items[i] == old_item)
			{
				new_item.transform.rotation = old_item.transform.rotation;
				new_item.transform.position = old_item.transform.position;
				new_item.transform.parent = old_item.transform.parent;
				if (this.m_ItemParent && this.m_ItemParent.m_Info.m_InventoryCellsGroup != null)
				{
					this.m_ItemParent.m_Info.m_InventoryCellsGroup.Insert(new_item, null);
				}
				new_item.m_Info.m_PrevInventoryCellsGroup = null;
				if (new_item.m_InventorySlot)
				{
					new_item.m_InventorySlot.m_ActivityUpdate = false;
				}
				new_item.m_CurrentSlot = this;
				new_item.StaticPhxRequestAdd();
				new_item.UpdatePhx();
				new_item.ReseScale();
				new_item.UpdateLayer();
				old_item.transform.parent = null;
				this.m_Items[i] = new_item;
				return;
			}
		}
	}

	public List<Item> m_Items = new List<Item>();

	[HideInInspector]
	public List<GameObject> m_StackDummies = new List<GameObject>();
}
