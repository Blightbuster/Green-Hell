using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class SpectrumResolver : SpectrumResolverCPU
	{
		public SpectrumResolver(Water water, WindWaves windWaves, Shader spectrumShader) : base(water, windWaves, 4)
		{
			this._Water = water;
			this._WindWaves = windWaves;
			this._AnimationMaterial = new Material(spectrumShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._AnimationMaterial.SetFloat(ShaderVariables.RenderTime, Time.time);
			if (windWaves.LoopDuration != 0f)
			{
				this._AnimationMaterial.EnableKeyword("_LOOPING");
				this._AnimationMaterial.SetFloat("_LoopDuration", windWaves.LoopDuration);
			}
		}

		public Texture TileSizeLookup
		{
			get
			{
				this.ValidateTileSizeLookup();
				return this._TileSizeLookup;
			}
		}

		public float RenderTime { get; private set; }

		public Texture RenderHeightSpectrumAt(float time)
		{
			this.CheckResources();
			RenderTexture rawDirectionalSpectrum = this.GetRawDirectionalSpectrum();
			this.RenderTime = time;
			this._AnimationMaterial.SetFloat(ShaderVariables.RenderTime, time);
			Graphics.Blit(rawDirectionalSpectrum, this._HeightSpectrum, this._AnimationMaterial, 0);
			return this._HeightSpectrum;
		}

		public Texture RenderNormalsSpectrumAt(float time)
		{
			this.CheckResources();
			RenderTexture rawDirectionalSpectrum = this.GetRawDirectionalSpectrum();
			this.RenderTime = time;
			this._AnimationMaterial.SetFloat(ShaderVariables.RenderTime, time);
			Graphics.Blit(rawDirectionalSpectrum, this._NormalSpectrum, this._AnimationMaterial, 1);
			return this._NormalSpectrum;
		}

		public void RenderDisplacementsSpectraAt(float time, out Texture height, out Texture displacement)
		{
			this.CheckResources();
			height = this._HeightSpectrum;
			displacement = this._DisplacementSpectrum;
			this._RenderTargetsx2[0] = this._HeightSpectrum.colorBuffer;
			this._RenderTargetsx2[1] = this._DisplacementSpectrum.colorBuffer;
			RenderTexture rawDirectionalSpectrum = this.GetRawDirectionalSpectrum();
			this.RenderTime = time;
			this._AnimationMaterial.SetFloat(ShaderVariables.RenderTime, time);
			Graphics.SetRenderTarget(this._RenderTargetsx2, this._HeightSpectrum.depthBuffer);
			Graphics.Blit(rawDirectionalSpectrum, this._AnimationMaterial, 5);
			Graphics.SetRenderTarget(null);
		}

		public void RenderCompleteSpectraAt(float time, out Texture height, out Texture normal, out Texture displacement)
		{
			this.CheckResources();
			height = this._HeightSpectrum;
			normal = this._NormalSpectrum;
			displacement = this._DisplacementSpectrum;
			this._RenderTargetsx3[0] = this._HeightSpectrum.colorBuffer;
			this._RenderTargetsx3[1] = this._NormalSpectrum.colorBuffer;
			this._RenderTargetsx3[2] = this._DisplacementSpectrum.colorBuffer;
			RenderTexture rawDirectionalSpectrum = this.GetRawDirectionalSpectrum();
			this.RenderTime = time;
			this._AnimationMaterial.SetFloat(ShaderVariables.RenderTime, time);
			Graphics.SetRenderTarget(this._RenderTargetsx3, this._HeightSpectrum.depthBuffer);
			Graphics.Blit(rawDirectionalSpectrum, this._AnimationMaterial, 2);
			Graphics.SetRenderTarget(null);
		}

		public Texture GetSpectrum(SpectrumResolver.SpectrumType type)
		{
			switch (type)
			{
			case SpectrumResolver.SpectrumType.Height:
				return this._HeightSpectrum;
			case SpectrumResolver.SpectrumType.Normal:
				return this._NormalSpectrum;
			case SpectrumResolver.SpectrumType.Displacement:
				return this._DisplacementSpectrum;
			case SpectrumResolver.SpectrumType.RawDirectional:
				return this._DirectionalSpectrum;
			case SpectrumResolver.SpectrumType.RawOmnidirectional:
				return this._OmnidirectionalSpectrum;
			default:
				throw new InvalidOperationException();
			}
		}

		public override void AddSpectrum(WaterWavesSpectrumDataBase spectrum)
		{
			base.AddSpectrum(spectrum);
			this._DirectionalSpectrumDirty = true;
		}

		public override void RemoveSpectrum(WaterWavesSpectrumDataBase spectrum)
		{
			base.RemoveSpectrum(spectrum);
			this._DirectionalSpectrumDirty = true;
		}

		public override void SetDirectionalSpectrumDirty()
		{
			base.SetDirectionalSpectrumDirty();
			this._DirectionalSpectrumDirty = true;
		}

		internal override void OnProfilesChanged()
		{
			base.OnProfilesChanged();
			if (this._TileSizes != this._WindWaves.TileSizes)
			{
				this._TileSizesLookupDirty = true;
				this._TileSizes = this._WindWaves.TileSizes;
			}
			this.RenderTotalOmnidirectionalSpectrum();
		}

		private void RenderTotalOmnidirectionalSpectrum()
		{
			this._AnimationMaterial.SetFloat("_Gravity", this._Water.Gravity);
			this._AnimationMaterial.SetVector("_TargetResolution", new Vector4((float)this._WindWaves.FinalResolution, (float)this._WindWaves.FinalResolution, 0f, 0f));
			Water.WeightedProfile[] profiles = this._Water.ProfilesManager.Profiles;
			if (profiles.Length > 1)
			{
				RenderTexture totalOmnidirectionalSpectrum = this.GetTotalOmnidirectionalSpectrum();
				totalOmnidirectionalSpectrum.Clear(Color.black, false, true);
				foreach (Water.WeightedProfile weightedProfile in profiles)
				{
					if (weightedProfile.Weight > 0.0001f)
					{
						WaterWavesSpectrum spectrum = weightedProfile.Profile.Spectrum;
						WaterWavesSpectrumData spectrumData;
						if (!this._SpectraDataCache.TryGetValue(spectrum, out spectrumData))
						{
							spectrumData = base.GetSpectrumData(spectrum);
						}
						this._AnimationMaterial.SetFloat("_Weight", spectrumData.Weight);
						Graphics.Blit(spectrumData.Texture, totalOmnidirectionalSpectrum, this._AnimationMaterial, 4);
					}
				}
				this._OmnidirectionalSpectrum = totalOmnidirectionalSpectrum;
			}
			else if (profiles.Length != 0)
			{
				WaterWavesSpectrum spectrum2 = profiles[0].Profile.Spectrum;
				WaterWavesSpectrumData spectrumData2;
				if (!this._SpectraDataCache.TryGetValue(spectrum2, out spectrumData2))
				{
					spectrumData2 = base.GetSpectrumData(spectrum2);
				}
				spectrumData2.Weight = 1f;
				this._OmnidirectionalSpectrum = spectrumData2.Texture;
			}
			this._Water.Renderer.PropertyBlock.SetFloat("_MaxDisplacement", base.MaxHorizontalDisplacement);
		}

		private void RenderDirectionalSpectrum()
		{
			if (this._OmnidirectionalSpectrum == null)
			{
				this.RenderTotalOmnidirectionalSpectrum();
			}
			this.ValidateTileSizeLookup();
			this._AnimationMaterial.SetFloat("_Directionality", 1f - this._WindWaves.SpectrumDirectionality);
			this._AnimationMaterial.SetVector("_WindDirection", base.WindDirection);
			this._AnimationMaterial.SetTexture("_TileSizeLookup", this._TileSizeLookup);
			Graphics.Blit(this._OmnidirectionalSpectrum, this._DirectionalSpectrum, this._AnimationMaterial, 3);
			this.AddOverlayToDirectionalSpectrum();
			this._DirectionalSpectrumDirty = false;
		}

		private void AddOverlayToDirectionalSpectrum()
		{
			if (this._SpectrumDownsamplingMesh == null)
			{
				this._SpectrumDownsamplingMesh = SpectrumResolver.CreateDownsamplingMesh();
			}
			for (int i = this._OverlayedSpectra.Count - 1; i >= 0; i--)
			{
				WaterWavesSpectrumDataBase waterWavesSpectrumDataBase = this._OverlayedSpectra[i];
				Texture2D texture = waterWavesSpectrumDataBase.Texture;
				this._AnimationMaterial.SetFloat("_Weight", waterWavesSpectrumDataBase.Weight);
				this._AnimationMaterial.SetVector("_WindDirection", waterWavesSpectrumDataBase.WindDirection);
				float weatherSystemRadius = waterWavesSpectrumDataBase.WeatherSystemRadius;
				this._AnimationMaterial.SetVector("_WeatherSystemRadius", new Vector4(2f * weatherSystemRadius, weatherSystemRadius * weatherSystemRadius, 0f, 0f));
				Vector2 weatherSystemOffset = waterWavesSpectrumDataBase.WeatherSystemOffset;
				this._AnimationMaterial.SetVector("_WeatherSystemOffset", new Vector4(weatherSystemOffset.x, weatherSystemOffset.y, weatherSystemOffset.magnitude, 0f));
				Graphics.Blit(texture, this._DirectionalSpectrum, this._AnimationMaterial, 6);
			}
		}

		internal RenderTexture GetRawDirectionalSpectrum()
		{
			if ((this._DirectionalSpectrumDirty || !this._DirectionalSpectrum.IsCreated()) && Application.isPlaying)
			{
				this.CheckResources();
				this.RenderDirectionalSpectrum();
			}
			return this._DirectionalSpectrum;
		}

		private RenderTexture GetTotalOmnidirectionalSpectrum()
		{
			if (this._TotalOmnidirectionalSpectrum == null)
			{
				int num = this._WindWaves.FinalResolution << 1;
				this._TotalOmnidirectionalSpectrum = new RenderTexture(num, num, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] SpectrumResolver - Total Omnidirectional Spectrum",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Point,
					wrapMode = TextureWrapMode.Repeat
				};
			}
			return this._TotalOmnidirectionalSpectrum;
		}

		private void CheckResources()
		{
			if (this._HeightSpectrum == null)
			{
				int num = this._WindWaves.FinalResolution << 1;
				bool finalHighPrecision = this._WindWaves.FinalHighPrecision;
				this._HeightSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] SpectrumResolver - Water Height Spectrum",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Point
				};
				this._NormalSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] SpectrumResolver - Water Normals Spectrum",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Point
				};
				this._DisplacementSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] SpectrumResolver - Water Displacement Spectrum",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Point
				};
				this._DirectionalSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear)
				{
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Point,
					wrapMode = TextureWrapMode.Clamp
				};
				this._RenderTargetsx2 = new RenderBuffer[]
				{
					this._HeightSpectrum.colorBuffer,
					this._DisplacementSpectrum.colorBuffer
				};
				this._RenderTargetsx3 = new RenderBuffer[]
				{
					this._HeightSpectrum.colorBuffer,
					this._NormalSpectrum.colorBuffer,
					this._DisplacementSpectrum.colorBuffer
				};
			}
		}

		internal override void OnMapsFormatChanged(bool resolution)
		{
			base.OnMapsFormatChanged(resolution);
			if (this._TotalOmnidirectionalSpectrum != null)
			{
				UnityEngine.Object.Destroy(this._TotalOmnidirectionalSpectrum);
				this._TotalOmnidirectionalSpectrum = null;
			}
			if (this._HeightSpectrum != null)
			{
				UnityEngine.Object.Destroy(this._HeightSpectrum);
				this._HeightSpectrum = null;
			}
			if (this._NormalSpectrum != null)
			{
				UnityEngine.Object.Destroy(this._NormalSpectrum);
				this._NormalSpectrum = null;
			}
			if (this._DisplacementSpectrum != null)
			{
				UnityEngine.Object.Destroy(this._DisplacementSpectrum);
				this._DisplacementSpectrum = null;
			}
			if (this._DirectionalSpectrum != null)
			{
				UnityEngine.Object.Destroy(this._DirectionalSpectrum);
				this._DirectionalSpectrum = null;
			}
			if (this._TileSizeLookup != null)
			{
				UnityEngine.Object.Destroy(this._TileSizeLookup);
				this._TileSizeLookup = null;
				this._TileSizesLookupDirty = true;
			}
			this._OmnidirectionalSpectrum = null;
			this._RenderTargetsx2 = null;
			this._RenderTargetsx3 = null;
		}

		private void ValidateTileSizeLookup()
		{
			if (this._TileSizesLookupDirty)
			{
				if (this._TileSizeLookup == null)
				{
					this._TileSizeLookup = new Texture2D(2, 2, (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat)) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat, false, true)
					{
						hideFlags = HideFlags.DontSave,
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Point
					};
				}
				this._TileSizeLookup.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f / this._TileSizes.x, 0f));
				this._TileSizeLookup.SetPixel(1, 0, new Color(1.5f, 0.5f, 1f / this._TileSizes.y, 0f));
				this._TileSizeLookup.SetPixel(0, 1, new Color(0.5f, 1.5f, 1f / this._TileSizes.z, 0f));
				this._TileSizeLookup.SetPixel(1, 1, new Color(1.5f, 1.5f, 1f / this._TileSizes.w, 0f));
				this._TileSizeLookup.Apply(false, false);
				this._TileSizesLookupDirty = false;
			}
		}

		private static void AddQuad(Vector3[] vertices, Vector3[] origins, Vector2[] uvs, int index, float xOffset, float yOffset, int originIndex)
		{
			originIndex += index;
			float num = xOffset * 2f - 1f;
			float num2 = yOffset * 2f - 1f;
			uvs[index] = new Vector2(xOffset, yOffset);
			vertices[index++] = new Vector3(num, num2, 0.1f);
			uvs[index] = new Vector2(xOffset, yOffset + 0.25f);
			vertices[index++] = new Vector3(num, num2 + 0.5f, 0.1f);
			uvs[index] = new Vector2(xOffset + 0.25f, yOffset + 0.25f);
			vertices[index++] = new Vector3(num + 0.5f, num2 + 0.5f, 0.1f);
			uvs[index] = new Vector2(xOffset + 0.25f, yOffset);
			vertices[index] = new Vector3(num + 0.5f, num2, 0.1f);
			origins[index--] = vertices[originIndex];
			origins[index--] = vertices[originIndex];
			origins[index--] = vertices[originIndex];
			origins[index] = vertices[originIndex];
		}

		private static void AddQuads(Vector3[] vertices, Vector3[] origins, Vector2[] uvs, int index, float xOffset, float yOffset)
		{
			SpectrumResolver.AddQuad(vertices, origins, uvs, index, xOffset, yOffset, 0);
			SpectrumResolver.AddQuad(vertices, origins, uvs, index + 4, xOffset + 0.25f, yOffset, 3);
			SpectrumResolver.AddQuad(vertices, origins, uvs, index + 8, xOffset, yOffset + 0.25f, 1);
			SpectrumResolver.AddQuad(vertices, origins, uvs, index + 12, xOffset + 0.25f, yOffset + 0.25f, 2);
		}

		private static Mesh CreateDownsamplingMesh()
		{
			Mesh mesh = new Mesh
			{
				name = "[PW Water] Spectrum Downsampling Mesh"
			};
			Vector3[] vertices = new Vector3[64];
			Vector3[] array = new Vector3[64];
			Vector2[] array2 = new Vector2[64];
			int[] array3 = new int[64];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = i;
			}
			SpectrumResolver.AddQuads(vertices, array, array2, 0, 0f, 0f);
			SpectrumResolver.AddQuads(vertices, array, array2, 16, 0.5f, 0f);
			SpectrumResolver.AddQuads(vertices, array, array2, 32, 0f, 0.5f);
			SpectrumResolver.AddQuads(vertices, array, array2, 48, 0.5f, 0.5f);
			mesh.vertices = vertices;
			mesh.normals = array;
			mesh.uv = array2;
			mesh.SetIndices(array3, MeshTopology.Quads, 0);
			return mesh;
		}

		private Texture2D _TileSizeLookup;

		private Texture _OmnidirectionalSpectrum;

		private RenderTexture _TotalOmnidirectionalSpectrum;

		private RenderTexture _DirectionalSpectrum;

		private RenderTexture _HeightSpectrum;

		private RenderTexture _NormalSpectrum;

		private RenderTexture _DisplacementSpectrum;

		private RenderBuffer[] _RenderTargetsx2;

		private RenderBuffer[] _RenderTargetsx3;

		private bool _TileSizesLookupDirty = true;

		private bool _DirectionalSpectrumDirty = true;

		private Vector4 _TileSizes;

		private Mesh _SpectrumDownsamplingMesh;

		private readonly Material _AnimationMaterial;

		private readonly Water _Water;

		private readonly WindWaves _WindWaves;

		public enum SpectrumType
		{
			Height,
			Normal,
			Displacement,
			RawDirectional,
			RawOmnidirectional
		}
	}
}
