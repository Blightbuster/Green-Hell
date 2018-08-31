using System;

public class HarvestingAnimalsSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_AnimationsCount, "AnimationsCountCurve");
		base.RegisterCurve(this.m_ItemsCountMul, "ItemsCountMulCurve");
	}

	public int GetAnimationsCount()
	{
		return (int)this.m_AnimationsCount.Evaluate(this.m_Value);
	}

	public int GetItemsCountMul()
	{
		return (int)this.m_ItemsCountMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_AnimationsCount = new SkillCurve();

	private SkillCurve m_ItemsCountMul = new SkillCurve();
}
