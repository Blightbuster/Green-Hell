using System;

public class SeedInfo : FoodInfo
{
	public override bool IsSeed()
	{
		return true;
	}

	public override bool IsConsumable()
	{
		return false;
	}
}
