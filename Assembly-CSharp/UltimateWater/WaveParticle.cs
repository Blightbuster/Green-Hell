using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class WaveParticle : IPoint2D
	{
		static WaveParticle()
		{
			for (int i = 0; i < 2048; i++)
			{
				double num = (double)(((float)i + 0.49f) / 2047f);
				double kh = 4.0 * (1.0 - Math.Pow(1.0 - num, 0.33333333));
				WaveParticle._AmplitudeFuncPrecomp[i] = WaveParticle.ComputeAmplitudeAtShore(kh);
				WaveParticle._FrequencyFuncPrecomp[i] = Mathf.Sqrt(1f / WaveParticle.ComputeWavelengthAtShore(kh));
			}
		}

		private WaveParticle(Vector2 position, Vector2 direction, float baseFrequency, float baseAmplitude, float lifetime, bool isShoreWave)
		{
			this._Position = position;
			this.Direction = direction;
			this.BaseFrequency = baseFrequency;
			this.BaseAmplitude = baseAmplitude;
			this.FadeFactor = 0f;
			this.Frequency = baseFrequency;
			this.Amplitude = baseAmplitude;
			this.IsShoreWave = isShoreWave;
			this.BaseSpeed = 2.5f * Mathf.Sqrt(9.81f / baseFrequency);
			this.Lifetime = lifetime;
			this.CostlyUpdate(null, 0.1f);
		}

		public Vector2 Position
		{
			get
			{
				return this._Position;
			}
		}

		public Vector4 PackedParticleData
		{
			get
			{
				return new Vector4(this.Direction.x * 2f * 3.14159274f / this.Frequency, this.Direction.y * 2f * 3.14159274f / this.Frequency, this.Shoaling, this.Speed);
			}
		}

		public Vector3 VertexData
		{
			get
			{
				return new Vector3(this._Position.x, this._Position.y, this.Amplitude);
			}
		}

		public Vector3 DebugData
		{
			get
			{
				return new Vector3((float)this.Group.Id, 0f, 0f);
			}
		}

		public static WaveParticle Create(Vector3 position, Vector2 direction, float baseFrequency, float baseAmplitude, float lifetime, bool isShoreWave)
		{
			return WaveParticle.Create(new Vector2(position.x, position.z), direction, baseFrequency, baseAmplitude, lifetime, isShoreWave);
		}

		public static WaveParticle Create(Vector2 position, Vector2 direction, float baseFrequency, float baseAmplitude, float lifetime, bool isShoreWave)
		{
			WaveParticle waveParticle;
			if (WaveParticle._WaveParticlesCache.Count != 0)
			{
				waveParticle = WaveParticle._WaveParticlesCache.Pop();
				waveParticle._Position = position;
				waveParticle.Direction = direction;
				waveParticle.BaseFrequency = baseFrequency;
				waveParticle.BaseAmplitude = baseAmplitude;
				waveParticle.FadeFactor = 0f;
				waveParticle.IsShoreWave = isShoreWave;
				waveParticle.BaseSpeed = 2.2f * Mathf.Sqrt(9.81f / baseFrequency);
				waveParticle.Amplitude = baseAmplitude;
				waveParticle.Frequency = baseFrequency;
				waveParticle.TargetSpeed = 1f;
				waveParticle.Invkh = 1f;
				waveParticle.TargetInvKh = 1f;
				waveParticle.EnergyBalance = 0f;
				waveParticle.Shoaling = 0f;
				waveParticle.Speed = 0f;
				waveParticle.TargetEnergyBalance = 0f;
				waveParticle.Lifetime = lifetime;
				waveParticle.AmplitudeModifiers = 0f;
				waveParticle.AmplitudeModifiers2 = 1f;
				waveParticle.ExpansionEnergyLoss = 0f;
				waveParticle.IsAlive = true;
				waveParticle.DisallowSubdivision = false;
				if (waveParticle.LeftNeighbour != null || waveParticle.RightNeighbour != null)
				{
					waveParticle.LeftNeighbour = null;
					waveParticle.RightNeighbour = null;
				}
				waveParticle.CostlyUpdate(null, 0.1f);
			}
			else
			{
				waveParticle = new WaveParticle(position, direction, baseFrequency, baseAmplitude, lifetime, isShoreWave);
			}
			return (waveParticle.BaseAmplitude == 0f) ? null : waveParticle;
		}

		public void Destroy()
		{
			this.BaseAmplitude = (this.Amplitude = 0f);
			this.IsAlive = false;
			if (this.LeftNeighbour != null)
			{
				this.LeftNeighbour.RightNeighbour = this.RightNeighbour;
				this.LeftNeighbour.DisallowSubdivision = true;
			}
			if (this.RightNeighbour != null)
			{
				this.RightNeighbour.LeftNeighbour = this.LeftNeighbour;
				this.RightNeighbour.DisallowSubdivision = true;
			}
			if (this.Group != null && this.Group.LeftParticle == this)
			{
				this.Group.LeftParticle = this.RightNeighbour;
			}
			this.LeftNeighbour = null;
			this.RightNeighbour = null;
		}

		public void DelayedDestroy()
		{
			this.BaseAmplitude = (this.Amplitude = 0f);
			this.IsAlive = false;
		}

		public void AddToCache()
		{
			WaveParticle._WaveParticlesCache.Push(this);
		}

		public WaveParticle Clone(Vector2 position)
		{
			WaveParticle waveParticle = WaveParticle.Create(position, this.Direction, this.BaseFrequency, this.BaseAmplitude, this.Lifetime, this.IsShoreWave);
			if (waveParticle == null)
			{
				return null;
			}
			waveParticle.Amplitude = this.Amplitude;
			waveParticle.Frequency = this.Frequency;
			waveParticle.Speed = this.Speed;
			waveParticle.TargetSpeed = this.TargetSpeed;
			waveParticle.EnergyBalance = this.EnergyBalance;
			waveParticle.Shoaling = this.Shoaling;
			waveParticle.Group = this.Group;
			return waveParticle;
		}

		public void Update(float deltaTime, float step, float invStep)
		{
			if (this.Lifetime > 0f)
			{
				if (this.FadeFactor != 1f)
				{
					this.FadeFactor += deltaTime;
					if (this.FadeFactor > 1f)
					{
						this.FadeFactor = 1f;
					}
				}
			}
			else
			{
				this.FadeFactor -= deltaTime;
				if (this.FadeFactor <= 0f)
				{
					this.Destroy();
					return;
				}
			}
			if (this.TargetEnergyBalance < this.EnergyBalance)
			{
				float num = step * 0.005f;
				this.EnergyBalance = this.EnergyBalance * (1f - num) + this.TargetEnergyBalance * num;
			}
			else
			{
				float num2 = step * 0.0008f;
				this.EnergyBalance = this.EnergyBalance * (1f - num2) + this.TargetEnergyBalance * num2;
			}
			this.BaseAmplitude += deltaTime * this.EnergyBalance;
			this.BaseAmplitude *= step * this.ExpansionEnergyLoss + 1f;
			if (this.BaseAmplitude <= 0.01f)
			{
				this.Destroy();
				return;
			}
			this.Speed = invStep * this.Speed + step * this.TargetSpeed;
			float num3 = this.Speed + this.EnergyBalance * -20f;
			this.Invkh = invStep * this.Invkh + step * this.TargetInvKh;
			int num4 = (int)(2047f * (1f - this.Invkh * this.Invkh * this.Invkh) - 0.49f);
			float num5 = (num4 < 2048) ? WaveParticle._FrequencyFuncPrecomp[num4] : 1f;
			this.Frequency = this.BaseFrequency * num5;
			this.Amplitude = this.FadeFactor * this.BaseAmplitude * ((num4 < 2048) ? WaveParticle._AmplitudeFuncPrecomp[num4] : 1f);
			this.Shoaling = this.AmplitudeModifiers * 0.004f * -this.EnergyBalance / this.Amplitude;
			this.Amplitude *= this.AmplitudeModifiers;
			float num6 = num3 * deltaTime;
			this._Position.x = this._Position.x + this.Direction.x * num6;
			this._Position.y = this._Position.y + this.Direction.y * num6;
		}

		public int CostlyUpdate(WaveParticlesQuadtree quadtree, float deltaTime)
		{
			float num;
			if (this.Frequency < 0.025f)
			{
				float x = this._Position.x + this.Direction.x / this.Frequency;
				float z = this._Position.y + this.Direction.y / this.Frequency;
				num = Mathf.Max(StaticWaterInteraction.GetTotalDepthAt(this._Position.x, this._Position.y), StaticWaterInteraction.GetTotalDepthAt(x, z));
			}
			else
			{
				num = StaticWaterInteraction.GetTotalDepthAt(this._Position.x, this._Position.y);
			}
			if (num <= 0.001f)
			{
				this.Destroy();
				return 0;
			}
			this.UpdateWaveParameters(deltaTime, num);
			int result = 0;
			if (quadtree != null && !this.DisallowSubdivision)
			{
				if (this.LeftNeighbour != null)
				{
					this.Subdivide(quadtree, this.LeftNeighbour, this, ref result);
				}
				if (this.RightNeighbour != null)
				{
					this.Subdivide(quadtree, this, this.RightNeighbour, ref result);
				}
			}
			return result;
		}

		private void UpdateWaveParameters(float deltaTime, float depth)
		{
			this.Lifetime -= deltaTime;
			this.TargetInvKh = 1f - 0.25f * this.BaseFrequency * depth;
			if (this.TargetInvKh < 0f)
			{
				this.TargetInvKh = 0f;
			}
			int num = (int)(this.BaseFrequency * depth * 512f);
			this.TargetSpeed = this.BaseSpeed * ((num < 2048) ? FastMath.PositiveTanhSqrtNoZero[num] : 1f);
			if (this.TargetSpeed < 0.5f)
			{
				this.TargetSpeed = 0.5f;
			}
			float num2 = 0.135f / this.Frequency;
			if (num2 < this.Amplitude)
			{
				this.TargetEnergyBalance = -this.Amplitude * 5f;
			}
			if (this.LeftNeighbour != null && this.RightNeighbour != null && !this.DisallowSubdivision)
			{
				Vector2 vector = new Vector2(this.RightNeighbour._Position.y - this.LeftNeighbour._Position.y, this.LeftNeighbour._Position.x - this.RightNeighbour._Position.x);
				float num3 = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
				if (num3 > 0.001f)
				{
					if (vector.x * this.Direction.x + vector.y * this.Direction.y < 0f)
					{
						num3 = -num3;
					}
					vector.x /= num3;
					vector.y /= num3;
					float num4 = 0.6f * deltaTime;
					if (num4 > 0.6f)
					{
						num4 = 0.6f;
					}
					this.Direction.x = this.Direction.x * (1f - num4) + vector.x * num4;
					this.Direction.y = this.Direction.y * (1f - num4) + vector.y * num4;
					float num5 = Mathf.Sqrt(this.Direction.x * this.Direction.x + this.Direction.y * this.Direction.y);
					this.Direction.x = this.Direction.x / num5;
					this.Direction.y = this.Direction.y / num5;
				}
				this.ExpansionEnergyLoss = -1f + 0.5f * (this.Direction.x * (this.LeftNeighbour.Direction.x + this.RightNeighbour.Direction.x) + this.Direction.y * (this.LeftNeighbour.Direction.y + this.RightNeighbour.Direction.y));
				if (this.ExpansionEnergyLoss < -1f)
				{
					this.ExpansionEnergyLoss = -1f;
				}
				if (this.LeftNeighbour.DisallowSubdivision)
				{
					this.LeftNeighbour.ExpansionEnergyLoss = this.ExpansionEnergyLoss;
				}
				if (this.RightNeighbour.DisallowSubdivision)
				{
					this.RightNeighbour.ExpansionEnergyLoss = this.ExpansionEnergyLoss;
				}
			}
			this.AmplitudeModifiers = 1f;
			if (this.IsShoreWave)
			{
				int num6 = (int)(depth * 5.12f);
				if (num6 < 2048)
				{
					this.AmplitudeModifiers *= 1f - FastMath.PositiveTanhSqrtNoZero[num6];
				}
			}
			this.AmplitudeModifiers *= this.AmplitudeModifiers2;
		}

		private void Subdivide(WaveParticlesQuadtree quadtree, WaveParticle left, WaveParticle right, ref int numSubdivisions)
		{
			Vector2 a = left._Position - right._Position;
			float magnitude = a.magnitude;
			if (magnitude * this.Frequency > 1f && magnitude > 1f && quadtree.FreeSpace != 0)
			{
				WaveParticle waveParticle = WaveParticle.Create(right._Position + a * 0.5f, (left.Direction + right.Direction) * 0.5f, (left.BaseFrequency + right.BaseFrequency) * 0.5f, (left.BaseAmplitude + right.BaseAmplitude) * 0.5f, (left.Lifetime + right.Lifetime) * 0.5f, left.IsShoreWave);
				if (waveParticle != null)
				{
					waveParticle.Group = left.Group;
					waveParticle.Amplitude = (left.Amplitude + right.Amplitude) * 0.5f;
					waveParticle.Frequency = (left.Frequency + right.Frequency) * 0.5f;
					waveParticle.Speed = (left.Speed + right.Speed) * 0.5f;
					waveParticle.TargetSpeed = (left.TargetSpeed + right.TargetSpeed) * 0.5f;
					waveParticle.EnergyBalance = (left.EnergyBalance + right.EnergyBalance) * 0.5f;
					waveParticle.Shoaling = (left.Shoaling + right.Shoaling) * 0.5f;
					waveParticle.TargetInvKh = (left.TargetInvKh + right.TargetInvKh) * 0.5f;
					waveParticle.Lifetime = (left.Lifetime + right.Lifetime) * 0.5f;
					waveParticle.TargetEnergyBalance = (left.TargetEnergyBalance + right.TargetEnergyBalance) * 0.5f;
					waveParticle.AmplitudeModifiers = (left.AmplitudeModifiers + right.AmplitudeModifiers) * 0.5f;
					waveParticle.AmplitudeModifiers2 = (left.AmplitudeModifiers2 + right.AmplitudeModifiers2) * 0.5f;
					waveParticle.Invkh = (left.Invkh + right.Invkh) * 0.5f;
					waveParticle.BaseSpeed = (left.BaseSpeed + right.BaseSpeed) * 0.5f;
					waveParticle.ExpansionEnergyLoss = (left.ExpansionEnergyLoss + right.ExpansionEnergyLoss) * 0.5f;
					waveParticle.Direction = left.Direction;
					if (quadtree.AddElement(waveParticle))
					{
						waveParticle.LeftNeighbour = left;
						waveParticle.RightNeighbour = right;
						left.RightNeighbour = waveParticle;
						right.LeftNeighbour = waveParticle;
					}
					numSubdivisions++;
				}
			}
		}

		private static float ComputeAmplitudeAtShore(double kh)
		{
			double num = Math.Cosh(kh);
			return (float)Math.Sqrt(2.0 * num * num / (Math.Sinh(2.0 * kh) + 2.0 * kh));
		}

		private static float ComputeWavelengthAtShore(double kh)
		{
			return (float)Math.Pow(Math.Tanh(Math.Pow(kh * Math.Tanh(kh), 0.75)), 0.666666);
		}

		[FormerlySerializedAs("direction")]
		public Vector2 Direction;

		[FormerlySerializedAs("speed")]
		public float Speed;

		[FormerlySerializedAs("targetSpeed")]
		public float TargetSpeed = 1f;

		[FormerlySerializedAs("baseFrequency")]
		public float BaseFrequency;

		[FormerlySerializedAs("frequency")]
		public float Frequency;

		[FormerlySerializedAs("baseAmplitude")]
		public float BaseAmplitude;

		[FormerlySerializedAs("amplitude")]
		public float Amplitude;

		[FormerlySerializedAs("fadeFactor")]
		public float FadeFactor;

		[FormerlySerializedAs("energyBalance")]
		public float EnergyBalance;

		[FormerlySerializedAs("targetEnergyBalance")]
		public float TargetEnergyBalance;

		[FormerlySerializedAs("shoaling")]
		public float Shoaling;

		[FormerlySerializedAs("invkh")]
		public float Invkh = 1f;

		[FormerlySerializedAs("targetInvKh")]
		public float TargetInvKh = 1f;

		[FormerlySerializedAs("baseSpeed")]
		public float BaseSpeed;

		[FormerlySerializedAs("lifetime")]
		public float Lifetime;

		[FormerlySerializedAs("amplitudeModifiers")]
		public float AmplitudeModifiers;

		[FormerlySerializedAs("amplitudeModifiers2")]
		public float AmplitudeModifiers2 = 1f;

		[FormerlySerializedAs("expansionEnergyLoss")]
		public float ExpansionEnergyLoss;

		[FormerlySerializedAs("isShoreWave")]
		public bool IsShoreWave;

		[FormerlySerializedAs("isAlive")]
		public bool IsAlive = true;

		[FormerlySerializedAs("disallowSubdivision")]
		public bool DisallowSubdivision;

		[FormerlySerializedAs("leftNeighbour")]
		public WaveParticle LeftNeighbour;

		[FormerlySerializedAs("rightNeighbour")]
		public WaveParticle RightNeighbour;

		[FormerlySerializedAs("group")]
		public WaveParticlesGroup Group;

		private Vector2 _Position;

		private static readonly Stack<WaveParticle> _WaveParticlesCache = new Stack<WaveParticle>();

		private static readonly float[] _AmplitudeFuncPrecomp = new float[2048];

		private static readonly float[] _FrequencyFuncPrecomp = new float[2048];
	}
}
