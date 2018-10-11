using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater.Internal
{
	[Serializable]
	public class WaterGeometry
	{
		public WaterGeometry.Type GeometryType
		{
			get
			{
				return this._Type;
			}
		}

		public int VertexCount
		{
			get
			{
				return this._BaseVertexCount;
			}
		}

		public int TesselatedBaseVertexCount
		{
			get
			{
				return this._TesselatedBaseVertexCount;
			}
		}

		public bool AdaptToResolution
		{
			get
			{
				return this._AdaptToResolution;
			}
		}

		public bool Triangular
		{
			get
			{
				return this._Type == WaterGeometry.Type.CustomMeshes && this._CustomSurfaceMeshes.Triangular;
			}
		}

		public WaterCustomSurfaceMeshes CustomSurfaceMeshes
		{
			get
			{
				return this._CustomSurfaceMeshes;
			}
		}

		public Mesh[] GetMeshes(WaterGeometryType geometryType, int vertexCount, bool volume)
		{
			if (geometryType == WaterGeometryType.ProjectionGrid)
			{
				throw new InvalidOperationException("Projection grid needs camera to be retrieved. Use GetTransformedMeshes instead.");
			}
			if (this._Type == WaterGeometry.Type.CustomMeshes)
			{
				geometryType = WaterGeometryType.Auto;
			}
			switch (geometryType)
			{
			case WaterGeometryType.Auto:
				switch (this._Type)
				{
				case WaterGeometry.Type.RadialGrid:
				{
					Matrix4x4 matrix4x;
					return this._RadialGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
				}
				case WaterGeometry.Type.ProjectionGrid:
				{
					Matrix4x4 matrix4x;
					return this._ProjectionGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
				}
				case WaterGeometry.Type.UniformGrid:
				{
					Matrix4x4 matrix4x;
					return this._UniformGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
				}
				case WaterGeometry.Type.CustomMeshes:
				{
					Matrix4x4 matrix4x;
					return this._CustomSurfaceMeshes.GetTransformedMeshes(null, out matrix4x, volume);
				}
				default:
					throw new InvalidOperationException("Unknown water geometry type.");
				}
				break;
			case WaterGeometryType.RadialGrid:
			{
				Matrix4x4 matrix4x;
				return this._RadialGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
			}
			case WaterGeometryType.ProjectionGrid:
			{
				Matrix4x4 matrix4x;
				return this._ProjectionGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
			}
			case WaterGeometryType.UniformGrid:
			{
				Matrix4x4 matrix4x;
				return this._UniformGrid.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
			}
			case WaterGeometryType.QuadGeometry:
			{
				Matrix4x4 matrix4x;
				return this._QuadGeometry.GetTransformedMeshes(null, out matrix4x, vertexCount, volume);
			}
			default:
				throw new InvalidOperationException("Unknown water geometry type.");
			}
		}

		public Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, WaterGeometryType geometryType, bool volume, int vertexCount = 0)
		{
			if (vertexCount == 0)
			{
				vertexCount = this.ComputeVertexCountForCamera(camera);
			}
			if (this._Type == WaterGeometry.Type.CustomMeshes)
			{
				geometryType = WaterGeometryType.Auto;
			}
			switch (geometryType)
			{
			case WaterGeometryType.Auto:
				switch (this._Type)
				{
				case WaterGeometry.Type.RadialGrid:
					return this._RadialGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case WaterGeometry.Type.ProjectionGrid:
					return this._ProjectionGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case WaterGeometry.Type.UniformGrid:
					return this._UniformGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case WaterGeometry.Type.CustomMeshes:
					return this._CustomSurfaceMeshes.GetTransformedMeshes(null, out matrix, volume);
				default:
					throw new InvalidOperationException("Unknown water geometry type.");
				}
				break;
			case WaterGeometryType.RadialGrid:
				return this._RadialGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			case WaterGeometryType.ProjectionGrid:
				return this._ProjectionGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			case WaterGeometryType.UniformGrid:
				return this._UniformGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			case WaterGeometryType.QuadGeometry:
				return this._QuadGeometry.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			default:
				throw new InvalidOperationException("Unknown water geometry type.");
			}
		}

		public int ComputeVertexCountForCamera(Camera camera)
		{
			return (!this._AdaptToResolution) ? this._ThisSystemVertexCount : ((int)((float)this._ThisSystemVertexCount * ((float)(camera.pixelWidth * camera.pixelHeight) / 2073600f) + 0.5f));
		}

		private void UpdateVertexCount()
		{
			this._ThisSystemVertexCount = ((!SystemInfo.supportsComputeShaders) ? Mathf.Min(this._BaseVertexCount, WaterQualitySettings.Instance.MaxVertexCount) : Mathf.Min(this._TesselatedBaseVertexCount, WaterQualitySettings.Instance.MaxTesselatedVertexCount));
		}

		internal void Awake(Water water)
		{
			this._Water = water;
		}

		internal void OnEnable()
		{
			this.OnValidate();
			this.UpdateVertexCount();
			this._RadialGrid.OnEnable(this._Water);
			this._ProjectionGrid.OnEnable(this._Water);
			this._UniformGrid.OnEnable(this._Water);
			this._CustomSurfaceMeshes.OnEnable(this._Water);
		}

		internal void OnDisable()
		{
			if (this._RadialGrid != null)
			{
				this._RadialGrid.OnDisable();
			}
			if (this._ProjectionGrid != null)
			{
				this._ProjectionGrid.OnDisable();
			}
			if (this._UniformGrid != null)
			{
				this._UniformGrid.OnDisable();
			}
			if (this._CustomSurfaceMeshes != null)
			{
				this._CustomSurfaceMeshes.OnDisable();
			}
		}

		internal void OnValidate()
		{
			if (this._RadialGrid == null)
			{
				this._RadialGrid = new WaterRadialGrid();
			}
			if (this._ProjectionGrid == null)
			{
				this._ProjectionGrid = new WaterProjectionGrid();
			}
			if (this._UniformGrid == null)
			{
				this._UniformGrid = new WaterUniformGrid();
			}
			if (this._CustomSurfaceMeshes == null)
			{
				this._CustomSurfaceMeshes = new WaterCustomSurfaceMeshes();
			}
			if (this._PreviousType != this._Type)
			{
				WaterGeometry.Type previousType = this._PreviousType;
				if (previousType != WaterGeometry.Type.RadialGrid)
				{
					if (previousType != WaterGeometry.Type.ProjectionGrid)
					{
						if (previousType == WaterGeometry.Type.UniformGrid)
						{
							this._UniformGrid.RemoveFromMaterial(this._Water);
						}
					}
					else
					{
						this._ProjectionGrid.RemoveFromMaterial(this._Water);
					}
				}
				else
				{
					this._RadialGrid.RemoveFromMaterial(this._Water);
				}
				WaterGeometry.Type type = this._Type;
				if (type != WaterGeometry.Type.RadialGrid)
				{
					if (type != WaterGeometry.Type.ProjectionGrid)
					{
						if (type == WaterGeometry.Type.UniformGrid)
						{
							this._UniformGrid.AddToMaterial(this._Water);
						}
					}
					else
					{
						this._ProjectionGrid.AddToMaterial(this._Water);
					}
				}
				else
				{
					this._RadialGrid.AddToMaterial(this._Water);
				}
				this._PreviousType = this._Type;
			}
			this.UpdateVertexCount();
			if (this._PreviousTargetVertexCount != this._ThisSystemVertexCount)
			{
				this._RadialGrid.Dispose();
				this._UniformGrid.Dispose();
				this._ProjectionGrid.Dispose();
				this._PreviousTargetVertexCount = this._ThisSystemVertexCount;
			}
		}

		internal void Update()
		{
			if (++this._FrameCount > 8)
			{
				this._FrameCount = 0;
			}
			int frameCount = this._FrameCount;
			if (frameCount != 0)
			{
				if (frameCount != 3)
				{
					if (frameCount == 6)
					{
						this._UniformGrid.Update();
					}
				}
				else
				{
					this._ProjectionGrid.Update();
				}
			}
			else
			{
				this._RadialGrid.Update();
			}
		}

		[SerializeField]
		[FormerlySerializedAs("type")]
		[Tooltip("Geometry type used for display.")]
		private WaterGeometry.Type _Type;

		[SerializeField]
		[FormerlySerializedAs("baseVertexCount")]
		[Tooltip("Water geometry vertex count at 1920x1080.")]
		private int _BaseVertexCount = 500000;

		[FormerlySerializedAs("tesselatedBaseVertexCount")]
		[SerializeField]
		[Tooltip("Water geometry vertex count at 1920x1080 on systems with tessellation support. Set it a bit lower as tessellation will place additional, better distributed vertices in shader.")]
		private int _TesselatedBaseVertexCount = 16000;

		[FormerlySerializedAs("adaptToResolution")]
		[SerializeField]
		private bool _AdaptToResolution = true;

		[FormerlySerializedAs("radialGrid")]
		[SerializeField]
		private WaterRadialGrid _RadialGrid;

		[FormerlySerializedAs("projectionGrid")]
		[SerializeField]
		private WaterProjectionGrid _ProjectionGrid;

		[FormerlySerializedAs("uniformGrid")]
		[SerializeField]
		private WaterUniformGrid _UniformGrid;

		[FormerlySerializedAs("customSurfaceMeshes")]
		[SerializeField]
		private WaterCustomSurfaceMeshes _CustomSurfaceMeshes;

		private Water _Water;

		private WaterGeometry.Type _PreviousType;

		private int _PreviousTargetVertexCount;

		private int _ThisSystemVertexCount;

		private int _FrameCount;

		private readonly WaterQuadGeometry _QuadGeometry = new WaterQuadGeometry();

		public enum Type
		{
			RadialGrid,
			ProjectionGrid,
			UniformGrid,
			CustomMeshes
		}
	}
}
