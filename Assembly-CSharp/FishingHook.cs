using System;
using AIs;
using UnityEngine;

public class FishingHook : MonoBehaviour, IItemSlotParent
{
	public void SetFishingRod(FishingRod fishing_rod)
	{
		this.m_FishingRod = fishing_rod;
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
		if (this.m_Bait)
		{
			this.m_Bait.enabled = true;
			this.m_Bait.ItemsManagerRegister(false);
			this.m_Bait = null;
		}
		slot.gameObject.SetActive(true);
		slot.Activate();
	}

	public void OnInsertItem(ItemSlot slot)
	{
		this.m_Bait = slot.m_Item;
		this.m_Bait.transform.position = this.m_BaitTransform.transform.position;
		this.m_Bait.transform.rotation = this.m_BaitTransform.transform.rotation;
		this.m_Bait.transform.parent = this.m_BaitTransform.transform;
		this.m_Bait.ItemsManagerUnregister();
		this.m_Bait.enabled = false;
		slot.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (!this.m_Bait)
		{
			this.m_BaitSlot.gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		this.m_BaitSlot.gameObject.SetActive(false);
	}

	public void DeleteBait()
	{
		UnityEngine.Object.Destroy(this.m_Bait.gameObject);
		this.m_Bait = null;
	}

	private FishingRod m_FishingRod;

	private Fish m_Fish;

	public GameObject m_BaitTransform;

	public ItemSlot m_BaitSlot;

	[HideInInspector]
	public Item m_Bait;
}
