using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class ComplexWavesEmitter : MonoBehaviour, IWavesParticleSystemPlugin
	{
		public void UpdateParticles(float time, float deltaTime)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			ComplexWavesEmitter.WavesSource wavesSource = this._WavesSource;
			if (wavesSource != ComplexWavesEmitter.WavesSource.CustomWaveFrequency)
			{
				if (wavesSource != ComplexWavesEmitter.WavesSource.WindWavesSpectrum)
				{
					if (wavesSource == ComplexWavesEmitter.WavesSource.Shoaling)
					{
						if (this._SpawnPoints == null)
						{
							this.CreateShoalingSpawnPoints();
						}
						this.UpdateSpawnPoints(deltaTime);
					}
				}
				else
				{
					if (this._SpawnPoints == null)
					{
						this.CreateSpectralWavesSpawnPoints();
					}
					this.UpdateSpawnPoints(deltaTime);
				}
			}
			else if (time > this._NextSpawnTime)
			{
				Vector3 position = base.transform.position;
				Vector3 forward = base.transform.forward;
				Vector2 position2 = new Vector2(position.x, position.z);
				Vector2 vector = new Vector2(forward.x, forward.z);
				WaveParticle waveParticle = WaveParticle.Create(position2, vector.normalized, 6.28318548f / this._Wavelength, this._Amplitude, this._Lifetime, this._ShoreWaves);
				if (waveParticle != null)
				{
					this._WavesParticleSystem.Spawn(waveParticle, this._Width, this._WaveShapeIrregularity, 2f, 0.35f);
					waveParticle.Destroy();
					waveParticle.AddToCache();
				}
				this._NextSpawnTime += this._TimeStep;
			}
		}

		private void Awake()
		{
			this._WindWaves = this._WavesParticleSystem.GetComponent<Water>().WindWaves;
			this.OnValidate();
			this._WavesParticleSystem.RegisterPlugin(this);
		}

		private void OnEnable()
		{
			this.OnValidate();
			this._NextSpawnTime = Time.time + UnityEngine.Random.Range(0f, this._TimeStep);
		}

		private void OnValidate()
		{
			this._TimeStep = this._Wavelength / this._EmissionRate;
		}

		private void UpdateSpawnPoints(float deltaTime)
		{
			deltaTime *= this._EmissionFrequencyScale;
			for (int i = 0; i < this._SpawnPoints.Length; i++)
			{
				ComplexWavesEmitter.SpawnPoint spawnPoint = this._SpawnPoints[i];
				spawnPoint.TimeLeft -= deltaTime;
				if (spawnPoint.TimeLeft < 0f)
				{
					float num = 6.28318548f / spawnPoint.Frequency;
					float num2 = this._Span * 0.3f / num;
					int min = Mathf.Max(2, Mathf.RoundToInt(num2 * 0.7f));
					int max = Mathf.Max(2, Mathf.RoundToInt(num2 * 1.429f));
					spawnPoint.TimeLeft += spawnPoint.TimeInterval;
					Vector2 position = spawnPoint.Position + new Vector2(spawnPoint.Direction.y, -spawnPoint.Direction.x) * UnityEngine.Random.Range(-this._Span * 0.35f, this._Span * 0.35f);
					WaveParticle waveParticle = WaveParticle.Create(position, spawnPoint.Direction, spawnPoint.Frequency, spawnPoint.Amplitude, this._Lifetime, this._ShoreWaves);
					if (waveParticle != null)
					{
						this._WavesParticleSystem.Spawn(waveParticle, UnityEngine.Random.Range(min, max), this._WaveShapeIrregularity, 2f, 0.35f);
						waveParticle.Destroy();
						waveParticle.AddToCache();
					}
				}
			}
		}

		private void CreateShoalingSpawnPoints()
		{
			Bounds bounds = new Bounds(base.transform.position, new Vector3(this._BoundsSize.x, 0f, this._BoundsSize.y));
			float x = bounds.min.x;
			float z = bounds.min.z;
			float x2 = bounds.max.x;
			float z2 = bounds.max.z;
			float num = Mathf.Sqrt(this._SpawnPointsDensity);
			float num2 = Mathf.Max(35f, bounds.size.x / 256f) / num;
			float num3 = Mathf.Max(35f, bounds.size.z / 256f) / num;
			bool[,] array = new bool[32, 32];
			List<ComplexWavesEmitter.SpawnPoint> list = new List<ComplexWavesEmitter.SpawnPoint>();
			GerstnerWave[] array2 = this._WindWaves.SpectrumResolver.SelectShorelineWaves(50, 0f, 360f);
			if (array2.Length == 0)
			{
				this._SpawnPoints = new ComplexWavesEmitter.SpawnPoint[0];
				return;
			}
			float num4 = this._SpawnDepth * 0.85f;
			float num5 = this._SpawnDepth * 1.18f;
			for (float num6 = z; num6 < z2; num6 += num3)
			{
				for (float num7 = x; num7 < x2; num7 += num2)
				{
					int num8 = Mathf.FloorToInt(32f * (num7 - x) / (x2 - x));
					int num9 = Mathf.FloorToInt(32f * (num6 - z) / (z2 - z));
					if (!array[num8, num9])
					{
						float totalDepthAt = StaticWaterInteraction.GetTotalDepthAt(num7, num6);
						if (totalDepthAt > num4 && totalDepthAt < num5 && UnityEngine.Random.value < 0.06f)
						{
							array[num8, num9] = true;
							Vector2 vector;
							vector.x = StaticWaterInteraction.GetTotalDepthAt(num7 - 3f, num6) - StaticWaterInteraction.GetTotalDepthAt(num7 + 3f, num6);
							vector.y = StaticWaterInteraction.GetTotalDepthAt(num7, num6 - 3f) - StaticWaterInteraction.GetTotalDepthAt(num7, num6 + 3f);
							vector.Normalize();
							GerstnerWave gerstnerWave = array2[0];
							float num10 = Vector2.Dot(vector, array2[0].Direction);
							for (int i = 1; i < array2.Length; i++)
							{
								float num11 = Vector2.Dot(vector, array2[i].Direction);
								if (num11 > num10)
								{
									num10 = num11;
									gerstnerWave = array2[i];
								}
							}
							list.Add(new ComplexWavesEmitter.SpawnPoint(new Vector2(num7, num6), vector, gerstnerWave.Frequency, Mathf.Abs(gerstnerWave.Amplitude), gerstnerWave.Speed));
						}
					}
				}
			}
			this._SpawnPoints = list.ToArray();
		}

		private void CreateSpectralWavesSpawnPoints()
		{
			Vector3 normalized = base.transform.forward.normalized;
			float num = Mathf.Atan2(normalized.x, normalized.z);
			GerstnerWave[] array = this._WindWaves.SpectrumResolver.SelectShorelineWaves(this._SpectrumWavesCount, num * 57.29578f, this._SpectrumCoincidenceRange);
			this._SpectrumWavesCount = array.Length;
			Vector3 vector = new Vector3(base.transform.position.x + this._Span * 0.5f, 0f, base.transform.position.z + this._Span * 0.5f);
			Vector2 a = new Vector2(vector.x, vector.z);
			List<ComplexWavesEmitter.SpawnPoint> list = new List<ComplexWavesEmitter.SpawnPoint>();
			for (int i = 0; i < this._SpectrumWavesCount; i++)
			{
				GerstnerWave gerstnerWave = array[i];
				if (gerstnerWave.Amplitude != 0f)
				{
					Vector2 position = a - gerstnerWave.Direction * this._Span * 0.5f;
					list.Add(new ComplexWavesEmitter.SpawnPoint(position, gerstnerWave.Direction, gerstnerWave.Frequency, Mathf.Abs(gerstnerWave.Amplitude), gerstnerWave.Speed));
				}
			}
			this._SpawnPoints = list.ToArray();
		}

		[FormerlySerializedAs("wavesParticleSystem")]
		[SerializeField]
		private WaveParticleSystem _WavesParticleSystem;

		[FormerlySerializedAs("wavesSource")]
		[SerializeField]
		private ComplexWavesEmitter.WavesSource _WavesSource;

		[FormerlySerializedAs("wavelength")]
		[SerializeField]
		private float _Wavelength = 120f;

		[FormerlySerializedAs("amplitude")]
		[SerializeField]
		private float _Amplitude = 0.6f;

		[FormerlySerializedAs("emissionRate")]
		[SerializeField]
		private float _EmissionRate = 2f;

		[FormerlySerializedAs("width")]
		[SerializeField]
		private int _Width = 8;

		[FormerlySerializedAs("spectrumCoincidenceRange")]
		[Range(0f, 180f)]
		[SerializeField]
		private float _SpectrumCoincidenceRange = 20f;

		[Range(0f, 100f)]
		[FormerlySerializedAs("spectrumWavesCount")]
		[SerializeField]
		private int _SpectrumWavesCount = 30;

		[SerializeField]
		[FormerlySerializedAs("span")]
		[Tooltip("Affects both waves and emission area width.")]
		private float _Span = 1000f;

		[FormerlySerializedAs("waveShapeIrregularity")]
		[SerializeField]
		[Range(1f, 3.5f)]
		private float _WaveShapeIrregularity = 2f;

		[FormerlySerializedAs("lifetime")]
		[SerializeField]
		private float _Lifetime = 200f;

		[FormerlySerializedAs("shoreWaves")]
		[SerializeField]
		private bool _ShoreWaves = true;

		[SerializeField]
		[FormerlySerializedAs("boundsSize")]
		private Vector2 _BoundsSize = new Vector2(500f, 500f);

		[FormerlySerializedAs("spawnDepth")]
		[SerializeField]
		[Range(3f, 80f)]
		private float _SpawnDepth = 8f;

		[SerializeField]
		[FormerlySerializedAs("emissionFrequencyScale")]
		[Range(0.01f, 2f)]
		private float _EmissionFrequencyScale = 1f;

		[FormerlySerializedAs("spawnPointsDensity")]
		[SerializeField]
		private float _SpawnPointsDensity = 1f;

		private ComplexWavesEmitter.SpawnPoint[] _SpawnPoints;

		private WindWaves _WindWaves;

		private float _NextSpawnTime;

		private float _TimeStep;

		public enum WavesSource
		{
			CustomWaveFrequency,
			WindWavesSpectrum,
			Shoaling,
			Vehicle
		}

		private class SpawnPoint
		{
			public SpawnPoint(Vector2 position, Vector2 direction, float frequency, float amplitude, float speed)
			{
				this.Position = position;
				this.Direction = direction;
				this.Frequency = frequency;
				this.Amplitude = amplitude;
				this.TimeInterval = 6.28318548f / speed * UnityEngine.Random.Range(1f, 8f);
				this.TimeLeft = UnityEngine.Random.Range(0f, this.TimeInterval);
			}

			public readonly Vector2 Position;

			public Vector2 Direction;

			public readonly float Frequency;

			public readonly float Amplitude;

			public readonly float TimeInterval;

			public float TimeLeft;
		}
	}
}
