using System;

public class MakeFireSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_ToolDurabilityConsumptionMul, "ToolDurabilityConsumptionMulCurve");
		base.RegisterCurve(this.m_StaminaConsumptionMul, "StaminaConsumptionMulCurve");
	}

	public float GetToolDurabilityConsumptionMul()
	{
		return this.m_ToolDurabilityConsumptionMul.Evaluate(this.m_Value);
	}

	public float GetStaminaConsumptionMul()
	{
		return this.m_StaminaConsumptionMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_ToolDurabilityConsumptionMul = new SkillCurve();

	private SkillCurve m_StaminaConsumptionMul = new SkillCurve();
}
