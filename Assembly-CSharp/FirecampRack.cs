using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class FirecampRack : Construction, IItemSlotParent, IFirecampAttach
{
	protected override void Awake()
	{
		base.Awake();
		this.m_NameHash = Animator.StringToHash(base.name);
		this.m_Slots = new List<ItemSlot>(base.GetComponentsInChildren<ItemSlot>());
		foreach (Bowl bowl in base.gameObject.GetComponentsInChildren<Bowl>())
		{
			bowl.m_RackChild = true;
			this.m_Bowls.Add(bowl);
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
		if (firecmap != null && this.m_BlockFirecampGiveDamage)
		{
			firecmap.m_BlockGivingDamage = true;
		}
		if (this.m_Firecamp == firecmap)
		{
			return;
		}
		this.m_Firecamp = firecmap;
		foreach (Bowl bowl in this.m_Bowls)
		{
			if (this.m_Firecamp)
			{
				bowl.OnFirecampAdd(this.m_Firecamp);
			}
			else
			{
				bowl.OnFirecampRemove(this.m_Firecamp);
			}
		}
		if (this.m_Info.m_ID == ItemID.Stone_Ring)
		{
			firecmap.SetStoneRing(this);
		}
	}

	public bool CanInsertItem(Item item)
	{
		if (item.m_Info.m_ID == ItemID.Bamboo_Bowl)
		{
			using (List<Bowl>.Enumerator enumerator = this.m_Bowls.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_RackChild)
					{
						return false;
					}
				}
			}
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				if (this.m_Slots[i].IsOccupied())
				{
					return false;
				}
			}
			return true;
		}
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		slot.m_Item.gameObject.GetComponent<Collider>().isTrigger = true;
		if (slot.m_Item.m_Info.m_Type == ItemType.Bowl)
		{
			Bowl bowl = (Bowl)slot.m_Item;
			bowl.OnFirecampAdd(this.m_Firecamp);
			this.m_Bowls.Add(bowl);
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
			this.m_Bowls.Remove(bowl);
		}
	}

	public bool IsBurning()
	{
		return this.m_Firecamp && this.m_Firecamp.IsBurning();
	}

	public void BurnoutFirecamp()
	{
		if (this.m_Firecamp)
		{
			this.m_Firecamp.BurnOut();
		}
	}

	public void SetEndlessFire()
	{
		if (this.m_Firecamp)
		{
			this.m_Firecamp.m_EndlessFire = true;
		}
	}

	public void ResetEndlessFire()
	{
		if (this.m_Firecamp)
		{
			this.m_Firecamp.m_EndlessFire = false;
		}
	}

	public override void DestroyMe(bool check_connected = true)
	{
		base.DestroyMe(check_connected);
		foreach (ItemSlot itemSlot in this.m_Slots)
		{
			itemSlot.RemoveItem();
		}
	}

	[HideInInspector]
	public Firecamp m_Firecamp;

	public static List<FirecampRack> s_FirecampRacks = new List<FirecampRack>();

	private List<ItemSlot> m_Slots;

	private List<Bowl> m_Bowls = new List<Bowl>();

	public Transform m_FirecampDummy;

	[HideInInspector]
	public int m_NameHash;

	public bool m_BlockFirecampGiveDamage;
}
