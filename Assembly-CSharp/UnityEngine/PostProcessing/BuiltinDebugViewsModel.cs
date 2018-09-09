using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class BuiltinDebugViewsModel : PostProcessingModel
	{
		public BuiltinDebugViewsModel.Settings settings
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

		public bool willInterrupt
		{
			get
			{
				return !this.IsModeActive(BuiltinDebugViewsModel.Mode.None) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.EyeAdaptation) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.PreGradingLog) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.LogLut) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.UserLut);
			}
		}

		public override void Reset()
		{
			this.settings = BuiltinDebugViewsModel.Settings.defaultSettings;
		}

		public bool IsModeActive(BuiltinDebugViewsModel.Mode mode)
		{
			return this.m_Settings.mode == mode;
		}

		[SerializeField]
		private BuiltinDebugViewsModel.Settings m_Settings = BuiltinDebugViewsModel.Settings.defaultSettings;

		[Serializable]
		public struct DepthSettings
		{
			public static BuiltinDebugViewsModel.DepthSettings defaultSettings
			{
				get
				{
					return new BuiltinDebugViewsModel.DepthSettings
					{
						scale = 1f
					};
				}
			}

			[Tooltip("Scales the camera far plane before displaying the depth map.")]
			[Range(0f, 1f)]
			public float scale;
		}

		[Serializable]
		public struct MotionVectorsSettings
		{
			public static BuiltinDebugViewsModel.MotionVectorsSettings defaultSettings
			{
				get
				{
					return new BuiltinDebugViewsModel.MotionVectorsSettings
					{
						sourceOpacity = 1f,
						motionImageOpacity = 0f,
						motionImageAmplitude = 16f,
						motionVectorsOpacity = 1f,
						motionVectorsResolution = 24,
						motionVectorsAmplitude = 64f
					};
				}
			}

			[Tooltip("Opacity of the source render.")]
			[Range(0f, 1f)]
			public float sourceOpacity;

			[Tooltip("Opacity of the per-pixel motion vector colors.")]
			[Range(0f, 1f)]
			public float motionImageOpacity;

			[Tooltip("Because motion vectors are mainly very small vectors, you can use this setting to make them more visible.")]
			[Min(0f)]
			public float motionImageAmplitude;

			[Tooltip("Opacity for the motion vector arrows.")]
			[Range(0f, 1f)]
			public float motionVectorsOpacity;

			[Tooltip("The arrow density on screen.")]
			[Range(8f, 64f)]
			public int motionVectorsResolution;

			[Tooltip("Tweaks the arrows length.")]
			[Min(0f)]
			public float motionVectorsAmplitude;
		}

		public enum Mode
		{
			None,
			Depth,
			Normals,
			MotionVectors,
			AmbientOcclusion,
			EyeAdaptation,
			FocusPlane,
			PreGradingLog,
			LogLut,
			UserLut
		}

		[Serializable]
		public struct Settings
		{
			public static BuiltinDebugViewsModel.Settings defaultSettings
			{
				get
				{
					return new BuiltinDebugViewsModel.Settings
					{
						mode = BuiltinDebugViewsModel.Mode.None,
						depth = BuiltinDebugViewsModel.DepthSettings.defaultSettings,
						motionVectors = BuiltinDebugViewsModel.MotionVectorsSettings.defaultSettings
					};
				}
			}

			public BuiltinDebugViewsModel.Mode mode;

			public BuiltinDebugViewsModel.DepthSettings depth;

			public BuiltinDebugViewsModel.MotionVectorsSettings motionVectors;
		}
	}
}
