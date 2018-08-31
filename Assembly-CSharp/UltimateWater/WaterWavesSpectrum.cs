using System;
using UnityEngine;

namespace UltimateWater
{
	public abstract class WaterWavesSpectrum
	{
		protected WaterWavesSpectrum(float tileSize, float gravity, float windSpeed, float amplitude)
		{
			this._TileSize = tileSize;
			this._Gravity = gravity;
			this._WindSpeed = windSpeed;
			this._Amplitude = amplitude;
		}

		public float TileSize
		{
			get
			{
				return this._TileSize * WaterQualitySettings.Instance.TileSizeScale;
			}
		}

		public float Gravity
		{
			get
			{
				return this._Gravity;
			}
		}

		public abstract void ComputeSpectrum(Vector3[] spectrum, float tileSizeMultiplier, int maxResolution, System.Random random);

		protected float _TileSize;

		protected float _Gravity;

		protected float _WindSpeed;

		protected float _Amplitude;
	}
}
