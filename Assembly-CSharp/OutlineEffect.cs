using System;
using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
	public Material material
	{
		get
		{
			if (this.mat == null)
			{
				this.mat = new Material(this.EdgeDetectShader);
				this.mat.hideFlags = HideFlags.HideAndDontSave;
			}
			return this.mat;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (!this.EdgeDetectShader && !this.EdgeDetectShader.isSupported)
		{
			base.enabled = false;
			return;
		}
		base.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		Shader shader = Shader.Find("Hidden/Image Effects/EdgeOutlineReplacementShader");
		base.GetComponent<Camera>().SetReplacementShader(shader, "RenderType");
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (this.EdgeDetectShader != null)
		{
			Graphics.Blit(src, dst, this.material);
		}
		else
		{
			Graphics.Blit(src, dst);
		}
	}

	public Shader EdgeDetectShader;

	private Material mat;
}
