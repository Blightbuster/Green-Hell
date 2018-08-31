using System;

public class Spear : Weapon
{
	public float GetThrowForceMul()
	{
		return Skill.Get<SpearSkill>().GetThrowForceMul();
	}
}
