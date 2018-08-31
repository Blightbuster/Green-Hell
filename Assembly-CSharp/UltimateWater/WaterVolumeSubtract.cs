using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water Volume Subtract")]
	public sealed class WaterVolumeSubtract : WaterVolumeBase
	{
		protected override CullMode _CullMode
		{
			get
			{
				return CullMode.Front;
			}
		}

		protected override void Register(Water water)
		{
			if (water != null)
			{
				water.Volume.AddSubtractor(this);
			}
		}

		protected override void Unregister(Water water)
		{
			if (water != null)
			{
				water.Volume.RemoveSubtractor(this);
			}
		}
	}
}
