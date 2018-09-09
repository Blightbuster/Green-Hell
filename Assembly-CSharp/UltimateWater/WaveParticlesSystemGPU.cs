using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WaveParticlesSystemGPU : MonoBehaviour, IOverlaysRenderer
	{
		public ComputeBuffer ParticlesBuffer
		{
			get
			{
				return this._ParticlesA;
			}
		}

		public int FoamAtlasWidth
		{
			get
			{
				return this._FoamAtlasWidth;
			}
		}

		public int FoamAtlasHeight
		{
			get
			{
				return this._FoamAtlasHeight;
			}
		}

		public void EmitParticle(WaveParticlesSystemGPU.ParticleData particleData)
		{
			if ((ulong)this._ParticlesToSpawnCount == (ulong)((long)this._ParticlesToSpawn.Length))
			{
				return;
			}
			this._ParticlesToSpawn[(int)((UIntPtr)(this._ParticlesToSpawnCount++))] = particleData;
		}

		public void RenderOverlays(DynamicWaterCameraData overlays)
		{
			Spray component = base.GetComponent<Spray>();
			if (component != null && component.ParticlesBuffer != null)
			{
				Graphics.SetRandomWriteTarget(3, component.ParticlesBuffer);
			}
			this._RenderBuffers[0] = overlays.DynamicDisplacementMap.colorBuffer;
			this._RenderBuffers[1] = overlays.NormalMap.colorBuffer;
			this._ParticlesRenderMaterial.SetBuffer("_Particles", this._ParticlesA);
			this._ParticlesRenderMaterial.SetMatrix("_ParticlesVP", GL.GetGPUProjectionMatrix(overlays.Camera.PlaneProjectorCamera.projectionMatrix, true) * overlays.Camera.PlaneProjectorCamera.worldToCameraMatrix);
			if (this._ParticlesRenderMaterial.SetPass(0))
			{
				Graphics.SetRenderTarget(this._RenderBuffers, overlays.DynamicDisplacementMap.depthBuffer);
				Graphics.DrawProceduralIndirect(MeshTopology.Points, this._ParticlesRenderInfo);
				Graphics.ClearRandomWriteTargets();
			}
			if (this._ParticlesRenderMaterial.SetPass(2))
			{
				Graphics.SetRenderTarget(overlays.DisplacementsMask);
				Graphics.DrawProceduralIndirect(MeshTopology.Points, this._ParticlesRenderInfo);
			}
			Graphics.SetRenderTarget(null);
		}

		public void RenderFoam(DynamicWaterCameraData overlays)
		{
			if (this._ParticlesRenderMaterial.SetPass(1))
			{
				Graphics.SetRenderTarget(overlays.FoamMap);
				Graphics.DrawProceduralIndirect(MeshTopology.Points, this._ParticlesRenderInfo);
			}
			if (this._ParticlesRenderMaterial.SetPass(3))
			{
				Graphics.DrawProceduralIndirect(MeshTopology.Points, this._ParticlesRenderInfo);
			}
		}

		private void OnValidate()
		{
			if (this._ParticlesRenderShader == null)
			{
				this._ParticlesRenderShader = Shader.Find("UltimateWater/Particles/GPU_Render");
			}
			if (this._ParticlesRenderMaterial != null)
			{
				this.SetMaterialProperties();
			}
		}

		private void Update()
		{
			this.CheckResources();
			this.UpdateParticles();
			this.SpawnParticles();
			this.SwapBuffers();
			ComputeBuffer.CopyCount(this._ParticlesA, this._ParticlesRenderInfo, 0);
		}

		private void Start()
		{
			this._Water = base.GetComponentInParent<Water>();
			this.OnValidate();
			this._ParticlesRenderMaterial = new Material(this._ParticlesRenderShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this.SetMaterialProperties();
			this._LastSurfaceOffset = this._Water.SurfaceOffset;
		}

		private void OnDestroy()
		{
			if (this._ParticlesA != null)
			{
				this._ParticlesA.Release();
				this._ParticlesA = null;
			}
			if (this._ParticlesB != null)
			{
				this._ParticlesB.Release();
				this._ParticlesB = null;
			}
			if (this._ParticlesRenderInfo != null)
			{
				this._ParticlesRenderInfo.Release();
				this._ParticlesRenderInfo = null;
			}
			if (this._ParticlesUpdateInfo != null)
			{
				this._ParticlesUpdateInfo.Release();
				this._ParticlesUpdateInfo = null;
			}
			if (this._SpawnBuffer != null)
			{
				this._SpawnBuffer.Release();
				this._SpawnBuffer = null;
			}
		}

		private void UpdateParticles()
		{
			this._ParticlesB.SetCounterValue(0u);
			Vector2 surfaceOffset = this._Water.SurfaceOffset;
			ComputeBuffer.CopyCount(this._ParticlesA, this._ParticlesUpdateInfo, 0);
			this._ControllerShader.SetFloat("deltaTime", Time.deltaTime);
			this._ControllerShader.SetVector("surfaceOffsetDelta", new Vector4(this._LastSurfaceOffset.x - surfaceOffset.x, this._LastSurfaceOffset.y - surfaceOffset.y, 0f, 0f));
			this._ControllerShader.SetBuffer(0, "Particles", this._ParticlesB);
			this._ControllerShader.SetBuffer(0, "SourceParticles", this._ParticlesA);
			this._ControllerShader.DispatchIndirect(0, this._ParticlesUpdateInfo);
			this._LastSurfaceOffset = surfaceOffset;
		}

		private void SpawnParticles()
		{
			if (this._ParticlesToSpawnCount == 0u)
			{
				return;
			}
			this._SpawnBuffer.SetData(this._ParticlesToSpawn);
			this._ControllerShader.SetBuffer(1, "Particles", this._ParticlesB);
			this._ControllerShader.SetBuffer(1, "SpawnedParticles", this._SpawnBuffer);
			this._ControllerShader.Dispatch(1, 1, 1, 1);
			int num = 0;
			while ((long)num < (long)((ulong)this._ParticlesToSpawnCount))
			{
				this._ParticlesToSpawn[num].Lifetime = 0f;
				num++;
			}
			this._ParticlesToSpawnCount = 0u;
		}

		private void SwapBuffers()
		{
			ComputeBuffer particlesA = this._ParticlesA;
			this._ParticlesA = this._ParticlesB;
			this._ParticlesB = particlesA;
		}

		private void CheckResources()
		{
			if (this._ParticlesA == null)
			{
				this._ParticlesA = new ComputeBuffer(this._MaxParticles, 48, ComputeBufferType.Append);
				this._ParticlesA.SetCounterValue(0u);
				this._ParticlesB = new ComputeBuffer(this._MaxParticles, 48, ComputeBufferType.Append);
				this._ParticlesB.SetCounterValue(0u);
				this._SpawnBuffer = new ComputeBuffer(16, 48, ComputeBufferType.Default);
			}
			if (this._ParticlesRenderInfo == null)
			{
				this._ParticlesRenderInfo = new ComputeBuffer(1, 16, ComputeBufferType.DrawIndirect);
				this._ParticlesRenderInfo.SetData(new int[]
				{
					0,
					1,
					0,
					0
				});
			}
			if (this._ParticlesUpdateInfo == null)
			{
				this._ParticlesUpdateInfo = new ComputeBuffer(1, 12, ComputeBufferType.DrawIndirect);
				this._ParticlesUpdateInfo.SetData(new int[]
				{
					0,
					1,
					1
				});
			}
		}

		private void SetMaterialProperties()
		{
			this._ParticlesRenderMaterial.SetVector("_FoamAtlasParams", new Vector4(1f / (float)this._FoamAtlasWidth, 1f / (float)this._FoamAtlasHeight, 0f, 0f));
			this._ParticlesRenderMaterial.SetTexture("_FoamAtlas", this._FoamTexture);
			this._ParticlesRenderMaterial.SetTexture("_FoamOverlayTexture", this._FoamOverlayTexture);
		}

		[SerializeField]
		[FormerlySerializedAs("maxParticles")]
		private int _MaxParticles = 80000;

		[FormerlySerializedAs("controllerShader")]
		[SerializeField]
		private ComputeShader _ControllerShader;

		[FormerlySerializedAs("particlesRenderShader")]
		[SerializeField]
		private Shader _ParticlesRenderShader;

		[FormerlySerializedAs("foamTexture")]
		[SerializeField]
		private Texture _FoamTexture;

		[SerializeField]
		[FormerlySerializedAs("foamOverlayTexture")]
		private Texture _FoamOverlayTexture;

		[FormerlySerializedAs("foamAtlasWidth")]
		[SerializeField]
		private int _FoamAtlasWidth = 8;

		[FormerlySerializedAs("foamAtlasHeight")]
		[SerializeField]
		private int _FoamAtlasHeight = 4;

		private Material _ParticlesRenderMaterial;

		private ComputeBuffer _ParticlesA;

		private ComputeBuffer _ParticlesB;

		private ComputeBuffer _SpawnBuffer;

		private ComputeBuffer _ParticlesRenderInfo;

		private ComputeBuffer _ParticlesUpdateInfo;

		private Vector2 _LastSurfaceOffset;

		private uint _ParticlesToSpawnCount;

		private Water _Water;

		private readonly WaveParticlesSystemGPU.ParticleData[] _ParticlesToSpawn = new WaveParticlesSystemGPU.ParticleData[16];

		private readonly RenderBuffer[] _RenderBuffers = new RenderBuffer[2];

		public struct ParticleData
		{
			public Vector2 Position;

			public Vector2 Direction;

			public float Wavelength;

			public float Amplitude;

			public float InitialLifetime;

			public float Lifetime;

			public float UvOffsetPack;

			public float Foam;

			public float TrailCalming;

			public float TrailFoam;
		}
	}
}
