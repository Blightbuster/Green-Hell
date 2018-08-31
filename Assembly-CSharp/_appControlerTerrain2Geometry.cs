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
		GUILayout.BeginVertical("box", new GUILayoutOption[0]);
		GUILayout.Label(string.Empty + FPSmeter.fps, new GUILayoutOption[0]);
		if (this.panel_enabled)
		{
			this.shadows = GUILayout.Toggle(this.shadows, "disable Unity's shadows", new GUILayoutOption[0]);
			Light component = GameObject.Find("Directional light").GetComponent<Light>();
			component.shadows = ((!this.shadows) ? LightShadows.Soft : LightShadows.None);
			this.forward_path = GUILayout.Toggle(this.forward_path, "forward rendering", new GUILayoutOption[0]);
			Camera component2 = GameObject.Find("Main Camera").GetComponent<Camera>();
			component2.renderingPath = ((!this.forward_path) ? RenderingPath.DeferredShading : RenderingPath.Forward);
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
			if (rtp_LODlevel != TerrainShaderLod.POM)
			{
				if (rtp_LODlevel != TerrainShaderLod.PM)
				{
					if (rtp_LODlevel == TerrainShaderLod.SIMPLE)
					{
						if (GUILayout.Button("SIMPLE shading", new GUILayoutOption[0]))
						{
							terrainShaderLod = TerrainShaderLod.POM;
						}
					}
				}
				else if (GUILayout.Button("PM shading", new GUILayoutOption[0]))
				{
					terrainShaderLod = TerrainShaderLod.SIMPLE;
				}
			}
			else if (GUILayout.Button("POM shading", new GUILayoutOption[0]))
			{
				terrainShaderLod = TerrainShaderLod.PM;
			}
			if (terrainShaderLod != TerrainShaderLod.POM)
			{
				if (terrainShaderLod != TerrainShaderLod.PM)
				{
					if (terrainShaderLod == TerrainShaderLod.SIMPLE)
					{
						if (terrainShaderLod != rtp_LODlevel)
						{
							GameObject gameObject = GameObject.Find("terrainMesh");
							ReliefTerrain reliefTerrain = gameObject.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
							reliefTerrain.globalSettingsHolder.Refresh(null, null);
							this.LODmanager.RTP_LODlevel = TerrainShaderLod.SIMPLE;
							this.LODmanager.RefreshLODlevel();
						}
					}
				}
				else if (terrainShaderLod != rtp_LODlevel)
				{
					GameObject gameObject2 = GameObject.Find("terrainMesh");
					ReliefTerrain reliefTerrain2 = gameObject2.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
					reliefTerrain2.globalSettingsHolder.Refresh(null, null);
					this.LODmanager.RTP_LODlevel = TerrainShaderLod.PM;
					this.LODmanager.RefreshLODlevel();
				}
			}
			else if (terrainShaderLod != rtp_LODlevel)
			{
				GameObject gameObject3 = GameObject.Find("terrainMesh");
				ReliefTerrain reliefTerrain3 = gameObject3.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
				reliefTerrain3.globalSettingsHolder.Refresh(null, null);
				this.LODmanager.RTP_LODlevel = TerrainShaderLod.POM;
				this.LODmanager.RefreshLODlevel();
			}
			if (terrainShaderLod == TerrainShaderLod.POM)
			{
				this.terrain_self_shadow = this.LODmanager.RTP_SHADOWS;
				bool flag = GUILayout.Toggle(this.terrain_self_shadow, "self shadowing", new GUILayoutOption[0]);
				if (flag != this.terrain_self_shadow)
				{
					this.LODmanager.RTP_SHADOWS = flag;
					this.LODmanager.RefreshLODlevel();
				}
				this.terrain_self_shadow = flag;
				if (this.terrain_self_shadow)
				{
					this.terrain_smooth_shadows = this.LODmanager.RTP_SOFT_SHADOWS;
					bool flag2 = GUILayout.Toggle(this.terrain_smooth_shadows, "smooth shadows", new GUILayoutOption[0]);
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
				GameObject gameObject4 = GameObject.Find("terrainMesh");
				ReliefTerrain reliefTerrain4 = gameObject4.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Label("Snow", new GUILayoutOption[]
				{
					GUILayout.MaxWidth(40f)
				});
				float num = GUILayout.HorizontalSlider(reliefTerrain4.globalSettingsHolder._snow_strength, 0f, 1f, new GUILayoutOption[0]);
				if (num != reliefTerrain4.globalSettingsHolder._snow_strength)
				{
					reliefTerrain4.globalSettingsHolder._snow_strength = num;
					reliefTerrain4.globalSettingsHolder.Refresh(null, null);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.Label("Light", new GUILayoutOption[]
			{
				GUILayout.MaxWidth(40f)
			});
			this.light_dir = GUILayout.HorizontalSlider(this.light_dir, 0f, 360f, new GUILayoutOption[0]);
			component.transform.rotation = Quaternion.Euler(40f, this.light_dir, 0f);
			GUILayout.Label("  F (hold) - freeze camera", new GUILayoutOption[0]);
			GUILayout.Label("  ,/. - change cam position", new GUILayoutOption[0]);
		}
		GUILayout.Label("  P - toggle panel", new GUILayoutOption[0]);
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
