using System;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessExample : MonoBehaviour
{
	private void Awake()
	{
		if (this.PostProcessMat == null)
		{
			base.enabled = false;
		}
		else
		{
			this.PostProcessMat.mainTexture = this.PostProcessMat.mainTexture;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, this.PostProcessMat);
	}

	public Material PostProcessMat;
}
