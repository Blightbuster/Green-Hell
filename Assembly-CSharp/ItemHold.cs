using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ItemHold : Item
{
	public override bool IsItemHold()
	{
		return true;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.TakeHold);
	}

	public override void GetInfoText(ref string result)
	{
	}

	public override bool Take()
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return false;
		}
		Item item = ItemsManager.Get().CreateItem(this.m_ReplaceInfoName, false);
		InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		if (insertResult != InventoryBackpack.InsertResult.Ok)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return false;
		}
		EventsManager.OnEvent(Enums.Event.TakeItem, 1, (int)item.m_Info.m_ID);
		if (this.m_CurrentSlot)
		{
			this.m_CurrentSlot.RemoveItem();
		}
		Player.Get().GetComponent<PlayerAudioModule>().PlayItemSound(item.m_Info.m_GrabSound);
		base.AddItemsCountMessage(item);
		if (item.m_Info != null && item.m_Info.IsHeavyObject())
		{
			PlayerAudioModule.Get().PlayHOPickupSound();
		}
		if (!this.m_IsThisUnlimited)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		return true;
	}

	public ItemInfo m_ReplaceItem;

	public bool m_IsThisUnlimited;

	[HideInInspector]
	public string m_ReplaceInfoName = string.Empty;
}
