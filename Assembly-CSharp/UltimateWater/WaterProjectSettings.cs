using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WaterProjectSettings : ScriptableObjectSingleton
	{
		public static WaterProjectSettings Instance
		{
			get
			{
				if (WaterProjectSettings._Instance == null)
				{
					WaterProjectSettings._Instance = ScriptableObjectSingleton.LoadSingleton<WaterProjectSettings>();
				}
				return WaterProjectSettings._Instance;
			}
		}

		public bool ClipWaterCameraRange
		{
			get
			{
				return this._ClipWaterCameraRange;
			}
		}

		public float CameraClipRange
		{
			get
			{
				return this._CameraClipRange;
			}
		}

		public int PhysicsThreads
		{
			get
			{
				return this._PhysicsThreads;
			}
			set
			{
				this._PhysicsThreads = value;
			}
		}

		public int WaterLayer
		{
			get
			{
				return this._WaterLayer;
			}
		}

		public int WaterTempLayer
		{
			get
			{
				return this._WaterTempLayer;
			}
		}

		public int WaterCollidersLayer
		{
			get
			{
				return this._WaterCollidersLayer;
			}
		}

		public System.Threading.ThreadPriority PhysicsThreadsPriority
		{
			get
			{
				return this._PhysicsThreadsPriority;
			}
		}

		public bool AllowCpuFFT
		{
			get
			{
				return this._AllowCpuFFT;
			}
		}

		public bool AllowFloatingPointMipMaps
		{
			get
			{
				if (WaterProjectSettings._AllowFloatingPointMipMapsChecked)
				{
					return WaterProjectSettings._AllowFloatingPointMipMaps && this._AllowFloatingPointMipMapsOverride;
				}
				WaterProjectSettings._AllowFloatingPointMipMapsChecked = true;
				string text = SystemInfo.graphicsDeviceVendor.ToLowerInvariant();
				WaterProjectSettings._AllowFloatingPointMipMaps = (!text.Contains("amd") && !text.Contains("ati") && !SystemInfo.graphicsDeviceName.ToLowerInvariant().Contains("radeon"));
				return WaterProjectSettings._AllowFloatingPointMipMaps && this._AllowFloatingPointMipMapsOverride;
			}
		}

		public WaterProjectSettings.AbsorptionEditMode InspectorAbsorptionEditMode
		{
			get
			{
				return this._AbsorptionEditMode;
			}
		}

		public WaterProjectSettings.SpecularEditMode InspectorSpecularEditMode
		{
			get
			{
				return this._SpecularEditMode;
			}
		}

		public bool DebugPhysics
		{
			get
			{
				return this._DebugPhysics;
			}
		}

		public bool AskForWaterCameras
		{
			get
			{
				return this._AskForWaterCameras;
			}
			set
			{
				this._AskForWaterCameras = value;
			}
		}

		public bool SinglePassStereoRendering
		{
			get
			{
				return this._SinglePassStereoRendering;
			}
		}

		public bool RenderInSceneView
		{
			get
			{
				return this._RenderInSceneView;
			}
			set
			{
				this._RenderInSceneView = value;
			}
		}

		public static readonly float CurrentVersion = 2.1f;

		public static readonly string CurrentVersionString = "2.1.0";

		[SerializeField]
		[FormerlySerializedAs("serializedVersion")]
		private float _SerializedVersion = 1f;

		[SerializeField]
		[FormerlySerializedAs("waterLayer")]
		private int _WaterLayer = 4;

		[FormerlySerializedAs("waterTempLayer")]
		[SerializeField]
		[Tooltip("Used for some camera effects. Has to be unused. You don't need to mask it on your cameras.")]
		private int _WaterTempLayer = 22;

		[Tooltip("UltimateWater internally uses colliders to detect camera entering into subtractive volumes etc. You will have to ignore this layer in your scripting raycasts.")]
		[SerializeField]
		[FormerlySerializedAs("waterCollidersLayer")]
		private int _WaterCollidersLayer = 1;

		[Tooltip("More threads increase physics precision under stress, but also decrease overall performance a bit.")]
		[SerializeField]
		[FormerlySerializedAs("physicsThreads")]
		private int _PhysicsThreads = 1;

		[FormerlySerializedAs("physicsThreadsPriority")]
		[SerializeField]
		private System.Threading.ThreadPriority _PhysicsThreadsPriority = System.Threading.ThreadPriority.BelowNormal;

		[SerializeField]
		[FormerlySerializedAs("allowCpuFFT")]
		private bool _AllowCpuFFT = true;

		[Tooltip("Some hardware doesn't support floating point mip maps correctly and they are forcefully disabled. You may simulate how the water would look like on such hardware by disabling this option. Most notably fp mip maps don't work correctly on most AMD graphic cards (for now).")]
		[SerializeField]
		[FormerlySerializedAs("allowFloatingPointMipMaps")]
		private bool _AllowFloatingPointMipMapsOverride = true;

		[FormerlySerializedAs("debugPhysics")]
		[SerializeField]
		private bool _DebugPhysics;

		[SerializeField]
		[FormerlySerializedAs("askForWaterCameras")]
		private bool _AskForWaterCameras = true;

		[SerializeField]
		[FormerlySerializedAs("absorptionEditMode")]
		private WaterProjectSettings.AbsorptionEditMode _AbsorptionEditMode = WaterProjectSettings.AbsorptionEditMode.Transmission;

		[SerializeField]
		[FormerlySerializedAs("specularEditMode")]
		private WaterProjectSettings.SpecularEditMode _SpecularEditMode;

		[SerializeField]
		private bool _SinglePassStereoRendering;

		[SerializeField]
		private bool _DisableMultisampling = true;

		[SerializeField]
		private bool _ClipWaterCameraRange;

		[SerializeField]
		private float _CameraClipRange = 1000f;

		[SerializeField]
		private bool _RenderInSceneView;

		private static WaterProjectSettings _Instance;

		private static bool _AllowFloatingPointMipMaps;

		private static bool _AllowFloatingPointMipMapsChecked;

		public enum AbsorptionEditMode
		{
			Absorption,
			Transmission
		}

		public enum SpecularEditMode
		{
			IndexOfRefraction,
			CustomColor
		}
	}
}
