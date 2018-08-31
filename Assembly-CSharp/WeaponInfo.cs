using System;
using Enums;

public class WeaponInfo : ItemToolInfo
{
	public WeaponInfo()
	{
		this.m_DefaultDamage = 0f;
		this.m_HumanDamage = 0f;
		this.m_AnimalDamage = 0f;
		this.m_PlantDamage = 0f;
		this.m_TreeDamage = 0f;
		this.m_PlayerDamage = 0f;
		this.m_WeaponType = WeaponType.None;
	}

	public float m_DefaultDamage { get; set; }

	public float m_HumanDamage { get; set; }

	public float m_AnimalDamage { get; set; }

	public float m_PlantDamage { get; set; }

	public float m_TreeDamage { get; set; }

	public float m_PlayerDamage { get; set; }

	public WeaponType m_WeaponType { get; set; }

	public override bool IsWeapon()
	{
		return true;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "DefaultDamage")
		{
			this.m_DefaultDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "HumanDamage")
		{
			this.m_HumanDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "AnimalDamage")
		{
			this.m_AnimalDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "PlantDamage")
		{
			this.m_PlantDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "TreeDamage")
		{
			this.m_TreeDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "PlayerDamage")
		{
			this.m_PlayerDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "WeaponType")
		{
			this.m_WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), key.GetVariable(0).SValue);
		}
		else
		{
			base.LoadParams(key);
		}
	}

	public override void GetInfoText(ref string result)
	{
		base.GetInfoText(ref result);
	}
}
