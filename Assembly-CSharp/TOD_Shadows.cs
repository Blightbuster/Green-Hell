using System;
using UnityEngine;

[AddComponentMenu("Time of Day/Camera Cloud Shadows")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class TOD_Shadows : TOD_ImageEffect
{
	protected void OnEnable()
	{
		if (!this.ShadowShader)
		{
			this.ShadowShader = Shader.Find("Hidden/Time of Day/Cloud Shadows");
		}
		this.shadowMaterial = base.CreateMaterial(this.ShadowShader);
	}

	protected void OnDisable()
	{
		if (this.shadowMaterial)
		{
			UnityEngine.Object.DestroyImmediate(this.shadowMaterial);
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
		this.sky.Components.Shadows = this;
		this.shadowMaterial.SetMatrix("_FrustumCornersWS", base.FrustumCorners());
		this.shadowMaterial.SetTexture("_CloudTex", this.CloudTexture);
		this.shadowMaterial.SetFloat("_Cutoff", this.Cutoff);
		this.shadowMaterial.SetFloat("_Fade", this.Fade);
		this.shadowMaterial.SetFloat("_Intensity", this.Intensity * Mathf.Clamp01(1f - this.sky.SunZenith / 90f));
		base.CustomBlit(source, destination, this.shadowMaterial, 0);
	}

	public Shader ShadowShader;

	public Texture2D CloudTexture;

	[Range(0f, 1f)]
	public float Cutoff;

	[Range(0f, 1f)]
	public float Fade;

	[Range(0f, 1f)]
	public float Intensity = 0.5f;

	private Material shadowMaterial;
}
