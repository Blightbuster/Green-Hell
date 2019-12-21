using System;

public class ArcherySkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_StaminaConsumptionMul, "StaminaConsumptionMul");
		base.RegisterCurve(this.m_ShotForceMul, "ShotForceMulCurve");
		base.RegisterCurve(this.m_AimDuration, "AimDuration");
		base.RegisterCurve(this.m_AimShakeMul, "AimShakeMul");
	}

	public float GetAimDuration()
	{
		return this.m_AimDuration.Evaluate(this.m_Value);
	}

	public float GetShotForceMul()
	{
		return this.m_ShotForceMul.Evaluate(this.m_Value);
	}

	public float GetStaminaConsumptionMul()
	{
		return this.m_StaminaConsumptionMul.Evaluate(this.m_Value);
	}

	public float GetAimShakeMul()
	{
		return this.m_AimShakeMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_ShotForceMul = new SkillCurve();

	private SkillCurve m_AimDuration = new SkillCurve();

	private SkillCurve m_StaminaConsumptionMul = new SkillCurve();

	private SkillCurve m_AimShakeMul = new SkillCurve();
}
