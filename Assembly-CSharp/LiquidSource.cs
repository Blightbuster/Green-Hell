using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class LiquidSource : Trigger, ITriggerThrough, IItemSlotParent
{
	protected override void Start()
	{
		base.Start();
		if (this.m_DisableCollisions)
		{
			Collider component = base.gameObject.GetComponent<Collider>();
			if (component != null)
			{
				component.isTrigger = true;
			}
			else if (base.gameObject.GetComponent<RamSpline>() == null)
			{
				DebugUtils.Assert("Water source has no collider " + base.gameObject.name, true, DebugUtils.AssertType.Info);
			}
		}
		if (DebugUtils.Assert(this.m_LiquidType > LiquidType.None, "[WaterSource:Start] LiquidType of object " + base.name + " is not set. LiquidType will be set to default UnsafeWater.", true, DebugUtils.AssertType.Info))
		{
			this.m_LiquidType = LiquidType.UnsafeWater;
		}
		this.InitializeSlots();
		base.gameObject.AddComponent<Rigidbody>().isKinematic = true;
		this.m_CanBeOutlined = false;
		base.gameObject.tag = "LiquidSource";
	}

	private void InitializeSlots()
	{
		if (!this.m_BoxCollider)
		{
			return;
		}
		GameObject gameObject = new GameObject();
		gameObject.name = "GetSlot";
		this.m_GetSlot = gameObject.AddComponent<ItemSlot>();
		Vector3 position = base.transform.position;
		position.y += this.m_BoxCollider.size.y;
		position.x += this.m_BoxCollider.size.x * 0.5f;
		this.m_GetSlot.transform.position = position;
		this.m_GetSlot.transform.rotation = base.transform.rotation;
		gameObject.transform.parent = base.transform;
		this.m_GetSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
		this.m_GetSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_GetSlot.m_GOParent = base.gameObject;
		this.m_GetSlot.m_ISParents = new IItemSlotParent[1];
		this.m_GetSlot.m_ISParents[0] = this;
		this.m_GetSlot.m_ForceTransformPos = true;
		this.m_GetSlot.SetIcon("HUD_get_water");
		this.m_GetSlot.m_CanSelect = false;
	}

	public override bool IsLiquidSource()
	{
		return true;
	}

	public override bool CheckRange()
	{
		return false;
	}

	public override bool CheckDot()
	{
		return false;
	}

	public override bool OnlyInCrosshair()
	{
		return true;
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Fill)
		{
			this.TakeLiquid();
			return;
		}
		if (action == TriggerAction.TYPE.Expand)
		{
			HUDItem.Get().Activate(this);
			return;
		}
		if (action == TriggerAction.TYPE.Drink || action == TriggerAction.TYPE.DrinkHold)
		{
			this.Drink();
		}
	}

	public void Drink()
	{
		Player.Get().GetComponent<EatingController>().Drink(this.m_LiquidType, this.m_SipHydration);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.DrinkHold);
		actions.Add(TriggerAction.TYPE.Expand);
	}

	public override string GetName()
	{
		return "LiquidType_" + this.m_LiquidType.ToString();
	}

	public override string GetIconName()
	{
		return "HUD_drinking_water";
	}

	public override Vector3 GetIconPos()
	{
		return TriggerController.Get().GetBestTriggerHitPos();
	}

	public override bool CanExecuteActions()
	{
		return base.CanExecuteActions() && !SwimController.Get().IsActive();
	}

	public void TakeLiquid()
	{
		if (BowlController.Get().IsActive())
		{
			BowlController.Get().TakeLiquid(this);
			return;
		}
		LiquidInHandsController.Get().TakeLiquid(this);
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (SwimController.Get().IsActive())
		{
			return false;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		return !currentItem || !currentItem.m_Info.IsHeavyObject();
	}

	public virtual bool CanInsertItem(Item item)
	{
		return true;
	}

	public virtual void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_GetSlot)
		{
			Item item = slot.m_Item;
			this.TransferLiquids((LiquidContainer)item);
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		}
	}

	public virtual void OnRemoveItem(ItemSlot slot)
	{
	}

	private void TransferLiquids(LiquidContainer to)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)to.m_Info;
		if (this.m_LiquidType != liquidContainerInfo.m_LiquidType)
		{
			if (liquidContainerInfo.m_Amount > 0f)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("Liquids_Conflict", true), null, HUDMessageIcon.None, "", null);
				return;
			}
			liquidContainerInfo.m_LiquidType = this.m_LiquidType;
		}
		liquidContainerInfo.m_Amount = liquidContainerInfo.m_Capacity;
		to.OnGet();
		to.ReplRequestOwnership(false);
	}

	protected override void Update()
	{
		base.Update();
		if (!this.m_GetSlot)
		{
			return;
		}
		this.UpdateSlotActivity();
		this.UpdateSlotPos();
	}

	private void UpdateSlotActivity()
	{
		if (!Inventory3DManager.Get().m_CarriedItem)
		{
			this.m_GetSlot.gameObject.SetActive(false);
			return;
		}
		if (Inventory3DManager.Get().m_CarriedItem.m_Info.m_ID == ItemID.Coconut)
		{
			this.m_GetSlot.gameObject.SetActive(false);
			return;
		}
		if (this.m_GetSlot.gameObject.activeSelf)
		{
			if (!this.m_GetSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
			{
				this.m_GetSlot.gameObject.SetActive(false);
				return;
			}
		}
		else if (this.m_GetSlot.CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)Inventory3DManager.Get().m_CarriedItem.m_Info;
			if (liquidContainerInfo.m_Amount < liquidContainerInfo.m_Capacity)
			{
				this.m_GetSlot.gameObject.SetActive(true);
			}
		}
	}

	private void UpdateSlotPos()
	{
		if (this.m_GetSlot.gameObject.activeSelf)
		{
			int num = Physics.RaycastNonAlloc(Camera.main.transform.position, Camera.main.transform.forward, LiquidSource.s_RaycastHitsTmp, ItemSlot.s_DistToActivate);
			for (int i = 0; i < num; i++)
			{
				if (LiquidSource.s_RaycastHitsTmp[i].collider.gameObject == base.gameObject)
				{
					this.m_GetSlot.transform.position = LiquidSource.s_RaycastHitsTmp[i].point;
					return;
				}
			}
			this.m_GetSlot.transform.position = base.m_Collider.ClosestPointOnBounds(Player.Get().transform.position);
		}
	}

	public void TakeClay()
	{
		ItemsManager.Get().CreateItem(ItemID.mud_from_water, true, Player.Get().transform).Take();
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	public float m_SipHydration = 20f;

	public LiquidType m_LiquidType;

	public bool m_DisableCollisions = true;

	[HideInInspector]
	public ItemSlot m_GetSlot;

	private static RaycastHit[] s_RaycastHitsTmp = new RaycastHit[20];
}
