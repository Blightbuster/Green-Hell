using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class WaterTileSpectrum
	{
		public WaterTileSpectrum(Water water, WindWaves windWaves, int index)
		{
			this.Water = water;
			this.WindWaves = windWaves;
			this.TileIndex = index;
		}

		public bool IsResolvedByFFT
		{
			get
			{
				return this.ResolveByFFT;
			}
		}

		public int ResolutionFFT
		{
			get
			{
				return this._ResolutionFFT;
			}
		}

		public int MipIndexFFT
		{
			get
			{
				return this._MipIndexFFT;
			}
		}

		public void SetDirty()
		{
			this.DirectionalSpectrumDirty = 2;
		}

		public bool SetResolveMode(bool resolveByFFT, int resolution)
		{
			if (this.ResolveByFFT != resolveByFFT || (this.ResolveByFFT && this._ResolutionFFT != resolution))
			{
				if (resolveByFFT)
				{
					lock (this)
					{
						bool flag = this.WindWaves.LoopDuration != 0f;
						int num = (!flag) ? 4 : ((int)(this.WindWaves.LoopDuration * 5f + 0.6f));
						this._ResolutionFFT = resolution;
						this._MipIndexFFT = WaterWavesSpectrumDataBase.GetMipIndex(resolution);
						int num2 = resolution * resolution;
						this.DirectionalSpectrum = new Vector2[num2];
						this.Displacements = new Vector2[num][];
						this.ForceAndHeight = new Vector4[num][];
						this.ResultsTiming = new float[num];
						this.SetDirty();
						this._CachedTime = float.NegativeInfinity;
						for (int i = 0; i < num; i++)
						{
							this.Displacements[i] = new Vector2[num2];
							this.ForceAndHeight[i] = new Vector4[num2];
						}
						if (!this.ResolveByFFT)
						{
							WaterAsynchronousTasks.Instance.AddFFTComputations(this);
							this.ResolveByFFT = true;
						}
					}
				}
				else
				{
					WaterAsynchronousTasks.Instance.RemoveFFTComputations(this);
					this.ResolveByFFT = false;
				}
				return true;
			}
			return false;
		}

		public void GetResults(float time, out Vector2[] da, out Vector2[] db, out Vector4[] fa, out Vector4[] fb, out float p)
		{
			if (this.WindWaves.LoopDuration != 0f)
			{
				time %= this.WindWaves.LoopDuration;
			}
			lock (this)
			{
				if (time == this._CachedTime)
				{
					da = this._CachedDisplacementsA;
					db = this._CachedDisplacementsB;
					fa = this._CachedForceAndHeightA;
					fb = this._CachedForceAndHeightB;
					p = this._CachedTimeProp;
				}
				else
				{
					int recentResultIndex = this.RecentResultIndex;
					for (int i = recentResultIndex - 1; i >= 0; i--)
					{
						if (this.ResultsTiming[i] <= time)
						{
							int num = i + 1;
							da = this.Displacements[i];
							db = this.Displacements[num];
							fa = this.ForceAndHeight[i];
							fb = this.ForceAndHeight[num];
							float num2 = this.ResultsTiming[num] - this.ResultsTiming[i];
							if (num2 != 0f)
							{
								p = (time - this.ResultsTiming[i]) / num2;
							}
							else
							{
								p = 0f;
							}
							if (time > this._CachedTime)
							{
								this._CachedDisplacementsA = da;
								this._CachedDisplacementsB = db;
								this._CachedForceAndHeightA = fa;
								this._CachedForceAndHeightB = fb;
								this._CachedTimeProp = p;
								this._CachedTime = time;
							}
							return;
						}
					}
					for (int j = this.ResultsTiming.Length - 1; j > recentResultIndex; j--)
					{
						if (this.ResultsTiming[j] <= time)
						{
							int num3 = (j == this.Displacements.Length - 1) ? 0 : (j + 1);
							da = this.Displacements[j];
							db = this.Displacements[num3];
							fa = this.ForceAndHeight[j];
							fb = this.ForceAndHeight[num3];
							float num4 = this.ResultsTiming[num3] - this.ResultsTiming[j];
							if (num4 != 0f)
							{
								p = (time - this.ResultsTiming[j]) / num4;
							}
							else
							{
								p = 0f;
							}
							return;
						}
					}
					da = this.Displacements[recentResultIndex];
					db = this.Displacements[recentResultIndex];
					fa = this.ForceAndHeight[recentResultIndex];
					fb = this.ForceAndHeight[recentResultIndex];
					p = 0f;
				}
			}
		}

		public readonly Water Water;

		public readonly WindWaves WindWaves;

		public readonly int TileIndex;

		public Vector2[] DirectionalSpectrum;

		public Vector2[][] Displacements;

		public Vector4[][] ForceAndHeight;

		public float[] ResultsTiming;

		public int RecentResultIndex;

		public bool ResolveByFFT;

		public int DirectionalSpectrumDirty;

		private int _ResolutionFFT;

		private int _MipIndexFFT;

		private float _CachedTime = float.NegativeInfinity;

		private float _CachedTimeProp;

		private Vector2[] _CachedDisplacementsA;

		private Vector2[] _CachedDisplacementsB;

		private Vector4[] _CachedForceAndHeightA;

		private Vector4[] _CachedForceAndHeightB;
	}
}
