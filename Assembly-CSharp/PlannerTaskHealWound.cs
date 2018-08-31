using System;

public class PlannerTaskHealWound : PlannerTask
{
	public override bool OnHealWound(bool planned)
	{
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}
}
