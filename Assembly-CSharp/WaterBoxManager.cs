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
		if (!this.m_CurrentWaters.Contains(water))
		{
			PlayerInjuryModule.Get().CheckLeeches();
			this.m_CurrentWaters.Add(water);
			if (water.m_PlayerCanSwimIn)
			{
				this.m_CurrentSwimWaters.Add(water);
			}
		}
	}

	public void OnExitWater(WaterCollider water)
	{
		if (this.m_CurrentWaters.Contains(water))
		{
			this.m_CurrentWaters.Remove(water);
		}
		if (this.m_CurrentSwimWaters.Contains(water))
		{
			this.m_CurrentSwimWaters.Remove(water);
		}
	}

	public bool IsInWater(Vector3 pos)
	{
		float num = 0f;
		return this.IsInWater(pos, ref num);
	}

	public bool IsInWater(Vector3 pos, ref float water_level)
	{
		float num = float.MinValue;
		for (int i = 0; i < this.m_CurrentWaters.Count; i++)
		{
			this.m_CurrentWaters[i].gameObject.GetComponents<BoxCollider>(this.m_Colliders);
			BoxCollider boxCollider = null;
			if (this.m_Colliders.Count > 0)
			{
				boxCollider = this.m_Colliders[0];
			}
			else
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
			float y = boxCollider.bounds.max.y;
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

	public bool IsInSwimWater(Vector3 pos, ref float water_level)
	{
		float num = float.MinValue;
		for (int i = 0; i < this.m_CurrentSwimWaters.Count; i++)
		{
			this.m_CurrentSwimWaters[i].gameObject.GetComponents<BoxCollider>(this.m_Colliders);
			BoxCollider boxCollider = null;
			if (this.m_Colliders.Count > 0)
			{
				boxCollider = this.m_Colliders[0];
			}
			else
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
			float y = boxCollider.bounds.max.y;
			if (y > num)
			{
				num = y;
			}
		}
		if (this.m_CurrentSwimWaters.Count > 0)
		{
			water_level = num;
			return water_level >= pos.y;
		}
		return false;
	}

	public WaterCollider GetWaterPlayerInside()
	{
		if (this.m_CurrentWaters.Count > 0)
		{
			return this.m_CurrentWaters[0];
		}
		return null;
	}

	private static WaterBoxManager s_Instance;

	private List<WaterCollider> m_CurrentWaters = new List<WaterCollider>();

	private List<WaterCollider> m_CurrentSwimWaters = new List<WaterCollider>();

	private List<BoxCollider> m_Colliders = new List<BoxCollider>(10);
}
