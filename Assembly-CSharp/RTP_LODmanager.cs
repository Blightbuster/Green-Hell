using System;
using UnityEngine;

public class RTP_LODmanager : MonoBehaviour
{
	private void Awake()
	{
		this.RefreshLODlevel();
	}

	public void RefreshLODlevel()
	{
		ReliefTerrain[] array = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
		ReliefTerrain reliefTerrain = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent(typeof(Terrain)))
			{
				reliefTerrain = array[i];
				break;
			}
		}
		if (reliefTerrain != null && reliefTerrain.globalSettingsHolder != null)
		{
			if (this.terrain_shader == null)
			{
				this.terrain_shader = Shader.Find("Relief Pack/ReliefTerrain-FirstPass");
			}
			if (this.terrain_shader_add == null)
			{
				this.terrain_shader_add = Shader.Find("Hidden/Relief Pack/ReliefTerrain-AddPass");
			}
		}
		else
		{
			if (this.terrain_shader == null)
			{
				this.terrain_shader = Shader.Find("Hidden/TerrainEngine/Splatmap/Lightmap-FirstPass");
			}
			if (this.terrain_shader_add == null)
			{
				this.terrain_shader_add = Shader.Find("Hidden/TerrainEngine/Splatmap/Lightmap-AddPass");
			}
			if (this.terrain_shader == null)
			{
				this.terrain_shader = Shader.Find("Nature/Terrain/Diffuse");
			}
			if (this.terrain_shader_add == null)
			{
				this.terrain_shader_add = Shader.Find("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass");
			}
		}
		if (this.terrain_shader_far == null)
		{
			this.terrain_shader_far = Shader.Find("Hidden/Relief Pack/ReliefTerrain-FarOnly");
		}
		if (this.terrain2geom_shader == null)
		{
			this.terrain2geom_shader = Shader.Find("Relief Pack/Terrain2Geometry");
		}
		if (this.terrain_geomBlend_shader == null)
		{
			this.terrain_geomBlend_shader = Shader.Find("Hidden/Relief Pack/ReliefTerrainGeometryBlendBase");
		}
		if (this.terrain2geom_geomBlend_shader == null)
		{
			this.terrain2geom_geomBlend_shader = Shader.Find("Hidden/Relief Pack/ReliefTerrain2GeometryBlendBase");
		}
		if (this.terrain_geomBlend_GeometryBlend_BumpedDetailSnow == null)
		{
			this.terrain_geomBlend_GeometryBlend_BumpedDetailSnow = Shader.Find("Relief Pack - GeometryBlend/Bumped Detail Snow");
		}
		if (this.geomblend_GeometryBlend_WaterShader_2VertexPaint_HB == null)
		{
			this.geomblend_GeometryBlend_WaterShader_2VertexPaint_HB = Shader.Find("Relief Pack - GeometryBlend/Water/2 Layers/ HeightBlend");
		}
		if (this.geomBlend_GeometryBlend_WaterShader_FlowMap_HB == null)
		{
			this.geomBlend_GeometryBlend_WaterShader_FlowMap_HB = Shader.Find("Relief Pack - GeometryBlend/Water/ FlowMap - HeightBlend");
		}
		int maximumLOD = 700;
		if (this.RTP_LODlevel == TerrainShaderLod.POM)
		{
			if (this.RTP_SHADOWS)
			{
				if (this.RTP_SOFT_SHADOWS)
				{
					Shader.EnableKeyword("RTP_POM_SHADING_HI");
					Shader.DisableKeyword("RTP_POM_SHADING_MED");
					Shader.DisableKeyword("RTP_POM_SHADING_LO");
				}
				else
				{
					Shader.EnableKeyword("RTP_POM_SHADING_MED");
					Shader.DisableKeyword("RTP_POM_SHADING_HI");
					Shader.DisableKeyword("RTP_POM_SHADING_LO");
				}
			}
			else
			{
				Shader.EnableKeyword("RTP_POM_SHADING_LO");
				Shader.DisableKeyword("RTP_POM_SHADING_MED");
				Shader.DisableKeyword("RTP_POM_SHADING_HI");
			}
			Shader.DisableKeyword("RTP_PM_SHADING");
			Shader.DisableKeyword("RTP_SIMPLE_SHADING");
		}
		else if (this.RTP_LODlevel == TerrainShaderLod.PM)
		{
			Shader.DisableKeyword("RTP_POM_SHADING_HI");
			Shader.DisableKeyword("RTP_POM_SHADING_MED");
			Shader.DisableKeyword("RTP_POM_SHADING_LO");
			Shader.EnableKeyword("RTP_PM_SHADING");
			Shader.DisableKeyword("RTP_SIMPLE_SHADING");
		}
		else
		{
			Shader.DisableKeyword("RTP_POM_SHADING_HI");
			Shader.DisableKeyword("RTP_POM_SHADING_MED");
			Shader.DisableKeyword("RTP_POM_SHADING_LO");
			Shader.DisableKeyword("RTP_PM_SHADING");
			Shader.EnableKeyword("RTP_SIMPLE_SHADING");
		}
		if (this.terrain_shader != null)
		{
			this.terrain_shader.maximumLOD = maximumLOD;
		}
		if (this.terrain_shader_far != null)
		{
			this.terrain_shader_far.maximumLOD = maximumLOD;
		}
		if (this.terrain_shader_add != null)
		{
			this.terrain_shader_add.maximumLOD = maximumLOD;
		}
		if (this.terrain2geom_shader != null)
		{
			this.terrain2geom_shader.maximumLOD = maximumLOD;
		}
		if (this.terrain_geomBlend_shader != null)
		{
			this.terrain_geomBlend_shader.maximumLOD = maximumLOD;
		}
		if (this.terrain2geom_geomBlend_shader != null)
		{
			this.terrain2geom_geomBlend_shader.maximumLOD = maximumLOD;
		}
		if (this.terrain_geomBlend_GeometryBlend_BumpedDetailSnow != null)
		{
			this.terrain_geomBlend_GeometryBlend_BumpedDetailSnow.maximumLOD = maximumLOD;
		}
		if (this.geomblend_GeometryBlend_WaterShader_2VertexPaint_HB != null)
		{
			this.geomblend_GeometryBlend_WaterShader_2VertexPaint_HB.maximumLOD = maximumLOD;
		}
		if (this.geomBlend_GeometryBlend_WaterShader_FlowMap_HB != null)
		{
			this.geomBlend_GeometryBlend_WaterShader_FlowMap_HB.maximumLOD = maximumLOD;
		}
	}

	public TerrainShaderLod RTP_LODlevel;

	public bool RTP_SHADOWS = true;

	public bool RTP_SOFT_SHADOWS = true;

	public bool show_first_features;

	public bool show_add_features;

	public bool RTP_NOFORWARDADD;

	public bool RTP_NO_DEFERRED;

	public bool RTP_FULLFORWARDSHADOWS;

	public bool RTP_NOLIGHTMAP;

	public bool RTP_NODIRLIGHTMAP;

	public bool RTP_NODYNLIGHTMAP;

	public bool NO_SPECULARITY;

	public bool RTP_ADDSHADOW;

	public bool RTP_CUT_HOLES;

	public bool FIX_REFRESHING_ISSUE = true;

	public bool RTP_USE_COLOR_ATLAS_FIRST;

	public bool RTP_USE_COLOR_ATLAS_ADD;

	public RTPFogType RTP_FOGTYPE;

	public bool ADV_COLOR_MAP_BLENDING_FIRST;

	public bool ADV_COLOR_MAP_BLENDING_ADD;

	public bool RTP_UV_BLEND_FIRST = true;

	public bool RTP_UV_BLEND_ADD = true;

	public bool RTP_DISTANCE_ONLY_UV_BLEND_FIRST = true;

	public bool RTP_DISTANCE_ONLY_UV_BLEND_ADD = true;

	public bool RTP_NORMALS_FOR_REPLACE_UV_BLEND_FIRST = true;

	public bool RTP_NORMALS_FOR_REPLACE_UV_BLEND_ADD = true;

	public bool RTP_SUPER_DETAIL_FIRST = true;

	public bool RTP_SUPER_DETAIL_ADD = true;

	public bool RTP_SUPER_DETAIL_MULTS_FIRST;

	public bool RTP_SUPER_DETAIL_MULTS_ADD;

	public bool RTP_SNOW_FIRST;

	public bool RTP_SNOW_ADD;

	public bool RTP_SNW_CHOOSEN_LAYER_COLOR_FIRST;

	public bool RTP_SNW_CHOOSEN_LAYER_COLOR_ADD;

	public int RTP_SNW_CHOOSEN_LAYER_COLOR_NUM_FIRST = 7;

	public int RTP_SNW_CHOOSEN_LAYER_COLOR_NUM_ADD = 7;

	public bool RTP_SNW_CHOOSEN_LAYER_NORMAL_FIRST;

	public bool RTP_SNW_CHOOSEN_LAYER_NORMAL_ADD;

	public int RTP_SNW_CHOOSEN_LAYER_NORMAL_NUM_FIRST = 7;

	public int RTP_SNW_CHOOSEN_LAYER_NORMAL_NUM_ADD = 7;

	public RTPLodLevel MAX_LOD_FIRST = RTPLodLevel.PM;

	public RTPLodLevel MAX_LOD_FIRST_PLUS4 = RTPLodLevel.SIMPLE;

	public RTPLodLevel MAX_LOD_ADD = RTPLodLevel.PM;

	public bool RTP_SHARPEN_HEIGHTBLEND_EDGES_PASS1_FIRST = true;

	public bool RTP_SHARPEN_HEIGHTBLEND_EDGES_PASS2_FIRST;

	public bool RTP_SHARPEN_HEIGHTBLEND_EDGES_PASS1_ADD = true;

	public bool RTP_SHARPEN_HEIGHTBLEND_EDGES_PASS2_ADD;

	public bool RTP_HEIGHTBLEND_AO_FIRST;

	public bool RTP_HEIGHTBLEND_AO_ADD;

	public bool RTP_EMISSION_FIRST;

	public bool RTP_EMISSION_ADD;

	public bool RTP_FUILD_EMISSION_WRAP_FIRST;

	public bool RTP_FUILD_EMISSION_WRAP_ADD;

	public bool RTP_HOTAIR_EMISSION_FIRST;

	public bool RTP_HOTAIR_EMISSION_ADD;

	public bool RTP_SHOW_OVERLAPPED;

	public bool RTP_TRIPLANAR_FIRST;

	public bool RTP_TRIPLANAR_ADD;

	public bool RTP_NORMALGLOBAL;

	public bool RTP_TESSELLATION;

	public bool RTP_TESSELLATION_SAMPLE_TEXTURE;

	public bool RTP_HEIGHTMAP_SAMPLE_BICUBIC = true;

	public bool RTP_DETAIL_HEIGHTMAP_SAMPLE;

	public bool RTP_TREESGLOBAL;

	public bool RTP_USE_BUMPMAPS_FIRST = true;

	public bool RTP_USE_BUMPMAPS_ADD = true;

	public bool RTP_USE_PERLIN_FIRST;

	public bool RTP_USE_PERLIN_ADD;

	public bool RTP_COLOR_MAP_BLEND_MULTIPLY_FIRST = true;

	public bool RTP_COLOR_MAP_BLEND_MULTIPLY_ADD = true;

	public bool RTP_SIMPLE_FAR_FIRST = true;

	public bool RTP_SIMPLE_FAR_ADD = true;

	public bool RTP_CROSSPASS_HEIGHTBLEND;

	public int[] UV_BLEND_ROUTE_NUM_FIRST = new int[]
	{
		0,
		1,
		2,
		3,
		4,
		5,
		6,
		7
	};

	public int[] UV_BLEND_ROUTE_NUM_ADD = new int[]
	{
		0,
		1,
		2,
		3,
		4,
		5,
		6,
		7
	};

	public bool RTP_HARD_CROSSPASS = true;

	public bool RTP_MAPPED_SHADOWS_FIRST;

	public bool RTP_MAPPED_SHADOWS_ADD;

	public bool RTP_VERTICAL_TEXTURE_FIRST;

	public bool RTP_VERTICAL_TEXTURE_ADD;

	public bool RTP_ADDITIONAL_FEATURES_IN_FALLBACKS = true;

	public bool RTP_4LAYERS_MODE;

	public int numLayers;

	public int numLayersProcessedByFarShader = 8;

	public bool ADDPASS_IN_BLENDBASE;

	public bool RTP_GLITTER_FIRST;

	public bool RTP_GLITTER_ADD;

	public bool RTP_WETNESS_FIRST;

	public bool RTP_WETNESS_ADD;

	public bool RTP_WET_RIPPLE_TEXTURE_FIRST;

	public bool RTP_WET_RIPPLE_TEXTURE_ADD;

	public bool RTP_CAUSTICS_FIRST;

	public bool RTP_CAUSTICS_ADD;

	public bool RTP_VERTALPHA_CAUSTICS;

	public bool SIMPLE_WATER_FIRST;

	public bool SIMPLE_WATER_ADD;

	public bool RTP_USE_EXTRUDE_REDUCTION_FIRST;

	public bool RTP_USE_EXTRUDE_REDUCTION_ADD;

	private Shader terrain_shader;

	private Shader terrain_shader_far;

	private Shader terrain_shader_add;

	private Shader terrain2geom_shader;

	private Shader terrain_geomBlend_shader;

	private Shader terrain2geom_geomBlend_shader;

	private Shader terrain_geomBlend_GeometryBlend_BumpedDetailSnow;

	private Shader geomblend_GeometryBlend_WaterShader_2VertexPaint_HB;

	private Shader geomBlend_GeometryBlend_WaterShader_FlowMap_HB;

	private Shader terrain_geomBlendActual_shader;

	public bool dont_sync;
}
