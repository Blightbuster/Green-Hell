using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class LiquidContainer : Item, IItemSlotParent
{
	protected override void Start()
	{
		base.Start();
		((LiquidContainerInfo)this.m_Info).m_ForceNoDrink = this.m_ForceNoDrink;
		((LiquidContainerInfo)this.m_Info).m_ForceNoSpill = this.m_ForceNoSpill;
		this.InitializeSlots();
	}

	private void InitializeSlots()
	{
		float num = 0.2f;
		Vector3 center = this.m_BoxCollider.bounds.center;
		GameObject gameObject;
		if (this.m_AddGetSlot)
		{
			gameObject = new GameObject();
			gameObject.name = "GetSlot";
			this.m_GetSlot = gameObject.AddComponent<ItemSlot>();
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
			this.m_GetSlot.m_CanSelect = false;
			this.m_GetSlot.gameObject.SetActive(false);
		}
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
		this.m_PourSlot.m_CanSelect = false;
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
		this.ReplRequestOwnership(false);
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
		this.ReplRequestOwnership(false);
		this.OnGet();
	}

	public void Fill(float amount)
	{
		this.ReplRequestOwnership(false);
		this.m_LCInfo.m_Amount += amount;
		this.m_LCInfo.m_Amount = Mathf.Clamp(this.m_LCInfo.m_Amount, 0f, this.m_LCInfo.m_Capacity);
	}

	public virtual void Spill(float amount = -1f)
	{
		if (this.m_LCInfo.m_Amount == 0f)
		{
			return;
		}
		this.ReplRequestOwnership(false);
		this.m_LCInfo.m_Amount = ((amount < 0f) ? 0f : (this.m_LCInfo.m_Amount - amount));
		this.m_LCInfo.m_Amount = Mathf.Clamp(this.m_LCInfo.m_Amount, 0f, this.m_LCInfo.m_Capacity);
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	public override void Drink()
	{
		base.Drink();
		Player.Get().GetComponent<EatingController>().Drink(this.m_LCInfo);
		this.ReplRequestOwnership(false);
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
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !(LiquidInHandsController.Get().m_Container == this) && base.CanTrigger();
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
			if (liquidContainerInfo2.m_Amount >= 1f)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("Liquids_Conflict", true), null, HUDMessageIcon.None, "", null);
				return;
			}
			liquidContainerInfo2.m_LiquidType = liquidContainerInfo.m_LiquidType;
		}
		float amount = liquidContainerInfo2.m_Amount;
		liquidContainerInfo2.m_Amount += liquidContainerInfo.m_Amount;
		liquidContainerInfo2.m_Amount = Mathf.Clamp(liquidContainerInfo2.m_Amount, 0f, liquidContainerInfo2.m_Capacity);
		float num = liquidContainerInfo2.m_Amount - amount;
		liquidContainerInfo.m_Amount -= num;
		to.ReplRequestOwnership(false);
		to.OnGet();
		from.ReplRequestOwnership(false);
		from.OnPour();
	}

	public override string GetTriggerInfoLocalized()
	{
		string result = string.Empty;
		Localization localization = GreenHellGame.Instance.GetLocalization();
		if (this.m_LCInfo.m_Amount >= 1f)
		{
			result = localization.Get(base.GetName(), true) + " - " + localization.Get("LiquidType_" + this.m_LCInfo.m_LiquidType.ToString(), true);
		}
		else
		{
			result = localization.Get(base.GetName(), true) + " - " + localization.Get("LiquidContainer_Empty", true);
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
		if (this.m_Info.m_ID == ItemID.Coconut)
		{
			this.m_GetSlot.gameObject.SetActive(false);
			this.m_PourSlot.gameObject.SetActive(false);
		}
		if (base.m_InInventory)
		{
			this.m_GetSlot.gameObject.SetActive(false);
			this.m_PourSlot.gameObject.SetActive(false);
			return;
		}
		if (this.m_GetSlot)
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
			if (liquidContainerInfo.m_Amount < 1f)
			{
				this.m_GetSlot.gameObject.SetActive(false);
			}
			else if (liquidContainerInfo.m_Amount > 0f && liquidContainerInfo.m_LiquidType != LiquidType.Water && liquidContainerInfo.m_LiquidType != LiquidType.UnsafeWater && liquidContainerInfo.m_LiquidType != LiquidType.DirtyWater)
			{
				this.m_GetSlot.gameObject.SetActive(false);
			}
			else if (this.m_GetSlot.gameObject.activeSelf)
			{
				if (liquidContainerInfo.m_Amount < 1f)
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
				if (Inventory3DManager.Get().m_CarriedItem.m_Info.m_ID == ItemID.Coconut)
				{
					this.m_GetSlot.gameObject.SetActive(false);
				}
				else
				{
					LiquidContainerInfo liquidContainerInfo3 = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
					if (liquidContainerInfo3.m_Amount < liquidContainerInfo3.m_Capacity)
					{
						this.m_GetSlot.gameObject.SetActive(true);
					}
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
					return;
				}
				if (((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_Amount < 1f)
				{
					this.m_PourSlot.gameObject.SetActive(false);
					return;
				}
				if (!this.m_PourSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
				{
					this.m_PourSlot.gameObject.SetActive(false);
					return;
				}
				LiquidContainerInfo liquidContainerInfo4 = (LiquidContainerInfo)this.m_Info;
				if (liquidContainerInfo4.m_Amount >= liquidContainerInfo4.m_Capacity)
				{
					this.m_PourSlot.gameObject.SetActive(false);
					return;
				}
			}
			else if (this.m_PourSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				if ((this.m_Info.m_ID == ItemID.Bidon || this.m_Info.m_ID == ItemID.Coconut_Bidon) && Inventory3DManager.Get().m_CarriedItem.m_Info.IsLiquidContainer() && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_Amount >= 1f && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.Water && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.UnsafeWater && ((LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info).m_LiquidType != LiquidType.DirtyWater)
				{
					this.m_PourSlot.gameObject.SetActive(false);
					return;
				}
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

	public override void OnReplicationPrepare_CJGenerated()
	{
		base.OnReplicationPrepare_CJGenerated();
		if (this.m_LCInfo_m_LiquidType_Repl != this.m_LCInfo.m_LiquidType)
		{
			this.m_LCInfo_m_LiquidType_Repl = this.m_LCInfo.m_LiquidType;
			this.ReplSetDirty();
		}
		if (Math.Abs(this.m_LCInfo_m_Capacity_Repl - this.m_LCInfo.m_Capacity) > 0.1f)
		{
			this.m_LCInfo_m_Capacity_Repl = this.m_LCInfo.m_Capacity;
			this.ReplSetDirty();
		}
		if (Math.Abs(this.m_LCInfo_m_Amount_Repl - this.m_LCInfo.m_Amount) > 0.1f)
		{
			this.m_LCInfo_m_Amount_Repl = this.m_LCInfo.m_Amount;
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		base.OnReplicationSerialize_CJGenerated(writer, initial_state);
		writer.Write((int)this.m_LCInfo_m_LiquidType_Repl);
		writer.Write(this.m_LCInfo_m_Capacity_Repl);
		writer.Write(this.m_LCInfo_m_Amount_Repl);
	}

	public override void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		base.OnReplicationDeserialize_CJGenerated(reader, initial_state);
		this.m_LCInfo_m_LiquidType_Repl = (LiquidType)reader.ReadInt32();
		this.m_LCInfo_m_Capacity_Repl = reader.ReadFloat();
		this.m_LCInfo_m_Amount_Repl = reader.ReadFloat();
	}

	public override void OnReplicationResolve_CJGenerated()
	{
		base.OnReplicationResolve_CJGenerated();
		this.m_LCInfo.m_LiquidType = this.m_LCInfo_m_LiquidType_Repl;
		this.m_LCInfo.m_Capacity = this.m_LCInfo_m_Capacity_Repl;
		this.m_LCInfo.m_Amount = this.m_LCInfo_m_Amount_Repl;
	}

	[Replicate(new string[]
	{
		"field:m_LiquidType",
		"field:m_Capacity",
		"field:m_Amount"
	})]
	public LiquidContainerInfo m_LCInfo;

	[HideInInspector]
	public ItemSlot m_GetSlot;

	[HideInInspector]
	public ItemSlot m_PourSlot;

	public bool m_AddGetSlot = true;

	public bool m_ForceNoDrink;

	public bool m_ForceNoSpill;

	private LiquidType m_LCInfo_m_LiquidType_Repl;

	private float m_LCInfo_m_Capacity_Repl;

	private float m_LCInfo_m_Amount_Repl;
}
