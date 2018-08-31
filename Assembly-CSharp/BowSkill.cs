using System;

public class BowSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_DamageMul, "DamageMul");
		base.RegisterCurve(this.m_StaminaMul, "StaminaConsumptionMul");
	}

	public float GetDamageMul()
	{
		return this.m_DamageMul.Evaluate(this.m_Value);
	}

	public float GetStaminaMul()
	{
		return this.m_StaminaMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_DamageMul = new SkillCurve();

	private SkillCurve m_StaminaMul = new SkillCurve();
}
