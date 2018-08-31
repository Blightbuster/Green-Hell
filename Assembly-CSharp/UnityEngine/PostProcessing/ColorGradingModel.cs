﻿using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class ColorGradingModel : PostProcessingModel
	{
		public ColorGradingModel.Settings settings
		{
			get
			{
				return this.m_Settings;
			}
			set
			{
				this.m_Settings = value;
				this.OnValidate();
			}
		}

		public bool isDirty { get; internal set; }

		public RenderTexture bakedLut { get; internal set; }

		public override void Reset()
		{
			this.m_Settings = ColorGradingModel.Settings.defaultSettings;
			this.OnValidate();
		}

		public override void OnValidate()
		{
			this.isDirty = true;
		}

		[SerializeField]
		private ColorGradingModel.Settings m_Settings = ColorGradingModel.Settings.defaultSettings;

		public enum Tonemapper
		{
			None,
			ACES,
			Neutral
		}

		[Serializable]
		public struct TonemappingSettings
		{
			public static ColorGradingModel.TonemappingSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.TonemappingSettings
					{
						tonemapper = ColorGradingModel.Tonemapper.Neutral,
						neutralBlackIn = 0.02f,
						neutralWhiteIn = 10f,
						neutralBlackOut = 0f,
						neutralWhiteOut = 10f,
						neutralWhiteLevel = 5.3f,
						neutralWhiteClip = 10f
					};
				}
			}

			[Tooltip("Tonemapping algorithm to use at the end of the color grading process. Use \"Neutral\" if you need a customizable tonemapper or \"Filmic\" to give a standard filmic look to your scenes.")]
			public ColorGradingModel.Tonemapper tonemapper;

			[Range(-0.1f, 0.1f)]
			public float neutralBlackIn;

			[Range(1f, 20f)]
			public float neutralWhiteIn;

			[Range(-0.09f, 0.1f)]
			public float neutralBlackOut;

			[Range(1f, 19f)]
			public float neutralWhiteOut;

			[Range(0.1f, 20f)]
			public float neutralWhiteLevel;

			[Range(1f, 10f)]
			public float neutralWhiteClip;
		}

		[Serializable]
		public struct BasicSettings
		{
			public static ColorGradingModel.BasicSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.BasicSettings
					{
						postExposure = 0f,
						temperature = 0f,
						tint = 0f,
						hueShift = 0f,
						saturation = 1f,
						contrast = 1f
					};
				}
			}

			[Tooltip("Adjusts the overall exposure of the scene in EV units. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
			public float postExposure;

			[Range(-100f, 100f)]
			[Tooltip("Sets the white balance to a custom color temperature.")]
			public float temperature;

			[Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
			[Range(-100f, 100f)]
			public float tint;

			[Tooltip("Shift the hue of all colors.")]
			[Range(-180f, 180f)]
			public float hueShift;

			[Range(0f, 2f)]
			[Tooltip("Pushes the intensity of all colors.")]
			public float saturation;

			[Tooltip("Expands or shrinks the overall range of tonal values.")]
			[Range(0f, 2f)]
			public float contrast;
		}

		[Serializable]
		public struct ChannelMixerSettings
		{
			public static ColorGradingModel.ChannelMixerSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.ChannelMixerSettings
					{
						red = new Vector3(1f, 0f, 0f),
						green = new Vector3(0f, 1f, 0f),
						blue = new Vector3(0f, 0f, 1f),
						currentEditingChannel = 0
					};
				}
			}

			public Vector3 red;

			public Vector3 green;

			public Vector3 blue;

			[HideInInspector]
			public int currentEditingChannel;
		}

		[Serializable]
		public struct LogWheelsSettings
		{
			public static ColorGradingModel.LogWheelsSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.LogWheelsSettings
					{
						slope = Color.clear,
						power = Color.clear,
						offset = Color.clear
					};
				}
			}

			[Trackball("GetSlopeValue")]
			public Color slope;

			[Trackball("GetPowerValue")]
			public Color power;

			[Trackball("GetOffsetValue")]
			public Color offset;
		}

		[Serializable]
		public struct LinearWheelsSettings
		{
			public static ColorGradingModel.LinearWheelsSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.LinearWheelsSettings
					{
						lift = Color.clear,
						gamma = Color.clear,
						gain = Color.clear
					};
				}
			}

			[Trackball("GetLiftValue")]
			public Color lift;

			[Trackball("GetGammaValue")]
			public Color gamma;

			[Trackball("GetGainValue")]
			public Color gain;
		}

		public enum ColorWheelMode
		{
			Linear,
			Log
		}

		[Serializable]
		public struct ColorWheelsSettings
		{
			public static ColorGradingModel.ColorWheelsSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.ColorWheelsSettings
					{
						mode = ColorGradingModel.ColorWheelMode.Log,
						log = ColorGradingModel.LogWheelsSettings.defaultSettings,
						linear = ColorGradingModel.LinearWheelsSettings.defaultSettings
					};
				}
			}

			public ColorGradingModel.ColorWheelMode mode;

			[TrackballGroup]
			public ColorGradingModel.LogWheelsSettings log;

			[TrackballGroup]
			public ColorGradingModel.LinearWheelsSettings linear;
		}

		[Serializable]
		public struct CurvesSettings
		{
			public static ColorGradingModel.CurvesSettings defaultSettings
			{
				get
				{
					return new ColorGradingModel.CurvesSettings
					{
						master = new ColorGradingCurve(new AnimationCurve(new Keyframe[]
						{
							new Keyframe(0f, 0f, 1f, 1f),
							new Keyframe(1f, 1f, 1f, 1f)
						}), 0f, false, new Vector2(0f, 1f)),
						red = new ColorGradingCurve(new AnimationCurve(new Keyframe[]
						{
							new Keyframe(0f, 0f, 1f, 1f),
							new Keyframe(1f, 1f, 1f, 1f)
						}), 0f, false, new Vector2(0f, 1f)),
						green = new ColorGradingCurve(new AnimationCurve(new Keyframe[]
						{
							new Keyframe(0f, 0f, 1f, 1f),
							new Keyframe(1f, 1f, 1f, 1f)
						}), 0f, false, new Vector2(0f, 1f)),
						blue = new ColorGradingCurve(new AnimationCurve(new Keyframe[]
						{
							new Keyframe(0f, 0f, 1f, 1f),
							new Keyframe(1f, 1f, 1f, 1f)
						}), 0f, false, new Vector2(0f, 1f)),
						hueVShue = new ColorGradingCurve(new AnimationCurve(), 0.5f, true, new Vector2(0f, 1f)),
						hueVSsat = new ColorGradingCurve(new AnimationCurve(), 0.5f, true, new Vector2(0f, 1f)),
						satVSsat = new ColorGradingCurve(new AnimationCurve(), 0.5f, false, new Vector2(0f, 1f)),
						lumVSsat = new ColorGradingCurve(new AnimationCurve(), 0.5f, false, new Vector2(0f, 1f)),
						e_CurrentEditingCurve = 0,
						e_CurveY = true,
						e_CurveR = false,
						e_CurveG = false,
						e_CurveB = false
					};
				}
			}

			public ColorGradingCurve master;

			public ColorGradingCurve red;

			public ColorGradingCurve green;

			public ColorGradingCurve blue;

			public ColorGradingCurve hueVShue;

			public ColorGradingCurve hueVSsat;

			public ColorGradingCurve satVSsat;

			public ColorGradingCurve lumVSsat;

			[HideInInspector]
			public int e_CurrentEditingCurve;

			[HideInInspector]
			public bool e_CurveY;

			[HideInInspector]
			public bool e_CurveR;

			[HideInInspector]
			public bool e_CurveG;

			[HideInInspector]
			public bool e_CurveB;
		}

		[Serializable]
		public struct Settings
		{
			public static ColorGradingModel.Settings defaultSettings
			{
				get
				{
					return new ColorGradingModel.Settings
					{
						tonemapping = ColorGradingModel.TonemappingSettings.defaultSettings,
						basic = ColorGradingModel.BasicSettings.defaultSettings,
						channelMixer = ColorGradingModel.ChannelMixerSettings.defaultSettings,
						colorWheels = ColorGradingModel.ColorWheelsSettings.defaultSettings,
						curves = ColorGradingModel.CurvesSettings.defaultSettings
					};
				}
			}

			public ColorGradingModel.TonemappingSettings tonemapping;

			public ColorGradingModel.BasicSettings basic;

			public ColorGradingModel.ChannelMixerSettings channelMixer;

			public ColorGradingModel.ColorWheelsSettings colorWheels;

			public ColorGradingModel.CurvesSettings curves;
		}
	}
}
