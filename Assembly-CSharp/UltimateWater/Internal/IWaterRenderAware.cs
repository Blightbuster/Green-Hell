using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public interface IWaterRenderAware
	{
		void OnWaterRender(Camera camera);

		void OnWaterPostRender(Camera camera);

		void BuildShaderVariant(ShaderVariant variant, Water water, WaterQualityLevel qualityLevel);

		void UpdateMaterial(Water water, WaterQualityLevel qualityLevel);
	}
}
