using System;

public class CraftingSkill : Skill
{
	public override void Initialize(string name)
	{
		base.Initialize(name);
		base.RegisterCurve(this.m_ItemHealthMul, "ItemHealthMulCurve");
		base.RegisterCurve(this.m_PlayerHealthMul, "PlayerHealthMulCurve");
	}

	public override void Load(Key key)
	{
		if (key.GetName() == "InitialHealthMul")
		{
			this.m_InitialHealthMul = key.GetVariable(0).FValue;
			return;
		}
		base.Load(key);
	}

	public float GetItemHealthMul(Item item)
	{
		return this.m_ItemHealthMul.Evaluate((float)ItemsManager.Get().m_CreationsData[(int)item.GetInfoID()]);
	}

	public float GetPlayerHealthMul()
	{
		return this.m_PlayerHealthMul.Evaluate(this.m_Value);
	}

	private SkillCurve m_ItemHealthMul = new SkillCurve();

	private SkillCurve m_PlayerHealthMul = new SkillCurve();

	public float m_InitialHealthMul;
}
