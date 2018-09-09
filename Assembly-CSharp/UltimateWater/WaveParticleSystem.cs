using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[RequireComponent(typeof(DynamicWater))]
	[AddComponentMenu("Water/Waves Particle System", 1)]
	public sealed class WaveParticleSystem : MonoBehaviour, IOverlaysRenderer
	{
		public WaveParticleSystem()
		{
			this._Plugins = new List<IWavesParticleSystemPlugin>();
		}

		public int ParticleCount
		{
			get
			{
				return this._Particles.Count;
			}
		}

		public float SimulationTime
		{
			get
			{
				return this._SimulationTime;
			}
		}

		public bool Spawn(WaveParticle particle, int clones, float waveShapeIrregularity, float centerElevation = 2f, float edgesElevation = 0.35f)
		{
			if (particle == null || this._Particles.FreeSpace < clones * 2 + 1)
			{
				return false;
			}
			particle.Group = new WaveParticlesGroup(this._SimulationTime);
			particle.BaseAmplitude *= this._Water.UniformWaterScale;
			particle.BaseFrequency /= this._Water.UniformWaterScale;
			WaveParticle waveParticle = null;
			float min = 1f / waveShapeIrregularity;
			for (int i = -clones; i <= clones; i++)
			{
				WaveParticle waveParticle2 = particle.Clone(particle.Position + new Vector2(particle.Direction.y, -particle.Direction.x) * ((float)i * 1.48f / particle.BaseFrequency));
				if (waveParticle2 != null)
				{
					waveParticle2.AmplitudeModifiers2 = UnityEngine.Random.Range(min, 1f) * (edgesElevation + (0.5f + Mathf.Cos(3.14159274f * (float)i / (float)clones) * 0.5f) * (centerElevation - edgesElevation));
					waveParticle2.LeftNeighbour = waveParticle;
					if (waveParticle != null)
					{
						waveParticle.RightNeighbour = waveParticle2;
						if (i == clones)
						{
							waveParticle2.DisallowSubdivision = true;
						}
					}
					else
					{
						waveParticle2.Group.LeftParticle = waveParticle2;
						waveParticle2.DisallowSubdivision = true;
					}
					if (!this._Particles.AddElement(waveParticle2))
					{
						return waveParticle != null;
					}
					waveParticle = waveParticle2;
				}
			}
			return true;
		}

		public void RenderOverlays(DynamicWaterCameraData overlays)
		{
		}

		public void RenderFoam(DynamicWaterCameraData overlays)
		{
			if (base.enabled)
			{
				this.RenderParticles(overlays);
			}
		}

		public void RegisterPlugin(IWavesParticleSystemPlugin plugin)
		{
			if (!this._Plugins.Contains(plugin))
			{
				this._Plugins.Add(plugin);
			}
		}

		public void UnregisterPlugin(IWavesParticleSystemPlugin plugin)
		{
			this._Plugins.Remove(plugin);
		}

		public bool AddParticle(WaveParticle particle)
		{
			if (particle == null)
			{
				return false;
			}
			if (particle.Group == null)
			{
				throw new ArgumentException("Particle has no group");
			}
			return this._Particles.AddElement(particle);
		}

		private void LateUpdate()
		{
			if (!this._Prewarmed)
			{
				this.Prewarm();
			}
			this.UpdateSimulation(Time.deltaTime);
		}

		private void OnValidate()
		{
			this._TimePerFrameExp = Mathf.Exp(this._TimePerFrame * 0.5f);
			if (this._WaterWavesParticlesShader == null)
			{
				this._WaterWavesParticlesShader = Shader.Find("UltimateWater/Particles/Particles");
			}
			if (this._Particles != null)
			{
				this._Particles.DebugMode = this._Water.ShaderSet.LocalEffectsDebug;
			}
		}

		private void Awake()
		{
			this._Water = base.GetComponent<Water>();
			this.OnValidate();
		}

		private void OnEnable()
		{
			this.CheckResources();
		}

		private void OnDisable()
		{
			this.FreeResources();
		}

		private void Prewarm()
		{
			this._Prewarmed = true;
			while (this._SimulationTime < this._PrewarmTime)
			{
				this.UpdateSimulationWithoutFrameBudget(0.1f);
			}
		}

		private void UpdateSimulation(float deltaTime)
		{
			this._SimulationTime += deltaTime;
			this.UpdatePlugins(deltaTime);
			this._Particles.UpdateSimulation(this._SimulationTime, this._TimePerFrameExp);
		}

		private void UpdateSimulationWithoutFrameBudget(float deltaTime)
		{
			this._SimulationTime += deltaTime;
			this.UpdatePlugins(deltaTime);
			this._Particles.UpdateSimulation(this._SimulationTime);
		}

		private void UpdatePlugins(float deltaTime)
		{
			int count = this._Plugins.Count;
			for (int i = 0; i < count; i++)
			{
				this._Plugins[i].UpdateParticles(this._SimulationTime, deltaTime);
			}
		}

		private void RenderParticles(DynamicWaterCameraData overlays)
		{
			Spray component = base.GetComponent<Spray>();
			if (component != null && component.ParticlesBuffer != null)
			{
				Graphics.SetRandomWriteTarget(3, component.ParticlesBuffer);
			}
			if (!this._Water.ShaderSet.LocalEffectsDebug)
			{
				Graphics.SetRenderTarget(new RenderBuffer[]
				{
					overlays.DynamicDisplacementMap.colorBuffer,
					overlays.NormalMap.colorBuffer
				}, overlays.DynamicDisplacementMap.depthBuffer);
			}
			else
			{
				Graphics.SetRenderTarget(new RenderBuffer[]
				{
					overlays.DynamicDisplacementMap.colorBuffer,
					overlays.NormalMap.colorBuffer,
					overlays.GetDebugMap(true).colorBuffer
				}, overlays.DynamicDisplacementMap.depthBuffer);
			}
			Shader.SetGlobalMatrix("_ParticlesVP", GL.GetGPUProjectionMatrix(overlays.Camera.PlaneProjectorCamera.projectionMatrix, true) * overlays.Camera.PlaneProjectorCamera.worldToCameraMatrix);
			Vector4 localMapsShaderCoords = overlays.Camera.LocalMapsShaderCoords;
			float uniformWaterScale = base.GetComponent<Water>().UniformWaterScale;
			this._WaterWavesParticlesMaterial.SetFloat("_WaterScale", uniformWaterScale);
			this._WaterWavesParticlesMaterial.SetVector("_LocalMapsCoords", localMapsShaderCoords);
			this._WaterWavesParticlesMaterial.SetPass((!this._Water.ShaderSet.LocalEffectsDebug) ? 0 : 1);
			this._Particles.Render(overlays.Camera.LocalMapsRect);
			Graphics.ClearRandomWriteTargets();
		}

		private void CheckResources()
		{
			if (this._WaterWavesParticlesMaterial == null)
			{
				this._WaterWavesParticlesMaterial = new Material(this._WaterWavesParticlesShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			if (this._Particles == null)
			{
				this._Particles = new WaveParticlesQuadtree(new Rect(-1000f, -1000f, 2000f, 2000f), this._MaxParticlesPerTile, this._MaxParticles)
				{
					DebugMode = this._Water.ShaderSet.LocalEffectsDebug
				};
			}
		}

		private void FreeResources()
		{
			if (this._WaterWavesParticlesMaterial != null)
			{
				this._WaterWavesParticlesMaterial.Destroy();
				this._WaterWavesParticlesMaterial = null;
			}
		}

		[FormerlySerializedAs("waterWavesParticlesShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _WaterWavesParticlesShader;

		[FormerlySerializedAs("maxParticles")]
		[SerializeField]
		private int _MaxParticles = 50000;

		[FormerlySerializedAs("maxParticlesPerTile")]
		[SerializeField]
		private int _MaxParticlesPerTile = 2000;

		[FormerlySerializedAs("prewarmTime")]
		[SerializeField]
		private float _PrewarmTime = 40f;

		[SerializeField]
		[Tooltip("Allowed execution time per frame.")]
		[FormerlySerializedAs("timePerFrame")]
		private float _TimePerFrame = 0.8f;

		private WaveParticlesQuadtree _Particles;

		private Water _Water;

		private Material _WaterWavesParticlesMaterial;

		private float _SimulationTime;

		private float _TimePerFrameExp;

		private bool _Prewarmed;

		private readonly List<IWavesParticleSystemPlugin> _Plugins;
	}
}
