using System;

public class BlowgunSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_ShakeMul, "ShakeMulCurve");
		base.RegisterCurve(this.m_ShotForceMul, "ShotForceMulCurve");
		base.RegisterCurve(this.m_AimDuration, "AimDuration");
	}

	public float GetAimDuration()
	{
		return this.m_AimDuration.Evaluate(this.m_Value);
	}

	public float GetShakeMul()
	{
		return this.m_ShakeMul.Evaluate(this.m_Value);
	}

	public float GetShotForceMul()
	{
		return this.m_ShotForceMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_ShakeMul = new SkillCurve();

	private SkillCurve m_ShotForceMul = new SkillCurve();

	private SkillCurve m_AimDuration = new SkillCurve();
}
