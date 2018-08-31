using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterBoxManager : MonoBehaviour
{
	public static WaterBoxManager Get()
	{
		return WaterBoxManager.s_Instance;
	}

	private void Awake()
	{
		WaterBoxManager.s_Instance = this;
	}

	public void OnEnterWater(WaterCollider water)
	{
		if (!this.m_CurrentWaters.Contains(water.gameObject))
		{
			PlayerInjuryModule.Get().CheckLeeches();
			if (water.m_PlayerCanSwimIn)
			{
				this.m_CurrentWaters.Add(water.gameObject);
			}
		}
	}

	public void OnExitWater(WaterCollider water)
	{
		if (this.m_CurrentWaters.Contains(water.gameObject))
		{
			this.m_CurrentWaters.Remove(water.gameObject);
		}
	}

	public bool IsInWater(Vector3 pos, ref float water_level)
	{
		float num = float.MinValue;
		for (int i = 0; i < this.m_CurrentWaters.Count; i++)
		{
			GameObject gameObject = this.m_CurrentWaters[i];
			float y = (gameObject.transform.position + gameObject.transform.localScale * 0.5f).y;
			if (y > num)
			{
				num = y;
			}
		}
		if (this.m_CurrentWaters.Count > 0)
		{
			water_level = num;
			return true;
		}
		return false;
	}

	public WaterCollider GetWaterPlayerInside()
	{
		if (this.m_CurrentWaters.Count > 0)
		{
			return this.m_CurrentWaters[0].GetComponent<WaterCollider>();
		}
		return null;
	}

	private static WaterBoxManager s_Instance;

	private List<GameObject> m_CurrentWaters = new List<GameObject>();
}
