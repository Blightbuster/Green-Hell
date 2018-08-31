using System;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public interface IWavesInteractive : IDynamicWaterEffects
	{
		void Render(CommandBuffer commandBuffer);
	}
}
