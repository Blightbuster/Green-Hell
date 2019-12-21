using System;
using UnityEngine;

public class WaterWarpImageEffect : MonoBehaviour
{
	private void Awake()
	{
		this.mtrl = new Material(Shader.Find("Hidden/WaterWarpImageEffect"));
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (this.mtrl == null || this.mtrl.shader == null || !this.mtrl.shader.isSupported)
		{
			base.enabled = false;
			return;
		}
		this.mtrl.SetFloat("_AnimationSpeed", this.animationSpeed);
		this.mtrl.SetFloat("_WarpSpeed", this.warpSpeed);
		this.mtrl.SetFloat("_FlowSpeedU", this.flowSpeedU);
		this.mtrl.SetFloat("_FlowSpeedV", this.flowSpeedV);
		this.mtrl.SetFloat("_Bump", this.bump);
		this.mtrl.SetFloat("_WaveSize", this.waveSize);
		this.mtrl.SetFloat("_Specular", this.specular);
		Graphics.Blit(src, dest, this.mtrl, 0);
	}

	private void OnDestroy()
	{
		if (this.mtrl != null)
		{
			UnityEngine.Object.Destroy(this.mtrl);
			this.mtrl = null;
		}
	}

	[Range(0f, 2f)]
	public float animationSpeed = 1f;

	[Range(0f, 1f)]
	public float warpSpeed = 0.2f;

	[Range(-1f, 1f)]
	public float flowSpeedU = 0.3f;

	[Range(-1f, 1f)]
	public float flowSpeedV = 0.3f;

	[Range(0f, 1f)]
	public float bump = 0.5f;

	[Range(0f, 4f)]
	public float waveSize = 1.8f;

	[Range(0f, 8f)]
	public float specular = 3f;

	private Material mtrl;
}
