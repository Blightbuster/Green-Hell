using System;
using UnityEngine;

public class SensorEnableDisableObject : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		if (this.m_Object != null)
		{
			this.m_Object.SetActive(true);
		}
	}

	protected override void OnExit()
	{
		base.OnExit();
		if (this.m_Object != null)
		{
			this.m_Object.SetActive(false);
		}
	}

	public GameObject m_Object;
}
