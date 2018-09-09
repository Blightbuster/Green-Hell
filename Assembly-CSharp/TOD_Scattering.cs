using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("Time of Day/Camera Atmospheric Scattering")]
public class TOD_Scattering : TOD_ImageEffect
{
	protected void OnEnable()
	{
		if (!this.ScatteringShader)
		{
			this.ScatteringShader = Shader.Find("Hidden/Time of Day/Scattering");
		}
		this.scatteringMaterial = base.CreateMaterial(this.ScatteringShader);
	}

	protected void OnDisable()
	{
		if (this.scatteringMaterial)
		{
			UnityEngine.Object.DestroyImmediate(this.scatteringMaterial);
		}
	}

	protected void OnPreCull()
	{
		if (this.sky && this.sky.Initialized)
		{
			this.sky.Components.AtmosphereRenderer.enabled = false;
		}
	}

	protected void OnPostRender()
	{
		if (this.sky && this.sky.Initialized)
		{
			this.sky.Components.AtmosphereRenderer.enabled = true;
		}
	}

	[ImageEffectOpaque]
	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!base.CheckSupport(true, false))
		{
			Graphics.Blit(source, destination);
			return;
		}
		this.sky.Components.Scattering = this;
		float heightFalloff = this.HeightFalloff;
		float y = Mathf.Exp(-heightFalloff * (this.cam.transform.position.y - this.ZeroLevel));
		float globalDensity = this.GlobalDensity;
		this.scatteringMaterial.SetMatrix("_FrustumCornersWS", base.FrustumCorners());
		this.scatteringMaterial.SetTexture("_DitheringTexture", this.DitheringTexture);
		this.scatteringMaterial.SetVector("_Density", new Vector4(heightFalloff, y, globalDensity, 0f));
		base.CustomBlit(source, destination, this.scatteringMaterial, 0);
	}

	public Shader ScatteringShader;

	public Texture2D DitheringTexture;

	[Range(0f, 1f)]
	public float GlobalDensity = 0.001f;

	[Range(0f, 1f)]
	public float HeightFalloff = 0.001f;

	public float ZeroLevel;

	private Material scatteringMaterial;
}
