using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class Quads
	{
		public static Mesh BipolarXY
		{
			get
			{
				if (!Quads._Initialized)
				{
					Quads.CreateMeshes();
				}
				return Quads._BipolarXY;
			}
		}

		public static Mesh BipolarXInversedY
		{
			get
			{
				if (!Quads._Initialized)
				{
					Quads.CreateMeshes();
				}
				return Quads._BipolarXInversedY;
			}
		}

		public static Mesh BipolarXZ
		{
			get
			{
				if (!Quads._Initialized)
				{
					Quads.CreateMeshes();
				}
				return Quads._BipolarXZ;
			}
		}

		private static void CreateMeshes()
		{
			Quads._BipolarXY = Quads.CreateBipolarXY(false);
			Quads._BipolarXInversedY = Quads.CreateBipolarXY(SystemInfo.graphicsDeviceVersion.Contains("Direct3D"));
			Quads._BipolarXZ = Quads.CreateBipolarXZ();
			Quads._Initialized = true;
		}

		private static Mesh CreateBipolarXY(bool inversedY)
		{
			Mesh mesh = new Mesh
			{
				hideFlags = HideFlags.DontSave,
				vertices = new Vector3[]
				{
					new Vector3(-1f, -1f, 0f),
					new Vector3(1f, -1f, 0f),
					new Vector3(1f, 1f, 0f),
					new Vector3(-1f, 1f, 0f)
				},
				uv = new Vector2[]
				{
					new Vector2(0f, (!inversedY) ? 0f : 1f),
					new Vector2(1f, (!inversedY) ? 0f : 1f),
					new Vector2(1f, (!inversedY) ? 1f : 0f),
					new Vector2(0f, (!inversedY) ? 1f : 0f)
				}
			};
			mesh.SetTriangles(new int[]
			{
				0,
				1,
				2,
				0,
				2,
				3
			}, 0);
			mesh.UploadMeshData(true);
			return mesh;
		}

		private static Mesh CreateBipolarXZ()
		{
			Mesh mesh = new Mesh
			{
				name = "Shoreline Quad Mesh",
				hideFlags = HideFlags.DontSave,
				vertices = new Vector3[]
				{
					new Vector3(-1f, 0f, -1f),
					new Vector3(-1f, 0f, 1f),
					new Vector3(1f, 0f, 1f),
					new Vector3(1f, 0f, -1f)
				},
				uv = new Vector2[]
				{
					new Vector2(0f, 0f),
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f)
				}
			};
			mesh.SetIndices(new int[]
			{
				0,
				1,
				2,
				3
			}, MeshTopology.Quads, 0);
			mesh.UploadMeshData(true);
			return mesh;
		}

		private static Mesh _BipolarXY;

		private static Mesh _BipolarXInversedY;

		private static Mesh _BipolarXZ;

		private static bool _Initialized;
	}
}
