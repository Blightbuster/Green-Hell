using System;
using UltimateWater.Internal;

namespace UltimateWater
{
	public interface IOverlaysRenderer
	{
		void RenderOverlays(DynamicWaterCameraData overlays);

		void RenderFoam(DynamicWaterCameraData overlays);
	}
}
