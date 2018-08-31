using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class ShipBowWavesEmitter : MonoBehaviour
	{
		private void Start()
		{
			if (this._GPUParticleSystem == null)
			{
				this._GPUParticleSystem = UnityEngine.Object.FindObjectOfType<WaveParticlesSystemGPU>();
			}
			this._UseBuiltinParticleSystem = (this._UnityParticleSystem == null);
			this._WaterComponent = this._GPUParticleSystem.GetComponent<Water>();
			this.OnValidate();
			Vector2 vector = ShipBowWavesEmitter.GetVector2(base.transform.position);
			Vector2 previousFrameBowPosition = vector + this._WaterComponent.SurfaceOffset;
			this._PreviousFrameBowPosition = previousFrameBowPosition;
		}

		private void OnValidate()
		{
			this._Space = this._Wavelength * this._EmissionSpacing;
			float f = Mathf.Acos(this._WaveSpeed);
			this._AngleSin = Mathf.Sin(f);
			this._AngleCos = Mathf.Cos(f);
		}

		private void LateUpdate()
		{
			Vector2 a = ShipBowWavesEmitter.GetVector2(base.transform.position);
			Vector2 vector = a + this._WaterComponent.SurfaceOffset;
			Vector2 vector2 = vector - this._PreviousFrameBowPosition;
			Vector2 normalized = ShipBowWavesEmitter.GetVector2(base.transform.forward).normalized;
			this._PreviousFrameBowPosition = vector;
			float num = vector2.x * normalized.x + vector2.y * normalized.y;
			if (num < 0f)
			{
				return;
			}
			float num2 = num;
			this._TotalBowDeltaMagnitude += num2;
			if (this._TotalBowDeltaMagnitude >= this._Space)
			{
				float time = Time.time;
				float num3 = time - this._LastBowEmitTime;
				this._LastBowEmitTime = time;
				float num4 = this._TotalBowDeltaMagnitude / num3;
				if (num4 >= this._MaxShipSpeed)
				{
					num4 = this._MaxShipSpeed;
				}
				float d = this._WaveSpeed * num4;
				Vector2 a2 = new Vector2(normalized.x * this._AngleCos - normalized.y * this._AngleSin, normalized.x * this._AngleSin + normalized.y * this._AngleCos);
				Vector2 a3 = new Vector2(normalized.x * this._AngleCos + normalized.y * this._AngleSin, normalized.y * this._AngleCos - normalized.x * this._AngleSin);
				Vector2 normalized2 = ShipBowWavesEmitter.GetVector2(base.transform.right).normalized;
				for (;;)
				{
					this._TotalBowDeltaMagnitude -= this._Space;
					if (this._AdvancedEmissionPositioning)
					{
						float y = this._WaterComponent.transform.position.y + this._WaterComponent.GetHeightAt(a.x, a.y, time);
						RaycastHit raycastHit;
						if (!this._ShipCollider.Raycast(new Ray(new Vector3(a.x, y, a.y), new Vector3(-normalized.x, 0f, -normalized.y)), out raycastHit, 100f))
						{
							break;
						}
						a = ShipBowWavesEmitter.GetVector2(raycastHit.point) + normalized * this._AdvancedEmissionOffset;
					}
					Vector2 horizontalDisplacementAt = this._WaterComponent.GetHorizontalDisplacementAt(a.x, a.y, time);
					a.x -= horizontalDisplacementAt.x;
					a.y -= horizontalDisplacementAt.y;
					if (this._UseBuiltinParticleSystem)
					{
						this._GPUParticleSystem.EmitParticle(new WaveParticlesSystemGPU.ParticleData
						{
							Position = a + normalized2 * this._LeftRightSpace,
							Direction = a3 * d,
							Amplitude = this._Amplitude,
							Wavelength = this._Wavelength,
							InitialLifetime = this._Lifetime,
							Lifetime = this._Lifetime,
							Foam = this._Foam,
							UvOffsetPack = (float)UnityEngine.Random.Range(0, this._GPUParticleSystem.FoamAtlasHeight) / (float)this._GPUParticleSystem.FoamAtlasHeight * 16f + (float)UnityEngine.Random.Range(this._MinTextureIndex, this._MaxTextureIndex) / (float)this._GPUParticleSystem.FoamAtlasWidth,
							TrailCalming = this._TrailCalming,
							TrailFoam = this._TrailFoam
						});
						this._GPUParticleSystem.EmitParticle(new WaveParticlesSystemGPU.ParticleData
						{
							Position = a + normalized2 * -this._LeftRightSpace,
							Direction = a2 * d,
							Amplitude = this._Amplitude,
							Wavelength = this._Wavelength,
							InitialLifetime = this._Lifetime,
							Lifetime = this._Lifetime,
							Foam = this._Foam,
							UvOffsetPack = (float)UnityEngine.Random.Range(0, this._GPUParticleSystem.FoamAtlasHeight) / (float)this._GPUParticleSystem.FoamAtlasHeight * 16f + (float)UnityEngine.Random.Range(this._MinTextureIndex, this._MaxTextureIndex) / (float)this._GPUParticleSystem.FoamAtlasWidth,
							TrailCalming = this._TrailCalming,
							TrailFoam = this._TrailFoam
						});
					}
					else
					{
						ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
						emitParams.position = new Vector3(a.x + normalized2.x * this._LeftRightSpace, this._WaterComponent.transform.position.y, a.y + normalized2.y * this._LeftRightSpace);
						emitParams.velocity = new Vector3(a3.x, 0f, a3.y) * d;
						this._UnityParticleSystem.Emit(emitParams, 1);
						emitParams.position = new Vector3(a.x + normalized2.x * -this._LeftRightSpace, emitParams.position.y, a.y + normalized2.y * -this._LeftRightSpace);
						emitParams.velocity = new Vector3(a2.x, 0f, a2.y) * d;
						this._UnityParticleSystem.Emit(emitParams, 1);
					}
					if (this._TotalBowDeltaMagnitude < this._Space)
					{
						return;
					}
				}
				return;
			}
		}

		private static Vector2 GetVector2(Vector3 vector3)
		{
			return new Vector2(vector3.x, vector3.z);
		}

		[FormerlySerializedAs("gpuParticleSystem")]
		[SerializeField]
		private WaveParticlesSystemGPU _GPUParticleSystem;

		[SerializeField]
		[FormerlySerializedAs("unityParticleSystem")]
		private ParticleSystem _UnityParticleSystem;

		[FormerlySerializedAs("waveSpeed")]
		[SerializeField]
		[Range(0.02f, 0.98f)]
		private float _WaveSpeed = 0.5f;

		[FormerlySerializedAs("amplitude")]
		[SerializeField]
		private float _Amplitude = 0.5f;

		[FormerlySerializedAs("wavelength")]
		[SerializeField]
		private float _Wavelength = 6f;

		[FormerlySerializedAs("lifetime")]
		[SerializeField]
		private float _Lifetime = 50f;

		[FormerlySerializedAs("foam")]
		[SerializeField]
		private float _Foam = 1f;

		[FormerlySerializedAs("maxShipSpeed")]
		[SerializeField]
		private float _MaxShipSpeed = 16.5f;

		[FormerlySerializedAs("leftRightSpace")]
		[SerializeField]
		private float _LeftRightSpace = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		[FormerlySerializedAs("trailCalming")]
		private float _TrailCalming = 1f;

		[SerializeField]
		[Range(0f, 8f)]
		[FormerlySerializedAs("trailFoam")]
		private float _TrailFoam = 1f;

		[FormerlySerializedAs("advancedEmissionPositioning")]
		[Tooltip("Use for submarines. Allows emission to be moved to exposed ship parts during submerge process and completely disabled after complete submarge.")]
		[Header("Advanced")]
		[SerializeField]
		private bool _AdvancedEmissionPositioning;

		[SerializeField]
		[Tooltip("Required if 'advancedEmissionPositioning' is enabled. Allows emitter to determine an emission point on that collider.")]
		[FormerlySerializedAs("shipCollider")]
		private Collider _ShipCollider;

		[FormerlySerializedAs("advancedEmissionOffset")]
		[SerializeField]
		private float _AdvancedEmissionOffset = 2f;

		[SerializeField]
		[FormerlySerializedAs("minTextureIndex")]
		private int _MinTextureIndex;

		[FormerlySerializedAs("maxTextureIndex")]
		[SerializeField]
		private int _MaxTextureIndex = 4;

		[SerializeField]
		[Range(0.1f, 1f)]
		[FormerlySerializedAs("emissionSpacing")]
		private float _EmissionSpacing = 0.45f;

		private Vector2 _PreviousFrameBowPosition;

		private float _TotalBowDeltaMagnitude;

		private float _LastBowEmitTime;

		private float _AngleSin;

		private float _AngleCos;

		private float _Space;

		private bool _UseBuiltinParticleSystem;

		private Water _WaterComponent;
	}
}
