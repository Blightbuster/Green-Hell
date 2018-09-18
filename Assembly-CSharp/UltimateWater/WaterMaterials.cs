using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	[Serializable]
	public class WaterMaterials
	{
		public Material SurfaceMaterial { get; private set; }

		public Material SurfaceBackMaterial { get; private set; }

		public Material VolumeMaterial { get; private set; }

		public Material VolumeBackMaterial { get; private set; }

		public Texture2D UnderwaterAbsorptionColorByDepth
		{
			get
			{
				if (this._AbsorptionGradientDirty)
				{
					this.ComputeAbsorptionGradient();
				}
				return this._AbsorptionGradient;
			}
		}

		public float HorizontalDisplacementScale { get; private set; }

		public string UsedKeywords { get; private set; }

		public float UnderwaterBlurSize { get; private set; }

		public float UnderwaterLightFadeScale { get; private set; }

		public float UnderwaterDistortionsIntensity { get; private set; }

		public float UnderwaterDistortionAnimationSpeed { get; private set; }

		public void SetKeyword(string keyword, bool enable)
		{
			if (enable)
			{
				this.SurfaceMaterial.EnableKeyword(keyword);
				this.SurfaceBackMaterial.EnableKeyword(keyword);
				this.VolumeMaterial.EnableKeyword(keyword);
				this.VolumeBackMaterial.EnableKeyword(keyword);
			}
			else
			{
				this.SurfaceMaterial.DisableKeyword(keyword);
				this.SurfaceBackMaterial.DisableKeyword(keyword);
				this.VolumeMaterial.DisableKeyword(keyword);
				this.VolumeBackMaterial.DisableKeyword(keyword);
			}
		}

		public void UpdateGlobalLookupTex()
		{
			int waterId = this._Water.WaterId;
			if (WaterMaterials._GlobalWaterLookupTex == null || waterId == -1)
			{
				return;
			}
			if (waterId >= 256)
			{
				Debug.LogError("There is more than 256 water objects in the scene. This is not supported in deferred water render mode. You have to switch to WaterCameraSimple.");
				return;
			}
			Color parameterValue = this.GetParameterValue(WaterMaterials.ColorParameter.AbsorptionColor);
			parameterValue.a = this._Water.Renderer.PropertyBlock.GetFloat(WaterMaterials._ParameterHashes[10]);
			WaterMaterials._GlobalWaterLookupTex.SetPixel(waterId, 0, parameterValue);
			Color parameterValue2 = this.GetParameterValue(WaterMaterials.ColorParameter.ReflectionColor);
			parameterValue2.a = this._ForwardScatteringIntensity;
			WaterMaterials._GlobalWaterLookupTex.SetPixel(waterId, 1, parameterValue2);
			Vector2 surfaceOffset = this._Water.SurfaceOffset;
			WaterMaterials._GlobalWaterLookupTex.SetPixel(waterId, 2, new Color(surfaceOffset.x, this._Water.transform.position.y, surfaceOffset.y, this._DirectionalLightsIntensity));
			Color parameterValue3 = this.GetParameterValue(WaterMaterials.ColorParameter.DiffuseColor);
			parameterValue3.a = this._Smoothness / this._AmbientSmoothness;
			WaterMaterials._GlobalWaterLookupTex.SetPixel(waterId, 3, parameterValue3);
			WaterMaterials._GlobalLookupDirty = true;
		}

		public static void ValidateGlobalWaterDataLookupTex()
		{
			if (WaterMaterials._GlobalWaterLookupTex == null)
			{
				WaterMaterials.CreateGlobalWaterDataLookupTex();
				WaterMaterials._GlobalLookupDirty = false;
			}
			else if (WaterMaterials._GlobalLookupDirty)
			{
				WaterMaterials._GlobalWaterLookupTex.Apply(false, false);
				WaterMaterials._GlobalLookupDirty = false;
				Shader.SetGlobalTexture("_GlobalWaterLookupTex", WaterMaterials._GlobalWaterLookupTex);
			}
		}

		public void UpdateSurfaceMaterial()
		{
			WaterQualityLevel currentQualityLevel = WaterQualitySettings.Instance.CurrentQualityLevel;
			this.SurfaceMaterial.SetFloat(ShaderVariables.Cull, 2f);
			float b = Mathf.Sqrt(2000000f / (float)Mathf.Min(this._Water.Geometry.TesselatedBaseVertexCount, WaterQualitySettings.Instance.MaxTesselatedVertexCount));
			this._Water.Renderer.PropertyBlock.SetFloat("_TesselationFactor", Mathf.Lerp(1f, b, Mathf.Min(this._TesselationFactor, currentQualityLevel.MaxTesselationFactor)));
			if (!Application.isPlaying)
			{
				bool blendMode = this._Water.ShaderSet.TransparencyMode == WaterTransparencyMode.Refractive && currentQualityLevel.AllowAlphaBlending;
				if (Camera.main != null)
				{
					WaterCamera waterCamera = Camera.main.GetComponent<WaterCamera>();
					if (waterCamera == null)
					{
						waterCamera = UnityEngine.Object.FindObjectOfType<WaterCamera>();
					}
					if (waterCamera != null && waterCamera.RenderMode < WaterRenderMode.ImageEffectDeferred)
					{
						this.SetBlendMode(blendMode);
					}
					else
					{
						this.SetBlendMode(false);
					}
				}
				else
				{
					this.SetBlendMode(false);
				}
			}
			this._Water.Renderer.PropertyBlock.SetFloat(WaterMaterials._ParameterHashes[23], -1f);
			if (this._AlphaBlendMode != 0)
			{
				this.SetBlendMode(this._AlphaBlendMode == 2);
			}
			string name = this.SurfaceMaterial.shader.name;
			if (name.Contains("_WAVES_FFT"))
			{
				this.SurfaceMaterial.EnableKeyword("_WAVES_FFT");
			}
			if (name.Contains("_BOUNDED_WATER"))
			{
				this.SurfaceMaterial.EnableKeyword("_BOUNDED_WATER");
			}
			if (name.Contains("_WAVES_ALIGN"))
			{
				this.SurfaceMaterial.EnableKeyword("_WAVES_ALIGN");
			}
			if (name.Contains("_WAVES_GERSTNER"))
			{
				this.SurfaceMaterial.EnableKeyword("_WAVES_GERSTNER");
			}
			if (this._Water.Geometry.Triangular)
			{
				this.SurfaceMaterial.EnableKeyword("_TRIANGLES");
			}
		}

		internal void Awake(Water water)
		{
			this._Water = water;
			water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			water.WaterIdChanged += this.OnWaterIdChanged;
			WaterMaterials.CreateParameterHashes();
			this.CreateMaterials();
		}

		internal void OnWaterRender(WaterCamera waterCamera)
		{
			Vector2 surfaceOffset = this._Water.SurfaceOffset;
			Vector4 vector = new Vector4(surfaceOffset.x, this._Water.transform.position.y, surfaceOffset.y, this._Water.UniformWaterScale);
			if (vector.x != this._LastSurfaceOffset.x || vector.y != this._LastSurfaceOffset.y || vector.z != this._LastSurfaceOffset.z || vector.w != this._LastSurfaceOffset.w)
			{
				this._LastSurfaceOffset = vector;
				this._Water.Renderer.PropertyBlock.SetVector(ShaderVariables.SurfaceOffset, vector);
				this.UpdateGlobalLookupTexOffset();
			}
			Shader.SetGlobalColor(WaterMaterials._ParameterHashes[0], this.GetParameterValue(WaterMaterials.ColorParameter.AbsorptionColor));
			if (waterCamera.Type == WaterCamera.CameraType.Normal)
			{
				int num;
				if (waterCamera.RenderMode < WaterRenderMode.ImageEffectDeferred)
				{
					WaterQualityLevel currentQualityLevel = WaterQualitySettings.Instance.CurrentQualityLevel;
					bool flag = this._Water.ShaderSet.TransparencyMode == WaterTransparencyMode.Refractive && currentQualityLevel.AllowAlphaBlending;
					num = ((!flag) ? 1 : 2);
				}
				else
				{
					num = 1;
				}
				if (this._AlphaBlendMode != num)
				{
					this.SetBlendMode(num == 2);
				}
			}
		}

		internal void OnDestroy()
		{
			this.SurfaceMaterial.Destroy();
			this.SurfaceMaterial = null;
			this.SurfaceBackMaterial.Destroy();
			this.SurfaceBackMaterial = null;
			this.VolumeMaterial.Destroy();
			this.VolumeMaterial = null;
			this.VolumeBackMaterial.Destroy();
			this.VolumeBackMaterial = null;
			TextureUtility.Release(ref WaterMaterials._GlobalWaterLookupTex);
			TextureUtility.Release(ref this._AbsorptionGradient);
			if (!Application.isPlaying)
			{
				return;
			}
			if (this._Water != null)
			{
				this._Water.ProfilesManager.Changed.RemoveListener(new UnityAction<Water>(this.OnProfilesChanged));
				this._Water.WaterIdChanged -= this.OnWaterIdChanged;
			}
		}

		internal void OnValidate()
		{
			if (this._Water != null)
			{
				this.UpdateShaders();
				this.UpdateSurfaceMaterial();
			}
		}

		private void SetBlendMode(bool alphaBlend)
		{
			this._AlphaBlendMode = ((!alphaBlend) ? 1 : 2);
			Material surfaceMaterial = this.SurfaceMaterial;
			surfaceMaterial.SetOverrideTag("RenderType", (!alphaBlend) ? "Opaque" : "Transparent");
			surfaceMaterial.SetFloat("_Mode", (float)((!alphaBlend) ? 0 : 2));
			surfaceMaterial.SetInt("_SrcBlend", (!alphaBlend) ? 1 : 5);
			surfaceMaterial.SetInt("_DstBlend", (!alphaBlend) ? 0 : 10);
			surfaceMaterial.renderQueue = ((!alphaBlend) ? 2000 : 2990);
			this.UpdateSurfaceBackMaterial();
			this.UpdateVolumeMaterials();
		}

		private void CreateMaterials()
		{
			if (this.SurfaceMaterial != null)
			{
				return;
			}
			int waterId = this._Water.WaterId;
			Shader shader;
			Shader shader2;
			this._Water.ShaderSet.FindBestShaders(out shader, out shader2);
			if (shader == null || shader2 == null)
			{
				throw new InvalidOperationException("Water in a scene '" + this._Water.gameObject.scene.name + "' doesn't contain necessary shaders to function properly. Please open this scene in editor and simply select the water to update its shaders.");
			}
			this.SurfaceMaterial = new Material(shader)
			{
				hideFlags = HideFlags.DontSave
			};
			this.SurfaceMaterial.SetFloat("_WaterStencilId", (float)waterId);
			this.SurfaceMaterial.SetFloat("_WaterStencilIdInv", (float)(~(float)waterId & 255));
			this._Water.Renderer.PropertyBlock.SetVector("_WaterId", new Vector4((float)(1 << waterId), (float)(1 << waterId + 1), ((float)waterId + 0.5f) / 256f, 0f));
			this.UpdateSurfaceMaterial();
			this.SurfaceBackMaterial = new Material(shader)
			{
				hideFlags = HideFlags.DontSave
			};
			this.VolumeMaterial = new Material(shader2)
			{
				hideFlags = HideFlags.DontSave
			};
			this.VolumeBackMaterial = new Material(shader2)
			{
				hideFlags = HideFlags.DontSave
			};
			this.UpdateSurfaceBackMaterial();
			this.UpdateVolumeMaterials();
			this.UsedKeywords = string.Join(" ", this.SurfaceMaterial.shaderKeywords);
		}

		private void UpdateShaders()
		{
			Shader shader;
			Shader shader2;
			this._Water.ShaderSet.FindBestShaders(out shader, out shader2);
			if (this.SurfaceMaterial.shader != shader || this.VolumeMaterial.shader != shader2)
			{
				this.SurfaceMaterial.shader = shader;
				this.SurfaceBackMaterial.shader = shader;
				this.VolumeMaterial.shader = shader2;
				this.VolumeBackMaterial.shader = shader2;
				this.UpdateSurfaceMaterial();
				this.UpdateSurfaceBackMaterial();
				this.UpdateVolumeMaterials();
			}
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			WaterProfileData profile = profiles[0].Profile;
			float num = 0f;
			foreach (Water.WeightedProfile weightedProfile in profiles)
			{
				if (profile == null || num < weightedProfile.Weight)
				{
					profile = weightedProfile.Profile;
					num = weightedProfile.Weight;
				}
			}
			this.HorizontalDisplacementScale = 0f;
			this.UnderwaterBlurSize = 0f;
			this.UnderwaterLightFadeScale = 0f;
			this.UnderwaterDistortionsIntensity = 0f;
			this.UnderwaterDistortionAnimationSpeed = 0f;
			Color color = new Color(0f, 0f, 0f, 0f);
			Color color2 = new Color(0f, 0f, 0f, 0f);
			Color color3 = new Color(0f, 0f, 0f, 0f);
			Color color4 = new Color(0f, 0f, 0f, 0f);
			Color color5 = new Color(0f, 0f, 0f, 0f);
			Color color6 = new Color(0f, 0f, 0f, 0f);
			Color color7 = new Color(0f, 0f, 0f, 0f);
			Color color8 = new Color(0f, 0f, 0f, 0f);
			Color color9 = new Color(0f, 0f, 0f, 0f);
			this._Smoothness = 0f;
			this._AmbientSmoothness = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			this._ForwardScatteringIntensity = 0f;
			Vector3 v = default(Vector3);
			Vector2 a = default(Vector2);
			NormalMapAnimation normalMapAnimation = default(NormalMapAnimation);
			NormalMapAnimation normalMapAnimation2 = default(NormalMapAnimation);
			for (int j = 0; j < profiles.Length; j++)
			{
				WaterProfileData profile2 = profiles[j].Profile;
				float weight = profiles[j].Weight;
				this.HorizontalDisplacementScale += profile2.HorizontalDisplacementScale * weight;
				this.UnderwaterBlurSize += profile2.UnderwaterBlurSize * weight;
				this.UnderwaterLightFadeScale += profile2.UnderwaterLightFadeScale * weight;
				this.UnderwaterDistortionsIntensity += profile2.UnderwaterDistortionsIntensity * weight;
				this.UnderwaterDistortionAnimationSpeed += profile2.UnderwaterDistortionAnimationSpeed * weight;
				color += profile2.AbsorptionColor * weight;
				color2 += profile2.DiffuseColor * weight;
				color3 += profile2.SpecularColor * weight;
				color4 += profile2.DepthColor * weight;
				color5 += profile2.EmissionColor * weight;
				color6 += profile2.ReflectionColor * weight;
				color7 += profile2.FoamDiffuseColor * weight;
				color8 += profile2.FoamSpecularColor * weight;
				this._Smoothness += profile2.Smoothness * weight;
				this._AmbientSmoothness += profile2.AmbientSmoothness * weight;
				num2 += profile2.RefractionDistortion * weight;
				color9 += profile2.SubsurfaceScatteringShoreColor * weight;
				num3 += profile2.DetailFadeDistance * weight;
				num4 += profile2.DisplacementNormalsIntensity * weight;
				num5 += profile2.EdgeBlendFactor * weight;
				num6 += profile2.DirectionalWrapSSS * weight;
				num7 += profile2.PointWrapSSS * weight;
				this._ForwardScatteringIntensity += profile2.ForwardScatteringIntensity * weight;
				v.x += profile2.PlanarReflectionIntensity * weight;
				v.y += profile2.PlanarReflectionFlatten * weight;
				v.z += profile2.PlanarReflectionVerticalOffset * weight;
				a += profile2.FoamTiling * weight;
				normalMapAnimation += profile2.NormalMapAnimation1 * weight;
				normalMapAnimation2 += profile2.NormalMapAnimation2 * weight;
			}
			if (water.WindWaves != null && water.WindWaves.FinalRenderMode == WaveSpectrumRenderMode.GerstnerAndFFTNormals)
			{
				num4 *= 0.5f;
			}
			MaterialPropertyBlock propertyBlock = water.Renderer.PropertyBlock;
			color9.a = this._ForwardScatteringIntensity;
			propertyBlock.SetVector(WaterMaterials._ParameterHashes[0], color);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[1], color2);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[2], color3);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[3], color4);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[4], color5);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[5], color6);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[24], color7);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[22], color8);
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[6], this.HorizontalDisplacementScale);
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[7], this._AmbientSmoothness);
			propertyBlock.SetVector(WaterMaterials._ParameterHashes[9], new Vector4(num6, 1f / (1f + num6), num7, 1f / (1f + num7)));
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[10], num2);
			propertyBlock.SetColor(WaterMaterials._ParameterHashes[11], color9);
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[12], 1f / num3);
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[13], num4);
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[14], 1f / num5);
			propertyBlock.SetVector(WaterMaterials._ParameterHashes[15], v);
			propertyBlock.SetVector(WaterMaterials._ParameterHashes[16], new Vector4(normalMapAnimation.Intensity, normalMapAnimation2.Intensity, -(normalMapAnimation.Intensity + normalMapAnimation2.Intensity) * 0.5f, 0f));
			propertyBlock.SetVector(WaterMaterials._ParameterHashes[17], new Vector2(a.x / normalMapAnimation.Tiling.x, a.y / normalMapAnimation.Tiling.y));
			propertyBlock.SetFloat(WaterMaterials._ParameterHashes[18], this._Smoothness / this._AmbientSmoothness);
			this.SurfaceMaterial.SetTexture(WaterMaterials._ParameterHashes[19], profile.NormalMap);
			this.SurfaceMaterial.SetTexture(WaterMaterials._ParameterHashes[20], profile.FoamDiffuseMap);
			this.SurfaceMaterial.SetTexture(WaterMaterials._ParameterHashes[21], profile.FoamNormalMap);
			water.UVAnimator.NormalMapAnimation1 = normalMapAnimation;
			water.UVAnimator.NormalMapAnimation2 = normalMapAnimation2;
			if (this._VectorOverrides != null)
			{
				this.ApplyOverridenParameters();
			}
			this._AbsorptionGradientDirty = true;
			this.UpdateSurfaceBackMaterial();
			this.UpdateVolumeMaterials();
			this.UpdateGlobalLookupTex();
		}

		private void UpdateSurfaceBackMaterial()
		{
			if (this.SurfaceBackMaterial == null)
			{
				return;
			}
			if (this.SurfaceBackMaterial.shader != this.SurfaceMaterial.shader)
			{
				this.SurfaceBackMaterial.shader = this.SurfaceMaterial.shader;
			}
			Color color = this.SurfaceBackMaterial.GetColor(WaterMaterials._ParameterHashes[2]);
			this.SurfaceBackMaterial.CopyPropertiesFromMaterial(this.SurfaceMaterial);
			this.SurfaceBackMaterial.SetColor(WaterMaterials._ParameterHashes[2], color);
			this.SurfaceBackMaterial.SetFloat(WaterMaterials._ParameterHashes[11], 0f);
			this.SurfaceBackMaterial.EnableKeyword("_WATER_BACK");
			this.SurfaceBackMaterial.SetFloat(ShaderVariables.Cull, 1f);
		}

		private void UpdateVolumeMaterials()
		{
			if (this.VolumeMaterial == null)
			{
				return;
			}
			this.VolumeMaterial.CopyPropertiesFromMaterial(this.SurfaceMaterial);
			this.VolumeBackMaterial.CopyPropertiesFromMaterial(this.SurfaceMaterial);
			if (this.SurfaceMaterial.renderQueue == 2990)
			{
				Material volumeBackMaterial = this.VolumeBackMaterial;
				int renderQueue = 2991;
				this.VolumeMaterial.renderQueue = renderQueue;
				volumeBackMaterial.renderQueue = renderQueue;
			}
			this.VolumeBackMaterial.SetFloat(ShaderVariables.WaterId, 1f);
		}

		private void ApplyOverridenParameters()
		{
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				propertyBlock.SetVector(this._VectorOverrides[i].Hash, this._VectorOverrides[i].Value);
			}
			for (int j = 0; j < this._FloatOverrides.Length; j++)
			{
				propertyBlock.SetFloat(this._FloatOverrides[j].Hash, this._FloatOverrides[j].Value);
			}
		}

		private void OnWaterIdChanged()
		{
			int waterId = this._Water.WaterId;
			this._Water.Renderer.PropertyBlock.SetVector(ShaderVariables.WaterId, new Vector4((float)(1 << waterId), (float)(1 << waterId + 1), ((float)waterId + 0.5f) / 256f, 0f));
		}

		private static void RemoveArrayElementAt<T>(ref T[] array, int index)
		{
			T[] array2 = array;
			T[] array3 = new T[array.Length - 1];
			for (int i = 0; i < index; i++)
			{
				array3[i] = array2[i];
			}
			for (int j = index; j < array3.Length; j++)
			{
				array3[j] = array2[j + 1];
			}
			array = array3;
		}

		private static void CreateParameterHashes()
		{
			if (WaterMaterials._ParameterHashes != null && WaterMaterials._ParameterHashes.Length == WaterMaterials._ParameterNames.Length)
			{
				return;
			}
			int num = WaterMaterials._ParameterNames.Length;
			WaterMaterials._ParameterHashes = new int[num];
			for (int i = 0; i < num; i++)
			{
				WaterMaterials._ParameterHashes[i] = Shader.PropertyToID(WaterMaterials._ParameterNames[i]);
			}
		}

		private void UpdateGlobalLookupTexOffset()
		{
			int waterId = this._Water.WaterId;
			if (WaterMaterials._GlobalWaterLookupTex == null || waterId == -1)
			{
				return;
			}
			if (waterId >= 256)
			{
				Debug.LogError("There is more than 256 water objects in the scene. This is not supported in deferred water render mode. You have to switch to WaterCameraSimple.");
				return;
			}
			Vector2 surfaceOffset = this._Water.SurfaceOffset;
			WaterMaterials._GlobalWaterLookupTex.SetPixel(waterId, 2, new Color(surfaceOffset.x, this._Water.transform.position.y, surfaceOffset.y, this._DirectionalLightsIntensity));
			WaterMaterials._GlobalLookupDirty = true;
		}

		private static void CreateGlobalWaterDataLookupTex()
		{
			WaterMaterials._GlobalWaterLookupTex = new Texture2D(256, 4, TextureFormat.RGBAHalf, false, true)
			{
				name = "[UWS] WaterMaterials - _GlobalWaterLookupTex",
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			for (int i = 0; i < 256; i++)
			{
				WaterMaterials._GlobalWaterLookupTex.SetPixel(i, 0, color);
				WaterMaterials._GlobalWaterLookupTex.SetPixel(i, 1, color);
				WaterMaterials._GlobalWaterLookupTex.SetPixel(i, 2, color);
				WaterMaterials._GlobalWaterLookupTex.SetPixel(i, 3, color);
			}
			List<Water> waters = ApplicationSingleton<WaterSystem>.Instance.Waters;
			int count = waters.Count;
			for (int j = 0; j < count; j++)
			{
				waters[j].Materials.UpdateGlobalLookupTex();
			}
			WaterMaterials._GlobalWaterLookupTex.Apply(false, false);
			Shader.SetGlobalTexture("_GlobalWaterLookupTex", WaterMaterials._GlobalWaterLookupTex);
		}

		private void ComputeAbsorptionGradient()
		{
			this._AbsorptionGradientDirty = false;
			if (this._AbsorptionGradient == null)
			{
				this._AbsorptionGradient = new Texture2D(512, 1, TextureFormat.RGBAHalf, false, true)
				{
					name = "[UWS] Water Materials - Absorption Gradient",
					hideFlags = HideFlags.DontSave,
					wrapMode = TextureWrapMode.Clamp,
					filterMode = FilterMode.Bilinear
				};
			}
			Water.WeightedProfile[] profiles = this._Water.ProfilesManager.Profiles;
			for (int i = 0; i < 512; i++)
			{
				WaterMaterials._AbsorptionColorsBuffer[i] = new Color(0f, 0f, 0f, 0f);
			}
			for (int j = 0; j < profiles.Length; j++)
			{
				Gradient absorptionColorByDepth = profiles[j].Profile.AbsorptionColorByDepth;
				float weight = profiles[j].Weight;
				for (int k = 0; k < 512; k++)
				{
					WaterMaterials._AbsorptionColorsBuffer[k] += absorptionColorByDepth.Evaluate((float)k / 31f) * weight;
				}
			}
			this._AbsorptionGradient.SetPixels(WaterMaterials._AbsorptionColorsBuffer);
			this._AbsorptionGradient.Apply();
		}

		public Color GetParameterValue(WaterMaterials.ColorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					return this._VectorOverrides[i].Value;
				}
			}
			return this._Water.Renderer.PropertyBlock.GetVector(num);
		}

		public WaterMaterials.WaterParameterVector4 GetParameterOverride(WaterMaterials.ColorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					return this._VectorOverrides[i];
				}
			}
			Vector4 vector = this._Water.Renderer.PropertyBlock.GetVector(num);
			Array.Resize<WaterMaterials.WaterParameterVector4>(ref this._VectorOverrides, this._VectorOverrides.Length + 1);
			return this._VectorOverrides[this._VectorOverrides.Length - 1] = new WaterMaterials.WaterParameterVector4(this._Water.Renderer.PropertyBlock, num, vector);
		}

		public void ResetParameter(WaterMaterials.ColorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					WaterMaterials.RemoveArrayElementAt<WaterMaterials.WaterParameterVector4>(ref this._VectorOverrides, i);
				}
			}
		}

		public Vector4 GetParameterValue(WaterMaterials.VectorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					return this._VectorOverrides[i].Value;
				}
			}
			return this._Water.Renderer.PropertyBlock.GetVector(num);
		}

		public WaterMaterials.WaterParameterVector4 GetParameterOverride(WaterMaterials.VectorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					return this._VectorOverrides[i];
				}
			}
			Vector4 vector = this._Water.Renderer.PropertyBlock.GetVector(num);
			Array.Resize<WaterMaterials.WaterParameterVector4>(ref this._VectorOverrides, this._VectorOverrides.Length + 1);
			return this._VectorOverrides[this._VectorOverrides.Length - 1] = new WaterMaterials.WaterParameterVector4(this._Water.Renderer.PropertyBlock, num, vector);
		}

		public void ResetParameter(WaterMaterials.VectorParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._VectorOverrides.Length; i++)
			{
				if (this._VectorOverrides[i].Hash == num)
				{
					WaterMaterials.RemoveArrayElementAt<WaterMaterials.WaterParameterVector4>(ref this._VectorOverrides, i);
				}
			}
		}

		public float GetParameterValue(WaterMaterials.FloatParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._FloatOverrides.Length; i++)
			{
				if (this._FloatOverrides[i].Hash == num)
				{
					return this._FloatOverrides[i].Value;
				}
			}
			return this._Water.Renderer.PropertyBlock.GetFloat(num);
		}

		public WaterMaterials.WaterParameterFloat GetParameterOverride(WaterMaterials.FloatParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._FloatOverrides.Length; i++)
			{
				if (this._FloatOverrides[i].Hash == num)
				{
					return this._FloatOverrides[i];
				}
			}
			float @float = this._Water.Renderer.PropertyBlock.GetFloat(num);
			Array.Resize<WaterMaterials.WaterParameterFloat>(ref this._FloatOverrides, this._FloatOverrides.Length + 1);
			return this._FloatOverrides[this._FloatOverrides.Length - 1] = new WaterMaterials.WaterParameterFloat(this._Water.Renderer.PropertyBlock, num, @float);
		}

		public void ResetParameter(WaterMaterials.FloatParameter parameter)
		{
			int num = WaterMaterials._ParameterHashes[(int)parameter];
			for (int i = 0; i < this._FloatOverrides.Length; i++)
			{
				if (this._FloatOverrides[i].Hash == num)
				{
					WaterMaterials.RemoveArrayElementAt<WaterMaterials.WaterParameterFloat>(ref this._FloatOverrides, i);
				}
			}
		}

		[SerializeField]
		private float _DirectionalLightsIntensity = 1f;

		[Range(0f, 1f)]
		[Tooltip("May hurt performance on some systems.")]
		[SerializeField]
		private float _TesselationFactor = 1f;

		private Water _Water;

		private float _Smoothness;

		private float _AmbientSmoothness;

		private float _ForwardScatteringIntensity;

		private Vector4 _LastSurfaceOffset;

		private Texture2D _AbsorptionGradient;

		private bool _AbsorptionGradientDirty = true;

		private int _AlphaBlendMode;

		private WaterMaterials.WaterParameterFloat[] _FloatOverrides = new WaterMaterials.WaterParameterFloat[0];

		private WaterMaterials.WaterParameterVector4[] _VectorOverrides = new WaterMaterials.WaterParameterVector4[0];

		private const int _GradientResolution = 512;

		private static readonly Color[] _AbsorptionColorsBuffer = new Color[512];

		private static int[] _ParameterHashes;

		private static Texture2D _GlobalWaterLookupTex;

		private static bool _GlobalLookupDirty;

		private static readonly string[] _ParameterNames = new string[]
		{
			"_AbsorptionColor",
			"_Color",
			"_SpecColor",
			"_DepthColor",
			"_EmissionColor",
			"_ReflectionColor",
			"_DisplacementsScale",
			"_Glossiness",
			"_SubsurfaceScatteringPack",
			"_WrapSubsurfaceScatteringPack",
			"_RefractionDistortion",
			"_SubsurfaceScatteringShoreColor",
			"_DetailFadeFactor",
			"_DisplacementNormalsIntensity",
			"_EdgeBlendFactorInv",
			"_PlanarReflectionPack",
			"_BumpScale",
			"_FoamTiling",
			"_LightSmoothnessMul",
			"_BumpMap",
			"_FoamTex",
			"_FoamNormalMap",
			"_FoamSpecularColor",
			"_RefractionMaxDepth",
			"_FoamDiffuseColor"
		};

		public enum ColorParameter
		{
			AbsorptionColor,
			DiffuseColor,
			SpecularColor,
			DepthColor,
			EmissionColor,
			ReflectionColor
		}

		public enum FloatParameter
		{
			DisplacementScale = 6,
			Glossiness,
			RefractionDistortion = 10,
			SpecularFresnelBias,
			DisplacementNormalsIntensity = 13,
			EdgeBlendFactorInv,
			LightSmoothnessMultiplier = 18
		}

		public enum VectorParameter
		{
			SubsurfaceScatteringPack = 8,
			WrapSubsurfaceScatteringPack,
			DetailFadeFactor = 12,
			PlanarReflectionPack = 15,
			BumpScale,
			FoamTiling
		}

		public enum TextureParameter
		{
			BumpMap = 19,
			FoamTex,
			FoamNormalMap
		}

		public abstract class AWaterParameter<T>
		{
			protected AWaterParameter(MaterialPropertyBlock propertyBlock, int hash, T value)
			{
				this._PropertyBlock = propertyBlock;
				this.Hash = hash;
				this._Value = value;
			}

			public abstract T Value { get; set; }

			public readonly int Hash;

			protected readonly MaterialPropertyBlock _PropertyBlock;

			protected T _Value;
		}

		public class WaterParameterFloat : WaterMaterials.AWaterParameter<float>
		{
			public WaterParameterFloat(MaterialPropertyBlock propertyBlock, int hash, float value) : base(propertyBlock, hash, value)
			{
			}

			public override float Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
					this._PropertyBlock.SetFloat(this.Hash, value);
				}
			}
		}

		public class WaterParameterVector4 : WaterMaterials.AWaterParameter<Vector4>
		{
			public WaterParameterVector4(MaterialPropertyBlock propertyBlock, int hash, Vector4 value) : base(propertyBlock, hash, value)
			{
			}

			public override Vector4 Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
					this._PropertyBlock.SetVector(this.Hash, value);
				}
			}
		}
	}
}
