using System;
using Enums;

public class FoodPoisoning : Disease
{
	public FoodPoisoning()
	{
		this.m_Type = ConsumeEffect.FoodPoisoning;
	}
}
