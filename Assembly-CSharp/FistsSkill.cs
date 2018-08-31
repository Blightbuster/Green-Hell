using System;

public class FistsSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_DamageMul, "DamageMulCurve");
	}

	public float GetDamageMul()
	{
		return this.m_DamageMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_DamageMul = new SkillCurve();
}
