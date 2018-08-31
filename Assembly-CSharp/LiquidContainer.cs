using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class LiquidContainer : Item, IItemSlotParent
{
	protected override void Start()
	{
		base.Start();
		this.InitializeSlots();
	}

	private void InitializeSlots()
	{
		float num = 0.2f;
		GameObject gameObject = new GameObject();
		gameObject.name = "GetSlot";
		this.m_GetSlot = gameObject.AddComponent<ItemSlot>();
		Vector3 center = this.m_BoxCollider.bounds.center;
		center.x += this.m_BoxCollider.size.x * num;
		this.m_GetSlot.transform.position = center;
		this.m_GetSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_GetSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_GetSlot.m_ItemTypeList.Add(ItemType.Bowl);
		this.m_GetSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_GetSlot.m_GOParent = base.gameObject;
		this.m_GetSlot.m_ISParents = new IItemSlotParent[1];
		this.m_GetSlot.m_ISParents[0] = this;
		this.m_GetSlot.m_ItemParent = this;
		this.m_GetSlot.SetIcon("HUD_get_water");
		this.m_GetSlot.gameObject.SetActive(false);
		gameObject = new GameObject();
		gameObject.name = "PourSlot";
		this.m_PourSlot = gameObject.AddComponent<ItemSlot>();
		center = this.m_BoxCollider.bounds.center;
		center.x -= this.m_BoxCollider.size.x * num;
		this.m_PourSlot.transform.position = center;
		this.m_PourSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_PourSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_PourSlot.m_ItemTypeList.Add(ItemType.Bowl);
		this.m_PourSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_PourSlot.m_GOParent = base.gameObject;
		this.m_PourSlot.m_ISParents = new IItemSlotParent[1];
		this.m_PourSlot.m_ISParents[0] = this;
		this.m_PourSlot.m_ItemParent = this;
		this.m_PourSlot.SetIcon("HUD_pourOut_water");
		this.m_PourSlot.gameObject.SetActive(false);
	}

	public override void SetupInfo()
	{
		base.SetupInfo();
		this.m_LCInfo = (LiquidContainerInfo)this.m_Info;
	}

	public bool IsEmpty()
	{
		return this.m_LCInfo.m_Amount == 0f;
	}

	public void Fill(LiquidContainer other)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)other.m_Info;
		if (this.m_LCInfo.m_LiquidType != liquidContainerInfo.m_LiquidType)
		{
			if (this.m_LCInfo.m_Amount > 0f)
			{
				return;
			}
			this.m_LCInfo.m_LiquidType = liquidContainerInfo.m_LiquidType;
		}
		float amount = this.m_LCInfo.m_Amount;
		this.m_LCInfo.m_Amount += liquidContainerInfo.m_Amount;
		this.m_LCInfo.m_Amount = Mathf.Clamp(this.m_LCInfo.m_Amount, 0f, this.m_LCInfo.m_Capacity);
		float num = this.m_LCInfo.m_Amount - amount;
		liquidContainerInfo.m_Amount -= num;
		this.OnGet();
		other.OnPour();
	}

	public void Fill(LiquidSource liquid_source)
	{
		if (this.m_LCInfo.m_LiquidType != liquid_source.m_LiquidType)
		{
			if (this.m_LCInfo.m_Amount > 0f)
			{
				return;
			}
			this.m_LCInfo.m_LiquidType = liquid_source.m_LiquidType;
		}
		this.m_LCInfo.m_Amount = this.m_LCInfo.m_Capacity;
		this.OnGet();
	}

	public void Spill(float amount = -1f)
	{
		if (this.m_LCInfo.m_Amount == 0f)
		{
			return;
		}
		this.m_LCInfo.m_Amount = ((amount >= 0f) ? (this.m_LCInfo.m_Amount - amount) : 0f);
		this.m_LCInfo.m_Amount = Mathf.Clamp(this.m_LCInfo.m_Amount, 0f, this.m_LCInfo.m_Capacity);
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	public override void Drink()
	{
		base.Drink();
		Player.Get().GetComponent<EatingController>().Drink(this.m_LCInfo);
		this.OnDrink();
	}

	public virtual void OnGet()
	{
	}

	protected virtual void OnPour()
	{
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (LiquidInHandsController.Get().m_Container != this)
		{
			base.GetActions(actions);
		}
	}

	public override bool CanTrigger()
	{
		return !(LiquidInHandsController.Get().m_Container == this) && base.CanTrigger();
	}

	public virtual bool CanInsertItem(Item item)
	{
		return true;
	}

	public virtual void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_GetSlot || slot == this.m_PourSlot)
		{
			Item item = slot.m_Item;
			if (slot == this.m_PourSlot)
			{
				LiquidContainer.TransferLiquids((LiquidContainer)item, this);
			}
			else if (slot == this.m_GetSlot)
			{
				LiquidContainer.TransferLiquids(this, (LiquidContainer)item);
			}
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		}
	}

	public virtual void OnRemoveItem(ItemSlot slot)
	{
	}

	private static void TransferLiquids(LiquidContainer from, LiquidContainer to)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)from.m_Info;
		LiquidContainerInfo liquidContainerInfo2 = (LiquidContainerInfo)to.m_Info;
		if (liquidContainerInfo.m_LiquidType != liquidContainerInfo2.m_LiquidType)
		{
			if (liquidContainerInfo2.m_Amount > 0f)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("Liquids_Conflict"), null, HUDMessageIcon.None, string.Empty);
				return;
			}
			liquidContainerInfo2.m_LiquidType = liquidContainerInfo.m_LiquidType;
		}
		float amount = liquidContainerInfo2.m_Amount;
		liquidContainerInfo2.m_Amount += liquidContainerInfo.m_Amount;
		liquidContainerInfo2.m_Amount = Mathf.Clamp(liquidContainerInfo2.m_Amount, 0f, liquidContainerInfo2.m_Capacity);
		float num = liquidContainerInfo2.m_Amount - amount;
		liquidContainerInfo.m_Amount -= num;
		to.OnGet();
		from.OnPour();
	}

	public override string GetTriggerInfoLocalized()
	{
		string result = string.Empty;
		Localization localization = GreenHellGame.Instance.GetLocalization();
		if (this.m_LCInfo.m_Amount > 0f)
		{
			result = localization.Get(base.GetName()) + " - " + localization.Get("LiquidType_" + this.m_LCInfo.m_LiquidType.ToString());
		}
		else
		{
			result = localization.Get(base.GetName()) + " - " + localization.Get("LiquidContainer_Empty");
		}
		return result;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSlotsActivity();
	}

	protected virtual void UpdateSlotsActivity()
	{
		if (this.m_GetSlot)
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
			if (liquidContainerInfo.m_Amount > 0f && liquidContainerInfo.m_LiquidType != LiquidType.Water && liquidContainerInfo.m_LiquidType != LiquidType.UnsafeWater && liquidContainerInfo.m_LiquidType != LiquidType.DirtyWater)
			{
				this.m_GetSlot.gameObject.SetActive(false);
			}
			else if (this.m_GetSlot.gameObject.activeSelf)
			{
				if (liquidContainerInfo.m_Amount == 0f)
				{
					this.m_GetSlot.gameObject.SetActive(false);
				}
				else if (!this.m_GetSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
				{
					this.m_GetSlot.gameObject.SetActive(false);
				}
				else
				{
					LiquidContainerInfo liquidContainerInfo2 = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
					if (liquidContainerInfo2.m_Amount >= liquidContainerInfo2.m_Capacity)
					{
						this.m_GetSlot.gameObject.SetActive(false);
					}
				}
			}
			else if (this.m_GetSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				LiquidContainerInfo liquidContainerInfo3 = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
				if (liquidContainerInfo3.m_Amount < liquidContainerInfo3.m_Capacity)
				{
					this.m_GetSlot.gameObject.SetActive(true);
				}
			}
		}
		if (this.m_PourSlot)
		{
			if (this.m_PourSlot.gameObject.activeSelf)
			{
				if (!Inventory3DManager.Get().m_CarriedItem)
				{
					this.m_PourSlot.gameObject.SetActive(false);
				}
				else if (((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_Amount == 0f)
				{
					this.m_PourSlot.gameObject.SetActive(false);
				}
				else if (!this.m_PourSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
				{
					this.m_PourSlot.gameObject.SetActive(false);
				}
				else
				{
					LiquidContainerInfo liquidContainerInfo4 = (LiquidContainerInfo)this.m_Info;
					if (liquidContainerInfo4.m_Amount >= liquidContainerInfo4.m_Capacity)
					{
						this.m_PourSlot.gameObject.SetActive(false);
					}
				}
			}
			else if (this.m_PourSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				LiquidContainerInfo liquidContainerInfo5 = (LiquidContainerInfo)this.m_Info;
				if (liquidContainerInfo5.m_Amount < liquidContainerInfo5.m_Capacity)
				{
					this.m_PourSlot.gameObject.SetActive(true);
				}
			}
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("LCType" + index.ToString(), (int)this.m_LCInfo.m_LiquidType);
		SaveGame.SaveVal("LCAmount" + index.ToString(), this.m_LCInfo.m_Amount);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_LCInfo.m_LiquidType = (LiquidType)SaveGame.LoadIVal("LCType" + index.ToString());
		this.m_LCInfo.m_Amount = SaveGame.LoadFVal("LCAmount" + index.ToString());
	}

	public LiquidContainerInfo m_LCInfo;

	[HideInInspector]
	public ItemSlot m_GetSlot;

	[HideInInspector]
	public ItemSlot m_PourSlot;
}
