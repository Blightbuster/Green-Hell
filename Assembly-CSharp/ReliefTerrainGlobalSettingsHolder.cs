using System;
using UnityEngine;

[Serializable]
public class ReliefTerrainGlobalSettingsHolder
{
	public ReliefTerrainGlobalSettingsHolder()
	{
		this.Bumps = new Texture2D[12];
		this.Heights = new Texture2D[12];
		this.FarSpecCorrection = new float[12];
		this.MIPmult = new float[12];
		this.MixScale = new float[12];
		this.MixBlend = new float[12];
		this.MixSaturation = new float[12];
		this.RTP_DiffFresnel = new float[12];
		this.RTP_metallic = new float[12];
		this.RTP_glossMin = new float[12];
		this.RTP_glossMax = new float[12];
		this.RTP_glitter = new float[12];
		this.RTP_heightMin = new float[12];
		this.RTP_heightMax = new float[12];
		this.MixBrightness = new float[12];
		this.MixReplace = new float[12];
		this.LayerBrightness = new float[12];
		this.LayerBrightness2Spec = new float[12];
		this.LayerAlbedo2SpecColor = new float[12];
		this.LayerSaturation = new float[12];
		this.LayerEmission = new float[12];
		this.LayerEmissionColor = new Color[12];
		this.LayerEmissionRefractStrength = new float[12];
		this.LayerEmissionRefractHBedge = new float[12];
		this.GlobalColorPerLayer = new float[12];
		this.GlobalColorBottom = new float[12];
		this.GlobalColorTop = new float[12];
		this.GlobalColorColormapLoSat = new float[12];
		this.GlobalColorColormapHiSat = new float[12];
		this.GlobalColorLayerLoSat = new float[12];
		this.GlobalColorLayerHiSat = new float[12];
		this.GlobalColorLoBlend = new float[12];
		this.GlobalColorHiBlend = new float[12];
		this.PER_LAYER_HEIGHT_MODIFIER = new float[12];
		this._snow_strength_per_layer = new float[12];
		this._SuperDetailStrengthMultA = new float[12];
		this._SuperDetailStrengthMultASelfMaskNear = new float[12];
		this._SuperDetailStrengthMultASelfMaskFar = new float[12];
		this._SuperDetailStrengthMultB = new float[12];
		this._SuperDetailStrengthMultBSelfMaskNear = new float[12];
		this._SuperDetailStrengthMultBSelfMaskFar = new float[12];
		this._SuperDetailStrengthNormal = new float[12];
		this._BumpMapGlobalStrength = new float[12];
		this.AO_strength = new float[12];
		this.VerticalTextureStrength = new float[12];
		this.TERRAIN_LayerWetStrength = new float[12];
		this.TERRAIN_WaterLevel = new float[12];
		this.TERRAIN_WaterLevelSlopeDamp = new float[12];
		this.TERRAIN_WaterEdge = new float[12];
		this.TERRAIN_WaterGloss = new float[12];
		this.TERRAIN_WaterGlossDamper = new float[12];
		this.TERRAIN_Refraction = new float[12];
		this.TERRAIN_WetRefraction = new float[12];
		this.TERRAIN_Flow = new float[12];
		this.TERRAIN_WetFlow = new float[12];
		this.TERRAIN_WaterMetallic = new float[12];
		this.TERRAIN_WetGloss = new float[12];
		this.TERRAIN_WaterColor = new Color[12];
		this.TERRAIN_WaterEmission = new float[12];
		this._GlitterStrength = new float[12];
		this.TERRAIN_WetnessAttackCurve = new AnimationCurve();
	}

	public void ReInit(Terrain terrainComp)
	{
		if (terrainComp.terrainData.terrainLayers.Length > this.numLayers)
		{
			Texture2D[] array = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			Texture2D[] array2 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			for (int i = 0; i < this.splats.Length; i++)
			{
				array[i] = this.splats[i];
				array2[i] = this.Bumps[i];
			}
			this.splats = array;
			this.Bumps = array2;
			this.splats[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[(terrainComp.terrainData.terrainLayers.Length - 2 >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].diffuseTexture;
			this.Bumps[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[(terrainComp.terrainData.terrainLayers.Length - 2 >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].normalMapTexture;
		}
		else if (terrainComp.terrainData.terrainLayers.Length < this.numLayers)
		{
			Texture2D[] array3 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			Texture2D[] array4 = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j] = this.splats[j];
				array4[j] = this.Bumps[j];
			}
			this.splats = array3;
			this.Bumps = array4;
		}
		this.numLayers = terrainComp.terrainData.terrainLayers.Length;
	}

	public void SetShaderParam(string name, Texture2D tex)
	{
		if (!tex)
		{
			return;
		}
		if (this.use_mat)
		{
			this.use_mat.SetTexture(name, tex);
			return;
		}
		Shader.SetGlobalTexture(name, tex);
	}

	public void SetShaderParam(string name, Cubemap tex)
	{
		if (!tex)
		{
			return;
		}
		if (this.use_mat)
		{
			this.use_mat.SetTexture(name, tex);
			return;
		}
		Shader.SetGlobalTexture(name, tex);
	}

	public void SetShaderParam(string name, Matrix4x4 mtx)
	{
		if (this.use_mat)
		{
			this.use_mat.SetMatrix(name, mtx);
			return;
		}
		Shader.SetGlobalMatrix(name, mtx);
	}

	public void SetShaderParam(string name, Vector4 vec)
	{
		if (this.use_mat)
		{
			this.use_mat.SetVector(name, vec);
			return;
		}
		Shader.SetGlobalVector(name, vec);
	}

	public void SetShaderParam(string name, float val)
	{
		if (this.use_mat)
		{
			this.use_mat.SetFloat(name, val);
			return;
		}
		Shader.SetGlobalFloat(name, val);
	}

	public void SetShaderParam(string name, Color col)
	{
		if (this.use_mat)
		{
			this.use_mat.SetColor(name, col);
			return;
		}
		Shader.SetGlobalColor(name, col);
	}

	public RTP_LODmanager Get_RTP_LODmanagerScript()
	{
		return this._RTP_LODmanagerScript;
	}

	private void CheckLightScriptForDefered()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		Light light = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].type == LightType.Directional)
			{
				if (!(array[i].gameObject.GetComponent<ReliefShaders_applyLightForDeferred>() == null))
				{
					return;
				}
				light = array[i];
			}
		}
		if (light)
		{
			(light.gameObject.AddComponent(typeof(ReliefShaders_applyLightForDeferred)) as ReliefShaders_applyLightForDeferred).lightForSelfShadowing = light;
		}
	}

	public void RefreshAll()
	{
		this.CheckLightScriptForDefered();
		ReliefTerrain[] array = UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain)) as ReliefTerrain[];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].globalSettingsHolder != null)
			{
				Terrain terrain = array[i].GetComponent(typeof(Terrain)) as Terrain;
				if (terrain)
				{
					array[i].globalSettingsHolder.Refresh(terrain.materialTemplate, null);
				}
				else
				{
					array[i].globalSettingsHolder.Refresh(array[i].GetComponent<Renderer>().sharedMaterial, null);
				}
				array[i].RefreshTextures(null, false);
			}
		}
		GeometryVsTerrainBlend[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(GeometryVsTerrainBlend)) as GeometryVsTerrainBlend[];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].SetupValues();
		}
	}

	public void Refresh(Material mat = null, ReliefTerrain rt_caller = null)
	{
		if (this.splats == null)
		{
			return;
		}
		if (mat == null && rt_caller != null && rt_caller.globalSettingsHolder == this)
		{
			Terrain terrain = rt_caller.GetComponent(typeof(Terrain)) as Terrain;
			if (terrain)
			{
				rt_caller.globalSettingsHolder.Refresh(terrain.materialTemplate, null);
			}
			else if (rt_caller.GetComponent<Renderer>() != null && rt_caller.GetComponent<Renderer>().sharedMaterial != null)
			{
				rt_caller.globalSettingsHolder.Refresh(rt_caller.GetComponent<Renderer>().sharedMaterial, null);
			}
		}
		this.use_mat = mat;
		if (mat != null)
		{
			mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
		}
		for (int i = 0; i < this.numLayers; i++)
		{
			if (i < 4)
			{
				this.SetShaderParam("_SplatA" + i, this.splats[i]);
			}
			else if (i < 8)
			{
				if (this._4LAYERS_SHADER_USED)
				{
					this.SetShaderParam("_SplatC" + (i - 4), this.splats[i]);
					this.SetShaderParam("_SplatB" + (i - 4), this.splats[i]);
				}
				else
				{
					this.SetShaderParam("_SplatB" + (i - 4), this.splats[i]);
				}
			}
			else if (i < 12)
			{
				this.SetShaderParam("_SplatC" + (i - 4), this.splats[i]);
			}
		}
		this.CheckAndUpdate(ref this.RTP_DiffFresnel, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_metallic, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_glossMin, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_glossMax, 1f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_glitter, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_heightMin, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.RTP_heightMax, 1f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WaterGloss, 0.1f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WaterGlossDamper, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WaterMetallic, 0.1f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WetGloss, 0.05f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WetFlow, 0.05f, this.numLayers);
		this.CheckAndUpdate(ref this.MixBrightness, 2f, this.numLayers);
		this.CheckAndUpdate(ref this.MixReplace, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerBrightness, 1f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerBrightness2Spec, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerAlbedo2SpecColor, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerSaturation, 1f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerEmission, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.FarSpecCorrection, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerEmissionColor, Color.black, this.numLayers);
		this.CheckAndUpdate(ref this.LayerEmissionRefractStrength, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.LayerEmissionRefractHBedge, 0f, this.numLayers);
		this.CheckAndUpdate(ref this.TERRAIN_WaterEmission, 0f, this.numLayers);
		this.CheckAndUpdate(ref this._GlitterStrength, 0f, this.numLayers);
		this.SetShaderParam("terrainTileSize", this.terrainTileSize);
		this.SetShaderParam("RTP_AOamp", this.RTP_AOamp);
		this.SetShaderParam("RTP_AOsharpness", this.RTP_AOsharpness);
		this.SetShaderParam("_occlusionStrength", this._occlusionStrength);
		this.SetShaderParam("EmissionRefractFiltering", this.EmissionRefractFiltering);
		this.SetShaderParam("EmissionRefractAnimSpeed", this.EmissionRefractAnimSpeed);
		this.SetShaderParam("_VerticalTexture", this.VerticalTexture);
		this.SetShaderParam("_GlobalColorMapBlendValues", this.GlobalColorMapBlendValues);
		this.SetShaderParam("_GlobalColorMapSaturation", this.GlobalColorMapSaturation);
		this.SetShaderParam("_GlobalColorMapSaturationFar", this.GlobalColorMapSaturationFar);
		this.SetShaderParam("_GlobalColorMapDistortByPerlin", this.GlobalColorMapDistortByPerlin);
		this.SetShaderParam("_GlobalColorMapBrightness", this.GlobalColorMapBrightness);
		this.SetShaderParam("_GlobalColorMapBrightnessFar", this.GlobalColorMapBrightnessFar);
		this.SetShaderParam("_GlobalColorMapNearMIP", this._GlobalColorMapNearMIP);
		this.SetShaderParam("_RTP_MIP_BIAS", this.RTP_MIP_BIAS);
		this.SetShaderParam("_BumpMapGlobalScale", this.BumpMapGlobalScale);
		this.SetShaderParam("_FarNormalDamp", this._FarNormalDamp);
		this.SetShaderParam("_blend_multiplier", this.blendMultiplier);
		this.SetShaderParam("_TERRAIN_ReliefTransform", this.ReliefTransform);
		this.SetShaderParam("_TERRAIN_ReliefTransformTriplanarZ", this.ReliefTransform.x);
		this.SetShaderParam("_TERRAIN_DIST_STEPS", this.DIST_STEPS);
		this.SetShaderParam("_TERRAIN_WAVELENGTH", this.WAVELENGTH);
		this.SetShaderParam("_TERRAIN_ExtrudeHeight", this.ExtrudeHeight);
		this.SetShaderParam("_TERRAIN_LightmapShading", this.LightmapShading);
		this.SetShaderParam("_TERRAIN_SHADOW_STEPS", this.SHADOW_STEPS);
		this.SetShaderParam("_TERRAIN_WAVELENGTH_SHADOWS", this.WAVELENGTH_SHADOWS);
		this.SetShaderParam("_TERRAIN_SelfShadowStrength", this.SelfShadowStrength);
		this.SetShaderParam("_TERRAIN_ShadowSmoothing", (1f - this.ShadowSmoothing) * 6f);
		this.SetShaderParam("_TERRAIN_ShadowSoftnessFade", this.ShadowSoftnessFade);
		this.SetShaderParam("CJ_flattenShadows", this.CJ_flattenShadows);
		this.SetShaderParam("_TERRAIN_distance_start", this.distance_start);
		this.SetShaderParam("_TERRAIN_distance_transition", this.distance_transition);
		this.SetShaderParam("_TERRAIN_distance_start_bumpglobal", this.distance_start_bumpglobal);
		this.SetShaderParam("_TERRAIN_distance_transition_bumpglobal", this.distance_transition_bumpglobal);
		this.SetShaderParam("rtp_perlin_start_val", this.rtp_perlin_start_val);
		Shader.SetGlobalVector("_TERRAIN_trees_shadow_values", new Vector4(this.trees_shadow_distance_start, this.trees_shadow_distance_transition, this.trees_shadow_value, this.global_normalMap_multiplier));
		Shader.SetGlobalVector("_TERRAIN_trees_pixel_values", new Vector4(this.trees_pixel_distance_start, this.trees_pixel_distance_transition, this.trees_pixel_blend_val, this.global_normalMap_farUsage));
		this.SetShaderParam("_Phong", this._Phong);
		this.SetShaderParam("_TessSubdivisions", this._TessSubdivisions);
		this.SetShaderParam("_TessSubdivisionsFar", this._TessSubdivisionsFar);
		this.SetShaderParam("_TessYOffset", this._TessYOffset);
		Shader.SetGlobalFloat("_AmbientEmissiveMultiplier", this._AmbientEmissiveMultiplier);
		Shader.SetGlobalFloat("_AmbientEmissiveRelief", this._AmbientEmissiveRelief);
		Shader.SetGlobalTexture("TERRAIN_CausticsMask", this.TERRAIN_CausticsMask);
		Shader.SetGlobalVector("TERRAIN_CausticsMaskWorld2UV", this.TERRAIN_CausticsMaskWorld2UV);
		Shader.SetGlobalTexture("TERRAIN_PuddleMask", this.TERRAIN_PuddleMask);
		Shader.SetGlobalFloat("TERRAIN_PuddleLevel", this.TERRAIN_PuddleLevel);
		Shader.SetGlobalTexture("TERRAIN_WetMask", this.TERRAIN_WetMask);
		this.SetShaderParam("_SuperDetailTiling", this._SuperDetailTiling);
		Shader.SetGlobalFloat("rtp_snow_strength", this._snow_strength);
		Shader.SetGlobalFloat("rtp_global_color_brightness_to_snow", this._global_color_brightness_to_snow);
		Shader.SetGlobalFloat("rtp_snow_slope_factor", this._snow_slope_factor);
		Shader.SetGlobalFloat("rtp_snow_edge_definition", this._snow_edge_definition);
		Shader.SetGlobalFloat("rtp_snow_height_treshold", this._snow_height_treshold);
		Shader.SetGlobalFloat("rtp_snow_height_transition", this._snow_height_transition);
		Shader.SetGlobalColor("rtp_snow_color", this._snow_color);
		Shader.SetGlobalFloat("rtp_snow_gloss", this._snow_gloss);
		Shader.SetGlobalFloat("rtp_snow_reflectivness", this._snow_reflectivness);
		Shader.SetGlobalFloat("rtp_snow_deep_factor", this._snow_deep_factor);
		Shader.SetGlobalFloat("rtp_snow_diff_fresnel", this._snow_diff_fresnel);
		Shader.SetGlobalFloat("rtp_snow_metallic", this._snow_metallic);
		Shader.SetGlobalFloat("rtp_snow_Frost", this._snow_Frost);
		Shader.SetGlobalFloat("rtp_snow_MicroTiling", this._snow_MicroTiling);
		Shader.SetGlobalFloat("rtp_snow_BumpMicro", this._snow_BumpMicro);
		Shader.SetGlobalFloat("rtp_snow_occlusionStrength", this._snow_occlusionStrength);
		Shader.SetGlobalFloat("rtp_snow_TranslucencyDeferredLightIndex", (float)this._snow_TranslucencyDeferredLightIndex);
		Shader.SetGlobalColor("_SnowGlitterColor", this._SnowGlitterColor);
		this.SetShaderParam("TERRAIN_CausticsAnimSpeed", this.TERRAIN_CausticsAnimSpeed);
		this.SetShaderParam("TERRAIN_CausticsColor", this.TERRAIN_CausticsColor);
		if (this.TERRAIN_CausticsWaterLevelRefObject)
		{
			this.TERRAIN_CausticsWaterLevel = this.TERRAIN_CausticsWaterLevelRefObject.transform.position.y;
		}
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevel", this.TERRAIN_CausticsWaterLevel);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevelByAngle", this.TERRAIN_CausticsWaterLevelByAngle);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterDeepFadeLength", this.TERRAIN_CausticsWaterDeepFadeLength);
		Shader.SetGlobalFloat("TERRAIN_CausticsWaterShallowFadeLength", this.TERRAIN_CausticsWaterShallowFadeLength);
		this.SetShaderParam("TERRAIN_CausticsTilingScale", this.TERRAIN_CausticsTilingScale);
		this.SetShaderParam("TERRAIN_CausticsTex", this.TERRAIN_CausticsTex);
		this.SetShaderParam("_GlitterColor", this._GlitterColor);
		this.SetShaderParam("_GlitterTiling", this._GlitterTiling);
		this.SetShaderParam("_GlitterDensity", this._GlitterDensity);
		this.SetShaderParam("_GlitterFilter", this._GlitterFilter);
		this.SetShaderParam("_GlitterColorization", this._GlitterColorization);
		this.SetShaderParam("_SparkleMap", this._SparkleMap);
		if (this.numLayers > 0)
		{
			int num = 512;
			for (int j = 0; j < this.numLayers; j++)
			{
				if (this.splats[j])
				{
					num = this.splats[j].width;
					break;
				}
			}
			this.SetShaderParam("rtp_mipoffset_color", -Mathf.Log(1024f / (float)num) / Mathf.Log(2f));
			if (this.Bump01 != null)
			{
				num = this.Bump01.width;
			}
			this.SetShaderParam("rtp_mipoffset_bump", -Mathf.Log(1024f / (float)num) / Mathf.Log(2f));
			if (this.HeightMap)
			{
				num = this.HeightMap.width;
			}
			else if (this.HeightMap2)
			{
				num = this.HeightMap2.width;
			}
			else if (this.HeightMap3)
			{
				num = this.HeightMap3.width;
			}
			this.SetShaderParam("rtp_mipoffset_height", -Mathf.Log(1024f / (float)num) / Mathf.Log(2f));
			num = this.BumpGlobalCombinedSize;
			this.SetShaderParam("rtp_mipoffset_globalnorm", -Mathf.Log(1024f / ((float)num * this.BumpMapGlobalScale)) / Mathf.Log(2f) + (float)this.rtp_mipoffset_globalnorm);
			this.SetShaderParam("rtp_mipoffset_superdetail", -Mathf.Log(1024f / ((float)num * this._SuperDetailTiling)) / Mathf.Log(2f));
			this.SetShaderParam("rtp_mipoffset_flow", -Mathf.Log(1024f / ((float)num * this.TERRAIN_FlowScale)) / Mathf.Log(2f) + this.TERRAIN_FlowMipOffset);
			if (this.TERRAIN_RippleMap)
			{
				num = this.TERRAIN_RippleMap.width;
			}
			this.SetShaderParam("rtp_mipoffset_ripple", -Mathf.Log(1024f / ((float)num * this.TERRAIN_RippleScale)) / Mathf.Log(2f));
			if (this.TERRAIN_CausticsTex)
			{
				num = this.TERRAIN_CausticsTex.width;
			}
			this.SetShaderParam("rtp_mipoffset_caustics", -Mathf.Log(1024f / ((float)num * this.TERRAIN_CausticsTilingScale)) / Mathf.Log(2f));
		}
		Shader.SetGlobalFloat("TERRAIN_GlobalWetness", this.TERRAIN_GlobalWetness);
		this.SetShaderParam("TERRAIN_RippleMap", this.TERRAIN_RippleMap);
		this.SetShaderParam("TERRAIN_RippleScale", this.TERRAIN_RippleScale);
		this.SetShaderParam("TERRAIN_FlowScale", this.TERRAIN_FlowScale);
		this.SetShaderParam("TERRAIN_FlowMipOffset", this.TERRAIN_FlowMipOffset);
		this.SetShaderParam("TERRAIN_FlowSpeed", this.TERRAIN_FlowSpeed);
		this.SetShaderParam("TERRAIN_FlowCycleScale", this.TERRAIN_FlowCycleScale);
		Shader.SetGlobalFloat("TERRAIN_RainIntensity", this.TERRAIN_RainIntensity);
		this.SetShaderParam("TERRAIN_DropletsSpeed", this.TERRAIN_DropletsSpeed);
		this.SetShaderParam("TERRAIN_WetDropletsStrength", this.TERRAIN_WetDropletsStrength);
		this.SetShaderParam("TERRAIN_WetDarkening", this.TERRAIN_WetDarkening);
		this.SetShaderParam("TERRAIN_mipoffset_flowSpeed", this.TERRAIN_mipoffset_flowSpeed);
		this.SetShaderParam("TERRAIN_WetHeight_Treshold", this.TERRAIN_WetHeight_Treshold);
		this.SetShaderParam("TERRAIN_WetHeight_Transition", this.TERRAIN_WetHeight_Transition);
		Shader.SetGlobalVector("RTP_LightDefVector", this.RTP_LightDefVector);
		this.SetShaderParam("_VerticalTextureGlobalBumpInfluence", this.VerticalTextureGlobalBumpInfluence);
		this.SetShaderParam("_VerticalTextureTiling", this.VerticalTextureTiling);
		this.SetShaderParam("_FarSpecCorrection0123", this.getVector(this.FarSpecCorrection, 0, 3));
		this.SetShaderParam("_MIPmult0123", this.getVector(this.MIPmult, 0, 3));
		this.SetShaderParam("_MixScale0123", this.getVector(this.MixScale, 0, 3));
		this.SetShaderParam("_MixBlend0123", this.getVector(this.MixBlend, 0, 3));
		this.SetShaderParam("_MixSaturation0123", this.getVector(this.MixSaturation, 0, 3));
		this.SetShaderParam("RTP_DiffFresnel0123", this.getVector(this.RTP_DiffFresnel, 0, 3));
		this.SetShaderParam("RTP_metallic0123", this.getVector(this.RTP_metallic, 0, 3));
		this.SetShaderParam("RTP_glossMin0123", this.getVector(this.RTP_glossMin, 0, 3));
		this.SetShaderParam("RTP_glossMax0123", this.getVector(this.RTP_glossMax, 0, 3));
		this.SetShaderParam("RTP_glitter0123", this.getVector(this.RTP_glitter, 0, 3));
		this.SetShaderParam("_MixBrightness0123", this.getVector(this.MixBrightness, 0, 3));
		this.SetShaderParam("_MixReplace0123", this.getVector(this.MixReplace, 0, 3));
		this.SetShaderParam("_LayerBrightness0123", this.MasterLayerBrightness * this.getVector(this.LayerBrightness, 0, 3));
		this.SetShaderParam("_LayerSaturation0123", this.MasterLayerSaturation * this.getVector(this.LayerSaturation, 0, 3));
		this.SetShaderParam("_LayerEmission0123", this.getVector(this.LayerEmission, 0, 3));
		this.SetShaderParam("_LayerEmissionColorR0123", this.getColorVector(this.LayerEmissionColor, 0, 3, 0));
		this.SetShaderParam("_LayerEmissionColorG0123", this.getColorVector(this.LayerEmissionColor, 0, 3, 1));
		this.SetShaderParam("_LayerEmissionColorB0123", this.getColorVector(this.LayerEmissionColor, 0, 3, 2));
		this.SetShaderParam("_LayerEmissionColorA0123", this.getColorVector(this.LayerEmissionColor, 0, 3, 3));
		this.SetShaderParam("_LayerBrightness2Spec0123", this.getVector(this.LayerBrightness2Spec, 0, 3));
		this.SetShaderParam("_LayerAlbedo2SpecColor0123", this.getVector(this.LayerAlbedo2SpecColor, 0, 3));
		this.SetShaderParam("_LayerEmissionRefractStrength0123", this.getVector(this.LayerEmissionRefractStrength, 0, 3));
		this.SetShaderParam("_LayerEmissionRefractHBedge0123", this.getVector(this.LayerEmissionRefractHBedge, 0, 3));
		this.SetShaderParam("_GlobalColorPerLayer0123", this.getVector(this.GlobalColorPerLayer, 0, 3));
		this.SetShaderParam("_GlobalColorBottom0123", this.getVector(this.GlobalColorBottom, 0, 3));
		this.SetShaderParam("_GlobalColorTop0123", this.getVector(this.GlobalColorTop, 0, 3));
		this.SetShaderParam("_GlobalColorColormapLoSat0123", this.getVector(this.GlobalColorColormapLoSat, 0, 3));
		this.SetShaderParam("_GlobalColorColormapHiSat0123", this.getVector(this.GlobalColorColormapHiSat, 0, 3));
		this.SetShaderParam("_GlobalColorLayerLoSat0123", this.getVector(this.GlobalColorLayerLoSat, 0, 3));
		this.SetShaderParam("_GlobalColorLayerHiSat0123", this.getVector(this.GlobalColorLayerHiSat, 0, 3));
		this.SetShaderParam("_GlobalColorLoBlend0123", this.getVector(this.GlobalColorLoBlend, 0, 3));
		this.SetShaderParam("_GlobalColorHiBlend0123", this.getVector(this.GlobalColorHiBlend, 0, 3));
		this.SetShaderParam("PER_LAYER_HEIGHT_MODIFIER0123", this.getVector(this.PER_LAYER_HEIGHT_MODIFIER, 0, 3));
		this.SetShaderParam("rtp_snow_strength_per_layer0123", this.getVector(this._snow_strength_per_layer, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultA0123", this.getVector(this._SuperDetailStrengthMultA, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultB0123", this.getVector(this._SuperDetailStrengthMultB, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthNormal0123", this.getVector(this._SuperDetailStrengthNormal, 0, 3));
		this.SetShaderParam("_BumpMapGlobalStrength0123", this.getVector(this._BumpMapGlobalStrength, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultASelfMaskNear0123", this.getVector(this._SuperDetailStrengthMultASelfMaskNear, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultASelfMaskFar0123", this.getVector(this._SuperDetailStrengthMultASelfMaskFar, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear0123", this.getVector(this._SuperDetailStrengthMultBSelfMaskNear, 0, 3));
		this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar0123", this.getVector(this._SuperDetailStrengthMultBSelfMaskFar, 0, 3));
		this.SetShaderParam("TERRAIN_LayerWetStrength0123", this.getVector(this.TERRAIN_LayerWetStrength, 0, 3));
		this.SetShaderParam("TERRAIN_WaterLevel0123", this.getVector(this.TERRAIN_WaterLevel, 0, 3));
		this.SetShaderParam("TERRAIN_WaterLevelSlopeDamp0123", this.getVector(this.TERRAIN_WaterLevelSlopeDamp, 0, 3));
		this.SetShaderParam("TERRAIN_WaterEdge0123", this.getVector(this.TERRAIN_WaterEdge, 0, 3));
		this.SetShaderParam("TERRAIN_WaterGloss0123", this.getVector(this.TERRAIN_WaterGloss, 0, 3));
		this.SetShaderParam("TERRAIN_WaterGlossDamper0123", this.getVector(this.TERRAIN_WaterGlossDamper, 0, 3));
		this.SetShaderParam("TERRAIN_Refraction0123", this.getVector(this.TERRAIN_Refraction, 0, 3));
		this.SetShaderParam("TERRAIN_WetRefraction0123", this.getVector(this.TERRAIN_WetRefraction, 0, 3));
		this.SetShaderParam("TERRAIN_Flow0123", this.getVector(this.TERRAIN_Flow, 0, 3));
		this.SetShaderParam("TERRAIN_WetFlow0123", this.getVector(this.TERRAIN_WetFlow, 0, 3));
		this.SetShaderParam("TERRAIN_WaterMetallic0123", this.getVector(this.TERRAIN_WaterMetallic, 0, 3));
		this.SetShaderParam("TERRAIN_WetGloss0123", this.getVector(this.TERRAIN_WetGloss, 0, 3));
		this.SetShaderParam("TERRAIN_WaterColorR0123", this.getColorVector(this.TERRAIN_WaterColor, 0, 3, 0));
		this.SetShaderParam("TERRAIN_WaterColorG0123", this.getColorVector(this.TERRAIN_WaterColor, 0, 3, 1));
		this.SetShaderParam("TERRAIN_WaterColorB0123", this.getColorVector(this.TERRAIN_WaterColor, 0, 3, 2));
		this.SetShaderParam("TERRAIN_WaterColorA0123", this.getColorVector(this.TERRAIN_WaterColor, 0, 3, 3));
		this.SetShaderParam("TERRAIN_WaterEmission0123", this.getVector(this.TERRAIN_WaterEmission, 0, 3));
		this.SetShaderParam("_GlitterStrength0123", this.getVector(this._GlitterStrength, 0, 3));
		this.SetShaderParam("RTP_AO_0123", this.getVector(this.AO_strength, 0, 3));
		this.SetShaderParam("_VerticalTexture0123", this.getVector(this.VerticalTextureStrength, 0, 3));
		if (this.numLayers > 4 && this._4LAYERS_SHADER_USED)
		{
			this.SetShaderParam("_FarSpecCorrection89AB", this.getVector(this.FarSpecCorrection, 4, 7));
			this.SetShaderParam("_MIPmult89AB", this.getVector(this.MIPmult, 4, 7));
			this.SetShaderParam("_MixScale89AB", this.getVector(this.MixScale, 4, 7));
			this.SetShaderParam("_MixBlend89AB", this.getVector(this.MixBlend, 4, 7));
			this.SetShaderParam("_MixSaturation89AB", this.getVector(this.MixSaturation, 4, 7));
			this.SetShaderParam("RTP_DiffFresnel89AB", this.getVector(this.RTP_DiffFresnel, 4, 7));
			this.SetShaderParam("RTP_metallic89AB", this.getVector(this.RTP_metallic, 4, 7));
			this.SetShaderParam("RTP_glossMin89AB", this.getVector(this.RTP_glossMin, 4, 7));
			this.SetShaderParam("RTP_glossMax89AB", this.getVector(this.RTP_glossMax, 4, 7));
			this.SetShaderParam("RTP_glitter89AB", this.getVector(this.RTP_glitter, 4, 7));
			this.SetShaderParam("_MixBrightness89AB", this.getVector(this.MixBrightness, 4, 7));
			this.SetShaderParam("_MixReplace89AB", this.getVector(this.MixReplace, 4, 7));
			this.SetShaderParam("_LayerBrightness89AB", this.MasterLayerBrightness * this.getVector(this.LayerBrightness, 4, 7));
			this.SetShaderParam("_LayerSaturation89AB", this.MasterLayerSaturation * this.getVector(this.LayerSaturation, 4, 7));
			this.SetShaderParam("_LayerEmission89AB", this.getVector(this.LayerEmission, 4, 7));
			this.SetShaderParam("_LayerEmissionColorR89AB", this.getColorVector(this.LayerEmissionColor, 4, 7, 0));
			this.SetShaderParam("_LayerEmissionColorG89AB", this.getColorVector(this.LayerEmissionColor, 4, 7, 1));
			this.SetShaderParam("_LayerEmissionColorB89AB", this.getColorVector(this.LayerEmissionColor, 4, 7, 2));
			this.SetShaderParam("_LayerEmissionColorA89AB", this.getColorVector(this.LayerEmissionColor, 4, 7, 3));
			this.SetShaderParam("_LayerBrightness2Spec89AB", this.getVector(this.LayerBrightness2Spec, 4, 7));
			this.SetShaderParam("_LayerAlbedo2SpecColor89AB", this.getVector(this.LayerAlbedo2SpecColor, 4, 7));
			this.SetShaderParam("_LayerEmissionRefractStrength89AB", this.getVector(this.LayerEmissionRefractStrength, 4, 7));
			this.SetShaderParam("_LayerEmissionRefractHBedge89AB", this.getVector(this.LayerEmissionRefractHBedge, 4, 7));
			this.SetShaderParam("_GlobalColorPerLayer89AB", this.getVector(this.GlobalColorPerLayer, 4, 7));
			this.SetShaderParam("_GlobalColorBottom89AB", this.getVector(this.GlobalColorBottom, 4, 7));
			this.SetShaderParam("_GlobalColorTop89AB", this.getVector(this.GlobalColorTop, 4, 7));
			this.SetShaderParam("_GlobalColorColormapLoSat89AB", this.getVector(this.GlobalColorColormapLoSat, 4, 7));
			this.SetShaderParam("_GlobalColorColormapHiSat89AB", this.getVector(this.GlobalColorColormapHiSat, 4, 7));
			this.SetShaderParam("_GlobalColorLayerLoSat89AB", this.getVector(this.GlobalColorLayerLoSat, 4, 7));
			this.SetShaderParam("_GlobalColorLayerHiSat89AB", this.getVector(this.GlobalColorLayerHiSat, 4, 7));
			this.SetShaderParam("_GlobalColorLoBlend89AB", this.getVector(this.GlobalColorLoBlend, 4, 7));
			this.SetShaderParam("_GlobalColorHiBlend89AB", this.getVector(this.GlobalColorHiBlend, 4, 7));
			this.SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", this.getVector(this.PER_LAYER_HEIGHT_MODIFIER, 4, 7));
			this.SetShaderParam("rtp_snow_strength_per_layer89AB", this.getVector(this._snow_strength_per_layer, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultA89AB", this.getVector(this._SuperDetailStrengthMultA, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultB89AB", this.getVector(this._SuperDetailStrengthMultB, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthNormal89AB", this.getVector(this._SuperDetailStrengthNormal, 4, 7));
			this.SetShaderParam("_BumpMapGlobalStrength89AB", this.getVector(this._BumpMapGlobalStrength, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", this.getVector(this._SuperDetailStrengthMultASelfMaskNear, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", this.getVector(this._SuperDetailStrengthMultASelfMaskFar, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", this.getVector(this._SuperDetailStrengthMultBSelfMaskNear, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", this.getVector(this._SuperDetailStrengthMultBSelfMaskFar, 4, 7));
			this.SetShaderParam("TERRAIN_LayerWetStrength89AB", this.getVector(this.TERRAIN_LayerWetStrength, 4, 7));
			this.SetShaderParam("TERRAIN_WaterLevel89AB", this.getVector(this.TERRAIN_WaterLevel, 4, 7));
			this.SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", this.getVector(this.TERRAIN_WaterLevelSlopeDamp, 4, 7));
			this.SetShaderParam("TERRAIN_WaterEdge89AB", this.getVector(this.TERRAIN_WaterEdge, 4, 7));
			this.SetShaderParam("TERRAIN_WaterGloss89AB", this.getVector(this.TERRAIN_WaterGloss, 4, 7));
			this.SetShaderParam("TERRAIN_WaterGlossDamper89AB", this.getVector(this.TERRAIN_WaterGlossDamper, 4, 7));
			this.SetShaderParam("TERRAIN_Refraction89AB", this.getVector(this.TERRAIN_Refraction, 4, 7));
			this.SetShaderParam("TERRAIN_WetRefraction89AB", this.getVector(this.TERRAIN_WetRefraction, 4, 7));
			this.SetShaderParam("TERRAIN_Flow89AB", this.getVector(this.TERRAIN_Flow, 4, 7));
			this.SetShaderParam("TERRAIN_WetFlow89AB", this.getVector(this.TERRAIN_WetFlow, 4, 7));
			this.SetShaderParam("TERRAIN_WaterMetallic89AB", this.getVector(this.TERRAIN_WaterMetallic, 4, 7));
			this.SetShaderParam("TERRAIN_WetGloss89AB", this.getVector(this.TERRAIN_WetGloss, 4, 7));
			this.SetShaderParam("TERRAIN_WaterColorR89AB", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 0));
			this.SetShaderParam("TERRAIN_WaterColorG89AB", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 1));
			this.SetShaderParam("TERRAIN_WaterColorB89AB", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 2));
			this.SetShaderParam("TERRAIN_WaterColorA89AB", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 3));
			this.SetShaderParam("TERRAIN_WaterEmission89AB", this.getVector(this.TERRAIN_WaterEmission, 4, 7));
			this.SetShaderParam("_GlitterStrength89AB", this.getVector(this._GlitterStrength, 4, 7));
			this.SetShaderParam("RTP_AO_89AB", this.getVector(this.AO_strength, 4, 7));
			this.SetShaderParam("_VerticalTexture89AB", this.getVector(this.VerticalTextureStrength, 4, 7));
		}
		else
		{
			this.SetShaderParam("_FarSpecCorrection4567", this.getVector(this.FarSpecCorrection, 4, 7));
			this.SetShaderParam("_MIPmult4567", this.getVector(this.MIPmult, 4, 7));
			this.SetShaderParam("_MixScale4567", this.getVector(this.MixScale, 4, 7));
			this.SetShaderParam("_MixBlend4567", this.getVector(this.MixBlend, 4, 7));
			this.SetShaderParam("_MixSaturation4567", this.getVector(this.MixSaturation, 4, 7));
			this.SetShaderParam("RTP_DiffFresnel4567", this.getVector(this.RTP_DiffFresnel, 4, 7));
			this.SetShaderParam("RTP_metallic4567", this.getVector(this.RTP_metallic, 4, 7));
			this.SetShaderParam("RTP_glossMin4567", this.getVector(this.RTP_glossMin, 4, 7));
			this.SetShaderParam("RTP_glossMax4567", this.getVector(this.RTP_glossMax, 4, 7));
			this.SetShaderParam("RTP_glitter4567", this.getVector(this.RTP_glitter, 4, 7));
			this.SetShaderParam("_MixBrightness4567", this.getVector(this.MixBrightness, 4, 7));
			this.SetShaderParam("_MixReplace4567", this.getVector(this.MixReplace, 4, 7));
			this.SetShaderParam("_LayerBrightness4567", this.MasterLayerBrightness * this.getVector(this.LayerBrightness, 4, 7));
			this.SetShaderParam("_LayerSaturation4567", this.MasterLayerSaturation * this.getVector(this.LayerSaturation, 4, 7));
			this.SetShaderParam("_LayerEmission4567", this.getVector(this.LayerEmission, 4, 7));
			this.SetShaderParam("_LayerEmissionColorR4567", this.getColorVector(this.LayerEmissionColor, 4, 7, 0));
			this.SetShaderParam("_LayerEmissionColorG4567", this.getColorVector(this.LayerEmissionColor, 4, 7, 1));
			this.SetShaderParam("_LayerEmissionColorB4567", this.getColorVector(this.LayerEmissionColor, 4, 7, 2));
			this.SetShaderParam("_LayerEmissionColorA4567", this.getColorVector(this.LayerEmissionColor, 4, 7, 3));
			this.SetShaderParam("_LayerBrightness2Spec4567", this.getVector(this.LayerBrightness2Spec, 4, 7));
			this.SetShaderParam("_LayerAlbedo2SpecColor4567", this.getVector(this.LayerAlbedo2SpecColor, 4, 7));
			this.SetShaderParam("_LayerEmissionRefractStrength4567", this.getVector(this.LayerEmissionRefractStrength, 4, 7));
			this.SetShaderParam("_LayerEmissionRefractHBedge4567", this.getVector(this.LayerEmissionRefractHBedge, 4, 7));
			this.SetShaderParam("_GlobalColorPerLayer4567", this.getVector(this.GlobalColorPerLayer, 4, 7));
			this.SetShaderParam("_GlobalColorBottom4567", this.getVector(this.GlobalColorBottom, 4, 7));
			this.SetShaderParam("_GlobalColorTop4567", this.getVector(this.GlobalColorTop, 4, 7));
			this.SetShaderParam("_GlobalColorColormapLoSat4567", this.getVector(this.GlobalColorColormapLoSat, 4, 7));
			this.SetShaderParam("_GlobalColorColormapHiSat4567", this.getVector(this.GlobalColorColormapHiSat, 4, 7));
			this.SetShaderParam("_GlobalColorLayerLoSat4567", this.getVector(this.GlobalColorLayerLoSat, 4, 7));
			this.SetShaderParam("_GlobalColorLayerHiSat4567", this.getVector(this.GlobalColorLayerHiSat, 4, 7));
			this.SetShaderParam("_GlobalColorLoBlend4567", this.getVector(this.GlobalColorLoBlend, 4, 7));
			this.SetShaderParam("_GlobalColorHiBlend4567", this.getVector(this.GlobalColorHiBlend, 4, 7));
			this.SetShaderParam("PER_LAYER_HEIGHT_MODIFIER4567", this.getVector(this.PER_LAYER_HEIGHT_MODIFIER, 4, 7));
			this.SetShaderParam("rtp_snow_strength_per_layer4567", this.getVector(this._snow_strength_per_layer, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultA4567", this.getVector(this._SuperDetailStrengthMultA, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultB4567", this.getVector(this._SuperDetailStrengthMultB, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthNormal4567", this.getVector(this._SuperDetailStrengthNormal, 4, 7));
			this.SetShaderParam("_BumpMapGlobalStrength4567", this.getVector(this._BumpMapGlobalStrength, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskNear4567", this.getVector(this._SuperDetailStrengthMultASelfMaskNear, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskFar4567", this.getVector(this._SuperDetailStrengthMultASelfMaskFar, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear4567", this.getVector(this._SuperDetailStrengthMultBSelfMaskNear, 4, 7));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar4567", this.getVector(this._SuperDetailStrengthMultBSelfMaskFar, 4, 7));
			this.SetShaderParam("TERRAIN_LayerWetStrength4567", this.getVector(this.TERRAIN_LayerWetStrength, 4, 7));
			this.SetShaderParam("TERRAIN_WaterLevel4567", this.getVector(this.TERRAIN_WaterLevel, 4, 7));
			this.SetShaderParam("TERRAIN_WaterLevelSlopeDamp4567", this.getVector(this.TERRAIN_WaterLevelSlopeDamp, 4, 7));
			this.SetShaderParam("TERRAIN_WaterEdge4567", this.getVector(this.TERRAIN_WaterEdge, 4, 7));
			this.SetShaderParam("TERRAIN_WaterGloss4567", this.getVector(this.TERRAIN_WaterGloss, 4, 7));
			this.SetShaderParam("TERRAIN_WaterGlossDamper4567", this.getVector(this.TERRAIN_WaterGlossDamper, 4, 7));
			this.SetShaderParam("TERRAIN_Refraction4567", this.getVector(this.TERRAIN_Refraction, 4, 7));
			this.SetShaderParam("TERRAIN_WetRefraction4567", this.getVector(this.TERRAIN_WetRefraction, 4, 7));
			this.SetShaderParam("TERRAIN_Flow4567", this.getVector(this.TERRAIN_Flow, 4, 7));
			this.SetShaderParam("TERRAIN_WetFlow4567", this.getVector(this.TERRAIN_WetFlow, 4, 7));
			this.SetShaderParam("TERRAIN_WaterMetallic4567", this.getVector(this.TERRAIN_WaterMetallic, 4, 7));
			this.SetShaderParam("TERRAIN_WetGloss4567", this.getVector(this.TERRAIN_WetGloss, 4, 7));
			this.SetShaderParam("TERRAIN_WaterColorR4567", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 0));
			this.SetShaderParam("TERRAIN_WaterColorG4567", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 1));
			this.SetShaderParam("TERRAIN_WaterColorB4567", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 2));
			this.SetShaderParam("TERRAIN_WaterColorA4567", this.getColorVector(this.TERRAIN_WaterColor, 4, 7, 3));
			this.SetShaderParam("TERRAIN_WaterEmission4567", this.getVector(this.TERRAIN_WaterEmission, 4, 7));
			this.SetShaderParam("_GlitterStrength4567", this.getVector(this._GlitterStrength, 4, 7));
			this.SetShaderParam("RTP_AO_4567", this.getVector(this.AO_strength, 4, 7));
			this.SetShaderParam("_VerticalTexture4567", this.getVector(this.VerticalTextureStrength, 4, 7));
			this.SetShaderParam("_FarSpecCorrection89AB", this.getVector(this.FarSpecCorrection, 8, 11));
			this.SetShaderParam("_MIPmult89AB", this.getVector(this.MIPmult, 8, 11));
			this.SetShaderParam("_MixScale89AB", this.getVector(this.MixScale, 8, 11));
			this.SetShaderParam("_MixBlend89AB", this.getVector(this.MixBlend, 8, 11));
			this.SetShaderParam("_MixSaturation89AB", this.getVector(this.MixSaturation, 8, 11));
			this.SetShaderParam("RTP_DiffFresnel89AB", this.getVector(this.RTP_DiffFresnel, 8, 11));
			this.SetShaderParam("RTP_metallic89AB", this.getVector(this.RTP_metallic, 8, 11));
			this.SetShaderParam("RTP_glossMin89AB", this.getVector(this.RTP_glossMin, 8, 11));
			this.SetShaderParam("RTP_glossMax89AB", this.getVector(this.RTP_glossMax, 8, 11));
			this.SetShaderParam("RTP_glitter89AB", this.getVector(this.RTP_glitter, 8, 11));
			this.SetShaderParam("_MixBrightness89AB", this.getVector(this.MixBrightness, 8, 11));
			this.SetShaderParam("_MixReplace89AB", this.getVector(this.MixReplace, 8, 11));
			this.SetShaderParam("_LayerBrightness89AB", this.MasterLayerBrightness * this.getVector(this.LayerBrightness, 8, 11));
			this.SetShaderParam("_LayerSaturation89AB", this.MasterLayerSaturation * this.getVector(this.LayerSaturation, 8, 11));
			this.SetShaderParam("_LayerEmission89AB", this.getVector(this.LayerEmission, 8, 11));
			this.SetShaderParam("_LayerEmissionColorR89AB", this.getColorVector(this.LayerEmissionColor, 8, 11, 0));
			this.SetShaderParam("_LayerEmissionColorG89AB", this.getColorVector(this.LayerEmissionColor, 8, 11, 1));
			this.SetShaderParam("_LayerEmissionColorB89AB", this.getColorVector(this.LayerEmissionColor, 8, 11, 2));
			this.SetShaderParam("_LayerEmissionColorA89AB", this.getColorVector(this.LayerEmissionColor, 8, 11, 3));
			this.SetShaderParam("_LayerBrightness2Spec89AB", this.getVector(this.LayerBrightness2Spec, 8, 11));
			this.SetShaderParam("_LayerAlbedo2SpecColor89AB", this.getVector(this.LayerAlbedo2SpecColor, 8, 11));
			this.SetShaderParam("_LayerEmissionRefractStrength89AB", this.getVector(this.LayerEmissionRefractStrength, 8, 11));
			this.SetShaderParam("_LayerEmissionRefractHBedge89AB", this.getVector(this.LayerEmissionRefractHBedge, 8, 11));
			this.SetShaderParam("_GlobalColorPerLayer89AB", this.getVector(this.GlobalColorPerLayer, 8, 11));
			this.SetShaderParam("_GlobalColorBottom89AB", this.getVector(this.GlobalColorBottom, 8, 11));
			this.SetShaderParam("_GlobalColorTop89AB", this.getVector(this.GlobalColorTop, 8, 11));
			this.SetShaderParam("_GlobalColorColormapLoSat89AB", this.getVector(this.GlobalColorColormapLoSat, 8, 11));
			this.SetShaderParam("_GlobalColorColormapHiSat89AB", this.getVector(this.GlobalColorColormapHiSat, 8, 11));
			this.SetShaderParam("_GlobalColorLayerLoSat89AB", this.getVector(this.GlobalColorLayerLoSat, 8, 11));
			this.SetShaderParam("_GlobalColorLayerHiSat89AB", this.getVector(this.GlobalColorLayerHiSat, 8, 11));
			this.SetShaderParam("_GlobalColorLoBlend89AB", this.getVector(this.GlobalColorLoBlend, 8, 11));
			this.SetShaderParam("_GlobalColorHiBlend89AB", this.getVector(this.GlobalColorHiBlend, 8, 11));
			this.SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", this.getVector(this.PER_LAYER_HEIGHT_MODIFIER, 8, 11));
			this.SetShaderParam("rtp_snow_strength_per_layer89AB", this.getVector(this._snow_strength_per_layer, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultA89AB", this.getVector(this._SuperDetailStrengthMultA, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultB89AB", this.getVector(this._SuperDetailStrengthMultB, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthNormal89AB", this.getVector(this._SuperDetailStrengthNormal, 8, 11));
			this.SetShaderParam("_BumpMapGlobalStrength89AB", this.getVector(this._BumpMapGlobalStrength, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", this.getVector(this._SuperDetailStrengthMultASelfMaskNear, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", this.getVector(this._SuperDetailStrengthMultASelfMaskFar, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", this.getVector(this._SuperDetailStrengthMultBSelfMaskNear, 8, 11));
			this.SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", this.getVector(this._SuperDetailStrengthMultBSelfMaskFar, 8, 11));
			this.SetShaderParam("TERRAIN_LayerWetStrength89AB", this.getVector(this.TERRAIN_LayerWetStrength, 8, 11));
			this.SetShaderParam("TERRAIN_WaterLevel89AB", this.getVector(this.TERRAIN_WaterLevel, 8, 11));
			this.SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", this.getVector(this.TERRAIN_WaterLevelSlopeDamp, 8, 11));
			this.SetShaderParam("TERRAIN_WaterEdge89AB", this.getVector(this.TERRAIN_WaterEdge, 8, 11));
			this.SetShaderParam("TERRAIN_WaterGloss89AB", this.getVector(this.TERRAIN_WaterGloss, 8, 11));
			this.SetShaderParam("TERRAIN_WaterGlossDamper89AB", this.getVector(this.TERRAIN_WaterGlossDamper, 8, 11));
			this.SetShaderParam("TERRAIN_Refraction89AB", this.getVector(this.TERRAIN_Refraction, 8, 11));
			this.SetShaderParam("TERRAIN_WetRefraction89AB", this.getVector(this.TERRAIN_WetRefraction, 8, 11));
			this.SetShaderParam("TERRAIN_Flow89AB", this.getVector(this.TERRAIN_Flow, 8, 11));
			this.SetShaderParam("TERRAIN_WetFlow89AB", this.getVector(this.TERRAIN_WetFlow, 8, 11));
			this.SetShaderParam("TERRAIN_WaterMetallic89AB", this.getVector(this.TERRAIN_WaterMetallic, 8, 11));
			this.SetShaderParam("TERRAIN_WetGloss89AB", this.getVector(this.TERRAIN_WetGloss, 8, 11));
			this.SetShaderParam("TERRAIN_WaterColorR89AB", this.getColorVector(this.TERRAIN_WaterColor, 8, 11, 0));
			this.SetShaderParam("TERRAIN_WaterColorG89AB", this.getColorVector(this.TERRAIN_WaterColor, 8, 11, 1));
			this.SetShaderParam("TERRAIN_WaterColorB89AB", this.getColorVector(this.TERRAIN_WaterColor, 8, 11, 2));
			this.SetShaderParam("TERRAIN_WaterColorA89AB", this.getColorVector(this.TERRAIN_WaterColor, 8, 11, 3));
			this.SetShaderParam("TERRAIN_WaterEmission89AB", this.getVector(this.TERRAIN_WaterEmission, 8, 11));
			this.SetShaderParam("_GlitterStrength89AB", this.getVector(this._GlitterStrength, 8, 11));
			this.SetShaderParam("RTP_AO_89AB", this.getVector(this.AO_strength, 8, 11));
			this.SetShaderParam("_VerticalTexture89AB", this.getVector(this.VerticalTextureStrength, 8, 11));
		}
		if (this.splat_atlases.Length == 2)
		{
			Texture2D texture2D = this.splat_atlases[0];
			Texture2D texture2D2 = this.splat_atlases[1];
			this.splat_atlases = new Texture2D[3];
			this.splat_atlases[0] = texture2D;
			this.splat_atlases[1] = texture2D2;
		}
		this.SetShaderParam("_SplatAtlasA", this.splat_atlases[0]);
		this.SetShaderParam("_BumpMap01", this.Bump01);
		this.SetShaderParam("_BumpMap23", this.Bump23);
		this.SetShaderParam("_TERRAIN_HeightMap", this.HeightMap);
		this.SetShaderParam("_SSColorCombinedA", this.SSColorCombinedA);
		if (this.numLayers > 4)
		{
			this.SetShaderParam("_SplatAtlasB", this.splat_atlases[1]);
			this.SetShaderParam("_SplatAtlasC", this.splat_atlases[1]);
			this.SetShaderParam("_TERRAIN_HeightMap2", this.HeightMap2);
			this.SetShaderParam("_SSColorCombinedB", this.SSColorCombinedB);
		}
		if (this.numLayers > 8)
		{
			this.SetShaderParam("_SplatAtlasC", this.splat_atlases[2]);
		}
		if (this.numLayers > 4 && this._4LAYERS_SHADER_USED)
		{
			this.SetShaderParam("_BumpMap89", this.Bump45);
			this.SetShaderParam("_BumpMapAB", this.Bump67);
			this.SetShaderParam("_TERRAIN_HeightMap3", this.HeightMap2);
			this.SetShaderParam("_BumpMap45", this.Bump45);
			this.SetShaderParam("_BumpMap67", this.Bump67);
		}
		else
		{
			this.SetShaderParam("_BumpMap45", this.Bump45);
			this.SetShaderParam("_BumpMap67", this.Bump67);
			this.SetShaderParam("_BumpMap89", this.Bump89);
			this.SetShaderParam("_BumpMapAB", this.BumpAB);
			this.SetShaderParam("_TERRAIN_HeightMap3", this.HeightMap3);
		}
		this.use_mat = null;
	}

	public Vector4 getVector(float[] vec, int idxA, int idxB)
	{
		if (vec == null)
		{
			return Vector4.zero;
		}
		Vector4 zero = Vector4.zero;
		for (int i = idxA; i <= idxB; i++)
		{
			if (i < vec.Length)
			{
				zero[i - idxA] = vec[i];
			}
		}
		return zero;
	}

	public Vector4 getColorVector(Color[] vec, int idxA, int idxB, int channel)
	{
		if (vec == null)
		{
			return Vector4.zero;
		}
		Vector4 zero = Vector4.zero;
		for (int i = idxA; i <= idxB; i++)
		{
			if (i < vec.Length)
			{
				zero[i - idxA] = vec[i][channel];
			}
		}
		return zero;
	}

	public Texture2D get_dumb_tex()
	{
		if (!this.dumb_tex)
		{
			this.dumb_tex = new Texture2D(32, 32, TextureFormat.RGB24, false);
			Color[] pixels = this.dumb_tex.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color.white;
			}
			this.dumb_tex.SetPixels(pixels);
			this.dumb_tex.Apply();
		}
		return this.dumb_tex;
	}

	public void SyncGlobalPropsAcrossTerrainGroups()
	{
		ReliefTerrain[] array = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
		ReliefTerrainGlobalSettingsHolder[] array2 = new ReliefTerrainGlobalSettingsHolder[array.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			for (int j = 0; j < num; j++)
			{
				if (array2[j] == array[i].globalSettingsHolder)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				array2[num++] = array[i].globalSettingsHolder;
			}
		}
		for (int k = 0; k < num; k++)
		{
			if (array2[k] != this)
			{
				array2[k].trees_shadow_distance_start = this.trees_shadow_distance_start;
				array2[k].trees_shadow_distance_transition = this.trees_shadow_distance_transition;
				array2[k].trees_shadow_value = this.trees_shadow_value;
				array2[k].global_normalMap_multiplier = this.global_normalMap_multiplier;
				array2[k].trees_pixel_distance_start = this.trees_pixel_distance_start;
				array2[k].trees_pixel_distance_transition = this.trees_pixel_distance_transition;
				array2[k].trees_pixel_blend_val = this.trees_pixel_blend_val;
				array2[k].global_normalMap_farUsage = this.global_normalMap_farUsage;
				array2[k]._AmbientEmissiveMultiplier = this._AmbientEmissiveMultiplier;
				array2[k]._AmbientEmissiveRelief = this._AmbientEmissiveRelief;
				array2[k]._snow_strength = this._snow_strength;
				array2[k]._global_color_brightness_to_snow = this._global_color_brightness_to_snow;
				array2[k]._snow_slope_factor = this._snow_slope_factor;
				array2[k]._snow_edge_definition = this._snow_edge_definition;
				array2[k]._snow_height_treshold = this._snow_height_treshold;
				array2[k]._snow_height_transition = this._snow_height_transition;
				array2[k]._snow_color = this._snow_color;
				array2[k]._snow_gloss = this._snow_gloss;
				array2[k]._snow_reflectivness = this._snow_reflectivness;
				array2[k]._snow_deep_factor = this._snow_deep_factor;
				array2[k]._snow_diff_fresnel = this._snow_diff_fresnel;
				array2[k]._snow_metallic = this._snow_metallic;
				array2[k]._snow_Frost = this._snow_Frost;
				array2[k]._snow_MicroTiling = this._snow_MicroTiling;
				array2[k]._snow_BumpMicro = this._snow_BumpMicro;
				array2[k]._snow_occlusionStrength = this._snow_occlusionStrength;
				array2[k]._snow_TranslucencyDeferredLightIndex = this._snow_TranslucencyDeferredLightIndex;
				array2[k]._SnowGlitterColor = this._SnowGlitterColor;
				array2[k]._GlitterColor = this._GlitterColor;
				array2[k]._GlitterTiling = this._GlitterTiling;
				array2[k]._GlitterDensity = this._GlitterDensity;
				array2[k]._GlitterFilter = this._GlitterFilter;
				array2[k]._GlitterColorization = this._GlitterColorization;
				array2[k]._SparkleMap = this._SparkleMap;
				array2[k].TERRAIN_CausticsWaterLevel = this.TERRAIN_CausticsWaterLevel;
				array2[k].TERRAIN_CausticsWaterLevelByAngle = this.TERRAIN_CausticsWaterLevelByAngle;
				array2[k].TERRAIN_CausticsWaterDeepFadeLength = this.TERRAIN_CausticsWaterDeepFadeLength;
				array2[k].TERRAIN_CausticsWaterShallowFadeLength = this.TERRAIN_CausticsWaterShallowFadeLength;
				array2[k].TERRAIN_GlobalWetness = this.TERRAIN_GlobalWetness;
				array2[k].TERRAIN_RainIntensity = this.TERRAIN_RainIntensity;
				array2[k].RTP_LightDefVector = this.RTP_LightDefVector;
			}
		}
	}

	public void RestorePreset(ReliefTerrainPresetHolder holder)
	{
		this.terrainLayers = holder.terrainLayers;
		this.numLayers = holder.numLayers;
		this.splats = new Texture2D[holder.splats.Length];
		for (int i = 0; i < holder.splats.Length; i++)
		{
			this.splats[i] = holder.splats[i];
		}
		this.splat_atlases = new Texture2D[3];
		for (int j = 0; j < this.splat_atlases.Length; j++)
		{
			this.splat_atlases[j] = holder.splat_atlases[j];
		}
		this.TERRAIN_CausticsMask = holder.TERRAIN_CausticsMask;
		this.globalCausticsModifed_flag = holder.globalCausticsModifed_flag;
		this.TERRAIN_CausticsMaskWorld2UV = holder.TERRAIN_CausticsMaskWorld2UV;
		this.causticsMaskSize = holder.causticsMaskSize;
		this.TERRAIN_PuddleMask = holder.TERRAIN_PuddleMask;
		this.TERRAIN_PuddleLevel = holder.TERRAIN_PuddleLevel;
		this.puddleMaskSize = holder.puddleMaskSize;
		this.globalPuddleModified_flag = holder.globalPuddleModified_flag;
		this.TERRAIN_WetMask = holder.TERRAIN_WetMask;
		this.wetMaskSize = holder.wetMaskSize;
		this.globalWetModified_flag = holder.globalWetModified_flag;
		this.RTP_MIP_BIAS = holder.RTP_MIP_BIAS;
		this.MasterLayerBrightness = holder.MasterLayerBrightness;
		this.MasterLayerSaturation = holder.MasterLayerSaturation;
		this.SuperDetailA_channel = holder.SuperDetailA_channel;
		this.SuperDetailB_channel = holder.SuperDetailB_channel;
		this.Bump01 = holder.Bump01;
		this.Bump23 = holder.Bump23;
		this.Bump45 = holder.Bump45;
		this.Bump67 = holder.Bump67;
		this.Bump89 = holder.Bump89;
		this.BumpAB = holder.BumpAB;
		this.SSColorCombinedA = holder.SSColorCombinedA;
		this.SSColorCombinedB = holder.SSColorCombinedB;
		this.BumpGlobal = holder.BumpGlobal;
		this.VerticalTexture = holder.VerticalTexture;
		this.BumpMapGlobalScale = holder.BumpMapGlobalScale;
		this.GlobalColorMapBlendValues = holder.GlobalColorMapBlendValues;
		this.GlobalColorMapSaturation = holder.GlobalColorMapSaturation;
		this.GlobalColorMapSaturationFar = holder.GlobalColorMapSaturationFar;
		this.GlobalColorMapDistortByPerlin = holder.GlobalColorMapDistortByPerlin;
		this.GlobalColorMapBrightness = holder.GlobalColorMapBrightness;
		this.GlobalColorMapBrightnessFar = holder.GlobalColorMapBrightnessFar;
		this._GlobalColorMapNearMIP = holder._GlobalColorMapNearMIP;
		this._FarNormalDamp = holder._FarNormalDamp;
		this.blendMultiplier = holder.blendMultiplier;
		this.HeightMap = holder.HeightMap;
		this.HeightMap2 = holder.HeightMap2;
		this.HeightMap3 = holder.HeightMap3;
		this.ReliefTransform = holder.ReliefTransform;
		this.DIST_STEPS = holder.DIST_STEPS;
		this.WAVELENGTH = holder.WAVELENGTH;
		this.ReliefBorderBlend = holder.ReliefBorderBlend;
		this.ExtrudeHeight = holder.ExtrudeHeight;
		this.LightmapShading = holder.LightmapShading;
		this.SHADOW_STEPS = holder.SHADOW_STEPS;
		this.WAVELENGTH_SHADOWS = holder.WAVELENGTH_SHADOWS;
		this.SelfShadowStrength = holder.SelfShadowStrength;
		this.ShadowSmoothing = holder.ShadowSmoothing;
		this.ShadowSoftnessFade = holder.ShadowSoftnessFade;
		this.CJ_flattenShadows = holder.CJ_flattenShadows;
		this.distance_start = holder.distance_start;
		this.distance_transition = holder.distance_transition;
		this.distance_start_bumpglobal = holder.distance_start_bumpglobal;
		this.distance_transition_bumpglobal = holder.distance_transition_bumpglobal;
		this.rtp_perlin_start_val = holder.rtp_perlin_start_val;
		this._Phong = holder._Phong;
		this.tessHeight = holder.tessHeight;
		this._TessSubdivisions = holder._TessSubdivisions;
		this._TessSubdivisionsFar = holder._TessSubdivisionsFar;
		this._TessYOffset = holder._TessYOffset;
		this.trees_shadow_distance_start = holder.trees_shadow_distance_start;
		this.trees_shadow_distance_transition = holder.trees_shadow_distance_transition;
		this.trees_shadow_value = holder.trees_shadow_value;
		this.trees_pixel_distance_start = holder.trees_pixel_distance_start;
		this.trees_pixel_distance_transition = holder.trees_pixel_distance_transition;
		this.trees_pixel_blend_val = holder.trees_pixel_blend_val;
		this.global_normalMap_multiplier = holder.global_normalMap_multiplier;
		this.global_normalMap_farUsage = holder.global_normalMap_farUsage;
		this._AmbientEmissiveMultiplier = holder._AmbientEmissiveMultiplier;
		this._AmbientEmissiveRelief = holder._AmbientEmissiveRelief;
		this.rtp_mipoffset_globalnorm = holder.rtp_mipoffset_globalnorm;
		this._SuperDetailTiling = holder._SuperDetailTiling;
		this.SuperDetailA = holder.SuperDetailA;
		this.SuperDetailB = holder.SuperDetailB;
		this.TERRAIN_GlobalWetness = holder.TERRAIN_GlobalWetness;
		this.TERRAIN_RippleMap = holder.TERRAIN_RippleMap;
		this.TERRAIN_RippleScale = holder.TERRAIN_RippleScale;
		this.TERRAIN_FlowScale = holder.TERRAIN_FlowScale;
		this.TERRAIN_FlowSpeed = holder.TERRAIN_FlowSpeed;
		this.TERRAIN_FlowCycleScale = holder.TERRAIN_FlowCycleScale;
		this.TERRAIN_FlowMipOffset = holder.TERRAIN_FlowMipOffset;
		this.TERRAIN_WetDarkening = holder.TERRAIN_WetDarkening;
		this.TERRAIN_WetDropletsStrength = holder.TERRAIN_WetDropletsStrength;
		this.TERRAIN_WetHeight_Treshold = holder.TERRAIN_WetHeight_Treshold;
		this.TERRAIN_WetHeight_Transition = holder.TERRAIN_WetHeight_Transition;
		this.TERRAIN_RainIntensity = holder.TERRAIN_RainIntensity;
		this.TERRAIN_DropletsSpeed = holder.TERRAIN_DropletsSpeed;
		this.TERRAIN_mipoffset_flowSpeed = holder.TERRAIN_mipoffset_flowSpeed;
		this.TERRAIN_CausticsAnimSpeed = holder.TERRAIN_CausticsAnimSpeed;
		this.TERRAIN_CausticsColor = holder.TERRAIN_CausticsColor;
		this.TERRAIN_CausticsWaterLevel = holder.TERRAIN_CausticsWaterLevel;
		this.TERRAIN_CausticsWaterLevelByAngle = holder.TERRAIN_CausticsWaterLevelByAngle;
		this.TERRAIN_CausticsWaterDeepFadeLength = holder.TERRAIN_CausticsWaterDeepFadeLength;
		this.TERRAIN_CausticsWaterShallowFadeLength = holder.TERRAIN_CausticsWaterShallowFadeLength;
		this.TERRAIN_CausticsTilingScale = holder.TERRAIN_CausticsTilingScale;
		this.TERRAIN_CausticsTex = holder.TERRAIN_CausticsTex;
		this.RTP_AOsharpness = holder.RTP_AOsharpness;
		this.RTP_AOamp = holder.RTP_AOamp;
		this._occlusionStrength = holder._occlusionStrength;
		this.RTP_LightDefVector = holder.RTP_LightDefVector;
		this.EmissionRefractFiltering = holder.EmissionRefractFiltering;
		this.EmissionRefractAnimSpeed = holder.EmissionRefractAnimSpeed;
		this.VerticalTextureGlobalBumpInfluence = holder.VerticalTextureGlobalBumpInfluence;
		this.VerticalTextureTiling = holder.VerticalTextureTiling;
		this._snow_strength = holder._snow_strength;
		this._global_color_brightness_to_snow = holder._global_color_brightness_to_snow;
		this._snow_slope_factor = holder._snow_slope_factor;
		this._snow_edge_definition = holder._snow_edge_definition;
		this._snow_height_treshold = holder._snow_height_treshold;
		this._snow_height_transition = holder._snow_height_transition;
		this._snow_color = holder._snow_color;
		this._snow_gloss = holder._snow_gloss;
		this._snow_reflectivness = holder._snow_reflectivness;
		this._snow_deep_factor = holder._snow_deep_factor;
		this._snow_diff_fresnel = holder._snow_diff_fresnel;
		this._snow_metallic = holder._snow_metallic;
		this._snow_Frost = holder._snow_Frost;
		this._snow_MicroTiling = holder._snow_MicroTiling;
		this._snow_BumpMicro = holder._snow_BumpMicro;
		this._snow_occlusionStrength = holder._snow_occlusionStrength;
		this._snow_TranslucencyDeferredLightIndex = holder._snow_TranslucencyDeferredLightIndex;
		this._SnowGlitterColor = holder._SnowGlitterColor;
		this._GlitterColor = holder._GlitterColor;
		this._GlitterTiling = holder._GlitterTiling;
		this._GlitterDensity = holder._GlitterDensity;
		this._GlitterFilter = holder._GlitterFilter;
		this._GlitterColorization = holder._GlitterColorization;
		this._SparkleMap = holder._SparkleMap;
		this.Bumps = new Texture2D[holder.Bumps.Length];
		this.FarSpecCorrection = new float[holder.Bumps.Length];
		this.MixScale = new float[holder.Bumps.Length];
		this.MixBlend = new float[holder.Bumps.Length];
		this.MixSaturation = new float[holder.Bumps.Length];
		this.RTP_DiffFresnel = new float[holder.Bumps.Length];
		this.RTP_metallic = new float[holder.Bumps.Length];
		this.RTP_glossMin = new float[holder.Bumps.Length];
		this.RTP_glossMax = new float[holder.Bumps.Length];
		this.RTP_glitter = new float[holder.Bumps.Length];
		this.RTP_heightMin = new float[holder.Bumps.Length];
		this.RTP_heightMax = new float[holder.Bumps.Length];
		this.MixBrightness = new float[holder.Bumps.Length];
		this.MixReplace = new float[holder.Bumps.Length];
		this.LayerBrightness = new float[holder.Bumps.Length];
		this.LayerBrightness2Spec = new float[holder.Bumps.Length];
		this.LayerAlbedo2SpecColor = new float[holder.Bumps.Length];
		this.LayerSaturation = new float[holder.Bumps.Length];
		this.LayerEmission = new float[holder.Bumps.Length];
		this.LayerEmissionColor = new Color[holder.Bumps.Length];
		this.LayerEmissionRefractStrength = new float[holder.Bumps.Length];
		this.LayerEmissionRefractHBedge = new float[holder.Bumps.Length];
		this.GlobalColorPerLayer = new float[holder.Bumps.Length];
		this.GlobalColorBottom = new float[holder.Bumps.Length];
		this.GlobalColorTop = new float[holder.Bumps.Length];
		this.GlobalColorColormapLoSat = new float[holder.Bumps.Length];
		this.GlobalColorColormapHiSat = new float[holder.Bumps.Length];
		this.GlobalColorLayerLoSat = new float[holder.Bumps.Length];
		this.GlobalColorLayerHiSat = new float[holder.Bumps.Length];
		this.GlobalColorLoBlend = new float[holder.Bumps.Length];
		this.GlobalColorHiBlend = new float[holder.Bumps.Length];
		this.PER_LAYER_HEIGHT_MODIFIER = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultA = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultASelfMaskNear = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultASelfMaskFar = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultB = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultBSelfMaskNear = new float[holder.Bumps.Length];
		this._SuperDetailStrengthMultBSelfMaskFar = new float[holder.Bumps.Length];
		this._SuperDetailStrengthNormal = new float[holder.Bumps.Length];
		this._BumpMapGlobalStrength = new float[holder.Bumps.Length];
		this.AO_strength = new float[holder.Bumps.Length];
		this.VerticalTextureStrength = new float[holder.Bumps.Length];
		this.Heights = new Texture2D[holder.Bumps.Length];
		this._snow_strength_per_layer = new float[holder.Bumps.Length];
		this.TERRAIN_LayerWetStrength = new float[holder.Bumps.Length];
		this.TERRAIN_WaterLevel = new float[holder.Bumps.Length];
		this.TERRAIN_WaterLevelSlopeDamp = new float[holder.Bumps.Length];
		this.TERRAIN_WaterEdge = new float[holder.Bumps.Length];
		this.TERRAIN_WaterGloss = new float[holder.Bumps.Length];
		this.TERRAIN_WaterGlossDamper = new float[holder.Bumps.Length];
		this.TERRAIN_Refraction = new float[holder.Bumps.Length];
		this.TERRAIN_WetRefraction = new float[holder.Bumps.Length];
		this.TERRAIN_Flow = new float[holder.Bumps.Length];
		this.TERRAIN_WetFlow = new float[holder.Bumps.Length];
		this.TERRAIN_WaterMetallic = new float[holder.Bumps.Length];
		this.TERRAIN_WetGloss = new float[holder.Bumps.Length];
		this.TERRAIN_WaterColor = new Color[holder.Bumps.Length];
		this.TERRAIN_WaterEmission = new float[holder.Bumps.Length];
		this._GlitterStrength = new float[holder.Bumps.Length];
		for (int k = 0; k < holder.Bumps.Length; k++)
		{
			this.Bumps[k] = holder.Bumps[k];
			this.FarSpecCorrection[k] = holder.FarSpecCorrection[k];
			this.MixScale[k] = holder.MixScale[k];
			this.MixBlend[k] = holder.MixBlend[k];
			this.MixSaturation[k] = holder.MixSaturation[k];
			this.CheckAndUpdate(ref holder.RTP_DiffFresnel, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_metallic, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_glossMin, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_glossMax, 1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_glitter, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_heightMin, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.RTP_heightMax, 1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WaterGloss, 0.1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WaterGlossDamper, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WaterMetallic, 0.1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WetGloss, 0.05f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WetFlow, 0.05f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.MixBrightness, 2f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.MixReplace, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerBrightness, 1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerBrightness2Spec, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerAlbedo2SpecColor, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerSaturation, 1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerEmission, 1f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerEmissionColor, Color.black, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.FarSpecCorrection, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerEmissionRefractStrength, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.LayerEmissionRefractHBedge, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder.TERRAIN_WaterEmission, 0f, holder.Bumps.Length);
			this.CheckAndUpdate(ref holder._GlitterStrength, 0f, holder.Bumps.Length);
			this.RTP_DiffFresnel[k] = holder.RTP_DiffFresnel[k];
			this.RTP_metallic[k] = holder.RTP_metallic[k];
			this.RTP_glossMin[k] = holder.RTP_glossMin[k];
			this.RTP_glossMax[k] = holder.RTP_glossMax[k];
			this.RTP_glitter[k] = holder.RTP_glitter[k];
			this.RTP_heightMin[k] = holder.RTP_heightMin[k];
			this.RTP_heightMax[k] = holder.RTP_heightMax[k];
			this.MixBrightness[k] = holder.MixBrightness[k];
			this.MixReplace[k] = holder.MixReplace[k];
			this.LayerBrightness[k] = holder.LayerBrightness[k];
			this.LayerBrightness2Spec[k] = holder.LayerBrightness2Spec[k];
			this.LayerAlbedo2SpecColor[k] = holder.LayerAlbedo2SpecColor[k];
			this.LayerSaturation[k] = holder.LayerSaturation[k];
			this.LayerEmission[k] = holder.LayerEmission[k];
			this.LayerEmissionColor[k] = holder.LayerEmissionColor[k];
			this.LayerEmissionRefractStrength[k] = holder.LayerEmissionRefractStrength[k];
			this.LayerEmissionRefractHBedge[k] = holder.LayerEmissionRefractHBedge[k];
			this.GlobalColorPerLayer[k] = holder.GlobalColorPerLayer[k];
			this.GlobalColorBottom[k] = holder.GlobalColorBottom[k];
			this.GlobalColorTop[k] = holder.GlobalColorTop[k];
			this.GlobalColorColormapLoSat[k] = holder.GlobalColorColormapLoSat[k];
			this.GlobalColorColormapHiSat[k] = holder.GlobalColorColormapHiSat[k];
			this.GlobalColorLayerLoSat[k] = holder.GlobalColorLayerLoSat[k];
			this.GlobalColorLayerHiSat[k] = holder.GlobalColorLayerHiSat[k];
			this.GlobalColorLoBlend[k] = holder.GlobalColorLoBlend[k];
			this.GlobalColorHiBlend[k] = holder.GlobalColorHiBlend[k];
			this.PER_LAYER_HEIGHT_MODIFIER[k] = holder.PER_LAYER_HEIGHT_MODIFIER[k];
			this._SuperDetailStrengthMultA[k] = holder._SuperDetailStrengthMultA[k];
			this._SuperDetailStrengthMultASelfMaskNear[k] = holder._SuperDetailStrengthMultASelfMaskNear[k];
			this._SuperDetailStrengthMultASelfMaskFar[k] = holder._SuperDetailStrengthMultASelfMaskFar[k];
			this._SuperDetailStrengthMultB[k] = holder._SuperDetailStrengthMultB[k];
			this._SuperDetailStrengthMultBSelfMaskNear[k] = holder._SuperDetailStrengthMultBSelfMaskNear[k];
			this._SuperDetailStrengthMultBSelfMaskFar[k] = holder._SuperDetailStrengthMultBSelfMaskFar[k];
			this._SuperDetailStrengthNormal[k] = holder._SuperDetailStrengthNormal[k];
			this._BumpMapGlobalStrength[k] = holder._BumpMapGlobalStrength[k];
			this.VerticalTextureStrength[k] = holder.VerticalTextureStrength[k];
			this.AO_strength[k] = holder.AO_strength[k];
			this.Heights[k] = holder.Heights[k];
			this._snow_strength_per_layer[k] = holder._snow_strength_per_layer[k];
			this.TERRAIN_LayerWetStrength[k] = holder.TERRAIN_LayerWetStrength[k];
			this.TERRAIN_WaterLevel[k] = holder.TERRAIN_WaterLevel[k];
			this.TERRAIN_WaterLevelSlopeDamp[k] = holder.TERRAIN_WaterLevelSlopeDamp[k];
			this.TERRAIN_WaterEdge[k] = holder.TERRAIN_WaterEdge[k];
			this.TERRAIN_WaterGloss[k] = holder.TERRAIN_WaterGloss[k];
			this.TERRAIN_WaterGlossDamper[k] = holder.TERRAIN_WaterGlossDamper[k];
			this.TERRAIN_Refraction[k] = holder.TERRAIN_Refraction[k];
			this.TERRAIN_WetRefraction[k] = holder.TERRAIN_WetRefraction[k];
			this.TERRAIN_Flow[k] = holder.TERRAIN_Flow[k];
			this.TERRAIN_WetFlow[k] = holder.TERRAIN_WetFlow[k];
			this.TERRAIN_WaterMetallic[k] = holder.TERRAIN_WaterMetallic[k];
			this.TERRAIN_WetGloss[k] = holder.TERRAIN_WetGloss[k];
			this.TERRAIN_WaterColor[k] = holder.TERRAIN_WaterColor[k];
			this.TERRAIN_WaterEmission[k] = holder.TERRAIN_WaterEmission[k];
			this._GlitterStrength[k] = holder._GlitterStrength[k];
		}
	}

	public void GlossBakeJob(Texture2D detailTex, Texture2D normalTex)
	{
		this.bakeJobArray = new Texture2D[]
		{
			detailTex,
			normalTex
		};
	}

	public void SavePreset(ref ReliefTerrainPresetHolder holder)
	{
		holder.terrainLayers = this.terrainLayers;
		holder.numLayers = this.numLayers;
		holder.splats = new Texture2D[this.splats.Length];
		for (int i = 0; i < holder.splats.Length; i++)
		{
			holder.splats[i] = this.splats[i];
		}
		holder.splat_atlases = new Texture2D[3];
		for (int j = 0; j < this.splat_atlases.Length; j++)
		{
			holder.splat_atlases[j] = this.splat_atlases[j];
		}
		holder.TERRAIN_CausticsMask = this.TERRAIN_CausticsMask;
		holder.globalCausticsModifed_flag = this.globalCausticsModifed_flag;
		holder.TERRAIN_CausticsMaskWorld2UV = this.TERRAIN_CausticsMaskWorld2UV;
		holder.causticsMaskSize = this.causticsMaskSize;
		holder.TERRAIN_PuddleMask = this.TERRAIN_PuddleMask;
		holder.TERRAIN_PuddleLevel = this.TERRAIN_PuddleLevel;
		holder.puddleMaskSize = this.puddleMaskSize;
		holder.globalPuddleModified_flag = this.globalPuddleModified_flag;
		holder.TERRAIN_WetMask = this.TERRAIN_WetMask;
		holder.wetMaskSize = this.wetMaskSize;
		holder.globalWetModified_flag = this.globalWetModified_flag;
		holder.RTP_MIP_BIAS = this.RTP_MIP_BIAS;
		holder.MasterLayerBrightness = this.MasterLayerBrightness;
		holder.MasterLayerSaturation = this.MasterLayerSaturation;
		holder.SuperDetailA_channel = this.SuperDetailA_channel;
		holder.SuperDetailB_channel = this.SuperDetailB_channel;
		holder.Bump01 = this.Bump01;
		holder.Bump23 = this.Bump23;
		holder.Bump45 = this.Bump45;
		holder.Bump67 = this.Bump67;
		holder.Bump89 = this.Bump89;
		holder.BumpAB = this.BumpAB;
		holder.SSColorCombinedA = this.SSColorCombinedA;
		holder.SSColorCombinedB = this.SSColorCombinedB;
		holder.BumpGlobal = this.BumpGlobal;
		holder.VerticalTexture = this.VerticalTexture;
		holder.BumpMapGlobalScale = this.BumpMapGlobalScale;
		holder.GlobalColorMapBlendValues = this.GlobalColorMapBlendValues;
		holder.GlobalColorMapSaturation = this.GlobalColorMapSaturation;
		holder.GlobalColorMapSaturationFar = this.GlobalColorMapSaturationFar;
		holder.GlobalColorMapDistortByPerlin = this.GlobalColorMapDistortByPerlin;
		holder.GlobalColorMapBrightness = this.GlobalColorMapBrightness;
		holder.GlobalColorMapBrightnessFar = this.GlobalColorMapBrightnessFar;
		holder._GlobalColorMapNearMIP = this._GlobalColorMapNearMIP;
		holder._FarNormalDamp = this._FarNormalDamp;
		holder.blendMultiplier = this.blendMultiplier;
		holder.HeightMap = this.HeightMap;
		holder.HeightMap2 = this.HeightMap2;
		holder.HeightMap3 = this.HeightMap3;
		holder.ReliefTransform = this.ReliefTransform;
		holder.DIST_STEPS = this.DIST_STEPS;
		holder.WAVELENGTH = this.WAVELENGTH;
		holder.ReliefBorderBlend = this.ReliefBorderBlend;
		holder.ExtrudeHeight = this.ExtrudeHeight;
		holder.LightmapShading = this.LightmapShading;
		holder.SHADOW_STEPS = this.SHADOW_STEPS;
		holder.WAVELENGTH_SHADOWS = this.WAVELENGTH_SHADOWS;
		holder.SelfShadowStrength = this.SelfShadowStrength;
		holder.ShadowSmoothing = this.ShadowSmoothing;
		holder.ShadowSoftnessFade = this.ShadowSoftnessFade;
		holder.CJ_flattenShadows = this.CJ_flattenShadows;
		holder.distance_start = this.distance_start;
		holder.distance_transition = this.distance_transition;
		holder.distance_start_bumpglobal = this.distance_start_bumpglobal;
		holder.distance_transition_bumpglobal = this.distance_transition_bumpglobal;
		holder.rtp_perlin_start_val = this.rtp_perlin_start_val;
		holder._Phong = this._Phong;
		holder.tessHeight = this.tessHeight;
		holder._TessSubdivisions = this._TessSubdivisions;
		holder._TessSubdivisionsFar = this._TessSubdivisionsFar;
		holder._TessYOffset = this._TessYOffset;
		holder.trees_shadow_distance_start = this.trees_shadow_distance_start;
		holder.trees_shadow_distance_transition = this.trees_shadow_distance_transition;
		holder.trees_shadow_value = this.trees_shadow_value;
		holder.trees_pixel_distance_start = this.trees_pixel_distance_start;
		holder.trees_pixel_distance_transition = this.trees_pixel_distance_transition;
		holder.trees_pixel_blend_val = this.trees_pixel_blend_val;
		holder.global_normalMap_multiplier = this.global_normalMap_multiplier;
		holder.global_normalMap_farUsage = this.global_normalMap_farUsage;
		holder._AmbientEmissiveMultiplier = this._AmbientEmissiveMultiplier;
		holder._AmbientEmissiveRelief = this._AmbientEmissiveRelief;
		holder.rtp_mipoffset_globalnorm = this.rtp_mipoffset_globalnorm;
		holder._SuperDetailTiling = this._SuperDetailTiling;
		holder.SuperDetailA = this.SuperDetailA;
		holder.SuperDetailB = this.SuperDetailB;
		holder.TERRAIN_GlobalWetness = this.TERRAIN_GlobalWetness;
		holder.TERRAIN_RippleMap = this.TERRAIN_RippleMap;
		holder.TERRAIN_RippleScale = this.TERRAIN_RippleScale;
		holder.TERRAIN_FlowScale = this.TERRAIN_FlowScale;
		holder.TERRAIN_FlowSpeed = this.TERRAIN_FlowSpeed;
		holder.TERRAIN_FlowCycleScale = this.TERRAIN_FlowCycleScale;
		holder.TERRAIN_FlowMipOffset = this.TERRAIN_FlowMipOffset;
		holder.TERRAIN_WetDarkening = this.TERRAIN_WetDarkening;
		holder.TERRAIN_WetDropletsStrength = this.TERRAIN_WetDropletsStrength;
		holder.TERRAIN_WetHeight_Treshold = this.TERRAIN_WetHeight_Treshold;
		holder.TERRAIN_WetHeight_Transition = this.TERRAIN_WetHeight_Transition;
		holder.TERRAIN_RainIntensity = this.TERRAIN_RainIntensity;
		holder.TERRAIN_DropletsSpeed = this.TERRAIN_DropletsSpeed;
		holder.TERRAIN_mipoffset_flowSpeed = this.TERRAIN_mipoffset_flowSpeed;
		holder.TERRAIN_CausticsAnimSpeed = this.TERRAIN_CausticsAnimSpeed;
		holder.TERRAIN_CausticsColor = this.TERRAIN_CausticsColor;
		holder.TERRAIN_CausticsWaterLevel = this.TERRAIN_CausticsWaterLevel;
		holder.TERRAIN_CausticsWaterLevelByAngle = this.TERRAIN_CausticsWaterLevelByAngle;
		holder.TERRAIN_CausticsWaterDeepFadeLength = this.TERRAIN_CausticsWaterDeepFadeLength;
		holder.TERRAIN_CausticsWaterShallowFadeLength = this.TERRAIN_CausticsWaterShallowFadeLength;
		holder.TERRAIN_CausticsTilingScale = this.TERRAIN_CausticsTilingScale;
		holder.TERRAIN_CausticsTex = this.TERRAIN_CausticsTex;
		holder.RTP_AOsharpness = this.RTP_AOsharpness;
		holder.RTP_AOamp = this.RTP_AOamp;
		holder._occlusionStrength = this._occlusionStrength;
		holder.RTP_LightDefVector = this.RTP_LightDefVector;
		holder.EmissionRefractFiltering = this.EmissionRefractFiltering;
		holder.EmissionRefractAnimSpeed = this.EmissionRefractAnimSpeed;
		holder.VerticalTextureGlobalBumpInfluence = this.VerticalTextureGlobalBumpInfluence;
		holder.VerticalTextureTiling = this.VerticalTextureTiling;
		holder._snow_strength = this._snow_strength;
		holder._global_color_brightness_to_snow = this._global_color_brightness_to_snow;
		holder._snow_slope_factor = this._snow_slope_factor;
		holder._snow_edge_definition = this._snow_edge_definition;
		holder._snow_height_treshold = this._snow_height_treshold;
		holder._snow_height_transition = this._snow_height_transition;
		holder._snow_color = this._snow_color;
		holder._snow_gloss = this._snow_gloss;
		holder._snow_reflectivness = this._snow_reflectivness;
		holder._snow_deep_factor = this._snow_deep_factor;
		holder._snow_diff_fresnel = this._snow_diff_fresnel;
		holder._snow_metallic = this._snow_metallic;
		holder._snow_Frost = this._snow_Frost;
		holder._snow_MicroTiling = this._snow_MicroTiling;
		holder._snow_BumpMicro = this._snow_BumpMicro;
		holder._snow_occlusionStrength = this._snow_occlusionStrength;
		holder._snow_TranslucencyDeferredLightIndex = this._snow_TranslucencyDeferredLightIndex;
		holder._SnowGlitterColor = this._SnowGlitterColor;
		holder._GlitterColor = this._GlitterColor;
		holder._GlitterTiling = this._GlitterTiling;
		holder._GlitterDensity = this._GlitterDensity;
		holder._GlitterFilter = this._GlitterFilter;
		holder._GlitterColorization = this._GlitterColorization;
		holder._SparkleMap = this._SparkleMap;
		holder.Bumps = new Texture2D[this.numLayers];
		holder.Spec = new float[this.numLayers];
		holder.FarSpecCorrection = new float[this.numLayers];
		holder.MixScale = new float[this.numLayers];
		holder.MixBlend = new float[this.numLayers];
		holder.MixSaturation = new float[this.numLayers];
		holder.RTP_DiffFresnel = new float[this.numLayers];
		holder.RTP_metallic = new float[this.numLayers];
		holder.RTP_glossMin = new float[this.numLayers];
		holder.RTP_glossMax = new float[this.numLayers];
		holder.RTP_glitter = new float[this.numLayers];
		holder.RTP_heightMin = new float[this.numLayers];
		holder.RTP_heightMax = new float[this.numLayers];
		holder.MixBrightness = new float[this.numLayers];
		holder.MixReplace = new float[this.numLayers];
		holder.LayerBrightness = new float[this.numLayers];
		holder.LayerBrightness2Spec = new float[this.numLayers];
		holder.LayerAlbedo2SpecColor = new float[this.numLayers];
		holder.LayerSaturation = new float[this.numLayers];
		holder.LayerEmission = new float[this.numLayers];
		holder.LayerEmissionColor = new Color[this.numLayers];
		holder.LayerEmissionRefractStrength = new float[this.numLayers];
		holder.LayerEmissionRefractHBedge = new float[this.numLayers];
		holder.GlobalColorPerLayer = new float[this.numLayers];
		holder.GlobalColorBottom = new float[this.numLayers];
		holder.GlobalColorTop = new float[this.numLayers];
		holder.GlobalColorColormapLoSat = new float[this.numLayers];
		holder.GlobalColorColormapHiSat = new float[this.numLayers];
		holder.GlobalColorLayerLoSat = new float[this.numLayers];
		holder.GlobalColorLayerHiSat = new float[this.numLayers];
		holder.GlobalColorLoBlend = new float[this.numLayers];
		holder.GlobalColorHiBlend = new float[this.numLayers];
		holder.PER_LAYER_HEIGHT_MODIFIER = new float[this.numLayers];
		holder._SuperDetailStrengthMultA = new float[this.numLayers];
		holder._SuperDetailStrengthMultASelfMaskNear = new float[this.numLayers];
		holder._SuperDetailStrengthMultASelfMaskFar = new float[this.numLayers];
		holder._SuperDetailStrengthMultB = new float[this.numLayers];
		holder._SuperDetailStrengthMultBSelfMaskNear = new float[this.numLayers];
		holder._SuperDetailStrengthMultBSelfMaskFar = new float[this.numLayers];
		holder._SuperDetailStrengthNormal = new float[this.numLayers];
		holder._BumpMapGlobalStrength = new float[this.numLayers];
		holder.VerticalTextureStrength = new float[this.numLayers];
		holder.AO_strength = new float[this.numLayers];
		holder.Heights = new Texture2D[this.numLayers];
		holder._snow_strength_per_layer = new float[this.numLayers];
		holder.TERRAIN_LayerWetStrength = new float[this.numLayers];
		holder.TERRAIN_WaterLevel = new float[this.numLayers];
		holder.TERRAIN_WaterLevelSlopeDamp = new float[this.numLayers];
		holder.TERRAIN_WaterEdge = new float[this.numLayers];
		holder.TERRAIN_WaterGloss = new float[this.numLayers];
		holder.TERRAIN_WaterGlossDamper = new float[this.numLayers];
		holder.TERRAIN_Refraction = new float[this.numLayers];
		holder.TERRAIN_WetRefraction = new float[this.numLayers];
		holder.TERRAIN_Flow = new float[this.numLayers];
		holder.TERRAIN_WetFlow = new float[this.numLayers];
		holder.TERRAIN_WaterMetallic = new float[this.numLayers];
		holder.TERRAIN_WetGloss = new float[this.numLayers];
		holder.TERRAIN_WaterColor = new Color[this.numLayers];
		holder.TERRAIN_WaterEmission = new float[this.numLayers];
		holder._GlitterStrength = new float[this.numLayers];
		for (int k = 0; k < this.numLayers; k++)
		{
			holder.Bumps[k] = this.Bumps[k];
			holder.FarSpecCorrection[k] = this.FarSpecCorrection[k];
			holder.MixScale[k] = this.MixScale[k];
			holder.MixBlend[k] = this.MixBlend[k];
			holder.MixSaturation[k] = this.MixSaturation[k];
			this.CheckAndUpdate(ref this.RTP_DiffFresnel, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_metallic, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_glossMin, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_glossMax, 1f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_glitter, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_heightMin, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.RTP_heightMax, 1f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WaterGloss, 0.1f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WaterGlossDamper, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WaterMetallic, 0.1f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WetGloss, 0.05f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WetFlow, 0.05f, this.numLayers);
			this.CheckAndUpdate(ref this.MixBrightness, 2f, this.numLayers);
			this.CheckAndUpdate(ref this.MixReplace, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerBrightness, 1f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerBrightness2Spec, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerAlbedo2SpecColor, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerSaturation, 1f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerEmission, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerEmissionColor, Color.black, this.numLayers);
			this.CheckAndUpdate(ref this.LayerEmissionRefractStrength, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.LayerEmissionRefractHBedge, 0f, this.numLayers);
			this.CheckAndUpdate(ref this.TERRAIN_WaterEmission, 0.5f, this.numLayers);
			this.CheckAndUpdate(ref this._GlitterStrength, 0f, this.numLayers);
			holder.RTP_DiffFresnel[k] = this.RTP_DiffFresnel[k];
			holder.RTP_metallic[k] = this.RTP_metallic[k];
			holder.RTP_glossMin[k] = this.RTP_glossMin[k];
			holder.RTP_glossMax[k] = this.RTP_glossMax[k];
			holder.RTP_glitter[k] = this.RTP_glitter[k];
			holder.RTP_heightMin[k] = this.RTP_heightMin[k];
			holder.RTP_heightMax[k] = this.RTP_heightMax[k];
			holder.TERRAIN_WaterEmission[k] = this.TERRAIN_WaterEmission[k];
			holder.MixBrightness[k] = this.MixBrightness[k];
			holder.MixReplace[k] = this.MixReplace[k];
			holder.LayerBrightness[k] = this.LayerBrightness[k];
			holder.LayerBrightness2Spec[k] = this.LayerBrightness2Spec[k];
			holder.LayerAlbedo2SpecColor[k] = this.LayerAlbedo2SpecColor[k];
			holder.LayerSaturation[k] = this.LayerSaturation[k];
			holder.LayerEmission[k] = this.LayerEmission[k];
			holder.LayerEmissionColor[k] = this.LayerEmissionColor[k];
			holder.LayerEmissionRefractStrength[k] = this.LayerEmissionRefractStrength[k];
			holder.LayerEmissionRefractHBedge[k] = this.LayerEmissionRefractHBedge[k];
			holder.GlobalColorPerLayer[k] = this.GlobalColorPerLayer[k];
			holder.GlobalColorBottom[k] = this.GlobalColorBottom[k];
			holder.GlobalColorTop[k] = this.GlobalColorTop[k];
			holder.GlobalColorColormapLoSat[k] = this.GlobalColorColormapLoSat[k];
			holder.GlobalColorColormapHiSat[k] = this.GlobalColorColormapHiSat[k];
			holder.GlobalColorLayerLoSat[k] = this.GlobalColorLayerLoSat[k];
			holder.GlobalColorLayerHiSat[k] = this.GlobalColorLayerHiSat[k];
			holder.GlobalColorLoBlend[k] = this.GlobalColorLoBlend[k];
			holder.GlobalColorHiBlend[k] = this.GlobalColorHiBlend[k];
			holder.PER_LAYER_HEIGHT_MODIFIER[k] = this.PER_LAYER_HEIGHT_MODIFIER[k];
			holder._SuperDetailStrengthMultA[k] = this._SuperDetailStrengthMultA[k];
			holder._SuperDetailStrengthMultASelfMaskNear[k] = this._SuperDetailStrengthMultASelfMaskNear[k];
			holder._SuperDetailStrengthMultASelfMaskFar[k] = this._SuperDetailStrengthMultASelfMaskFar[k];
			holder._SuperDetailStrengthMultB[k] = this._SuperDetailStrengthMultB[k];
			holder._SuperDetailStrengthMultBSelfMaskNear[k] = this._SuperDetailStrengthMultBSelfMaskNear[k];
			holder._SuperDetailStrengthMultBSelfMaskFar[k] = this._SuperDetailStrengthMultBSelfMaskFar[k];
			holder._SuperDetailStrengthNormal[k] = this._SuperDetailStrengthNormal[k];
			holder._BumpMapGlobalStrength[k] = this._BumpMapGlobalStrength[k];
			holder.VerticalTextureStrength[k] = this.VerticalTextureStrength[k];
			holder.AO_strength[k] = this.AO_strength[k];
			holder.Heights[k] = this.Heights[k];
			holder._snow_strength_per_layer[k] = this._snow_strength_per_layer[k];
			holder.TERRAIN_LayerWetStrength[k] = this.TERRAIN_LayerWetStrength[k];
			holder.TERRAIN_WaterLevel[k] = this.TERRAIN_WaterLevel[k];
			holder.TERRAIN_WaterLevelSlopeDamp[k] = this.TERRAIN_WaterLevelSlopeDamp[k];
			holder.TERRAIN_WaterEdge[k] = this.TERRAIN_WaterEdge[k];
			holder.TERRAIN_WaterGloss[k] = this.TERRAIN_WaterGloss[k];
			holder.TERRAIN_WaterGlossDamper[k] = this.TERRAIN_WaterGlossDamper[k];
			holder.TERRAIN_Refraction[k] = this.TERRAIN_Refraction[k];
			holder.TERRAIN_WetRefraction[k] = this.TERRAIN_WetRefraction[k];
			holder.TERRAIN_Flow[k] = this.TERRAIN_Flow[k];
			holder.TERRAIN_WetFlow[k] = this.TERRAIN_WetFlow[k];
			holder.TERRAIN_WaterMetallic[k] = this.TERRAIN_WaterMetallic[k];
			holder.TERRAIN_WetGloss[k] = this.TERRAIN_WetGloss[k];
			holder.TERRAIN_WaterColor[k] = this.TERRAIN_WaterColor[k];
			holder._GlitterStrength[k] = this._GlitterStrength[k];
		}
	}

	public void InterpolatePresets(ReliefTerrainPresetHolder holderA, ReliefTerrainPresetHolder holderB, float t)
	{
		this.RTP_MIP_BIAS = Mathf.Lerp(holderA.RTP_MIP_BIAS, holderB.RTP_MIP_BIAS, t);
		this.MasterLayerBrightness = Mathf.Lerp(holderA.MasterLayerBrightness, holderB.MasterLayerBrightness, t);
		this.MasterLayerSaturation = Mathf.Lerp(holderA.MasterLayerSaturation, holderB.MasterLayerSaturation, t);
		this.BumpMapGlobalScale = Mathf.Lerp(holderA.BumpMapGlobalScale, holderB.BumpMapGlobalScale, t);
		this.GlobalColorMapBlendValues = Vector3.Lerp(holderA.GlobalColorMapBlendValues, holderB.GlobalColorMapBlendValues, t);
		this.GlobalColorMapSaturation = Mathf.Lerp(holderA.GlobalColorMapSaturation, holderB.GlobalColorMapSaturation, t);
		this.GlobalColorMapSaturationFar = Mathf.Lerp(holderA.GlobalColorMapSaturationFar, holderB.GlobalColorMapSaturationFar, t);
		this.GlobalColorMapDistortByPerlin = Mathf.Lerp(holderA.GlobalColorMapDistortByPerlin, holderB.GlobalColorMapDistortByPerlin, t);
		this.GlobalColorMapBrightness = Mathf.Lerp(holderA.GlobalColorMapBrightness, holderB.GlobalColorMapBrightness, t);
		this.GlobalColorMapBrightnessFar = Mathf.Lerp(holderA.GlobalColorMapBrightnessFar, holderB.GlobalColorMapBrightnessFar, t);
		this._GlobalColorMapNearMIP = Mathf.Lerp(holderA._GlobalColorMapNearMIP, holderB._GlobalColorMapNearMIP, t);
		this._FarNormalDamp = Mathf.Lerp(holderA._FarNormalDamp, holderB._FarNormalDamp, t);
		this.blendMultiplier = Mathf.Lerp(holderA.blendMultiplier, holderB.blendMultiplier, t);
		this.ReliefTransform = Vector4.Lerp(holderA.ReliefTransform, holderB.ReliefTransform, t);
		this.DIST_STEPS = Mathf.Lerp(holderA.DIST_STEPS, holderB.DIST_STEPS, t);
		this.WAVELENGTH = Mathf.Lerp(holderA.WAVELENGTH, holderB.WAVELENGTH, t);
		this.ReliefBorderBlend = Mathf.Lerp(holderA.ReliefBorderBlend, holderB.ReliefBorderBlend, t);
		this.ExtrudeHeight = Mathf.Lerp(holderA.ExtrudeHeight, holderB.ExtrudeHeight, t);
		this.LightmapShading = Mathf.Lerp(holderA.LightmapShading, holderB.LightmapShading, t);
		this.SHADOW_STEPS = Mathf.Lerp(holderA.SHADOW_STEPS, holderB.SHADOW_STEPS, t);
		this.WAVELENGTH_SHADOWS = Mathf.Lerp(holderA.WAVELENGTH_SHADOWS, holderB.WAVELENGTH_SHADOWS, t);
		this.SelfShadowStrength = Mathf.Lerp(holderA.SelfShadowStrength, holderB.SelfShadowStrength, t);
		this.ShadowSmoothing = Mathf.Lerp(holderA.ShadowSmoothing, holderB.ShadowSmoothing, t);
		this.ShadowSoftnessFade = Mathf.Lerp(holderA.ShadowSoftnessFade, holderB.ShadowSoftnessFade, t);
		this.CJ_flattenShadows = Mathf.Lerp(holderA.CJ_flattenShadows, holderB.CJ_flattenShadows, t);
		this.distance_start = Mathf.Lerp(holderA.distance_start, holderB.distance_start, t);
		this.distance_transition = Mathf.Lerp(holderA.distance_transition, holderB.distance_transition, t);
		this.distance_start_bumpglobal = Mathf.Lerp(holderA.distance_start_bumpglobal, holderB.distance_start_bumpglobal, t);
		this.distance_transition_bumpglobal = Mathf.Lerp(holderA.distance_transition_bumpglobal, holderB.distance_transition_bumpglobal, t);
		this.rtp_perlin_start_val = Mathf.Lerp(holderA.rtp_perlin_start_val, holderB.rtp_perlin_start_val, t);
		this.trees_shadow_distance_start = Mathf.Lerp(holderA.trees_shadow_distance_start, holderB.trees_shadow_distance_start, t);
		this.trees_shadow_distance_transition = Mathf.Lerp(holderA.trees_shadow_distance_transition, holderB.trees_shadow_distance_transition, t);
		this.trees_shadow_value = Mathf.Lerp(holderA.trees_shadow_value, holderB.trees_shadow_value, t);
		this.trees_pixel_distance_start = Mathf.Lerp(holderA.trees_pixel_distance_start, holderB.trees_pixel_distance_start, t);
		this.trees_pixel_distance_transition = Mathf.Lerp(holderA.trees_pixel_distance_transition, holderB.trees_pixel_distance_transition, t);
		this.trees_pixel_blend_val = Mathf.Lerp(holderA.trees_pixel_blend_val, holderB.trees_pixel_blend_val, t);
		this.global_normalMap_multiplier = Mathf.Lerp(holderA.global_normalMap_multiplier, holderB.global_normalMap_multiplier, t);
		this.global_normalMap_farUsage = Mathf.Lerp(holderA.global_normalMap_farUsage, holderB.global_normalMap_farUsage, t);
		this._AmbientEmissiveMultiplier = Mathf.Lerp(holderA._AmbientEmissiveMultiplier, holderB._AmbientEmissiveMultiplier, t);
		this._AmbientEmissiveRelief = Mathf.Lerp(holderA._AmbientEmissiveRelief, holderB._AmbientEmissiveRelief, t);
		this._SuperDetailTiling = Mathf.Lerp(holderA._SuperDetailTiling, holderB._SuperDetailTiling, t);
		this.TERRAIN_GlobalWetness = Mathf.Lerp(holderA.TERRAIN_GlobalWetness, holderB.TERRAIN_GlobalWetness, t);
		this.TERRAIN_RippleScale = Mathf.Lerp(holderA.TERRAIN_RippleScale, holderB.TERRAIN_RippleScale, t);
		this.TERRAIN_FlowScale = Mathf.Lerp(holderA.TERRAIN_FlowScale, holderB.TERRAIN_FlowScale, t);
		this.TERRAIN_FlowSpeed = Mathf.Lerp(holderA.TERRAIN_FlowSpeed, holderB.TERRAIN_FlowSpeed, t);
		this.TERRAIN_FlowCycleScale = Mathf.Lerp(holderA.TERRAIN_FlowCycleScale, holderB.TERRAIN_FlowCycleScale, t);
		this.TERRAIN_FlowMipOffset = Mathf.Lerp(holderA.TERRAIN_FlowMipOffset, holderB.TERRAIN_FlowMipOffset, t);
		this.TERRAIN_WetDarkening = Mathf.Lerp(holderA.TERRAIN_WetDarkening, holderB.TERRAIN_WetDarkening, t);
		this.TERRAIN_WetDropletsStrength = Mathf.Lerp(holderA.TERRAIN_WetDropletsStrength, holderB.TERRAIN_WetDropletsStrength, t);
		this.TERRAIN_WetHeight_Treshold = Mathf.Lerp(holderA.TERRAIN_WetHeight_Treshold, holderB.TERRAIN_WetHeight_Treshold, t);
		this.TERRAIN_WetHeight_Transition = Mathf.Lerp(holderA.TERRAIN_WetHeight_Transition, holderB.TERRAIN_WetHeight_Transition, t);
		this.TERRAIN_RainIntensity = Mathf.Lerp(holderA.TERRAIN_RainIntensity, holderB.TERRAIN_RainIntensity, t);
		this.TERRAIN_DropletsSpeed = Mathf.Lerp(holderA.TERRAIN_DropletsSpeed, holderB.TERRAIN_DropletsSpeed, t);
		this.TERRAIN_mipoffset_flowSpeed = Mathf.Lerp(holderA.TERRAIN_mipoffset_flowSpeed, holderB.TERRAIN_mipoffset_flowSpeed, t);
		this.TERRAIN_CausticsAnimSpeed = Mathf.Lerp(holderA.TERRAIN_CausticsAnimSpeed, holderB.TERRAIN_CausticsAnimSpeed, t);
		this.TERRAIN_CausticsColor = Color.Lerp(holderA.TERRAIN_CausticsColor, holderB.TERRAIN_CausticsColor, t);
		this.TERRAIN_CausticsWaterLevel = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevel, holderB.TERRAIN_CausticsWaterLevel, t);
		this.TERRAIN_CausticsWaterLevelByAngle = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevelByAngle, holderB.TERRAIN_CausticsWaterLevelByAngle, t);
		this.TERRAIN_CausticsWaterDeepFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterDeepFadeLength, holderB.TERRAIN_CausticsWaterDeepFadeLength, t);
		this.TERRAIN_CausticsWaterShallowFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterShallowFadeLength, holderB.TERRAIN_CausticsWaterShallowFadeLength, t);
		this.TERRAIN_CausticsTilingScale = Mathf.Lerp(holderA.TERRAIN_CausticsTilingScale, holderB.TERRAIN_CausticsTilingScale, t);
		this.RTP_AOsharpness = Mathf.Lerp(holderA.RTP_AOsharpness, holderB.RTP_AOsharpness, t);
		this.RTP_AOamp = Mathf.Lerp(holderA.RTP_AOamp, holderB.RTP_AOamp, t);
		this._occlusionStrength = Mathf.Lerp(holderA._occlusionStrength, holderB._occlusionStrength, t);
		this.RTP_LightDefVector = Vector4.Lerp(holderA.RTP_LightDefVector, holderB.RTP_LightDefVector, t);
		this.EmissionRefractFiltering = Mathf.Lerp(holderA.EmissionRefractFiltering, holderB.EmissionRefractFiltering, t);
		this.EmissionRefractAnimSpeed = Mathf.Lerp(holderA.EmissionRefractAnimSpeed, holderB.EmissionRefractAnimSpeed, t);
		this.VerticalTextureGlobalBumpInfluence = Mathf.Lerp(holderA.VerticalTextureGlobalBumpInfluence, holderB.VerticalTextureGlobalBumpInfluence, t);
		this.VerticalTextureTiling = Mathf.Lerp(holderA.VerticalTextureTiling, holderB.VerticalTextureTiling, t);
		this._snow_strength = Mathf.Lerp(holderA._snow_strength, holderB._snow_strength, t);
		this._global_color_brightness_to_snow = Mathf.Lerp(holderA._global_color_brightness_to_snow, holderB._global_color_brightness_to_snow, t);
		this._snow_slope_factor = Mathf.Lerp(holderA._snow_slope_factor, holderB._snow_slope_factor, t);
		this._snow_edge_definition = Mathf.Lerp(holderA._snow_edge_definition, holderB._snow_edge_definition, t);
		this._snow_height_treshold = Mathf.Lerp(holderA._snow_height_treshold, holderB._snow_height_treshold, t);
		this._snow_height_transition = Mathf.Lerp(holderA._snow_height_transition, holderB._snow_height_transition, t);
		this._snow_color = Color.Lerp(holderA._snow_color, holderB._snow_color, t);
		this._snow_gloss = Mathf.Lerp(holderA._snow_gloss, holderB._snow_gloss, t);
		this._snow_reflectivness = Mathf.Lerp(holderA._snow_reflectivness, holderB._snow_reflectivness, t);
		this._snow_deep_factor = Mathf.Lerp(holderA._snow_deep_factor, holderB._snow_deep_factor, t);
		this._snow_diff_fresnel = Mathf.Lerp(holderA._snow_diff_fresnel, holderB._snow_diff_fresnel, t);
		this._snow_metallic = Mathf.Lerp(holderA._snow_metallic, holderB._snow_metallic, t);
		this._snow_Frost = Mathf.Lerp(holderA._snow_Frost, holderB._snow_Frost, t);
		this._snow_MicroTiling = Mathf.Lerp(holderA._snow_MicroTiling, holderB._snow_MicroTiling, t);
		this._snow_BumpMicro = Mathf.Lerp(holderA._snow_BumpMicro, holderB._snow_BumpMicro, t);
		this._snow_occlusionStrength = Mathf.Lerp(holderA._snow_occlusionStrength, holderB._snow_occlusionStrength, t);
		this._SnowGlitterColor = Color.Lerp(holderA._SnowGlitterColor, holderB._SnowGlitterColor, t);
		this._GlitterColor = Color.Lerp(holderA._GlitterColor, holderB._GlitterColor, t);
		this._GlitterTiling = Mathf.Lerp(holderA._GlitterTiling, holderB._GlitterTiling, t);
		this._GlitterDensity = Mathf.Lerp(holderA._GlitterDensity, holderB._GlitterDensity, t);
		this._GlitterFilter = Mathf.Lerp(holderA._GlitterFilter, holderB._GlitterFilter, t);
		this._GlitterColorization = Mathf.Lerp(holderA._GlitterColorization, holderB._GlitterColorization, t);
		for (int i = 0; i < holderA.Spec.Length; i++)
		{
			if (i < this.FarSpecCorrection.Length)
			{
				this.FarSpecCorrection[i] = Mathf.Lerp(holderA.FarSpecCorrection[i], holderB.FarSpecCorrection[i], t);
				this.MixScale[i] = Mathf.Lerp(holderA.MixScale[i], holderB.MixScale[i], t);
				this.MixBlend[i] = Mathf.Lerp(holderA.MixBlend[i], holderB.MixBlend[i], t);
				this.MixSaturation[i] = Mathf.Lerp(holderA.MixSaturation[i], holderB.MixSaturation[i], t);
				this.RTP_DiffFresnel[i] = Mathf.Lerp(holderA.RTP_DiffFresnel[i], holderB.RTP_DiffFresnel[i], t);
				this.RTP_metallic[i] = Mathf.Lerp(holderA.RTP_metallic[i], holderB.RTP_metallic[i], t);
				this.RTP_glossMin[i] = Mathf.Lerp(holderA.RTP_glossMin[i], holderB.RTP_glossMin[i], t);
				this.RTP_glossMax[i] = Mathf.Lerp(holderA.RTP_glossMax[i], holderB.RTP_glossMax[i], t);
				this.RTP_glitter[i] = Mathf.Lerp(holderA.RTP_glitter[i], holderB.RTP_glitter[i], t);
				this.MixBrightness[i] = Mathf.Lerp(holderA.MixBrightness[i], holderB.MixBrightness[i], t);
				this.MixReplace[i] = Mathf.Lerp(holderA.MixReplace[i], holderB.MixReplace[i], t);
				this.LayerBrightness[i] = Mathf.Lerp(holderA.LayerBrightness[i], holderB.LayerBrightness[i], t);
				this.LayerBrightness2Spec[i] = Mathf.Lerp(holderA.LayerBrightness2Spec[i], holderB.LayerBrightness2Spec[i], t);
				this.LayerAlbedo2SpecColor[i] = Mathf.Lerp(holderA.LayerAlbedo2SpecColor[i], holderB.LayerAlbedo2SpecColor[i], t);
				this.LayerSaturation[i] = Mathf.Lerp(holderA.LayerSaturation[i], holderB.LayerSaturation[i], t);
				this.LayerEmission[i] = Mathf.Lerp(holderA.LayerEmission[i], holderB.LayerEmission[i], t);
				this.LayerEmissionColor[i] = Color.Lerp(holderA.LayerEmissionColor[i], holderB.LayerEmissionColor[i], t);
				this.LayerEmissionRefractStrength[i] = Mathf.Lerp(holderA.LayerEmissionRefractStrength[i], holderB.LayerEmissionRefractStrength[i], t);
				this.LayerEmissionRefractHBedge[i] = Mathf.Lerp(holderA.LayerEmissionRefractHBedge[i], holderB.LayerEmissionRefractHBedge[i], t);
				this.GlobalColorPerLayer[i] = Mathf.Lerp(holderA.GlobalColorPerLayer[i], holderB.GlobalColorPerLayer[i], t);
				this.GlobalColorBottom[i] = Mathf.Lerp(holderA.GlobalColorBottom[i], holderB.GlobalColorBottom[i], t);
				this.GlobalColorTop[i] = Mathf.Lerp(holderA.GlobalColorTop[i], holderB.GlobalColorTop[i], t);
				this.GlobalColorColormapLoSat[i] = Mathf.Lerp(holderA.GlobalColorColormapLoSat[i], holderB.GlobalColorColormapLoSat[i], t);
				this.GlobalColorColormapHiSat[i] = Mathf.Lerp(holderA.GlobalColorColormapHiSat[i], holderB.GlobalColorColormapHiSat[i], t);
				this.GlobalColorLayerLoSat[i] = Mathf.Lerp(holderA.GlobalColorLayerLoSat[i], holderB.GlobalColorLayerLoSat[i], t);
				this.GlobalColorLayerHiSat[i] = Mathf.Lerp(holderA.GlobalColorLayerHiSat[i], holderB.GlobalColorLayerHiSat[i], t);
				this.GlobalColorLoBlend[i] = Mathf.Lerp(holderA.GlobalColorLoBlend[i], holderB.GlobalColorLoBlend[i], t);
				this.GlobalColorHiBlend[i] = Mathf.Lerp(holderA.GlobalColorHiBlend[i], holderB.GlobalColorHiBlend[i], t);
				this.PER_LAYER_HEIGHT_MODIFIER[i] = Mathf.Lerp(holderA.PER_LAYER_HEIGHT_MODIFIER[i], holderB.PER_LAYER_HEIGHT_MODIFIER[i], t);
				this._SuperDetailStrengthMultA[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultA[i], holderB._SuperDetailStrengthMultA[i], t);
				this._SuperDetailStrengthMultASelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskNear[i], holderB._SuperDetailStrengthMultASelfMaskNear[i], t);
				this._SuperDetailStrengthMultASelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskFar[i], holderB._SuperDetailStrengthMultASelfMaskFar[i], t);
				this._SuperDetailStrengthMultB[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultB[i], holderB._SuperDetailStrengthMultB[i], t);
				this._SuperDetailStrengthMultBSelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskNear[i], holderB._SuperDetailStrengthMultBSelfMaskNear[i], t);
				this._SuperDetailStrengthMultBSelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskFar[i], holderB._SuperDetailStrengthMultBSelfMaskFar[i], t);
				this._SuperDetailStrengthNormal[i] = Mathf.Lerp(holderA._SuperDetailStrengthNormal[i], holderB._SuperDetailStrengthNormal[i], t);
				this._BumpMapGlobalStrength[i] = Mathf.Lerp(holderA._BumpMapGlobalStrength[i], holderB._BumpMapGlobalStrength[i], t);
				this.AO_strength[i] = Mathf.Lerp(holderA.AO_strength[i], holderB.AO_strength[i], t);
				this.VerticalTextureStrength[i] = Mathf.Lerp(holderA.VerticalTextureStrength[i], holderB.VerticalTextureStrength[i], t);
				this._snow_strength_per_layer[i] = Mathf.Lerp(holderA._snow_strength_per_layer[i], holderB._snow_strength_per_layer[i], t);
				this.TERRAIN_LayerWetStrength[i] = Mathf.Lerp(holderA.TERRAIN_LayerWetStrength[i], holderB.TERRAIN_LayerWetStrength[i], t);
				this.TERRAIN_WaterLevel[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevel[i], holderB.TERRAIN_WaterLevel[i], t);
				this.TERRAIN_WaterLevelSlopeDamp[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevelSlopeDamp[i], holderB.TERRAIN_WaterLevelSlopeDamp[i], t);
				this.TERRAIN_WaterEdge[i] = Mathf.Lerp(holderA.TERRAIN_WaterEdge[i], holderB.TERRAIN_WaterEdge[i], t);
				this.TERRAIN_WaterGloss[i] = Mathf.Lerp(holderA.TERRAIN_WaterGloss[i], holderB.TERRAIN_WaterGloss[i], t);
				this.TERRAIN_WaterGlossDamper[i] = Mathf.Lerp(holderA.TERRAIN_WaterGlossDamper[i], holderB.TERRAIN_WaterGlossDamper[i], t);
				this.TERRAIN_Refraction[i] = Mathf.Lerp(holderA.TERRAIN_Refraction[i], holderB.TERRAIN_Refraction[i], t);
				this.TERRAIN_WetRefraction[i] = Mathf.Lerp(holderA.TERRAIN_WetRefraction[i], holderB.TERRAIN_WetRefraction[i], t);
				this.TERRAIN_Flow[i] = Mathf.Lerp(holderA.TERRAIN_Flow[i], holderB.TERRAIN_Flow[i], t);
				this.TERRAIN_WetFlow[i] = Mathf.Lerp(holderA.TERRAIN_WetFlow[i], holderB.TERRAIN_WetFlow[i], t);
				this.TERRAIN_WaterMetallic[i] = Mathf.Lerp(holderA.TERRAIN_WaterMetallic[i], holderB.TERRAIN_WaterMetallic[i], t);
				this.TERRAIN_WetGloss[i] = Mathf.Lerp(holderA.TERRAIN_WetGloss[i], holderB.TERRAIN_WetGloss[i], t);
				this.TERRAIN_WaterColor[i] = Color.Lerp(holderA.TERRAIN_WaterColor[i], holderB.TERRAIN_WaterColor[i], t);
				this.TERRAIN_WaterEmission[i] = Mathf.Lerp(holderA.TERRAIN_WaterEmission[i], holderB.TERRAIN_WaterEmission[i], t);
				this._GlitterStrength[i] = Mathf.Lerp(holderA._GlitterStrength[i], holderB._GlitterStrength[i], t);
			}
		}
	}

	public void ReturnToDefaults(string what = "", int layerIdx = -1)
	{
		if (what == "" || what == "main")
		{
			this.ReliefTransform = new Vector4(3f, 3f, 0f, 0f);
			this.distance_start = 5f;
			this.distance_transition = 20f;
			this.RTP_LightDefVector = new Vector4(0.05f, 0.5f, 0.5f, 25f);
			this.ReliefBorderBlend = 6f;
			this.LightmapShading = 0f;
			this.RTP_MIP_BIAS = 0f;
			this.RTP_AOsharpness = 1.5f;
			this.RTP_AOamp = 0.1f;
			this._occlusionStrength = 1f;
			this.MasterLayerBrightness = 1f;
			this.MasterLayerSaturation = 1f;
			this.EmissionRefractFiltering = 4f;
			this.EmissionRefractAnimSpeed = 4f;
		}
		if (what == "" || what == "perlin")
		{
			this.BumpMapGlobalScale = 0.1f;
			this._FarNormalDamp = 0.2f;
			this.distance_start_bumpglobal = 30f;
			this.distance_transition_bumpglobal = 30f;
			this.rtp_perlin_start_val = 0f;
		}
		if (what == "" || what == "global_color")
		{
			this.GlobalColorMapBlendValues = new Vector3(0.2f, 0.4f, 0.5f);
			this.GlobalColorMapSaturation = 1f;
			this.GlobalColorMapSaturationFar = 1f;
			this.GlobalColorMapDistortByPerlin = 0.005f;
			this.GlobalColorMapBrightness = 1f;
			this.GlobalColorMapBrightnessFar = 1f;
			this._GlobalColorMapNearMIP = 0f;
			this.trees_shadow_distance_start = 50f;
			this.trees_shadow_distance_transition = 10f;
			this.trees_shadow_value = 0.5f;
			this.trees_pixel_distance_start = 500f;
			this.trees_pixel_distance_transition = 10f;
			this.trees_pixel_blend_val = 2f;
			this.global_normalMap_multiplier = 1f;
			this.global_normalMap_farUsage = 0f;
			this._Phong = 0f;
			this.tessHeight = 300f;
			this._TessSubdivisions = 1f;
			this._TessSubdivisionsFar = 1f;
			this._TessYOffset = 0f;
			this._AmbientEmissiveMultiplier = 1f;
			this._AmbientEmissiveRelief = 0.5f;
		}
		if (what == "" || what == "uvblend")
		{
			this.blendMultiplier = 1f;
		}
		if (what == "" || what == "pom/pm")
		{
			this.ExtrudeHeight = 0.05f;
			this.DIST_STEPS = 20f;
			this.WAVELENGTH = 2f;
			this.SHADOW_STEPS = 20f;
			this.WAVELENGTH_SHADOWS = 2f;
			this.SelfShadowStrength = 0.8f;
			this.ShadowSmoothing = 1f;
			this.ShadowSoftnessFade = 0.8f;
			this.CJ_flattenShadows = 0f;
		}
		if (what == "" || what == "snow")
		{
			this._global_color_brightness_to_snow = 0.5f;
			this._snow_strength = 0f;
			this._snow_slope_factor = 2f;
			this._snow_edge_definition = 2f;
			this._snow_height_treshold = -200f;
			this._snow_height_transition = 1f;
			this._snow_color = Color.white;
			this._snow_gloss = 0.7f;
			this._snow_reflectivness = 0.7f;
			this._snow_deep_factor = 1.5f;
			this._snow_diff_fresnel = 0.5f;
			this._SnowGlitterColor = new Color(1f, 1f, 1f, 0.1f);
		}
		if (what == "" || what == "superdetail")
		{
			this._SuperDetailTiling = 8f;
		}
		if (what == "" || what == "vertical")
		{
			this.VerticalTextureGlobalBumpInfluence = 0f;
			this.VerticalTextureTiling = 50f;
		}
		if (what == "" || what == "glitter")
		{
			this._GlitterColor = new Color(1f, 1f, 1f, 0.1f);
			this._GlitterTiling = 1f;
			this._GlitterDensity = 0.1f;
			this._GlitterFilter = 0f;
			this._GlitterColorization = 0.5f;
		}
		if (what == "" || what == "water")
		{
			this.TERRAIN_GlobalWetness = 1f;
			this.TERRAIN_RippleScale = 4f;
			this.TERRAIN_FlowScale = 1f;
			this.TERRAIN_FlowSpeed = 0.5f;
			this.TERRAIN_FlowCycleScale = 1f;
			this.TERRAIN_RainIntensity = 1f;
			this.TERRAIN_DropletsSpeed = 10f;
			this.TERRAIN_mipoffset_flowSpeed = 1f;
			this.TERRAIN_FlowMipOffset = 0f;
			this.TERRAIN_WetDarkening = 0.5f;
			this.TERRAIN_WetDropletsStrength = 0f;
			this.TERRAIN_WetHeight_Treshold = -200f;
			this.TERRAIN_WetHeight_Transition = 5f;
		}
		if (what == "" || what == "caustics")
		{
			this.TERRAIN_CausticsAnimSpeed = 2f;
			this.TERRAIN_CausticsColor = Color.white;
			this.TERRAIN_CausticsWaterLevel = 30f;
			this.TERRAIN_CausticsWaterLevelByAngle = 2f;
			this.TERRAIN_CausticsWaterDeepFadeLength = 50f;
			this.TERRAIN_CausticsWaterShallowFadeLength = 30f;
			this.TERRAIN_CausticsTilingScale = 1f;
		}
		if (what == "" || what == "layer")
		{
			int num = 0;
			int num2 = (this.numLayers < 12) ? this.numLayers : 12;
			if (layerIdx >= 0)
			{
				num = layerIdx;
				num2 = layerIdx + 1;
			}
			for (int i = num; i < num2; i++)
			{
				this.FarSpecCorrection[i] = 0f;
				this.MIPmult[i] = 0f;
				this.MixScale[i] = 0.2f;
				this.MixBlend[i] = 0.5f;
				this.MixSaturation[i] = 0.3f;
				this.RTP_DiffFresnel[i] = 0f;
				this.RTP_metallic[i] = 0f;
				this.RTP_glossMin[i] = 0f;
				this.RTP_glossMax[i] = 1f;
				this.RTP_glitter[i] = 0f;
				this.RTP_heightMin[i] = 0f;
				this.RTP_heightMax[i] = 1f;
				this.MixBrightness[i] = 2f;
				this.MixReplace[i] = 0f;
				this.LayerBrightness[i] = 1f;
				this.LayerBrightness2Spec[i] = 0f;
				this.LayerAlbedo2SpecColor[i] = 0f;
				this.LayerSaturation[i] = 1f;
				this.LayerEmission[i] = 0f;
				this.LayerEmissionColor[i] = Color.black;
				this.LayerEmissionRefractStrength[i] = 0f;
				this.LayerEmissionRefractHBedge[i] = 0f;
				this.GlobalColorPerLayer[i] = 1f;
				this.GlobalColorBottom[i] = 0f;
				this.GlobalColorTop[i] = 1f;
				this.GlobalColorColormapLoSat[i] = 1f;
				this.GlobalColorColormapHiSat[i] = 1f;
				this.GlobalColorLayerLoSat[i] = 1f;
				this.GlobalColorLayerHiSat[i] = 1f;
				this.GlobalColorLoBlend[i] = 1f;
				this.GlobalColorHiBlend[i] = 1f;
				this.PER_LAYER_HEIGHT_MODIFIER[i] = 0f;
				this._SuperDetailStrengthMultA[i] = 0f;
				this._SuperDetailStrengthMultASelfMaskNear[i] = 0f;
				this._SuperDetailStrengthMultASelfMaskFar[i] = 0f;
				this._SuperDetailStrengthMultB[i] = 0f;
				this._SuperDetailStrengthMultBSelfMaskNear[i] = 0f;
				this._SuperDetailStrengthMultBSelfMaskFar[i] = 0f;
				this._SuperDetailStrengthNormal[i] = 0f;
				this._BumpMapGlobalStrength[i] = 0.3f;
				this._snow_strength_per_layer[i] = 1f;
				this.VerticalTextureStrength[i] = 0.5f;
				this.AO_strength[i] = 1f;
				this.TERRAIN_LayerWetStrength[i] = 1f;
				this.TERRAIN_WaterLevel[i] = 0.5f;
				this.TERRAIN_WaterLevelSlopeDamp[i] = 0.5f;
				this.TERRAIN_WaterEdge[i] = 2f;
				this.TERRAIN_WaterGloss[i] = 0.1f;
				this.TERRAIN_WaterGlossDamper[i] = 0f;
				this.TERRAIN_Refraction[i] = 0.01f;
				this.TERRAIN_WetRefraction[i] = 0.2f;
				this.TERRAIN_Flow[i] = 0.3f;
				this.TERRAIN_WetFlow[i] = 0.05f;
				this.TERRAIN_WaterMetallic[i] = 0.1f;
				this.TERRAIN_WetGloss[i] = 0.05f;
				this.TERRAIN_WaterColor[i] = new Color(0.9f, 0.9f, 1f, 0.5f);
				this.TERRAIN_WaterEmission[i] = 0f;
				this._GlitterStrength[i] = 0f;
			}
		}
	}

	public bool CheckAndUpdate(ref float[] aLayerPropArray, float defVal, int len)
	{
		if (aLayerPropArray == null || aLayerPropArray.Length < len)
		{
			aLayerPropArray = new float[len];
			for (int i = 0; i < len; i++)
			{
				aLayerPropArray[i] = defVal;
			}
			return true;
		}
		return false;
	}

	public bool CheckAndUpdate(ref Color[] aLayerPropArray, Color defVal, int len)
	{
		if (aLayerPropArray == null || aLayerPropArray.Length < len)
		{
			aLayerPropArray = new Color[len];
			for (int i = 0; i < len; i++)
			{
				aLayerPropArray[i] = defVal;
			}
			return true;
		}
		return false;
	}

	public int numTiles;

	public int numLayers;

	public TerrainLayer[] terrainLayers;

	[NonSerialized]
	public bool dont_check_weak_references;

	[NonSerialized]
	public bool dont_check_for_interfering_terrain_replacement_shaders;

	[NonSerialized]
	public Texture2D[] bakeJobArray;

	public Texture2D tmp_globalCausticsMap;

	public Texture2D tmp_globalPuddleMap;

	public Texture2D tmp_globalWetMap;

	public Texture2D[] splats;

	public Texture2D[] splat_atlases = new Texture2D[3];

	public string save_path_atlasA = "";

	public string save_path_atlasB = "";

	public string save_path_atlasC = "";

	public string save_path_terrain_steepness = "";

	public string save_path_terrain_height = "";

	public string save_path_terrain_direction = "";

	public string save_path_Bump01 = "";

	public string save_path_Bump23 = "";

	public string save_path_Bump45 = "";

	public string save_path_Bump67 = "";

	public string save_path_Bump89 = "";

	public string save_path_BumpAB = "";

	public string save_path_HeightMap = "";

	public string save_path_HeightMap2 = "";

	public string save_path_HeightMap3 = "";

	public string save_path_SSColorCombinedA = "";

	public string save_path_SSColorCombinedB = "";

	public string save_path_CausticsMask = "";

	public string save_path_PuddleMask = "";

	public string save_path_WetMask = "";

	public string newPresetName = "a preset name...";

	public Texture2D activateObject;

	private GameObject _RTP_LODmanager;

	public RTP_LODmanager _RTP_LODmanagerScript;

	public float RTP_MIP_BIAS;

	public float MasterLayerBrightness = 1f;

	public float MasterLayerSaturation = 1f;

	public float EmissionRefractFiltering = 4f;

	public float EmissionRefractAnimSpeed = 4f;

	public RTPColorChannels SuperDetailA_channel;

	public RTPColorChannels SuperDetailB_channel;

	public Texture2D Bump01;

	public Texture2D Bump23;

	public Texture2D Bump45;

	public Texture2D Bump67;

	public Texture2D Bump89;

	public Texture2D BumpAB;

	public Texture2D BumpGlobal;

	public int BumpGlobalCombinedSize = 1024;

	public Texture2D SSColorCombinedA;

	public Texture2D SSColorCombinedB;

	public Texture2D VerticalTexture;

	public float BumpMapGlobalScale;

	public Vector3 GlobalColorMapBlendValues;

	public float _GlobalColorMapNearMIP;

	public float GlobalColorMapSaturation;

	public float GlobalColorMapSaturationFar = 1f;

	public float GlobalColorMapDistortByPerlin = 0.005f;

	public float GlobalColorMapBrightness;

	public float GlobalColorMapBrightnessFar = 1f;

	public float _FarNormalDamp;

	public float blendMultiplier;

	public Vector3 terrainTileSize;

	public Texture2D HeightMap;

	public Vector4 ReliefTransform;

	public float DIST_STEPS;

	public float WAVELENGTH;

	public float ReliefBorderBlend;

	public float ExtrudeHeight;

	public float LightmapShading;

	public float RTP_AOsharpness;

	public float RTP_AOamp;

	public float _occlusionStrength = 1f;

	public float SHADOW_STEPS;

	public float WAVELENGTH_SHADOWS;

	public float SelfShadowStrength;

	public float ShadowSmoothing;

	public float ShadowSoftnessFade = 0.8f;

	public float CJ_flattenShadows;

	public float distance_start;

	public float distance_transition;

	public float distance_start_bumpglobal;

	public float distance_transition_bumpglobal;

	public float rtp_perlin_start_val;

	public float _Phong;

	public float tessHeight = 300f;

	public float _TessSubdivisions = 1f;

	public float _TessSubdivisionsFar = 1f;

	public float _TessYOffset;

	public float trees_shadow_distance_start;

	public float trees_shadow_distance_transition;

	public float trees_shadow_value;

	public float trees_pixel_distance_start;

	public float trees_pixel_distance_transition;

	public float trees_pixel_blend_val;

	public float global_normalMap_multiplier;

	public float global_normalMap_farUsage;

	public float _AmbientEmissiveMultiplier = 1f;

	public float _AmbientEmissiveRelief = 0.5f;

	public Texture2D HeightMap2;

	public Texture2D HeightMap3;

	public int rtp_mipoffset_globalnorm;

	public float _SuperDetailTiling;

	public Texture2D SuperDetailA;

	public Texture2D SuperDetailB;

	public float TERRAIN_GlobalWetness;

	public AnimationCurve TERRAIN_WetnessAttackCurve;

	public float TERRAIN_WetnessReleaseFastTime;

	public float TERRAIN_WetnessReleaseFastValue;

	public float TERRAIN_WetnessReleaseSlowTime;

	public Texture2D TERRAIN_RippleMap;

	public float TERRAIN_RippleScale;

	public float TERRAIN_FlowScale;

	public float TERRAIN_FlowSpeed;

	public float TERRAIN_FlowCycleScale;

	public float TERRAIN_FlowMipOffset;

	public float TERRAIN_WetDarkening;

	public float TERRAIN_WetDropletsStrength;

	public float TERRAIN_WetHeight_Treshold;

	public float TERRAIN_WetHeight_Transition;

	public float TERRAIN_RainIntensity;

	public float TERRAIN_DropletsSpeed;

	public float TERRAIN_mipoffset_flowSpeed;

	public float TERRAIN_CausticsAnimSpeed;

	public Color TERRAIN_CausticsColor;

	public GameObject TERRAIN_CausticsWaterLevelRefObject;

	public float TERRAIN_CausticsWaterLevel;

	public float TERRAIN_CausticsWaterLevelByAngle;

	public float TERRAIN_CausticsWaterDeepFadeLength;

	public float TERRAIN_CausticsWaterShallowFadeLength;

	public float TERRAIN_CausticsTilingScale;

	public Texture2D TERRAIN_CausticsTex;

	public Texture2D TERRAIN_CausticsMask;

	public int causticsMaskSize = 2048;

	public bool globalCausticsModifed_flag;

	public Vector4 TERRAIN_CausticsMaskWorld2UV = Vector4.zero;

	public Texture2D TERRAIN_PuddleMask;

	public float TERRAIN_PuddleLevel = 1f;

	public int puddleMaskSize = 2048;

	public bool globalPuddleModified_flag;

	public Texture2D TERRAIN_WetMask;

	public int wetMaskSize = 2048;

	public bool globalWetModified_flag;

	public Vector4 RTP_LightDefVector;

	public Texture2D[] Bumps;

	public float[] FarSpecCorrection;

	public float[] MIPmult;

	public float[] MixScale;

	public float[] MixBlend;

	public float[] MixSaturation;

	public float[] RTP_DiffFresnel;

	public float[] RTP_metallic;

	public float[] RTP_glossMin;

	public float[] RTP_glossMax;

	public float[] RTP_glitter;

	public float[] RTP_heightMin;

	public float[] RTP_heightMax;

	public float[] GlobalColorBottom;

	public float[] GlobalColorTop;

	public float[] GlobalColorColormapLoSat;

	public float[] GlobalColorColormapHiSat;

	public float[] GlobalColorLayerLoSat;

	public float[] GlobalColorLayerHiSat;

	public float[] GlobalColorLoBlend;

	public float[] GlobalColorHiBlend;

	public float[] MixBrightness;

	public float[] MixReplace;

	public float[] LayerBrightness;

	public float[] LayerBrightness2Spec;

	public float[] LayerAlbedo2SpecColor;

	public float[] LayerSaturation;

	public float[] LayerEmission;

	public Color[] LayerEmissionColor;

	public float[] LayerEmissionRefractStrength;

	public float[] LayerEmissionRefractHBedge;

	public float[] GlobalColorPerLayer;

	public float[] PER_LAYER_HEIGHT_MODIFIER;

	public float[] _SuperDetailStrengthMultA;

	public float[] _SuperDetailStrengthMultASelfMaskNear;

	public float[] _SuperDetailStrengthMultASelfMaskFar;

	public float[] _SuperDetailStrengthMultB;

	public float[] _SuperDetailStrengthMultBSelfMaskNear;

	public float[] _SuperDetailStrengthMultBSelfMaskFar;

	public float[] _SuperDetailStrengthNormal;

	public float[] _BumpMapGlobalStrength;

	public float[] AO_strength = new float[]
	{
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f,
		1f
	};

	public float[] VerticalTextureStrength;

	public float VerticalTextureGlobalBumpInfluence;

	public float VerticalTextureTiling;

	public Texture2D[] Heights;

	public float[] _snow_strength_per_layer;

	public float[] TERRAIN_LayerWetStrength;

	public float[] TERRAIN_WaterLevel;

	public float[] TERRAIN_WaterLevelSlopeDamp;

	public float[] TERRAIN_WaterEdge;

	public float[] TERRAIN_WaterGloss;

	public float[] TERRAIN_WaterGlossDamper;

	public float[] TERRAIN_Refraction;

	public float[] TERRAIN_WetRefraction;

	public float[] TERRAIN_Flow;

	public float[] TERRAIN_WetFlow;

	public float[] TERRAIN_WaterMetallic;

	public float[] TERRAIN_WetGloss;

	public Color[] TERRAIN_WaterColor;

	public float[] TERRAIN_WaterEmission;

	public float _snow_strength;

	public float _global_color_brightness_to_snow;

	public float _snow_slope_factor;

	public float _snow_edge_definition;

	public float _snow_height_treshold;

	public float _snow_height_transition;

	public Color _snow_color;

	public float _snow_gloss;

	public float _snow_reflectivness;

	public float _snow_deep_factor;

	public float _snow_diff_fresnel;

	public float _snow_metallic;

	public float _snow_Frost;

	public float _snow_MicroTiling = 1f;

	public float _snow_BumpMicro = 0.2f;

	public Color _SnowGlitterColor = new Color(1f, 1f, 1f, 0.1f);

	public float _snow_occlusionStrength = 0.5f;

	public int _snow_TranslucencyDeferredLightIndex;

	public Color _GlitterColor;

	public float[] _GlitterStrength;

	public float _GlitterTiling;

	public float _GlitterDensity;

	public float _GlitterFilter;

	public float _GlitterColorization;

	public Texture2D _SparkleMap;

	public bool _4LAYERS_SHADER_USED;

	public bool flat_dir_ref = true;

	public bool flip_dir_ref = true;

	public GameObject direction_object;

	public bool show_details;

	public bool show_details_main;

	public bool show_details_atlasing;

	public bool show_details_layers;

	public bool show_details_uv_blend;

	public bool show_controlmaps;

	public bool show_controlmaps_build;

	public bool show_controlmaps_helpers;

	public bool show_controlmaps_highcost;

	public bool show_controlmaps_splats;

	public bool show_vert_texture;

	public bool show_global_color;

	public bool show_snow;

	public bool show_global_bump;

	public bool show_global_bump_normals;

	public bool show_global_bump_superdetail;

	public ReliefTerrainMenuItems submenu;

	public ReliefTerrainSettingsItems submenu_settings;

	public ReliefTerrainDerivedTexturesItems submenu_derived_textures;

	public ReliefTerrainControlTexturesItems submenu_control_textures;

	public bool show_global_wet_settings;

	public bool show_global_reflection_settings;

	public int show_active_layer;

	public bool show_derivedmaps;

	public bool show_settings;

	public bool undo_flag;

	public bool paint_flag;

	public float paint_size = 0.5f;

	public float paint_smoothness;

	public float paint_opacity = 1f;

	public Color paintColor = new Color(0.5f, 0.3f, 0f, 0f);

	public bool preserveBrightness = true;

	public bool paint_alpha_flag;

	public bool paint_wetmask;

	public bool paint_causticsmask;

	public bool paint_puddlemask;

	public float paintTargetLevel = 1f;

	public bool paintTargetLevelFlag;

	public RaycastHit paintHitInfo;

	public bool paintHitInfo_flag;

	public bool cut_holes;

	private Texture2D dumb_tex;

	public Color[] paintColorSwatches;

	public Material use_mat;
}
