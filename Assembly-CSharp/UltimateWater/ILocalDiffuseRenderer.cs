using System;
using UltimateWater.Internal;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public interface ILocalDiffuseRenderer : IDynamicWaterEffects
	{
		void RenderLocalDiffuse(CommandBuffer commandBuffer, DynamicWaterCameraData overlays);
	}
}
