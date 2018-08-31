using System;
using Enums;

public class WaterCollector : Construction, ITriggerThrough, IItemSlotParent
{
	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot.m_Item.m_Info.IsBowl())
		{
			BowlInfo bowlInfo = (BowlInfo)slot.m_Item.m_Info;
			if (bowlInfo.m_LiquidType != LiquidType.Water)
			{
				Bowl bowl = (Bowl)slot.m_Item;
				bowl.Spill(-1f);
				bowlInfo.m_LiquidType = LiquidType.Water;
			}
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}
}
