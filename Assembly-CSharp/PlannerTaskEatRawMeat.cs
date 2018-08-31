using System;
using CJTools;
using Enums;

public class PlannerTaskEatRawMeat : PlannerTask
{
	public override bool OnEat(ItemID item, bool planned)
	{
		return General.IsItemRawMeat(item);
	}
}
