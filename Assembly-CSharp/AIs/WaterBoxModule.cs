using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class WaterBoxModule : AIModule
	{
		public void OnEnterWater(WaterCollider water)
		{
			if (!this.m_CurrentWaters.Contains(water))
			{
				this.m_CurrentWaters.Add(water);
			}
		}

		public void OnExitWater(WaterCollider water)
		{
			if (this.m_CurrentWaters.Contains(water))
			{
				this.m_CurrentWaters.Remove(water);
			}
		}

		public float GetWaterLevel()
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
			return num;
		}

		private List<WaterCollider> m_CurrentWaters = new List<WaterCollider>();

		private List<BoxCollider> m_Colliders = new List<BoxCollider>(10);
	}
}
