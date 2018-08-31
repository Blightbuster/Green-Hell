using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SensorManager : MonoBehaviour, ISaveLoad
{
	private SensorManager()
	{
		SensorManager.s_Instance = this;
	}

	public static SensorManager Get()
	{
		return SensorManager.s_Instance;
	}

	public void RegisterSensor(SensorBase sensor)
	{
		if (!this.m_Sensors.ContainsKey(sensor.name))
		{
			this.m_Sensors.Add(sensor.name, sensor);
		}
		else if (this.m_Sensors.ContainsValue(sensor))
		{
			DebugUtils.Assert("[SensorManager:RegisterSensor] ERROR - More than one sensor with name " + sensor.name, true, DebugUtils.AssertType.Info);
		}
	}

	public void UnregisterSensor(SensorBase sensor)
	{
		this.m_Sensors.Remove(sensor.name);
	}

	public bool PlayerIsInSensor(string sensor_name)
	{
		SensorBase sensorBase = null;
		this.m_Sensors.TryGetValue(sensor_name, out sensorBase);
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

	private Dictionary<string, SensorBase> m_Sensors = new Dictionary<string, SensorBase>();

	private static SensorManager s_Instance;
}
