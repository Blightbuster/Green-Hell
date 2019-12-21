using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class FoodProcessor : MonoBehaviour, IItemSlotParent, IFirecampAttach, IProcessor, ITriggerThrough
{
	public void SetFirecamp(Firecamp firecmap)
	{
		this.m_ConnectedFirecamp = firecmap;
	}

	private void Awake()
	{
		FoodProcessor.s_AllFoodProcessors.Add(this);
		this.m_Firecamp = base.GetComponent<Firecamp>();
	}

	private void OnDestroy()
	{
		FoodProcessor.s_AllFoodProcessors.Remove(this);
	}

	public void UpdateProcessing()
	{
		if (!ItemsManager.Get())
		{
			return;
		}
		float num = MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
		if (HUDSleeping.Get().GetState() == HUDSleepingState.Progress)
		{
			num = SleepController.Get().m_HoursDelta;
		}
		else if (ConsciousnessController.Get().IsUnconscious())
		{
			num = ConsciousnessController.Get().m_HoursDelta;
		}
		foreach (ItemSlot itemSlot in this.m_ActiveSlots)
		{
			Food food = (Food)itemSlot.m_Item;
			if (this.m_Firecamp)
			{
				if (!this.m_Firecamp.m_Burning)
				{
					if (food.m_ProcessDuration > 0f)
					{
						food.m_ProcessDuration -= num;
					}
					else
					{
						food.m_ProcessDuration = 0f;
					}
					if (!itemSlot.gameObject.activeSelf)
					{
						itemSlot.gameObject.SetActive(true);
					}
					if (!itemSlot.m_Item.enabled)
					{
						itemSlot.m_Item.enabled = true;
						continue;
					}
					continue;
				}
			}
			else if (this.m_Type != FoodProcessor.Type.Dryer && (!this.m_ConnectedFirecamp || !this.m_ConnectedFirecamp.m_Burning))
			{
				if (food.m_ProcessDuration > 0f)
				{
					food.m_ProcessDuration -= num;
				}
				else
				{
					food.m_ProcessDuration = 0f;
				}
				if (!itemSlot.gameObject.activeSelf)
				{
					itemSlot.gameObject.SetActive(true);
				}
				if (!itemSlot.m_Item.enabled)
				{
					itemSlot.m_Item.enabled = true;
					continue;
				}
				continue;
			}
			if (this.m_Type == FoodProcessor.Type.Dryer)
			{
				if (!RainManager.Get().IsRain() || RainManager.Get().IsInRainCutter(base.transform.position))
				{
					food.m_ProcessDuration += num;
				}
				else if (food.m_ProcessDuration > 0f)
				{
					food.m_ProcessDuration -= num;
				}
				else
				{
					food.m_ProcessDuration = 0f;
				}
			}
			else
			{
				food.m_ProcessDuration += num;
			}
			FoodInfo foodInfo = (FoodInfo)itemSlot.m_Item.m_Info;
			if (food.m_ProcessDuration >= this.GetProcessingTime(foodInfo) || this.m_DebugImmediate)
			{
				HUDProcess.Get().UnregisterProcess(itemSlot.m_Item);
				Item item = ItemsManager.Get().CreateItem(this.GetResultItemID(foodInfo), true, itemSlot.m_Item.transform.position, itemSlot.m_Item.transform.rotation);
				itemSlot.ReplaceItem(item);
				if (!this.m_ProcessedSlots.Contains(itemSlot))
				{
					this.m_ProcessedSlots.Add(itemSlot);
				}
				this.m_DebugImmediate = false;
				if (this.m_Type == FoodProcessor.Type.Fire && !foodInfo.m_CanCook)
				{
					food.m_Burned = true;
					break;
				}
				break;
			}
		}
	}

	private ItemID GetResultItemID(FoodInfo info)
	{
		switch (this.m_Type)
		{
		case FoodProcessor.Type.Smoker:
			return info.m_SmokingItemID;
		case FoodProcessor.Type.Dryer:
			return info.m_DryingItemID;
		case FoodProcessor.Type.Fire:
			if (info.m_CanCook)
			{
				return info.m_CookingItemID;
			}
			if (!GreenHellGame.ROADSHOW_DEMO)
			{
				return info.m_BurningItemID;
			}
			return ItemID.None;
		default:
			return ItemID.None;
		}
	}

	private float GetProcessingTime(FoodInfo info)
	{
		switch (this.m_Type)
		{
		case FoodProcessor.Type.Smoker:
			return info.m_SmokingLength;
		case FoodProcessor.Type.Dryer:
			return info.m_DryingLength;
		case FoodProcessor.Type.Fire:
			if (info.m_CanCook)
			{
				return info.m_CookingLength * Skill.Get<CookingSkill>().GetCookingDurationMul();
			}
			return info.m_BurningLength * Skill.Get<CookingSkill>().GetBurningDurationMul();
		default:
			return 0f;
		}
	}

	public virtual bool CanInsertItem(Item item)
	{
		if (!item.m_Info.IsFood())
		{
			return false;
		}
		FoodInfo foodInfo = (FoodInfo)item.m_Info;
		switch (this.m_Type)
		{
		case FoodProcessor.Type.Smoker:
			return foodInfo.m_CanSmoke;
		case FoodProcessor.Type.Dryer:
			return foodInfo.m_CanDry;
		case FoodProcessor.Type.Fire:
			return foodInfo.m_CanCook;
		default:
			return false;
		}
	}

	public virtual void OnInsertItem(ItemSlot slot)
	{
		if (this.m_ActiveSlots.Contains(slot))
		{
			return;
		}
		if (!slot.m_Item || !slot.m_Item.m_Info.IsFood())
		{
			return;
		}
		Food food = (Food)slot.m_Item;
		if (!ItemsManager.Get().m_SetupAfterLoad)
		{
			food.m_ProcessDuration = 0f;
		}
		FoodInfo foodInfo = (FoodInfo)slot.m_Item.m_Info;
		if (this.GetResultItemID(foodInfo) == ItemID.None)
		{
			return;
		}
		this.m_ActiveSlots.Add(slot);
		if (foodInfo.m_CanCook || foodInfo.m_CanDry)
		{
			HUDProcess.Get().RegisterProcess(slot.m_Item, slot.m_Item.GetIconName(), this, true);
		}
	}

	public virtual void OnRemoveItem(ItemSlot slot)
	{
		if (this.m_ActiveSlots.Contains(slot))
		{
			this.m_ActiveSlots.Remove(slot);
		}
		if (this.m_ProcessedSlots.Contains(slot) && slot.m_Item)
		{
			if (slot.m_Item.m_Info.IsFood() && !((Food)slot.m_Item).m_Burned)
			{
				Skill.Get<CookingSkill>().OnSkillAction();
			}
			this.m_ProcessedSlots.Remove(slot);
		}
		if (slot.m_Item)
		{
			slot.m_Item.enabled = true;
			HUDProcess.Get().UnregisterProcess(slot.m_Item);
		}
	}

	public float GetProcessProgress(Trigger trigger)
	{
		Item y = (Item)trigger;
		foreach (ItemSlot itemSlot in this.m_ActiveSlots)
		{
			if (itemSlot.m_Item == y)
			{
				Food food = (Food)itemSlot.m_Item;
				FoodInfo info = (FoodInfo)itemSlot.m_Item.m_Info;
				return food.m_ProcessDuration / this.GetProcessingTime(info);
			}
		}
		return 0f;
	}

	public void OnDestroyConstruction()
	{
		while (this.m_ActiveSlots.Count > 0)
		{
			if (this.m_ActiveSlots[0].m_Item)
			{
				this.m_ActiveSlots[0].RemoveItem();
			}
			else
			{
				this.m_ActiveSlots.RemoveAt(0);
			}
		}
		while (this.m_ProcessedSlots.Count > 0)
		{
			if (this.m_ProcessedSlots[0].m_Item)
			{
				this.m_ProcessedSlots[0].RemoveItem();
			}
			else
			{
				this.m_ProcessedSlots.RemoveAt(0);
			}
		}
	}

	public FoodProcessor.Type m_Type;

	private List<ItemSlot> m_ActiveSlots = new List<ItemSlot>();

	private List<ItemSlot> m_ProcessedSlots = new List<ItemSlot>();

	[HideInInspector]
	public Firecamp m_ConnectedFirecamp;

	private Firecamp m_Firecamp;

	public bool m_DebugImmediate;

	public static List<FoodProcessor> s_AllFoodProcessors = new List<FoodProcessor>();

	public enum Type
	{
		None,
		Smoker,
		Dryer,
		Fire
	}
}
