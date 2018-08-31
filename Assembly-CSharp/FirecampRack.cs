using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class FirecampRack : Construction, IFirecampAttach, IItemSlotParent
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Slots = new List<ItemSlot>(base.GetComponentsInChildren<ItemSlot>());
		this.m_Bowl = base.gameObject.GetComponentInChildren<Bowl>();
		if (this.m_Bowl)
		{
			this.m_Bowl.m_RackChild = true;
		}
		FirecampRack.s_FirecampRacks.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		FirecampRack.s_FirecampRacks.Remove(this);
	}

	public void SetFirecamp(Firecamp firecmap)
	{
		if (this.m_Firecamp == firecmap)
		{
			return;
		}
		this.m_Firecamp = firecmap;
		if (this.m_Bowl)
		{
			if (this.m_Firecamp)
			{
				this.m_Bowl.OnFirecampAdd(this.m_Firecamp);
			}
			else
			{
				this.m_Bowl.OnFirecampRemove(this.m_Firecamp);
			}
		}
	}

	public bool CanInsertItem(Item item)
	{
		if (item.m_Info.m_ID == ItemID.Bamboo_Bowl)
		{
			if (this.m_Bowl)
			{
				return false;
			}
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				if (this.m_Slots[i].IsOccupied())
				{
					return false;
				}
			}
		}
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		Collider component = slot.m_Item.gameObject.GetComponent<Collider>();
		component.isTrigger = true;
		if (slot.m_Item.m_Info.m_Type == ItemType.Bowl)
		{
			Bowl bowl = (Bowl)slot.m_Item;
			bowl.OnFirecampAdd(this.m_Firecamp);
		}
		if (slot.m_Item.m_Info.m_ID == ItemID.Bamboo_Bowl)
		{
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				this.m_Slots[i].m_ActivityUpdate = false;
				this.m_Slots[i].Deactivate();
			}
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (slot && !slot.m_IsBeingDestroyed)
		{
			slot.gameObject.SetActive(true);
		}
		if (slot.m_Item == null)
		{
			return;
		}
		if (slot.m_Item.m_Info.m_ID == ItemID.Bamboo_Bowl)
		{
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				this.m_Slots[i].m_ActivityUpdate = true;
				this.m_Slots[i].Activate();
			}
		}
		if (slot.m_Item.m_Info.m_Type == ItemType.Bowl)
		{
			Bowl bowl = (Bowl)slot.m_Item;
			bowl.OnFirecampRemove(this.m_Firecamp);
		}
	}

	[HideInInspector]
	public Firecamp m_Firecamp;

	public static List<FirecampRack> s_FirecampRacks = new List<FirecampRack>();

	private List<ItemSlot> m_Slots;

	private Bowl m_Bowl;
}
