using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public sealed class WaterRipples : SceneSingleton<WaterRipples>
	{
		public static void Register(WaterSimulationArea area)
		{
			WaterRipples instance = SceneSingleton<WaterRipples>.Instance;
			if (instance == null || instance._Areas.Contains(area))
			{
				return;
			}
			instance._Areas.Add(area);
		}

		public static void Unregister(WaterSimulationArea area)
		{
			WaterRipples instance = SceneSingleton<WaterRipples>.Instance;
			if (instance == null)
			{
				return;
			}
			instance._Areas.Remove(area);
		}

		public static void AddForce(List<WaterForce.Data> data, float radius = 1f)
		{
			WaterRipples instance = SceneSingleton<WaterRipples>.Instance;
			if (instance == null || Time.timeScale == 0f)
			{
				return;
			}
			for (int i = instance._Areas.Count - 1; i >= 0; i--)
			{
				WaterSimulationArea waterSimulationArea = instance._Areas[i];
				waterSimulationArea.AddForce(data, radius);
			}
		}

		private void FixedUpdate()
		{
			int iterations = WaterQualitySettings.Instance.Ripples.Iterations;
			for (int i = 0; i < iterations; i++)
			{
				for (int j = 0; j < this._Areas.Count; j++)
				{
					this._Areas[j].Simulate();
				}
				for (int k = 0; k < this._Areas.Count; k++)
				{
					this._Areas[k].Smooth();
				}
				for (int l = 0; l < this._Areas.Count; l++)
				{
					this._Areas[l].Swap();
				}
			}
		}

		private void OnDisable()
		{
			this._Areas.Clear();
		}

		private readonly List<WaterSimulationArea> _Areas = new List<WaterSimulationArea>();
	}
}
