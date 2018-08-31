using System;
using UnityEngine;

public class SensorEnableDisable : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		if (this.m_Object != null)
		{
			this.m_Object.SetActive(this.m_Action == SensorEnableDisable.Action.Enable);
		}
	}

	public GameObject m_Object;

	public SensorEnableDisable.Action m_Action;

	public enum Action
	{
		Enable,
		Disable
	}
}
