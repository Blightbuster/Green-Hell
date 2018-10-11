using System;
using UnityEngine;

namespace VolumetricFogAndMist
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera), typeof(VolumetricFog))]
	public class VolumetricFogPreT : MonoBehaviour, IVolumetricFogRenderComponent
	{
		public VolumetricFog fog { get; set; }

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this.fog == null || !this.fog.enabled)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (this.fog.renderBeforeTransparent)
			{
				this.fog.DoOnRenderImage(source, destination);
			}
			else
			{
				RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				this.fog.DoOnRenderImage(source, temporary);
				Shader.SetGlobalTexture("_VolumetricFog_OpaqueFrame", temporary);
				Graphics.Blit(temporary, destination);
			}
		}

		private void OnPostRender()
		{
			if (this.opaqueFrame != null)
			{
				RenderTexture.ReleaseTemporary(this.opaqueFrame);
				this.opaqueFrame = null;
			}
		}

		public void DestroySelf()
		{
			UnityEngine.Object.DestroyImmediate(this);
		}

		private RenderTexture opaqueFrame;
	}
}
