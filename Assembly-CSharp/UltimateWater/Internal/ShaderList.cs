using System;

namespace UltimateWater.Internal
{
	public enum ShaderList
	{
		DepthCopy,
		WaterDepth,
		VolumesFront,
		VolumesBack,
		VolumesFrontSimple,
		Depth,
		Velocity,
		Simulation,
		Translate,
		ScreenSpaceMask,
		BaseIME,
		ComposeUnderWaterMask,
		WaterdropsMask,
		WaterdropsNormal,
		RaindropsFade,
		RaindropsFinal,
		RaindropsParticle,
		CollectLight,
		Transmission,
		GBuffer0Mix,
		GBuffer123Mix,
		FinalColorMix,
		DeferredReflections,
		DeferredShading,
		ShorelineMask,
		ShorelineMaskSimple,
		Noise,
		ShadowEnforcer,
		MergeDisplacements
	}
}
