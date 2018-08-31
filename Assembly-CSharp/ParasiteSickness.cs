using System;
using Enums;

public class ParasiteSickness : Disease
{
	public ParasiteSickness()
	{
		this.m_Type = ConsumeEffect.ParasiteSickness;
	}

	public override void Load(Key key)
	{
		if (key.GetName() == "MacroNutricientFatLossMul")
		{
			this.m_MacroNutricientFatLossMul = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "MacroNutricientCarboLossMul")
		{
			this.m_MacroNutricientCarboLossMul = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "MacroNutricientProteinsLossMul")
		{
			this.m_MacroNutricientProteinsLossMul = key.GetVariable(0).FValue;
		}
		else
		{
			base.Load(key);
		}
	}

	public float m_MacroNutricientFatLossMul = 1f;

	public float m_MacroNutricientCarboLossMul = 1f;

	public float m_MacroNutricientProteinsLossMul = 1f;
}
