using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	public class ParticleController : MonoBehaviour
	{
		private void Awake()
		{
			this._CurrentEmission = this.Emission;
			this._Modifier.Initialize(this._Particles.ToArray());
			this._Modifier.Speed = this.Speed;
			this._Modifier.Active = false;
		}

		private void OnEnable()
		{
			this.Sampler.OnSubmersionStateChanged.AddListener(new UnityAction<WaterSampler.SubmersionState>(this.OnChange));
		}

		private void OnDisable()
		{
			this.Sampler.OnSubmersionStateChanged.RemoveListener(new UnityAction<WaterSampler.SubmersionState>(this.OnChange));
		}

		private void LateUpdate()
		{
			this._Modifier.Emission = ((this._CurrentEmission <= 0f) ? 0f : this._CurrentEmission);
			if (this._CurrentEmission > 0f)
			{
				this._CurrentEmission -= Time.deltaTime * this.Decrease;
				if (this._CurrentEmission <= 0f)
				{
					this._Modifier.Active = false;
				}
			}
		}

		private void OnValidate()
		{
			if (!Application.isPlaying || !this._Modifier.IsInitialized)
			{
				return;
			}
			this._Modifier.Speed = this.Speed;
		}

		private void Reset()
		{
			this.Sampler = base.GetComponent<WaterSampler>();
		}

		private void OnChange(WaterSampler.SubmersionState state)
		{
			bool flag = false;
			ParticleController.EmissionType type = this.Type;
			if (type != ParticleController.EmissionType.OnWaterEnter)
			{
				if (type != ParticleController.EmissionType.OnWaterExit)
				{
					if (type == ParticleController.EmissionType.OnWaterEnterAndExit)
					{
						flag = true;
					}
				}
				else
				{
					flag = (state == WaterSampler.SubmersionState.Above);
				}
			}
			else
			{
				flag = (state == WaterSampler.SubmersionState.Under);
			}
			if (!flag)
			{
				return;
			}
			this._Modifier.Active = true;
			this._Modifier.Emission = (this._CurrentEmission = this.Emission);
		}

		[SerializeField]
		private List<ParticleSystem> _Particles;

		[Space]
		public WaterSampler Sampler;

		[Space]
		public ParticleController.EmissionType Type = ParticleController.EmissionType.OnWaterEnterAndExit;

		[Space]
		public float Emission = 1f;

		public float Speed = 1f;

		[Tooltip("How fast the emission decreases")]
		public float Decrease = 1f;

		private readonly ParticleModifier _Modifier = new ParticleModifier();

		private float _CurrentEmission;

		public enum EmissionType
		{
			OnWaterEnter,
			OnWaterExit,
			OnWaterEnterAndExit
		}
	}
}
