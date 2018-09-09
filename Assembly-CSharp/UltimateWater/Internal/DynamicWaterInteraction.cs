using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater.Internal
{
	public sealed class DynamicWaterInteraction : MonoBehaviour, ILocalFoamRenderer, IDynamicWaterEffects
	{
		public void Enable()
		{
			throw new NotImplementedException();
		}

		public void Disable()
		{
			throw new NotImplementedException();
		}

		public void RenderLocalFoam(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			if (this._DetectContactArea)
			{
				Bounds bounds = this._MeshFilters[0].GetComponent<MeshRenderer>().bounds;
				for (int i = this._MeshFilters.Length - 1; i > 0; i--)
				{
					bounds.Encapsulate(this._MeshFilters[i].GetComponent<MeshRenderer>().bounds);
				}
				Vector3 center = bounds.center;
				Vector3 extents = bounds.extents;
				extents.x += this._FoamRange;
				extents.y += this._FoamRange;
				extents.z += this._FoamRange;
				center.y -= extents.y + 1f;
				commandBuffer.GetTemporaryRT(this._OcclusionMap2Hash, this._FoamOcclusionMapResolution, this._FoamOcclusionMapResolution, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				commandBuffer.SetRenderTarget(this._OcclusionMap2Hash);
				commandBuffer.ClearRenderTarget(false, true, new Color(0f, 0f, 0f, 0f));
				Camera effectsCamera = overlays.Camera.EffectsCamera;
				effectsCamera.transform.position = center;
				effectsCamera.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
				effectsCamera.orthographic = true;
				effectsCamera.orthographicSize = Mathf.Max(extents.x, extents.z);
				effectsCamera.nearClipPlane = 1f;
				effectsCamera.farClipPlane = extents.y * 2f + 10f;
				Matrix4x4 matrix4x = effectsCamera.projectionMatrix * effectsCamera.worldToCameraMatrix;
				commandBuffer.SetGlobalMatrix(this._OcclusionMapProjectionMatrixHash, matrix4x);
				commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, matrix4x);
				for (int j = this._MeshFilters.Length - 1; j >= 0; j--)
				{
					commandBuffer.DrawMesh(this._MeshFilters[j].sharedMesh, this._MeshFilters[j].transform.localToWorldMatrix, this._Material, 0, 1);
				}
				commandBuffer.GetTemporaryRT(this._OcclusionMapHash, this._FoamOcclusionMapResolution, this._FoamOcclusionMapResolution, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
				commandBuffer.Blit(this._OcclusionMap2Hash, this._OcclusionMapHash, this._Material, 2);
				commandBuffer.ReleaseTemporaryRT(this._OcclusionMap2Hash);
				commandBuffer.SetRenderTarget(overlays.FoamMap);
				Camera planeProjectorCamera = overlays.Camera.PlaneProjectorCamera;
				commandBuffer.SetViewProjectionMatrices(planeProjectorCamera.worldToCameraMatrix, planeProjectorCamera.projectionMatrix);
			}
			for (int k = this._MeshFilters.Length - 1; k >= 0; k--)
			{
				commandBuffer.DrawMesh(this._MeshFilters[k].sharedMesh, this._MeshFilters[k].transform.localToWorldMatrix, this._Material, 0, (!this._DetectContactArea) ? 3 : 0, overlays.DynamicWater.Water.Renderer.PropertyBlock);
			}
			if (this._DetectContactArea)
			{
				commandBuffer.ReleaseTemporaryRT(this._OcclusionMapHash);
			}
			if (this._Waves && this._WaveEmissionFrequency != 0f)
			{
				DynamicWaterInteraction._MatrixTemp[0] = base.transform.localToWorldMatrix;
				this._ObjectToWorld.SetData(DynamicWaterInteraction._MatrixTemp);
				this._ColliderInteractionShader.SetVector("_LocalMapsCoords", overlays.Camera.LocalMapsShaderCoords);
				this._ColliderInteractionShader.SetTexture(0, "TotalDisplacementMap", overlays.GetTotalDisplacementMap());
				this._ColliderInteractionShader.SetBuffer(0, "Vertices", this._ColliderVerticesBuffer);
				this._ColliderInteractionShader.SetBuffer(0, "Particles", this._Water.ParticlesBuffer);
				this._ColliderInteractionShader.SetBuffer(0, "ObjectToWorld", this._ObjectToWorld);
				this._ColliderInteractionShader.Dispatch(0, Mathf.CeilToInt((float)(this._ColliderVerticesBuffer.count >> 1) / 256f), 1, 1);
			}
		}

		private void Start()
		{
			this.OnValidate();
			if (this._MeshFilters == null || this._MeshFilters.Length == 0)
			{
				this._MeshFilters = base.GetComponentsInChildren<MeshFilter>(true);
			}
			this._Material = new Material(this._BaseShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._OcclusionMapHash = ShaderVariables.OcclusionMap;
			this._OcclusionMap2Hash = ShaderVariables.OcclusionMap2;
			this._OcclusionMapProjectionMatrixHash = ShaderVariables.OcclusionMapProjection;
			this.OnValidate();
			if (this._Waves && this._WaveEmissionFrequency != 0f)
			{
				this.CreateComputeBuffers();
			}
		}

		private void OnEnable()
		{
			if (this._Foam)
			{
				DynamicWater.AddRenderer<DynamicWaterInteraction>(this);
			}
		}

		private void OnDisable()
		{
			DynamicWater.RemoveRenderer<DynamicWaterInteraction>(this);
		}

		private void OnDestroy()
		{
			if (this._ColliderVerticesBuffer != null)
			{
				this._ColliderVerticesBuffer.Release();
				this._ColliderVerticesBuffer = null;
			}
			if (this._ObjectToWorld != null)
			{
				this._ObjectToWorld.Release();
				this._ObjectToWorld = null;
			}
		}

		private void OnValidate()
		{
			if (this._MaskDisplayShader == null)
			{
				this._MaskDisplayShader = Shader.Find("UltimateWater/Utility/ShorelineMaskRender");
			}
			if (this._BaseShader == null)
			{
				this._BaseShader = Shader.Find("UltimateWater/Utility/DynamicWaterInteraction");
			}
			if (this._Material != null)
			{
				this._Material.SetVector("_FoamIntensity", new Vector4(this._UniformFoamAmount, this._NoisyFoamAmount, this._FoamIntensity, 0f));
				this._Material.SetFloat("_FoamRange", this._FoamRange);
				this._Material.SetFloat("_FoamIntensityMaskTiling", this._FoamPatternTiling);
			}
		}

		private void CreateComputeBuffers()
		{
			MeshCollider component = base.GetComponent<MeshCollider>();
			Mesh sharedMesh = component.sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			Vector3[] normals = sharedMesh.normals;
			int[] indices = sharedMesh.GetIndices(0);
			this._ColliderVerticesBuffer = new ComputeBuffer(indices.Length * 2, 24, ComputeBufferType.Default);
			this._ObjectToWorld = new ComputeBuffer(1, 64, ComputeBufferType.Default);
			DynamicWaterInteraction.VertexData[] array = new DynamicWaterInteraction.VertexData[indices.Length * 2];
			int num = 0;
			for (int i = 0; i < indices.Length; i++)
			{
				int num2 = indices[i];
				int num3 = indices[(i % 3 != 0) ? (i - 1) : (i + 2)];
				array[num++] = new DynamicWaterInteraction.VertexData
				{
					Position = vertices[num3],
					Normal = normals[num3]
				};
				array[num++] = new DynamicWaterInteraction.VertexData
				{
					Position = vertices[num2],
					Normal = normals[num2]
				};
			}
			this._ColliderVerticesBuffer.SetData(array);
		}

		[FormerlySerializedAs("colliderInteractionShader")]
		[SerializeField]
		[HideInInspector]
		private ComputeShader _ColliderInteractionShader;

		[FormerlySerializedAs("maskDisplayShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _MaskDisplayShader;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("baseShader")]
		private Shader _BaseShader;

		[FormerlySerializedAs("foam")]
		[SerializeField]
		[Header("Contact Foam")]
		private bool _Foam = true;

		[FormerlySerializedAs("foamPatternTiling")]
		[SerializeField]
		private float _FoamPatternTiling = 1f;

		[FormerlySerializedAs("foamRange")]
		[SerializeField]
		private float _FoamRange = 1.6f;

		[SerializeField]
		[FormerlySerializedAs("uniformFoamAmount")]
		private float _UniformFoamAmount = 30.5f;

		[SerializeField]
		[FormerlySerializedAs("noisyFoamAmount")]
		private float _NoisyFoamAmount = 30.5f;

		[FormerlySerializedAs("foamIntensity")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _FoamIntensity = 0.45f;

		[FormerlySerializedAs("detectContactArea")]
		[SerializeField]
		private bool _DetectContactArea;

		[FormerlySerializedAs("foamOcclusionMapResolution")]
		[SerializeField]
		private int _FoamOcclusionMapResolution = 128;

		private int _OcclusionMap2Hash;

		private int _OcclusionMapProjectionMatrixHash;

		[SerializeField]
		[FormerlySerializedAs("meshFilters")]
		private MeshFilter[] _MeshFilters;

		[FormerlySerializedAs("waves")]
		[SerializeField]
		[Header("Waves")]
		private bool _Waves = true;

		[FormerlySerializedAs("water")]
		[SerializeField]
		private WaveParticlesSystemGPU _Water;

		[Range(0f, 4f)]
		[SerializeField]
		[FormerlySerializedAs("waveEmissionFrequency")]
		private float _WaveEmissionFrequency = 1f;

		private Material _Material;

		private int _OcclusionMapHash;

		private ComputeBuffer _ColliderVerticesBuffer;

		private ComputeBuffer _ObjectToWorld;

		private static readonly Matrix4x4[] _MatrixTemp = new Matrix4x4[1];

		private struct VertexData
		{
			public Vector3 Position;

			public Vector3 Normal;
		}
	}
}
