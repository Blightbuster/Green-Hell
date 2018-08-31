using System;
using System.Collections.Generic;
using Enums;

public class FoodInfo : ConsumableInfo
{
	public FoodInfo()
	{
		this.m_CanCook = false;
		this.m_CanDry = false;
		this.m_CanSmoke = false;
		this.m_CookingLength = 0f;
		this.m_BurningLength = 0f;
		this.m_DryingLength = 0f;
		this.m_SmokingLength = 0f;
		this.m_CookingItemID = ItemID.None;
		this.m_BurningItemID = ItemID.None;
		this.m_DryingItemID = ItemID.None;
		this.m_SmokingItemID = ItemID.None;
		this.m_SpoilEffectID = ItemID.None;
		this.m_SpoilTime = -1f;
		this.m_DisappearTime = 0f;
		this.m_EatingResultItems = new List<ItemID>();
		this.m_MeatType = MeatType.None;
		this.m_SpoilOnlyIfTriggered = false;
	}

	public bool m_CanCook { get; set; }

	public bool m_CanDry { get; set; }

	public bool m_CanSmoke { get; set; }

	public float m_CookingLength { get; set; }

	public float m_BurningLength { get; set; }

	public float m_DryingLength { get; set; }

	public float m_SmokingLength { get; set; }

	public ItemID m_CookingItemID { get; set; }

	public ItemID m_BurningItemID { get; set; }

	public ItemID m_DryingItemID { get; set; }

	public ItemID m_SmokingItemID { get; set; }

	public ItemID m_SpoilEffectID { get; set; }

	public float m_SpoilTime { get; set; }

	public float m_DisappearTime { get; set; }

	public List<ItemID> m_EatingResultItems { get; set; }

	public MeatType m_MeatType { get; set; }

	public bool m_SpoilOnlyIfTriggered { get; set; }

	public override bool IsFood()
	{
		return true;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "CanCookOnFire")
		{
			this.m_CanCook = (key.GetVariable(0).IValue == 1);
		}
		else if (key.GetName() == "CanDry")
		{
			this.m_CanDry = (key.GetVariable(0).IValue == 1);
		}
		else if (key.GetName() == "CanSmoke")
		{
			this.m_CanSmoke = (key.GetVariable(0).IValue == 1);
		}
		else if (key.GetName() == "CookingLength")
		{
			this.m_CookingLength = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "BurningLength")
		{
			this.m_BurningLength = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "DryingLength")
		{
			this.m_DryingLength = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "SmokingLength")
		{
			this.m_SmokingLength = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "CookingItemID")
		{
			this.m_CookingItemID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "BurningItemID")
		{
			this.m_BurningItemID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "DryingItemID")
		{
			this.m_DryingItemID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "SmokingItemID")
		{
			this.m_SmokingItemID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "SpoilTime")
		{
			this.m_SpoilTime = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "SpoilEffectID")
		{
			this.m_SpoilEffectID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "DisappearTime")
		{
			this.m_DisappearTime = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "EatingResult")
		{
			string svalue = key.GetVariable(0).SValue;
			string[] array = svalue.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				this.m_EatingResultItems.Add((ItemID)Enum.Parse(typeof(ItemID), array[i]));
			}
		}
		else if (key.GetName() == "MeatType")
		{
			this.m_MeatType = (MeatType)Enum.Parse(typeof(MeatType), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "SpoilOnlyIfTriggered")
		{
			this.m_SpoilOnlyIfTriggered = (key.GetVariable(0).IValue != 0);
		}
		else
		{
			base.LoadParams(key);
		}
	}

	public override void GetInfoText(ref string result)
	{
		base.GetInfoText(ref result);
		result = result + "Weight = " + base.m_Capacity.ToString() + "\n";
		result = result + "CookingLength = " + this.m_CookingLength.ToString() + "\n";
		result = result + "DryingLength = " + this.m_DryingLength.ToString() + "\n";
	}
}
