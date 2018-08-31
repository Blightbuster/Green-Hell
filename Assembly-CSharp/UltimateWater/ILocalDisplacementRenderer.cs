using System;
using UltimateWater.Internal;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public interface ILocalDisplacementRenderer : IDynamicWaterEffects
	{
		void RenderLocalDisplacement(CommandBuffer commandBuffer, DynamicWaterCameraData overlays);
	}
}
