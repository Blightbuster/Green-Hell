using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace UltimateWater.Internal
{
	public sealed class WaterAsynchronousTasks : MonoBehaviour
	{
		public static WaterAsynchronousTasks Instance
		{
			get
			{
				if (WaterAsynchronousTasks._Instance == null)
				{
					WaterAsynchronousTasks._Instance = UnityEngine.Object.FindObjectOfType<WaterAsynchronousTasks>();
					if (WaterAsynchronousTasks._Instance == null)
					{
						GameObject gameObject = new GameObject("Ultimate Water Spectrum Sampler")
						{
							hideFlags = HideFlags.HideInHierarchy
						};
						WaterAsynchronousTasks._Instance = gameObject.AddComponent<WaterAsynchronousTasks>();
					}
				}
				return WaterAsynchronousTasks._Instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return WaterAsynchronousTasks._Instance != null;
			}
		}

		public void AddWaterSampleComputations(WaterSample computation)
		{
			object computations = this._Computations;
			lock (computations)
			{
				this._Computations.Add(computation);
			}
		}

		public void RemoveWaterSampleComputations(WaterSample computation)
		{
			object computations = this._Computations;
			lock (computations)
			{
				int num = this._Computations.IndexOf(computation);
				if (num != -1)
				{
					if (num < this._ComputationIndex)
					{
						this._ComputationIndex--;
					}
					this._Computations.RemoveAt(num);
				}
			}
		}

		public void AddFFTComputations(WaterTileSpectrum scale)
		{
			object fftspectra = this._FFTSpectra;
			lock (fftspectra)
			{
				this._FFTSpectra.Add(scale);
			}
		}

		public void RemoveFFTComputations(WaterTileSpectrum scale)
		{
			object fftspectra = this._FFTSpectra;
			lock (fftspectra)
			{
				int num = this._FFTSpectra.IndexOf(scale);
				if (num != -1)
				{
					if (num < this._FFTSpectrumIndex)
					{
						this._FFTSpectrumIndex--;
					}
					this._FFTSpectra.RemoveAt(num);
				}
			}
		}

		private void Awake()
		{
			this._Run = true;
			if (WaterAsynchronousTasks._Instance == null)
			{
				WaterAsynchronousTasks._Instance = this;
			}
			else if (WaterAsynchronousTasks._Instance != this)
			{
				base.gameObject.Destroy();
				return;
			}
			if (!Application.isPlaying)
			{
				return;
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			for (int i = 0; i < WaterProjectSettings.Instance.PhysicsThreads; i++)
			{
				Thread thread = new Thread(new ThreadStart(this.RunSamplingTask))
				{
					Priority = WaterProjectSettings.Instance.PhysicsThreadsPriority
				};
				thread.Start();
			}
			Thread thread2 = new Thread(new ThreadStart(this.RunFFTTask))
			{
				Priority = WaterProjectSettings.Instance.PhysicsThreadsPriority
			};
			thread2.Start();
		}

		private void OnDisable()
		{
			this._Run = false;
			if (this._ThreadException != null)
			{
				UnityEngine.Debug.LogException(this._ThreadException);
			}
		}

		private void RunSamplingTask()
		{
			try
			{
				while (this._Run)
				{
					WaterSample waterSample = null;
					object computations = this._Computations;
					lock (computations)
					{
						if (this._Computations.Count != 0)
						{
							if (this._ComputationIndex >= this._Computations.Count)
							{
								this._ComputationIndex = 0;
							}
							waterSample = this._Computations[this._ComputationIndex++];
						}
					}
					if (waterSample == null)
					{
						Thread.Sleep(2);
					}
					else
					{
						object obj = waterSample;
						lock (obj)
						{
							waterSample.ComputationStep(false);
						}
					}
				}
			}
			catch (Exception threadException)
			{
				this._ThreadException = threadException;
			}
		}

		private void RunFFTTask()
		{
			try
			{
				CpuFFT cpuFFT = new CpuFFT();
				Stopwatch stopwatch = new Stopwatch();
				bool flag = false;
				while (this._Run)
				{
					WaterTileSpectrum waterTileSpectrum = null;
					object fftspectra = this._FFTSpectra;
					lock (fftspectra)
					{
						if (this._FFTSpectra.Count != 0)
						{
							if (this._FFTSpectrumIndex >= this._FFTSpectra.Count)
							{
								this._FFTSpectrumIndex = 0;
							}
							if (this._FFTSpectrumIndex == 0)
							{
								if ((float)stopwatch.ElapsedMilliseconds > this._FFTTimeStep * 900f)
								{
									if (flag)
									{
										this._FFTTimeStep += 0.05f;
									}
									else
									{
										flag = true;
									}
								}
								else
								{
									flag = false;
									if (this._FFTTimeStep > 0.2f)
									{
										this._FFTTimeStep -= 0.001f;
									}
								}
								stopwatch.Reset();
								stopwatch.Start();
							}
							waterTileSpectrum = this._FFTSpectra[this._FFTSpectrumIndex++];
						}
					}
					if (waterTileSpectrum == null)
					{
						stopwatch.Reset();
						Thread.Sleep(6);
					}
					else
					{
						bool flag2 = false;
						SpectrumResolver spectrumResolver = waterTileSpectrum.WindWaves.SpectrumResolver;
						if (spectrumResolver != null)
						{
							int recentResultIndex = waterTileSpectrum.RecentResultIndex;
							int num = (recentResultIndex + 2) % waterTileSpectrum.ResultsTiming.Length;
							int num2 = (recentResultIndex + 1) % waterTileSpectrum.ResultsTiming.Length;
							float num3 = waterTileSpectrum.ResultsTiming[recentResultIndex];
							float num4 = waterTileSpectrum.ResultsTiming[num];
							float num5 = waterTileSpectrum.ResultsTiming[num2];
							float lastFrameTime = spectrumResolver.LastFrameTime;
							if (num4 <= lastFrameTime || num5 > lastFrameTime)
							{
								float loopDuration = waterTileSpectrum.WindWaves.LoopDuration;
								float num6;
								if (loopDuration != 0f)
								{
									num6 = Mathf.Round((num3 % loopDuration + 0.2f) / 0.2f) * 0.2f;
								}
								else if (num5 > lastFrameTime)
								{
									num6 = lastFrameTime + this._FFTTimeStep;
								}
								else
								{
									num6 = Mathf.Max(num3, lastFrameTime) + this._FFTTimeStep;
								}
								if (num6 != num5)
								{
									cpuFFT.Compute(waterTileSpectrum, num6, num2);
									waterTileSpectrum.ResultsTiming[num2] = num6;
									flag2 = true;
								}
								waterTileSpectrum.RecentResultIndex = num2;
							}
							if (!flag2)
							{
								stopwatch.Reset();
								Thread.Sleep(3);
							}
						}
					}
				}
			}
			catch (Exception threadException)
			{
				this._ThreadException = threadException;
			}
		}

		private static WaterAsynchronousTasks _Instance;

		private bool _Run;

		private readonly List<WaterTileSpectrum> _FFTSpectra = new List<WaterTileSpectrum>();

		private int _FFTSpectrumIndex;

		private float _FFTTimeStep = 0.2f;

		private readonly List<WaterSample> _Computations = new List<WaterSample>();

		private int _ComputationIndex;

		private Exception _ThreadException;
	}
}
