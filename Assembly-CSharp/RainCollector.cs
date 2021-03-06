﻿using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class RainCollector : Trigger, IRainCollector, IProcessor
{
	protected override void Awake()
	{
		base.Awake();
		RainCollector.s_AllRainCollectors.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RainCollector.s_AllRainCollectors.Remove(this);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Drink)
		{
			this.Drink();
		}
	}

	private void Drink()
	{
		float num = Mathf.Min(this.m_Amount, this.m_SipHydration);
		if (num > 0f)
		{
			Player.Get().GetComponent<EatingController>().Drink(LiquidType.Water, num);
			this.m_Amount -= num;
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_ContainerSlot.m_Item && this.m_Amount > 0.1f)
		{
			actions.Add(TriggerAction.TYPE.Drink);
		}
	}

	public override string GetName()
	{
		return GreenHellGame.Instance.GetLocalization().Get("Water", true);
	}

	public override Vector3 GetIconPos()
	{
		return TriggerController.Get().GetBestTriggerHitPos();
	}

	public override bool OnlyInCrosshair()
	{
		return true;
	}

	public override bool CanExecuteActions()
	{
		return !this.m_CantExecuteActionsDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying();
	}

	public void Pour(float fill_amount)
	{
		if (!this.m_ContainerSlot.m_Item)
		{
			return;
		}
		BowlInfo bowlInfo = (BowlInfo)this.m_ContainerSlot.m_Item.m_Info;
		bowlInfo.m_Amount += fill_amount;
		bowlInfo.m_Amount = Mathf.Min(bowlInfo.m_Amount, bowlInfo.m_Capacity);
		this.m_Amount += fill_amount;
		this.m_Amount = Mathf.Min(this.m_Amount, this.m_Capacity);
	}

	protected override void Update()
	{
		base.Update();
		if (!this.m_IsContainer && this.m_ContainerSlot.m_Item)
		{
			this.m_IsContainer = true;
			HUDProcess.Get().RegisterProcess(this.m_ContainerSlot.m_Item, this.m_ContainerSlot.m_Item.GetIconName(), this, false);
		}
	}

	public float GetProcessProgress(Trigger trigger)
	{
		if (this.m_ContainerSlot.m_Item == (Item)trigger)
		{
			return this.m_Amount / this.m_Capacity;
		}
		return 0f;
	}

	public float m_Capacity = 100f;

	public float m_Amount;

	public float m_SipHydration = 20f;

	public ItemSlot m_ContainerSlot;

	private bool m_IsContainer;

	public static List<RainCollector> s_AllRainCollectors = new List<RainCollector>();
}
