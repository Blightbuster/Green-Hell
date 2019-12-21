using System;
using System.Collections.Generic;
using UnityEngine;

public class SensorCreepyAppearGroup : MonoBehaviour
{
	private void Awake()
	{
		this.m_Sensors = new List<SensorCreepyAppear>(base.GetComponentsInChildren<SensorCreepyAppear>());
		foreach (SensorCreepyAppear sensorCreepyAppear in this.m_Sensors)
		{
			sensorCreepyAppear.m_SensorCreepyAppearGroup = this;
		}
	}

	public void OnAppearObject()
	{
		this.m_LastAppearObjectTime = Time.time;
		bool flag = true;
		using (List<SensorCreepyAppear>.Enumerator enumerator = this.m_Sensors.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.enabled)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			base.enabled = false;
			this.m_Completed = true;
		}
	}

	public bool CanAppearObject()
	{
		return Time.time - this.m_LastAppearObjectTime >= this.m_AppearObjectInterval;
	}

	public bool IsCompleted()
	{
		return this.m_Completed;
	}

	private List<SensorCreepyAppear> m_Sensors;

	private float m_LastAppearObjectTime;

	public float m_AppearObjectInterval;

	private bool m_Completed;
}
