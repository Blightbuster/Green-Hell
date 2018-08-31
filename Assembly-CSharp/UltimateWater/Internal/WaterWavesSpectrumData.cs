using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public sealed class WaterWavesSpectrumData : WaterWavesSpectrumDataBase
	{
		public WaterWavesSpectrumData(Water water, WindWaves windWaves, WaterWavesSpectrum spectrum) : base(water, windWaves, spectrum.TileSize, spectrum.Gravity)
		{
			this._Spectrum = spectrum;
		}

		protected override void GenerateContents(Vector3[][] spectrumValues)
		{
			int finalResolution = this._WindWaves.FinalResolution;
			Vector4 tileSizeScales = this._WindWaves.TileSizeScales;
			int num = (this._Water.Seed == 0) ? UnityEngine.Random.Range(0, 1000000) : this._Water.Seed;
			WaterQualityLevel[] qualityLevelsDirect = WaterQualitySettings.Instance.GetQualityLevelsDirect();
			int maxSpectrumResolution = qualityLevelsDirect[qualityLevelsDirect.Length - 1].MaxSpectrumResolution;
			if (finalResolution > maxSpectrumResolution)
			{
				Debug.LogWarningFormat("In water quality settings spectrum resolution of {0} is max, but at runtime a spectrum with resolution of {1} is generated. That may generate some unexpected behaviour. Make sure that the last water quality level has the highest resolution and don't alter it at runtime.", new object[]
				{
					maxSpectrumResolution,
					finalResolution
				});
			}
			for (byte b = 0; b < 4; b += 1)
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				UnityEngine.Random.InitState(num + (int)b);
				this._Spectrum.ComputeSpectrum(spectrumValues[(int)b], tileSizeScales[(int)b], maxSpectrumResolution, null);
				UnityEngine.Random.state = state;
			}
		}

		private readonly WaterWavesSpectrum _Spectrum;
	}
}
