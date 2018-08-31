using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace DynamicFogAndMist
{
	[HelpURL("http://kronnect.com/taptapgo")]
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	public class DynamicFog : MonoBehaviour
	{
		public FOG_TYPE effectType
		{
			get
			{
				return this._effectType;
			}
			set
			{
				if (value != this._effectType)
				{
					this._effectType = value;
					this._preset = FOG_PRESET.Custom;
					this.UpdateMaterialProperties();
				}
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
					this.UpdateMaterialProperties();
				}
			}
		}

		public DynamicFogProfile profile
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
						this.UpdateMaterialProperties();
					}
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
				}
			}
		}

		public bool enableDithering
		{
			get
			{
				return this._enableDithering;
			}
			set
			{
				if (value != this._enableDithering)
				{
					this._enableDithering = value;
					this.UpdateMaterialProperties();
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
					this._alpha = value;
					this.UpdateMaterialProperties();
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
					this._noiseStrength = value;
					this.UpdateMaterialProperties();
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
					this._noiseScale = value;
					this.UpdateMaterialProperties();
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
					this._distance = value;
					this.UpdateMaterialProperties();
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
					this._distanceFallOff = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float maxDistance
		{
			get
			{
				return this._maxDistance;
			}
			set
			{
				if (value != this._maxDistance)
				{
					this._maxDistance = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float maxDistanceFallOff
		{
			get
			{
				return this._maxDistanceFallOff;
			}
			set
			{
				if (value != this._maxDistanceFallOff)
				{
					this._maxDistanceFallOff = value;
					this.UpdateMaterialProperties();
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
					this._height = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float maxHeight
		{
			get
			{
				return this._maxHeight;
			}
			set
			{
				if (value != this._maxHeight)
				{
					this._maxHeight = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float heightFallOff
		{
			get
			{
				return this._heightFallOff;
			}
			set
			{
				if (value != this._heightFallOff)
				{
					this._heightFallOff = value;
					this.UpdateMaterialProperties();
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
					this._baselineHeight = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public bool clipUnderBaseline
		{
			get
			{
				return this._clipUnderBaseline;
			}
			set
			{
				if (value != this._clipUnderBaseline)
				{
					this._clipUnderBaseline = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float turbulence
		{
			get
			{
				return this._turbulence;
			}
			set
			{
				if (value != this._turbulence)
				{
					this._turbulence = value;
					this.UpdateMaterialProperties();
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
					this._speed = value;
					this.UpdateMaterialProperties();
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
					this._color = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public Color color2
		{
			get
			{
				return this._color2;
			}
			set
			{
				if (value != this._color2)
				{
					this._color2 = value;
					this.UpdateMaterialProperties();
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
					this._skyHaze = value;
					this.UpdateMaterialProperties();
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
					this._skySpeed = value;
					this.UpdateMaterialProperties();
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
					this._skyNoiseStrength = value;
					this.UpdateMaterialProperties();
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
					this._skyAlpha = value;
					this.UpdateMaterialProperties();
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
					this.UpdateMaterialProperties();
				}
			}
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
					this.UpdateMaterialProperties();
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
				if (value != this._fogOfWarSize)
				{
					this._fogOfWarSize = value;
					this.UpdateMaterialProperties();
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
				if (value != this._fogOfWarTextureSize)
				{
					this._fogOfWarTextureSize = value;
					this.UpdateMaterialProperties();
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
					this.UpdateMaterialProperties();
				}
			}
		}

		public bool useXZDistance
		{
			get
			{
				return this._useXZDistance;
			}
			set
			{
				if (value != this._useXZDistance)
				{
					this._useXZDistance = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public float scattering
		{
			get
			{
				return this._scattering;
			}
			set
			{
				if (value != this._scattering)
				{
					this._scattering = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public Color scatteringColor
		{
			get
			{
				return this._scatteringColor;
			}
			set
			{
				if (value != this._scatteringColor)
				{
					this._scatteringColor = value;
					this.UpdateMaterialProperties();
				}
			}
		}

		public static DynamicFog instance
		{
			get
			{
				if (DynamicFog._fog == null)
				{
					foreach (Camera camera in Camera.allCameras)
					{
						DynamicFog._fog = camera.GetComponent<DynamicFog>();
						if (DynamicFog._fog != null)
						{
							break;
						}
					}
				}
				return DynamicFog._fog;
			}
		}

		public string GetCurrentPresetName()
		{
			return Enum.GetName(typeof(FOG_PRESET), this.preset);
		}

		public Camera fogCamera
		{
			get
			{
				return this.currentCamera;
			}
		}

		private void OnEnable()
		{
			this.Init();
			this.UpdateMaterialPropertiesNow();
		}

		private void Reset()
		{
			this.UpdateMaterialPropertiesNow();
		}

		private void OnDestroy()
		{
			this.fogMat = null;
			if (this.fogMatVol != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatVol);
				this.fogMatVol = null;
				if (this.fogMatDesktopPlusOrthogonal != null)
				{
					UnityEngine.Object.DestroyImmediate(this.fogMatDesktopPlusOrthogonal);
					this.fogMatDesktopPlusOrthogonal = null;
				}
			}
			if (this.fogMatAdv != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatAdv);
				this.fogMatAdv = null;
			}
			if (this.fogMatFogSky != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatFogSky);
				this.fogMatFogSky = null;
			}
			if (this.fogMatOnlyFog != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatOnlyFog);
				this.fogMatOnlyFog = null;
			}
			if (this.fogMatSimple != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatSimple);
				this.fogMatSimple = null;
			}
			if (this.fogMatBasic != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatBasic);
				this.fogMatBasic = null;
			}
			if (this.fogMatOrthogonal != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatOrthogonal);
				this.fogMatOrthogonal = null;
			}
			if (this.fogMatDesktopPlusOrthogonal != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogMatDesktopPlusOrthogonal);
				this.fogMatOrthogonal = null;
			}
			if (this.fogOfWarTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogOfWarTexture);
				this.fogOfWarTexture = null;
			}
		}

		private void Init()
		{
			this.targetFogAlpha = -1f;
			this.targetSkyHazeAlpha = -1f;
			this.currentCamera = base.GetComponent<Camera>();
			this.UpdateFogOfWarTexture();
			if (this._profile != null)
			{
				this._profile.Load(this);
			}
		}

		private void Update()
		{
			if (this.fogMat == null)
			{
				return;
			}
			if (this.transitionProfile)
			{
				float num = (Time.time - this.transitionStartTime) / this.transitionDuration;
				if (num > 1f)
				{
					num = 1f;
				}
				DynamicFogProfile.Lerp(this.initialProfile, this.targetProfile, num, this);
				if (num >= 1f)
				{
					this.transitionProfile = false;
				}
			}
			if (this.transitionAlpha)
			{
				if (this.targetFogAlpha >= 0f)
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
						this.SetSkyData();
					}
				}
				else if (this.currentFogAlpha != this.alpha || this.targetSkyHazeAlpha != this.currentSkyHazeAlpha)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentFogAlpha = Mathf.Lerp(this.initialFogAlpha, this.alpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
						this.currentSkyHazeAlpha = Mathf.Lerp(this.initialSkyHazeAlpha, this.alpha, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentFogAlpha = this.alpha;
						this.currentSkyHazeAlpha = this.skyAlpha;
						this.transitionAlpha = false;
					}
					this.fogMat.SetFloat("_FogAlpha", this.currentFogAlpha);
					this.SetSkyData();
				}
			}
			if (this.transitionColor)
			{
				if (this.targetFogColors)
				{
					if (this.targetFogColor1 != this.currentFogColor1 || this.targetFogColor2 != this.currentFogColor2)
					{
						if (this.transitionDuration > 0f)
						{
							this.currentFogColor1 = Color.Lerp(this.initialFogColor1, this.targetFogColor1, (Time.time - this.transitionStartTime) / this.transitionDuration);
							this.currentFogColor2 = Color.Lerp(this.initialFogColor2, this.targetFogColor2, (Time.time - this.transitionStartTime) / this.transitionDuration);
						}
						else
						{
							this.currentFogColor1 = this.targetFogColor1;
							this.currentFogColor2 = this.targetFogColor2;
							this.transitionColor = false;
						}
						this.fogMat.SetColor("_FogColor", this.currentFogColor1);
						this.fogMat.SetColor("_FogColor2", this.currentFogColor2);
					}
				}
				else if (this.currentFogColor1 != this.color || this.currentFogColor2 != this.color2)
				{
					if (this.transitionDuration > 0f)
					{
						this.currentFogColor1 = Color.Lerp(this.initialFogColor1, this.color, (Time.time - this.transitionStartTime) / this.transitionDuration);
						this.currentFogColor2 = Color.Lerp(this.initialFogColor2, this.color2, (Time.time - this.transitionStartTime) / this.transitionDuration);
					}
					else
					{
						this.currentFogColor1 = this.color;
						this.currentFogColor2 = this.color2;
						this.transitionColor = false;
					}
					this.fogMat.SetColor("_FogColor", this.currentFogColor1);
					this.fogMat.SetColor("_FogColor2", this.currentFogColor2);
				}
			}
			if (this.sun != null)
			{
				bool flag = false;
				if (this.sun.transform.forward != this.sunDirection)
				{
					flag = true;
				}
				if (this.sunLight != null && (this.sunLight.color != this.sunColor || this.sunLight.intensity != this.sunIntensity))
				{
					flag = true;
				}
				if (flag)
				{
					this.UpdateFogColor();
				}
			}
		}

		public void CheckPreset()
		{
			if (this._preset != FOG_PRESET.Custom)
			{
				this._effectType = FOG_TYPE.DesktopFogWithSkyHaze;
			}
			switch (this.preset)
			{
			case FOG_PRESET.Clear:
				this.alpha = 0f;
				break;
			case FOG_PRESET.Mist:
				this.alpha = 0.75f;
				this.skySpeed = 0.11f;
				this.skyHaze = 15f;
				this.skyNoiseStrength = 1f;
				this.skyAlpha = 0.33f;
				this.distance = 0f;
				this.distanceFallOff = 0.07f;
				this.height = 4.4f;
				this.heightFallOff = 1f;
				this.turbulence = 0f;
				this.noiseStrength = 0.6f;
				this.speed = 0.01f;
				this.color = new Color(0.89f, 0.89f, 0.89f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			case FOG_PRESET.WindyMist:
				this.alpha = 0.75f;
				this.skySpeed = 0.3f;
				this.skyHaze = 35f;
				this.skyNoiseStrength = 0.32f;
				this.skyAlpha = 0.33f;
				this.distance = 0f;
				this.distanceFallOff = 0.07f;
				this.height = 2f;
				this.heightFallOff = 1f;
				this.turbulence = 2f;
				this.noiseStrength = 0.6f;
				this.speed = 0.06f;
				this.color = new Color(0.89f, 0.89f, 0.89f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			case FOG_PRESET.GroundFog:
				this.alpha = 1f;
				this.skySpeed = 0.3f;
				this.skyHaze = 35f;
				this.skyNoiseStrength = 0.32f;
				this.skyAlpha = 0.33f;
				this.distance = 0f;
				this.distanceFallOff = 0f;
				this.height = 1f;
				this.heightFallOff = 1f;
				this.turbulence = 0.4f;
				this.noiseStrength = 0.7f;
				this.speed = 0.005f;
				this.color = new Color(0.89f, 0.89f, 0.89f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			case FOG_PRESET.Fog:
				this.alpha = 0.96f;
				this.skySpeed = 0.3f;
				this.skyHaze = 155f;
				this.skyNoiseStrength = 0.6f;
				this.skyAlpha = 0.93f;
				this.distance = ((!this.effectType.isPlus()) ? 0.01f : 0.2f);
				this.distanceFallOff = 0.04f;
				this.height = 20f;
				this.heightFallOff = 1f;
				this.turbulence = 0.4f;
				this.noiseStrength = 0.4f;
				this.speed = 0.005f;
				this.color = new Color(0.89f, 0.89f, 0.89f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			case FOG_PRESET.HeavyFog:
				this.alpha = 1f;
				this.skySpeed = 0.05f;
				this.skyHaze = 350f;
				this.skyNoiseStrength = 0.8f;
				this.skyAlpha = 0.97f;
				this.distance = ((!this.effectType.isPlus()) ? 0f : 0.1f);
				this.distanceFallOff = 0.045f;
				this.height = 35f;
				this.heightFallOff = 0.88f;
				this.turbulence = 0.4f;
				this.noiseStrength = 0.24f;
				this.speed = 0.003f;
				this.color = new Color(0.86f, 0.847f, 0.847f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			case FOG_PRESET.SandStorm:
				this.alpha = 1f;
				this.skySpeed = 0.49f;
				this.skyHaze = 333f;
				this.skyNoiseStrength = 0.72f;
				this.skyAlpha = 0.97f;
				this.distance = ((!this.effectType.isPlus()) ? 0f : 0.15f);
				this.distanceFallOff = 0.028f;
				this.height = 83f;
				this.heightFallOff = 0f;
				this.turbulence = 15f;
				this.noiseStrength = 0.45f;
				this.speed = 0.2f;
				this.color = new Color(0.364f, 0.36f, 0.36f, 1f);
				this.color2 = this.color;
				this.maxDistance = 0.999f;
				this.maxDistanceFallOff = 0f;
				break;
			}
		}

		private void OnPreCull()
		{
			if (this.currentCamera != null && this.currentCamera.depthTextureMode == DepthTextureMode.None)
			{
				this.currentCamera.depthTextureMode = DepthTextureMode.Depth;
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this.fogMat == null || this._alpha == 0f || this.currentCamera == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (this.shouldUpdateMaterialProperties)
			{
				this.shouldUpdateMaterialProperties = false;
				this.UpdateMaterialPropertiesNow();
			}
			if (this.currentCamera.orthographic)
			{
				if (!this.matOrtho)
				{
					this.ResetMaterial();
				}
				this.fogMat.SetVector("_ClipDir", this.currentCamera.transform.forward);
			}
			else if (this.matOrtho)
			{
				this.ResetMaterial();
			}
			if (this.useSinglePassStereoRenderingMatrix && XRSettings.enabled)
			{
				this.fogMat.SetMatrix("_ClipToWorld", this.currentCamera.cameraToWorldMatrix);
			}
			else
			{
				this.fogMat.SetMatrix("_ClipToWorld", this.currentCamera.cameraToWorldMatrix * this.currentCamera.projectionMatrix.inverse);
			}
			Graphics.Blit(source, destination, this.fogMat);
		}

		private void ResetMaterial()
		{
			this.fogMat = null;
			this.fogMatAdv = null;
			this.fogMatFogSky = null;
			this.fogMatOnlyFog = null;
			this.fogMatSimple = null;
			this.fogMatBasic = null;
			this.fogMatVol = null;
			this.fogMatDesktopPlusOrthogonal = null;
			this.fogMatOrthogonal = null;
			this.UpdateMaterialProperties();
		}

		public void UpdateMaterialProperties()
		{
			if (Application.isPlaying)
			{
				this.shouldUpdateMaterialProperties = true;
			}
			else
			{
				this.UpdateMaterialPropertiesNow();
			}
		}

		private void UpdateMaterialPropertiesNow()
		{
			this.CheckPreset();
			this.CopyTransitionValues();
			switch (this.effectType)
			{
			case FOG_TYPE.MobileFogWithSkyHaze:
				if (this.fogMatFogSky == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFOWithSky";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGWithSky";
					}
					this.fogMatFogSky = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatFogSky.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatFogSky;
				break;
			case FOG_TYPE.MobileFogOnlyGround:
				if (this.fogMatOnlyFog == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFOOnlyFog";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGOnlyFog";
					}
					this.fogMatOnlyFog = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatOnlyFog.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatOnlyFog;
				break;
			case FOG_TYPE.DesktopFogPlusWithSkyHaze:
				if (this.fogMatVol == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFODesktopPlus";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGDesktopPlus";
					}
					this.fogMatVol = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatVol.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatVol;
				break;
			case FOG_TYPE.MobileFogSimple:
				if (this.fogMatSimple == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFOSimple";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGSimple";
					}
					this.fogMatSimple = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatSimple.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatSimple;
				break;
			case FOG_TYPE.MobileFogBasic:
				if (this.fogMatBasic == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFOBasic";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGBasic";
					}
					this.fogMatBasic = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatBasic.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatBasic;
				break;
			case FOG_TYPE.MobileFogOrthogonal:
				if (this.fogMatOrthogonal == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFOOrthogonal";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGOrthogonal";
					}
					this.fogMatOrthogonal = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatOrthogonal.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatOrthogonal;
				break;
			case FOG_TYPE.DesktopFogPlusOrthogonal:
				if (this.fogMatDesktopPlusOrthogonal == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFODesktopPlusOrthogonal";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGDesktopPlusOrthogonal";
					}
					this.fogMatDesktopPlusOrthogonal = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatDesktopPlusOrthogonal.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatDesktopPlusOrthogonal;
				break;
			default:
				if (this.fogMatAdv == null)
				{
					string path;
					if (this.currentCamera.orthographic)
					{
						this.matOrtho = true;
						path = "Materials/DFODesktop";
					}
					else
					{
						this.matOrtho = false;
						path = "Materials/DFGDesktop";
					}
					this.fogMatAdv = UnityEngine.Object.Instantiate<Material>(Resources.Load<Material>(path));
					this.fogMatAdv.hideFlags = HideFlags.DontSave;
				}
				this.fogMat = this.fogMatAdv;
				break;
			}
			if (this.fogMat == null)
			{
				return;
			}
			if (this.currentCamera == null)
			{
				this.currentCamera = base.GetComponent<Camera>();
			}
			this.fogMat.SetFloat("_FogSpeed", (this.effectType != FOG_TYPE.DesktopFogPlusWithSkyHaze) ? this._speed : (this._speed * 5f));
			Vector4 value = new Vector4(this._noiseStrength, this._turbulence, this.currentCamera.farClipPlane * 15f / 1000f, this._noiseScale);
			this.fogMat.SetVector("_FogNoiseData", value);
			Vector4 value2 = new Vector4(this._height + 0.001f, this._baselineHeight, (!this._clipUnderBaseline) ? -10000f : -0.01f, this._heightFallOff);
			if (this._effectType == FOG_TYPE.MobileFogOrthogonal || this._effectType == FOG_TYPE.DesktopFogPlusOrthogonal)
			{
				value2.z = this.maxHeight;
			}
			this.fogMat.SetVector("_FogHeightData", value2);
			this.fogMat.SetFloat("_FogAlpha", this.currentFogAlpha);
			Vector4 value3 = new Vector4(this._distance, this._distanceFallOff, this._maxDistance, this._maxDistanceFallOff);
			if (this.effectType.isPlus())
			{
				value3.x = this.currentCamera.farClipPlane * this._distance;
				value3.y = this.distanceFallOff * value3.x + 0.0001f;
				value3.z *= this.currentCamera.farClipPlane;
			}
			this.fogMat.SetVector("_FogDistance", value3);
			this.UpdateFogColor();
			this.SetSkyData();
			if (this.shaderKeywords == null)
			{
				this.shaderKeywords = new List<string>();
			}
			else
			{
				this.shaderKeywords.Clear();
			}
			if (this.fogOfWarEnabled)
			{
				if (this.fogOfWarTexture == null)
				{
					this.UpdateFogOfWarTexture();
				}
				this.fogMat.SetTexture("_FogOfWar", this.fogOfWarTexture);
				this.fogMat.SetVector("_FogOfWarCenter", this._fogOfWarCenter);
				this.fogMat.SetVector("_FogOfWarSize", this._fogOfWarSize);
				Vector3 vector = this.fogOfWarCenter - 0.5f * this._fogOfWarSize;
				this.fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(vector.x / this._fogOfWarSize.x, 1f, vector.z / this._fogOfWarSize.z));
				this.shaderKeywords.Add("FOG_OF_WAR_ON");
			}
			if (this._enableDithering)
			{
				this.fogMat.SetFloat("_FogDither", this._ditherStrength);
				this.shaderKeywords.Add("DITHER_ON");
			}
			this.fogMat.shaderKeywords = this.shaderKeywords.ToArray();
		}

		private void CopyTransitionValues()
		{
			this.currentFogAlpha = this._alpha;
			this.currentSkyHazeAlpha = this._skyAlpha;
			this.currentFogColor1 = this._color;
			this.currentFogColor2 = this._color2;
		}

		private void SetSkyData()
		{
			Vector4 value = new Vector4(this._skyHaze, this._skySpeed, this._skyNoiseStrength, this.currentSkyHazeAlpha);
			this.fogMat.SetVector("_FogSkyData", value);
		}

		private void UpdateFogColor()
		{
			if (this.fogMat == null)
			{
				return;
			}
			if (this._sun != null)
			{
				if (this.sunLight == null)
				{
					this.sunLight = this._sun.GetComponent<Light>();
				}
				if (this.sunLight != null && this.sunLight.transform != this._sun.transform)
				{
					this.sunLight = this._sun.GetComponent<Light>();
				}
				this.sunDirection = this._sun.transform.forward;
				if (this.sunLight != null)
				{
					this.sunColor = this.sunLight.color;
					this.sunIntensity = this.sunLight.intensity;
				}
			}
			float b = this.sunIntensity * Mathf.Clamp01(1f - this.sunDirection.y);
			this.fogMat.SetColor("_FogColor", b * this.currentFogColor1 * this.sunColor);
			this.fogMat.SetColor("_FogColor2", b * this.currentFogColor2 * this.sunColor);
			Color color = b * this.scatteringColor;
			this.fogMat.SetColor("_SunColor", new Vector4(color.r, color.g, color.b, this.scattering));
			this.fogMat.SetVector("_SunDir", -this.sunDirection);
		}

		public void SetTargetProfile(DynamicFogProfile targetProfile, float duration)
		{
			if (!this._useFogVolumes)
			{
				return;
			}
			this.preset = FOG_PRESET.Custom;
			this.initialProfile = ScriptableObject.CreateInstance<DynamicFogProfile>();
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
			if (!this.useFogVolumes)
			{
				return;
			}
			this.preset = FOG_PRESET.Custom;
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

		public void SetTargetColors(Color color1, Color color2, float duration)
		{
			if (!this.useFogVolumes)
			{
				return;
			}
			this.preset = FOG_PRESET.Custom;
			this.initialFogColor1 = this.currentFogColor1;
			this.initialFogColor2 = this.currentFogColor2;
			this.targetFogColor1 = color1;
			this.targetFogColor2 = color2;
			this.transitionDuration = duration;
			this.transitionStartTime = Time.time;
			this.targetFogColors = true;
			this.transitionColor = true;
		}

		public void ClearTargetColors(float duration)
		{
			this.targetFogColors = false;
			this.SetTargetColors(this.color, this.color2, duration);
		}

		private void UpdateFogOfWarTexture()
		{
			if (!this.fogOfWarEnabled)
			{
				return;
			}
			int scaledSize = this.GetScaledSize(this.fogOfWarTextureSize, 1f);
			this.fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.ARGB32, false);
			this.fogOfWarTexture.hideFlags = HideFlags.DontSave;
			this.fogOfWarTexture.filterMode = FilterMode.Bilinear;
			this.fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
			this.ResetFogOfWar();
		}

		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
		{
			if (this.fogOfWarTexture == null)
			{
				return;
			}
			float num = (worldPosition.x - this.fogOfWarCenter.x) / this.fogOfWarSize.x + 0.5f;
			if (num < 0f || num > 1f)
			{
				return;
			}
			float num2 = (worldPosition.z - this.fogOfWarCenter.z) / this.fogOfWarSize.z + 0.5f;
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
				float num6 = radius / this.fogOfWarSize.z;
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
									color2.a = (byte)Mathf.Lerp((float)b, (float)color2.a, (float)num8 / (float)num7);
									this.fogOfWarColorBuffer[num5] = color2;
									this.fogOfWarTexture.SetPixel(j, i, color2);
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
			float num = (worldPosition.x - this.fogOfWarCenter.x) / this.fogOfWarSize.x + 0.5f;
			if (num < 0f || num > 1f)
			{
				return;
			}
			float num2 = (worldPosition.z - this.fogOfWarCenter.z) / this.fogOfWarSize.z + 0.5f;
			if (num2 < 0f || num2 > 1f)
			{
				return;
			}
			int width = this.fogOfWarTexture.width;
			int height = this.fogOfWarTexture.height;
			int num3 = (int)(num * (float)width);
			int num4 = (int)(num2 * (float)height);
			int num5 = num4 * width + num3;
			float num6 = radius / this.fogOfWarSize.z;
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
			if (this.fogOfWarTexture == null)
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

		[SerializeField]
		private FOG_TYPE _effectType = FOG_TYPE.DesktopFogPlusWithSkyHaze;

		[SerializeField]
		private FOG_PRESET _preset = FOG_PRESET.Mist;

		[SerializeField]
		private DynamicFogProfile _profile;

		[SerializeField]
		private bool _useFogVolumes;

		[SerializeField]
		private bool _enableDithering;

		[Range(0f, 0.2f)]
		[SerializeField]
		private float _ditherStrength = 0.03f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _alpha = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _noiseStrength = 0.5f;

		[Range(0.01f, 1f)]
		[SerializeField]
		private float _noiseScale = 0.1f;

		[Range(0f, 0.999f)]
		[SerializeField]
		private float _distance = 0.1f;

		[Range(0.0001f, 2f)]
		[SerializeField]
		private float _distanceFallOff = 0.01f;

		[Range(0f, 1.2f)]
		[SerializeField]
		private float _maxDistance = 0.999f;

		[Range(0.0001f, 0.5f)]
		[SerializeField]
		private float _maxDistanceFallOff;

		[Range(0f, 500f)]
		[SerializeField]
		private float _height = 1f;

		[Range(0f, 500f)]
		[SerializeField]
		private float _maxHeight = 100f;

		[Range(0.0001f, 1f)]
		[SerializeField]
		private float _heightFallOff = 0.1f;

		[SerializeField]
		private float _baselineHeight;

		[SerializeField]
		private bool _clipUnderBaseline;

		[Range(0f, 15f)]
		[SerializeField]
		private float _turbulence = 0.1f;

		[Range(0f, 5f)]
		[SerializeField]
		private float _speed = 0.1f;

		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private Color _color2 = Color.gray;

		[Range(0f, 500f)]
		[SerializeField]
		private float _skyHaze = 50f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _skySpeed = 0.3f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _skyNoiseStrength = 0.1f;

		[Range(0f, 1f)]
		[SerializeField]
		private float _skyAlpha = 1f;

		[SerializeField]
		private GameObject _sun;

		[SerializeField]
		private bool _fogOfWarEnabled;

		[SerializeField]
		private Vector3 _fogOfWarCenter;

		[SerializeField]
		private Vector3 _fogOfWarSize = new Vector3(1024f, 0f, 1024f);

		[SerializeField]
		private int _fogOfWarTextureSize = 256;

		[SerializeField]
		private bool _useSinglePassStereoRenderingMatrix;

		[SerializeField]
		private bool _useXZDistance;

		[SerializeField]
		[Range(0f, 1f)]
		private float _scattering = 0.7f;

		[SerializeField]
		private Color _scatteringColor = new Color(1f, 1f, 0.8f);

		private Material fogMatAdv;

		private Material fogMatFogSky;

		private Material fogMatOnlyFog;

		private Material fogMatVol;

		private Material fogMatSimple;

		private Material fogMatBasic;

		private Material fogMatOrthogonal;

		private Material fogMatDesktopPlusOrthogonal;

		[SerializeField]
		private Material fogMat;

		private float initialFogAlpha;

		private float targetFogAlpha;

		private float initialSkyHazeAlpha;

		private float targetSkyHazeAlpha;

		private bool targetFogColors;

		private Color initialFogColor1;

		private Color targetFogColor1;

		private Color initialFogColor2;

		private Color targetFogColor2;

		private float transitionDuration;

		private float transitionStartTime;

		private float currentFogAlpha;

		private float currentSkyHazeAlpha;

		private bool transitionAlpha;

		private bool transitionColor;

		private bool transitionProfile;

		private DynamicFogProfile initialProfile;

		private DynamicFogProfile targetProfile;

		private Color currentFogColor1;

		private Color currentFogColor2;

		private Camera currentCamera;

		private Texture2D fogOfWarTexture;

		private Color32[] fogOfWarColorBuffer;

		private Light sunLight;

		private Vector3 sunDirection = Vector3.zero;

		private Color sunColor = Color.white;

		private float sunIntensity = 1f;

		private static DynamicFog _fog;

		private List<string> shaderKeywords;

		private bool matOrtho;

		private bool shouldUpdateMaterialProperties;
	}
}
