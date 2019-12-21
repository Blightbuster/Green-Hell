using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[PostProcess(typeof(BrightnessSettingPPRenderer), PostProcessEvent.AfterStack, "Custom/BrightnessSettingPP", true)]
	[Serializable]
	public sealed class BrightnessSettingPP : PostProcessEffectSettings
	{
		public override void OnEnable()
		{
			base.OnEnable();
			GameSettings.OnBrightnessChanged += this.OnBrightnessChangedDel;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			GameSettings.OnBrightnessChanged -= this.OnBrightnessChangedDel;
		}

		private void OnBrightnessChangedDel(float mul)
		{
			this.m_Val.value = mul;
		}

		public FloatParameter m_Val = new FloatParameter
		{
			value = 1f
		};
	}
}
