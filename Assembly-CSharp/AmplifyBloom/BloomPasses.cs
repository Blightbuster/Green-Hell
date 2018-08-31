using System;

namespace AmplifyBloom
{
	public enum BloomPasses
	{
		Threshold,
		ThresholdMask,
		AnamorphicGlare,
		LensFlare0,
		LensFlare1,
		LensFlare2,
		LensFlare3,
		LensFlare4,
		LensFlare5,
		DownsampleNoWeightedAvg,
		DownsampleWithKaris,
		DownsampleWithoutKaris,
		DownsampleWithTempFilterWithKaris,
		DownsampleWithTempFilterWithoutKaris,
		HorizontalBlur,
		VerticalBlur,
		VerticalBlurWithTempFilter,
		UpscaleFirstPass,
		Upscale,
		WeightedAddPS1,
		WeightedAddPS2,
		WeightedAddPS3,
		WeightedAddPS4,
		WeightedAddPS5,
		WeightedAddPS6,
		WeightedAddPS7,
		WeightedAddPS8,
		BokehWeightedBlur,
		BokehComposition2S,
		BokehComposition3S,
		BokehComposition4S,
		BokehComposition5S,
		BokehComposition6S,
		Decode,
		TotalPasses
	}
}
