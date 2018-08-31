using System;
using System.Collections.Generic;
using System.Linq;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public class SpectrumResolverCPU
	{
		public SpectrumResolverCPU(Water water, WindWaves windWaves, int numScales)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._SpectraDataCache = new Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>();
			this._SpectraDataList = new List<WaterWavesSpectrumDataBase>();
			this._OverlayedSpectra = new List<WaterWavesSpectrumDataBase>();
			this._NumTiles = numScales;
			this._CachedSeed = water.Seed;
			this.CreateSpectraLevels();
		}

		public WaterWave[] DirectWaves
		{
			get
			{
				WaterWave[] validatedDirectWavesList;
				lock (this)
				{
					validatedDirectWavesList = this.GetValidatedDirectWavesList();
				}
				return validatedDirectWavesList;
			}
		}

		public float MaxVerticalDisplacement { get; private set; }

		public float MaxHorizontalDisplacement { get; private set; }

		public Vector2 WindDirection { get; private set; }

		public float LastFrameTime { get; private set; }

		public float StdDev { get; private set; }

		public WaterTileSpectrum GetTile(int index)
		{
			return this._TileSpectra[index];
		}

		public List<WaterWavesSpectrumDataBase> GetOverlayedSpectraDirect()
		{
			return this._OverlayedSpectra;
		}

		public void DisposeCachedSpectra()
		{
			object spectraDataCache = this._SpectraDataCache;
			lock (spectraDataCache)
			{
				Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>.Enumerator enumerator = this._SpectraDataCache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<WaterWavesSpectrum, WaterWavesSpectrumData> keyValuePair = enumerator.Current;
					keyValuePair.Value.Dispose(false);
				}
				enumerator.Dispose();
			}
			this.SetDirectionalSpectrumDirty();
		}

		public WaterWavesSpectrumData GetSpectrumData(WaterWavesSpectrum spectrum)
		{
			WaterWavesSpectrumData waterWavesSpectrumData;
			if (!this._SpectraDataCache.TryGetValue(spectrum, out waterWavesSpectrumData))
			{
				object spectraDataCache = this._SpectraDataCache;
				lock (spectraDataCache)
				{
					waterWavesSpectrumData = (this._SpectraDataCache[spectrum] = new WaterWavesSpectrumData(this._Water, this._WindWaves, spectrum));
				}
				waterWavesSpectrumData.ValidateSpectrumData();
				this._CpuWavesDirty = true;
				object spectraDataList = this._SpectraDataList;
				lock (spectraDataList)
				{
					this._SpectraDataList.Add(waterWavesSpectrumData);
				}
			}
			return waterWavesSpectrumData;
		}

		public virtual void AddSpectrum(WaterWavesSpectrumDataBase spectrum)
		{
			object overlayedSpectra = this._OverlayedSpectra;
			lock (overlayedSpectra)
			{
				this._OverlayedSpectra.Add(spectrum);
			}
		}

		public virtual void RemoveSpectrum(WaterWavesSpectrumDataBase spectrum)
		{
			object overlayedSpectra = this._OverlayedSpectra;
			lock (overlayedSpectra)
			{
				this._OverlayedSpectra.Remove(spectrum);
			}
		}

		public bool ContainsSpectrum(WaterWavesSpectrumDataBase spectrum)
		{
			return this._OverlayedSpectra.Contains(spectrum);
		}

		public void CacheSpectrum(WaterWavesSpectrum spectrum)
		{
			this.GetSpectrumData(spectrum);
		}

		public Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> GetCachedSpectraDirect()
		{
			return this._SpectraDataCache;
		}

		public Vector3 GetDisplacementAt(float x, float z, float time)
		{
			Vector3 vector = default(Vector3);
			x = -(x + this._SurfaceOffset.x);
			z = -(z + this._SurfaceOffset.y);
			if (this._TargetDirectWavesCount == -1)
			{
				for (int i = this._NumTiles - 1; i >= 0; i--)
				{
					if (this._TileSpectra[i].ResolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int num;
						int num2;
						int num3;
						int num4;
						this.InterpolationParams(x, z, i, this._WindWaves.TileSizes[i], out fx, out invFx, out fy, out invFy, out num, out num2, out num3, out num4);
						Vector2[] array;
						Vector2[] array2;
						Vector4[] array3;
						Vector4[] array4;
						float t;
						this._TileSpectra[i].GetResults(time, out array, out array2, out array3, out array4, out t);
						Vector2 vector2 = FastMath.Interpolate(ref array[num], ref array[num2], ref array[num3], ref array[num4], ref array2[num], ref array2[num2], ref array2[num3], ref array2[num4], fx, invFx, fy, invFy, t);
						vector.x -= vector2.x;
						vector.z -= vector2.y;
						vector.y += FastMath.Interpolate(array3[num].w, array3[num2].w, array3[num3].w, array3[num4].w, array4[num].w, array4[num2].w, array4[num3].w, array4[num4].w, fx, invFx, fy, invFy, t);
					}
				}
			}
			else
			{
				lock (this)
				{
					WaterWave[] validatedDirectWavesList = this.GetValidatedDirectWavesList();
					if (validatedDirectWavesList.Length != 0)
					{
						Vector3 vector3 = default(Vector3);
						for (int j = 0; j < validatedDirectWavesList.Length; j++)
						{
							vector3 += validatedDirectWavesList[j].GetDisplacementAt(x, z, time);
						}
						vector += vector3;
					}
				}
			}
			float num5 = -this._Water.Materials.HorizontalDisplacementScale * this._UniformWaterScale;
			vector.x *= num5;
			vector.y *= this._UniformWaterScale;
			vector.z *= num5;
			return vector;
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float time)
		{
			Vector2 vector = default(Vector2);
			x = -(x + this._SurfaceOffset.x);
			z = -(z + this._SurfaceOffset.y);
			if (this._TargetDirectWavesCount == -1)
			{
				for (int i = this._NumTiles - 1; i >= 0; i--)
				{
					if (this._TileSpectra[i].ResolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int num;
						int num2;
						int num3;
						int num4;
						this.InterpolationParams(x, z, i, this._WindWaves.TileSizes[i], out fx, out invFx, out fy, out invFy, out num, out num2, out num3, out num4);
						Vector2[] array;
						Vector2[] array2;
						Vector4[] array3;
						Vector4[] array4;
						float t;
						this._TileSpectra[i].GetResults(time, out array, out array2, out array3, out array4, out t);
						vector -= FastMath.Interpolate(ref array[num], ref array[num2], ref array[num3], ref array[num4], ref array2[num], ref array2[num2], ref array2[num3], ref array2[num4], fx, invFx, fy, invFy, t);
					}
				}
			}
			else
			{
				lock (this)
				{
					WaterWave[] validatedDirectWavesList = this.GetValidatedDirectWavesList();
					if (validatedDirectWavesList.Length != 0)
					{
						Vector2 vector2 = default(Vector2);
						for (int j = 0; j < validatedDirectWavesList.Length; j++)
						{
							vector2 += validatedDirectWavesList[j].GetRawHorizontalDisplacementAt(x, z, time);
						}
						vector += vector2;
					}
				}
			}
			float num5 = -this._Water.Materials.HorizontalDisplacementScale * this._UniformWaterScale;
			vector.x *= num5;
			vector.y *= num5;
			return vector;
		}

		public Vector4 GetForceAndHeightAt(float x, float z, float time)
		{
			Vector4 vector = default(Vector4);
			x = -(x + this._SurfaceOffset.x);
			z = -(z + this._SurfaceOffset.y);
			if (this._TargetDirectWavesCount == -1)
			{
				for (int i = this._NumTiles - 1; i >= 0; i--)
				{
					WaterTileSpectrum waterTileSpectrum = this._TileSpectra[i];
					if (waterTileSpectrum.ResolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int num;
						int num2;
						int num3;
						int num4;
						this.InterpolationParams(x, z, i, this._WindWaves.TileSizes[i], out fx, out invFx, out fy, out invFy, out num, out num2, out num3, out num4);
						Vector2[] array;
						Vector2[] array2;
						Vector4[] array3;
						Vector4[] array4;
						float t;
						waterTileSpectrum.GetResults(time, out array, out array2, out array3, out array4, out t);
						vector += FastMath.Interpolate(array3[num], array3[num2], array3[num3], array3[num4], array4[num], array4[num2], array4[num3], array4[num4], fx, invFx, fy, invFy, t);
					}
				}
			}
			else
			{
				lock (this)
				{
					WaterWave[] validatedDirectWavesList = this.GetValidatedDirectWavesList();
					if (validatedDirectWavesList.Length != 0)
					{
						Vector4 b = default(Vector4);
						for (int j = 0; j < validatedDirectWavesList.Length; j++)
						{
							validatedDirectWavesList[j].GetForceAndHeightAt(x, z, time, ref b);
						}
						vector += b;
					}
				}
			}
			float num5 = -this._Water.Materials.HorizontalDisplacementScale * this._UniformWaterScale;
			vector.x *= num5;
			vector.z *= num5;
			vector.y *= 0.5f * this._UniformWaterScale;
			vector.w *= this._UniformWaterScale;
			return vector;
		}

		public float GetHeightAt(float x, float z, float time)
		{
			float num = 0f;
			x = -(x + this._SurfaceOffset.x);
			z = -(z + this._SurfaceOffset.y);
			if (this._TargetDirectWavesCount == -1)
			{
				for (int i = this._NumTiles - 1; i >= 0; i--)
				{
					if (this._TileSpectra[i].ResolveByFFT)
					{
						float num2;
						float num3;
						float num4;
						float num5;
						int num6;
						int num7;
						int num8;
						int num9;
						this.InterpolationParams(x, z, i, this._WindWaves.TileSizes[i], out num2, out num3, out num4, out num5, out num6, out num7, out num8, out num9);
						Vector2[] array;
						Vector2[] array2;
						Vector4[] array3;
						Vector4[] array4;
						float num10;
						this._TileSpectra[i].GetResults(time, out array, out array2, out array3, out array4, out num10);
						float num11 = array3[num6].w * num2 + array3[num7].w * num3;
						float num12 = array3[num8].w * num2 + array3[num9].w * num3;
						float num13 = num11 * num4 + num12 * num5;
						float num14 = array4[num6].w * num2 + array4[num7].w * num3;
						float num15 = array4[num8].w * num2 + array4[num9].w * num3;
						float num16 = num14 * num4 + num15 * num5;
						num += num13 * (1f - num10) + num16 * num10;
					}
				}
			}
			else
			{
				lock (this)
				{
					WaterWave[] validatedDirectWavesList = this.GetValidatedDirectWavesList();
					if (validatedDirectWavesList.Length != 0)
					{
						float num17 = 0f;
						for (int j = 0; j < validatedDirectWavesList.Length; j++)
						{
							num17 += validatedDirectWavesList[j].GetHeightAt(x, z, time);
						}
						num += num17;
					}
				}
			}
			return num * this._UniformWaterScale;
		}

		public void SetDirectWaveEvaluationMode(int waveCount)
		{
			lock (this)
			{
				if (this._DirectWaves == null)
				{
					this._DirectWaves = new WaterWave[0];
				}
				this._TargetDirectWavesCount = waveCount;
				this._CpuWavesDirty = true;
			}
		}

		public void SetFFTEvaluationMode()
		{
			lock (this)
			{
				this._DirectWaves = null;
				this._TargetDirectWavesCount = -1;
			}
		}

		public WaterWave[] FindMostMeaningfulWaves(int waveCount)
		{
			Heap<WaterWave> heap = new Heap<WaterWave>();
			for (int i = this._SpectraDataList.Count - 1; i >= 0; i--)
			{
				WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = this._SpectraDataList[i];
				waterWavesSpectrumDataBase.UpdateSpectralValues(this.WindDirection, this._WindWaves.SpectrumDirectionality);
				object obj = waterWavesSpectrumDataBase;
				lock (obj)
				{
					float weight = waterWavesSpectrumDataBase.Weight;
					foreach (WaterWave element in waterWavesSpectrumDataBase.CpuWaves)
					{
						element._Amplitude *= weight;
						element._CPUPriority *= weight;
						heap.Insert(element);
						if (heap.Count > waveCount)
						{
							heap.ExtractMax();
						}
					}
				}
			}
			return heap.ToArray<WaterWave>();
		}

		public virtual void SetDirectionalSpectrumDirty()
		{
			this._CpuWavesDirty = true;
			for (int i = this._SpectraDataList.Count - 1; i >= 0; i--)
			{
				this._SpectraDataList[i].SetCpuWavesDirty();
			}
			for (int j = 0; j < this._NumTiles; j++)
			{
				this._TileSpectra[j].SetDirty();
			}
		}

		internal void Update()
		{
			this._SurfaceOffset = this._Water.SurfaceOffset;
			float num = this._Water.Time;
			if (this._WindWaves.LoopDuration != 0f)
			{
				num %= this._WindWaves.LoopDuration;
			}
			this.LastFrameTime = num;
			this._UniformWaterScale = this._Water.UniformWaterScale;
			this.UpdateCachedSeed();
			bool allowCpuFFT = WaterProjectSettings.Instance.AllowCpuFFT;
			for (int i = 0; i < this._NumTiles; i++)
			{
				int num2 = 16;
				int num3 = 0;
				for (;;)
				{
					float num4 = 0f;
					for (int j = this._SpectraDataList.Count - 1; j >= 0; j--)
					{
						WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = this._SpectraDataList[j];
						waterWavesSpectrumDataBase.ValidateSpectrumData();
						float standardDeviation = waterWavesSpectrumDataBase.GetStandardDeviation(i, num3);
						num4 += standardDeviation * waterWavesSpectrumDataBase.Weight;
					}
					for (int k = this._OverlayedSpectra.Count - 1; k >= 0; k--)
					{
						WaterWavesSpectrumDataBase waterWavesSpectrumDataBase2 = this._OverlayedSpectra[k];
						waterWavesSpectrumDataBase2.ValidateSpectrumData();
						float standardDeviation2 = waterWavesSpectrumDataBase2.GetStandardDeviation(i, num3);
						num4 += standardDeviation2 * waterWavesSpectrumDataBase2.Weight;
					}
					if (num4 < this._WindWaves.CpuDesiredStandardError * 0.25f || num2 >= this._WindWaves.FinalResolution)
					{
						break;
					}
					num2 <<= 1;
					num3++;
				}
				if (num2 > this._WindWaves.FinalResolution)
				{
					num2 = this._WindWaves.FinalResolution;
				}
				WaterTileSpectrum waterTileSpectrum = this._TileSpectra[i];
				if (waterTileSpectrum.SetResolveMode(num2 >= 16 && allowCpuFFT, num2))
				{
					this._CpuWavesDirty = true;
				}
			}
		}

		internal void SetWindDirection(Vector2 windDirection)
		{
			this.WindDirection = windDirection;
			this.SetDirectionalSpectrumDirty();
		}

		private void InterpolationParams(float x, float z, int scaleIndex, float tileSize, out float fx, out float invFx, out float fy, out float invFy, out int index0, out int index1, out int index2, out int index3)
		{
			int resolutionFFT = this._TileSpectra[scaleIndex].ResolutionFFT;
			float num = 0.5f / (float)this._WindWaves.FinalResolution * tileSize;
			x += num;
			z += num;
			float num2 = (float)resolutionFFT / tileSize;
			fx = x * num2;
			fy = z * num2;
			int num3 = (int)fx;
			if ((float)num3 > fx)
			{
				num3--;
			}
			int num4 = (int)fy;
			if ((float)num4 > fy)
			{
				num4--;
			}
			fx -= (float)num3;
			fy -= (float)num4;
			num3 %= resolutionFFT;
			num4 %= resolutionFFT;
			if (num3 < 0)
			{
				num3 += resolutionFFT;
			}
			if (num4 < 0)
			{
				num4 += resolutionFFT;
			}
			num3 = resolutionFFT - num3 - 1;
			num4 = resolutionFFT - num4 - 1;
			int num5 = num3 + 1;
			int num6 = num4 + 1;
			if (num5 == resolutionFFT)
			{
				num5 = 0;
			}
			if (num6 == resolutionFFT)
			{
				num6 = 0;
			}
			num4 *= resolutionFFT;
			num6 *= resolutionFFT;
			index0 = num4 + num3;
			index1 = num4 + num5;
			index2 = num6 + num3;
			index3 = num6 + num5;
			invFx = 1f - fx;
			invFy = 1f - fy;
		}

		private WaterWave[] GetValidatedDirectWavesList()
		{
			if (this._CpuWavesDirty)
			{
				this._CpuWavesDirty = false;
				WaterWave[] source = this.FindMostMeaningfulWaves(this._TargetDirectWavesCount);
				this._DirectWaves = source.ToArray<WaterWave>();
			}
			return this._DirectWaves;
		}

		public GerstnerWave[] SelectShorelineWaves(int waveCount, float angle, float coincidenceRange)
		{
			Heap<WaterWave> heap = new Heap<WaterWave>();
			for (int i = this._SpectraDataList.Count - 1; i >= 0; i--)
			{
				WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = this._SpectraDataList[i];
				waterWavesSpectrumDataBase.UpdateSpectralValues(this.WindDirection, this._WindWaves.SpectrumDirectionality);
				object obj = waterWavesSpectrumDataBase;
				lock (obj)
				{
					float weight = waterWavesSpectrumDataBase.Weight;
					foreach (WaterWave element in waterWavesSpectrumDataBase.ShorelineCandidates)
					{
						element._Amplitude *= weight;
						element._CPUPriority *= weight;
						float current = Mathf.Atan2(element._Nkx, element._Nky) * 57.29578f;
						if (Mathf.Abs(Mathf.DeltaAngle(current, angle)) < coincidenceRange && element._Amplitude > 0.025f)
						{
							heap.Insert(element);
							if (heap.Count > waveCount)
							{
								heap.ExtractMax();
							}
						}
					}
				}
			}
			Vector2[] array = new Vector2[4];
			for (int k = 0; k < 4; k++)
			{
				float num = this._WindWaves.TileSizes[k];
				array[k].x = num + 0.5f / (float)this._WindWaves.FinalResolution * num;
				array[k].y = -num + 0.5f / (float)this._WindWaves.FinalResolution * num;
			}
			WaterWave[] array2 = heap.ToArray<WaterWave>();
			int num2 = Mathf.Min(heap.Count, waveCount);
			GerstnerWave[] array3 = new GerstnerWave[num2];
			for (int l = 0; l < num2; l++)
			{
				array3[l] = new GerstnerWave(array2[heap.Count - l - 1], array);
			}
			return array3;
		}

		private void UpdateCachedSeed()
		{
			if (this._CachedSeed == this._Water.Seed)
			{
				return;
			}
			this._CachedSeed = this._Water.Seed;
			this.DisposeCachedSpectra();
			this.OnProfilesChanged();
		}

		internal virtual void OnProfilesChanged()
		{
			Water.WeightedProfile[] profiles = this._Water.ProfilesManager.Profiles;
			Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>.Enumerator enumerator = this._SpectraDataCache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<WaterWavesSpectrum, WaterWavesSpectrumData> keyValuePair = enumerator.Current;
				keyValuePair.Value.Weight = 0f;
			}
			enumerator.Dispose();
			foreach (Water.WeightedProfile weightedProfile in profiles)
			{
				if (weightedProfile.Weight > 0.0001f)
				{
					WaterWavesSpectrum spectrum = weightedProfile.Profile.Spectrum;
					WaterWavesSpectrumData spectrumData;
					if (!this._SpectraDataCache.TryGetValue(spectrum, out spectrumData))
					{
						spectrumData = this.GetSpectrumData(spectrum);
					}
					spectrumData.Weight = weightedProfile.Weight;
				}
			}
			this.SetDirectionalSpectrumDirty();
			this.StdDev = 0f;
			float num = 0f;
			enumerator = this._SpectraDataCache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<WaterWavesSpectrum, WaterWavesSpectrumData> keyValuePair2 = enumerator.Current;
				WaterWavesSpectrumData value = keyValuePair2.Value;
				value.ValidateSpectrumData();
				this.StdDev += value.GetStandardDeviation() * value.Weight;
				if (value.CpuWaves.Length != 0)
				{
					num += value.CpuWaves[0]._Amplitude * value.Weight;
				}
			}
			enumerator.Dispose();
			for (int j = this._OverlayedSpectra.Count - 1; j >= 0; j--)
			{
				WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = this._OverlayedSpectra[j];
				waterWavesSpectrumDataBase.ValidateSpectrumData();
				this.StdDev += waterWavesSpectrumDataBase.GetStandardDeviation() * waterWavesSpectrumDataBase.Weight;
				if (waterWavesSpectrumDataBase.CpuWaves.Length != 0)
				{
					num += waterWavesSpectrumDataBase.CpuWaves[0]._Amplitude * waterWavesSpectrumDataBase.Weight;
				}
			}
			this.MaxVerticalDisplacement = this.StdDev * 1.6f + num;
			this.MaxHorizontalDisplacement = this.MaxVerticalDisplacement * this._Water.Materials.HorizontalDisplacementScale;
		}

		private void CreateSpectraLevels()
		{
			this._TileSpectra = new WaterTileSpectrum[this._NumTiles];
			for (int i = 0; i < this._NumTiles; i++)
			{
				this._TileSpectra[i] = new WaterTileSpectrum(this._Water, this._WindWaves, i);
			}
		}

		internal virtual void OnMapsFormatChanged(bool resolution)
		{
			if (this._SpectraDataCache != null)
			{
				Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>.Enumerator enumerator = this._SpectraDataCache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<WaterWavesSpectrum, WaterWavesSpectrumData> keyValuePair = enumerator.Current;
					keyValuePair.Value.Dispose(!resolution);
				}
				enumerator.Dispose();
			}
			if (resolution)
			{
				for (int i = this._OverlayedSpectra.Count - 1; i >= 0; i--)
				{
					this._OverlayedSpectra[i].Dispose(false);
				}
			}
			this.SetDirectionalSpectrumDirty();
		}

		internal virtual void OnDestroy()
		{
			this.OnMapsFormatChanged(true);
			this._SpectraDataCache = null;
			for (int i = this._OverlayedSpectra.Count - 1; i >= 0; i--)
			{
				this._OverlayedSpectra[i].Dispose(false);
			}
			this._OverlayedSpectra.Clear();
			object spectraDataList = this._SpectraDataList;
			lock (spectraDataList)
			{
				this._SpectraDataList.Clear();
			}
		}

		private WaterTileSpectrum[] _TileSpectra;

		private Vector2 _SurfaceOffset;

		private float _UniformWaterScale;

		private WaterWave[] _DirectWaves;

		private int _TargetDirectWavesCount = -1;

		private int _CachedSeed;

		private bool _CpuWavesDirty;

		private readonly int _NumTiles;

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		private readonly List<WaterWavesSpectrumDataBase> _SpectraDataList;

		protected readonly List<WaterWavesSpectrumDataBase> _OverlayedSpectra;

		protected Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> _SpectraDataCache;
	}
}
