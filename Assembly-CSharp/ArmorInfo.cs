using System;

public class ArmorInfo : ItemInfo
{
	public float m_Absorption { get; set; }

	public ArmorType m_ArmorType { get; set; }

	public ArmorInfo()
	{
		this.m_Absorption = 0f;
		this.m_ArmorType = ArmorType.None;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "DamageAbsorption")
		{
			this.m_Absorption = key.GetVariable(0).FValue;
			return;
		}
		if (key.GetName() == "ArmorType")
		{
			this.m_ArmorType = (ArmorType)Enum.Parse(typeof(ArmorType), key.GetVariable(0).SValue);
			return;
		}
		base.LoadParams(key);
	}

	public override bool IsArmor()
	{
		return true;
	}
}
