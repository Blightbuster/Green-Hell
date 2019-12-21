using System;
using System.Collections.Generic;

public class WeaponRack : Construction
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Slots = new List<ItemSlot>(base.gameObject.GetComponentsInChildren<ItemSlot>());
	}

	public void InsertWeapon(Item weapon)
	{
		foreach (ItemSlot itemSlot in this.m_Slots)
		{
			if (itemSlot.CanInsertItem(weapon))
			{
				itemSlot.InsertItem(weapon);
				break;
			}
		}
	}

	public override void DestroyMe(bool check_connected = true)
	{
		foreach (ItemSlot itemSlot in this.m_Slots)
		{
			itemSlot.RemoveItem();
		}
		base.DestroyMe(check_connected);
	}

	private List<ItemSlot> m_Slots;
}
