using System;
using UnityEngine;

public class EnvironmentLightingSensor : SensorBase
{
	private void OnEnable()
	{
		if (EnvironmentLightingSensor.s_DefaultIntenistyMul < 0f)
		{
			EnvironmentLightingSensor.s_DefaultIntenistyMul = RenderSettings.ambientIntensity;
		}
		EnvironmentLightingSensor.s_IntenistyMul = EnvironmentLightingSensor.s_DefaultIntenistyMul;
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		EnvironmentLightingSensor.s_PlayerInEnvironmentLightingSensor = true;
		EnvironmentLightingSensor.s_IntenistyMul = this.m_IntenistyMul;
		EnvironmentLightingSensor.s_NumEnters++;
	}

	protected override void OnExit()
	{
		base.OnExit();
		EnvironmentLightingSensor.s_PlayerInEnvironmentLightingSensor = false;
		EnvironmentLightingSensor.s_IntenistyMul = EnvironmentLightingSensor.s_DefaultIntenistyMul;
		EnvironmentLightingSensor.s_NumEnters--;
	}

	private void OnApplicationQuit()
	{
		EnvironmentLightingSensor.s_IntenistyMul = EnvironmentLightingSensor.s_DefaultIntenistyMul;
		MainLevel.Instance.m_TODSky.m_AmbientIntensity = EnvironmentLightingSensor.s_DefaultIntenistyMul;
	}

	public float m_IntenistyMul;

	public static float s_DefaultIntenistyMul = -1f;

	public static bool s_PlayerInEnvironmentLightingSensor = false;

	public static float s_IntenistyMul = 0f;

	public static int s_NumEnters = 0;
}
