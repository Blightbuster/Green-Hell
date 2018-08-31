using System;
using UnityEngine;

namespace VolumetricFogAndMist
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera), typeof(VolumetricFog))]
	public class VolumetricFogPosT : MonoBehaviour, IVolumetricFogRenderComponent
	{
		public VolumetricFog fog { get; set; }

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this.fog == null || !this.fog.enabled)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (this.fog.transparencyBlendMode == TRANSPARENT_MODE.None)
			{
				this.fog.DoOnRenderImage(source, destination);
			}
			else
			{
				RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				if (this.copyOpaqueMat == null)
				{
					this.copyOpaqueMat = new Material(Shader.Find("VolumetricFogAndMist/CopyOpaque"));
				}
				this.copyOpaqueMat.SetFloat("_BlendPower", this.fog.transparencyBlendPower);
				Graphics.Blit(source, destination, this.copyOpaqueMat, (!this.fog.computeDepth || this.fog.downsampling != 1) ? 0 : 1);
				RenderTexture.ReleaseTemporary(temporary);
			}
		}

		public void DestroySelf()
		{
			UnityEngine.Object.DestroyImmediate(this);
		}

		private Material copyOpaqueMat;
	}
}
