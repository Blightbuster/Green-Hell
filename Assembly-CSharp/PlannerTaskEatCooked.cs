using System;
using CJTools;
using Enums;

public class PlannerTaskEatCooked : PlannerTask
{
	public override bool OnEat(ItemID item, bool planned)
	{
		if (!General.IsItemCooked(item))
		{
			return false;
		}
		if (planned)
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.PlannedAction, 1);
			return true;
		}
		return false;
	}
}
