using System;
using UnityEngine;

namespace UltimateWater
{
	public static class ButterflyFFTUtility
	{
		private static void BitReverse(int[] indices, int N, int n)
		{
			for (int i = 0; i < N; i++)
			{
				int num = 0;
				int num2 = indices[i];
				for (int j = 0; j < n; j++)
				{
					int num3 = 1 & num2;
					num = (num << 1 | num3);
					num2 >>= 1;
				}
				indices[i] = num;
			}
		}

		private static void ComputeWeights(Vector2[][] weights, int resolution, int numButterflies)
		{
			int num = resolution >> 1;
			int num2 = 1;
			float num3 = 1f / (float)resolution;
			for (int i = 0; i < numButterflies; i++)
			{
				int num4 = 0;
				int num5 = num2;
				Vector2[] array = weights[i];
				for (int j = 0; j < num; j++)
				{
					int k = num4;
					int num6 = 0;
					while (k < num5)
					{
						float f = 6.28318548f * (float)num6 * (float)num * num3;
						float num7 = Mathf.Cos(f);
						float num8 = -Mathf.Sin(f);
						array[k].x = num7;
						array[k].y = num8;
						array[k + num2].x = -num7;
						array[k + num2].y = -num8;
						k++;
						num6++;
					}
					num4 += num2 << 1;
					num5 = num4 + num2;
				}
				num >>= 1;
				num2 <<= 1;
			}
		}

		private static void ComputeWeights(float[][] weights, int resolution, int numButterflies)
		{
			int num = resolution >> 1;
			int num2 = 2;
			float num3 = 1f / (float)resolution;
			for (int i = 0; i < numButterflies; i++)
			{
				int num4 = 0;
				int num5 = num2;
				float[] array = weights[i];
				for (int j = 0; j < num; j++)
				{
					int k = num4;
					int num6 = 0;
					while (k < num5)
					{
						float f = 6.28318548f * (float)num6 * (float)num * num3;
						float num7 = Mathf.Cos(f);
						float num8 = -Mathf.Sin(f);
						array[k] = num7;
						array[k + 1] = num8;
						array[k + num2] = -num7;
						array[k + num2 + 1] = -num8;
						k += 2;
						num6++;
					}
					num4 += num2 << 1;
					num5 = num4 + num2;
				}
				num >>= 1;
				num2 <<= 1;
			}
		}

		private static void ComputeIndices(int[][] indices, int resolution, int numButterflies)
		{
			int num = resolution;
			int num2 = 1;
			for (int i = 0; i < numButterflies; i++)
			{
				num >>= 1;
				int num3 = num << 1;
				int num4 = 0;
				int num5 = 0;
				int num6 = num3;
				int[] array = indices[i];
				for (int j = 0; j < num2; j++)
				{
					int k = num5;
					int num7 = num4;
					int num8 = 0;
					while (k < num6)
					{
						array[k] = num7;
						array[k + 1] = num7 + num;
						array[num8 + num6] = num7;
						array[num8 + num6 + 1] = num7 + num;
						k += 2;
						num8 += 2;
						num7++;
					}
					num5 += num3 << 1;
					num6 += num3 << 1;
					num4 += num3;
				}
				num2 <<= 1;
			}
			ButterflyFFTUtility.BitReverse(indices[numButterflies - 1], resolution << 1, numButterflies);
		}

		public static void ComputeButterfly(int resolution, int numButterflies, out int[][] indices, out Vector2[][] weights)
		{
			indices = new int[numButterflies][];
			weights = new Vector2[numButterflies][];
			for (int i = 0; i < numButterflies; i++)
			{
				indices[i] = new int[resolution << 1];
				weights[i] = new Vector2[resolution];
			}
			ButterflyFFTUtility.ComputeIndices(indices, resolution, numButterflies);
			ButterflyFFTUtility.ComputeWeights(weights, resolution, numButterflies);
		}

		public static void ComputeButterfly(int resolution, int numButterflies, out int[][] indices, out float[][] weights)
		{
			indices = new int[numButterflies][];
			weights = new float[numButterflies][];
			for (int i = 0; i < numButterflies; i++)
			{
				indices[i] = new int[resolution << 1];
				weights[i] = new float[resolution << 1];
			}
			ButterflyFFTUtility.ComputeIndices(indices, resolution, numButterflies);
			ButterflyFFTUtility.ComputeWeights(weights, resolution, numButterflies);
		}
	}
}
