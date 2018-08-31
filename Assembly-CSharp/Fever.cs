using System;
using Enums;

public class Fever : Disease
{
	public Fever()
	{
		this.m_Type = ConsumeEffect.Fever;
	}

	protected override void CheckAutoHeal()
	{
		if (PlayerInjuryModule.Get().GetPosionLevel() > 0)
		{
			this.m_StartTime = MainLevel.Instance.GetCurrentTimeMinutes();
		}
		base.CheckAutoHeal();
	}
}
