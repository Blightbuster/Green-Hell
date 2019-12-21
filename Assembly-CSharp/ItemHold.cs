using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ItemHold : Item
{
	protected override void Awake()
	{
		base.Awake();
		if (this.m_ReplaceInfoName != string.Empty)
		{
			this.m_ReplaceInfoID = EnumUtils<ItemID>.GetValue(this.m_ReplaceInfoName);
		}
		ItemHold.s_AllItemHolds.Add(this);
	}

	protected override void Start()
	{
		base.Start();
		if (this.m_IgnoreCollisionWithPlayer)
		{
			UnityEngine.Object.Destroy(base.m_Collider);
			MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
			meshCollider.convex = true;
			base.m_Collider = meshCollider;
			Physics.IgnoreCollision(base.m_Collider, Player.Get().m_Collider);
		}
	}

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
		actions.Add(this.m_ActionType);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ItemHold.s_AllItemHolds.Remove(this);
	}

	public static ItemHold FindByID(ItemID id)
	{
		foreach (ItemHold itemHold in ItemHold.s_AllItemHolds)
		{
			if (itemHold.m_Info != null)
			{
				if (itemHold.GetInfoID() == id)
				{
					return itemHold;
				}
			}
			else if (EnumUtils<ItemID>.GetValue(itemHold.m_InfoName) == id)
			{
				return itemHold;
			}
		}
		return null;
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
		if (InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true) != InsertResult.Ok)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			return false;
		}
		this.UpdateChildrenItems();
		EventsManager.OnEvent(Enums.Event.TakeItem, 1, (int)item.m_Info.m_ID);
		if (base.m_CurrentSlot)
		{
			base.m_CurrentSlot.RemoveItem();
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

	private void UpdateChildrenItems()
	{
		if (this.m_Info.m_ID == ItemID.Bird_Nest_ToHoldHarvest)
		{
			Item[] componentsInChildren = base.GetComponentsInChildren<Item>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i] == null) && componentsInChildren[i].m_Info.m_ID != ItemID.Bird_Nest_ToHoldHarvest)
				{
					ItemsManager.Get().CreateItem(componentsInChildren[i].m_Info.m_ID, true, componentsInChildren[i].transform.position, componentsInChildren[i].transform.rotation);
				}
			}
		}
	}

	public bool m_IsThisUnlimited;

	[HideInInspector]
	public string m_ReplaceInfoName = string.Empty;

	[HideInInspector]
	public ItemID m_ReplaceInfoID = ItemID.None;

	public bool m_IgnoreCollisionWithPlayer;

	public TriggerAction.TYPE m_ActionType = TriggerAction.TYPE.TakeHold;

	public static HashSet<ItemHold> s_AllItemHolds = new HashSet<ItemHold>();
}
