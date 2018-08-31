using System;
using UnityEngine;

namespace UltimateWater
{
	public class PhillipsSpectrum : WaterWavesSpectrum
	{
		public PhillipsSpectrum(float tileSize, float gravity, float windSpeed, float amplitude, float cutoffFactor) : base(tileSize, gravity, windSpeed, amplitude)
		{
			this._CutoffFactor = cutoffFactor;
		}

		public override void ComputeSpectrum(Vector3[] spectrum, float tileSizeMultiplier, int maxResolution, System.Random random)
		{
			float num = base.TileSize * tileSizeMultiplier;
			float num2 = this._Amplitude * PhillipsSpectrum.ComputeWaveAmplitude(this._WindSpeed);
			float num3 = 1f / num;
			int num4 = Mathf.RoundToInt(Mathf.Sqrt((float)spectrum.Length));
			int num5 = num4 / 2;
			float windSpeed = this._WindSpeed;
			float num6 = windSpeed * windSpeed / this._Gravity;
			float num7 = num6 * num6;
			float num8 = FastMath.Pow2(num6 / this._CutoffFactor);
			float num9 = Mathf.Sqrt(num2 * Mathf.Pow(100f / num, 2.35f) / 2000000f);
			for (int i = 0; i < num4; i++)
			{
				float num10 = 6.28318548f * (float)(i - num5) * num3;
				for (int j = 0; j < num4; j++)
				{
					float num11 = 6.28318548f * (float)(j - num5) * num3;
					float num12 = Mathf.Sqrt(num10 * num10 + num11 * num11);
					float num13 = num12 * num12;
					float num14 = num13 * num13;
					float num15 = Mathf.Exp(-1f / (num13 * num7) - num13 * num8) / num14;
					num15 = num9 * Mathf.Sqrt(num15);
					float x = FastMath.Gauss01() * num15;
					float y = FastMath.Gauss01() * num15;
					int num16 = (i + num5) % num4;
					int num17 = (j + num5) % num4;
					if (i == num5 && j == num5)
					{
						x = 0f;
						y = 0f;
					}
					spectrum[num16 * num4 + num17] = new Vector3(x, y, 1f);
				}
			}
		}

		private static float ComputeWaveAmplitude(float windSpeed)
		{
			return 0.002f * windSpeed * windSpeed * windSpeed;
		}

		private readonly float _CutoffFactor;
	}
}
