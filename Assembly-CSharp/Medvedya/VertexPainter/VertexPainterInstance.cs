using System;
using UnityEngine;

namespace Medvedya.VertexPainter
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Vertex painter/Vertex painter instance")]
	[RequireComponent(typeof(MeshFilter))]
	[ExecuteInEditMode]
	public class VertexPainterInstance : MonoBehaviour, IPainting
	{
		public MeshFilter meshFilter
		{
			get
			{
				if (this._meshFilter == null)
				{
					this._meshFilter = base.GetComponent<MeshFilter>();
				}
				return this._meshFilter;
			}
		}

		public VertexPainter vertexPainter
		{
			get
			{
				return this._vertexPainter;
			}
			set
			{
				if (value == this._vertexPainter)
				{
					return;
				}
				this._vertexPainter = value;
				MeshRenderer component = base.GetComponent<MeshRenderer>();
				component.sharedMaterials = this.vertexPainter.GetComponent<MeshRenderer>().sharedMaterials;
				this.Init();
			}
		}

		public ModifyInfo modifyInfo
		{
			get
			{
				return this._modifyInfo;
			}
		}

		private void OnDrawGizmos()
		{
			this.wireMesh.Do(this);
		}

		private void OnValidate()
		{
			this.Init();
		}

		private void Awake()
		{
			this.Init();
		}

		private void Init()
		{
			if (this._vertexPainter != null)
			{
				this._vertexPainter.Init();
				this.meshFilter.sharedMesh = this._vertexPainter.meshFilter.sharedMesh;
				this._modifyInfo = new ModifyInfo(this._vertexPainter, base.transform);
				if (this._vertexPainter.referensMesh != null)
				{
					MeshCollider component = base.GetComponent<MeshCollider>();
					if (component != null)
					{
						component.sharedMesh = this._vertexPainter.referensMesh;
					}
				}
			}
		}

		[SerializeField]
		private MeshFilter _meshFilter;

		[SerializeField]
		private ModifyInfo _modifyInfo;

		[SerializeField]
		private VertexPainter _vertexPainter;

		public WireMesh wireMesh = new WireMesh();
	}
}
