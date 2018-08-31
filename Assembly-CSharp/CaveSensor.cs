using System;
using System.Collections.Generic;

public class CaveSensor : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		CaveSensor.s_NumSensorsInside++;
		this.UpdateObservables();
	}

	protected override void OnExit()
	{
		base.OnExit();
		CaveSensor.s_NumSensorsInside--;
		this.UpdateObservables();
	}

	private void UpdateObservables()
	{
		for (int i = 0; i < CaveSensor.s_ICaveSensorObservables.Count; i++)
		{
			CaveSensor.s_ICaveSensorObservables[i].OnUpdateObservable();
		}
	}

	public static int s_NumSensorsInside = 0;

	public static List<ICaveSensorObservable> s_ICaveSensorObservables = new List<ICaveSensorObservable>();
}
