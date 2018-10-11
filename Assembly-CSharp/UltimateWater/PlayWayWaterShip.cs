using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class PlayWayWaterShip : MonoBehaviour
	{
		private void Start()
		{
			this._PropellerEffectsData = new PlayWayWaterShip.ParticleSystemData[this._SternEffects.Length];
			for (int i = this._PropellerEffectsData.Length - 1; i >= 0; i--)
			{
				this._PropellerEffectsData[i] = new PlayWayWaterShip.ParticleSystemData(this._SternEffects[i]);
			}
			this._BowSprayEmittersData = new PlayWayWaterShip.ParticleSystemData[this._BowSprayEmitters.Length];
			for (int j = this._BowSprayEmittersData.Length - 1; j >= 0; j--)
			{
				this._BowSprayEmittersData[j] = new PlayWayWaterShip.ParticleSystemData(this._BowSprayEmitters[j]);
			}
		}

		private void OnEnable()
		{
			this.SetEnabled(true);
		}

		private void OnDisable()
		{
			this.SetEnabled(false);
		}

		private void Update()
		{
			float num = this._RigidBody.velocity.magnitude / this._MaxVelocity;
			float num2 = Mathf.Clamp01(1f - (-this._BowWavesEmitter.transform.position.y - 1f) * 0.5f);
			float num3 = PlayWayWaterShip.ClampIntensity(num * num2);
			if (num3 != this._PreviousBowEffectsIntensity)
			{
				this._PreviousBowEffectsIntensity = num3;
				this._BowWavesEmitter.enabled = (num3 > 0f);
				for (int i = this._BowSprayEmitters.Length - 1; i >= 0; i--)
				{
					PlayWayWaterShip.SetEffectIntensity(this._BowSprayEmitters[i], this._BowSprayEmittersData[i], num3);
				}
			}
			float num4 = Mathf.Clamp01(1f - (-this._MainCollider.bounds.max.y - 1f) * 0.5f);
			float num5 = PlayWayWaterShip.ClampIntensity(num * num4);
			if (num5 != this._PreviousSternEffectsIntensity)
			{
				this._PreviousSternEffectsIntensity = num5;
				for (int j = this._SternWaveEmitters.Length - 1; j >= 0; j--)
				{
					this._SternWaveEmitters[j].Power = num5;
				}
				for (int k = 0; k < this._SternEffects.Length; k++)
				{
					PlayWayWaterShip.SetEffectIntensity(this._SternEffects[k], this._PropellerEffectsData[k], num5);
				}
			}
		}

		private static void SetEffectIntensity(ParticleSystem particleSystem, PlayWayWaterShip.ParticleSystemData data, float intensity)
		{
			float num = (intensity != 0f) ? (0.5f + intensity * 0.5f) : 0f;
			float num2 = intensity * intensity;
			particleSystem.emission.rateOverTimeMultiplier = data.RateOverTime * num;
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSpeedMultiplier = data.StartSpeed * num;
			if (data.UseAlphaGradient)
			{
				Gradient gradient = data.Gradient;
				GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
				for (int i = 0; i < alphaKeys.Length; i++)
				{
					GradientAlphaKey[] array = alphaKeys;
					int num3 = i;
					array[num3].alpha = array[num3].alpha * num2;
				}
				main.startColor = gradient;
			}
			else
			{
				Color color = data.Color;
				color.a *= num2;
				main.startColor = color;
			}
		}

		private static float ClampIntensity(float x)
		{
			return (x <= 1f) ? ((x >= 0.2f) ? x : 0f) : 1f;
		}

		private void SetEnabled(bool enable)
		{
			for (int i = 0; i < this._SternWaveEmitters.Length; i++)
			{
				this._SternWaveEmitters[i].enabled = enable;
			}
			if (this._BowWavesEmitter != null)
			{
				this._BowWavesEmitter.enabled = enable;
			}
		}

		[FormerlySerializedAs("rigidBody")]
		[SerializeField]
		private Rigidbody _RigidBody;

		[FormerlySerializedAs("mainCollider")]
		[SerializeField]
		private Collider _MainCollider;

		[FormerlySerializedAs("sternEffects")]
		[SerializeField]
		private ParticleSystem[] _SternEffects;

		[SerializeField]
		[FormerlySerializedAs("sternWaveEmitters")]
		private WavesEmitter[] _SternWaveEmitters;

		[SerializeField]
		[FormerlySerializedAs("bowWavesEmitter")]
		private ShipBowWavesEmitter _BowWavesEmitter;

		[SerializeField]
		[FormerlySerializedAs("bowSprayEmitters")]
		private ParticleSystem[] _BowSprayEmitters;

		[FormerlySerializedAs("maxVelocity")]
		[SerializeField]
		private float _MaxVelocity = 7.5f;

		private PlayWayWaterShip.ParticleSystemData[] _PropellerEffectsData;

		private PlayWayWaterShip.ParticleSystemData[] _BowSprayEmittersData;

		private float _PreviousSternEffectsIntensity = float.NaN;

		private float _PreviousBowEffectsIntensity = float.NaN;

		public class ParticleSystemData
		{
			public ParticleSystemData(ParticleSystem particleSystem)
			{
				this.RateOverTime = particleSystem.emission.rateOverTimeMultiplier;
				this.StartSpeed = particleSystem.main.startSpeedMultiplier;
				switch (particleSystem.main.startColor.mode)
				{
				case ParticleSystemGradientMode.Color:
					this.UseAlphaGradient = false;
					this.Color = particleSystem.main.startColor.color;
					return;
				case ParticleSystemGradientMode.Gradient:
				case ParticleSystemGradientMode.RandomColor:
					this.UseAlphaGradient = true;
					this.Gradient = particleSystem.main.startColor.gradient;
					return;
				}
				throw new ArgumentException("Unsupported startColor mode: " + particleSystem.main.startColor.mode);
			}

			public float RateOverTime;

			public float StartSpeed;

			public bool UseAlphaGradient;

			public Color Color;

			public Gradient Gradient;
		}
	}
}
