using System;

public class PlannerTaskSleep : PlannerTask
{
	public override bool OnSleep(bool bed, bool planned)
	{
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}
}
