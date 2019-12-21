using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class WaterFilter : Construction, IItemSlotParent, IProcessor
{
	protected override void Awake()
	{
		base.Awake();
		base.RegisterConstantUpdateItem();
	}

	protected override void Start()
	{
		base.Start();
		HUDProcess.Get().RegisterProcess(this, this.GetIconName(), this, true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.UnregisterConstantUpdateItem();
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (this.m_TargetContainerSlot == slot)
		{
			((LiquidContainer)slot.m_Item).Spill(-1f);
			this.m_TargetContainerInfo = (LiquidContainerInfo)slot.m_Item.m_Info;
			this.m_TargetContainerInfo.m_LiquidType = LiquidType.Water;
			HUDProcess.Get().RegisterProcess(slot.m_Item, slot.m_Item.GetIconName(), this, false);
			return;
		}
		if (slot.m_Item.m_Info.IsLiquidContainer())
		{
			Item item = slot.m_Item;
			this.Fill((LiquidContainerInfo)item.m_Info);
			slot.RemoveItem();
			if (InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true) != InsertResult.Ok)
			{
				DebugUtils.Assert("Tomuś, do something with this situation!", true, DebugUtils.AssertType.Info);
			}
			if (Inventory3DManager.Get().isActiveAndEnabled)
			{
				Inventory3DManager.Get().OnLiquidTransfer();
			}
		}
	}

	private void Fill(LiquidContainerInfo lc_info)
	{
		if (lc_info.m_Amount == 0f)
		{
			return;
		}
		float amount = this.m_Amount;
		this.m_Amount += lc_info.m_Amount;
		this.m_Amount = Mathf.Clamp(this.m_Amount, 0f, this.m_Capacity);
		float num = this.m_Amount - amount;
		lc_info.m_Amount -= num;
		PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (this.m_TargetContainerSlot == slot)
		{
			this.m_TargetContainerInfo = null;
		}
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		float delta_time = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			delta_time = Player.GetSleepTimeFactor();
		}
		this.UpdateProcessing(delta_time);
		this.UpdateParticles();
	}

	private void UpdateProcessing(float delta_time)
	{
		if (this.m_Amount == 0f)
		{
			return;
		}
		float num = delta_time * this.m_AmountPerSec;
		float amount = this.m_Amount;
		this.m_Amount -= num;
		float num2 = amount - this.m_Amount;
		this.m_Amount = Mathf.Clamp(this.m_Amount, 0f, this.m_Capacity);
		if (this.m_TargetContainerInfo != null)
		{
			this.m_TargetContainerInfo.m_LiquidType = LiquidType.Water;
			this.m_TargetContainerInfo.m_Amount = Mathf.Clamp(this.m_TargetContainerInfo.m_Amount + num2, 0f, this.m_TargetContainerInfo.m_Capacity);
		}
	}

	private void UpdateParticles()
	{
		bool flag = this.m_Amount > 0f;
		if (this.m_Particle.gameObject.activeSelf != flag)
		{
			this.m_Particle.gameObject.SetActive(flag);
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (LiquidInHandsController.Get().IsActive() || BowlController.Get().IsActive())
		{
			actions.Add(TriggerAction.TYPE.Pour);
		}
	}

	public override bool CanTrigger()
	{
		return false;
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Pour)
		{
			if (LiquidInHandsController.Get().IsActive())
			{
				this.Fill(LiquidInHandsController.Get().m_Container.m_LCInfo);
			}
			else if (BowlController.Get().IsActive())
			{
				this.Fill(BowlController.Get().m_Bowl.m_LCInfo);
			}
			if (Inventory3DManager.Get().isActiveAndEnabled)
			{
				Inventory3DManager.Get().OnLiquidTransfer();
			}
		}
	}

	public override void GetInfoText(ref string result)
	{
		result = result + "Capacity = " + this.m_Capacity.ToString() + "\n";
		result = result + "Amount = " + this.m_Amount.ToString() + "\n";
	}

	public float GetProcessProgress(Trigger trigger)
	{
		Item item = (Item)trigger;
		if (item == this)
		{
			return this.m_Amount / this.m_Capacity;
		}
		if (this.m_TargetContainerSlot.m_Item == item)
		{
			return this.m_TargetContainerInfo.m_Amount / this.m_TargetContainerInfo.m_Capacity;
		}
		return 0f;
	}

	public override Vector3 GetIconPos()
	{
		return this.m_IconDummy.position;
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("WTAmount" + index, this.m_Amount);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Amount = SaveGame.LoadFVal("WTAmount" + index);
	}

	public ItemSlot m_TargetContainerSlot;

	private LiquidContainerInfo m_TargetContainerInfo;

	public float m_AmountPerSec;

	private float m_Amount;

	public float m_Capacity;

	public GameObject m_Particle;

	public Transform m_IconDummy;
}
