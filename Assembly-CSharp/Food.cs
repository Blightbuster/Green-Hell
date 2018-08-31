using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Food : Consumable
{
	public override void SetupInfo()
	{
		base.SetupInfo();
		this.m_FInfo = (FoodInfo)this.m_Info;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Food.s_AllFoods.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Food.s_AllFoods.Remove(this);
	}

	public bool CanSpoil()
	{
		return this.m_FInfo.m_SpoilEffectID != ItemID.None && (!this.m_FInfo.m_SpoilOnlyIfTriggered || this.m_WasTriggered);
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_CurrentSlot && this.m_CurrentSlot.m_FoodProcessorChild)
		{
			return;
		}
		if (this.CanSpoil())
		{
			float num = (!this.m_FInfo.m_SpoilOnlyIfTriggered) ? this.m_FInfo.m_CreationTime : this.m_FirstTriggerTime;
			bool flag = MainLevel.Instance.m_TODSky.Cycle.GameTime - num >= this.m_FInfo.m_SpoilTime;
			if (flag)
			{
				this.Spoil();
			}
		}
	}

	private void Spoil()
	{
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			return;
		}
		if (this.m_FInfo.m_SpoilEffectID == ItemID.None)
		{
			return;
		}
		Item item = ItemsManager.Get().CreateItem(this.m_FInfo.m_SpoilEffectID, !this.m_InInventory && !this.m_OnCraftingTable, base.transform.position, base.transform.rotation);
		if (this.m_InInventory)
		{
			InventoryBackpack.Get().RemoveItem(this, false);
			if (this.m_Info.m_InventoryCellsGroup != null)
			{
				this.m_Info.m_InventoryCellsGroup.Remove(this);
			}
			if (!this.m_CurrentSlot && this.m_InventorySlot && this.m_InventorySlot.m_Items.Count > 0)
			{
				this.m_InventorySlot.RemoveItem(this, false);
			}
			else if (this.m_CurrentSlot)
			{
				if (this.m_CurrentSlot.m_InventoryStackSlot)
				{
					this.m_CurrentSlot.RemoveItem(this, false);
				}
				else
				{
					this.m_CurrentSlot.RemoveItem();
				}
			}
			InventoryBackpack.Get().InsertItem(item, this.m_CurrentSlot, this.m_Info.m_InventoryCellsGroup, true, true, true, true, true);
			if (this.m_InventorySlot)
			{
				this.m_InventorySlot.m_Blocked = true;
				if (item.m_InventorySlot)
				{
					for (int i = 0; i < this.m_InventorySlot.m_Items.Count; i++)
					{
						item.m_InventorySlot.InsertItem(this.m_InventorySlot.m_Items[i]);
					}
				}
				else
				{
					while (this.m_InventorySlot.m_Items.Count > 0)
					{
						Item item2 = this.m_InventorySlot.m_Items[0];
						InventoryBackpack.Get().RemoveItem(item2, false);
						InventoryBackpack.Get().InsertItem(item2, null, null, true, true, true, true, true);
					}
				}
			}
		}
		else if (this.m_OnCraftingTable)
		{
			CraftingManager.Get().RemoveItem(this);
			CraftingManager.Get().AddItem(item, false);
		}
		if (HUDItem.Get().m_Item == this)
		{
			HUDItem.Get().Activate(item);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void Eat()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return;
		}
		base.Eat();
		if (base.transform.parent != null)
		{
			DestroyIfNoChildren component = base.transform.parent.GetComponent<DestroyIfNoChildren>();
			if (component)
			{
				component.OnObjectDestroyed();
			}
		}
		Player.Get().GetComponent<EatingController>().Eat(this.m_FInfo);
		UnityEngine.Object.Destroy(base.gameObject);
		bool flag = InventoryBackpack.Get().Contains(this);
		List<ItemID> eatingResultItems = ((FoodInfo)this.m_Info).m_EatingResultItems;
		for (int i = 0; i < eatingResultItems.Count; i++)
		{
			ItemID item_id = eatingResultItems[i];
			if (flag)
			{
				Item item = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
				InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, false);
			}
			else
			{
				GameObject prefab = GreenHellGame.Instance.GetPrefab(item_id.ToString());
				if (!prefab)
				{
					DebugUtils.Assert("[Item:Harvest] Can't find prefab - " + item_id.ToString(), true, DebugUtils.AssertType.Info);
				}
				else
				{
					UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position, base.transform.rotation);
				}
			}
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ItemBurned" + index, this.m_Burned);
		SaveGame.SaveVal("ItemmProcessDuration" + index, this.m_ProcessDuration);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Burned = SaveGame.LoadBVal("ItemBurned" + index);
		this.m_ProcessDuration = SaveGame.LoadFVal("ItemmProcessDuration" + index);
	}

	public static List<Food> s_AllFoods = new List<Food>();

	[HideInInspector]
	public FoodInfo m_FInfo;

	public bool m_Burned;

	[HideInInspector]
	public float m_ProcessDuration;
}
