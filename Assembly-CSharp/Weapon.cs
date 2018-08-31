using System;
using Enums;

public class Weapon : ItemTool
{
	protected override void Awake()
	{
		base.Awake();
		this.m_TimeFromDestroyToDissapear = 5f;
	}

	public WeaponType GetWeaponType()
	{
		return ((WeaponInfo)this.m_Info).m_WeaponType;
	}
}
