using System;
using UltimateWater.Internal;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public interface ILocalDisplacementMaskRenderer : IDynamicWaterEffects
	{
		void RenderLocalMask(CommandBuffer commandBuffer, DynamicWaterCameraData overlays);
	}
}
