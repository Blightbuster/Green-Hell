using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class CpuFFT
	{
		public void Compute(WaterTileSpectrum targetSpectrum, float time, int outputBufferIndex)
		{
			this._TargetSpectrum = targetSpectrum;
			this._Time = time;
			Vector2[] directionalSpectrum;
			Vector2[] displacements;
			Vector4[] forceAndHeight;
			lock (targetSpectrum)
			{
				this._Resolution = targetSpectrum.ResolutionFFT;
				directionalSpectrum = targetSpectrum.DirectionalSpectrum;
				displacements = targetSpectrum.Displacements[outputBufferIndex];
				forceAndHeight = targetSpectrum.ForceAndHeight[outputBufferIndex];
			}
			CpuFFT.FFTBuffers fftbuffers;
			if (!CpuFFT._BuffersCache.TryGetValue(this._Resolution, out fftbuffers))
			{
				fftbuffers = (CpuFFT._BuffersCache[this._Resolution] = new CpuFFT.FFTBuffers(this._Resolution));
			}
			float tileSize = targetSpectrum.WindWaves.UnscaledTileSizes[targetSpectrum.TileIndex];
			Vector3[] precomputedK = fftbuffers.GetPrecomputedK(tileSize);
			if (targetSpectrum.DirectionalSpectrumDirty > 0)
			{
				this.ComputeDirectionalSpectra(targetSpectrum.TileIndex, directionalSpectrum, precomputedK);
				targetSpectrum.DirectionalSpectrumDirty--;
			}
			this.ComputeTimedSpectra(directionalSpectrum, fftbuffers.Timed, precomputedK);
			this.ComputeFFT(fftbuffers.Timed, displacements, forceAndHeight, fftbuffers.Indices, fftbuffers.Weights, fftbuffers.PingPongA, fftbuffers.PingPongB);
		}

		private void ComputeDirectionalSpectra(int scaleIndex, Vector2[] directional, Vector3[] kMap)
		{
			float num = 1f - this._TargetSpectrum.WindWaves.SpectrumDirectionality;
			Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> cachedSpectraDirect = this._TargetSpectrum.WindWaves.SpectrumResolver.GetCachedSpectraDirect();
			int num2 = this._Resolution * this._Resolution;
			int num3 = this._Resolution >> 1;
			int finalResolution = this._TargetSpectrum.WindWaves.FinalResolution;
			Vector2 windDirection = this._TargetSpectrum.WindWaves.SpectrumResolver.WindDirection;
			for (int i = 0; i < num2; i++)
			{
				directional[i].x = 0f;
				directional[i].y = 0f;
			}
			object obj = cachedSpectraDirect;
			lock (obj)
			{
				Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>.ValueCollection.Enumerator enumerator = cachedSpectraDirect.Values.GetEnumerator();
				while (enumerator.MoveNext())
				{
					WaterWavesSpectrumData waterWavesSpectrumData = enumerator.Current;
					if (waterWavesSpectrumData != null)
					{
						float weight = waterWavesSpectrumData.Weight;
						if (waterWavesSpectrumData.GetStandardDeviation() * weight > 0.003f)
						{
							int num4 = 0;
							int num5 = 0;
							Vector3[] array = waterWavesSpectrumData.SpectrumValues[scaleIndex];
							for (int j = 0; j < this._Resolution; j++)
							{
								if (j == num3)
								{
									num5 += (finalResolution - this._Resolution) * finalResolution;
								}
								for (int k = 0; k < this._Resolution; k++)
								{
									if (k == num3)
									{
										num5 += finalResolution - this._Resolution;
									}
									float x = kMap[num4].x;
									float y = kMap[num4].y;
									if (x == 0f && y == 0f)
									{
										x = windDirection.x;
										y = windDirection.y;
									}
									float num6 = windDirection.x * x + windDirection.y * y;
									float num7 = Mathf.Sqrt(1f + array[num5].z * (2f * num6 * num6 - 1f));
									if (num6 < 0f)
									{
										num7 *= num;
									}
									float num8 = num7 * weight;
									int num9 = num4;
									directional[num9].x = directional[num9].x + array[num5].x * num8;
									int num10 = num4++;
									directional[num10].y = directional[num10].y + array[num5++].y * num8;
								}
							}
						}
					}
				}
				enumerator.Dispose();
			}
			List<WaterWavesSpectrumDataBase> overlayedSpectraDirect = this._TargetSpectrum.WindWaves.SpectrumResolver.GetOverlayedSpectraDirect();
			object obj2 = overlayedSpectraDirect;
			lock (obj2)
			{
				for (int l = overlayedSpectraDirect.Count - 1; l >= 0; l--)
				{
					WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = overlayedSpectraDirect[l];
					float weight2 = waterWavesSpectrumDataBase.Weight;
					windDirection = waterWavesSpectrumDataBase.WindDirection;
					if (waterWavesSpectrumDataBase.GetStandardDeviation() * weight2 > 0.003f)
					{
						float x2 = waterWavesSpectrumDataBase.WeatherSystemOffset.x;
						float y2 = waterWavesSpectrumDataBase.WeatherSystemOffset.y;
						float weatherSystemRadius = waterWavesSpectrumDataBase.WeatherSystemRadius;
						float num11 = Mathf.Sqrt(x2 * x2 + y2 * y2);
						float num12 = 0.84f * Mathf.Pow((float)Math.Tanh((double)Mathf.Pow(num11 / 22000f, 0.4f)), -0.75f);
						float num13 = waterWavesSpectrumDataBase.Gravity * FastMath.Pow2(num12 / 10f);
						int num14 = 0;
						int num15 = 0;
						Vector3[] array2 = waterWavesSpectrumDataBase.SpectrumValues[scaleIndex];
						for (int m = 0; m < this._Resolution; m++)
						{
							if (m == num3)
							{
								num15 += (finalResolution - this._Resolution) * finalResolution;
							}
							int n = 0;
							while (n < this._Resolution)
							{
								if (n == num3)
								{
									num15 += finalResolution - this._Resolution;
								}
								float x3 = kMap[num14].x;
								float y3 = kMap[num14].y;
								if (x3 == 0f && y3 == 0f)
								{
									x3 = windDirection.x;
									y3 = windDirection.y;
								}
								float num16 = windDirection.x * x3 + windDirection.y * y3;
								float num17 = Mathf.Sqrt(1f + array2[num15].z * (2f * num16 * num16 - 1f));
								if (num16 < 0f)
								{
									num17 *= num;
								}
								float num18 = num17 * weight2;
								if (weatherSystemRadius == 0f)
								{
									goto IL_5EA;
								}
								float z = kMap[num14].z;
								float num19 = -2f * x3 * x2 + -2f * y3 * y2;
								float num20 = x2 * x2 + y2 * y2 - weatherSystemRadius * weatherSystemRadius;
								float num21 = num19 * num19 - 4f * num20;
								if (num21 < 0f)
								{
									directional[num14].x = 0f;
									directional[num14++].y = 0f;
									num15++;
								}
								else
								{
									float num22 = Mathf.Sqrt(num21);
									float num23 = (num22 - num19) * 0.5f;
									float num24 = (-num22 - num19) * 0.5f;
									if (num23 > 0f && num24 > 0f)
									{
										directional[num14].x = 0f;
										directional[num14++].y = 0f;
										num15++;
									}
									else
									{
										Vector2 a = new Vector2(x3 * num23, y3 * num23);
										Vector2 b = new Vector2(x3 * num24, y3 * num24);
										float num25 = Vector2.Distance(a, b) / (weatherSystemRadius * 2f);
										num18 *= num25;
										if (num23 * num24 > 0f)
										{
											float num26 = Mathf.Min(-num23, -num24);
											float num27 = z / num13;
											float num28 = Mathf.Exp(-1E-06f * num26 * num27 * num27);
											num18 *= num28;
											goto IL_5EA;
										}
										goto IL_5EA;
									}
								}
								IL_63E:
								n++;
								continue;
								IL_5EA:
								int num29 = num14;
								directional[num29].x = directional[num29].x + array2[num15].x * num18;
								int num30 = num14++;
								directional[num30].y = directional[num30].y + array2[num15++].y * num18;
								goto IL_63E;
							}
						}
					}
				}
			}
		}

		private void ComputeTimedSpectra(Vector2[] directional, float[] timed, Vector3[] kMap)
		{
			Vector2 windDirection = this._TargetSpectrum.WindWaves.SpectrumResolver.WindDirection;
			float gravity = this._TargetSpectrum.Water.Gravity;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < this._Resolution; i++)
			{
				for (int j = 0; j < this._Resolution; j++)
				{
					float x = kMap[num].x;
					float y = kMap[num].y;
					float z = kMap[num].z;
					if (x == 0f && y == 0f)
					{
						x = windDirection.x;
						y = windDirection.y;
					}
					int num3 = this._Resolution * ((this._Resolution - i) % this._Resolution) + (this._Resolution - j) % this._Resolution;
					Vector2 vector = directional[num];
					Vector2 vector2 = directional[num3];
					float f = this._Time * Mathf.Sqrt(gravity * z);
					float num4 = Mathf.Sin(f);
					float num5 = Mathf.Cos(f);
					float num6 = (vector.x + vector2.x) * num5 - (vector.y + vector2.y) * num4;
					float num7 = (vector.x - vector2.x) * num4 + (vector.y - vector2.y) * num5;
					timed[num2++] = num7 * x;
					timed[num2++] = num7 * y;
					timed[num2++] = -num6;
					timed[num2++] = num7;
					timed[num2++] = num6 * x;
					timed[num2++] = num6 * y;
					timed[num2++] = num7;
					timed[num2++] = num6;
					timed[num2++] = num7 * x;
					timed[num2++] = -num6 * x;
					timed[num2++] = num7 * y;
					timed[num2++] = -num6 * y;
					num++;
				}
			}
		}

		private void ComputeFFT(float[] data, Vector2[] displacements, Vector4[] forceAndHeight, int[][] indices, float[][] weights, float[] pingPongA, float[] pingPongB)
		{
			int num = pingPongA.Length;
			int num2 = 0;
			for (int i = this._Resolution - 1; i >= 0; i--)
			{
				Array.Copy(data, num2, pingPongA, 0, num);
				this.FFT(indices, weights, ref pingPongA, ref pingPongB);
				Array.Copy(pingPongA, 0, data, num2, num);
				num2 += num;
			}
			num2 = this._Resolution * (this._Resolution + 1) * 12;
			for (int j = this._Resolution - 1; j >= 0; j--)
			{
				num2 -= 12;
				int num3 = num2;
				for (int k = num - 12; k >= 0; k -= 12)
				{
					num3 -= num;
					for (int l = 0; l < 12; l++)
					{
						pingPongA[k + l] = data[num3 + l];
					}
				}
				this.FFT(indices, weights, ref pingPongA, ref pingPongB);
				num3 = num2 / 12;
				for (int m = num - 12; m >= 0; m -= 12)
				{
					num3 -= this._Resolution;
					forceAndHeight[num3] = new Vector4(pingPongA[m], pingPongA[m + 2], pingPongA[m + 1], pingPongA[m + 7]);
					displacements[num3] = new Vector2(pingPongA[m + 8], pingPongA[m + 10]);
				}
			}
		}

		private void FFT(int[][] indices, float[][] weights, ref float[] pingPongA, ref float[] pingPongB)
		{
			int num = weights.Length;
			for (int i = 0; i < num; i++)
			{
				int[] array = indices[num - i - 1];
				float[] array2 = weights[i];
				int num2 = (this._Resolution - 1) * 12;
				for (int j = array.Length - 2; j >= 0; j -= 2)
				{
					int num3 = array[j];
					int num4 = array[j + 1];
					float num5 = array2[j];
					float num6 = array2[j + 1];
					int num7 = num4 + 4;
					pingPongB[num2++] = pingPongA[num3++] + num6 * pingPongA[num7++] + num5 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num6 * pingPongA[num7++] + num5 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num6 * pingPongA[num7++] + num5 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num6 * pingPongA[num7] + num5 * pingPongA[num4++];
					num7 = num4;
					num4 -= 4;
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4++];
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4];
					num4 = num7;
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4 + 1];
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] + num6 * pingPongA[num4];
					pingPongB[num2++] = pingPongA[num3++] + num5 * pingPongA[num7++] - num6 * pingPongA[num4 + 3];
					pingPongB[num2] = pingPongA[num3] + num5 * pingPongA[num7] + num6 * pingPongA[num4 + 2];
					num2 -= 23;
				}
				float[] array3 = pingPongA;
				pingPongA = pingPongB;
				pingPongB = array3;
			}
		}

		private WaterTileSpectrum _TargetSpectrum;

		private float _Time;

		private int _Resolution;

		private static readonly Dictionary<int, CpuFFT.FFTBuffers> _BuffersCache = new Dictionary<int, CpuFFT.FFTBuffers>();

		public class FFTBuffers
		{
			public FFTBuffers(int resolution)
			{
				this._Resolution = resolution;
				this.Timed = new float[resolution * resolution * 12];
				this.PingPongA = new float[resolution * 12];
				this.PingPongB = new float[resolution * 12];
				this.NumButterflies = (int)(Mathf.Log((float)resolution) / Mathf.Log(2f));
				ButterflyFFTUtility.ComputeButterfly(resolution, this.NumButterflies, out this.Indices, out this.Weights);
				for (int i = 0; i < this.Indices.Length; i++)
				{
					int[] array = this.Indices[i];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] *= 12;
					}
				}
			}

			public Vector3[] GetPrecomputedK(float tileSize)
			{
				Vector3[] array;
				if (!this._PrecomputedKMap.TryGetValue(tileSize, out array))
				{
					int num = this._Resolution >> 1;
					float num2 = 6.28318548f / tileSize;
					array = new Vector3[this._Resolution * this._Resolution];
					int num3 = 0;
					for (int i = 0; i < this._Resolution; i++)
					{
						int num4 = (i + num) % this._Resolution;
						float num5 = num2 * (float)(num4 - num);
						for (int j = 0; j < this._Resolution; j++)
						{
							int num6 = (j + num) % this._Resolution;
							float num7 = num2 * (float)(num6 - num);
							float num8 = Mathf.Sqrt(num7 * num7 + num5 * num5);
							array[num3++] = new Vector3((num8 == 0f) ? 0f : (num7 / num8), (num8 == 0f) ? 0f : (num5 / num8), num8);
						}
					}
					this._PrecomputedKMap[tileSize] = array;
				}
				return array;
			}

			public readonly float[] Timed;

			public readonly float[] PingPongA;

			public readonly float[] PingPongB;

			public readonly int[][] Indices;

			public readonly float[][] Weights;

			public readonly int NumButterflies;

			private readonly int _Resolution;

			private readonly Dictionary<float, Vector3[]> _PrecomputedKMap = new Dictionary<float, Vector3[]>(new FloatEqualityComparer());
		}
	}
}
