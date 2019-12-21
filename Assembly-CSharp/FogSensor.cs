using System;
using UnityEngine;

public class FogSensor : SensorBase
{
	private void OnEnable()
	{
		if (FogSensor.s_DefaultDensityStart < 0f)
		{
			FogSensor.s_DefaultDensityStart = RenderSettings.fogStartDistance;
		}
		if (FogSensor.s_DefaultDensityEnd < 0f)
		{
			FogSensor.s_DefaultDensityEnd = RenderSettings.fogEndDistance;
		}
		FogSensor.s_FogDensityStart = FogSensor.s_DefaultDensityStart;
		FogSensor.s_FogDensityEnd = FogSensor.s_DefaultDensityEnd;
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		FogSensor.s_PlayerInFogSensor = true;
		FogSensor.s_FogDensityStart = this.m_DensityStart;
		FogSensor.s_FogDensityEnd = this.m_DensityEnd;
		FogSensor.s_NumEnters++;
	}

	protected override void OnExit()
	{
		base.OnExit();
		FogSensor.s_PlayerInFogSensor = false;
		FogSensor.s_FogDensityStart = FogSensor.s_DefaultDensityStart;
		FogSensor.s_FogDensityEnd = FogSensor.s_DefaultDensityEnd;
		FogSensor.s_NumEnters--;
	}

	public float m_DensityStart;

	public float m_DensityEnd;

	private static float s_DefaultDensityStart = -1f;

	private static float s_DefaultDensityEnd = -1f;

	public static bool s_PlayerInFogSensor = false;

	public static float s_FogDensityStart = 0f;

	public static float s_FogDensityEnd = 0f;

	public static int s_NumEnters = 0;
}
