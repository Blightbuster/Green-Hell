using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public struct WaterQualityLevel
	{
		public void ResetToDefaults()
		{
			this.Name = string.Empty;
			this.MaxSpectrumResolution = 256;
			this.AllowHighPrecisionTextures = true;
			this.TileSizeScale = 1f;
			this.WavesMode = WaterWavesMode.AllowAll;
			this.AllowSpray = true;
			this.FoamQuality = 1f;
			this.MaxTesselationFactor = 1f;
			this.MaxVertexCount = 500000;
			this.MaxTesselatedVertexCount = 120000;
			this.AllowAlphaBlending = true;
		}

		[FormerlySerializedAs("name")]
		public string Name;

		[FormerlySerializedAs("maxSpectrumResolution")]
		[Tooltip("All simulations will be performed at most with this resolution")]
		public int MaxSpectrumResolution;

		[FormerlySerializedAs("allowHighPrecisionTextures")]
		public bool AllowHighPrecisionTextures;

		[FormerlySerializedAs("allowHighQualityNormalMaps")]
		public bool AllowHighQualityNormalMaps;

		[Range(0f, 1f)]
		[FormerlySerializedAs("tileSizeScale")]
		public float TileSizeScale;

		[FormerlySerializedAs("wavesMode")]
		public WaterWavesMode WavesMode;

		[FormerlySerializedAs("allowSpray")]
		public bool AllowSpray;

		[Range(0f, 1f)]
		[FormerlySerializedAs("foamQuality")]
		public float FoamQuality;

		[FormerlySerializedAs("maxTesselationFactor")]
		[Range(0f, 1f)]
		public float MaxTesselationFactor;

		[FormerlySerializedAs("maxVertexCount")]
		public int MaxVertexCount;

		[FormerlySerializedAs("maxTesselatedVertexCount")]
		public int MaxTesselatedVertexCount;

		[FormerlySerializedAs("allowAlphaBlending")]
		public bool AllowAlphaBlending;
	}
}
