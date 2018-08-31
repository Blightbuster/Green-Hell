using System;
using UnityEngine;

namespace UltimateWater.Utils
{
	public static class Math
	{
		public static bool IsNaN(this Vector3 vector)
		{
			return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
		}

		public static Vector3 RaycastPlane(Camera camera, float planeHeight, Vector3 pos)
		{
			Ray ray = camera.ViewportPointToRay(pos);
			Vector3 direction = ray.direction;
			if (camera.transform.position.y > planeHeight)
			{
				if (direction.y > -0.01f)
				{
					direction.y = -direction.y - 0.02f;
				}
			}
			else if (direction.y < 0.01f)
			{
				direction.y = -direction.y + 0.02f;
			}
			float num = -(ray.origin.y - planeHeight) / direction.y;
			float f = -camera.transform.eulerAngles.y * 0.0174532924f;
			float num2 = Mathf.Sin(f);
			float num3 = Mathf.Cos(f);
			return new Vector3(num * (direction.x * num3 + direction.z * num2), num * direction.y, num * (direction.x * num2 + direction.z * num3));
		}

		public static Vector3 ViewportWaterPerpendicular(Camera camera)
		{
			Vector3 result = camera.worldToCameraMatrix.MultiplyVector(new Vector3(0f, -1f, 0f));
			float num = 0.5f / Mathf.Sqrt(result.x * result.x + result.y * result.y);
			result.x = result.x * num + 0.5f;
			result.y = result.y * num + 0.5f;
			return result;
		}

		public static Vector3 ViewportWaterRight(Camera camera)
		{
			Vector3 result = camera.worldToCameraMatrix.MultiplyVector(camera.transform.right);
			float num = 0.5f / Mathf.Sqrt(result.x * result.x + result.y * result.y);
			result.x *= num;
			result.y *= num;
			return result;
		}

		public static float[] GaussianTerms(float sigma)
		{
			int num = 3;
			float[] gaussianTerms = Math._GaussianTerms;
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				gaussianTerms[i] = Math.Gaussian(i - num / 2, sigma);
				num2 += gaussianTerms[i];
			}
			for (int j = 0; j < num; j++)
			{
				gaussianTerms[j] /= num2;
			}
			return gaussianTerms;
		}

		private static float Gaussian(int x, float sigma)
		{
			float num = 2f * sigma * sigma;
			return Mathf.Exp((float)(-(float)x * x) / num) / (num * 3.14159274f);
		}

		private static readonly float[] _GaussianTerms = new float[3];
	}
}
