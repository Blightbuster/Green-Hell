using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Generator : Trigger, IItemSlotParent
{
	public LiquidType m_LiquidType { get; set; }

	protected override void Awake()
	{
		base.Awake();
		this.m_Slot = base.gameObject.GetComponentInChildren<ItemSlot>();
		this.m_Slot.SetIcon(this.m_IconName);
		Generator.s_AllGenerators.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Generator.s_AllGenerators.Remove(this);
	}

	public bool CanInsertItem(Item item)
	{
		return this.m_FuelAmount < this.m_FuelCapacity;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		LiquidContainer liquidContainer = (LiquidContainer)slot.m_Item;
		if (liquidContainer.m_LCInfo.m_LiquidType != this.m_LiquidType)
		{
			this.m_FuelAmount += liquidContainer.m_LCInfo.m_Amount;
			this.m_FuelAmount = Mathf.Clamp(this.m_FuelAmount, 0f, this.m_FuelCapacity);
			liquidContainer.m_LCInfo.m_Amount = 0f;
		}
		Item item = slot.m_Item;
		slot.RemoveItem();
		InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		if (this.IsFull())
		{
			slot.Deactivate();
			slot.m_ActivityUpdate = false;
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public bool IsFull()
	{
		return this.m_FuelAmount >= this.m_FuelCapacity;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.TurnOn);
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && this.IsFull();
	}

	public void SaveGenerator()
	{
		SaveGame.SaveVal(base.name, this.m_FuelAmount);
	}

	public void LoadGenerator()
	{
		this.m_FuelAmount = SaveGame.LoadFVal(base.name);
		if (this.IsFull())
		{
			this.m_Slot.Deactivate();
			this.m_Slot.m_ActivityUpdate = false;
			return;
		}
		this.m_Slot.Activate();
		this.m_Slot.m_ActivityUpdate = true;
	}

	public float m_FuelAmount;

	public float m_FuelCapacity;

	public string m_IconName = string.Empty;

	private ItemSlot m_Slot;

	public static List<Generator> s_AllGenerators = new List<Generator>();
}
