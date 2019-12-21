using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SensorManager : MonoBehaviour, ISaveLoad
{
	public static SensorManager Get()
	{
		return SensorManager.s_Instance;
	}

	private SensorManager()
	{
		SensorManager.s_Instance = this;
	}

	public void RegisterSensor(SensorBase sensor)
	{
		int key = Animator.StringToHash(sensor.name);
		if (!this.m_Sensors.ContainsKey(key))
		{
			this.m_Sensors.Add(key, sensor);
			return;
		}
		if (this.m_Sensors.ContainsValue(sensor))
		{
			DebugUtils.Assert("[SensorManager:RegisterSensor] ERROR - More than one sensor with name " + sensor.name, true, DebugUtils.AssertType.Info);
		}
	}

	public void UnregisterSensor(SensorBase sensor)
	{
		int key = Animator.StringToHash(sensor.name);
		this.m_Sensors.Remove(key);
	}

	public bool PlayerIsInSensor(string sensor_name)
	{
		int key = Animator.StringToHash(sensor_name);
		SensorBase sensorBase = null;
		this.m_Sensors.TryGetValue(key, out sensorBase);
		return sensorBase && sensorBase.IsInside();
	}

	public void Save()
	{
		for (int i = 0; i < this.m_Sensors.Count; i++)
		{
			this.m_Sensors.Values.ElementAt(i).Save();
		}
	}

	public void Load()
	{
		for (int i = 0; i < this.m_Sensors.Count; i++)
		{
			this.m_Sensors.Values.ElementAt(i).Load();
		}
	}

	private Dictionary<int, SensorBase> m_Sensors = new Dictionary<int, SensorBase>();

	private static SensorManager s_Instance;
}
