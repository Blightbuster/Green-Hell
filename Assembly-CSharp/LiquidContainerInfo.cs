using System;
using Enums;

public class LiquidContainerInfo : ItemInfo
{
	public LiquidContainerInfo()
	{
		this.m_LiquidType = LiquidType.Water;
		this.m_Capacity = 100f;
		this.m_Amount = 0f;
	}

	public LiquidType m_LiquidType { get; set; }

	public float m_Capacity { get; set; }

	public float m_Amount { get; set; }

	public override bool IsLiquidContainer()
	{
		return true;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "LiquidType")
		{
			this.m_LiquidType = (LiquidType)Enum.Parse(typeof(LiquidType), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "LiquidAmount")
		{
			this.m_Amount = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Capacity")
		{
			this.m_Capacity = key.GetVariable(0).FValue;
		}
		else
		{
			base.LoadParams(key);
		}
	}

	public override void GetInfoText(ref string result)
	{
		base.GetInfoText(ref result);
		result = result + "LiquidType = " + this.m_LiquidType.ToString() + "\n";
		result = result + "Capacity = " + this.m_Capacity.ToString() + "\n";
		result = result + "Amount = " + this.m_Amount.ToString() + "\n";
	}

	public void OnCooked()
	{
		LiquidData liquidData = LiquidManager.Get().GetLiquidData(this.m_LiquidType);
		if (liquidData.m_CookingResult != LiquidType.None)
		{
			this.m_LiquidType = liquidData.m_CookingResult;
		}
	}

	public override bool CanDrink()
	{
		return this.m_Amount >= 1f;
	}
}
