using System;

public class PlannerTaskMakeFire : PlannerTask
{
	public override bool OnMakeFire(bool planned)
	{
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}
}
