using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AdvancedTerrainGrass
{
	[Serializable]
	public class GrassCellContent
	{
		public void ReleaseCompleteCellContent()
		{
			this.state = 0;
			this.v_matrices = null;
			this.block.Clear();
			if (this.matrixBuffer != null)
			{
				this.matrixBuffer.Release();
			}
			if (this.argsBuffer != null)
			{
				this.argsBuffer.Release();
			}
		}

		public void ReleaseCellContent()
		{
			this.state = 0;
			this.v_matrices = null;
			this.block.Clear();
			if (this.matrixBuffer != null)
			{
				this.matrixBuffer.Release();
			}
		}

		public void InitCellContent_Delegated()
		{
			this.matrixBuffer = new ComputeBuffer(this.v_matrices.Length, 64);
			this.matrixBuffer.SetData(this.v_matrices);
			this.block.Clear();
			this.block.SetBuffer(this.GrassMatrixBufferPID, this.matrixBuffer);
			uint num = (this.v_mesh != null) ? this.v_mesh.GetIndexCount(0) : 0u;
			this.args[0] = num;
			this.args[1] = (uint)this.v_matrices.Length;
			this.argsBuffer.SetData(this.args);
			this.bounds.center = this.Center;
			float num2 = (this.Center.x - this.Pivot.x) * 2f;
			this.bounds.extents = new Vector3(num2, num2, num2);
			this.state = 3;
		}

		public void DrawCellContent_Delegated(Camera CameraInWichGrassWillBeDrawn, int CameraLayer)
		{
			if (this.v_mesh == null)
			{
				return;
			}
			Graphics.DrawMeshInstancedIndirect(this.v_mesh, 0, this.v_mat, this.bounds, this.argsBuffer, 0, this.block, this.ShadowCastingMode, true, CameraLayer, CameraInWichGrassWillBeDrawn);
		}

		public int index;

		public int Layer;

		public int[] SoftlyMergedLayers;

		public int state;

		public Mesh v_mesh;

		public Material v_mat;

		public int GrassMatrixBufferPID;

		public ShadowCastingMode ShadowCastingMode;

		public int Instances;

		public Vector3 Center;

		public Vector3 Pivot;

		public Matrix4x4[] v_matrices;

		public int PatchOffsetX;

		public int PatchOffsetZ;

		private ComputeBuffer matrixBuffer;

		public ComputeBuffer argsBuffer;

		public uint[] args = new uint[5];

		private Bounds bounds;

		public MaterialPropertyBlock block;
	}
}
