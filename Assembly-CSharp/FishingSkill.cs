using System;

public class FishingSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_BiteDurationMul, "BiteDurationMulCurve");
	}

	public float GetBiteDurationMul()
	{
		return this.m_BiteDurationMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_BiteDurationMul = new SkillCurve();
}
