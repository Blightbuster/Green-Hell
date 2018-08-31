using System;
using UnityEngine;

public class ChangeSceneSensor : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		Player.Get().OnPlayerChangeScene(this.m_Spawner);
	}

	public GameObject m_Spawner;
}
