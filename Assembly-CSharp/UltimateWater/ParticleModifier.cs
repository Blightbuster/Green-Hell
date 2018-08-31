using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater
{
	public class ParticleModifier
	{
		public float Emission
		{
			set
			{
				for (int i = 0; i < this._Systems.Length; i++)
				{
					ParticleModifier.InitialData initialData = this._InitialData[i];
					ParticleSystem particleSystem = this._Systems[i];
					ParticleSystem.EmissionModule emission = particleSystem.emission;
					ParticleSystem.MinMaxCurve emissionRate = initialData.EmissionRate;
					emission.rateOverTime = ParticleModifier.MultiplyMinMaxCurve(value, emissionRate);
					int bursts = emission.GetBursts(ParticleModifier._Bursts);
					for (int j = 0; j < bursts; j++)
					{
						ParticleModifier._Bursts[j].minCount = (short)(initialData.BurstRates[j].x * value);
						ParticleModifier._Bursts[j].maxCount = (short)(initialData.BurstRates[j].y * value);
					}
					emission.SetBursts(ParticleModifier._Bursts, bursts);
				}
			}
		}

		public float Speed
		{
			set
			{
				for (int i = 0; i < this._Systems.Length; i++)
				{
					ParticleModifier.InitialData initialData = this._InitialData[i];
					ParticleSystem particleSystem = this._Systems[i];
					particleSystem.main.startSpeedMultiplier = initialData.Speed * value;
				}
			}
		}

		public bool Active
		{
			set
			{
				if (value)
				{
					for (int i = 0; i < this._Systems.Length; i++)
					{
						ParticleSystem particleSystem = this._Systems[i];
						particleSystem.Play();
					}
				}
				else
				{
					for (int j = 0; j < this._Systems.Length; j++)
					{
						ParticleSystem particleSystem2 = this._Systems[j];
						particleSystem2.Stop();
					}
				}
			}
		}

		public bool IsInitialized
		{
			get
			{
				return this._Systems != null;
			}
		}

		public void Initialize(ParticleSystem[] particleSystems)
		{
			this._Systems = particleSystems;
			this._InitialData = new List<ParticleModifier.InitialData>(this._Systems.Length);
			foreach (ParticleSystem particleSystem in this._Systems)
			{
				ParticleModifier.InitialData item;
				item.Speed = particleSystem.main.startSpeedMultiplier;
				item.EmissionRate = particleSystem.emission.rateOverTime;
				item.BurstRates = new List<Vector2>();
				int bursts = particleSystem.emission.GetBursts(ParticleModifier._Bursts);
				for (int j = 0; j < bursts; j++)
				{
					item.BurstRates.Add(new Vector2((float)ParticleModifier._Bursts[j].minCount, (float)ParticleModifier._Bursts[j].maxCount));
				}
				this._InitialData.Add(item);
			}
		}

		private static ParticleSystem.MinMaxCurve MultiplyMinMaxCurve(float value, ParticleSystem.MinMaxCurve result)
		{
			switch (result.mode)
			{
			case ParticleSystemCurveMode.Constant:
				result.constant *= value;
				break;
			case ParticleSystemCurveMode.Curve:
			case ParticleSystemCurveMode.TwoCurves:
				result.curveMultiplier = value;
				break;
			case ParticleSystemCurveMode.TwoConstants:
				result.constantMin *= value;
				result.constantMax *= value;
				break;
			}
			return result;
		}

		private ParticleSystem[] _Systems;

		private List<ParticleModifier.InitialData> _InitialData;

		private static readonly ParticleSystem.Burst[] _Bursts = new ParticleSystem.Burst[4];

		private struct InitialData
		{
			public ParticleSystem.MinMaxCurve EmissionRate;

			public List<Vector2> BurstRates;

			public float Speed;
		}
	}
}
