using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class PlantFruit : Trigger
{
	protected override void Start()
	{
		base.Start();
		DebugUtils.Assert(this.m_InfoName != "None", "m_InfoName of object " + base.name + " is not set!", true, DebugUtils.AssertType.Info);
		this.m_ItemInfo = ItemsManager.Get().GetInfo(this.m_InfoName);
		this.m_Eatable = this.m_ItemInfo.m_Eatable;
		this.m_DefaultLayer = LayerMask.NameToLayer("SmallPlant");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		BalanceSystem20.Get().OnItemDestroyed(this);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Take);
		actions.Add(TriggerAction.TYPE.Expand);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Take)
		{
			this.Take();
			return;
		}
		if (action == TriggerAction.TYPE.Expand)
		{
			HUDItem.Get().Activate(this);
		}
	}

	public bool Take()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return false;
		}
		Item item = ItemsManager.Get().CreateItem(this.m_ItemInfo.m_ID, false, Vector3.zero, Quaternion.identity);
		item.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		item.m_WasTriggered = true;
		if (this.m_Acre)
		{
			this.m_Acre.OnTake(this);
		}
		if (item.Take())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return true;
		}
		UnityEngine.Object.Destroy(item.gameObject);
		return false;
	}

	public void Eat()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return;
		}
		if (this.m_Acre)
		{
			this.m_Acre.OnEat(this);
		}
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), this.m_InfoName);
		ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity).Eat();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override string GetIconName()
	{
		return this.m_ItemInfo.m_IconName;
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_ItemInfo.m_LockedInfoID != string.Empty && !ItemsManager.Get().m_UnlockedItemInfos.Contains(this.m_ItemInfo.m_ID))
		{
			return GreenHellGame.Instance.GetLocalization().Get(this.m_ItemInfo.m_LockedInfoID, true);
		}
		return base.GetTriggerInfoLocalized();
	}

	[HideInInspector]
	public string m_InfoName = string.Empty;

	[HideInInspector]
	public bool m_Eatable;

	[HideInInspector]
	public ItemInfo m_ItemInfo;

	public Acre m_Acre;
}
