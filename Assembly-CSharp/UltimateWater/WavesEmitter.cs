using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WavesEmitter : MonoBehaviour
	{
		public float Power
		{
			get
			{
				return this._Power;
			}
			set
			{
				this._Power = ((value <= 0f) ? 0f : value);
				this._FinalEmissionInterval = this._EmissionInterval / this._Power;
				base.enabled = (this._Power != 0f);
			}
		}

		private void OnValidate()
		{
			this._FinalEmissionInterval = this._EmissionInterval / this._Power;
		}

		private void LateUpdate()
		{
			float time = Time.time;
			if (time - this._LastEmitTime >= this._FinalEmissionInterval)
			{
				this._LastEmitTime = time;
				Vector2 vector = WavesEmitter.GetVector2(base.transform.position);
				Vector2 normalized = WavesEmitter.GetVector2(base.transform.forward).normalized;
				Vector2 normalized2 = WavesEmitter.GetVector2(base.transform.right).normalized;
				if (normalized.x == 0f && normalized.y == 0f)
				{
					normalized = WavesEmitter.GetVector2(base.transform.up).normalized;
				}
				float num = this._EmissionAngle * 0.0174532924f;
				float x = UnityEngine.Random.Range(-num, num);
				float num2;
				float num3;
				FastMath.SinCos2048(x, out num2, out num3);
				Vector2 a = new Vector2(normalized.x * num3 - normalized.y * num2, normalized.x * num2 + normalized.y * num3);
				Vector2 position = vector + normalized2 * UnityEngine.Random.Range(-this._EmissionArea, this._EmissionArea);
				Vector2 horizontalDisplacementAt = this._WaterComponent.GetHorizontalDisplacementAt(position.x, position.y, Time.time);
				position.x -= horizontalDisplacementAt.x;
				position.y -= horizontalDisplacementAt.y;
				this._Water.EmitParticle(new WaveParticlesSystemGPU.ParticleData
				{
					Position = position,
					Direction = a * (this._Speed * this._Power),
					Amplitude = this._Amplitude * this._Power,
					Wavelength = this._Wavelength,
					InitialLifetime = this._Lifetime * this._Power,
					Lifetime = this._Lifetime * this._Power,
					Foam = this._Foam * this._Power,
					UvOffsetPack = (float)UnityEngine.Random.Range(0, this._Water.FoamAtlasHeight) / (float)this._Water.FoamAtlasHeight * 16f + (float)UnityEngine.Random.Range(this._MinTextureU, this._MaxTextureU) / (float)this._Water.FoamAtlasWidth,
					TrailCalming = this._TrailCalming,
					TrailFoam = this._TrailFoam
				});
			}
		}

		private void Start()
		{
			if (this._Water == null)
			{
				this._Water = UnityEngine.Object.FindObjectOfType<WaveParticlesSystemGPU>();
			}
			this._WaterComponent = this._Water.GetComponent<Water>();
			this.Power = this._InitialPower;
		}

		private static Vector2 GetVector2(Vector3 vector3)
		{
			return new Vector2(vector3.x, vector3.z);
		}

		[SerializeField]
		[FormerlySerializedAs("water")]
		private WaveParticlesSystemGPU _Water;

		[FormerlySerializedAs("amplitude")]
		[SerializeField]
		private float _Amplitude = 0.1f;

		[FormerlySerializedAs("wavelength")]
		[SerializeField]
		private float _Wavelength = 10f;

		[FormerlySerializedAs("lifetime")]
		[SerializeField]
		private float _Lifetime = 50f;

		[FormerlySerializedAs("speed")]
		[SerializeField]
		private float _Speed = 3.5f;

		[FormerlySerializedAs("foam")]
		[SerializeField]
		private float _Foam = 1f;

		[FormerlySerializedAs("emissionArea")]
		[SerializeField]
		private float _EmissionArea = 1f;

		[FormerlySerializedAs("emissionInterval")]
		[SerializeField]
		private float _EmissionInterval = 0.15f;

		[SerializeField]
		[FormerlySerializedAs("trailCalming")]
		[Range(0f, 1f)]
		private float _TrailCalming = 1f;

		[SerializeField]
		[FormerlySerializedAs("trailFoam")]
		[Range(0f, 8f)]
		private float _TrailFoam = 1f;

		[SerializeField]
		[FormerlySerializedAs("emissionAngle")]
		[Range(0f, 180f)]
		private float _EmissionAngle;

		[SerializeField]
		[FormerlySerializedAs("minTextureU")]
		[Header("Advanced")]
		private int _MinTextureU = 4;

		[FormerlySerializedAs("maxTextureU")]
		[SerializeField]
		private int _MaxTextureU = 8;

		[SerializeField]
		[FormerlySerializedAs("initialPower")]
		[Range(0f, 1f)]
		private float _InitialPower = 1f;

		private float _Power = -1f;

		private float _LastEmitTime;

		private float _FinalEmissionInterval;

		private Water _WaterComponent;
	}
}
