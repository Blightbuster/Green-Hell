using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater.Internal
{
	public static class GraphicsUtilities
	{
		public static void Blit(Texture source, RenderTexture target, Material material, int shaderPass, MaterialPropertyBlock properties)
		{
			if (GraphicsUtilities._CommandBuffer == null)
			{
				GraphicsUtilities._CommandBuffer = new CommandBuffer
				{
					name = "UltimateWater Custom Blit"
				};
			}
			GL.PushMatrix();
			GL.modelview = Matrix4x4.identity;
			GL.LoadProjectionMatrix(Matrix4x4.identity);
			material.mainTexture = source;
			GraphicsUtilities._CommandBuffer.SetRenderTarget(target);
			GraphicsUtilities._CommandBuffer.DrawMesh(Quads.BipolarXY, Matrix4x4.identity, material, 0, shaderPass, properties);
			Graphics.ExecuteCommandBuffer(GraphicsUtilities._CommandBuffer);
			RenderTexture.active = target;
			GL.PopMatrix();
			GraphicsUtilities._CommandBuffer.Clear();
		}

		private static CommandBuffer _CommandBuffer;
	}
}
