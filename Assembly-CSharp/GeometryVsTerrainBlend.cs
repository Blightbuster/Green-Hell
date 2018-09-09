using System;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Relief Terrain/Geometry Blend")]
public class GeometryVsTerrainBlend : MonoBehaviour
{
	private void Start()
	{
		this.SetupValues();
	}

	private void GetRenderer()
	{
		if (!this._renderer)
		{
			this._renderer = base.GetComponent<Renderer>();
		}
	}

	public void SetupValues()
	{
		if (this.blendedObject && (this.blendedObject.GetComponent(typeof(MeshRenderer)) != null || this.blendedObject.GetComponent(typeof(Terrain)) != null))
		{
			if (this.underlying_transform == null)
			{
				this.underlying_transform = base.transform.Find("RTP_blend_underlying");
			}
			if (this.underlying_transform != null)
			{
				GameObject gameObject = this.underlying_transform.gameObject;
				this.underlying_renderer = (MeshRenderer)gameObject.GetComponent(typeof(MeshRenderer));
			}
			if (this.underlying_renderer != null && this.underlying_renderer.sharedMaterial != null)
			{
				ReliefTerrain reliefTerrain = (ReliefTerrain)this.blendedObject.GetComponent(typeof(ReliefTerrain));
				if (reliefTerrain)
				{
					Material sharedMaterial = this.underlying_renderer.sharedMaterial;
					reliefTerrain.RefreshTextures(sharedMaterial, false);
					reliefTerrain.globalSettingsHolder.Refresh(sharedMaterial, null);
					if (sharedMaterial.HasProperty("RTP_DeferredAddPassSpec"))
					{
						sharedMaterial.SetFloat("RTP_DeferredAddPassSpec", this._DeferredBlendGloss);
					}
					if (reliefTerrain.controlA)
					{
						sharedMaterial.SetTexture("_Control", reliefTerrain.controlA);
					}
					if (reliefTerrain.ColorGlobal)
					{
						sharedMaterial.SetTexture("_Splat0", reliefTerrain.ColorGlobal);
					}
					if (reliefTerrain.NormalGlobal)
					{
						sharedMaterial.SetTexture("_Splat1", reliefTerrain.NormalGlobal);
					}
					if (reliefTerrain.TreesGlobal)
					{
						sharedMaterial.SetTexture("_Splat2", reliefTerrain.TreesGlobal);
					}
					if (reliefTerrain.BumpGlobalCombined)
					{
						sharedMaterial.SetTexture("_Splat3", reliefTerrain.BumpGlobalCombined);
					}
				}
				Terrain terrain = (Terrain)this.blendedObject.GetComponent(typeof(Terrain));
				if (terrain)
				{
					this.underlying_renderer.lightmapIndex = terrain.lightmapIndex;
					this.underlying_renderer.lightmapScaleOffset = terrain.lightmapScaleOffset;
					this.underlying_renderer.realtimeLightmapIndex = terrain.realtimeLightmapIndex;
					this.underlying_renderer.realtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset;
				}
				else
				{
					this.underlying_renderer.lightmapIndex = this.blendedObject.GetComponent<Renderer>().lightmapIndex;
					this.underlying_renderer.lightmapScaleOffset = this.blendedObject.GetComponent<Renderer>().lightmapScaleOffset;
					this.underlying_renderer.realtimeLightmapIndex = this.blendedObject.GetComponent<Renderer>().realtimeLightmapIndex;
					this.underlying_renderer.realtimeLightmapScaleOffset = this.blendedObject.GetComponent<Renderer>().realtimeLightmapScaleOffset;
				}
				if (this.Sticked)
				{
					if (terrain)
					{
						base.GetComponent<Renderer>().lightmapIndex = terrain.lightmapIndex;
						base.GetComponent<Renderer>().lightmapScaleOffset = terrain.lightmapScaleOffset;
						base.GetComponent<Renderer>().realtimeLightmapIndex = terrain.realtimeLightmapIndex;
						base.GetComponent<Renderer>().realtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset;
					}
					else
					{
						base.GetComponent<Renderer>().lightmapIndex = this.blendedObject.GetComponent<Renderer>().lightmapIndex;
						base.GetComponent<Renderer>().lightmapScaleOffset = this.blendedObject.GetComponent<Renderer>().lightmapScaleOffset;
						base.GetComponent<Renderer>().realtimeLightmapIndex = this.blendedObject.GetComponent<Renderer>().realtimeLightmapIndex;
						base.GetComponent<Renderer>().realtimeLightmapScaleOffset = this.blendedObject.GetComponent<Renderer>().realtimeLightmapScaleOffset;
					}
				}
			}
		}
	}

	public double UpdTim;

	private int progress_count_max;

	private int progress_count_current;

	private const int progress_granulation = 1000;

	private string progress_description = string.Empty;

	public float blend_distance = 0.1f;

	public GameObject blendedObject;

	public bool VoxelBlendedObject;

	public float _DeferredBlendGloss = 0.8f;

	[HideInInspector]
	public bool undo_flag;

	[HideInInspector]
	public bool paint_flag;

	[HideInInspector]
	public int paint_mode;

	[HideInInspector]
	public float paint_size = 0.5f;

	[HideInInspector]
	public float paint_smoothness;

	[HideInInspector]
	public float paint_opacity = 1f;

	[HideInInspector]
	public RTPColorChannels vertex_paint_channel = RTPColorChannels.A;

	[HideInInspector]
	public int addTrisSubdivision;

	[HideInInspector]
	public float addTrisMinAngle;

	[HideInInspector]
	public float addTrisMaxAngle = 90f;

	private Vector3[] paint_vertices;

	private Vector3[] paint_normals;

	private int[] paint_tris;

	private Transform underlying_transform;

	private MeshRenderer underlying_renderer;

	[HideInInspector]
	public RaycastHit paintHitInfo;

	[HideInInspector]
	public bool paintHitInfo_flag;

	[HideInInspector]
	private Texture2D tmp_globalColorMap;

	[HideInInspector]
	public Vector3[] normals_orig;

	[HideInInspector]
	public Vector4[] tangents_orig;

	[HideInInspector]
	public bool baked_normals;

	[HideInInspector]
	public Mesh orig_mesh;

	[HideInInspector]
	public Mesh pmesh;

	[HideInInspector]
	public bool shader_global_blend_capabilities;

	[HideInInspector]
	public float StickOffset = 0.03f;

	[HideInInspector]
	public bool Sticked;

	[HideInInspector]
	public bool StickedOptimized = true;

	[HideInInspector]
	public bool ModifyTris;

	[HideInInspector]
	public bool BuildMeshFlag;

	[HideInInspector]
	public bool RealizePaint_Flag;

	[HideInInspector]
	public string save_path = string.Empty;

	[HideInInspector]
	public bool isBatched;

	private Renderer _renderer;
}
