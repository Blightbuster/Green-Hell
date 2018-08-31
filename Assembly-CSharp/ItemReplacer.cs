using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ItemReplacer : Trigger
{
	protected override void Start()
	{
		base.Start();
		this.m_CanBeOutlined = true;
		this.m_ReplaceInfo = ItemsManager.Get().GetInfo(this.m_ReplaceInfoName);
		if (this.m_ReplaceByDistance)
		{
			ItemReplacer.s_ToreplaceByDistance.Add(this);
		}
	}

	public override bool IsItemReplacer()
	{
		return true;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (!this.m_ReplaceInfo.IsHeavyObject() || currentItem.m_Info.m_ID != this.m_ReplaceInfo.m_ID)
			{
				return;
			}
		}
		actions.Add(TriggerAction.TYPE.Take);
		actions.Add(TriggerAction.TYPE.Expand);
	}

	public override string GetName()
	{
		return this.m_ReplaceInfoName;
	}

	public override string GetIconName()
	{
		return this.m_ReplaceInfo.m_IconName;
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Expand)
		{
			HUDItem.Get().Activate(this);
		}
		else
		{
			Item item = this.ReplaceItem(true);
			if (item)
			{
				item.OnExecute(action);
				if (!item.m_InInventory && !item.m_Info.IsHeavyObject())
				{
					Inventory3DManager.Get().DropItem(item);
				}
			}
		}
	}

	public override bool CanExecuteActions()
	{
		if (!base.CanExecuteActions())
		{
			return false;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return true;
		}
		HeavyObject heavyObject = null;
		if (this.m_ReplaceInfo.IsHeavyObject() && currentItem.m_Info.IsHeavyObject())
		{
			heavyObject = (HeavyObject)currentItem;
		}
		return (!heavyObject || currentItem.m_Info.m_ID == this.m_ReplaceInfo.m_ID) && (!heavyObject || heavyObject.FindFreeSlot());
	}

	public Item ReplaceItem(bool can_disappear = true)
	{
		if (this.m_Hallucination)
		{
			base.Disappear(true);
			return null;
		}
		this.OnReplaceItem();
		Item result = ItemsManager.Get().CreateItem(this.m_ReplaceInfo.m_ID, false, Vector3.zero, Quaternion.identity);
		if (!this.m_IsThisUnlimited)
		{
			base.TryRemoveFromFallenObjectsMan();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		return result;
	}

	private void OnReplaceItem()
	{
		for (int i = 0; i < this.m_DestroyOnReplace.Count; i++)
		{
			GameObject gameObject = this.m_DestroyOnReplace[i];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		ItemReplacer.s_ToreplaceByDistance.Remove(this);
	}

	public static void UpdateByDistance()
	{
		for (int i = 0; i < ItemReplacer.s_ToreplaceByDistance.Count; i++)
		{
			if (ItemReplacer.s_ToreplaceByDistance[i] == null)
			{
				ItemReplacer.s_ToreplaceByDistance.RemoveAt(i);
				return;
			}
			if (Player.Get().transform.position.Distance(ItemReplacer.s_ToreplaceByDistance[i].transform.position) > 20f)
			{
				ItemReplacer.s_ToreplaceByDistance[i].ReplaceItem(false);
				ItemReplacer.s_ToreplaceByDistance.RemoveAt(i);
				return;
			}
		}
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_ReplaceInfo.m_LockedInfoID != string.Empty && !ItemsManager.Get().m_UnlockedItemInfos.Contains(this.m_ReplaceInfo.m_ID))
		{
			return GreenHellGame.Instance.GetLocalization().Get(this.m_ReplaceInfo.m_LockedInfoID);
		}
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName());
	}

	public ItemInfo m_ReplaceInfo;

	public bool m_IsThisUnlimited;

	[HideInInspector]
	public string m_ReplaceInfoName = string.Empty;

	public static List<ItemReplacer> s_ToreplaceByDistance = new List<ItemReplacer>();

	public bool m_ReplaceByDistance;

	public List<GameObject> m_DestroyOnReplace = new List<GameObject>();
}
