using System;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/MIP solver for standalone shader")]
[ExecuteInEditMode]
public class ReliefTerrainVertexBlendTriplanar : MonoBehaviour
{
	public void SetupMIPOffsets()
	{
		if (base.GetComponent<Renderer>() == null && this.material == null)
		{
			return;
		}
		Material sharedMaterial;
		if (base.GetComponent<Renderer>() != null)
		{
			sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
		}
		else
		{
			sharedMaterial = this.material;
		}
		if (sharedMaterial == null)
		{
			return;
		}
		if (sharedMaterial.HasProperty("_SplatAtlasA"))
		{
			this.SetupTex("_SplatAtlasA", "rtp_mipoffset_color", 1f, -1f);
		}
		if (sharedMaterial.HasProperty("_SplatA0"))
		{
			this.SetupTex("_SplatA0", "rtp_mipoffset_color", 1f, 0f);
		}
		this.SetupTex("_BumpMap01", "rtp_mipoffset_bump", 1f, 0f);
		this.SetupTex("_TERRAIN_HeightMap", "rtp_mipoffset_height", 1f, 0f);
		if (sharedMaterial.HasProperty("_BumpMapGlobal"))
		{
			this.SetupTex("_BumpMapGlobal", "rtp_mipoffset_globalnorm", sharedMaterial.GetFloat("_BumpMapGlobalScale"), sharedMaterial.GetFloat("rtp_mipoffset_globalnorm_offset"));
			if (sharedMaterial.HasProperty("_SuperDetailTiling"))
			{
				this.SetupTex("_BumpMapGlobal", "rtp_mipoffset_superdetail", sharedMaterial.GetFloat("_SuperDetailTiling"), 0f);
			}
			if (sharedMaterial.HasProperty("TERRAIN_FlowScale"))
			{
				this.SetupTex("_BumpMapGlobal", "rtp_mipoffset_flow", sharedMaterial.GetFloat("TERRAIN_FlowScale"), sharedMaterial.GetFloat("TERRAIN_FlowMipOffset"));
			}
		}
		if (sharedMaterial.HasProperty("TERRAIN_RippleMap"))
		{
			this.SetupTex("TERRAIN_RippleMap", "rtp_mipoffset_ripple", sharedMaterial.GetFloat("TERRAIN_RippleScale"), 0f);
		}
		if (sharedMaterial.HasProperty("TERRAIN_CausticsTex"))
		{
			this.SetupTex("TERRAIN_CausticsTex", "rtp_mipoffset_caustics", sharedMaterial.GetFloat("TERRAIN_CausticsTilingScale"), 0f);
		}
	}

	private void SetupTex(string tex_name, string param_name, float _mult = 1f, float _add = 0f)
	{
		if (base.GetComponent<Renderer>() == null && this.material == null)
		{
			return;
		}
		Material sharedMaterial;
		if (base.GetComponent<Renderer>() != null)
		{
			sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
		}
		else
		{
			sharedMaterial = this.material;
		}
		if (sharedMaterial == null)
		{
			return;
		}
		if (sharedMaterial.GetTexture(tex_name) != null)
		{
			int width = sharedMaterial.GetTexture(tex_name).width;
			sharedMaterial.SetFloat(param_name, -Mathf.Log(1024f / ((float)width * _mult)) / Mathf.Log(2f) + _add);
		}
	}

	public void SetTopPlanarUVBounds()
	{
		MeshFilter meshFilter = base.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if (meshFilter == null)
		{
			return;
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		if (vertices.Length == 0)
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(vertices[0]);
		Vector4 value;
		value.x = vector.x;
		value.y = vector.z;
		value.z = vector.x;
		value.w = vector.z;
		for (int i = 1; i < vertices.Length; i++)
		{
			vector = base.transform.TransformPoint(vertices[i]);
			if (vector.x < value.x)
			{
				value.x = vector.x;
			}
			if (vector.z < value.y)
			{
				value.y = vector.z;
			}
			if (vector.x > value.z)
			{
				value.z = vector.x;
			}
			if (vector.z > value.w)
			{
				value.w = vector.z;
			}
		}
		value.z -= value.x;
		value.w -= value.y;
		if (base.GetComponent<Renderer>() == null && this.material == null)
		{
			return;
		}
		Material sharedMaterial;
		if (base.GetComponent<Renderer>() != null)
		{
			sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
		}
		else
		{
			sharedMaterial = this.material;
		}
		if (sharedMaterial == null)
		{
			return;
		}
		sharedMaterial.SetVector("_TERRAIN_PosSize", value);
	}

	private void Awake()
	{
		this.SetupMIPOffsets();
		if (Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			this.SetupMIPOffsets();
		}
	}

	public Texture2D tmp_globalColorMap;

	public string save_path_colormap = string.Empty;

	public GeometryVsTerrainBlend painterInstance;

	public Material material;
}
