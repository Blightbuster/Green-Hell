using System;
using UnityEngine;

[AddComponentMenu("Image Effects/Sonic Ether/SE Natural Bloom and Dirty Lens")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SENaturalBloomAndDirtyLens : MonoBehaviour
{
	private void Start()
	{
		this.isSupported = true;
		if (!this.material)
		{
			this.material = new Material(this.shader);
		}
		if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			this.isSupported = false;
		}
	}

	private void OnDisable()
	{
		if (this.material)
		{
			UnityEngine.Object.DestroyImmediate(this.material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!this.isSupported)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (!this.material)
		{
			this.material = new Material(this.shader);
		}
		this.material.hideFlags = HideFlags.HideAndDontSave;
		this.material.SetFloat("_BloomIntensity", Mathf.Exp(this.bloomIntensity) - 1f);
		this.material.SetFloat("_LensDirtIntensity", Mathf.Exp(this.lensDirtIntensity) - 1f);
		source.filterMode = FilterMode.Bilinear;
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		Graphics.Blit(source, temporary, this.material, 4);
		int num = source.width / 2;
		int num2 = source.height / 2;
		RenderTexture source2 = temporary;
		int num3 = 2;
		for (int i = 0; i < 6; i++)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(num, num2, 0, source.format);
			renderTexture.filterMode = FilterMode.Bilinear;
			Graphics.Blit(source2, renderTexture, this.material, 1);
			source2 = renderTexture;
			float num4;
			if (i > 1)
			{
				num4 = 1f;
			}
			else
			{
				num4 = 0.5f;
			}
			if (i == 2)
			{
				num4 = 0.75f;
			}
			for (int j = 0; j < num3; j++)
			{
				this.material.SetFloat("_BlurSize", (this.blurSize * 0.5f + (float)j) * num4);
				RenderTexture temporary2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
				temporary2.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture, temporary2, this.material, 2);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary2;
				temporary2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
				temporary2.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture, temporary2, this.material, 3);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary2;
			}
			switch (i)
			{
			case 0:
				this.material.SetTexture("_Bloom0", renderTexture);
				break;
			case 1:
				this.material.SetTexture("_Bloom1", renderTexture);
				break;
			case 2:
				this.material.SetTexture("_Bloom2", renderTexture);
				break;
			case 3:
				this.material.SetTexture("_Bloom3", renderTexture);
				break;
			case 4:
				this.material.SetTexture("_Bloom4", renderTexture);
				break;
			case 5:
				this.material.SetTexture("_Bloom5", renderTexture);
				break;
			}
			RenderTexture.ReleaseTemporary(renderTexture);
			num /= 2;
			num2 /= 2;
		}
		this.material.SetTexture("_LensDirt", this.lensDirtTexture);
		Graphics.Blit(temporary, destination, this.material, 0);
		RenderTexture.ReleaseTemporary(temporary);
	}

	[Range(0f, 0.4f)]
	public float bloomIntensity = 0.05f;

	public Shader shader;

	private Material material;

	public Texture2D lensDirtTexture;

	[Range(0f, 0.95f)]
	public float lensDirtIntensity = 0.05f;

	private bool isSupported;

	private float blurSize = 4f;

	public bool inputIsHDR;
}
