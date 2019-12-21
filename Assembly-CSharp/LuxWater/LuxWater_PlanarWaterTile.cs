using System;
using UnityEngine;

namespace LuxWater
{
	[ExecuteInEditMode]
	public class LuxWater_PlanarWaterTile : MonoBehaviour
	{
		public void OnEnable()
		{
			this.AcquireComponents();
		}

		private void AcquireComponents()
		{
			if (!this.reflection)
			{
				if (base.transform.parent)
				{
					this.reflection = base.transform.parent.GetComponent<LuxWater_PlanarReflection>();
					return;
				}
				this.reflection = base.transform.GetComponent<LuxWater_PlanarReflection>();
			}
		}

		public void OnWillRenderObject()
		{
			if (this.reflection)
			{
				this.reflection.WaterTileBeingRendered(base.transform, Camera.current);
			}
		}

		public LuxWater_PlanarReflection reflection;
	}
}
