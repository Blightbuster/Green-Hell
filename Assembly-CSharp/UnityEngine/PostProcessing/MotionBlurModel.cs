using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class MotionBlurModel : PostProcessingModel
	{
		public MotionBlurModel.Settings settings
		{
			get
			{
				return this.m_Settings;
			}
			set
			{
				this.m_Settings = value;
			}
		}

		public override void Reset()
		{
			this.m_Settings = MotionBlurModel.Settings.defaultSettings;
		}

		[SerializeField]
		private MotionBlurModel.Settings m_Settings = MotionBlurModel.Settings.defaultSettings;

		[Serializable]
		public struct Settings
		{
			public static MotionBlurModel.Settings defaultSettings
			{
				get
				{
					return new MotionBlurModel.Settings
					{
						shutterAngle = 270f,
						sampleCount = 10,
						frameBlending = 0f
					};
				}
			}

			[Tooltip("The angle of rotary shutter. Larger values give longer exposure.")]
			[Range(0f, 360f)]
			public float shutterAngle;

			[Tooltip("The amount of sample points, which affects quality and performances.")]
			[Range(4f, 32f)]
			public int sampleCount;

			[Tooltip("The strength of multiple frame blending. The opacity of preceding frames are determined from this coefficient and time differences.")]
			[Range(0f, 1f)]
			public float frameBlending;
		}
	}
}
