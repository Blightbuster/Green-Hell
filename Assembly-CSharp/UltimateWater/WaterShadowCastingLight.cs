using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public sealed class WaterShadowCastingLight : MonoBehaviour
	{
		private void Start()
		{
			int waterShadowmap = ShaderVariables.WaterShadowmap;
			this._CommandBuffer = new CommandBuffer
			{
				name = "Water: Copy Shadowmap"
			};
			this._CommandBuffer.GetTemporaryRT(waterShadowmap, Screen.width, Screen.height, 32, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			this._CommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, waterShadowmap);
			Light component = base.GetComponent<Light>();
			component.AddCommandBuffer(LightEvent.AfterScreenspaceMask, this._CommandBuffer);
		}

		private CommandBuffer _CommandBuffer;
	}
}
