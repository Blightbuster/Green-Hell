using System;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	public sealed class WavesRendererGerstner
	{
		public WavesRendererGerstner(Water water, WindWaves windWaves, WavesRendererGerstner.Data data)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._Data = data;
		}

		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
		}

		public void OnWaterRender(Camera camera)
		{
			if (!Application.isPlaying || !this._Enabled)
			{
				return;
			}
			this.UpdateWaves();
		}

		public void OnWaterPostRender(Camera camera)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WindWaves windWaves, WaterQualityLevel qualityLevel)
		{
			variant.SetUnityKeyword("_WAVES_GERSTNER", this._Enabled);
		}

		private void UpdateWaves()
		{
			int frameCount = Time.frameCount;
			if (this._LastUpdateFrame == frameCount)
			{
				return;
			}
			this._LastUpdateFrame = frameCount;
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			float time = Time.time;
			for (int i = 0; i < this._GerstnerFours.Length; i++)
			{
				Gerstner4 gerstner = this._GerstnerFours[i];
				Vector4 value;
				value.x = gerstner.Wave0.Offset + gerstner.Wave0.Speed * time;
				value.y = gerstner.Wave1.Offset + gerstner.Wave1.Speed * time;
				value.z = gerstner.Wave2.Offset + gerstner.Wave2.Speed * time;
				value.w = gerstner.Wave3.Offset + gerstner.Wave3.Speed * time;
				propertyBlock.SetVector("_GrOff" + i, value);
			}
		}

		internal void Enable()
		{
			if (this._Enabled)
			{
				return;
			}
			this._Enabled = true;
			if (Application.isPlaying)
			{
				this._Water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
				this.FindMostMeaningfulWaves();
			}
		}

		internal void Disable()
		{
			if (!this._Enabled)
			{
				return;
			}
			this._Enabled = false;
		}

		internal void OnValidate(WindWaves windWaves)
		{
			if (this._Enabled)
			{
				this.FindMostMeaningfulWaves();
			}
		}

		private void FindMostMeaningfulWaves()
		{
			this._WindWaves.SpectrumResolver.SetDirectWaveEvaluationMode(this._Data.NumGerstners);
			WaterWave[] directWaves = this._WindWaves.SpectrumResolver.DirectWaves;
			int num = 0;
			int num2 = directWaves.Length >> 2;
			this._GerstnerFours = new Gerstner4[num2];
			Vector2[] array = new Vector2[4];
			for (int i = 0; i < 4; i++)
			{
				float num3 = this._WindWaves.TileSizes[i];
				array[i].x = num3 + 0.5f / (float)this._WindWaves.FinalResolution * num3;
				array[i].y = -num3 + 0.5f / (float)this._WindWaves.FinalResolution * num3;
			}
			for (int j = 0; j < num2; j++)
			{
				GerstnerWave wave = (num >= directWaves.Length) ? new GerstnerWave() : new GerstnerWave(directWaves[num++], array);
				GerstnerWave wave2 = (num >= directWaves.Length) ? new GerstnerWave() : new GerstnerWave(directWaves[num++], array);
				GerstnerWave wave3 = (num >= directWaves.Length) ? new GerstnerWave() : new GerstnerWave(directWaves[num++], array);
				GerstnerWave wave4 = (num >= directWaves.Length) ? new GerstnerWave() : new GerstnerWave(directWaves[num++], array);
				this._GerstnerFours[j] = new Gerstner4(wave, wave2, wave3, wave4);
			}
			this.UpdateMaterial();
		}

		private void UpdateMaterial()
		{
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			for (int i = 0; i < this._GerstnerFours.Length; i++)
			{
				Gerstner4 gerstner = this._GerstnerFours[i];
				Vector4 value;
				value.x = gerstner.Wave0.Amplitude;
				Vector4 value2;
				value2.x = gerstner.Wave0.Frequency;
				Vector4 value3;
				value3.x = gerstner.Wave0.Direction.x;
				value3.y = gerstner.Wave0.Direction.y;
				value.y = gerstner.Wave1.Amplitude;
				value2.y = gerstner.Wave1.Frequency;
				value3.z = gerstner.Wave1.Direction.x;
				value3.w = gerstner.Wave1.Direction.y;
				value.z = gerstner.Wave2.Amplitude;
				value2.z = gerstner.Wave2.Frequency;
				Vector4 value4;
				value4.x = gerstner.Wave2.Direction.x;
				value4.y = gerstner.Wave2.Direction.y;
				value.w = gerstner.Wave3.Amplitude;
				value2.w = gerstner.Wave3.Frequency;
				value4.z = gerstner.Wave3.Direction.x;
				value4.w = gerstner.Wave3.Direction.y;
				propertyBlock.SetVector("_GrAB" + i, value3);
				propertyBlock.SetVector("_GrCD" + i, value4);
				propertyBlock.SetVector("_GrAmp" + i, value);
				propertyBlock.SetVector("_GrFrq" + i, value2);
			}
			for (int j = this._GerstnerFours.Length; j < 5; j++)
			{
				propertyBlock.SetVector("_GrAmp" + j, Vector4.zero);
			}
		}

		private void OnProfilesChanged(Water water)
		{
			this.FindMostMeaningfulWaves();
		}

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		private readonly WavesRendererGerstner.Data _Data;

		private Gerstner4[] _GerstnerFours;

		private int _LastUpdateFrame;

		private bool _Enabled;

		[Serializable]
		public class Data
		{
			[Range(0f, 20f)]
			public int NumGerstners = 20;
		}
	}
}
