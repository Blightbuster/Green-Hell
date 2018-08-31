using System;
using UnityEngine;

namespace UltimateWater
{
	public class UnifiedSpectrum : WaterWavesSpectrum
	{
		public UnifiedSpectrum(float tileSize, float gravity, float windSpeed, float amplitude, float freqScale, float fetch) : base(tileSize, gravity, windSpeed, amplitude)
		{
			this._Fetch = fetch;
			this._FreqScale = freqScale;
		}

		public override void ComputeSpectrum(Vector3[] spectrum, float tileSizeMultiplier, int maxResolution, System.Random random)
		{
			int num = Mathf.RoundToInt(Mathf.Sqrt((float)spectrum.Length));
			int num2 = num / 2;
			int num3 = (maxResolution - num) / 2;
			if (num3 < 0)
			{
				num3 = 0;
			}
			float num4 = 6.28318548f / (base.TileSize * tileSizeMultiplier * this._FreqScale);
			float windSpeed = this._WindSpeed;
			float num5 = 0.84f * Mathf.Pow((float)Math.Tanh((double)Mathf.Pow(this._Fetch / 22000f, 0.4f)), -0.75f);
			float f = (num5 > 1f) ? (1.7f + 6f * Mathf.Log(num5)) : 1.7f;
			float num6 = 2f * this._Gravity / 0.0529f;
			float num7 = this._Gravity * FastMath.Pow2(num5 / windSpeed);
			float num8 = this.PhaseSpeed(num7, num6);
			float num9 = windSpeed / num8;
			float num10 = 0.006f * Mathf.Sqrt(num9);
			float num11 = -num9 / Mathf.Sqrt(10f);
			float num12 = 0.08f * (1f + 4f * Mathf.Pow(num5, -3f));
			float num13 = 1f / (2f * num12 * num12);
			float num14 = 3.7E-05f * windSpeed * windSpeed / this._Gravity * Mathf.Pow(windSpeed / num8, 0.9f);
			float num15 = windSpeed * 0.41f / Mathf.Log(10f / num14);
			float num16 = Mathf.Log(2f) / 4f;
			float num17 = 4f;
			float num18 = 0.13f * num15 / 0.23f;
			float num19 = 0.01f * ((num15 >= 0.23f) ? (1f + 3f * Mathf.Log(num15 / 0.23f)) : (1f + Mathf.Log(num15 / 0.23f)));
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < maxResolution; j++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value = UnityEngine.Random.value;
				}
			}
			for (int k = 0; k < num; k++)
			{
				float num20 = num4 * (float)(k - num2);
				float num21 = num20 * num20;
				int num22 = (k + num2) % num;
				int num23 = num22 * num;
				for (int l = 0; l < num3; l++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value2 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value2 = UnityEngine.Random.value;
				}
				for (int m = 0; m < num; m++)
				{
					float num24 = num4 * (float)(m - num2);
					float num25 = Mathf.Sqrt(num21 + num24 * num24);
					float num26 = this.PhaseSpeed(num25, num6);
					float num27 = Mathf.Exp(-1.25f * FastMath.Pow2(num7 / num25));
					float num28 = Mathf.Sqrt(num25 / num7) - 1f;
					float p = Mathf.Exp(-FastMath.Pow2(num28) * num13);
					float num29 = Mathf.Pow(f, p);
					float num30 = num27 * num29 * Mathf.Exp(num11 * num28);
					float num31 = 0.5f * num10 * (num8 / num26) * num30;
					float num32 = Mathf.Exp(-0.25f * FastMath.Pow2(num25 / num6 - 1f));
					float num33 = 0.5f * num19 * (0.23f / num26) * num32 * num27;
					float z = (float)Math.Tanh((double)(num16 + num17 * Mathf.Pow(num26 / num8, 2.5f) + num18 * Mathf.Pow(0.23f / num26, 2.5f)));
					float num34 = this._Amplitude * (num31 + num33) / (num25 * num25 * num25 * num25 * 2f * 3.14159274f);
					if (num34 > 0f)
					{
						num34 = Mathf.Sqrt(num34) * num4 * 0.5f;
					}
					else
					{
						num34 = 0f;
					}
					float x = FastMath.Gauss01() * num34;
					float y = FastMath.Gauss01() * num34;
					int num35 = (m + num2) % num;
					if (k == num2 && m == num2)
					{
						x = 0f;
						y = 0f;
						z = 0f;
					}
					spectrum[num23 + num35] = new Vector3(x, y, z);
				}
				for (int n = 0; n < num3; n++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value3 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value3 = UnityEngine.Random.value;
				}
			}
			for (int num36 = 0; num36 < num3; num36++)
			{
				for (int num37 = 0; num37 < maxResolution; num37++)
				{
					UnityEngine.Random.Range(1E-06f, 1f);
					float value4 = UnityEngine.Random.value;
					UnityEngine.Random.Range(1E-06f, 1f);
					value4 = UnityEngine.Random.value;
				}
			}
		}

		private float PhaseSpeed(float k, float km)
		{
			return Mathf.Sqrt(this._Gravity / k * (1f + FastMath.Pow2(k / km)));
		}

		private readonly float _Fetch;

		private readonly float _FreqScale;
	}
}
