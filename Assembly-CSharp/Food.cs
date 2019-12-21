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
		return !(base.gameObject.GetComponentInParent<FoodProcessor>() != null) && !(Inventory3DManager.Get().m_CarriedItem == this) && !Inventory3DManager.Get().m_StackItems.Contains(this) && ItemsManager.Get().m_ItemsToSetupAfterLoad.Count <= 0 && this.m_FInfo.m_SpoilEffectID != ItemID.None && (!this.m_FInfo.m_SpoilOnlyIfTriggered || this.m_WasTriggered);
	}

	protected override void Update()
	{
		base.Update();
		float num = this.m_FInfo.m_SpoilOnlyIfTriggered ? this.m_FirstTriggerTime : this.m_FInfo.m_CreationTime;
		if (MainLevel.Instance.m_TODSky.Cycle.GameTime - num >= this.m_FInfo.m_SpoilTime && this.CanSpoil())
		{
			this.Spoil();
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
		bool inventoryRotated = this.m_Info.m_InventoryRotated;
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		Item item = ItemsManager.Get().CreateItem(this.m_FInfo.m_SpoilEffectID, !base.m_InInventory && !base.m_InStorage && !base.m_OnCraftingTable, base.transform.position, base.transform.rotation);
		if (Inventory3DManager.Get().m_CarriedItem == this)
		{
			foreach (Item item2 in Inventory3DManager.Get().m_StackItems)
			{
				Quaternion localRotation = item2.transform.localRotation;
				Vector3 localPosition = item2.transform.localPosition;
				item2.transform.parent = item.transform;
				item2.transform.localRotation = localRotation;
				item2.transform.localPosition = localPosition;
			}
			Inventory3DManager.Get().SetCarriedItem(null, false);
			Inventory3DManager.Get().SetCarriedItem(item, false);
			ItemsManager.Get().ActivateItem(item);
		}
		else if (Inventory3DManager.Get().m_StackItems.Contains(this))
		{
			Inventory3DManager.Get().m_StackItems.Remove(this);
			this.UpdateLayer();
			item.transform.parent = item.transform;
			item.transform.localRotation = base.transform.localRotation;
			item.transform.localPosition = base.transform.localPosition;
			Inventory3DManager.Get().m_StackItems.Add(item);
			item.UpdateLayer();
			ItemsManager.Get().ActivateItem(item);
		}
		else if (base.m_CurrentSlot && base.m_CurrentSlot.m_InventoryStackSlot)
		{
			ItemSlot currentSlot = base.m_CurrentSlot;
			InventoryBackpack.Get().m_Items.Remove(this);
			if (this.m_Info.m_InventoryCellsGroup != null)
			{
				this.m_Info.m_InventoryCellsGroup.Remove(this);
			}
			((ItemSlotStack)currentSlot).ReplaceItem(this, item);
			item.gameObject.isStatic = false;
			if (base.m_InInventory)
			{
				InventoryBackpack.Get().m_Items.Add(item);
				item.OnAddToInventory();
				item.gameObject.SetActive(base.gameObject.activeSelf);
				InventoryBackpack.Get().OnInventoryChanged();
			}
			else
			{
				this.m_Storage.m_Items.Add(item);
				item.OnAddToStorage(this.m_Storage);
				item.gameObject.SetActive(base.gameObject.activeSelf);
			}
		}
		else
		{
			if (base.m_InInventory)
			{
				ItemSlot currentSlot2 = base.m_CurrentSlot;
				InventoryCellsGroup inventoryCellsGroup = this.m_Info.m_InventoryCellsGroup;
				List<Item> list = this.m_InventorySlot ? new List<Item>(this.m_InventorySlot.m_Items) : new List<Item>();
				InventoryBackpack.Get().RemoveItem(this, false);
				if (!base.m_CurrentSlot && this.m_InventorySlot && list.Count > 0)
				{
					using (List<Item>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Item item3 = enumerator.Current;
							if (item3.m_Info.m_InventoryCellsGroup != null)
							{
								item3.m_Info.m_InventoryCellsGroup.Remove(item3);
							}
						}
						goto IL_3A3;
					}
				}
				if (base.m_CurrentSlot)
				{
					if (base.m_CurrentSlot.m_InventoryStackSlot)
					{
						base.m_CurrentSlot.RemoveItem(this, false);
					}
					else
					{
						base.m_CurrentSlot.RemoveItem();
					}
				}
				IL_3A3:
				InventoryBackpack.Get().InsertItem(item, currentSlot2, inventoryCellsGroup, true, true, true, true, true);
				if (!item.m_InventorySlot || list.Count <= 0)
				{
					goto IL_5BD;
				}
				using (List<Item>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item item4 = enumerator.Current;
						item.m_InventorySlot.InsertItem(item4);
					}
					goto IL_5BD;
				}
			}
			if (base.m_OnCraftingTable)
			{
				CraftingManager.Get().RemoveItem(this, false);
				CraftingManager.Get().AddItem(item, false);
			}
			else if (base.m_CurrentSlot)
			{
				ItemSlot currentSlot3 = base.m_CurrentSlot;
				currentSlot3.RemoveItem();
				currentSlot3.InsertItem(item);
			}
			else if (base.m_InStorage && this.m_Storage)
			{
				Storage storage = this.m_Storage;
				ItemSlot currentSlot4 = base.m_CurrentSlot;
				InventoryCellsGroup inventoryCellsGroup2 = this.m_Info.m_InventoryCellsGroup;
				List<Item> list2 = this.m_InventorySlot ? new List<Item>(this.m_InventorySlot.m_Items) : new List<Item>();
				storage.RemoveItem(this, false);
				if (!base.m_CurrentSlot && this.m_InventorySlot && list2.Count > 0)
				{
					using (List<Item>.Enumerator enumerator = list2.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Item item5 = enumerator.Current;
							item5.m_Info.m_InventoryCellsGroup.Remove(item5);
						}
						goto IL_559;
					}
				}
				if (base.m_CurrentSlot)
				{
					if (base.m_CurrentSlot.m_InventoryStackSlot)
					{
						base.m_CurrentSlot.RemoveItem(this, false);
					}
					else
					{
						base.m_CurrentSlot.RemoveItem();
					}
				}
				IL_559:
				storage.InsertItem(item, base.m_CurrentSlot, inventoryCellsGroup2, true, true);
				if (item.m_InventorySlot && list2.Count > 0)
				{
					foreach (Item item6 in list2)
					{
						item.m_InventorySlot.InsertItem(item6);
					}
				}
			}
		}
		IL_5BD:
		if (inventoryRotated)
		{
			Inventory3DManager.Get().RotateItem(item, true);
		}
		if (HUDItem.Get().m_Item == this)
		{
			HUDItem.Get().Activate(item);
		}
		UnityEngine.Object.Destroy(base.gameObject);
		item.transform.rotation = rotation;
		item.transform.position = position;
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
			else if (this.m_Storage != null)
			{
				Item item2 = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
				InventoryCellsGroup inventoryCellsGroup = this.m_Info.m_InventoryCellsGroup;
				Storage storage = this.m_Storage;
				storage.RemoveItem(this, false);
				storage.InsertItem(item2, base.m_CurrentSlot, inventoryCellsGroup, true, true);
				item2.gameObject.SetActive(true);
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
		if (this.m_Acre)
		{
			this.m_Acre.OnEat(this);
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
