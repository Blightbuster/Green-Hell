using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class WaterSprayEmitter : MonoBehaviour
	{
		public float StartVelocity
		{
			get
			{
				return this._StartVelocity;
			}
			set
			{
				this._StartVelocity = value;
			}
		}

		private void Update()
		{
			int num = 0;
			this._TotalTime += Time.deltaTime;
			while (this._TotalTime >= this._TimeStep && num < this._Particles.Length)
			{
				this._TotalTime -= this._TimeStep;
				this._Particles[num].Lifetime = new Vector2(this._Lifetime, this._Lifetime);
				this._Particles[num].MaxIntensity = this._StartIntensity;
				this._Particles[num].Position = base.transform.position + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f));
				this._Particles[num].Velocity = base.transform.forward * this._StartVelocity;
				this._Particles[num++].Offset = UnityEngine.Random.Range(0f, 10f);
			}
			if (num != 0)
			{
				this._Water.SpawnCustomParticles(this._Particles, num);
			}
		}

		private void OnValidate()
		{
			this._TimeStep = 1f / this._EmissionRate;
		}

		private void Start()
		{
			this.OnValidate();
			this._Particles = new Spray.Particle[Mathf.Max(1, (int)this._EmissionRate)];
		}

		[SerializeField]
		[FormerlySerializedAs("water")]
		private Spray _Water;

		[SerializeField]
		[FormerlySerializedAs("emissionRate")]
		private float _EmissionRate = 5f;

		[FormerlySerializedAs("startIntensity")]
		[SerializeField]
		private float _StartIntensity = 1f;

		[FormerlySerializedAs("startVelocity")]
		[SerializeField]
		private float _StartVelocity = 1f;

		[FormerlySerializedAs("lifetime")]
		[SerializeField]
		private float _Lifetime = 4f;

		private float _TotalTime;

		private float _TimeStep;

		private Spray.Particle[] _Particles;
	}
}
