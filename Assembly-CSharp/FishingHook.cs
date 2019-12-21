using System;
using System.Collections.Generic;
using AIs;
using UnityEngine;

public class FishingHook : MonoBehaviour, IItemSlotParent
{
	private void Awake()
	{
		this.m_BaitSlot.gameObject.SetActive(false);
		this.m_BaitSlot.m_IsHookBaitSlot = true;
		this.m_Item = base.gameObject.GetComponent<Item>();
	}

	public void SetFishingRod(FishingRod fishing_rod)
	{
		this.m_FishingRod = fishing_rod;
		this.m_BaitSlot.gameObject.SetActive(this.m_FishingRod != null);
	}

	public FishingRod GetFishingRod()
	{
		return this.m_FishingRod;
	}

	public void SetFish(Fish fish)
	{
		this.m_Fish = fish;
	}

	public Fish GetFish()
	{
		return this.m_Fish;
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (slot == this.m_BaitSlot)
		{
			if (this.m_Bait)
			{
				this.m_Bait.ItemsManagerRegister(false);
				this.m_Bait = null;
			}
			this.m_Item.m_BlockTrigger = false;
			base.enabled = true;
			if (slot.m_Item)
			{
				slot.m_Item.m_CantSave = false;
			}
		}
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_BaitSlot)
		{
			this.m_Bait = slot.m_Item;
			this.m_Bait.transform.position = this.m_BaitTransform.transform.position;
			this.m_Bait.transform.rotation = this.m_BaitTransform.transform.rotation;
			this.m_Bait.transform.parent = this.m_BaitTransform.transform;
			this.m_Bait.ItemsManagerUnregister();
			this.m_Bait.transform.localScale = this.m_BaitTransform.FindChild(this.m_Bait.GetInfoID().ToString()).transform.localScale;
			base.enabled = false;
			this.m_Item.m_BlockTrigger = true;
			slot.m_Item.m_CantSave = true;
			HintsManager.Get().ShowHint("Cast_FishingRod", 10f);
		}
	}

	public void DeleteBait()
	{
		if (this.m_Bait)
		{
			UnityEngine.Object.Destroy(this.m_Bait.gameObject);
			this.m_Bait = null;
		}
	}

	public bool CanTrigger()
	{
		return !this.m_Item || !this.m_Item.m_CurrentSlot || Inventory3DManager.Get().IsActive();
	}

	private FishingRod m_FishingRod;

	private Fish m_Fish;

	public GameObject m_BaitTransform;

	public ItemSlot m_BaitSlot;

	[HideInInspector]
	public Item m_Bait;

	public List<AI.AIID> m_AvailableFishes = new List<AI.AIID>();

	[HideInInspector]
	public Item m_Item;
}
