using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LuxWater_CameraDepthMode : MonoBehaviour
	{
		private void OnEnable()
		{
			this.cam = base.GetComponent<Camera>();
			this.cam.depthTextureMode |= DepthTextureMode.Depth;
			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
			{
				Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnPrecull));
				this.CamCallBackAdded = true;
				this.CopyDepthMat = new Material(Shader.Find("Hidden/Lux Water/CopyDepth"));
				this.format = RenderTextureFormat.RFloat;
				if (!SystemInfo.SupportsRenderTextureFormat(this.format))
				{
					this.format = RenderTextureFormat.RHalf;
				}
				if (!SystemInfo.SupportsRenderTextureFormat(this.format))
				{
					this.format = RenderTextureFormat.ARGBHalf;
				}
			}
		}

		private void OnDisable()
		{
			if (this.CamCallBackAdded)
			{
				Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnPrecull));
				foreach (KeyValuePair<Camera, CommandBuffer> keyValuePair in this.m_cmdBuffer)
				{
					if (keyValuePair.Key != null)
					{
						keyValuePair.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, keyValuePair.Value);
					}
				}
				this.m_cmdBuffer.Clear();
			}
			this.ShowShaderWarning = true;
		}

		private void OnPrecull(Camera camera)
		{
			if (this.GrabDepthTexture)
			{
				if (this.cam.actualRenderingPath == RenderingPath.DeferredShading && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
				{
					CommandBuffer commandBuffer;
					if (!this.m_cmdBuffer.TryGetValue(camera, out commandBuffer))
					{
						commandBuffer = new CommandBuffer();
						commandBuffer.name = "Lux Water Grab Depth";
						camera.AddCommandBuffer(CameraEvent.AfterLighting, commandBuffer);
						this.m_cmdBuffer[camera] = commandBuffer;
					}
					commandBuffer.Clear();
					int pixelWidth = camera.pixelWidth;
					int pixelHeight = camera.pixelHeight;
					int nameID = Shader.PropertyToID("_Lux_GrabbedDepth");
					commandBuffer.GetTemporaryRT(nameID, pixelWidth, pixelHeight, 0, FilterMode.Point, this.format, RenderTextureReadWrite.Linear);
					commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, nameID, this.CopyDepthMat, 0);
					return;
				}
				this.GrabDepthTexture = false;
				foreach (KeyValuePair<Camera, CommandBuffer> keyValuePair in this.m_cmdBuffer)
				{
					if (keyValuePair.Key != null)
					{
						keyValuePair.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, keyValuePair.Value);
					}
				}
				this.m_cmdBuffer.Clear();
				this.ShowShaderWarning = true;
			}
		}

		public bool GrabDepthTexture;

		private Camera cam;

		private CommandBuffer cb_DepthGrab;

		private Material CopyDepthMat;

		private RenderTextureFormat format;

		private Dictionary<Camera, CommandBuffer> m_cmdBuffer = new Dictionary<Camera, CommandBuffer>();

		private bool CamCallBackAdded;

		[HideInInspector]
		public bool ShowShaderWarning = true;
	}
}
