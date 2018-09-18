using System;
using System.Collections.Generic;
using UltimateWater.Utils;
using UnityEngine;

namespace UltimateWater
{
	[RequireComponent(typeof(ParticleSystem))]
	[AddComponentMenu("Ultimate Water/Dynamic/Water Particles")]
	public class WaterParticleDisplacement : MonoBehaviour
	{
		public bool Initialized
		{
			get
			{
				return this.Water != null;
			}
		}

		public void Initialize(Water water)
		{
			if (this.Initialized)
			{
				return;
			}
			this.Water = water;
			this.Start();
		}

		private void Awake()
		{
			this.Water = Utilities.GetWaterReference();
			if (this.Water.IsNullReference(this))
			{
				return;
			}
			this._System = base.GetComponent<ParticleSystem>();
			if (this._System.IsNullReference(this))
			{
				return;
			}
			this._System.main.simulationSpace = ParticleSystemSimulationSpace.World;
		}

		private void Start()
		{
			if (!this.Initialized)
			{
				return;
			}
			this._Particles = new ParticleSystem.Particle[this._System.main.maxParticles];
			this._Sampler = new WaterSample(this.Water, WaterSample.DisplacementMode.Height, 1f);
		}

		private void Update()
		{
			if (!this.Initialized || this._System.particleCount == 0)
			{
				return;
			}
			WaterParticleDisplacement._Data.Clear();
			int particles = this._System.GetParticles(this._Particles);
			int num = 0;
			int num2 = 0;
			int num3 = particles;
			if (particles > 64)
			{
				num2 = (int)((float)particles * (float)this._CurrentFrame / ((float)this.FrameSplit + 1f));
				num3 = (int)((float)particles * ((float)this._CurrentFrame + 1f) / ((float)this.FrameSplit + 1f));
				this.NextBatch();
			}
			for (int i = num2; i < num3; i++)
			{
				if (this._Particles[i].remainingLifetime > 0f)
				{
					float x = this._Particles[i].position.x;
					float z = this._Particles[i].position.z;
					float y = this._Particles[i].position.y;
					Vector3 andReset = this._Sampler.GetAndReset(x, z, WaterSample.ComputationsMode.Normal);
					if (y < andReset.y && Mathf.Abs(y - andReset.y) < 0.1f)
					{
						if (this._Particles[i].startLifetime - this._Particles[i].remainingLifetime >= 0.1f && this.UsedParticles > UnityEngine.Random.Range(0f, 1f))
						{
							float magnitude = this._Particles[i].velocity.magnitude;
							float num4 = this.ForceOverSpeed.Evaluate(magnitude);
							float num5 = this.Force * (1f + magnitude * num4 * 0.1f);
							WaterForce.Data item;
							item.Position = new Vector3(x, 0f, z);
							item.Force = num5 * (1f + this.SizeMultipier * this._Particles[i].GetCurrentSize(this._System));
							WaterParticleDisplacement._Data.Add(item);
						}
						this._Particles[i].remainingLifetime = 0f;
						num++;
					}
				}
			}
			if (WaterParticleDisplacement._Data.Count != 0)
			{
				WaterRipples.AddForce(WaterParticleDisplacement._Data, 1f);
			}
			if (num != 0)
			{
				this._System.SetParticles(this._Particles, particles);
			}
		}

		private void NextBatch()
		{
			if (this._CurrentFrame < this.FrameSplit)
			{
				this._CurrentFrame++;
				return;
			}
			this._CurrentFrame = 0;
		}

		private string Validation()
		{
			string text = string.Empty;
			if (this.Water == null)
			{
				text += "warning: assign water component";
			}
			return text;
		}

		[Tooltip("What water to use to check particle collisions")]
		public Water Water;

		[Tooltip("Force that each particle inflicts on water")]
		public float Force = 1f;

		[Tooltip("Percentage of particles causing wave effects")]
		[Range(0f, 1f)]
		public float UsedParticles = 1f;

		[Tooltip("Particle force modifier based on particle speed")]
		public AnimationCurve ForceOverSpeed = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(10f, 0f)
		});

		[Tooltip("Particle force modifier based on particle size")]
		public float SizeMultipier;

		[Range(0f, 16f)]
		[Tooltip("Calculate only a subset of all particles per frame")]
		public int FrameSplit = 2;

		private ParticleSystem _System;

		private WaterSample _Sampler;

		private ParticleSystem.Particle[] _Particles;

		private int _CurrentFrame;

		private static readonly List<WaterForce.Data> _Data = new List<WaterForce.Data>(256);

		private const int _SplitThreshold = 64;

		[SerializeField]
		[InspectorWarning("Validation", InspectorWarningAttribute.InfoType.Warning)]
		private string _Validation;
	}
}
