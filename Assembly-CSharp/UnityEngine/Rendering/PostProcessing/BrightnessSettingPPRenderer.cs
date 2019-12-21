using System;

namespace UnityEngine.Rendering.PostProcessing
{
	public sealed class BrightnessSettingPPRenderer : PostProcessEffectRenderer<BrightnessSettingPP>
	{
		public override void Render(PostProcessRenderContext context)
		{
			PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Custom/BrightnessSettingPP"));
			propertySheet.properties.SetFloat("_Val", base.settings.m_Val);
			context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0, false);
		}
	}
}
