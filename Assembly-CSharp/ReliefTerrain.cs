using System;
using System.Reflection;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Engine - Terrain or Mesh")]
[ExecuteInEditMode]
public class ReliefTerrain : MonoBehaviour
{
	public void GetGlobalSettingsHolder()
	{
		if (this.globalSettingsHolder == null)
		{
			ReliefTerrain[] array = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
			bool flag = base.GetComponent(typeof(Terrain));
			int i = 0;
			while (i < array.Length)
			{
				if (array[i].transform.parent == base.transform.parent && array[i].globalSettingsHolder != null && ((flag && array[i].GetComponent(typeof(Terrain)) != null) || (!flag && array[i].GetComponent(typeof(Terrain)) == null)))
				{
					this.globalSettingsHolder = array[i].globalSettingsHolder;
					if (this.globalSettingsHolder.Get_RTP_LODmanagerScript() && !this.globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_FIRST && !this.globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_ADD)
					{
						this.BumpGlobalCombined = array[i].BumpGlobalCombined;
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (this.globalSettingsHolder == null)
			{
				this.globalSettingsHolder = new ReliefTerrainGlobalSettingsHolder();
				if (flag)
				{
					this.globalSettingsHolder.numTiles = 0;
					Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
					this.globalSettingsHolder.terrainLayers = new TerrainLayer[terrain.terrainData.terrainLayers.Length];
					Array.Copy(terrain.terrainData.terrainLayers, this.globalSettingsHolder.terrainLayers, this.globalSettingsHolder.terrainLayers.Length);
					this.globalSettingsHolder.splats = new Texture2D[terrain.terrainData.terrainLayers.Length];
					this.globalSettingsHolder.Bumps = new Texture2D[terrain.terrainData.terrainLayers.Length];
					this.globalSettingsHolder.terrainLayers = terrain.terrainData.terrainLayers;
					for (int j = 0; j < terrain.terrainData.terrainLayers.Length; j++)
					{
						this.globalSettingsHolder.splats[j] = terrain.terrainData.terrainLayers[j].diffuseTexture;
						this.globalSettingsHolder.Bumps[j] = terrain.terrainData.terrainLayers[j].normalMapTexture;
					}
				}
				else
				{
					this.globalSettingsHolder.splats = new Texture2D[4];
				}
				this.globalSettingsHolder.numLayers = this.globalSettingsHolder.splats.Length;
				this.globalSettingsHolder.ReturnToDefaults("", -1);
			}
			else if (flag)
			{
				this.GetSplatsFromGlobalSettingsHolder();
			}
			this.source_controls_mask = new Texture2D[12];
			this.source_controls = new Texture2D[12];
			this.source_controls_channels = new RTPColorChannels[12];
			this.source_controls_mask_channels = new RTPColorChannels[12];
			this.splat_layer_seq = new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10,
				11
			};
			this.splat_layer_boost = new float[]
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
			this.splat_layer_calc = new bool[12];
			this.splat_layer_masked = new bool[12];
			this.source_controls_invert = new bool[12];
			this.source_controls_mask_invert = new bool[12];
			if (flag)
			{
				this.globalSettingsHolder.numTiles++;
			}
		}
	}

	private void GetSplatsFromGlobalSettingsHolder()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (this.globalSettingsHolder.terrainLayers != null && this.globalSettingsHolder.terrainLayers.Length == this.globalSettingsHolder.numLayers && this.globalSettingsHolder.terrainLayers.Length != 0 && this.globalSettingsHolder.terrainLayers[0] != null)
		{
			if (terrain.terrainData.terrainLayers.Length != 0 && terrain.terrainData.terrainLayers[0] == null)
			{
				TerrainLayer[] array = new TerrainLayer[this.globalSettingsHolder.numLayers];
				Array.Copy(this.globalSettingsHolder.terrainLayers, array, this.globalSettingsHolder.terrainLayers.Length);
				terrain.terrainData.terrainLayers = array;
				return;
			}
		}
		else
		{
			TerrainLayer[] array2 = new TerrainLayer[this.globalSettingsHolder.numLayers];
			ReliefTerrain[] array3 = (ReliefTerrain[])UnityEngine.Object.FindObjectsOfType(typeof(ReliefTerrain));
			bool flag = false;
			for (int i = 0; i < array3.Length; i++)
			{
				Terrain component = array3[i].GetComponent<Terrain>();
				if (component != null && (array3.Length == 1 || array3[i] != this) && component.terrainData.terrainLayers.Length == array2.Length)
				{
					this.globalSettingsHolder.terrainLayers = new TerrainLayer[component.terrainData.terrainLayers.Length];
					Array.Copy(component.terrainData.terrainLayers, this.globalSettingsHolder.terrainLayers, this.globalSettingsHolder.terrainLayers.Length);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("TerrainLayers from GlobalSettingsHolder can't be found. Create a set of layers and setup first terrain before adding RTP script to it!");
				for (int j = 0; j < this.globalSettingsHolder.numLayers; j++)
				{
					array2[j] = new TerrainLayer();
					array2[j].tileSize = Vector2.one;
					array2[j].tileOffset = new Vector2(1f / this.customTiling.x, 1f / this.customTiling.y);
					array2[j].diffuseTexture = this.globalSettingsHolder.splats[j];
					array2[j].normalMapTexture = this.globalSettingsHolder.Bumps[j];
				}
			}
			terrain.terrainData.terrainLayers = array2;
		}
	}

	public void InitTerrainTileSizes()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (terrain)
		{
			this.globalSettingsHolder.terrainTileSize = terrain.terrainData.size;
			return;
		}
		this.globalSettingsHolder.terrainTileSize = base.GetComponent<Renderer>().bounds.size;
		this.globalSettingsHolder.terrainTileSize.y = this.globalSettingsHolder.tessHeight;
	}

	private void Awake()
	{
		this.UpdateBasemapDistance(false);
		this.RefreshTextures(null, false);
	}

	public void InitArrays()
	{
		this.RefreshTextures(null, false);
	}

	private void UpdateBasemapDistance(bool apply_material_if_applicable)
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (terrain && this.globalSettingsHolder != null)
		{
			terrain.basemapDistance = this.globalSettingsHolder.distance_start + this.globalSettingsHolder.distance_transition;
			if (apply_material_if_applicable)
			{
				if (terrain.materialTemplate == null)
				{
					terrain.materialType = Terrain.MaterialType.Custom;
					Shader shader = Shader.Find("Relief Pack/ReliefTerrain-FirstPass");
					if (shader)
					{
						terrain.materialTemplate = new Material(shader)
						{
							name = base.gameObject.name + " material"
						};
					}
				}
				else
				{
					Material materialTemplate = terrain.materialTemplate;
					terrain.materialTemplate = null;
					terrain.materialTemplate = materialTemplate;
				}
			}
			if (this.globalSettingsHolder != null && this.globalSettingsHolder._RTP_LODmanagerScript != null && this.globalSettingsHolder._RTP_LODmanagerScript.numLayersProcessedByFarShader != this.globalSettingsHolder.numLayers)
			{
				terrain.basemapDistance = 500000f;
			}
			this.globalSettingsHolder.Refresh(terrain.materialTemplate, null);
		}
	}

	public void RefreshTextures(Material mat = null, bool check_weak_references = false)
	{
		this.GetGlobalSettingsHolder();
		this.InitTerrainTileSizes();
		if (this.globalSettingsHolder != null && this.BumpGlobalCombined != null)
		{
			this.globalSettingsHolder.BumpGlobalCombinedSize = this.BumpGlobalCombined.width;
		}
		this.UpdateBasemapDistance(true);
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		this.globalSettingsHolder.use_mat = mat;
		if (!terrain && !mat)
		{
			if (base.GetComponent<Renderer>().sharedMaterial == null || base.GetComponent<Renderer>().sharedMaterial.name != "RTPMaterial")
			{
				base.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Relief Pack/Terrain2Geometry"));
				base.GetComponent<Renderer>().sharedMaterial.name = "RTPMaterial";
			}
			this.globalSettingsHolder.use_mat = base.GetComponent<Renderer>().sharedMaterial;
		}
		if (terrain && terrain.materialTemplate != null)
		{
			this.globalSettingsHolder.use_mat = terrain.materialTemplate;
			terrain.materialTemplate.SetVector("RTP_CustomTiling", new Vector4(1f / this.customTiling.x, 1f / this.customTiling.y, 0f, 0f));
		}
		this.globalSettingsHolder.use_mat = null;
		this.RefreshControlMaps(mat);
		if (mat)
		{
			mat.SetVector("RTP_CustomTiling", new Vector4(1f / this.customTiling.x, 1f / this.customTiling.y, 0f, 0f));
		}
	}

	public void RefreshControlMaps(Material mat = null)
	{
		this.globalSettingsHolder.use_mat = mat;
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (!terrain && !mat)
		{
			this.globalSettingsHolder.use_mat = base.GetComponent<Renderer>().sharedMaterial;
		}
		if (terrain && !mat && terrain.materialTemplate != null)
		{
			this.globalSettingsHolder.use_mat = terrain.materialTemplate;
		}
		this.globalSettingsHolder.SetShaderParam("_Control1", this.controlA);
		if (this.globalSettingsHolder.numLayers > 4)
		{
			this.globalSettingsHolder.SetShaderParam("_Control3", this.controlB);
			this.globalSettingsHolder.SetShaderParam("_Control2", this.controlB);
		}
		if (this.globalSettingsHolder.numLayers > 8)
		{
			this.globalSettingsHolder.SetShaderParam("_Control3", this.controlC);
		}
		this.globalSettingsHolder.SetShaderParam("_ColorMapGlobal", this.ColorGlobal);
		this.globalSettingsHolder.SetShaderParam("_NormalMapGlobal", this.NormalGlobal);
		this.globalSettingsHolder.SetShaderParam("_TreesMapGlobal", this.TreesGlobal);
		this.globalSettingsHolder.SetShaderParam("_AmbientEmissiveMapGlobal", this.AmbientEmissiveMap);
		this.globalSettingsHolder.SetShaderParam("_BumpMapGlobal", this.BumpGlobalCombined);
		this.globalSettingsHolder.use_mat = null;
	}

	public void GetControlMaps()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (!terrain)
		{
			Debug.Log("Can't fint terrain component !!!");
			return;
		}
		PropertyInfo property = terrain.terrainData.GetType().GetProperty("alphamapTextures", BindingFlags.Instance | BindingFlags.Public);
		if (!(property != null))
		{
			Debug.LogError("Can't access alphamapTexture directly...");
			return;
		}
		Texture2D[] array = (Texture2D[])property.GetValue(terrain.terrainData, null);
		if (array.Length != 0)
		{
			this.controlA = array[0];
		}
		else
		{
			this.controlA = null;
		}
		if (array.Length > 1)
		{
			this.controlB = array[1];
		}
		else
		{
			this.controlB = null;
		}
		if (array.Length > 2)
		{
			this.controlC = array[2];
			return;
		}
		this.controlC = null;
	}

	public void SetCustomControlMaps()
	{
		Terrain terrain = (Terrain)base.GetComponent(typeof(Terrain));
		if (!terrain)
		{
			Debug.Log("Can't fint terrain component !!!");
			return;
		}
		if (this.controlA == null)
		{
			return;
		}
		if (terrain.terrainData.alphamapResolution != this.controlA.width)
		{
			Debug.LogError("Terrain controlmap resolution differs fromrequested control texture...");
			return;
		}
		if (!this.controlA)
		{
			return;
		}
		float[,,] alphamaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution);
		Color[] pixels = this.controlA.GetPixels();
		for (int i = 0; i < terrain.terrainData.alphamapLayers; i++)
		{
			int num = 0;
			if (i == 4)
			{
				if (!this.controlB)
				{
					return;
				}
				pixels = this.controlB.GetPixels();
			}
			else if (i == 8)
			{
				if (!this.controlC)
				{
					return;
				}
				pixels = this.controlC.GetPixels();
			}
			int index = i & 3;
			for (int j = 0; j < terrain.terrainData.alphamapResolution; j++)
			{
				for (int k = 0; k < terrain.terrainData.alphamapResolution; k++)
				{
					alphamaps[j, k, i] = pixels[num++][index];
				}
			}
		}
		terrain.terrainData.SetAlphamaps(0, 0, alphamaps);
	}

	public void RestorePreset(ReliefTerrainPresetHolder holder)
	{
		this.controlA = holder.controlA;
		this.controlB = holder.controlB;
		this.controlC = holder.controlC;
		this.SetCustomControlMaps();
		this.ColorGlobal = holder.ColorGlobal;
		this.NormalGlobal = holder.NormalGlobal;
		this.TreesGlobal = holder.TreesGlobal;
		this.AmbientEmissiveMap = holder.AmbientEmissiveMap;
		this.BumpGlobalCombined = holder.BumpGlobalCombined;
		this.globalColorModifed_flag = holder.globalColorModifed_flag;
		this.globalCombinedModifed_flag = holder.globalCombinedModifed_flag;
		this.RefreshTextures(null, false);
		this.globalSettingsHolder.RestorePreset(holder);
	}

	public ReliefTerrainPresetHolder GetPresetByID(string PresetID)
	{
		if (this.presetHolders != null)
		{
			for (int i = 0; i < this.presetHolders.Length; i++)
			{
				if (this.presetHolders[i].PresetID == PresetID)
				{
					return this.presetHolders[i];
				}
			}
		}
		return null;
	}

	public ReliefTerrainPresetHolder GetPresetByName(string PresetName)
	{
		if (this.presetHolders != null)
		{
			for (int i = 0; i < this.presetHolders.Length; i++)
			{
				if (this.presetHolders[i].PresetName == PresetName)
				{
					return this.presetHolders[i];
				}
			}
		}
		return null;
	}

	public bool InterpolatePresets(string PresetID1, string PresetID2, float t)
	{
		ReliefTerrainPresetHolder presetByID = this.GetPresetByID(PresetID1);
		ReliefTerrainPresetHolder presetByID2 = this.GetPresetByID(PresetID2);
		if (presetByID == null || presetByID2 == null || presetByID.Spec == null || presetByID2.Spec == null || presetByID.Spec.Length != presetByID2.Spec.Length)
		{
			return false;
		}
		this.globalSettingsHolder.InterpolatePresets(presetByID, presetByID2, t);
		return true;
	}

	public Texture2D controlA;

	public Texture2D controlB;

	public Texture2D controlC;

	public string save_path_controlA = "";

	public string save_path_controlB = "";

	public string save_path_controlC = "";

	public string save_path_colormap = "";

	public string save_path_BumpGlobalCombined = "";

	public Texture2D NormalGlobal;

	public Texture2D TreesGlobal;

	public Texture2D ColorGlobal;

	public Texture2D AmbientEmissiveMap;

	public Texture2D BumpGlobalCombined;

	public Texture2D tmp_globalColorMap;

	public bool globalColorModifed_flag;

	public bool globalCombinedModifed_flag;

	public bool splat_layer_ordered_mode;

	public RTPColorChannels[] source_controls_channels;

	public int[] splat_layer_seq;

	public float[] splat_layer_boost;

	public bool[] splat_layer_calc;

	public bool[] splat_layer_masked;

	public RTPColorChannels[] source_controls_mask_channels;

	public Texture2D[] source_controls;

	public bool[] source_controls_invert;

	public Texture2D[] source_controls_mask;

	public bool[] source_controls_mask_invert;

	public Vector2 customTiling = new Vector2(3f, 3f);

	[SerializeField]
	public ReliefTerrainPresetHolder[] presetHolders;

	[SerializeField]
	public ReliefTerrainGlobalSettingsHolder globalSettingsHolder;
}
