using System;
using UltimateWater.Internal;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public interface ILocalFoamRenderer : IDynamicWaterEffects
	{
		void RenderLocalFoam(CommandBuffer commandBuffer, DynamicWaterCameraData overlays);
	}
}
