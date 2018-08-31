using System;
using UnityEngine;

namespace UltimateWater
{
	public class ForwardModule : IRenderModule
	{
		public void OnEnable(WaterCamera camera)
		{
		}

		public void OnDisable(WaterCamera camera)
		{
		}

		public void OnValidate(WaterCamera camera)
		{
		}

		public void Process(WaterCamera waterCamera)
		{
		}

		public void Render(WaterCamera waterCamera, RenderTexture source, RenderTexture destination)
		{
			this.RenderWater(waterCamera, source);
			Graphics.Blit(source, destination);
		}

		private void RenderWater(WaterCamera waterCamera, RenderTexture source)
		{
			Camera waterRenderCamera = waterCamera._WaterRenderCamera;
			waterRenderCamera.CopyFrom(waterCamera.CameraComponent);
			waterRenderCamera.rect = new Rect(0f, 0f, 1f, 1f);
			waterRenderCamera.enabled = false;
			waterRenderCamera.clearFlags = CameraClearFlags.Nothing;
			waterRenderCamera.depthTextureMode = DepthTextureMode.None;
			waterRenderCamera.renderingPath = RenderingPath.Forward;
			waterRenderCamera.allowHDR = true;
			waterRenderCamera.targetTexture = source;
			waterRenderCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			waterRenderCamera.Render();
			waterRenderCamera.targetTexture = null;
		}
	}
}
