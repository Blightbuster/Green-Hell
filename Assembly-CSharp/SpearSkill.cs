using System;

public class SpearSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_DamageMul, "DamageMul");
		base.RegisterCurve(this.m_StaminaMul, "StaminaConsumptionMul");
		base.RegisterCurve(this.m_ThrowForceMul, "ThrowForceMul");
		base.RegisterCurve(this.m_AimDuration, "AimDuration");
	}

	public float GetDamageMul()
	{
		return this.m_DamageMul.Evaluate(this.m_Value);
	}

	public float GetStaminaMul()
	{
		return this.m_StaminaMul.Evaluate(this.m_Value);
	}

	public float GetAimDuration()
	{
		return this.m_AimDuration.Evaluate(this.m_Value);
	}

	public float GetThrowForceMul()
	{
		return this.m_ThrowForceMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_DamageMul = new SkillCurve();

	private SkillCurve m_StaminaMul = new SkillCurve();

	private SkillCurve m_ThrowForceMul = new SkillCurve();

	private SkillCurve m_AimDuration = new SkillCurve();
}
