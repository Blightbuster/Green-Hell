using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace VolumetricFogAndMist
{
	[ExecuteInEditMode]
	[HelpURL("http://kronnect.com/taptapgo")]
	[AddComponentMenu("Image Effects/Rendering/Volumetric Fog & Mist")]
	public class VolumetricFog : MonoBehaviour
	{
		public static VolumetricFog instance
		{
			get
			{
				if (VolumetricFog._fog == null)
				{
					if (Camera.main != null)
					{
						VolumetricFog._fog = Camera.main.GetComponent<VolumetricFog>();
					}
					if (VolumetricFog._fog == null)
					{
						foreach (Camera camera in Camera.allCameras)
						{
							VolumetricFog._fog = camera.GetComponent<VolumetricFog>();
							if (VolumetricFog._fog != null)
							{
								break;
							}
						}
					}
				}
				return VolumetricFog._fog;
			}
		}

		public FOG_PRESET preset
		{
			get
			{
				return this._preset;
			}
			set
			{
				if (value != this._preset)
				{
					this._preset = value;
					this.UpdatePreset();
					this.isDirty = true;
				}
			}
		}

		public VolumetricFogProfile profile
		{
			get
			{
				return this._profile;
			}
			set
			{
				if (value != this._profile)
				{
					this._profile = value;
					if (this._profile != null)
					{
						this._profile.Load(this);
						this._preset = FOG_PRESET.Custom;
					}
					this.isDirty = true;
				}
			}
		}

		public bool useFogVolumes
		{
			get
			{
				return this._useFogVolumes;
			}
			set
			{
				if (value != this._useFogVolumes)
				{
					this._useFogVolumes = value;
					this.isDirty = true;
				}
			}
		}

		public bool debugDepthPass
		{
			get
			{
				return this._debugPass;
			}
			set
			{
				if (value != this._debugPass)
				{
					this._debugPass = value;
					this.isDirty = true;
				}
			}
		}

		public TRANSPARENT_MODE transparencyBlendMode
		{
			get
			{
				return this._transparencyBlendMode;
			}
			set
			{
				if (value != this._transparencyBlendMode)
				{
					this._transparencyBlendMode = value;
					this.UpdateMaterialProperties();
					this.UpdateRenderComponents();
					this.isDirty = true;
				}
			}
		}

		public float transparencyBlendPower
		{
			get
			{
				return this._transparencyBlendPower;
			}
			set
			{
				if (value != this._transparencyBlendPower)
				{
					this._transparencyBlendPower = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public LayerMask transparencyLayerMask
		{
			get
			{
				return this._transparencyLayerMask;
			}
			set
			{
				if (this._transparencyLayerMask != value)
				{
					this._transparencyLayerMask = value;
					this.isDirty = true;
				}
			}
		}

		public LIGHTING_MODEL lightingModel
		{
			get
			{
				return this._lightingModel;
			}
			set
			{
				if (value != this._lightingModel)
				{
					this._lightingModel = value;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public bool computeDepth
		{
			get
			{
				return this._computeDepth;
			}
			set
			{
				if (value != this._computeDepth)
				{
					this._computeDepth = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public COMPUTE_DEPTH_SCOPE computeDepthScope
		{
			get
			{
				return this._computeDepthScope;
			}
			set
			{
				if (value != this._computeDepthScope)
				{
					this._computeDepthScope = value;
					if (this._computeDepthScope == COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)
					{
						this._transparencyBlendMode = TRANSPARENT_MODE.None;
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float transparencyCutOff
		{
			get
			{
				return this._transparencyCutOff;
			}
			set
			{
				if (value != this._transparencyCutOff)
				{
					this._transparencyCutOff = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool renderBeforeTransparent
		{
			get
			{
				return this._renderBeforeTransparent;
			}
			set
			{
				if (value != this._renderBeforeTransparent)
				{
					this._renderBeforeTransparent = value;
					if (this._renderBeforeTransparent)
					{
						this._transparencyBlendMode = TRANSPARENT_MODE.None;
					}
					this.UpdateMaterialProperties();
					this.UpdateRenderComponents();
					this.isDirty = true;
				}
			}
		}

		public GameObject sun
		{
			get
			{
				return this._sun;
			}
			set
			{
				if (value != this._sun)
				{
					this._sun = value;
					this.UpdateSun();
				}
			}
		}

		public bool sunCopyColor
		{
			get
			{
				return this._sunCopyColor;
			}
			set
			{
				if (value != this._sunCopyColor)
				{
					this._sunCopyColor = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float density
		{
			get
			{
				return this._density;
			}
			set
			{
				if (value != this._density)
				{
					this._preset = FOG_PRESET.Custom;
					this._density = value;
					this.UpdateMaterialProperties();
					this.UpdateTextureAlpha();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float noiseStrength
		{
			get
			{
				return this._noiseStrength;
			}
			set
			{
				if (value != this._noiseStrength)
				{
					this._preset = FOG_PRESET.Custom;
					this._noiseStrength = value;
					this.UpdateMaterialProperties();
					this.UpdateTextureAlpha();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float noiseFinalMultiplier
		{
			get
			{
				return this._noiseFinalMultiplier;
			}
			set
			{
				if (value != this._noiseFinalMultiplier)
				{
					this._preset = FOG_PRESET.Custom;
					this._noiseFinalMultiplier = value;
					this.UpdateMaterialProperties();
					this.UpdateTextureAlpha();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float noiseSparse
		{
			get
			{
				return this._noiseSparse;
			}
			set
			{
				if (value != this._noiseSparse)
				{
					this._preset = FOG_PRESET.Custom;
					this._noiseSparse = value;
					this.UpdateMaterialProperties();
					this.UpdateTextureAlpha();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float distance
		{
			get
			{
				return this._distance;
			}
			set
			{
				if (value != this._distance)
				{
					this._preset = FOG_PRESET.Custom;
					this._distance = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float maxFogLength
		{
			get
			{
				return this._maxFogLength;
			}
			set
			{
				if (value != this._maxFogLength)
				{
					this._maxFogLength = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float maxFogLengthFallOff
		{
			get
			{
				return this._maxFogLengthFallOff;
			}
			set
			{
				if (value != this._maxFogLengthFallOff)
				{
					this._maxFogLengthFallOff = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float distanceFallOff
		{
			get
			{
				return this._distanceFallOff;
			}
			set
			{
				if (value != this._distanceFallOff)
				{
					this._preset = FOG_PRESET.Custom;
					this._distanceFallOff = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float height
		{
			get
			{
				return this._height;
			}
			set
			{
				if (value != this._height)
				{
					this._preset = FOG_PRESET.Custom;
					this._height = Mathf.Max(value, 1E-05f);
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float baselineHeight
		{
			get
			{
				return this._baselineHeight;
			}
			set
			{
				if (value != this._baselineHeight)
				{
					this._preset = FOG_PRESET.Custom;
					this._baselineHeight = value;
					if (this._fogAreaRadius > 0f)
					{
						this._fogAreaPosition.y = this._baselineHeight;
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool baselineRelativeToCamera
		{
			get
			{
				return this._baselineRelativeToCamera;
			}
			set
			{
				if (value != this._baselineRelativeToCamera)
				{
					this._preset = FOG_PRESET.Custom;
					this._baselineRelativeToCamera = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float baselineRelativeToCameraDelay
		{
			get
			{
				return this._baselineRelativeToCameraDelay;
			}
			set
			{
				if (value != this._baselineRelativeToCameraDelay)
				{
					this._baselineRelativeToCameraDelay = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float noiseScale
		{
			get
			{
				return this._noiseScale;
			}
			set
			{
				if (value != this._noiseScale)
				{
					this._preset = FOG_PRESET.Custom;
					this._noiseScale = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float alpha
		{
			get
			{
				return this._alpha;
			}
			set
			{
				if (value != this._alpha)
				{
					this._preset = FOG_PRESET.Custom;
					this._alpha = value;
					this.currentFogAlpha = this._alpha;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Color color
		{
			get
			{
				return this._color;
			}
			set
			{
				if (value != this._color)
				{
					this._preset = FOG_PRESET.Custom;
					this._color = value;
					this.currentFogColor = this._color;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Color specularColor
		{
			get
			{
				return this._specularColor;
			}
			set
			{
				if (value != this._specularColor)
				{
					this._preset = FOG_PRESET.Custom;
					this._specularColor = value;
					this.currentFogSpecularColor = this._specularColor;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float specularThreshold
		{
			get
			{
				return this._specularThreshold;
			}
			set
			{
				if (value != this._specularThreshold)
				{
					this._preset = FOG_PRESET.Custom;
					this._specularThreshold = value;
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float specularIntensity
		{
			get
			{
				return this._specularIntensity;
			}
			set
			{
				if (value != this._specularIntensity)
				{
					this._preset = FOG_PRESET.Custom;
					this._specularIntensity = value;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public Vector3 lightDirection
		{
			get
			{
				return this._lightDirection;
			}
			set
			{
				if (value != this._lightDirection)
				{
					this._preset = FOG_PRESET.Custom;
					this._lightDirection = value;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public float lightIntensity
		{
			get
			{
				return this._lightIntensity;
			}
			set
			{
				if (value != this._lightIntensity)
				{
					this._preset = FOG_PRESET.Custom;
					this._lightIntensity = value;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public Color lightColor
		{
			get
			{
				return this._lightColor;
			}
			set
			{
				if (value != this._lightColor)
				{
					this._preset = FOG_PRESET.Custom;
					this._lightColor = value;
					this.currentLightColor = this._lightColor;
					this.UpdateMaterialProperties();
					this.UpdateTexture();
					this.isDirty = true;
				}
			}
		}

		public int updateTextureSpread
		{
			get
			{
				return this._updateTextureSpread;
			}
			set
			{
				if (value != this._updateTextureSpread)
				{
					this._updateTextureSpread = value;
					this.isDirty = true;
				}
			}
		}

		public float speed
		{
			get
			{
				return this._speed;
			}
			set
			{
				if (value != this._speed)
				{
					this._preset = FOG_PRESET.Custom;
					this._speed = value;
					if (!Application.isPlaying)
					{
						this.UpdateWindSpeedQuick();
					}
					this.isDirty = true;
				}
			}
		}

		public Vector3 windDirection
		{
			get
			{
				return this._windDirection;
			}
			set
			{
				if (value != this._windDirection)
				{
					this._preset = FOG_PRESET.Custom;
					this._windDirection = value.normalized;
					if (!Application.isPlaying)
					{
						this.UpdateWindSpeedQuick();
					}
					this.isDirty = true;
				}
			}
		}

		public bool useRealTime
		{
			get
			{
				return this._useRealTime;
			}
			set
			{
				if (value != this._useRealTime)
				{
					this._useRealTime = value;
					this.isDirty = true;
				}
			}
		}

		public Color skyColor
		{
			get
			{
				return this._skyColor;
			}
			set
			{
				if (value != this._skyColor)
				{
					this._preset = FOG_PRESET.Custom;
					this._skyColor = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float skyHaze
		{
			get
			{
				return this._skyHaze;
			}
			set
			{
				if (value != this._skyHaze)
				{
					this._preset = FOG_PRESET.Custom;
					this._skyHaze = value;
					if (!Application.isPlaying)
					{
						this.UpdateWindSpeedQuick();
					}
					this.isDirty = true;
				}
			}
		}

		public float skySpeed
		{
			get
			{
				return this._skySpeed;
			}
			set
			{
				if (value != this._skySpeed)
				{
					this._preset = FOG_PRESET.Custom;
					this._skySpeed = value;
					this.isDirty = true;
				}
			}
		}

		public float skyNoiseStrength
		{
			get
			{
				return this._skyNoiseStrength;
			}
			set
			{
				if (value != this._skyNoiseStrength)
				{
					this._preset = FOG_PRESET.Custom;
					this._skyNoiseStrength = value;
					if (!Application.isPlaying)
					{
						this.UpdateWindSpeedQuick();
					}
					this.isDirty = true;
				}
			}
		}

		public float skyAlpha
		{
			get
			{
				return this._skyAlpha;
			}
			set
			{
				if (value != this._skyAlpha)
				{
					this._preset = FOG_PRESET.Custom;
					this._skyAlpha = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float skyDepth
		{
			get
			{
				return this._skyDepth;
			}
			set
			{
				if (value != this._skyDepth)
				{
					this._skyDepth = value;
					if (!Application.isPlaying)
					{
						this.UpdateWindSpeedQuick();
					}
					this.isDirty = true;
				}
			}
		}

		public GameObject character
		{
			get
			{
				return this._character;
			}
			set
			{
				if (value != this._character)
				{
					this._character = value;
					this.isDirty = true;
				}
			}
		}

		public FOG_VOID_TOPOLOGY fogVoidTopology
		{
			get
			{
				return this._fogVoidTopology;
			}
			set
			{
				if (value != this._fogVoidTopology)
				{
					this._fogVoidTopology = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogVoidFallOff
		{
			get
			{
				return this._fogVoidFallOff;
			}
			set
			{
				if (value != this._fogVoidFallOff)
				{
					this._preset = FOG_PRESET.Custom;
					this._fogVoidFallOff = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogVoidRadius
		{
			get
			{
				return this._fogVoidRadius;
			}
			set
			{
				if (value != this._fogVoidRadius)
				{
					this._preset = FOG_PRESET.Custom;
					this._fogVoidRadius = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Vector3 fogVoidPosition
		{
			get
			{
				return this._fogVoidPosition;
			}
			set
			{
				if (value != this._fogVoidPosition)
				{
					this._preset = FOG_PRESET.Custom;
					this._fogVoidPosition = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogVoidDepth
		{
			get
			{
				return this._fogVoidDepth;
			}
			set
			{
				if (value != this._fogVoidDepth)
				{
					this._preset = FOG_PRESET.Custom;
					this._fogVoidDepth = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogVoidHeight
		{
			get
			{
				return this._fogVoidHeight;
			}
			set
			{
				if (value != this._fogVoidHeight)
				{
					this._preset = FOG_PRESET.Custom;
					this._fogVoidHeight = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		[Obsolete("Fog Void inverted is now deprecated. Use Fog Area settings.")]
		public bool fogVoidInverted
		{
			get
			{
				return this._fogVoidInverted;
			}
			set
			{
				this._fogVoidInverted = value;
			}
		}

		public GameObject fogAreaCenter
		{
			get
			{
				return this._fogAreaCenter;
			}
			set
			{
				if (value != this._character)
				{
					this._fogAreaCenter = value;
					this.isDirty = true;
				}
			}
		}

		public float fogAreaFallOff
		{
			get
			{
				return this._fogAreaFallOff;
			}
			set
			{
				if (value != this._fogAreaFallOff)
				{
					this._fogAreaFallOff = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public FOG_AREA_FOLLOW_MODE fogAreaFollowMode
		{
			get
			{
				return this._fogAreaFollowMode;
			}
			set
			{
				if (value != this._fogAreaFollowMode)
				{
					this._fogAreaFollowMode = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public FOG_AREA_TOPOLOGY fogAreaTopology
		{
			get
			{
				return this._fogAreaTopology;
			}
			set
			{
				if (value != this._fogAreaTopology)
				{
					this._fogAreaTopology = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogAreaRadius
		{
			get
			{
				return this._fogAreaRadius;
			}
			set
			{
				if (value != this._fogAreaRadius)
				{
					this._fogAreaRadius = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Vector3 fogAreaPosition
		{
			get
			{
				return this._fogAreaPosition;
			}
			set
			{
				if (value != this._fogAreaPosition)
				{
					this._fogAreaPosition = value;
					if (this._fogAreaCenter == null || this._fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.RestrictToXZPlane)
					{
						this._baselineHeight = this._fogAreaPosition.y;
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogAreaDepth
		{
			get
			{
				return this._fogAreaDepth;
			}
			set
			{
				if (value != this._fogAreaDepth)
				{
					this._fogAreaDepth = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogAreaHeight
		{
			get
			{
				return this._fogAreaHeight;
			}
			set
			{
				if (value != this._fogAreaHeight)
				{
					this._fogAreaHeight = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public FOG_AREA_SORTING_MODE fogAreaSortingMode
		{
			get
			{
				return this._fogAreaSortingMode;
			}
			set
			{
				if (value != this._fogAreaSortingMode)
				{
					this._fogAreaSortingMode = value;
					this.lastTimeSortInstances = 0f;
					this.isDirty = true;
				}
			}
		}

		public int fogAreaRenderOrder
		{
			get
			{
				return this._fogAreaRenderOrder;
			}
			set
			{
				if (value != this._fogAreaRenderOrder)
				{
					this._fogAreaRenderOrder = value;
					this.lastTimeSortInstances = 0f;
					this.isDirty = true;
				}
			}
		}

		public bool pointLightTrackAuto
		{
			get
			{
				return this._pointLightTrackingAuto;
			}
			set
			{
				if (value != this._pointLightTrackingAuto)
				{
					this._pointLightTrackingAuto = value;
					this.TrackPointLights();
					this.isDirty = true;
				}
			}
		}

		public int pointLightTrackingCount
		{
			get
			{
				return this._pointLightTrackingCount;
			}
			set
			{
				if (value != this._pointLightTrackingCount)
				{
					this._pointLightTrackingCount = Mathf.Clamp(value, 0, 6);
					this.TrackPointLights();
					this.isDirty = true;
				}
			}
		}

		public float pointLightTrackingCheckInterval
		{
			get
			{
				return this._pointLightTrackingCheckInterval;
			}
			set
			{
				if (value != this._pointLightTrackingCheckInterval)
				{
					this._pointLightTrackingCheckInterval = value;
					this.TrackPointLights();
					this.isDirty = true;
				}
			}
		}

		public int downsampling
		{
			get
			{
				return this._downsampling;
			}
			set
			{
				if (value != this._downsampling)
				{
					this._preset = FOG_PRESET.Custom;
					this._downsampling = value;
					this.isDirty = true;
				}
			}
		}

		public bool edgeImprove
		{
			get
			{
				return this._edgeImprove;
			}
			set
			{
				if (value != this._edgeImprove)
				{
					this._preset = FOG_PRESET.Custom;
					this._edgeImprove = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float edgeThreshold
		{
			get
			{
				return this._edgeThreshold;
			}
			set
			{
				if (value != this._edgeThreshold)
				{
					this._preset = FOG_PRESET.Custom;
					this._edgeThreshold = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float stepping
		{
			get
			{
				return this._stepping;
			}
			set
			{
				if (value != this._stepping)
				{
					this._preset = FOG_PRESET.Custom;
					this._stepping = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float steppingNear
		{
			get
			{
				return this._steppingNear;
			}
			set
			{
				if (value != this._steppingNear)
				{
					this._preset = FOG_PRESET.Custom;
					this._steppingNear = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool dithering
		{
			get
			{
				return this._dithering;
			}
			set
			{
				if (value != this._dithering)
				{
					this._dithering = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float ditherStrength
		{
			get
			{
				return this._ditherStrength;
			}
			set
			{
				if (value != this._ditherStrength)
				{
					this._ditherStrength = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float jitterStrength
		{
			get
			{
				return this._jitterStrength;
			}
			set
			{
				if (value != this._jitterStrength)
				{
					this._jitterStrength = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool lightScatteringEnabled
		{
			get
			{
				return this._lightScatteringEnabled;
			}
			set
			{
				if (value != this._lightScatteringEnabled)
				{
					this._lightScatteringEnabled = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringDiffusion
		{
			get
			{
				return this._lightScatteringDiffusion;
			}
			set
			{
				if (value != this._lightScatteringDiffusion)
				{
					this._lightScatteringDiffusion = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringSpread
		{
			get
			{
				return this._lightScatteringSpread;
			}
			set
			{
				if (value != this._lightScatteringSpread)
				{
					this._lightScatteringSpread = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public int lightScatteringSamples
		{
			get
			{
				return this._lightScatteringSamples;
			}
			set
			{
				if (value != this._lightScatteringSamples)
				{
					this._lightScatteringSamples = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringWeight
		{
			get
			{
				return this._lightScatteringWeight;
			}
			set
			{
				if (value != this._lightScatteringWeight)
				{
					this._lightScatteringWeight = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringIllumination
		{
			get
			{
				return this._lightScatteringIllumination;
			}
			set
			{
				if (value != this._lightScatteringIllumination)
				{
					this._lightScatteringIllumination = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringDecay
		{
			get
			{
				return this._lightScatteringDecay;
			}
			set
			{
				if (value != this._lightScatteringDecay)
				{
					this._lightScatteringDecay = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringExposure
		{
			get
			{
				return this._lightScatteringExposure;
			}
			set
			{
				if (value != this._lightScatteringExposure)
				{
					this._lightScatteringExposure = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float lightScatteringJittering
		{
			get
			{
				return this._lightScatteringJittering;
			}
			set
			{
				if (value != this._lightScatteringJittering)
				{
					this._lightScatteringJittering = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool fogBlur
		{
			get
			{
				return this._fogBlur;
			}
			set
			{
				if (value != this._fogBlur)
				{
					this._fogBlur = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogBlurDepth
		{
			get
			{
				return this._fogBlurDepth;
			}
			set
			{
				if (value != this._fogBlurDepth)
				{
					this._fogBlurDepth = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool sunShadows
		{
			get
			{
				return this._sunShadows;
			}
			set
			{
				if (value != this._sunShadows)
				{
					this._sunShadows = value;
					this.CleanUpTextureDepthSun();
					if (this._sunShadows)
					{
						this.needUpdateDepthSunTexture = true;
					}
					else
					{
						this.DestroySunShadowsDependencies();
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public LayerMask sunShadowsLayerMask
		{
			get
			{
				return this._sunShadowsLayerMask;
			}
			set
			{
				if (this._sunShadowsLayerMask != value)
				{
					this._sunShadowsLayerMask = value;
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsStrength
		{
			get
			{
				return this._sunShadowsStrength;
			}
			set
			{
				if (value != this._sunShadowsStrength)
				{
					this._sunShadowsStrength = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsBias
		{
			get
			{
				return this._sunShadowsBias;
			}
			set
			{
				if (value != this._sunShadowsBias)
				{
					this._sunShadowsBias = value;
					this.needUpdateDepthSunTexture = true;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsJitterStrength
		{
			get
			{
				return this._sunShadowsJitterStrength;
			}
			set
			{
				if (value != this._sunShadowsJitterStrength)
				{
					this._sunShadowsJitterStrength = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public int sunShadowsResolution
		{
			get
			{
				return this._sunShadowsResolution;
			}
			set
			{
				if (value != this._sunShadowsResolution)
				{
					this._sunShadowsResolution = value;
					this.needUpdateDepthSunTexture = true;
					this.CleanUpTextureDepthSun();
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsMaxDistance
		{
			get
			{
				return this._sunShadowsMaxDistance;
			}
			set
			{
				if (value != this._sunShadowsMaxDistance)
				{
					this._sunShadowsMaxDistance = value;
					this.needUpdateDepthSunTexture = true;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public SUN_SHADOWS_BAKE_MODE sunShadowsBakeMode
		{
			get
			{
				return this._sunShadowsBakeMode;
			}
			set
			{
				if (value != this._sunShadowsBakeMode)
				{
					this._sunShadowsBakeMode = value;
					this.needUpdateDepthSunTexture = true;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsRefreshInterval
		{
			get
			{
				return this._sunShadowsRefreshInterval;
			}
			set
			{
				if (value != this._sunShadowsRefreshInterval)
				{
					this._sunShadowsRefreshInterval = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float sunShadowsCancellation
		{
			get
			{
				return this._sunShadowsCancellation;
			}
			set
			{
				if (value != this._sunShadowsCancellation)
				{
					this._sunShadowsCancellation = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float turbulenceStrength
		{
			get
			{
				return this._turbulenceStrength;
			}
			set
			{
				if (value != this._turbulenceStrength)
				{
					this._turbulenceStrength = value;
					if (this._turbulenceStrength <= 0f)
					{
						this.UpdateTexture();
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool useXYPlane
		{
			get
			{
				return this._useXYPlane;
			}
			set
			{
				if (value != this._useXYPlane)
				{
					this._useXYPlane = value;
					if (this._sunShadows)
					{
						this.needUpdateDepthSunTexture = true;
					}
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public bool useSinglePassStereoRenderingMatrix
		{
			get
			{
				return this._useSinglePassStereoRenderingMatrix;
			}
			set
			{
				if (value != this._useSinglePassStereoRenderingMatrix)
				{
					this._useSinglePassStereoRenderingMatrix = value;
					this.isDirty = true;
				}
			}
		}

		public SPSR_BEHAVIOUR spsrBehaviour
		{
			get
			{
				return this._spsrBehaviour;
			}
			set
			{
				if (value != this._spsrBehaviour)
				{
					this._spsrBehaviour = value;
					this.isDirty = true;
				}
			}
		}

		public Camera fogCamera
		{
			get
			{
				return this.mainCamera;
			}
		}

		public int renderingInstancesCount
		{
			get
			{
				return this._renderingInstancesCount;
			}
		}

		public bool hasCamera
		{
			get
			{
				return this._hasCamera;
			}
		}

		private void OnEnable()
		{
			this.isPartOfScene = (this.isPartOfScene || this.IsPartOfScene());
			if (!this.isPartOfScene)
			{
				return;
			}
			if (this._fogVoidInverted)
			{
				this._fogVoidInverted = false;
				this._fogAreaCenter = this._character;
				this._fogAreaDepth = this._fogVoidDepth;
				this._fogAreaFallOff = this._fogVoidFallOff;
				this._fogAreaHeight = this._fogVoidHeight;
				this._fogAreaPosition = this._fogVoidPosition;
				this._fogAreaRadius = this._fogVoidRadius;
				this._fogVoidRadius = 0f;
				this._character = null;
			}
			this.mainCamera = base.gameObject.GetComponent<Camera>();
			this._hasCamera = (this.mainCamera != null);
			if (this._hasCamera)
			{
				this.fogRenderer = this;
				if (this.mainCamera.depthTextureMode == DepthTextureMode.None)
				{
					this.mainCamera.depthTextureMode = DepthTextureMode.Depth;
				}
			}
			else if (this.fogRenderer == null)
			{
				this.mainCamera = Camera.main;
				if (this.mainCamera == null)
				{
					return;
				}
				this.fogRenderer = this.mainCamera.GetComponent<VolumetricFog>();
				if (this.fogRenderer == null)
				{
					this.fogRenderer = this.mainCamera.gameObject.AddComponent<VolumetricFog>();
					this.fogRenderer.density = 0f;
				}
			}
			else
			{
				this.mainCamera = this.fogRenderer.mainCamera;
				if (this.mainCamera == null)
				{
					this.mainCamera = this.fogRenderer.GetComponent<Camera>();
				}
			}
			if (this._pointLights.Length < 6)
			{
				this._pointLights = new GameObject[6];
			}
			if (this._pointLightColors.Length < 6)
			{
				this._pointLightColors = new Color[6];
			}
			if (this._pointLightIntensities.Length < 6)
			{
				this._pointLightIntensities = new float[6];
			}
			if (this._pointLightIntensitiesMultiplier.Length < 6)
			{
				this._pointLightIntensitiesMultiplier = new float[6];
			}
			if (this._pointLightPositions.Length < 6)
			{
				this._pointLightPositions = new Vector3[6];
			}
			if (this._pointLightRanges.Length < 6)
			{
				this._pointLightRanges = new float[6];
			}
			if (this.fogMat == null)
			{
				this.InitFogMaterial();
				if (this._profile != null)
				{
					this._profile.Load(this);
				}
			}
			else
			{
				this.UpdateMaterialPropertiesNow();
			}
			this.RegisterWithRenderer();
		}

		private void OnDestroy()
		{
			if (!this._hasCamera && this.fogRenderer != null)
			{
				this.fogRenderer.UnregisterFogArea(this);
			}
			else
			{
				this.UnregisterFogArea(this);
			}
			if (this.depthCamObj != null)
			{
				UnityEngine.Object.DestroyImmediate(this.depthCamObj);
				this.depthCamObj = null;
			}
			if (this.adjustedTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(this.adjustedTexture);
				this.adjustedTexture = null;
			}
			if (this.chaosLerpMat != null)
			{
				UnityEngine.Object.DestroyImmediate(this.chaosLerpMat);
				this.chaosLerpMat = null;
			}
			if (this.adjustedChaosTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(this.adjustedChaosTexture);
				this.adjustedChaosTexture = null;
			}
			if (this.blurMat != null)
			{
				UnityEngine.Object.DestroyImmediate(this.blurMat);
				this.blurMat = null;
			}
			if (this.fogMat != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMat);
				this.fogMat = null;
			}
			this.CleanUpDepthTexture();
			this.DestroySunShadowsDependencies();
		}

		public void DestroySelf()
		{
			this.DestroyRenderComponent<VolumetricFogPreT>();
			this.DestroyRenderComponent<VolumetricFogPosT>();
			UnityEngine.Object.DestroyImmediate(this);
		}

		private void Start()
		{
			this.currentFogAlpha = this._alpha;
			this.currentSkyHazeAlpha = this._skyAlpha;
			this.lastTextureUpdate = Time.time + 0.2f;
			this.RegisterWithRenderer();
			this.Update();
		}

		private void Update()
		{
			if (!this.isPartOfScene)
			{
				return;
			}
			if (this.fogRenderer.sun != null)
			{
				Vector3 forward = this.fogRenderer.sun.transform.forward;
				bool flag = !Application.isPlaying || (this.updatingTextureSlice < 0 && Time.time - this.lastTextureUpdate >= 0.2f);
				if (flag)
				{
					if (forward != this._lightDirection)
					{
						this._lightDirection = forward;
						this.needUpdateTexture = true;
						this.needUpdateDepthSunTexture = true;
					}
					if (this.sunLight != null)
					{
						if (this._sunCopyColor && this.sunLight.color != this._lightColor)
						{
							this._lightColor = this.sunLight.color;
							this.needUpdateTexture = true;
						}
						if (this.sunLightIntensity != this.sunLight.intensity)
						{
							this.sunLightIntensity = this.sunLight.intensity;
							this.needUpdateTexture = true;
						}
					}
				}
			}
			if (!this.needUpdateTexture)
			{
				if (this._lightingModel == LIGHTING_MODEL.Classic)
				{
					if (this.lastRenderSettingsAmbientIntensity != RenderSettings.ambientIntensity)
					{
						this.needUpdateTexture = true;
					}
					else if (this.lastRenderSettingsAmbientLight != RenderSettings.ambientLight)
					{
						this.needUpdateTexture = true;
					}
				}
				else if (this._lightingModel == LIGHTING_MODEL.Natural && this.lastRenderSettingsAmbientLight != RenderSettings.ambientLight)
				{
					this.needUpdateTexture = true;
				}
			}
			if (this.transitionProfile)
			{
				float num = (Time.time - this.transitionStartTime) / this.transitionDuration;
				if (num > 1f)
				{
					num = 1f;
				}
				VolumetricFogProfile.Lerp(this.initialProfile, this.targetProfile, num, this);
				if (num >= 1f)
				{
					this.transitionProfile = false;
				}
			}
			if (this.transitionAlpha)
			{
				if (this.targetFogAlpha >= 0f || this.targetSkyHazeAlpha >= 0f)
				{
					if (this.targetFogAlpha != this.currentFogAlpha || this.targetSkyHazeAlpha != this.currentSkyHazeAlpha)
					{
						if (this.transitionDuration > 0f)
						{
							this.currentFogAlpha = Mathf.Lerp(this.initialFogAlpha, this.targetFogAlpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
							this.currentSkyHazeAlpha = Mathf.Lerp(this.initialSkyHazeAlpha, this.targetSkyHazeAlpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
						}
						else
						{
							this.currentFogAlpha = this.targetFogAlpha;
							this.currentSkyHazeAlpha = this.targetSkyHazeAlpha;
							this.transitionAlpha = false;
						}
						this.fogMat.SetFloat("_FogAlpha", this.currentFogAlpha);
						this.UpdateSkyColor(this.currentSkyHazeAlpha);
					}
				}
				else if (this.currentFogAlpha != this._alpha || this.currentSkyHazeAlpha != this._skyAlpha)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentFogAlpha = Mathf.Lerp(this.initialFogAlpha, this._alpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
						this.currentSkyHazeAlpha = Mathf.Lerp(this.initialSkyHazeAlpha, this.alpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentFogAlpha = this._alpha;
						this.currentSkyHazeAlpha = this._skyAlpha;
						this.transitionAlpha = false;
					}
					this.fogMat.SetFloat("_FogAlpha", this.currentFogAlpha);
					this.UpdateSkyColor(this.currentSkyHazeAlpha);
				}
			}
			if (this.transitionColor)
			{
				if (this.targetColorActive)
				{
					if (this.targetFogColor != this.currentFogColor)
					{
						if (this.transitionDuration > 0f)
						{
							this.currentFogColor = Color.Lerp(this.initialFogColor, this.targetFogColor, (Time.time - this.transitionStartTime) / this.transitionDuration);
						}
						else
						{
							this.currentFogColor = this.targetFogColor;
							this.transitionColor = false;
						}
					}
				}
				else if (this.currentFogColor != this._color)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentFogColor = Color.Lerp(this.initialFogColor, this._color, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentFogColor = this._color;
						this.transitionColor = false;
					}
				}
				this.UpdateMaterialFogColor();
			}
			if (this.transitionSpecularColor)
			{
				if (this.targetSpecularColorActive)
				{
					if (this.targetFogSpecularColor != this.currentFogSpecularColor)
					{
						if (this.transitionDuration > 0f)
						{
							this.currentFogSpecularColor = Color.Lerp(this.initialFogSpecularColor, this.targetFogSpecularColor, (Time.time - this.transitionStartTime) / this.transitionDuration);
						}
						else
						{
							this.currentFogSpecularColor = this.targetFogSpecularColor;
							this.transitionSpecularColor = false;
						}
						this.needUpdateTexture = true;
					}
				}
				else if (this.currentFogSpecularColor != this._specularColor)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentFogSpecularColor = Color.Lerp(this.initialFogSpecularColor, this._specularColor, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentFogSpecularColor = this._specularColor;
						this.transitionSpecularColor = false;
					}
					this.needUpdateTexture = true;
				}
			}
			if (this.transitionLightColor)
			{
				if (this.targetLightColorActive)
				{
					if (this.targetLightColor != this.currentLightColor)
					{
						if (this.transitionDuration > 0f)
						{
							this.currentLightColor = Color.Lerp(this.initialLightColor, this.targetLightColor, (Time.time - this.transitionStartTime) / this.transitionDuration);
						}
						else
						{
							this.currentLightColor = this.targetLightColor;
							this.transitionLightColor = false;
						}
						this.needUpdateTexture = true;
					}
				}
				else if (this.currentLightColor != this._lightColor)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentLightColor = Color.Lerp(this.initialLightColor, this._lightColor, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentLightColor = this._lightColor;
						this.transitionLightColor = false;
					}
					this.needUpdateTexture = true;
				}
			}
			if (this._baselineRelativeToCamera)
			{
				this.UpdateMaterialHeights();
			}
			else if (this._character != null)
			{
				this._fogVoidPosition = this._character.transform.position;
				this.UpdateMaterialHeights();
			}
			if (this._fogAreaCenter != null)
			{
				if (this._fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.FullXYZ)
				{
					this._fogAreaPosition = this._fogAreaCenter.transform.position;
				}
				else
				{
					this._fogAreaPosition.x = this._fogAreaCenter.transform.position.x;
					this._fogAreaPosition.z = this._fogAreaCenter.transform.position.z;
				}
				this.UpdateMaterialHeights();
			}
			if (this._pointLightTrackingAuto && (!Application.isPlaying || Time.time - this.trackPointAutoLastTime > this._pointLightTrackingCheckInterval))
			{
				this.trackPointAutoLastTime = Time.time;
				this.TrackPointLights();
			}
			if (this.updatingTextureSlice >= 0)
			{
				this.UpdateTextureColors(this.adjustedColors, false);
			}
			else if (this.needUpdateTexture)
			{
				this.UpdateTexture();
			}
			if (this._hasCamera)
			{
				if (this._fogOfWarEnabled)
				{
					this.FogOfWarUpdate();
				}
				if (this._sunShadows && this.fogRenderer.sun)
				{
					this.CastSunShadows();
				}
				int count = this.fogInstances.Count;
				if (count > 1)
				{
					Vector3 position = this.mainCamera.transform.position;
					bool flag2 = !Application.isPlaying || Time.time - this.lastTimeSortInstances >= 2f;
					if (!flag2)
					{
						float num2 = (position.x - this.lastCamPos.x) * (position.x - this.lastCamPos.x) + (position.y - this.lastCamPos.y) * (position.y - this.lastCamPos.y) + (position.z - this.lastCamPos.z) * (position.z - this.lastCamPos.z);
						if (num2 > 625f)
						{
							this.lastCamPos = position;
							flag2 = true;
						}
					}
					if (flag2)
					{
						this.lastTimeSortInstances = Time.time;
						float x2 = position.x;
						float y2 = position.y;
						float z = position.z;
						for (int i = 0; i < count; i++)
						{
							VolumetricFog volumetricFog = this.fogInstances[i];
							if (volumetricFog != null)
							{
								Vector3 position2 = volumetricFog.transform.position;
								position2.y = volumetricFog.currentFogAltitude;
								float num3 = x2 - position2.x;
								float num4 = y2 - position2.y;
								float num5 = num4 * num4;
								float num6 = y2 - (position2.y + volumetricFog.height);
								float num7 = num6 * num6;
								volumetricFog.distanceToCameraYAxis = ((num5 >= num7) ? num7 : num5);
								float num8 = z - position2.z;
								float num9 = num3 * num3 + num4 * num4 + num8 * num8;
								volumetricFog.distanceToCamera = num9;
								Vector3 position3 = position2 - volumetricFog.transform.localScale * 0.5f;
								Vector3 position4 = position2 + volumetricFog.transform.localScale * 0.5f;
								volumetricFog.distanceToCameraMin = this.mainCamera.WorldToScreenPoint(position3).z;
								volumetricFog.distanceToCameraMax = this.mainCamera.WorldToScreenPoint(position4).z;
							}
						}
						this.fogInstances.Sort(delegate(VolumetricFog x, VolumetricFog y)
						{
							if (!x || !y)
							{
								return 0;
							}
							if (x.fogAreaSortingMode == FOG_AREA_SORTING_MODE.Fixed || y.fogAreaSortingMode == FOG_AREA_SORTING_MODE.Fixed)
							{
								if (x.fogAreaRenderOrder < y.fogAreaRenderOrder)
								{
									return -1;
								}
								if (x.fogAreaRenderOrder > y.fogAreaRenderOrder)
								{
									return 1;
								}
								return 0;
							}
							else if ((x.distanceToCameraMin < y.distanceToCameraMin && x.distanceToCameraMax > y.distanceToCameraMax) || (y.distanceToCameraMin < x.distanceToCameraMin && y.distanceToCameraMax > x.distanceToCameraMax) || x.fogAreaSortingMode == FOG_AREA_SORTING_MODE.Altitude || y.fogAreaSortingMode == FOG_AREA_SORTING_MODE.Altitude)
							{
								if (x.distanceToCameraYAxis < y.distanceToCameraYAxis)
								{
									return 1;
								}
								if (x.distanceToCameraYAxis > y.distanceToCameraYAxis)
								{
									return -1;
								}
								return 0;
							}
							else
							{
								if (x.distanceToCamera < y.distanceToCamera)
								{
									return 1;
								}
								if (x.distanceToCamera > y.distanceToCamera)
								{
									return -1;
								}
								return 0;
							}
						});
					}
				}
			}
		}

		private void OnPreCull()
		{
			if (!base.enabled || !base.gameObject.activeSelf || this.fogMat == null || !this._hasCamera || this.mainCamera == null)
			{
				return;
			}
			if (this.mainCamera.depthTextureMode == DepthTextureMode.None)
			{
				this.mainCamera.depthTextureMode = DepthTextureMode.Depth;
			}
			if (this._computeDepth)
			{
				this.GetTransparentDepth();
			}
		}

		private bool IsPartOfScene()
		{
			VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == this)
				{
					return true;
				}
			}
			return false;
		}

		private void InitFogMaterial()
		{
			this.targetFogAlpha = -1f;
			this.targetSkyHazeAlpha = -1f;
			this._skyColor.a = this._skyAlpha;
			this.updatingTextureSlice = -1;
			this.fogMat = new Material(Shader.Find("VolumetricFogAndMist/VolumetricFog"));
			this.fogMat.hideFlags = HideFlags.DontSave;
			Texture2D texture2D = Resources.Load<Texture2D>("Textures/Noise3");
			this.noiseColors = texture2D.GetPixels();
			this.adjustedColors = new Color[this.noiseColors.Length];
			this.adjustedTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
			this.adjustedTexture.hideFlags = HideFlags.DontSave;
			this.timeOfLastRender = Time.time;
			this.UpdateTextureAlpha();
			this.UpdateSun();
			if (this._pointLightTrackingAuto)
			{
				this.TrackPointLights();
			}
			else
			{
				this.UpdatePointLights();
			}
			this.FogOfWarInit();
			if (this.fogOfWarTexture == null)
			{
				this.FogOfWarUpdateTexture();
			}
			this.CopyTransitionValues();
			this.UpdatePreset();
			this.oldBaselineRelativeCameraY = this.mainCamera.transform.position.y;
			if (this._sunShadows)
			{
				this.needUpdateDepthSunTexture = true;
			}
		}

		private void UpdateRenderComponents()
		{
			if (!this._hasCamera)
			{
				return;
			}
			if (this._renderBeforeTransparent)
			{
				this.AssignRenderComponent<VolumetricFogPreT>();
				this.DestroyRenderComponent<VolumetricFogPosT>();
			}
			else if (this._transparencyBlendMode == TRANSPARENT_MODE.Blend)
			{
				this.AssignRenderComponent<VolumetricFogPreT>();
				this.AssignRenderComponent<VolumetricFogPosT>();
			}
			else
			{
				this.AssignRenderComponent<VolumetricFogPosT>();
				this.DestroyRenderComponent<VolumetricFogPreT>();
			}
		}

		private void DestroyRenderComponent<T>() where T : IVolumetricFogRenderComponent
		{
			T[] componentsInChildren = base.GetComponentsInChildren<T>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].fog == this || componentsInChildren[i].fog == null)
				{
					componentsInChildren[i].DestroySelf();
				}
			}
		}

		private void AssignRenderComponent<T>() where T : Component, IVolumetricFogRenderComponent
		{
			T[] componentsInChildren = base.GetComponentsInChildren<T>(true);
			int num = -1;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].fog == this)
				{
					return;
				}
				if (componentsInChildren[i].fog == null)
				{
					num = i;
				}
			}
			if (num < 0)
			{
				T t = base.gameObject.AddComponent<T>();
				t.fog = this;
			}
			else
			{
				componentsInChildren[num].fog = this;
			}
		}

		private void RegisterFogArea(VolumetricFog fog)
		{
			if (this.fogInstances.Contains(fog))
			{
				return;
			}
			this.fogInstances.Add(fog);
		}

		private void UnregisterFogArea(VolumetricFog fog)
		{
			if (!this.fogInstances.Contains(fog))
			{
				return;
			}
			this.fogInstances.Remove(fog);
		}

		private void RegisterWithRenderer()
		{
			if (!this._hasCamera && this.fogRenderer != null)
			{
				this.fogRenderer.RegisterFogArea(this);
			}
			else
			{
				this.RegisterFogArea(this);
			}
			this.lastTimeSortInstances = 0f;
		}

		internal void DoOnRenderImage(RenderTexture source, RenderTexture destination)
		{
			int count = this.fogInstances.Count;
			this.fogRenderInstances.Clear();
			for (int i = 0; i < count; i++)
			{
				if (this.fogInstances[i] != null && this.fogInstances[i].isActiveAndEnabled && this.fogInstances[i].density > 0f)
				{
					this.fogRenderInstances.Add(this.fogInstances[i]);
				}
			}
			this._renderingInstancesCount = this.fogRenderInstances.Count;
			if (this._renderingInstancesCount == 0 || this.mainCamera == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (this._hasCamera && this._density <= 0f && this.shouldUpdateMaterialProperties)
			{
				this.UpdateMaterialPropertiesNow();
			}
			if (this._renderingInstancesCount == 1)
			{
				this.fogRenderInstances[0].DoOnRenderImageInstance(source, destination);
			}
			else
			{
				RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
				this.fogRenderInstances[0].DoOnRenderImageInstance(source, temporary);
				if (this._renderingInstancesCount == 2)
				{
					this.fogRenderInstances[1].DoOnRenderImageInstance(temporary, destination);
				}
				if (this._renderingInstancesCount >= 3)
				{
					RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
					RenderTexture source2 = temporary;
					RenderTexture renderTexture = temporary2;
					int num = this._renderingInstancesCount - 1;
					for (int j = 1; j < num; j++)
					{
						if (j > 1)
						{
							renderTexture.DiscardContents();
						}
						this.fogRenderInstances[j].DoOnRenderImageInstance(source2, renderTexture);
						if (renderTexture == temporary2)
						{
							source2 = temporary2;
							renderTexture = temporary;
						}
						else
						{
							source2 = temporary;
							renderTexture = temporary2;
						}
					}
					this.fogRenderInstances[num].DoOnRenderImageInstance(source2, destination);
					RenderTexture.ReleaseTemporary(temporary2);
				}
				RenderTexture.ReleaseTemporary(temporary);
			}
		}

		internal void DoOnRenderImageInstance(RenderTexture source, RenderTexture destination)
		{
			if (this.mainCamera == null || this.fogMat == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (!this._hasCamera)
			{
				this.CheckFogAreaDimensions();
				if (this._sunShadows && !this.fogRenderer.sunShadows)
				{
					this.fogRenderer.sunShadows = true;
				}
			}
			if (this.shouldUpdateMaterialProperties)
			{
				this.UpdateMaterialPropertiesNow();
			}
			if (Application.isPlaying)
			{
				if (this._useRealTime)
				{
					this.deltaTime = Time.time - this.timeOfLastRender;
					this.timeOfLastRender = Time.time;
				}
				else
				{
					this.deltaTime = Time.deltaTime;
				}
				this.UpdateWindSpeedQuick();
			}
			if (this._hasCamera)
			{
				if (this._spsrBehaviour == SPSR_BEHAVIOUR.ForcedOn && !this._useSinglePassStereoRenderingMatrix)
				{
					this.useSinglePassStereoRenderingMatrix = true;
				}
				else if (this._spsrBehaviour == SPSR_BEHAVIOUR.ForcedOff && this._useSinglePassStereoRenderingMatrix)
				{
					this.useSinglePassStereoRenderingMatrix = false;
				}
			}
			if (this.fogRenderer.useSinglePassStereoRenderingMatrix && XRSettings.enabled)
			{
				this.fogMat.SetMatrix("_ClipToWorld", this.mainCamera.cameraToWorldMatrix);
			}
			else
			{
				this.fogMat.SetMatrix("_ClipToWorld", this.mainCamera.cameraToWorldMatrix * this.mainCamera.projectionMatrix.inverse);
			}
			if (this.mainCamera.orthographic)
			{
				this.fogMat.SetVector("_ClipDir", this.mainCamera.transform.forward);
			}
			if (this.fogRenderer.sun && this._lightScatteringEnabled)
			{
				this.UpdateScatteringData();
			}
			for (int i = 0; i < 6; i++)
			{
				Light light = this.pointLightComponents[i];
				if (light != null)
				{
					if (this._pointLightColors[i] != light.color)
					{
						this._pointLightColors[i] = light.color;
						this.isDirty = true;
					}
					if (this._pointLightRanges[i] != light.range)
					{
						this._pointLightRanges[i] = light.range;
						this.isDirty = true;
					}
					if (this._pointLightPositions[i] != light.transform.position)
					{
						this._pointLightPositions[i] = light.transform.position;
						this.isDirty = true;
					}
					if (this._pointLightIntensities[i] != light.intensity)
					{
						this._pointLightIntensities[i] = light.intensity;
						this.isDirty = true;
					}
				}
				if (this._pointLightRanges[i] * this._pointLightIntensities[i] > 0f)
				{
					this.SetMaterialLightData(i, light);
				}
			}
			if (Application.isPlaying && this._turbulenceStrength > 0f)
			{
				this.ApplyChaos();
			}
			if ((float)this._downsampling > 1f)
			{
				int scaledSize = this.GetScaledSize(source.width, (float)this._downsampling);
				int scaledSize2 = this.GetScaledSize(source.width, (float)this._downsampling);
				this.reducedDestination = RenderTexture.GetTemporary(scaledSize, scaledSize2, 0, RenderTextureFormat.ARGB32);
				RenderTextureFormat format = (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat)) ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.RFloat;
				RenderTexture temporary = RenderTexture.GetTemporary(scaledSize, scaledSize2, 0, format);
				if (this._fogBlur)
				{
					RenderTexture temporary2 = RenderTexture.GetTemporary(scaledSize, scaledSize2, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(source, temporary2);
					this.SetBlurTexture(temporary2);
					RenderTexture.ReleaseTemporary(temporary2);
				}
				if (!this._edgeImprove || XRSettings.enabled || SystemInfo.supportedRenderTargetCount < 2)
				{
					Graphics.Blit(source, this.reducedDestination, this.fogMat, 3);
					if (this._edgeImprove)
					{
						Graphics.Blit(source, temporary, this.fogMat, 4);
					}
				}
				else
				{
					this.fogMat.SetTexture("_MainTex", source);
					if (this.mrt == null)
					{
						this.mrt = new RenderBuffer[2];
					}
					this.mrt[0] = this.reducedDestination.colorBuffer;
					this.mrt[1] = temporary.colorBuffer;
					Graphics.SetRenderTarget(this.mrt, this.reducedDestination.depthBuffer);
					Graphics.Blit(null, this.fogMat, 1);
				}
				this.fogMat.SetTexture("_FogDownsampled", this.reducedDestination);
				this.fogMat.SetTexture("_DownsampledDepth", temporary);
				Graphics.Blit(source, destination, this.fogMat, 2);
				RenderTexture.ReleaseTemporary(temporary);
				RenderTexture.ReleaseTemporary(this.reducedDestination);
			}
			else
			{
				if (this._fogBlur)
				{
					RenderTexture temporary3 = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(source, temporary3);
					this.SetBlurTexture(temporary3);
					RenderTexture.ReleaseTemporary(temporary3);
				}
				Graphics.Blit(source, destination, this.fogMat, 0);
			}
		}

		private int GetScaledSize(int size, float factor)
		{
			size = (int)((float)size / factor);
			size /= 4;
			if (size < 1)
			{
				size = 1;
			}
			return size * 4;
		}

		private void CleanUpDepthTexture()
		{
			if (this.depthTexture)
			{
				RenderTexture.ReleaseTemporary(this.depthTexture);
				this.depthTexture = null;
			}
		}

		private void GetTransparentDepth()
		{
			this.CleanUpDepthTexture();
			if (this.depthCam == null)
			{
				if (this.depthCamObj == null)
				{
					this.depthCamObj = GameObject.Find("VFMDepthCamera");
				}
				if (this.depthCamObj == null)
				{
					this.depthCamObj = new GameObject("VFMDepthCamera");
					this.depthCam = this.depthCamObj.AddComponent<Camera>();
					this.depthCam.enabled = false;
					this.depthCamObj.hideFlags = HideFlags.HideAndDontSave;
				}
				else
				{
					this.depthCam = this.depthCamObj.GetComponent<Camera>();
					if (this.depthCam == null)
					{
						UnityEngine.Object.DestroyImmediate(this.depthCamObj);
						this.depthCamObj = null;
						return;
					}
				}
			}
			this.depthCam.CopyFrom(this.mainCamera);
			this.depthCam.depthTextureMode = DepthTextureMode.None;
			this.depthTexture = RenderTexture.GetTemporary(this.mainCamera.pixelWidth, this.mainCamera.pixelHeight, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
			this.depthCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
			this.depthCam.clearFlags = CameraClearFlags.Color;
			this.depthCam.cullingMask = this._transparencyLayerMask;
			this.depthCam.targetTexture = this.depthTexture;
			this.depthCam.renderingPath = RenderingPath.Forward;
			if (this.depthShader == null)
			{
				this.depthShader = Shader.Find("VolumetricFogAndMist/CopyDepth");
			}
			if (this.depthShaderAndTrans == null)
			{
				this.depthShaderAndTrans = Shader.Find("VolumetricFogAndMist/CopyDepthAndTrans");
			}
			COMPUTE_DEPTH_SCOPE computeDepthScope = this._computeDepthScope;
			if (computeDepthScope != COMPUTE_DEPTH_SCOPE.OnlyTreeBillboards)
			{
				if (computeDepthScope != COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)
				{
					this.depthCam.RenderWithShader(this.depthShaderAndTrans, null);
				}
				else
				{
					this.depthCam.RenderWithShader(this.depthShaderAndTrans, "RenderType");
				}
			}
			else
			{
				this.depthCam.RenderWithShader(this.depthShader, "RenderType");
			}
			Shader.SetGlobalTexture("_VolumetricFogDepthTexture", this.depthTexture);
		}

		private void CastSunShadows()
		{
			if (!base.enabled || !base.gameObject.activeSelf || this.fogMat == null)
			{
				return;
			}
			if (this._sunShadowsBakeMode == SUN_SHADOWS_BAKE_MODE.Discrete && this._sunShadowsRefreshInterval > 0f && Time.time > this.lastShadowUpdateFrame + this._sunShadowsRefreshInterval)
			{
				this.needUpdateDepthSunTexture = true;
			}
			if (!Application.isPlaying || this.needUpdateDepthSunTexture || this.depthSunTexture == null || !this.depthSunTexture.IsCreated())
			{
				this.needUpdateDepthSunTexture = false;
				this.lastShadowUpdateFrame = Time.time;
				this.GetSunShadows();
			}
		}

		private void GetSunShadows()
		{
			if (this._sun == null || !this._sunShadows)
			{
				return;
			}
			if (this.depthSunCam == null)
			{
				if (this.depthSunCamObj == null)
				{
					this.depthSunCamObj = GameObject.Find("VFMDepthSunCamera");
				}
				if (this.depthSunCamObj == null)
				{
					this.depthSunCamObj = new GameObject("VFMDepthSunCamera");
					this.depthSunCamObj.hideFlags = HideFlags.HideAndDontSave;
					this.depthSunCam = this.depthSunCamObj.AddComponent<Camera>();
				}
				else
				{
					this.depthSunCam = this.depthSunCamObj.GetComponent<Camera>();
					if (this.depthSunCam == null)
					{
						UnityEngine.Object.DestroyImmediate(this.depthSunCamObj);
						this.depthSunCamObj = null;
						return;
					}
				}
				if (this.depthSunShader == null)
				{
					this.depthSunShader = Shader.Find("VolumetricFogAndMist/CopySunDepth");
				}
				this.depthSunCam.SetReplacementShader(this.depthSunShader, "RenderType");
				this.depthSunCam.nearClipPlane = 1f;
				this.depthSunCam.renderingPath = RenderingPath.Forward;
				this.depthSunCam.orthographic = true;
				this.depthSunCam.aspect = 1f;
				this.depthSunCam.backgroundColor = new Color(0f, 0f, 0.5f, 0f);
				this.depthSunCam.clearFlags = CameraClearFlags.Color;
				this.depthSunCam.depthTextureMode = DepthTextureMode.None;
			}
			float orthographicSize = this._sunShadowsMaxDistance / 0.95f;
			this.depthSunCam.transform.position = this.mainCamera.transform.position - this._sun.transform.forward * 2000f;
			this.depthSunCam.transform.rotation = this._sun.transform.rotation;
			this.depthSunCam.farClipPlane = 4000f;
			this.depthSunCam.orthographicSize = orthographicSize;
			if (this.sunLight != null)
			{
				this.depthSunCam.cullingMask = this._sunShadowsLayerMask;
			}
			if (this.depthSunTexture == null)
			{
				int num = (int)Mathf.Pow(2f, (float)(this._sunShadowsResolution + 9));
				this.depthSunTexture = new RenderTexture(num, num, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				this.depthSunTexture.hideFlags = HideFlags.DontSave;
				this.depthSunTexture.filterMode = FilterMode.Point;
				this.depthSunTexture.wrapMode = TextureWrapMode.Clamp;
				this.depthSunTexture.Create();
			}
			this.depthSunCam.targetTexture = this.depthSunTexture;
			Shader.SetGlobalFloat("_VF_ShadowBias", this._sunShadowsBias);
			if (Application.isPlaying && this._sunShadowsBakeMode == SUN_SHADOWS_BAKE_MODE.Realtime)
			{
				if (!this.depthSunCam.enabled)
				{
					this.depthSunCam.enabled = true;
				}
			}
			else
			{
				if (this.depthSunCam.enabled)
				{
					this.depthSunCam.enabled = false;
				}
				this.depthSunCam.Render();
			}
			Shader.SetGlobalMatrix("_VolumetricFogSunProj", this.depthSunCam.projectionMatrix * this.depthSunCam.worldToCameraMatrix);
			Shader.SetGlobalTexture("_VolumetricFogSunDepthTexture", this.depthSunTexture);
			Vector4 value = this.depthSunCam.transform.position;
			value.w = Mathf.Min(this._sunShadowsMaxDistance, this._maxFogLength);
			Shader.SetGlobalVector("_VolumetricFogSunWorldPos", value);
			this.UpdateSunShadowsData();
		}

		private void SetBlurTexture(RenderTexture source)
		{
			if (this.blurMat == null)
			{
				Shader shader = Shader.Find("VolumetricFogAndMist/Blur");
				this.blurMat = new Material(shader);
				this.blurMat.hideFlags = HideFlags.DontSave;
			}
			if (this.blurMat == null)
			{
				return;
			}
			this.blurMat.SetFloat("_BlurDepth", this._fogBlurDepth);
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			Graphics.Blit(source, temporary, this.blurMat, 0);
			RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			Graphics.Blit(temporary, temporary2, this.blurMat, 1);
			this.blurMat.SetFloat("_BlurDepth", this._fogBlurDepth * 2f);
			temporary.DiscardContents();
			Graphics.Blit(temporary2, temporary, this.blurMat, 0);
			temporary2.DiscardContents();
			Graphics.Blit(temporary, temporary2, this.blurMat, 1);
			this.fogMat.SetTexture("_BlurTex", temporary2);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void DestroySunShadowsDependencies()
		{
			if (this.depthSunCamObj != null)
			{
				UnityEngine.Object.DestroyImmediate(this.depthSunCamObj);
				this.depthSunCamObj = null;
			}
			this.CleanUpTextureDepthSun();
		}

		private void CleanUpTextureDepthSun()
		{
			if (this.depthSunTexture != null)
			{
				this.depthSunTexture.Release();
				this.depthSunTexture = null;
			}
		}

		public string GetCurrentPresetName()
		{
			return Enum.GetName(typeof(FOG_PRESET), this._preset);
		}

		private void UpdatePreset()
		{
			FOG_PRESET preset = this._preset;
			switch (preset)
			{
			case FOG_PRESET.SandStorm1:
				this._skySpeed = 0.35f;
				this._skyHaze = 388f;
				this._skyNoiseStrength = 0.847f;
				this._skyAlpha = 1f;
				this._density = 0.487f;
				this._noiseStrength = 0.758f;
				this._noiseScale = 1.71f;
				this._noiseSparse = 0f;
				this._distance = 0f;
				this._distanceFallOff = 0f;
				this._height = 16f;
				this._stepping = 6f;
				this._steppingNear = 0f;
				this._alpha = 1f;
				this._color = new Color(0.505f, 0.505f, 0.505f, 1f);
				this._skyColor = this._color;
				this._specularColor = new Color(1f, 1f, 0.8f, 1f);
				this._specularIntensity = 0f;
				this._specularThreshold = 0.6f;
				this._lightColor = Color.white;
				this._lightIntensity = 0f;
				this._speed = 0.3f;
				this._windDirection = Vector3.right;
				this._downsampling = 1;
				this._baselineRelativeToCamera = false;
				this.CheckWaterLevel(false);
				this._fogVoidRadius = 0f;
				this.CopyTransitionValues();
				break;
			case FOG_PRESET.Smoke:
				this._skySpeed = 0.109f;
				this._skyHaze = 10f;
				this._skyNoiseStrength = 0.119f;
				this._skyAlpha = 1f;
				this._density = 1f;
				this._noiseStrength = 0.767f;
				this._noiseScale = 1.6f;
				this._noiseSparse = 0f;
				this._distance = 0f;
				this._distanceFallOff = 0f;
				this._height = 8f;
				this._stepping = 12f;
				this._steppingNear = 25f;
				this._alpha = 1f;
				this._color = new Color(0.125f, 0.125f, 0.125f, 1f);
				this._skyColor = this._color;
				this._specularColor = new Color(1f, 1f, 1f, 1f);
				this._specularIntensity = 0.575f;
				this._specularThreshold = 0.6f;
				this._lightColor = Color.white;
				this._lightIntensity = 1f;
				this._speed = 0.075f;
				this._windDirection = Vector3.right;
				this._downsampling = 1;
				this._baselineRelativeToCamera = false;
				this.CheckWaterLevel(false);
				this._baselineHeight += 8f;
				this._fogVoidRadius = 0f;
				this.CopyTransitionValues();
				break;
			case FOG_PRESET.ToxicSwamp:
				this._skySpeed = 0.062f;
				this._skyHaze = 22f;
				this._skyNoiseStrength = 0.694f;
				this._skyAlpha = 1f;
				this._density = 1f;
				this._noiseStrength = 1f;
				this._noiseScale = 1f;
				this._noiseSparse = 0f;
				this._distance = 0f;
				this._distanceFallOff = 0f;
				this._height = 2.5f;
				this._stepping = 20f;
				this._steppingNear = 50f;
				this._alpha = 0.95f;
				this._color = new Color(0.0238f, 0.175f, 0.109f, 1f);
				this._skyColor = this._color;
				this._specularColor = new Color(0.593f, 0.625f, 0.207f, 1f);
				this._specularIntensity = 0.735f;
				this._specularThreshold = 0.6f;
				this._lightColor = new Color(0.73f, 0.746f, 0.511f, 1f);
				this._lightIntensity = 0.492f;
				this._speed = 0.0003f;
				this._windDirection = Vector3.right;
				this._downsampling = 1;
				this._baselineRelativeToCamera = false;
				this.CheckWaterLevel(false);
				this._fogVoidRadius = 0f;
				this.CopyTransitionValues();
				break;
			case FOG_PRESET.SandStorm2:
				this._skySpeed = 0f;
				this._skyHaze = 0f;
				this._skyNoiseStrength = 0.729f;
				this._skyAlpha = 0.55f;
				this._density = 0.545f;
				this._noiseStrength = 1f;
				this._noiseScale = 3f;
				this._noiseSparse = 0f;
				this._distance = 0f;
				this._distanceFallOff = 0f;
				this._height = 12f;
				this._stepping = 5f;
				this._steppingNear = 19.6f;
				this._alpha = 0.96f;
				this._color = new Color(0.609f, 0.609f, 0.609f, 1f);
				this._skyColor = this._color;
				this._specularColor = new Color(0.589f, 0.621f, 0.207f, 1f);
				this._specularIntensity = 0.505f;
				this._specularThreshold = 0.6f;
				this._lightColor = new Color(0.726f, 0.742f, 0.507f, 1f);
				this._lightIntensity = 0.581f;
				this._speed = 0.168f;
				this._windDirection = Vector3.right;
				this._downsampling = 1;
				this._baselineRelativeToCamera = false;
				this.CheckWaterLevel(false);
				this._fogVoidRadius = 0f;
				this.CopyTransitionValues();
				break;
			default:
				switch (preset)
				{
				case FOG_PRESET.GroundFog:
					this._skySpeed = 0.3f;
					this._skyHaze = 0f;
					this._skyNoiseStrength = 0.1f;
					this._skyAlpha = 0.85f;
					this._density = 0.6f;
					this._noiseStrength = 0.479f;
					this._noiseScale = 1.15f;
					this._noiseSparse = 0f;
					this._distance = 5f;
					this._distanceFallOff = 1f;
					this._height = 1.5f;
					this._stepping = 8f;
					this._steppingNear = 0f;
					this._alpha = 0.95f;
					this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
					this._skyColor = this._color;
					this._specularColor = new Color(1f, 1f, 0.8f, 1f);
					this._specularIntensity = 0.2f;
					this._specularThreshold = 0.6f;
					this._lightColor = Color.white;
					this._lightIntensity = 0.2f;
					this._speed = 0.01f;
					this._downsampling = 1;
					this._baselineRelativeToCamera = false;
					this.CheckWaterLevel(false);
					this._fogVoidRadius = 0f;
					this.CopyTransitionValues();
					break;
				case FOG_PRESET.FrostedGround:
					this._skySpeed = 0f;
					this._skyHaze = 0f;
					this._skyNoiseStrength = 0.729f;
					this._skyAlpha = 0.55f;
					this._density = 1f;
					this._noiseStrength = 0.164f;
					this._noiseScale = 1.81f;
					this._noiseSparse = 0f;
					this._distance = 0f;
					this._distanceFallOff = 0f;
					this._height = 0.5f;
					this._stepping = 20f;
					this._steppingNear = 50f;
					this._alpha = 0.97f;
					this._color = new Color(0.546f, 0.648f, 0.71f, 1f);
					this._skyColor = this._color;
					this._specularColor = new Color(0.792f, 0.792f, 0.792f, 1f);
					this._specularIntensity = 1f;
					this._specularThreshold = 0.866f;
					this._lightColor = new Color(0.972f, 0.972f, 0.972f, 1f);
					this._lightIntensity = 0.743f;
					this._speed = 0f;
					this._downsampling = 1;
					this._baselineRelativeToCamera = false;
					this.CheckWaterLevel(false);
					this._fogVoidRadius = 0f;
					this.CopyTransitionValues();
					break;
				case FOG_PRESET.FoggyLake:
					this._skySpeed = 0.3f;
					this._skyHaze = 40f;
					this._skyNoiseStrength = 0.574f;
					this._skyAlpha = 0.827f;
					this._density = 1f;
					this._noiseStrength = 0.03f;
					this._noiseScale = 5.77f;
					this._noiseSparse = 0f;
					this._distance = 0f;
					this._distanceFallOff = 0f;
					this._height = 4f;
					this._stepping = 6f;
					this._steppingNear = 14.4f;
					this._alpha = 1f;
					this._color = new Color(0f, 0.96f, 1f, 1f);
					this._skyColor = this._color;
					this._specularColor = Color.white;
					this._lightColor = Color.white;
					this._specularIntensity = 0.861f;
					this._specularThreshold = 0.907f;
					this._lightIntensity = 0.126f;
					this._speed = 0f;
					this._downsampling = 1;
					this._baselineRelativeToCamera = false;
					this.CheckWaterLevel(false);
					this._fogVoidRadius = 0f;
					this.CopyTransitionValues();
					break;
				default:
					if (preset != FOG_PRESET.Mist)
					{
						if (preset != FOG_PRESET.WindyMist)
						{
							if (preset != FOG_PRESET.LowClouds)
							{
								if (preset != FOG_PRESET.SeaClouds)
								{
									if (preset != FOG_PRESET.Fog)
									{
										if (preset != FOG_PRESET.HeavyFog)
										{
											if (preset != FOG_PRESET.Clear)
											{
												if (preset == FOG_PRESET.WorldEdge)
												{
													this._skySpeed = 0.3f;
													this._skyHaze = 60f;
													this._skyNoiseStrength = 1f;
													this._skyAlpha = 0.96f;
													this._density = 1f;
													this._noiseStrength = 1f;
													this._noiseScale = 3f;
													this._noiseSparse = 0f;
													this._distance = 0f;
													this._distanceFallOff = 0f;
													this._height = 20f;
													this._stepping = 6f;
													this._alpha = 0.98f;
													this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
													this._skyColor = this._color;
													this._specularColor = new Color(1f, 1f, 0.8f, 1f);
													this._specularIntensity = 0.259f;
													this._specularThreshold = 0.6f;
													this._lightColor = Color.white;
													this._lightIntensity = 0.15f;
													this._speed = 0.03f;
													this._downsampling = 2;
													this._baselineRelativeToCamera = false;
													this.CheckWaterLevel(false);
													Terrain activeTerrain = VolumetricFog.GetActiveTerrain();
													if (activeTerrain != null)
													{
														this._fogVoidPosition = activeTerrain.transform.position + activeTerrain.terrainData.size * 0.5f;
														this._fogVoidRadius = activeTerrain.terrainData.size.x * 0.45f;
														this._fogVoidHeight = activeTerrain.terrainData.size.y;
														this._fogVoidDepth = activeTerrain.terrainData.size.z * 0.45f;
														this._fogVoidFallOff = 6f;
														this._fogAreaRadius = 0f;
														this._character = null;
														this._fogAreaCenter = null;
														float x = activeTerrain.terrainData.size.x;
														if (this.mainCamera.farClipPlane < x)
														{
															this.mainCamera.farClipPlane = x;
														}
														if (this._maxFogLength < x * 0.6f)
														{
															this._maxFogLength = x * 0.6f;
														}
													}
													this.CopyTransitionValues();
												}
											}
											else
											{
												this._density = 0f;
												this._fogOfWarEnabled = false;
												this._fogVoidRadius = 0f;
											}
										}
										else
										{
											this._skySpeed = 0.05f;
											this._skyHaze = 500f;
											this._skyNoiseStrength = 0.96f;
											this._skyAlpha = 1f;
											this._density = 0.35f;
											this._noiseStrength = 0.1f;
											this._noiseScale = 1f;
											this._noiseSparse = 0f;
											this._distance = 20f;
											this._distanceFallOff = 0.8f;
											this._height = 18f;
											this._stepping = 6f;
											this._steppingNear = 0f;
											this._alpha = 1f;
											this._color = new Color(0.91f, 0.91f, 0.91f, 1f);
											this._skyColor = this._color;
											this._specularColor = new Color(1f, 1f, 0.8f, 1f);
											this._specularIntensity = 0f;
											this._specularThreshold = 0.6f;
											this._lightColor = Color.white;
											this._lightIntensity = 0f;
											this._speed = 0.015f;
											this._downsampling = 1;
											this._baselineRelativeToCamera = false;
											this.CheckWaterLevel(false);
											this._fogVoidRadius = 0f;
											this.CopyTransitionValues();
										}
									}
									else
									{
										this._skySpeed = 0.3f;
										this._skyHaze = 144f;
										this._skyNoiseStrength = 0.7f;
										this._skyAlpha = 0.9f;
										this._density = 0.35f;
										this._noiseStrength = 0.3f;
										this._noiseScale = 1f;
										this._noiseSparse = 0f;
										this._distance = 20f;
										this._distanceFallOff = 0.7f;
										this._height = 8f;
										this._stepping = 8f;
										this._steppingNear = 0f;
										this._alpha = 0.97f;
										this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
										this._skyColor = this._color;
										this._specularColor = new Color(1f, 1f, 0.8f, 1f);
										this._specularIntensity = 0f;
										this._specularThreshold = 0.6f;
										this._lightColor = Color.white;
										this._lightIntensity = 0f;
										this._speed = 0.05f;
										this._downsampling = 1;
										this._baselineRelativeToCamera = false;
										this.CheckWaterLevel(false);
										this._fogVoidRadius = 0f;
										this.CopyTransitionValues();
									}
								}
								else
								{
									this._skySpeed = 0.3f;
									this._skyHaze = 60f;
									this._skyNoiseStrength = 1f;
									this._skyAlpha = 0.96f;
									this._density = 1f;
									this._noiseStrength = 1f;
									this._noiseScale = 1.5f;
									this._noiseSparse = 0f;
									this._distance = 0f;
									this._distanceFallOff = 0f;
									this._height = 12.4f;
									this._stepping = 6f;
									this._alpha = 0.98f;
									this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
									this._skyColor = this._color;
									this._specularColor = new Color(1f, 1f, 0.8f, 1f);
									this._specularIntensity = 0.259f;
									this._specularThreshold = 0.6f;
									this._lightColor = Color.white;
									this._lightIntensity = 0.15f;
									this._speed = 0.008f;
									this._downsampling = 1;
									this._baselineRelativeToCamera = false;
									this.CheckWaterLevel(false);
									this._fogVoidRadius = 0f;
									this.CopyTransitionValues();
								}
							}
							else
							{
								this._skySpeed = 0.3f;
								this._skyHaze = 60f;
								this._skyNoiseStrength = 1f;
								this._skyAlpha = 0.96f;
								this._density = 1f;
								this._noiseStrength = 0.7f;
								this._noiseScale = 1f;
								this._noiseSparse = 0f;
								this._distance = 0f;
								this._distanceFallOff = 0f;
								this._height = 4f;
								this._stepping = 12f;
								this._steppingNear = 0f;
								this._alpha = 0.98f;
								this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
								this._skyColor = this._color;
								this._specularColor = new Color(1f, 1f, 0.8f, 1f);
								this._specularIntensity = 0.15f;
								this._specularThreshold = 0.6f;
								this._lightColor = Color.white;
								this._lightIntensity = 0.15f;
								this._speed = 0.008f;
								this._downsampling = 1;
								this._baselineRelativeToCamera = false;
								this.CheckWaterLevel(false);
								this._fogVoidRadius = 0f;
								this.CopyTransitionValues();
							}
						}
						else
						{
							this._skySpeed = 0.3f;
							this._skyHaze = 25f;
							this._skyNoiseStrength = 0.1f;
							this._skyAlpha = 0.85f;
							this._density = 0.3f;
							this._noiseStrength = 0.5f;
							this._noiseScale = 1.15f;
							this._noiseSparse = 0f;
							this._distance = 0f;
							this._distanceFallOff = 0f;
							this._height = 6.5f;
							this._stepping = 10f;
							this._steppingNear = 0f;
							this._alpha = 1f;
							this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
							this._skyColor = this._color;
							this._specularColor = new Color(1f, 1f, 0.8f, 1f);
							this._specularIntensity = 0.1f;
							this._specularThreshold = 0.6f;
							this._lightColor = Color.white;
							this._lightIntensity = 0f;
							this._speed = 0.15f;
							this._downsampling = 1;
							this._baselineRelativeToCamera = false;
							this.CheckWaterLevel(false);
							this._fogVoidRadius = 0f;
							this.CopyTransitionValues();
						}
					}
					else
					{
						this._skySpeed = 0.3f;
						this._skyHaze = 15f;
						this._skyNoiseStrength = 0.1f;
						this._skyAlpha = 0.8f;
						this._density = 0.3f;
						this._noiseStrength = 0.6f;
						this._noiseScale = 1f;
						this._noiseSparse = 0f;
						this._distance = 0f;
						this._distanceFallOff = 0f;
						this._height = 6f;
						this._stepping = 8f;
						this._steppingNear = 0f;
						this._alpha = 1f;
						this._color = new Color(0.89f, 0.89f, 0.89f, 1f);
						this._skyColor = this._color;
						this._specularColor = new Color(1f, 1f, 0.8f, 1f);
						this._specularIntensity = 0.1f;
						this._specularThreshold = 0.6f;
						this._lightColor = Color.white;
						this._lightIntensity = 0.12f;
						this._speed = 0.01f;
						this._downsampling = 1;
						this._baselineRelativeToCamera = false;
						this.CheckWaterLevel(false);
						this._fogVoidRadius = 0f;
						this.CopyTransitionValues();
					}
					break;
				}
				break;
			}
			this.FogOfWarUpdateTexture();
			this.UpdateMaterialProperties();
			this.UpdateRenderComponents();
			this.UpdateTextureAlpha();
			this.UpdateTexture();
		}

		public void CheckWaterLevel(bool baseZero)
		{
			if (this.mainCamera == null)
			{
				return;
			}
			if (this._baselineHeight > this.mainCamera.transform.position.y || baseZero)
			{
				this._baselineHeight = 0f;
			}
			GameObject gameObject = GameObject.Find("Water");
			if (gameObject == null)
			{
				GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null && array[i].layer == 4)
					{
						gameObject = array[i];
						break;
					}
				}
			}
			if (gameObject != null)
			{
				this._renderBeforeTransparent = false;
				if (this._baselineHeight < gameObject.transform.position.y)
				{
					this._baselineHeight = gameObject.transform.position.y;
				}
			}
			this.UpdateMaterialHeights();
		}

		public static Terrain GetActiveTerrain()
		{
			Terrain terrain = Terrain.activeTerrain;
			if (terrain != null && terrain.isActiveAndEnabled)
			{
				return terrain;
			}
			for (int i = 0; i < Terrain.activeTerrains.Length; i++)
			{
				terrain = Terrain.activeTerrains[i];
				if (terrain != null && terrain.isActiveAndEnabled)
				{
					return terrain;
				}
			}
			return null;
		}

		private void UpdateMaterialFogColor()
		{
			this.fogMat.SetColor("_Color", this.currentFogColor * 2f);
		}

		private void UpdateMaterialHeights()
		{
			this.currentFogAltitude = this._baselineHeight;
			Vector3 fogAreaPosition = this._fogAreaPosition;
			if (this._fogAreaRadius > 0f)
			{
				if (this._fogAreaCenter != null && this._fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.FullXYZ)
				{
					this.currentFogAltitude += this._fogAreaCenter.transform.position.y;
				}
				fogAreaPosition.y = 0f;
			}
			if (this._baselineRelativeToCamera && !this._useXYPlane)
			{
				this.oldBaselineRelativeCameraY += (this.mainCamera.transform.position.y - this.oldBaselineRelativeCameraY) * Mathf.Clamp01(1.001f - this._baselineRelativeToCameraDelay);
				this.currentFogAltitude += this.oldBaselineRelativeCameraY - 1f;
			}
			float w = 0.01f / this._noiseScale;
			this.fogMat.SetVector("_FogData", new Vector4(this.currentFogAltitude, this._height, 1f / this._density, w));
			this.fogMat.SetFloat("_FogSkyHaze", this._skyHaze + this.currentFogAltitude);
			Vector3 v = this._fogVoidPosition - this.currentFogAltitude * Vector3.up;
			this.fogMat.SetVector("_FogVoidPosition", v);
			this.fogMat.SetVector("_FogAreaPosition", fogAreaPosition);
		}

		private void UpdateMaterialProperties()
		{
			this.shouldUpdateMaterialProperties = true;
			if (!Application.isPlaying)
			{
				this.UpdateMaterialPropertiesNow();
			}
		}

		private void UpdateMaterialPropertiesNow()
		{
			if (this.fogMat == null)
			{
				return;
			}
			this.shouldUpdateMaterialProperties = false;
			this.UpdateSkyColor(this._skyAlpha);
			Vector4 value = new Vector4(1f / (this._stepping + 1f), 1f / (1f + this._steppingNear), this._edgeThreshold, (!this._dithering) ? 0f : (this._ditherStrength * 0.1f));
			this.fogMat.SetFloat("_Jitter", this._jitterStrength);
			if (!this._edgeImprove)
			{
				value.z = 0f;
			}
			this.fogMat.SetVector("_FogStepping", value);
			this.fogMat.SetFloat("_FogAlpha", this.currentFogAlpha);
			this.UpdateMaterialHeights();
			float num = 0.01f / this._noiseScale;
			float w = this._maxFogLength * this._maxFogLengthFallOff + 1f;
			this.fogMat.SetVector("_FogDistance", new Vector4(num * num * this._distance * this._distance, this._distanceFallOff * this._distanceFallOff + 0.1f, this._maxFogLength, w));
			this.UpdateMaterialFogColor();
			if (this.shaderKeywords == null)
			{
				this.shaderKeywords = new List<string>();
			}
			else
			{
				this.shaderKeywords.Clear();
			}
			if (this._distance > 0f)
			{
				this.shaderKeywords.Add("FOG_DISTANCE_ON");
			}
			if (this._fogVoidRadius > 0f && this._fogVoidFallOff > 0f)
			{
				Vector4 value2 = new Vector4(1f / (1f + this._fogVoidRadius), 1f / (1f + this._fogVoidHeight), 1f / (1f + this._fogVoidDepth), this._fogVoidFallOff);
				if (this._fogVoidTopology == FOG_VOID_TOPOLOGY.Box)
				{
					this.shaderKeywords.Add("FOG_VOID_BOX");
				}
				else
				{
					this.shaderKeywords.Add("FOG_VOID_SPHERE");
				}
				this.fogMat.SetVector("_FogVoidData", value2);
			}
			if (this._fogAreaRadius > 0f && this._fogAreaFallOff > 0f)
			{
				Vector4 value3 = new Vector4(1f / (0.0001f + this._fogAreaRadius), 1f / (0.0001f + this._fogAreaHeight), 1f / (0.0001f + this._fogAreaDepth), this._fogAreaFallOff);
				if (this._fogAreaTopology == FOG_AREA_TOPOLOGY.Box)
				{
					this.shaderKeywords.Add("FOG_AREA_BOX");
				}
				else
				{
					this.shaderKeywords.Add("FOG_AREA_SPHERE");
					value3.y = this._fogAreaRadius * this._fogAreaRadius;
					value3.x /= num;
					value3.z /= num;
				}
				this.fogMat.SetVector("_FogAreaData", value3);
			}
			if (this._skyHaze > 0f && this._skyAlpha > 0f && !this._useXYPlane)
			{
				this.shaderKeywords.Add("FOG_HAZE_ON");
			}
			if (this._fogOfWarEnabled)
			{
				this.shaderKeywords.Add("FOG_OF_WAR_ON");
				this.fogMat.SetTexture("_FogOfWar", this.fogOfWarTexture);
				this.fogMat.SetVector("_FogOfWarCenter", this._fogOfWarCenter);
				this.fogMat.SetVector("_FogOfWarSize", this._fogOfWarSize);
				Vector3 vector = this._fogOfWarCenter - 0.5f * this._fogOfWarSize;
				if (this._useXYPlane)
				{
					this.fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / this._fogOfWarSize.x, vector.y / (this._fogOfWarSize.y + 0.0001f), 1f));
				}
				else
				{
					this.fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / this._fogOfWarSize.x, 1f, vector.z / (this._fogOfWarSize.z + 0.0001f)));
				}
			}
			int num2 = -1;
			for (int i = 0; i < 6; i++)
			{
				if (this._pointLights[i] != null || this._pointLightRanges[i] * this._pointLightIntensities[i] > 0f)
				{
					num2 = i;
				}
			}
			if (num2 >= 0)
			{
				this.shaderKeywords.Add("FOG_POINT_LIGHT" + num2.ToString());
			}
			if (this.fogRenderer.sun)
			{
				this.UpdateScatteringData();
				if (this._lightScatteringEnabled && this._lightScatteringExposure > 0f)
				{
					this.shaderKeywords.Add("FOG_SCATTERING_ON");
				}
				if (this._sunShadows)
				{
					this.shaderKeywords.Add("FOG_SUN_SHADOWS_ON");
					this.UpdateSunShadowsData();
				}
			}
			if (this._fogBlur)
			{
				this.shaderKeywords.Add("FOG_BLUR_ON");
				this.fogMat.SetFloat("_FogBlurDepth", this._fogBlurDepth);
			}
			if (this._useXYPlane)
			{
				this.shaderKeywords.Add("FOG_USE_XY_PLANE");
			}
			if (this.fogRenderer.computeDepth)
			{
				this.shaderKeywords.Add("FOG_COMPUTE_DEPTH");
			}
			this.fogMat.shaderKeywords = this.shaderKeywords.ToArray();
			if (this._computeDepth && this._computeDepthScope == COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)
			{
				Shader.SetGlobalFloat("_VFM_CutOff", this._transparencyCutOff);
			}
		}

		private void UpdateSunShadowsData()
		{
			if (this._sun == null || !this._sunShadows || this.fogMat == null)
			{
				return;
			}
			float num = this._sunShadowsStrength * Mathf.Clamp01(-this._sun.transform.forward.y * 10f);
			if (num < 0f)
			{
				num = 0f;
			}
			if (num > 0f && !this.fogMat.IsKeywordEnabled("FOG_SUN_SHADOWS_ON"))
			{
				this.fogMat.EnableKeyword("FOG_SUN_SHADOWS_ON");
			}
			else if (num <= 0f && this.fogMat.IsKeywordEnabled("FOG_SUN_SHADOWS_ON"))
			{
				this.fogMat.DisableKeyword("FOG_SUN_SHADOWS_ON");
			}
			if (this._hasCamera)
			{
				Shader.SetGlobalVector("_VolumetricFogSunShadowsData", new Vector4(num, this._sunShadowsJitterStrength, this._sunShadowsCancellation, 0f));
			}
		}

		private void UpdateWindSpeedQuick()
		{
			if (this.fogMat == null)
			{
				return;
			}
			float d = 0.01f / this._noiseScale;
			this.windSpeedAcum += this.deltaTime * this._windDirection * this._speed / d;
			this.fogMat.SetVector("_FogWindDir", this.windSpeedAcum);
			this.skyHazeSpeedAcum += this.deltaTime * this._skySpeed / 20f;
			this.fogMat.SetVector("_FogSkyData", new Vector4(this._skyHaze, this._skyNoiseStrength, this.skyHazeSpeedAcum, this._skyDepth));
		}

		private void UpdateScatteringData()
		{
			Vector3 vector = this.mainCamera.WorldToViewportPoint(this.fogRenderer.sun.transform.forward * 10000f);
			if (vector.z < 0f)
			{
				Vector2 vector2 = new Vector2(vector.x, vector.y);
				float num = Mathf.Clamp01(1f - this._lightDirection.y);
				if (vector2 != this.oldSunPos)
				{
					this.oldSunPos = vector2;
					this.fogMat.SetVector("_SunPosition", vector2);
					this.sunFade = Mathf.SmoothStep(1f, 0f, (vector2 - Vector2.one * 0.5f).magnitude * 0.5f) * num;
				}
				if (this._lightScatteringEnabled && !this.fogMat.IsKeywordEnabled("FOG_SCATTERING_ON"))
				{
					this.fogMat.EnableKeyword("FOG_SCATTERING_ON");
				}
				float num2 = this._lightScatteringExposure * this.sunFade;
				this.fogMat.SetVector("_FogScatteringData", new Vector4(this._lightScatteringSpread / (float)this._lightScatteringSamples, (float)((num2 <= 0f) ? 0 : this._lightScatteringSamples), num2, this._lightScatteringWeight / (float)this._lightScatteringSamples));
				this.fogMat.SetVector("_FogScatteringData2", new Vector4(this._lightScatteringIllumination, this._lightScatteringDecay, this._lightScatteringJittering, (!this._lightScatteringEnabled) ? 0f : (1.2f * this._lightScatteringDiffusion * num * this.sunLightIntensity)));
				this.fogMat.SetVector("_SunDir", -this._lightDirection);
				this.fogMat.SetColor("_SunColor", this._lightColor);
			}
			else if (this.fogMat.IsKeywordEnabled("FOG_SCATTERING_ON"))
			{
				this.fogMat.DisableKeyword("FOG_SCATTERING_ON");
			}
		}

		private void UpdateSun()
		{
			if (this.fogRenderer.sun != null)
			{
				this.sunLight = this.fogRenderer.sun.GetComponent<Light>();
			}
			else
			{
				this.sunLight = null;
			}
		}

		private void UpdateSkyColor(float alpha)
		{
			if (this.fogMat == null)
			{
				return;
			}
			float num = (this._lightIntensity + this.sunLightIntensity) * Mathf.Clamp01(1f - this._lightDirection.y);
			if (num < 0f)
			{
				num = 0f;
			}
			else if (num > 1f)
			{
				num = 1f;
			}
			this._skyColor.a = alpha;
			Color value = num * this._skyColor;
			this.fogMat.SetColor("_FogSkyColor", value);
		}

		private void UpdatePointLights()
		{
			for (int i = 0; i < this._pointLights.Length; i++)
			{
				GameObject gameObject = this._pointLights[i];
				if (gameObject != null)
				{
					this.pointLightComponents[i] = gameObject.GetComponent<Light>();
				}
				else
				{
					this.pointLightComponents[i] = null;
				}
			}
		}

		private void UpdateTextureAlpha()
		{
			if (this.adjustedColors == null)
			{
				return;
			}
			float num = Mathf.Clamp(this._noiseStrength, 0f, 0.95f);
			for (int i = 0; i < this.adjustedColors.Length; i++)
			{
				float num2 = 1f - (this._noiseSparse + this.noiseColors[i].b) * num;
				num2 *= this._density * this._noiseFinalMultiplier;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				else if (num2 > 1f)
				{
					num2 = 1f;
				}
				this.adjustedColors[i].a = num2;
			}
			this.hasChangeAdjustedColorsAlpha = true;
		}

		private void UpdateTexture()
		{
			if (this.fogMat == null)
			{
				return;
			}
			this.UpdateSkyColor(this._skyAlpha);
			float num = this._lightIntensity + this.sunLightIntensity;
			if (!this._useXYPlane)
			{
				num *= Mathf.Clamp01(1f - this._lightDirection.y * 2f);
			}
			LIGHTING_MODEL lightingModel = this._lightingModel;
			if (lightingModel != LIGHTING_MODEL.Natural)
			{
				if (lightingModel != LIGHTING_MODEL.SingleLight)
				{
					Color a = RenderSettings.ambientLight * RenderSettings.ambientIntensity;
					this.updatingTextureLightColor = Color.Lerp(a, this.currentLightColor * num, num);
					this.lastRenderSettingsAmbientLight = RenderSettings.ambientLight;
					this.lastRenderSettingsAmbientIntensity = RenderSettings.ambientIntensity;
				}
				else
				{
					this.updatingTextureLightColor = Color.Lerp(Color.black, this.currentLightColor * num, this._lightIntensity);
				}
			}
			else
			{
				Color ambientLight = RenderSettings.ambientLight;
				this.lastRenderSettingsAmbientLight = RenderSettings.ambientLight;
				this.updatingTextureLightColor = Color.Lerp(ambientLight, this.currentLightColor * num + ambientLight, this._lightIntensity);
			}
			if (Application.isPlaying)
			{
				this.updatingTextureSlice = 0;
			}
			else
			{
				this.updatingTextureSlice = -1;
			}
			this.UpdateTextureColors(this.adjustedColors, this.hasChangeAdjustedColorsAlpha);
			this.needUpdateTexture = false;
		}

		private void UpdateTextureColors(Color[] colors, bool forceUpdateEntireTexture)
		{
			float num = 1.0001f - this._specularThreshold;
			int width = this.adjustedTexture.width;
			Vector3 vector = new Vector3(-this._lightDirection.x, 0f, -this._lightDirection.z);
			Vector3 vector2 = vector.normalized * 0.3f;
			vector2.y = ((this._lightDirection.y <= 0f) ? (1f - Mathf.Clamp01(-this._lightDirection.y)) : Mathf.Clamp01(1f - this._lightDirection.y));
			int num2 = Mathf.FloorToInt(vector2.z * (float)width) * width;
			int num3 = (int)((float)num2 + vector2.x * (float)width) + colors.Length;
			float num4 = vector2.y / num;
			Color color = this.currentFogSpecularColor * (1f + this._specularIntensity) * this._specularIntensity;
			bool flag = false;
			if (this.updatingTextureSlice >= 1 || forceUpdateEntireTexture)
			{
				flag = true;
			}
			float num5 = this.updatingTextureLightColor.r * 0.5f;
			float num6 = this.updatingTextureLightColor.g * 0.5f;
			float num7 = this.updatingTextureLightColor.b * 0.5f;
			float num8 = color.r * 0.5f;
			float num9 = color.g * 0.5f;
			float num10 = color.b * 0.5f;
			int num11 = colors.Length;
			int num12 = 0;
			int num13 = num11;
			if (this.updatingTextureSlice >= 0)
			{
				if (this.updatingTextureSlice > this._updateTextureSpread)
				{
					this.updatingTextureSlice = -1;
					this.needUpdateTexture = true;
					return;
				}
				num12 = num11 * this.updatingTextureSlice / this._updateTextureSpread;
				num13 = num11 * (this.updatingTextureSlice + 1) / this._updateTextureSpread;
			}
			int num14 = 0;
			for (int i = num12; i < num13; i++)
			{
				int num15 = (i + num3) % num11;
				float a = colors[i].a;
				float num16 = (a - colors[num15].a) * num4;
				if (num16 < 0f)
				{
					num16 = 0f;
				}
				else if (num16 > 1f)
				{
					num16 = 1f;
				}
				float num17 = num5 + num8 * num16;
				float num18 = num6 + num9 * num16;
				float num19 = num7 + num10 * num16;
				if (!flag)
				{
					if (num14++ < 100)
					{
						if (num17 != colors[i].r || num18 != colors[i].g || num19 != colors[i].b)
						{
							flag = true;
						}
					}
					else if (!flag)
					{
						break;
					}
				}
				colors[i].r = num17;
				colors[i].g = num18;
				colors[i].b = num19;
			}
			bool flag2 = forceUpdateEntireTexture;
			if (flag)
			{
				if (this.updatingTextureSlice >= 0)
				{
					this.updatingTextureSlice++;
					if (this.updatingTextureSlice >= this._updateTextureSpread)
					{
						this.updatingTextureSlice = -1;
						flag2 = true;
					}
				}
				else
				{
					flag2 = true;
				}
			}
			else
			{
				this.updatingTextureSlice = -1;
			}
			if (flag2)
			{
				if (Application.isPlaying && this._turbulenceStrength > 0f && this.adjustedChaosTexture)
				{
					this.adjustedChaosTexture.SetPixels(this.adjustedColors);
					this.adjustedChaosTexture.Apply();
				}
				else
				{
					this.adjustedTexture.SetPixels(this.adjustedColors);
					this.adjustedTexture.Apply();
					this.fogMat.SetTexture("_NoiseTex", this.adjustedTexture);
				}
				this.lastTextureUpdate = Time.time;
			}
		}

		private void ApplyChaos()
		{
			if (!this.adjustedTexture)
			{
				return;
			}
			if (this.chaosLerpMat == null)
			{
				Shader shader = Shader.Find("VolumetricFogAndMist/Chaos Lerp");
				this.chaosLerpMat = new Material(shader);
				this.chaosLerpMat.hideFlags = HideFlags.DontSave;
			}
			this.turbAcum += this.deltaTime * this._turbulenceStrength;
			this.chaosLerpMat.SetFloat("_Amount", this.turbAcum);
			if (!this.adjustedChaosTexture)
			{
				this.adjustedChaosTexture = UnityEngine.Object.Instantiate<Texture2D>(this.adjustedTexture);
				this.adjustedChaosTexture.hideFlags = HideFlags.DontSave;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(this.adjustedTexture.width, this.adjustedTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			temporary.wrapMode = TextureWrapMode.Repeat;
			Graphics.Blit(this.adjustedChaosTexture, temporary, this.chaosLerpMat);
			this.fogMat.SetTexture("_NoiseTex", temporary);
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void CopyTransitionValues()
		{
			this.currentFogAlpha = this._alpha;
			this.currentSkyHazeAlpha = this._skyAlpha;
			this.currentFogColor = this._color;
			this.currentFogSpecularColor = this._specularColor;
			this.currentLightColor = this._lightColor;
		}

		public void SetTargetProfile(VolumetricFogProfile targetProfile, float duration)
		{
			if (!this._useFogVolumes)
			{
				return;
			}
			this.initialProfile = ScriptableObject.CreateInstance<VolumetricFogProfile>();
			this.initialProfile.Save(this);
			this.targetProfile = targetProfile;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.transitionProfile = true;
		}

		public void ClearTargetProfile(float duration)
		{
			this.SetTargetProfile(this.initialProfile, duration);
		}

		public void SetTargetAlpha(float newFogAlpha, float newSkyHazeAlpha, float duration)
		{
			if (!this._useFogVolumes)
			{
				return;
			}
			this.initialFogAlpha = this.currentFogAlpha;
			this.initialSkyHazeAlpha = this.currentSkyHazeAlpha;
			this.targetFogAlpha = newFogAlpha;
			this.targetSkyHazeAlpha = newSkyHazeAlpha;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.transitionAlpha = true;
		}

		public void ClearTargetAlpha(float duration)
		{
			this.SetTargetAlpha(-1f, -1f, duration);
		}

		public void SetTargetColor(Color newColor, float duration)
		{
			if (!this.useFogVolumes)
			{
				return;
			}
			this.initialFogColor = this.currentFogColor;
			this.targetFogColor = newColor;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.transitionColor = true;
			this.targetColorActive = true;
		}

		public void ClearTargetColor(float duration)
		{
			this.SetTargetColor(this._color, duration);
			this.targetColorActive = false;
		}

		public void SetTargetSpecularColor(Color newSpecularColor, float duration)
		{
			if (!this.useFogVolumes)
			{
				return;
			}
			this.initialFogSpecularColor = this.currentFogSpecularColor;
			this.targetFogSpecularColor = newSpecularColor;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.transitionSpecularColor = true;
			this.targetSpecularColorActive = true;
		}

		public void ClearTargetSpecularColor(float duration)
		{
			this.SetTargetSpecularColor(this._specularColor, duration);
			this.targetSpecularColorActive = false;
		}

		public void SetTargetLightColor(Color newLightColor, float duration)
		{
			if (!this.useFogVolumes)
			{
				return;
			}
			this._sunCopyColor = false;
			this.initialLightColor = this.currentLightColor;
			this.targetLightColor = newLightColor;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.transitionLightColor = true;
			this.targetLightColorActive = true;
		}

		public void ClearTargetLightColor(float duration)
		{
			this.SetTargetLightColor(this._lightColor, duration);
			this.targetLightColorActive = false;
		}

		private void SetMaterialLightData(int k, Light lightComponent)
		{
			string str = k.ToString();
			Vector3 v = this._pointLightPositions[k];
			v.y -= this._baselineHeight;
			Vector3 v2 = new Vector3(this._pointLightColors[k].r, this._pointLightColors[k].g, this._pointLightColors[k].b) * this._pointLightIntensities[k] * 0.1f * this._pointLightIntensitiesMultiplier[k] * (this._pointLightRanges[k] * this._pointLightRanges[k]);
			this.fogMat.SetVector("_FogPointLightPosition" + str, v);
			this.fogMat.SetVector("_FogPointLightColor" + str, v2);
		}

		public GameObject GetPointLight(int index)
		{
			if (index < 0 || index > this._pointLights.Length)
			{
				return null;
			}
			return this._pointLights[index];
		}

		public void SetPointLight(int index, GameObject pointLight)
		{
			if (index < 0 || index > this._pointLights.Length)
			{
				return;
			}
			if (this._pointLights[index] != pointLight)
			{
				this._pointLights[index] = pointLight;
				this.UpdatePointLights();
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		public float GetPointLightRange(int index)
		{
			if (index < 0 || index > this._pointLightRanges.Length)
			{
				return 0f;
			}
			return this._pointLightRanges[index];
		}

		public void SetPointLightRange(int index, float range)
		{
			if (index < 0 || index > this._pointLightRanges.Length)
			{
				return;
			}
			if (range != this._pointLightRanges[index])
			{
				this._pointLightRanges[index] = range;
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		public float GetPointLightIntensity(int index)
		{
			if (index < 0 || index > this._pointLightIntensities.Length)
			{
				return 0f;
			}
			return this._pointLightIntensities[index];
		}

		public void SetPointLightIntensity(int index, float intensity)
		{
			if (index < 0 || index > this._pointLightIntensities.Length)
			{
				return;
			}
			if (intensity != this._pointLightIntensities[index])
			{
				this._pointLightIntensities[index] = intensity;
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		public float GetPointLightIntensityMultiplier(int index)
		{
			if (index < 0 || index > this._pointLightIntensitiesMultiplier.Length)
			{
				return 0f;
			}
			return this._pointLightIntensitiesMultiplier[index];
		}

		public void SetPointLightIntensityMultiplier(int index, float intensityMultiplier)
		{
			if (index < 0 || index > this._pointLightIntensitiesMultiplier.Length)
			{
				return;
			}
			if (intensityMultiplier != this._pointLightIntensitiesMultiplier[index])
			{
				this._pointLightIntensitiesMultiplier[index] = intensityMultiplier;
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		public Vector3 GetPointLightPosition(int index)
		{
			if (index < 0 || index > this._pointLightPositions.Length)
			{
				return Vector3.zero;
			}
			return this._pointLightPositions[index];
		}

		public void SetPointLightPosition(int index, Vector3 position)
		{
			if (index < 0 || index > this._pointLightPositions.Length)
			{
				return;
			}
			if (position != this._pointLightPositions[index])
			{
				this._pointLightPositions[index] = position;
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		public Color GetPointLightColor(int index)
		{
			if (index < 0 || index > this._pointLightColors.Length)
			{
				return Color.white;
			}
			return this._pointLightColors[index];
		}

		public void SetPointLightColor(int index, Color color)
		{
			if (index < 0 || index > this._pointLightColors.Length)
			{
				return;
			}
			if (color != this._pointLightColors[index])
			{
				this._pointLightColors[index] = color;
				this.UpdateMaterialProperties();
				this.isDirty = true;
			}
		}

		private void TrackNewLights()
		{
			this.lastFoundLights = UnityEngine.Object.FindObjectsOfType<Light>();
		}

		private void TrackPointLights()
		{
			if (!this._pointLightTrackingAuto)
			{
				return;
			}
			if (this.lastFoundLights == null || !Application.isPlaying || Time.time - this.trackPointCheckNewLightsLastTime > 3f)
			{
				this.trackPointCheckNewLightsLastTime = Time.time;
				this.TrackNewLights();
			}
			int num = this.lastFoundLights.Length;
			if (this.lightBuffer == null || this.lightBuffer.Length != num)
			{
				this.lightBuffer = new Light[num];
			}
			for (int i = 0; i < num; i++)
			{
				this.lightBuffer[i] = this.lastFoundLights[i];
			}
			bool flag = false;
			for (int j = 0; j < 6; j++)
			{
				GameObject gameObject = null;
				if (j < this._pointLightTrackingCount)
				{
					gameObject = this.GetNearestLight(this.lightBuffer);
				}
				this._pointLights[j] = gameObject;
				this._pointLightRanges[j] = 0f;
				if (this.currentLights[j] != gameObject)
				{
					this.currentLights[j] = gameObject;
					flag = true;
				}
			}
			if (flag)
			{
				this.UpdatePointLights();
				this.UpdateMaterialProperties();
			}
		}

		private GameObject GetNearestLight(Light[] lights)
		{
			float num = float.MaxValue;
			Vector3 position = this.mainCamera.transform.position;
			GameObject result = null;
			int num2 = -1;
			for (int i = 0; i < lights.Length; i++)
			{
				Light light = lights[i];
				if (!(light == null) && light.enabled && light.type == LightType.Point)
				{
					GameObject gameObject = lights[i].gameObject;
					if (gameObject.activeSelf)
					{
						float sqrMagnitude = (gameObject.transform.position - position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							result = gameObject;
							num = sqrMagnitude;
							num2 = i;
						}
					}
				}
			}
			if (num2 >= 0)
			{
				lights[num2] = null;
			}
			return result;
		}

		public static VolumetricFog CreateFogArea(Vector3 position, float radius, float height = 16f, float fallOff = 1f)
		{
			VolumetricFog volumetricFog = VolumetricFog.CreateFogAreaPlaceholder(true, position, radius, height, radius);
			volumetricFog.preset = FOG_PRESET.SeaClouds;
			volumetricFog.transform.position = position;
			volumetricFog.skyHaze = 0f;
			volumetricFog.dithering = true;
			return volumetricFog;
		}

		public static VolumetricFog CreateFogArea(Vector3 position, Vector3 boxSize)
		{
			VolumetricFog volumetricFog = VolumetricFog.CreateFogAreaPlaceholder(false, position, boxSize.x * 0.5f, boxSize.y * 0.5f, boxSize.z * 0.5f);
			volumetricFog.preset = FOG_PRESET.SeaClouds;
			volumetricFog.transform.position = position;
			volumetricFog.height = boxSize.y * 0.98f;
			volumetricFog.skyHaze = 0f;
			return volumetricFog;
		}

		private static VolumetricFog CreateFogAreaPlaceholder(bool spherical, Vector3 position, float radius, float height, float depth)
		{
			GameObject original = (!spherical) ? Resources.Load<GameObject>("Prefabs/FogBoxArea") : Resources.Load<GameObject>("Prefabs/FogSphereArea");
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
			gameObject.transform.position = position;
			gameObject.transform.localScale = new Vector3(radius, height, depth);
			return gameObject.GetComponent<VolumetricFog>();
		}

		public static void RemoveAllFogAreas()
		{
			VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && !array[i].hasCamera)
				{
					UnityEngine.Object.DestroyImmediate(array[i].gameObject);
				}
			}
		}

		private void CheckFogAreaDimensions()
		{
			if (this.mr == null)
			{
				this.mr = base.GetComponent<MeshRenderer>();
			}
			if (this.mr == null)
			{
				return;
			}
			Vector3 extents = this.mr.bounds.extents;
			FOG_AREA_TOPOLOGY fogAreaTopology = this._fogAreaTopology;
			if (fogAreaTopology != FOG_AREA_TOPOLOGY.Box)
			{
				if (fogAreaTopology == FOG_AREA_TOPOLOGY.Sphere)
				{
					this.fogAreaRadius = extents.x;
					if (base.transform.localScale.z != base.transform.localScale.x)
					{
						base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.x);
					}
				}
			}
			else
			{
				this.fogAreaRadius = extents.x;
				this.fogAreaHeight = extents.y;
				this.fogAreaDepth = extents.z;
			}
			if (this._fogAreaCenter != null)
			{
				if (this._fogAreaFollowMode == FOG_AREA_FOLLOW_MODE.FullXYZ)
				{
					base.transform.position = this._fogAreaCenter.transform.position;
				}
				else
				{
					base.transform.position = new Vector3(this._fogAreaCenter.transform.position.x, base.transform.position.y, this._fogAreaCenter.transform.position.z);
				}
			}
			this.fogAreaPosition = base.transform.position;
		}

		public bool fogOfWarEnabled
		{
			get
			{
				return this._fogOfWarEnabled;
			}
			set
			{
				if (value != this._fogOfWarEnabled)
				{
					this._fogOfWarEnabled = value;
					this.FogOfWarUpdateTexture();
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Vector3 fogOfWarCenter
		{
			get
			{
				return this._fogOfWarCenter;
			}
			set
			{
				if (value != this._fogOfWarCenter)
				{
					this._fogOfWarCenter = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public Vector3 fogOfWarSize
		{
			get
			{
				return this._fogOfWarSize;
			}
			set
			{
				if (value != this._fogOfWarSize && value.x > 0f && value.z > 0f)
				{
					this._fogOfWarSize = value;
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public int fogOfWarTextureSize
		{
			get
			{
				return this._fogOfWarTextureSize;
			}
			set
			{
				if (value != this._fogOfWarTextureSize && value > 16)
				{
					this._fogOfWarTextureSize = value;
					this.FogOfWarUpdateTexture();
					this.UpdateMaterialProperties();
					this.isDirty = true;
				}
			}
		}

		public float fogOfWarRestoreDelay
		{
			get
			{
				return this._fogOfWarRestoreDelay;
			}
			set
			{
				if (value != this._fogOfWarRestoreDelay)
				{
					this._fogOfWarRestoreDelay = value;
					this.isDirty = true;
				}
			}
		}

		public float fogOfWarRestoreDuration
		{
			get
			{
				return this._fogOfWarRestoreDuration;
			}
			set
			{
				if (value != this._fogOfWarRestoreDuration)
				{
					this._fogOfWarRestoreDuration = value;
					this.isDirty = true;
				}
			}
		}

		private void FogOfWarInit()
		{
			this.fowTransitionList = new List<VolumetricFog.FogOfWarTransition>();
		}

		private void FogOfWarUpdateTexture()
		{
			if (!this._fogOfWarEnabled)
			{
				return;
			}
			int scaledSize = this.GetScaledSize(this._fogOfWarTextureSize, 1f);
			this.fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.Alpha8, false);
			this.fogOfWarTexture.hideFlags = HideFlags.DontSave;
			this.fogOfWarTexture.filterMode = FilterMode.Bilinear;
			this.fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
			this.ResetFogOfWar();
		}

		private void FogOfWarUpdate()
		{
			if (!this._fogOfWarEnabled)
			{
				return;
			}
			int count = this.fowTransitionList.Count;
			int width = this.fogOfWarTexture.width;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				VolumetricFog.FogOfWarTransition fogOfWarTransition = this.fowTransitionList[i];
				if (fogOfWarTransition.enabled)
				{
					float num = Time.time - fogOfWarTransition.startTime - fogOfWarTransition.startDelay;
					if (num > 0f)
					{
						float num2 = (fogOfWarTransition.duration > 0f) ? (num / fogOfWarTransition.duration) : 1f;
						num2 = Mathf.Clamp01(num2);
						byte a = (byte)Mathf.Lerp((float)fogOfWarTransition.initialAlpha, (float)fogOfWarTransition.targetAlpha, num2);
						int num3 = fogOfWarTransition.y * width + fogOfWarTransition.x;
						this.fogOfWarColorBuffer[num3].a = a;
						this.fogOfWarTexture.SetPixel(fogOfWarTransition.x, fogOfWarTransition.y, this.fogOfWarColorBuffer[num3]);
						flag = true;
						if (num2 >= 1f)
						{
							fogOfWarTransition.enabled = false;
							if (fogOfWarTransition.targetAlpha < 255 && this._fogOfWarRestoreDelay > 0f)
							{
								this.AddFowOfWarTransitionSlot(fogOfWarTransition.x, fogOfWarTransition.y, fogOfWarTransition.targetAlpha, byte.MaxValue, this._fogOfWarRestoreDelay, this._fogOfWarRestoreDuration);
							}
						}
					}
				}
			}
			if (flag)
			{
				this.fogOfWarTexture.Apply();
			}
		}

		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
		{
			this.SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, 1f);
		}

		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration)
		{
			if (this.fogOfWarTexture == null)
			{
				return;
			}
			float num = (worldPosition.x - this._fogOfWarCenter.x) / this._fogOfWarSize.x + 0.5f;
			if (num < 0f || num > 1f)
			{
				return;
			}
			float num2 = (worldPosition.z - this._fogOfWarCenter.z) / this._fogOfWarSize.z + 0.5f;
			if (num2 < 0f || num2 > 1f)
			{
				return;
			}
			int width = this.fogOfWarTexture.width;
			int height = this.fogOfWarTexture.height;
			int num3 = (int)(num * (float)width);
			int num4 = (int)(num2 * (float)height);
			int num5 = num4 * width + num3;
			byte b = (byte)(fogNewAlpha * 255f);
			Color32 color = this.fogOfWarColorBuffer[num5];
			if (b != color.a)
			{
				float num6 = radius / this._fogOfWarSize.z;
				int num7 = Mathf.FloorToInt((float)height * num6);
				for (int i = num4 - num7; i <= num4 + num7; i++)
				{
					if (i > 0 && i < height - 1)
					{
						for (int j = num3 - num7; j <= num3 + num7; j++)
						{
							if (j > 0 && j < width - 1)
							{
								int num8 = Mathf.FloorToInt(Mathf.Sqrt((float)((num4 - i) * (num4 - i) + (num3 - j) * (num3 - j))));
								if (num8 <= num7)
								{
									num5 = i * width + j;
									Color32 color2 = this.fogOfWarColorBuffer[num5];
									byte b2 = (byte)Mathf.Lerp((float)b, (float)color2.a, (float)num8 / (float)num7);
									if (duration > 0f)
									{
										this.AddFowOfWarTransitionSlot(j, i, color2.a, b2, 0f, duration);
									}
									else
									{
										color2.a = b2;
										this.fogOfWarColorBuffer[num5] = color2;
										this.fogOfWarTexture.SetPixel(j, i, color2);
										if (this._fogOfWarRestoreDuration > 0f)
										{
											this.AddFowOfWarTransitionSlot(j, i, b2, byte.MaxValue, this._fogOfWarRestoreDelay, this._fogOfWarRestoreDuration);
										}
									}
								}
							}
						}
					}
				}
				this.fogOfWarTexture.Apply();
			}
		}

		public void ResetFogOfWarAlpha(Vector3 worldPosition, float radius)
		{
			if (this.fogOfWarTexture == null)
			{
				return;
			}
			float num = (worldPosition.x - this._fogOfWarCenter.x) / this._fogOfWarSize.x + 0.5f;
			if (num < 0f || num > 1f)
			{
				return;
			}
			float num2 = (worldPosition.z - this._fogOfWarCenter.z) / this._fogOfWarSize.z + 0.5f;
			if (num2 < 0f || num2 > 1f)
			{
				return;
			}
			int width = this.fogOfWarTexture.width;
			int height = this.fogOfWarTexture.height;
			int num3 = (int)(num * (float)width);
			int num4 = (int)(num2 * (float)height);
			int num5 = num4 * width + num3;
			float num6 = radius / this._fogOfWarSize.z;
			int num7 = Mathf.FloorToInt((float)height * num6);
			for (int i = num4 - num7; i <= num4 + num7; i++)
			{
				if (i > 0 && i < height - 1)
				{
					for (int j = num3 - num7; j <= num3 + num7; j++)
					{
						if (j > 0 && j < width - 1)
						{
							int num8 = Mathf.FloorToInt(Mathf.Sqrt((float)((num4 - i) * (num4 - i) + (num3 - j) * (num3 - j))));
							if (num8 <= num7)
							{
								num5 = i * width + j;
								Color32 color = this.fogOfWarColorBuffer[num5];
								color.a = byte.MaxValue;
								this.fogOfWarColorBuffer[num5] = color;
								this.fogOfWarTexture.SetPixel(j, i, color);
							}
						}
					}
				}
				this.fogOfWarTexture.Apply();
			}
		}

		public void ResetFogOfWar()
		{
			if (this.fogOfWarTexture == null || !this.isPartOfScene)
			{
				return;
			}
			int height = this.fogOfWarTexture.height;
			int width = this.fogOfWarTexture.width;
			int num = height * width;
			if (this.fogOfWarColorBuffer == null || this.fogOfWarColorBuffer.Length != num)
			{
				this.fogOfWarColorBuffer = new Color32[num];
			}
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			for (int i = 0; i < num; i++)
			{
				this.fogOfWarColorBuffer[i] = color;
			}
			this.fogOfWarTexture.SetPixels32(this.fogOfWarColorBuffer);
			this.fogOfWarTexture.Apply();
			this.fowTransitionList.Clear();
			this.isDirty = true;
		}

		public Color32[] fogOfWarTextureData
		{
			get
			{
				return this.fogOfWarColorBuffer;
			}
			set
			{
				this.fogOfWarEnabled = true;
				this.fogOfWarColorBuffer = value;
				if (value == null || this.fogOfWarTexture == null)
				{
					return;
				}
				if (value.Length != this.fogOfWarTexture.width * this.fogOfWarTexture.height)
				{
					return;
				}
				this.fogOfWarTexture.SetPixels32(this.fogOfWarColorBuffer);
				this.fogOfWarTexture.Apply();
			}
		}

		private void AddFowOfWarTransitionSlot(int x, int y, byte initialAlpha, byte targetAlpha, float delay, float duration)
		{
			int count = this.fowTransitionList.Count;
			VolumetricFog.FogOfWarTransition fogOfWarTransition = null;
			for (int i = 0; i < count; i++)
			{
				VolumetricFog.FogOfWarTransition fogOfWarTransition2 = this.fowTransitionList[i];
				if (fogOfWarTransition2.x == x && fogOfWarTransition2.y == y)
				{
					fogOfWarTransition = fogOfWarTransition2;
					break;
				}
				if (!fogOfWarTransition2.enabled)
				{
					fogOfWarTransition = fogOfWarTransition2;
				}
			}
			if (fogOfWarTransition == null)
			{
				fogOfWarTransition = new VolumetricFog.FogOfWarTransition();
				this.fowTransitionList.Add(fogOfWarTransition);
			}
			fogOfWarTransition.x = x;
			fogOfWarTransition.y = y;
			fogOfWarTransition.duration = duration;
			fogOfWarTransition.startTime = Time.time;
			fogOfWarTransition.startDelay = delay;
			fogOfWarTransition.initialAlpha = initialAlpha;
			fogOfWarTransition.targetAlpha = targetAlpha;
			fogOfWarTransition.enabled = true;
		}

		public const string SKW_FOG_DISTANCE_ON = "FOG_DISTANCE_ON";

		public const string SKW_LIGHT_SCATTERING = "FOG_SCATTERING_ON";

		public const string SKW_FOG_AREA_BOX = "FOG_AREA_BOX";

		public const string SKW_FOG_AREA_SPHERE = "FOG_AREA_SPHERE";

		public const string SKW_FOG_VOID_BOX = "FOG_VOID_BOX";

		public const string SKW_FOG_VOID_SPHERE = "FOG_VOID_SPHERE";

		public const string SKW_FOG_HAZE_ON = "FOG_HAZE_ON";

		public const string SKW_FOG_OF_WAR_ON = "FOG_OF_WAR_ON";

		public const string SKW_FOG_BLUR = "FOG_BLUR_ON";

		public const string SKW_SUN_SHADOWS = "FOG_SUN_SHADOWS_ON";

		public const string SKW_FOG_USE_XY_PLANE = "FOG_USE_XY_PLANE";

		public const string SKW_FOG_COMPUTE_DEPTH = "FOG_COMPUTE_DEPTH";

		private const float TIME_BETWEEN_TEXTURE_UPDATES = 0.2f;

		private const string DEPTH_CAM_NAME = "VFMDepthCamera";

		private const string DEPTH_SUN_CAM_NAME = "VFMDepthSunCamera";

		private const string VFM_BUILD_HINT = "VFMBuildHint81d";

		private static VolumetricFog _fog;

		[HideInInspector]
		public bool isDirty;

		[SerializeField]
		private FOG_PRESET _preset = FOG_PRESET.Mist;

		[SerializeField]
		private VolumetricFogProfile _profile;

		[SerializeField]
		private bool _useFogVolumes;

		[SerializeField]
		private bool _debugPass;

		[SerializeField]
		private TRANSPARENT_MODE _transparencyBlendMode;

		[SerializeField]
		private float _transparencyBlendPower = 1f;

		[SerializeField]
		private LayerMask _transparencyLayerMask = -1;

		[SerializeField]
		private LIGHTING_MODEL _lightingModel;

		[SerializeField]
		private bool _computeDepth;

		[SerializeField]
		private COMPUTE_DEPTH_SCOPE _computeDepthScope;

		[SerializeField]
		private float _transparencyCutOff = 0.1f;

		[SerializeField]
		private bool _renderBeforeTransparent;

		[SerializeField]
		private GameObject _sun;

		[SerializeField]
		private bool _sunCopyColor = true;

		[SerializeField]
		private float _density = 1f;

		[SerializeField]
		private float _noiseStrength = 0.8f;

		[SerializeField]
		private float _noiseFinalMultiplier = 1f;

		[SerializeField]
		private float _noiseSparse;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _maxFogLength = 1000f;

		[SerializeField]
		private float _maxFogLengthFallOff;

		[SerializeField]
		private float _distanceFallOff;

		[SerializeField]
		private float _height = 4f;

		[SerializeField]
		private float _baselineHeight;

		[SerializeField]
		private bool _baselineRelativeToCamera;

		[SerializeField]
		private float _baselineRelativeToCameraDelay;

		[SerializeField]
		private float _noiseScale = 1f;

		[SerializeField]
		private float _alpha = 1f;

		[SerializeField]
		private Color _color = new Color(0.89f, 0.89f, 0.89f, 1f);

		[SerializeField]
		private Color _specularColor = new Color(1f, 1f, 0.8f, 1f);

		[SerializeField]
		private float _specularThreshold = 0.6f;

		[SerializeField]
		private float _specularIntensity = 0.2f;

		[SerializeField]
		private Vector3 _lightDirection = new Vector3(1f, 0f, -1f);

		[SerializeField]
		private float _lightIntensity = 0.2f;

		[SerializeField]
		private Color _lightColor = Color.white;

		[SerializeField]
		private int _updateTextureSpread = 1;

		[SerializeField]
		private float _speed = 0.01f;

		[SerializeField]
		private Vector3 _windDirection = new Vector3(-1f, 0f, 0f);

		[SerializeField]
		private bool _useRealTime;

		[SerializeField]
		private Color _skyColor = new Color(0.89f, 0.89f, 0.89f, 1f);

		[SerializeField]
		private float _skyHaze = 50f;

		[SerializeField]
		private float _skySpeed = 0.3f;

		[SerializeField]
		private float _skyNoiseStrength = 0.1f;

		[SerializeField]
		private float _skyAlpha = 1f;

		[SerializeField]
		private float _skyDepth = 0.999f;

		[SerializeField]
		private GameObject _character;

		[SerializeField]
		private FOG_VOID_TOPOLOGY _fogVoidTopology;

		[SerializeField]
		private float _fogVoidFallOff = 1f;

		[SerializeField]
		private float _fogVoidRadius;

		[SerializeField]
		private Vector3 _fogVoidPosition = Vector3.zero;

		[SerializeField]
		private float _fogVoidDepth;

		[SerializeField]
		private float _fogVoidHeight;

		[SerializeField]
		private bool _fogVoidInverted;

		[SerializeField]
		private GameObject _fogAreaCenter;

		[SerializeField]
		private float _fogAreaFallOff = 1f;

		[SerializeField]
		private FOG_AREA_FOLLOW_MODE _fogAreaFollowMode;

		[SerializeField]
		private FOG_AREA_TOPOLOGY _fogAreaTopology = FOG_AREA_TOPOLOGY.Sphere;

		[SerializeField]
		private float _fogAreaRadius;

		[SerializeField]
		private Vector3 _fogAreaPosition = Vector3.zero;

		[SerializeField]
		private float _fogAreaDepth;

		[SerializeField]
		private float _fogAreaHeight;

		[SerializeField]
		private FOG_AREA_SORTING_MODE _fogAreaSortingMode;

		[SerializeField]
		private int _fogAreaRenderOrder = 1;

		public const int MAX_POINT_LIGHTS = 6;

		[SerializeField]
		private GameObject[] _pointLights = new GameObject[6];

		[SerializeField]
		private float[] _pointLightRanges = new float[6];

		[SerializeField]
		private float[] _pointLightIntensities = new float[]
		{
			1f,
			1f,
			1f,
			1f,
			1f,
			1f
		};

		[SerializeField]
		private float[] _pointLightIntensitiesMultiplier = new float[]
		{
			1f,
			1f,
			1f,
			1f,
			1f,
			1f
		};

		[SerializeField]
		private Vector3[] _pointLightPositions = new Vector3[6];

		[SerializeField]
		private Color[] _pointLightColors = new Color[]
		{
			new Color(1f, 1f, 0f, 1f),
			new Color(1f, 1f, 0f, 1f),
			new Color(1f, 1f, 0f, 1f),
			new Color(1f, 1f, 0f, 1f),
			new Color(1f, 1f, 0f, 1f),
			new Color(1f, 1f, 0f, 1f)
		};

		[SerializeField]
		private bool _pointLightTrackingAuto;

		[SerializeField]
		private int _pointLightTrackingCount;

		[SerializeField]
		private float _pointLightTrackingCheckInterval = 1f;

		[SerializeField]
		private int _downsampling = 1;

		[SerializeField]
		private bool _edgeImprove;

		[SerializeField]
		private float _edgeThreshold = 0.0005f;

		[SerializeField]
		private float _stepping = 12f;

		[SerializeField]
		private float _steppingNear = 1f;

		[SerializeField]
		private bool _dithering;

		[SerializeField]
		private float _ditherStrength = 0.75f;

		[SerializeField]
		private float _jitterStrength = 0.5f;

		[SerializeField]
		private bool _lightScatteringEnabled;

		[SerializeField]
		private float _lightScatteringDiffusion = 0.7f;

		[SerializeField]
		private float _lightScatteringSpread = 0.686f;

		[SerializeField]
		private int _lightScatteringSamples = 16;

		[SerializeField]
		private float _lightScatteringWeight = 1.9f;

		[SerializeField]
		private float _lightScatteringIllumination = 18f;

		[SerializeField]
		private float _lightScatteringDecay = 0.986f;

		[SerializeField]
		private float _lightScatteringExposure;

		[SerializeField]
		private float _lightScatteringJittering = 0.5f;

		[SerializeField]
		private bool _fogBlur;

		[SerializeField]
		private float _fogBlurDepth = 0.05f;

		[SerializeField]
		private bool _sunShadows;

		[SerializeField]
		private LayerMask _sunShadowsLayerMask = -1;

		[SerializeField]
		private float _sunShadowsStrength = 0.5f;

		[SerializeField]
		private float _sunShadowsBias = 0.1f;

		[SerializeField]
		private float _sunShadowsJitterStrength = 0.1f;

		[SerializeField]
		private int _sunShadowsResolution = 2;

		[SerializeField]
		private float _sunShadowsMaxDistance = 200f;

		[SerializeField]
		private SUN_SHADOWS_BAKE_MODE _sunShadowsBakeMode = SUN_SHADOWS_BAKE_MODE.Discrete;

		[SerializeField]
		private float _sunShadowsRefreshInterval;

		[SerializeField]
		private float _sunShadowsCancellation;

		[SerializeField]
		private float _turbulenceStrength;

		[SerializeField]
		private bool _useXYPlane;

		[SerializeField]
		private bool _useSinglePassStereoRenderingMatrix;

		[SerializeField]
		private SPSR_BEHAVIOUR _spsrBehaviour;

		[NonSerialized]
		public float distanceToCameraMin;

		[NonSerialized]
		public float distanceToCameraMax;

		[NonSerialized]
		public float distanceToCamera;

		[NonSerialized]
		public float distanceToCameraYAxis;

		public VolumetricFog fogRenderer;

		private bool isPartOfScene;

		private float initialFogAlpha;

		private float targetFogAlpha;

		private float initialSkyHazeAlpha;

		private float targetSkyHazeAlpha;

		private bool transitionAlpha;

		private bool transitionColor;

		private bool transitionSpecularColor;

		private bool transitionLightColor;

		private bool transitionProfile;

		private bool targetColorActive;

		private bool targetSpecularColorActive;

		private bool targetLightColorActive;

		private Color initialFogColor;

		private Color targetFogColor;

		private Color initialFogSpecularColor;

		private Color targetFogSpecularColor;

		private Color initialLightColor;

		private Color targetLightColor;

		private float transitionDuration;

		private float transitionStartTime;

		private float currentFogAlpha;

		private float currentSkyHazeAlpha;

		private Color currentFogColor;

		private Color currentFogSpecularColor;

		private Color currentLightColor;

		private VolumetricFogProfile initialProfile;

		private VolumetricFogProfile targetProfile;

		private float oldBaselineRelativeCameraY;

		private float currentFogAltitude;

		private float skyHazeSpeedAcum;

		private bool _hasCamera;

		private Camera mainCamera;

		private List<string> shaderKeywords;

		private Material blurMat;

		private RenderBuffer[] mrt;

		private int _renderingInstancesCount;

		private bool shouldUpdateMaterialProperties;

		[NonSerialized]
		public Material fogMat;

		private RenderTexture depthTexture;

		private RenderTexture depthSunTexture;

		private RenderTexture reducedDestination;

		private Light[] pointLightComponents = new Light[6];

		private Light[] lastFoundLights;

		private Light[] lightBuffer;

		private GameObject[] currentLights = new GameObject[6];

		private float trackPointAutoLastTime;

		private float trackPointCheckNewLightsLastTime;

		private Shader depthShader;

		private Shader depthShaderAndTrans;

		private GameObject depthCamObj;

		private Camera depthCam;

		private float lastTextureUpdate;

		private Vector3 windSpeedAcum;

		private Texture2D adjustedTexture;

		private Color[] noiseColors;

		private Color[] adjustedColors;

		private float sunLightIntensity = 1f;

		private bool needUpdateTexture;

		private bool hasChangeAdjustedColorsAlpha;

		private int updatingTextureSlice;

		private Color updatingTextureLightColor;

		private Color lastRenderSettingsAmbientLight;

		private float lastRenderSettingsAmbientIntensity;

		private Light sunLight;

		private Vector2 oldSunPos;

		private float sunFade = 1f;

		private GameObject depthSunCamObj;

		private Camera depthSunCam;

		private Shader depthSunShader;

		private bool needUpdateDepthSunTexture;

		private float lastShadowUpdateFrame;

		private Texture2D adjustedChaosTexture;

		private Material chaosLerpMat;

		private float turbAcum;

		private float deltaTime;

		private float timeOfLastRender;

		private List<VolumetricFog> fogInstances = new List<VolumetricFog>();

		private List<VolumetricFog> fogRenderInstances = new List<VolumetricFog>();

		private MeshRenderer mr;

		private float lastTimeSortInstances;

		private const float FOG_INSTANCES_SORT_INTERVAL = 2f;

		private Vector3 lastCamPos;

		[SerializeField]
		private bool _fogOfWarEnabled;

		[SerializeField]
		private Vector3 _fogOfWarCenter;

		[SerializeField]
		private Vector3 _fogOfWarSize = new Vector3(1024f, 0f, 1024f);

		[SerializeField]
		private int _fogOfWarTextureSize = 256;

		[SerializeField]
		private float _fogOfWarRestoreDelay;

		[SerializeField]
		private float _fogOfWarRestoreDuration = 2f;

		private Texture2D fogOfWarTexture;

		private Color32[] fogOfWarColorBuffer;

		private List<VolumetricFog.FogOfWarTransition> fowTransitionList;

		private class FogOfWarTransition
		{
			public bool enabled;

			public int x;

			public int y;

			public float startTime;

			public float startDelay;

			public float duration;

			public byte initialAlpha;

			public byte targetAlpha;
		}
	}
}
