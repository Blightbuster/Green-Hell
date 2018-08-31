using System;

public class CookingSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_CookingDurationMul, "CookingDurationMulCurve");
		base.RegisterCurve(this.m_BurningDurationMul, "BurningDurationMulCurve");
	}

	public float GetCookingDurationMul()
	{
		return this.m_CookingDurationMul.Evaluate(this.m_Value);
	}

	public float GetBurningDurationMul()
	{
		return this.m_BurningDurationMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_CookingDurationMul = new SkillCurve();

	private SkillCurve m_BurningDurationMul = new SkillCurve();
}
