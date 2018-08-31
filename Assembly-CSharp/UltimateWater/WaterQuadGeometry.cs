using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public class WaterQuadGeometry : WaterPrimitiveBase
	{
		public override Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, int vertexCount, bool volume)
		{
			matrix = this.GetMatrix(camera);
			return (this._Meshes == null) ? (this._Meshes = new Mesh[]
			{
				Quads.BipolarXZ
			}) : this._Meshes;
		}

		protected override Matrix4x4 GetMatrix(Camera camera)
		{
			Vector3 position = camera.transform.position;
			float farClipPlane = camera.farClipPlane;
			return new Matrix4x4
			{
				m03 = position.x,
				m13 = position.y,
				m23 = position.z,
				m00 = farClipPlane,
				m11 = farClipPlane,
				m22 = farClipPlane
			};
		}

		protected override Mesh[] CreateMeshes(int vertexCount, bool volume)
		{
			return new Mesh[]
			{
				Quads.BipolarXZ
			};
		}

		private Mesh[] _Meshes;
	}
}
