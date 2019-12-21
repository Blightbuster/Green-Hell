using System;
using UnityEngine;

public class _appControlerTerrain2Geometry : MonoBehaviour
{
	private void Awake()
	{
		this.GetLODManager();
		this.panel_enabled = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			this.panel_enabled = !this.panel_enabled;
		}
		if (Input.GetKey(KeyCode.Period))
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, Mathf.Min(base.transform.localPosition.y + 0.5f, 50f), base.transform.localPosition.z);
		}
		if (Input.GetKey(KeyCode.Comma))
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, Mathf.Max(base.transform.localPosition.y - 0.5f, 0.9f), base.transform.localPosition.z);
		}
	}

	private void GetLODManager()
	{
		GameObject gameObject = GameObject.Find("_RTP_LODmanager");
		if (gameObject == null)
		{
			return;
		}
		this.LODmanager = (RTP_LODmanager)gameObject.GetComponent(typeof(RTP_LODmanager));
	}

	private void OnGUI()
	{
		if (!this.LODmanager)
		{
			this.GetLODManager();
			return;
		}
		GUILayout.Space(10f);
		GUILayout.BeginVertical("box", Array.Empty<GUILayoutOption>());
		GUILayout.Label(string.Concat(FPSmeter.fps), Array.Empty<GUILayoutOption>());
		if (this.panel_enabled)
		{
			this.shadows = GUILayout.Toggle(this.shadows, "disable Unity's shadows", Array.Empty<GUILayoutOption>());
			Light component = GameObject.Find("Directional light").GetComponent<Light>();
			component.shadows = (this.shadows ? LightShadows.None : LightShadows.Soft);
			this.forward_path = GUILayout.Toggle(this.forward_path, "forward rendering", Array.Empty<GUILayoutOption>());
			GameObject.Find("Main Camera").GetComponent<Camera>().renderingPath = (this.forward_path ? RenderingPath.Forward : RenderingPath.DeferredShading);
			if (this.forward_path)
			{
				RenderSettings.ambientLight = new Color32(25, 25, 25, 0);
			}
			else
			{
				RenderSettings.ambientLight = new Color32(93, 103, 122, 0);
			}
			TerrainShaderLod rtp_LODlevel = this.LODmanager.RTP_LODlevel;
			TerrainShaderLod terrainShaderLod = rtp_LODlevel;
			switch (rtp_LODlevel)
			{
			case TerrainShaderLod.POM:
				if (GUILayout.Button("POM shading", Array.Empty<GUILayoutOption>()))
				{
					terrainShaderLod = TerrainShaderLod.PM;
				}
				break;
			case TerrainShaderLod.PM:
				if (GUILayout.Button("PM shading", Array.Empty<GUILayoutOption>()))
				{
					terrainShaderLod = TerrainShaderLod.SIMPLE;
				}
				break;
			case TerrainShaderLod.SIMPLE:
				if (GUILayout.Button("SIMPLE shading", Array.Empty<GUILayoutOption>()))
				{
					terrainShaderLod = TerrainShaderLod.POM;
				}
				break;
			}
			switch (terrainShaderLod)
			{
			case TerrainShaderLod.POM:
				if (terrainShaderLod != rtp_LODlevel)
				{
					(GameObject.Find("terrainMesh").GetComponent(typeof(ReliefTerrain)) as ReliefTerrain).globalSettingsHolder.Refresh(null, null);
					this.LODmanager.RTP_LODlevel = TerrainShaderLod.POM;
					this.LODmanager.RefreshLODlevel();
				}
				break;
			case TerrainShaderLod.PM:
				if (terrainShaderLod != rtp_LODlevel)
				{
					(GameObject.Find("terrainMesh").GetComponent(typeof(ReliefTerrain)) as ReliefTerrain).globalSettingsHolder.Refresh(null, null);
					this.LODmanager.RTP_LODlevel = TerrainShaderLod.PM;
					this.LODmanager.RefreshLODlevel();
				}
				break;
			case TerrainShaderLod.SIMPLE:
				if (terrainShaderLod != rtp_LODlevel)
				{
					(GameObject.Find("terrainMesh").GetComponent(typeof(ReliefTerrain)) as ReliefTerrain).globalSettingsHolder.Refresh(null, null);
					this.LODmanager.RTP_LODlevel = TerrainShaderLod.SIMPLE;
					this.LODmanager.RefreshLODlevel();
				}
				break;
			}
			if (terrainShaderLod == TerrainShaderLod.POM)
			{
				this.terrain_self_shadow = this.LODmanager.RTP_SHADOWS;
				bool flag = GUILayout.Toggle(this.terrain_self_shadow, "self shadowing", Array.Empty<GUILayoutOption>());
				if (flag != this.terrain_self_shadow)
				{
					this.LODmanager.RTP_SHADOWS = flag;
					this.LODmanager.RefreshLODlevel();
				}
				this.terrain_self_shadow = flag;
				if (this.terrain_self_shadow)
				{
					this.terrain_smooth_shadows = this.LODmanager.RTP_SOFT_SHADOWS;
					bool flag2 = GUILayout.Toggle(this.terrain_smooth_shadows, "smooth shadows", Array.Empty<GUILayoutOption>());
					if (flag2 != this.terrain_smooth_shadows)
					{
						this.LODmanager.RTP_SOFT_SHADOWS = flag2;
						this.LODmanager.RefreshLODlevel();
					}
					this.terrain_smooth_shadows = flag2;
				}
			}
			if (this.LODmanager.RTP_SNOW_FIRST)
			{
				ReliefTerrain reliefTerrain = GameObject.Find("terrainMesh").GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label("Snow", new GUILayoutOption[]
				{
					GUILayout.MaxWidth(40f)
				});
				float num = GUILayout.HorizontalSlider(reliefTerrain.globalSettingsHolder._snow_strength, 0f, 1f, Array.Empty<GUILayoutOption>());
				if (num != reliefTerrain.globalSettingsHolder._snow_strength)
				{
					reliefTerrain.globalSettingsHolder._snow_strength = num;
					reliefTerrain.globalSettingsHolder.Refresh(null, null);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.Label("Light", new GUILayoutOption[]
			{
				GUILayout.MaxWidth(40f)
			});
			this.light_dir = GUILayout.HorizontalSlider(this.light_dir, 0f, 360f, Array.Empty<GUILayoutOption>());
			component.transform.rotation = Quaternion.Euler(40f, this.light_dir, 0f);
			GUILayout.Label("  F (hold) - freeze camera", Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ,/. - change cam position", Array.Empty<GUILayoutOption>());
		}
		GUILayout.Label("  P - toggle panel", Array.Empty<GUILayoutOption>());
		GUILayout.EndVertical();
	}

	public bool shadows;

	public bool forward_path = true;

	public bool terrain_self_shadow;

	public bool terrain_smooth_shadows = true;

	private bool panel_enabled;

	public float light_dir = 285f;

	public float preset_param_interp;

	private RTP_LODmanager LODmanager;
}
