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
	}

	private void OnDestroy()
	{
		FoodProcessor.s_AllFoodProcessors.Remove(this);
	}

	protected void Start()
	{
		this.m_ItemsManager = ItemsManager.Get();
		this.m_Firecamp = base.GetComponent<Firecamp>();
	}

	public void UpdateProcessing()
	{
		foreach (ItemSlot itemSlot in this.m_ActiveSlots)
		{
			Food food = (Food)itemSlot.m_Item;
			if (this.m_Firecamp)
			{
				if (!this.m_Firecamp.m_Burning)
				{
					if (food.m_ProcessDuration > 0f)
					{
						food.m_ProcessDuration -= MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
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
					}
					continue;
				}
			}
			else if (this.m_Type != FoodProcessor.Type.Dryer && (!this.m_ConnectedFirecamp || !this.m_ConnectedFirecamp.m_Burning))
			{
				if (food.m_ProcessDuration > 0f)
				{
					food.m_ProcessDuration -= MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
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
				}
				continue;
			}
			if (this.m_Type == FoodProcessor.Type.Dryer)
			{
				if (!RainManager.Get().IsRain())
				{
					food.m_ProcessDuration += MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
				}
				else if (food.m_ProcessDuration > 0f)
				{
					food.m_ProcessDuration -= MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
				}
				else
				{
					food.m_ProcessDuration = 0f;
				}
			}
			else
			{
				food.m_ProcessDuration += MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
			}
			FoodInfo foodInfo = (FoodInfo)itemSlot.m_Item.m_Info;
			if (itemSlot.m_Item.enabled)
			{
				if (this.m_Type != FoodProcessor.Type.Dryer)
				{
					itemSlot.m_Item.enabled = !foodInfo.m_CanCook;
				}
				else
				{
					itemSlot.m_Item.enabled = !foodInfo.m_CanDry;
				}
			}
			if (food.m_ProcessDuration >= this.GetProcessingTime(foodInfo) || this.m_DebugImmediate)
			{
				HUDProcess.Get().UnregisterProcess(itemSlot.m_Item);
				Item item = this.m_ItemsManager.CreateItem(this.GetResultItemID(foodInfo), true, itemSlot.m_Item.transform.position, itemSlot.m_Item.transform.rotation);
				itemSlot.ReplaceItem(item);
				if (!this.m_ProcessedSlots.Contains(itemSlot))
				{
					this.m_ProcessedSlots.Add(itemSlot);
				}
				this.m_DebugImmediate = false;
				if (this.m_Type == FoodProcessor.Type.Fire && !foodInfo.m_CanCook)
				{
					food.m_Burned = true;
				}
				break;
			}
		}
	}

	private ItemID GetResultItemID(FoodInfo info)
	{
		FoodProcessor.Type type = this.m_Type;
		if (type == FoodProcessor.Type.Dryer)
		{
			return info.m_DryingItemID;
		}
		if (type == FoodProcessor.Type.Smoker)
		{
			return info.m_SmokingItemID;
		}
		if (type != FoodProcessor.Type.Fire)
		{
			return ItemID.None;
		}
		return (!info.m_CanCook) ? ((!GreenHellGame.ROADSHOW_DEMO) ? info.m_BurningItemID : ItemID.None) : info.m_CookingItemID;
	}

	private float GetProcessingTime(FoodInfo info)
	{
		FoodProcessor.Type type = this.m_Type;
		if (type == FoodProcessor.Type.Dryer)
		{
			return info.m_DryingLength;
		}
		if (type == FoodProcessor.Type.Smoker)
		{
			return info.m_SmokingLength;
		}
		if (type != FoodProcessor.Type.Fire)
		{
			return 0f;
		}
		if (info.m_CanCook)
		{
			return info.m_CookingLength * Skill.Get<CookingSkill>().GetCookingDurationMul();
		}
		return info.m_BurningLength * Skill.Get<CookingSkill>().GetBurningDurationMul();
	}

	public virtual bool CanInsertItem(Item item)
	{
		if (!item.m_Info.IsFood())
		{
			return false;
		}
		FoodInfo foodInfo = (FoodInfo)item.m_Info;
		FoodProcessor.Type type = this.m_Type;
		if (type == FoodProcessor.Type.Dryer)
		{
			return foodInfo.m_CanDry;
		}
		if (type != FoodProcessor.Type.Smoker)
		{
			return type == FoodProcessor.Type.Fire && foodInfo.m_CanCook;
		}
		return foodInfo.m_CanSmoke;
	}

	public virtual void OnInsertItem(ItemSlot slot)
	{
		if (this.m_ActiveSlots.Contains(slot))
		{
			return;
		}
		if (!slot.m_Item.m_Info.IsFood())
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
			HUDProcess.Get().RegisterProcess(slot.m_Item, slot.m_Item.GetIconName(), this, false);
		}
	}

	public virtual void OnRemoveItem(ItemSlot slot)
	{
		if (this.m_ActiveSlots.Contains(slot))
		{
			this.m_ActiveSlots.Remove(slot);
		}
		if ((this.m_Type == FoodProcessor.Type.Fire || this.m_Type == FoodProcessor.Type.Smoker) && this.m_ProcessedSlots.Contains(slot) && slot.m_Item)
		{
			Food food = (Food)slot.m_Item;
			if (!food.m_Burned)
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

	public float GetProcessProgress(Item item)
	{
		foreach (ItemSlot itemSlot in this.m_ActiveSlots)
		{
			if (itemSlot.m_Item == item)
			{
				Food food = (Food)itemSlot.m_Item;
				FoodInfo info = (FoodInfo)itemSlot.m_Item.m_Info;
				return food.m_ProcessDuration / this.GetProcessingTime(info);
			}
		}
		return 0f;
	}

	public FoodProcessor.Type m_Type;

	private List<ItemSlot> m_ActiveSlots = new List<ItemSlot>();

	private List<ItemSlot> m_ProcessedSlots = new List<ItemSlot>();

	[HideInInspector]
	public Firecamp m_ConnectedFirecamp;

	private Firecamp m_Firecamp;

	private ItemsManager m_ItemsManager;

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
