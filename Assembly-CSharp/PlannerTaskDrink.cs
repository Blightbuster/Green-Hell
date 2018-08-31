using System;

public class PlannerTaskDrink : PlannerTask
{
	public override bool OnDrink(bool planned)
	{
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}
}
