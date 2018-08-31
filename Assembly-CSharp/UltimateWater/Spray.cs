using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Spray", 1)]
	[RequireComponent(typeof(Water))]
	public sealed class Spray : MonoBehaviour, IOverlaysRenderer
	{
		public int MaxParticles
		{
			get
			{
				return this._MaxParticles;
			}
		}

		public int SpawnedParticles
		{
			get
			{
				if (this._ParticlesA != null)
				{
					ComputeBuffer.CopyCount(this._ParticlesA, this._ParticlesInfo, 0);
					this._ParticlesInfo.GetData(this._CountBuffer);
					return this._CountBuffer[0];
				}
				return 0;
			}
		}

		public ComputeBuffer ParticlesBuffer
		{
			get
			{
				return this._ParticlesA;
			}
		}

		public void SpawnCustomParticle(Spray.Particle particle)
		{
			if (!base.enabled)
			{
				return;
			}
			if (this._ParticlesToSpawn.Length <= this._NumParticlesToSpawn)
			{
				Array.Resize<Spray.Particle>(ref this._ParticlesToSpawn, this._ParticlesToSpawn.Length << 1);
			}
			this._ParticlesToSpawn[this._NumParticlesToSpawn] = particle;
			this._NumParticlesToSpawn++;
		}

		public void SpawnCustomParticles(Spray.Particle[] particles, int numParticles)
		{
			if (!base.enabled)
			{
				return;
			}
			this.CheckResources();
			if (this._SpawnBuffer == null || this._SpawnBuffer.count < particles.Length)
			{
				if (this._SpawnBuffer != null)
				{
					this._SpawnBuffer.Release();
				}
				this._SpawnBuffer = new ComputeBuffer(particles.Length, 40);
			}
			this._SpawnBuffer.SetData(particles);
			this._SprayControllerShader.SetFloat("particleCount", (float)numParticles);
			this._SprayControllerShader.SetBuffer(2, "SourceParticles", this._SpawnBuffer);
			this._SprayControllerShader.SetBuffer(2, "TargetParticles", this._ParticlesA);
			this._SprayControllerShader.Dispatch(2, 1, 1, 1);
		}

		public void RenderOverlays(DynamicWaterCameraData overlays)
		{
		}

		public void RenderFoam(DynamicWaterCameraData overlays)
		{
			if (!base.enabled)
			{
				return;
			}
			this.CheckResources();
			if (this._SprayToFoam)
			{
				this.GenerateLocalFoam(overlays);
			}
		}

		private void Start()
		{
			this._Water = base.GetComponent<Water>();
			this._WindWaves = this._Water.WindWaves;
			this._Overlays = this._Water.DynamicWater;
			this._WindWaves.ResolutionChanged.AddListener(new UnityAction<WindWaves>(this.OnResolutionChanged));
			this._Supported = this.CheckSupport();
			this._LastSurfaceOffset = this._Water.SurfaceOffset;
			if (!this._Supported)
			{
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			this._Water = base.GetComponent<Water>();
			this._Water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			this.OnProfilesChanged(this._Water);
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraPreCull));
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraPreCull));
		}

		private void OnDisable()
		{
			if (this._Water != null)
			{
				this._Water.ProfilesManager.Changed.RemoveListener(new UnityAction<Water>(this.OnProfilesChanged));
			}
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraPreCull));
			this.Dispose();
		}

		private void LateUpdate()
		{
			if (Time.frameCount < 10)
			{
				return;
			}
			if (!this._ResourcesReady)
			{
				this.CheckResources();
			}
			this.SwapParticleBuffers();
			this.ClearParticles();
			this.UpdateParticles();
			if (Camera.main != null)
			{
				this.SpawnWindWavesParticlesTiled(Camera.main.transform);
			}
			if (this._NumParticlesToSpawn != 0)
			{
				this.SpawnCustomParticles(this._ParticlesToSpawn, this._NumParticlesToSpawn);
				this._NumParticlesToSpawn = 0;
			}
		}

		private void OnValidate()
		{
			this._MaxParticles = Mathf.RoundToInt((float)this._MaxParticles / 65535f) * 65535;
			if (this._SprayTiledGeneratorShader == null)
			{
				this._SprayTiledGeneratorShader = Shader.Find("UltimateWater/Spray/Generator (Tiles)");
			}
			if (this._SprayLocalGeneratorShader == null)
			{
				this._SprayLocalGeneratorShader = Shader.Find("UltimateWater/Spray/Generator (Local)");
			}
			if (this._SprayToFoamShader == null)
			{
				this._SprayToFoamShader = Shader.Find("UltimateWater/Spray/Spray To Foam");
			}
			this.UpdatePrecomputedParams();
		}

		private void OnSomeCameraPreCull(Camera cameraComponent)
		{
			if (!this._ResourcesReady)
			{
				return;
			}
			WaterCamera waterCamera = WaterCamera.GetWaterCamera(cameraComponent, false);
			if (waterCamera != null && waterCamera.Type == WaterCamera.CameraType.Normal)
			{
				this._SprayMaterial.SetBuffer("_Particles", this._ParticlesA);
				this._SprayMaterial.SetVector("_CameraUp", cameraComponent.transform.up);
				this._SprayMaterial.SetVector("_WrapSubsurfaceScatteringPack", this._Water.Renderer.PropertyBlock.GetVector("_WrapSubsurfaceScatteringPack"));
				this._SprayMaterial.SetFloat("_UniformWaterScale", this._Water.UniformWaterScale);
				if (this._ProbeAnchor == null)
				{
					GameObject gameObject = new GameObject("Spray Probe Anchor")
					{
						hideFlags = HideFlags.HideAndDontSave
					};
					this._ProbeAnchor = gameObject.transform;
				}
				this._ProbeAnchor.position = cameraComponent.transform.position;
				int num = this._PropertyBlocks.Length;
				for (int i = 0; i < num; i++)
				{
					Graphics.DrawMesh(this._Mesh, Matrix4x4.identity, this._SprayMaterial, 0, cameraComponent, 0, this._PropertyBlocks[i], ShadowCastingMode.Off, false, this._ProbeAnchor);
				}
			}
		}

		private void SpawnWindWavesParticlesTiled(Transform origin)
		{
			Vector3 position = origin.position;
			float num = 400f / (float)this._BlankOutput.width;
			this._SprayTiledGeneratorMaterial.CopyPropertiesFromMaterial(this._Water.Materials.SurfaceMaterial);
			this._SprayTiledGeneratorMaterial.SetVector("_SurfaceOffset", new Vector3(this._Water.SurfaceOffset.x, this._Water.transform.position.y, this._Water.SurfaceOffset.y));
			this._SprayTiledGeneratorMaterial.SetVector("_Params", new Vector4(this._SpawnThreshold * 0.25835f, this._SkipRatioPrecomp, 0f, this._Scale * 0.455f));
			this._SprayTiledGeneratorMaterial.SetVector("_Coordinates", new Vector4(position.x - 200f + UnityEngine.Random.value * num, position.z - 200f + UnityEngine.Random.value * num, 400f, 400f));
			if (this._Overlays == null)
			{
				this._SprayTiledGeneratorMaterial.SetTexture("_LocalNormalMap", this.GetBlankWhiteTex());
			}
			Graphics.SetRandomWriteTarget(1, this._ParticlesA);
			GraphicsUtilities.Blit(null, this._BlankOutput, this._SprayTiledGeneratorMaterial, 0, this._Water.Renderer.PropertyBlock);
			Graphics.ClearRandomWriteTargets();
		}

		private void GenerateLocalFoam(DynamicWaterCameraData data)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
			Graphics.SetRenderTarget(temporary);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			this._SprayToFoamMaterial.SetBuffer("_Particles", this._ParticlesA);
			this._SprayToFoamMaterial.SetVector("_LocalMapsCoords", data.Camera.LocalMapsShaderCoords);
			this._SprayToFoamMaterial.SetFloat("_UniformWaterScale", 50f * this._Water.UniformWaterScale / data.Camera.LocalMapsRect.width);
			Vector4 vector = this._SprayMaterial.GetVector("_ParticleParams");
			vector.x *= 8f;
			vector.z = 1f;
			this._SprayToFoamMaterial.SetVector("_ParticleParams", vector);
			int num = this._PropertyBlocks.Length;
			for (int i = 0; i < num; i++)
			{
				this._SprayToFoamMaterial.SetFloat("_ParticleOffset", (float)(i * 65535));
				if (this._SprayToFoamMaterial.SetPass(0))
				{
					Graphics.DrawMeshNow(this._Mesh, Matrix4x4.identity, 0);
				}
			}
			Camera planeProjectorCamera = data.Camera.PlaneProjectorCamera;
			Rect localMapsRect = data.Camera.LocalMapsRect;
			Vector2 center = localMapsRect.center;
			float num2 = localMapsRect.width * 0.5f;
			Matrix4x4 matrix = new Matrix4x4
			{
				m03 = center.x,
				m13 = this._Water.transform.position.y,
				m23 = center.y,
				m00 = num2,
				m11 = num2,
				m22 = num2,
				m33 = 1f
			};
			GL.PushMatrix();
			GL.modelview = planeProjectorCamera.worldToCameraMatrix;
			GL.LoadProjectionMatrix(planeProjectorCamera.projectionMatrix);
			Graphics.SetRenderTarget(data.FoamMap);
			this._SprayToFoamMaterial.mainTexture = temporary;
			if (this._SprayToFoamMaterial.SetPass(1))
			{
				Graphics.DrawMeshNow(Quads.BipolarXZ, matrix, 0);
			}
			GL.PopMatrix();
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void UpdateParticles()
		{
			Vector2 vector = this._WindWaves.WindSpeed * 0.0008f;
			Vector3 gravity = Physics.gravity;
			float deltaTime = Time.deltaTime;
			if (this._Overlays != null)
			{
				DynamicWaterCameraData cameraOverlaysData = this._Overlays.GetCameraOverlaysData(Camera.main, false);
				if (cameraOverlaysData != null)
				{
					this._SprayControllerShader.SetTexture(0, "TotalDisplacementMap", cameraOverlaysData.TotalDisplacementMap);
					WaterCamera waterCamera = WaterCamera.GetWaterCamera(Camera.main, false);
					if (waterCamera != null)
					{
						this._SprayControllerShader.SetVector("localMapsCoords", waterCamera.LocalMapsShaderCoords);
					}
				}
				else
				{
					this._SprayControllerShader.SetTexture(0, "TotalDisplacementMap", this.GetBlankWhiteTex());
				}
			}
			else
			{
				this._SprayControllerShader.SetTexture(0, "TotalDisplacementMap", this.GetBlankWhiteTex());
			}
			Vector2 surfaceOffset = this._Water.SurfaceOffset;
			this._SprayControllerShader.SetVector("deltaTime", new Vector4(deltaTime, 1f - deltaTime * 0.2f, 0f, 0f));
			this._SprayControllerShader.SetVector("externalForces", new Vector3((vector.x + gravity.x) * deltaTime, gravity.y * deltaTime, (vector.y + gravity.z) * deltaTime));
			this._SprayControllerShader.SetVector("surfaceOffsetDelta", new Vector3(this._LastSurfaceOffset.x - surfaceOffset.x, 0f, this._LastSurfaceOffset.y - surfaceOffset.y));
			this._SprayControllerShader.SetFloat("surfaceOffsetY", base.transform.position.y);
			this._SprayControllerShader.SetVector("waterTileSizesInv", this._WindWaves.TileSizesInv);
			this._SprayControllerShader.SetBuffer(0, "SourceParticles", this._ParticlesB);
			this._SprayControllerShader.SetBuffer(0, "TargetParticles", this._ParticlesA);
			this._SprayControllerShader.Dispatch(0, this._MaxParticles / 256, 1, 1);
			this._LastSurfaceOffset = surfaceOffset;
		}

		private Texture2D GetBlankWhiteTex()
		{
			if (this._BlankWhiteTex == null)
			{
				this._BlankWhiteTex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						this._BlankWhiteTex.SetPixel(i, j, new Color(1f, 1f, 1f, 1f));
					}
				}
				this._BlankWhiteTex.Apply(false, true);
			}
			return this._BlankWhiteTex;
		}

		private void ClearParticles()
		{
			this._SprayControllerShader.SetBuffer(1, "TargetParticlesFlat", this._ParticlesA);
			this._SprayControllerShader.Dispatch(1, this._MaxParticles / 256, 1, 1);
		}

		private void SwapParticleBuffers()
		{
			ComputeBuffer particlesB = this._ParticlesB;
			this._ParticlesB = this._ParticlesA;
			this._ParticlesA = particlesB;
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			if (this._BlankOutput != null)
			{
				UnityEngine.Object.Destroy(this._BlankOutput);
				this._BlankOutput = null;
			}
			this._ResourcesReady = false;
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			this._SpawnThreshold = 0f;
			this._SpawnSkipRatio = 0f;
			this._Scale = 0f;
			if (profiles != null)
			{
				foreach (Water.WeightedProfile weightedProfile in profiles)
				{
					WaterProfileData profile = weightedProfile.Profile;
					float weight = weightedProfile.Weight;
					this._SpawnThreshold += profile.SprayThreshold * weight;
					this._SpawnSkipRatio += profile.SpraySkipRatio * weight;
					this._Scale += profile.SpraySize * weight;
				}
			}
		}

		private void UpdatePrecomputedParams()
		{
			if (this._Water != null)
			{
				this._Resolution = this._WindWaves.FinalResolution;
			}
			this._SkipRatioPrecomp = Mathf.Pow(this._SpawnSkipRatio, 1024f / (float)this._Resolution);
		}

		private bool CheckSupport()
		{
			return SystemInfo.supportsComputeShaders && this._SprayTiledGeneratorShader != null && this._SprayTiledGeneratorShader.isSupported;
		}

		private void CheckResources()
		{
			if (this._SprayTiledGeneratorMaterial == null)
			{
				this._SprayTiledGeneratorMaterial = new Material(this._SprayTiledGeneratorShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			if (this._SprayLocalGeneratorMaterial == null)
			{
				this._SprayLocalGeneratorMaterial = new Material(this._SprayLocalGeneratorShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			if (this._SprayToFoamMaterial == null)
			{
				this._SprayToFoamMaterial = new Material(this._SprayToFoamShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			if (this._BlankOutput == null)
			{
				this.UpdatePrecomputedParams();
				this._BlankOutput = new RenderTexture(this._Resolution, this._Resolution, 0, (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] Spray - WaterSpray Blank Output Texture",
					filterMode = FilterMode.Point
				};
				this._BlankOutput.Create();
			}
			if (this._Mesh == null)
			{
				int num = Mathf.Min(this._MaxParticles, 65535);
				this._Mesh = new Mesh
				{
					name = "Spray",
					hideFlags = HideFlags.DontSave,
					vertices = new Vector3[num]
				};
				int[] array = new int[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = i;
				}
				this._Mesh.SetIndices(array, MeshTopology.Points, 0);
				this._Mesh.bounds = new Bounds(Vector3.zero, new Vector3(1E+07f, 1E+07f, 1E+07f));
			}
			if (this._PropertyBlocks == null)
			{
				int num2 = Mathf.CeilToInt((float)this._MaxParticles / 65535f);
				this._PropertyBlocks = new MaterialPropertyBlock[num2];
				for (int j = 0; j < num2; j++)
				{
					MaterialPropertyBlock materialPropertyBlock = this._PropertyBlocks[j] = new MaterialPropertyBlock();
					materialPropertyBlock.SetFloat("_ParticleOffset", (float)(j * 65535));
				}
			}
			if (this._ParticlesA == null)
			{
				this._ParticlesA = new ComputeBuffer(this._MaxParticles, 40, ComputeBufferType.Append);
			}
			if (this._ParticlesB == null)
			{
				this._ParticlesB = new ComputeBuffer(this._MaxParticles, 40, ComputeBufferType.Append);
			}
			if (this._ParticlesInfo == null)
			{
				this._ParticlesInfo = new ComputeBuffer(1, 16, ComputeBufferType.DrawIndirect);
				this._ParticlesInfo.SetData(new int[]
				{
					0,
					1,
					0,
					0
				});
			}
			this._ResourcesReady = true;
		}

		private void Dispose()
		{
			if (this._BlankOutput != null)
			{
				UnityEngine.Object.Destroy(this._BlankOutput);
				this._BlankOutput = null;
			}
			if (this._ParticlesA != null)
			{
				this._ParticlesA.Dispose();
				this._ParticlesA = null;
			}
			if (this._ParticlesB != null)
			{
				this._ParticlesB.Dispose();
				this._ParticlesB = null;
			}
			if (this._ParticlesInfo != null)
			{
				this._ParticlesInfo.Release();
				this._ParticlesInfo = null;
			}
			if (this._Mesh != null)
			{
				UnityEngine.Object.Destroy(this._Mesh);
				this._Mesh = null;
			}
			if (this._ProbeAnchor != null)
			{
				UnityEngine.Object.Destroy(this._ProbeAnchor.gameObject);
				this._ProbeAnchor = null;
			}
			if (this._SpawnBuffer != null)
			{
				this._SpawnBuffer.Release();
				this._SpawnBuffer = null;
			}
			this._ResourcesReady = false;
		}

		[HideInInspector]
		[FormerlySerializedAs("sprayTiledGeneratorShader")]
		[SerializeField]
		private Shader _SprayTiledGeneratorShader;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("sprayLocalGeneratorShader")]
		private Shader _SprayLocalGeneratorShader;

		[FormerlySerializedAs("sprayToFoamShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _SprayToFoamShader;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("sprayControllerShader")]
		private ComputeShader _SprayControllerShader;

		[FormerlySerializedAs("sprayMaterial")]
		[SerializeField]
		private Material _SprayMaterial;

		[Range(16f, 327675f)]
		[SerializeField]
		[FormerlySerializedAs("maxParticles")]
		private int _MaxParticles = 65535;

		[SerializeField]
		[FormerlySerializedAs("sprayToFoam")]
		private bool _SprayToFoam = true;

		private float _SpawnThreshold = 1f;

		private float _SpawnSkipRatio = 0.9f;

		private float _Scale = 1f;

		private Water _Water;

		private WindWaves _WindWaves;

		private DynamicWater _Overlays;

		private Material _SprayTiledGeneratorMaterial;

		private Material _SprayLocalGeneratorMaterial;

		private Material _SprayToFoamMaterial;

		private Transform _ProbeAnchor;

		private RenderTexture _BlankOutput;

		private Texture2D _BlankWhiteTex;

		private ComputeBuffer _ParticlesA;

		private ComputeBuffer _ParticlesB;

		private ComputeBuffer _ParticlesInfo;

		private ComputeBuffer _SpawnBuffer;

		private int _Resolution;

		private Mesh _Mesh;

		private bool _Supported;

		private bool _ResourcesReady;

		private Vector2 _LastSurfaceOffset;

		private readonly int[] _CountBuffer = new int[4];

		private float _SkipRatioPrecomp;

		private Spray.Particle[] _ParticlesToSpawn = new Spray.Particle[10];

		private int _NumParticlesToSpawn;

		private MaterialPropertyBlock[] _PropertyBlocks;

		public struct Particle
		{
			public Particle(Vector3 position, Vector3 velocity, float lifetime, float offset, float maxIntensity)
			{
				this.Position = position;
				this.Velocity = velocity;
				this.Lifetime = new Vector2(lifetime, lifetime);
				this.Offset = offset;
				this.MaxIntensity = maxIntensity;
			}

			public Vector3 Position;

			public Vector3 Velocity;

			public Vector2 Lifetime;

			public float Offset;

			public float MaxIntensity;
		}
	}
}
