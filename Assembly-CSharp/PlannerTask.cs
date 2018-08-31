using System;
using Enums;

public class PlannerTask
{
	public virtual void Parse(Key key)
	{
		this.m_LocalizedText = GreenHellGame.Instance.GetLocalization().Get(key.GetVariable(1).SValue);
		this.m_ShowInList = key.GetVariable(2).BValue;
		this.m_PositivePlanned = key.GetVariable(3).FValue;
		this.m_NegativePlanned = key.GetVariable(4).FValue;
		this.m_PositiveNotPlanned = key.GetVariable(5).FValue;
		this.m_NegativeNotPlanned = key.GetVariable(6).FValue;
	}

	public virtual bool OnDrink(bool planned)
	{
		return false;
	}

	public virtual bool OnEat(ItemID item, bool planned)
	{
		return false;
	}

	public virtual bool OnBuild(ItemID item, bool planned)
	{
		return false;
	}

	public virtual bool OnMakeFire(bool planned)
	{
		return false;
	}

	public virtual bool OnSleep(bool bed, bool planned)
	{
		return false;
	}

	public virtual bool OnHealWound(bool planned)
	{
		return false;
	}

	public float m_PositivePlanned;

	public float m_NegativePlanned;

	public float m_PositiveNotPlanned;

	public float m_NegativeNotPlanned;

	public string m_LocalizedText = string.Empty;

	public bool m_Fullfiled;

	public bool m_ShowInList = true;
}
