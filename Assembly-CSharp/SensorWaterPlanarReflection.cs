using System;
using UltimateWater;
using UnityEngine;

public class SensorWaterPlanarReflection : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		if (Camera.main != null && Camera.main.GetComponent<WaterCamera>() != null)
		{
			Camera.main.GetComponent<WaterCamera>().enabled = true;
		}
	}

	protected override void OnExit()
	{
		base.OnExit();
		if (Camera.main != null && Camera.main.GetComponent<WaterCamera>() != null)
		{
			Camera.main.GetComponent<WaterCamera>().enabled = false;
		}
	}
}
