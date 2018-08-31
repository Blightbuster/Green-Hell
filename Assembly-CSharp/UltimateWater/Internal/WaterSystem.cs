using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public sealed class WaterSystem : ApplicationSingleton<WaterSystem>
	{
		public List<Water> Waters
		{
			get
			{
				return this._Waters;
			}
		}

		public List<Water> BoundlessWaters
		{
			get
			{
				return this._BoundlessWaters;
			}
		}

		public event Action OnQuit;

		public static bool IsWaterPossiblyVisible()
		{
			WaterSystem instance = ApplicationSingleton<WaterSystem>.Instance;
			return !(instance == null) && instance.Waters.Count != 0;
		}

		public static void Register(Water water)
		{
			WaterSystem.RequestId(water);
			WaterSystem instance = ApplicationSingleton<WaterSystem>.Instance;
			if (instance == null)
			{
				return;
			}
			if (!instance._Waters.Contains(water))
			{
				instance._Waters.Add(water);
			}
			if ((water.Volume == null || water.Volume.Boundless) && !instance._BoundlessWaters.Contains(water))
			{
				instance._BoundlessWaters.Add(water);
			}
		}

		public static void Unregister(Water water)
		{
			WaterSystem.FreeId(water);
			WaterSystem instance = ApplicationSingleton<WaterSystem>.Instance;
			if (instance == null)
			{
				return;
			}
			instance._Waters.Remove(water);
			instance._BoundlessWaters.Remove(water);
		}

		public static Water FindWater(Vector3 position, float radius)
		{
			bool flag;
			bool flag2;
			return WaterSystem.FindWater(position, radius, null, out flag, out flag2);
		}

		public static Water FindWater(Vector3 position, float radius, out bool isInsideSubtractiveVolume, out bool isInsideAdditiveVolume)
		{
			return WaterSystem.FindWater(position, radius, null, out isInsideSubtractiveVolume, out isInsideAdditiveVolume);
		}

		public static Water FindWater(Vector3 position, float radius, List<Water> allowedWaters, out bool isInsideSubtractiveVolume, out bool isInsideAdditiveVolume)
		{
			isInsideSubtractiveVolume = false;
			isInsideAdditiveVolume = false;
			int num = Physics.OverlapSphereNonAlloc(position, radius, WaterSystem._CollidersBuffer, 1 << WaterProjectSettings.Instance.WaterCollidersLayer, QueryTriggerInteraction.Collide);
			WaterSystem._PossibleWaters.Clear();
			WaterSystem._ExcludedWaters.Clear();
			for (int i = 0; i < num; i++)
			{
				WaterVolumeBase waterVolume = WaterVolumeBase.GetWaterVolume(WaterSystem._CollidersBuffer[i]);
				if (waterVolume != null)
				{
					if (waterVolume is WaterVolumeAdd)
					{
						isInsideAdditiveVolume = true;
						if (allowedWaters == null || allowedWaters.Contains(waterVolume.Water))
						{
							WaterSystem._PossibleWaters.Add(waterVolume.Water);
						}
					}
					else
					{
						isInsideSubtractiveVolume = true;
						WaterSystem._ExcludedWaters.Add(waterVolume.Water);
					}
				}
			}
			for (int j = 0; j < WaterSystem._PossibleWaters.Count; j++)
			{
				if (!WaterSystem._ExcludedWaters.Contains(WaterSystem._PossibleWaters[j]))
				{
					return WaterSystem._PossibleWaters[j];
				}
			}
			List<Water> boundlessWaters = ApplicationSingleton<WaterSystem>.Instance.BoundlessWaters;
			int count = boundlessWaters.Count;
			for (int k = 0; k < count; k++)
			{
				Water water = boundlessWaters[k];
				if ((allowedWaters == null || allowedWaters.Contains(water)) && water.Volume.IsPointInsideMainVolume(position, radius) && !WaterSystem._ExcludedWaters.Contains(water))
				{
					return water;
				}
			}
			return null;
		}

		protected override void OnDestroy()
		{
			if (this.OnQuit != null)
			{
				this.OnQuit();
			}
			base.OnDestroy();
		}

		private static bool RequestId(Water water)
		{
			if (water._WaterId != -1)
			{
				return false;
			}
			if (WaterSystem._FreeIds.Count == 0)
			{
				water._WaterId = WaterSystem._CurrentId++;
			}
			else
			{
				int index = WaterSystem._FreeIds.Count - 1;
				water._WaterId = WaterSystem._FreeIds[index];
				WaterSystem._FreeIds.RemoveAt(index);
			}
			return true;
		}

		private static void FreeId(Water water)
		{
			if (water._WaterId == -1)
			{
				return;
			}
			WaterSystem._FreeIds.Add(water._WaterId);
			water._WaterId = -1;
		}

		private readonly List<Water> _Waters = new List<Water>();

		private readonly List<Water> _BoundlessWaters = new List<Water>();

		private static readonly List<Water> _PossibleWaters = new List<Water>();

		private static readonly List<Water> _ExcludedWaters = new List<Water>();

		private static readonly Collider[] _CollidersBuffer = new Collider[30];

		private static int _CurrentId = 1;

		private static readonly List<int> _FreeIds = new List<int>();
	}
}
