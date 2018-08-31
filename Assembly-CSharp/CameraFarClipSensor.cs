using System;
using UnityEngine;

public class CameraFarClipSensor : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		Camera main = Camera.main;
		this.m_FarClipToRestore = main.farClipPlane;
		main.farClipPlane = this.m_FarClip;
	}

	protected override void OnExit()
	{
		base.OnExit();
		Camera main = Camera.main;
		main.farClipPlane = this.m_FarClipToRestore;
	}

	public float m_FarClip = 300f;

	private float m_FarClipToRestore = 300f;
}
