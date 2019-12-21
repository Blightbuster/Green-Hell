using System;
using Enums;

public class LiquidContainerInfo : ItemInfo
{
	public LiquidType m_LiquidType { get; set; }

	public float m_Capacity { get; set; }

	public float m_Amount { get; set; }

	public override bool IsLiquidContainer()
	{
		return true;
	}

	public LiquidContainerInfo()
	{
		this.m_LiquidType = LiquidType.Water;
		this.m_Capacity = 100f;
		this.m_Amount = 0f;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "LiquidType")
		{
			this.m_LiquidType = (LiquidType)Enum.Parse(typeof(LiquidType), key.GetVariable(0).SValue);
			return;
		}
		if (key.GetName() == "LiquidAmount")
		{
			this.m_Amount = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "Capacity")
		{
			this.m_Capacity = key.GetVariable(0).FValue;
			return;
		}
		base.LoadParams(key);
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
		return !this.m_ForceNoDrink && this.m_Amount >= 1f;
	}

	public virtual bool CanSpill()
	{
		return !this.m_ForceNoSpill && this.m_Amount >= 1f;
	}

	public override float GetMass()
	{
		return base.m_Mass + this.m_Amount * 0.01f;
	}

	public bool m_ForceNoDrink;

	public bool m_ForceNoSpill;
}
