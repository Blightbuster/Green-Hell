using System;
using System.Collections.Generic;
using UltimateWater.Utils;
using UnityEngine;

namespace UltimateWater.Internal
{
	[Serializable]
	public class WaterRadialGrid : WaterPrimitiveBase
	{
		static WaterRadialGrid()
		{
			for (int i = 0; i < WaterRadialGrid._HorizontalVerticesToVertexCount.Length; i++)
			{
				WaterRadialGrid._HorizontalVerticesToVertexCount[i] = new int[600];
				float num = ((float)(i + 1) * 5f - 1f) * 0.0174532924f;
				Vector2 vector = new Vector2(Mathf.Sin(-num), Mathf.Cos(-num));
				Vector2 normalized = vector.normalized;
				for (int j = 0; j < 600; j++)
				{
					int num2 = j + 2;
					float f = num * (2f / (float)(num2 - 1) - 1f);
					Vector2 vector2 = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
					Vector2 normalized2 = vector2.normalized;
					float num3 = 1f;
					int num4 = 0;
					float num5 = Vector2.Distance(normalized, normalized2);
					while (num3 >= 0.005f)
					{
						num4++;
						num3 -= num5 * num3;
					}
					num4 += 2;
					WaterRadialGrid._HorizontalVerticesToVertexCount[i][j] = num2 * num4;
				}
			}
		}

		public override Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, int vertexCount, bool volume)
		{
			int num2;
			if (camera != null)
			{
				float num = 2f * Mathf.Atan(Mathf.Tan(camera.fieldOfView * 0.5f * 0.0174532924f) * camera.aspect) * 57.29578f;
				num2 = ((!camera.orthographic) ? Mathf.CeilToInt(num * 0.5f * 0.2f - 0.8f) : 14);
				if (num2 >= WaterRadialGrid._HorizontalVerticesToVertexCount.Length)
				{
					num2 = WaterRadialGrid._HorizontalVerticesToVertexCount.Length - 1;
				}
				matrix = this.GetMatrix(camera, ((float)(num2 + 1) * 5f - 1f) * 0.0174532924f);
			}
			else
			{
				matrix = Matrix4x4.identity;
				num2 = 14;
			}
			int num3 = vertexCount | num2 << 26;
			if (volume)
			{
				num3 = -num3;
			}
			WaterPrimitiveBase.CachedMeshSet cachedMeshSet;
			if (!this._Cache.TryGetValue(num3, out cachedMeshSet))
			{
				cachedMeshSet = (this._Cache[num3] = new WaterPrimitiveBase.CachedMeshSet(this.CreateMeshes(vertexCount, volume, num2)));
			}
			else
			{
				cachedMeshSet.Update();
			}
			return cachedMeshSet.Meshes;
		}

		protected override Mesh[] CreateMeshes(int vertexCount, bool volume)
		{
			throw new NotImplementedException();
		}

		protected Mesh[] CreateMeshes(int vertexCount, bool volume, int fovIndex)
		{
			int num = 0;
			int[] array = WaterRadialGrid._HorizontalVerticesToVertexCount[fovIndex];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > vertexCount)
				{
					num = i + 1;
					break;
				}
			}
			int num2 = Mathf.FloorToInt((float)vertexCount / (float)num);
			int num3 = num2;
			List<Mesh> list = new List<Mesh>();
			List<Vector3> list2 = new List<Vector3>();
			List<int> list3 = new List<int>();
			int num4 = 0;
			int num5 = 0;
			float num6 = ((float)(fovIndex + 1) * 5f - 1f) * 0.0174532924f;
			Vector2[] array2 = new Vector2[num];
			for (int j = 0; j < num; j++)
			{
				float num7 = (float)j / (float)(num - 1) * 2f - 1f;
				num7 = ((num7 < 0f) ? (-1f - Mathf.Sin((num7 - 1f) * 3.14159274f * 0.5f)) : (1f + Mathf.Sin((num7 - 1f) * 3.14159274f * 0.5f)));
				num7 *= num6;
				Vector2[] array3 = array2;
				int num8 = j;
				Vector2 vector = new Vector2(Mathf.Sin(num7), Mathf.Cos(num7));
				array3[num8] = vector.normalized;
			}
			float num9 = 1f;
			float num10 = 1f;
			float num11 = Vector2.Distance(array2[0], array2[1]);
			if (volume)
			{
				while (num9 > 0.4f)
				{
					num10 = num9;
					num9 -= num11 * num9;
					num3--;
				}
			}
			for (int k = 0; k < num3; k++)
			{
				for (int l = 0; l < num; l++)
				{
					Vector2 vector2 = array2[l] * num9;
					if (k == num3 - 1)
					{
						list2.Add(new Vector3(0f, 0f, 0f));
					}
					else if (k > 1 || !volume)
					{
						list2.Add(new Vector3(vector2.x, 0f, vector2.y));
					}
					else if (k == 1)
					{
						vector2 = array2[l] * (1f - num11);
						list2.Add(new Vector3(vector2.x * 10f, -0.9f, vector2.y) * 0.5f);
					}
					else
					{
						vector2 = array2[l];
						list2.Add(new Vector3(vector2.x * 10f, -0.9f, vector2.y * -10f) * 0.5f);
					}
					if (l != 0 && k != 0 && num4 > num)
					{
						list3.Add(num4);
						list3.Add(num4 - 1);
						list3.Add(num4 - num - 1);
						list3.Add(num4 - num);
					}
					num4++;
					if (num4 == 65000)
					{
						Mesh item = base.CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Radial Grid {0}x{1} - {2:00}", num, num2, num5), false);
						list.Add(item);
						l--;
						k--;
						num9 = num10;
						num4 = 0;
						list2.Clear();
						list3.Clear();
						num5++;
					}
				}
				num10 = num9;
				num9 -= num11 * num9;
			}
			if (num4 != 0)
			{
				Mesh item2 = base.CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Radial Grid {0}x{1} - {2:00}", num, num2, num5), false);
				list.Add(item2);
			}
			return list.ToArray();
		}

		protected Matrix4x4 GetMatrix(Camera camera, float fov)
		{
			float y = this._Water.transform.position.y;
			Vector3 vector = UltimateWater.Utils.Math.ViewportWaterPerpendicular(camera);
			Vector3 vector2 = UltimateWater.Utils.Math.ViewportWaterRight(camera);
			float num;
			if (vector.IsNaN() || vector2.IsNaN())
			{
				num = 2f * camera.worldToCameraMatrix.MultiplyPoint(new Vector3(0f, y, 0f)).magnitude;
			}
			else
			{
				Vector3 vector3 = UltimateWater.Utils.Math.RaycastPlane(camera, y, vector - vector2);
				num = UltimateWater.Utils.Math.RaycastPlane(camera, y, vector + vector2).x - vector3.x;
				if (num < 0f)
				{
					num = -num;
				}
			}
			float f = Mathf.Clamp01(Vector3.Dot(camera.transform.forward, Vector3.down) - 0.5f);
			float value = WaterRadialGrid._WidthCorrection * Mathf.Pow(f, 1f) / camera.aspect;
			num *= 1f + Mathf.Clamp(value, 0f, float.MaxValue);
			float num2 = camera.farClipPlane;
			if (WaterProjectSettings.Instance.ClipWaterCameraRange)
			{
				num2 = Mathf.Min(camera.farClipPlane, WaterProjectSettings.Instance.CameraClipRange);
			}
			Vector3 position = camera.transform.position;
			float num3 = Mathf.Tan(fov * 0.5f);
			float num4 = -(num + Mathf.Max(0f, num2 * 0.005f - camera.nearClipPlane) + (this._Water.MaxHorizontalDisplacement + this._Water.MaxVerticalDisplacement) / num3);
			if (camera.orthographic)
			{
				num4 -= camera.orthographicSize * 3.2f;
			}
			float y2 = camera.transform.forward.y;
			Vector3 vector4 = (y2 >= -0.98f && y2 <= 0.98f) ? camera.transform.forward : (-camera.transform.up);
			float num5 = Mathf.Sqrt(vector4.x * vector4.x + vector4.z * vector4.z);
			vector4.x /= num5;
			vector4.z /= num5;
			float num6 = num2 - num4;
			return Matrix4x4.TRS(new Vector3(position.x + vector4.x * num4, y, position.z + vector4.z * num4), Quaternion.AngleAxis(Mathf.Atan2(vector4.x, vector4.z) * 57.29578f, Vector3.up), new Vector3(num6, num6, num6));
		}

		protected override Matrix4x4 GetMatrix(Camera camera)
		{
			throw new InvalidOperationException();
		}

		private static float _WidthCorrection = 5f;

		private static readonly int[][] _HorizontalVerticesToVertexCount = new int[17][];
	}
}
