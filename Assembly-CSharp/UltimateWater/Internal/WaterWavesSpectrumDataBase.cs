using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UltimateWater.Internal
{
	public abstract class WaterWavesSpectrumDataBase
	{
		protected WaterWavesSpectrumDataBase(Water water, WindWaves windWaves, float tileSize, float gravity)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._TileSize = tileSize;
			this._Gravity = gravity;
		}

		public WaterWave[] CpuWaves { get; private set; }

		public WaterWave[] ShorelineCandidates { get; private set; }

		public Vector3[][] SpectrumValues { get; private set; }

		public Texture2D Texture
		{
			get
			{
				if (this._Texture == null)
				{
					this.CreateSpectrumTexture();
				}
				return this._Texture;
			}
		}

		public float Weight { get; set; }

		public float Gravity
		{
			get
			{
				return this._Gravity;
			}
		}

		public Vector2 WeatherSystemOffset { get; set; }

		public float WeatherSystemRadius { get; set; }

		public Vector2 WindDirection
		{
			get
			{
				return this._WindDirection;
			}
			set
			{
				this._WindDirection = value;
			}
		}

		public static int GetMipIndex(int i)
		{
			if (i == 0)
			{
				return 0;
			}
			int num = (int)Mathf.Log((float)i, 2f) - 3;
			return (num < 0) ? 0 : num;
		}

		public float GetStandardDeviation()
		{
			return this._StdDev;
		}

		public float GetStandardDeviation(int scaleIndex, int mipLevel)
		{
			float[] array = this._StandardDeviationData[scaleIndex];
			return (mipLevel >= array.Length) ? 0f : array[mipLevel];
		}

		public void SetCpuWavesDirty()
		{
			this._CpuWavesDirty = true;
		}

		public void ValidateSpectrumData()
		{
			if (this.CpuWaves != null)
			{
				return;
			}
			lock (this)
			{
				if (this.CpuWaves == null)
				{
					if (this.SpectrumValues == null)
					{
						this.SpectrumValues = new Vector3[4][];
						this._StandardDeviationData = new float[4][];
					}
					int finalResolution = this._WindWaves.FinalResolution;
					int num = finalResolution * finalResolution;
					int num2 = Mathf.RoundToInt(Mathf.Log((float)finalResolution, 2f)) - 4;
					if (this.SpectrumValues[0] == null || this.SpectrumValues[0].Length != num)
					{
						for (int i = 0; i < 4; i++)
						{
							this.SpectrumValues[i] = new Vector3[num];
							this._StandardDeviationData[i] = new float[num2 + 1];
						}
					}
					this.GenerateContents(this.SpectrumValues);
					this.AnalyzeSpectrum();
				}
			}
		}

		public void UpdateSpectralValues(Vector2 windDirection, float directionality)
		{
			this.ValidateSpectrumData();
			if (this._CpuWavesDirty)
			{
				lock (this)
				{
					if (this.CpuWaves != null && this._CpuWavesDirty)
					{
						object cpuWaves = this.CpuWaves;
						lock (cpuWaves)
						{
							this._CpuWavesDirty = false;
							float directionalityInv = 1f - directionality;
							float horizontalDisplacementScale = this._Water.Materials.HorizontalDisplacementScale;
							int finalResolution = this._WindWaves.FinalResolution;
							bool mostlySorted = Vector2.Dot(this._LastWindDirection, windDirection) >= 0.97f;
							WaterWave[] cpuWaves2 = this.CpuWaves;
							for (int i = 0; i < cpuWaves2.Length; i++)
							{
								cpuWaves2[i].UpdateSpectralValues(this.SpectrumValues, windDirection, directionalityInv, finalResolution, horizontalDisplacementScale);
							}
							WaterWavesSpectrumDataBase.SortCpuWaves(cpuWaves2, mostlySorted);
							WaterWave[] shorelineCandidates = this.ShorelineCandidates;
							for (int j = 0; j < shorelineCandidates.Length; j++)
							{
								shorelineCandidates[j].UpdateSpectralValues(this.SpectrumValues, windDirection, directionalityInv, finalResolution, horizontalDisplacementScale);
							}
							this._LastWindDirection = windDirection;
						}
					}
				}
			}
		}

		public static void SortCpuWaves(WaterWave[] windWaves, bool mostlySorted)
		{
			if (!mostlySorted)
			{
				Array.Sort<WaterWave>(windWaves, delegate(WaterWave a, WaterWave b)
				{
					if (a._CPUPriority > b._CPUPriority)
					{
						return -1;
					}
					return (a._CPUPriority != b._CPUPriority) ? 1 : 0;
				});
			}
			else
			{
				int num = windWaves.Length;
				int num2 = 0;
				for (int i = 1; i < num; i++)
				{
					if (windWaves[num2]._CPUPriority < windWaves[i]._CPUPriority)
					{
						WaterWave waterWave = windWaves[num2];
						windWaves[num2] = windWaves[i];
						windWaves[i] = waterWave;
						if (i != 1)
						{
							i -= 2;
						}
					}
					num2 = i;
				}
			}
		}

		public void Dispose(bool onlyTexture)
		{
			if (this._Texture != null)
			{
				this._Texture.Destroy();
				this._Texture = null;
			}
			if (!onlyTexture)
			{
				lock (this)
				{
					this.SpectrumValues = null;
					this.CpuWaves = null;
					this._CpuWavesDirty = true;
				}
			}
		}

		private void CreateSpectrumTexture()
		{
			this.ValidateSpectrumData();
			int resolution = this._WindWaves.FinalResolution;
			int width = resolution << 1;
			int num = resolution << 1;
			if (WaterWavesSpectrumDataBase._Colors.Length < width * num)
			{
				WaterWavesSpectrumDataBase._Colors = new Color[width * num];
			}
			Thread[] array = new Thread[4];
			for (int i = 0; i < 4; i++)
			{
				int index = i;
				array[i] = new Thread(delegate
				{
					this.Calculate(index, resolution, width);
				});
			}
			for (int j = 0; j < 4; j++)
			{
				array[j].Start();
			}
			this._Texture = new Texture2D(width, num, TextureFormat.RGBAFloat, false, true)
			{
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Repeat
			};
			for (int k = 0; k < 4; k++)
			{
				array[k].Join();
			}
			this._Texture.SetPixels(0, 0, width, num, WaterWavesSpectrumDataBase._Colors);
			this._Texture.Apply(false, true);
		}

		private void Calculate(int scaleIndex, int resolution, int width)
		{
			Vector3[] array = this.SpectrumValues[scaleIndex];
			int num = ((scaleIndex & 1) != 0) ? resolution : 0;
			int num2 = ((scaleIndex & 2) != 0) ? resolution : 0;
			int num3 = num2 * width + num;
			for (int i = 0; i < resolution; i++)
			{
				int num4 = i * resolution;
				int num5 = i * width + num3;
				for (int j = 0; j < resolution; j++)
				{
					Vector3 vector = array[num4 + j];
					int num6 = num5 + j;
					WaterWavesSpectrumDataBase._Colors[num6].r = vector.x;
					WaterWavesSpectrumDataBase._Colors[num6].g = vector.y;
					WaterWavesSpectrumDataBase._Colors[num6].b = vector.z;
				}
			}
		}

		private void AnalyzeSpectrum()
		{
			int finalResolution = this._WindWaves.FinalResolution;
			int num = finalResolution >> 1;
			int num2 = Mathf.RoundToInt(Mathf.Log((float)(finalResolution >> 1), 2f)) - 4;
			Heap<WaterWave> heap = new Heap<WaterWave>();
			Heap<WaterWave> heap2 = new Heap<WaterWave>();
			this._StdDev = 0f;
			for (byte b = 0; b < 4; b += 1)
			{
				Vector3[] array = this.SpectrumValues[(int)b];
				float[] array2 = this._StandardDeviationData[(int)b] = new float[num2 + 1];
				float num3 = 6.28318548f / this._TileSize;
				float offsetX = this._TileSize + 0.5f / (float)finalResolution * this._TileSize;
				float offsetZ = -this._TileSize + 0.5f / (float)finalResolution * this._TileSize;
				for (int i = 0; i < finalResolution; i++)
				{
					float num4 = num3 * (float)(i - num);
					ushort num5 = (ushort)((i + num) % finalResolution);
					ushort num6 = (ushort)((int)num5 * finalResolution);
					for (int j = 0; j < finalResolution; j++)
					{
						float num7 = num3 * (float)(j - num);
						ushort num8 = (ushort)((j + num) % finalResolution);
						Vector3 vector = array[(int)(num6 + num8)];
						float num9 = vector.x * vector.x + vector.y * vector.y;
						float num10 = Mathf.Sqrt(num9);
						float num11 = Mathf.Sqrt(num4 * num4 + num7 * num7);
						float w = Mathf.Sqrt(this._Gravity * num11);
						if (num10 >= 0.0025f)
						{
							heap2.Insert(new WaterWave(b, offsetX, offsetZ, num5, num8, num4, num7, num11, w, num10));
							if (heap2.Count > 100)
							{
								heap2.ExtractMax();
							}
						}
						if (num10 > 0.025f)
						{
							heap.Insert(new WaterWave(b, offsetX, offsetZ, num5, num8, num4, num7, num11, w, num10));
							if (heap.Count > 200)
							{
								heap.ExtractMax();
							}
						}
						int mipIndex = WaterWavesSpectrumDataBase.GetMipIndex(Mathf.Max(Mathf.Min((int)num5, finalResolution - (int)num5 - 1), Mathf.Min((int)num8, finalResolution - (int)num8 - 1)));
						array2[mipIndex] += num9;
					}
				}
				for (int k = 0; k < array2.Length; k++)
				{
					array2[k] = Mathf.Sqrt(2f * array2[k]);
					this._StdDev += array2[k];
				}
			}
			this.CpuWaves = heap2.ToArray<WaterWave>();
			WaterWavesSpectrumDataBase.SortCpuWaves(this.CpuWaves, false);
			this.ShorelineCandidates = heap.ToArray<WaterWave>();
			Array.Sort<WaterWave>(this.ShorelineCandidates);
		}

		protected abstract void GenerateContents(Vector3[][] spectrumValues);

		private float[][] _StandardDeviationData;

		private bool _CpuWavesDirty = true;

		private Vector2 _LastWindDirection;

		private float _StdDev;

		private Vector2 _WindDirection = new Vector2(1f, 0f);

		private Texture2D _Texture;

		private readonly float _TileSize;

		private readonly float _Gravity;

		protected readonly Water _Water;

		protected readonly WindWaves _WindWaves;

		private static Color[] _Colors = new Color[262144];
	}
}
