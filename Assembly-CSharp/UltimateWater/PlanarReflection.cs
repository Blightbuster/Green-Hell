using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	public sealed class PlanarReflection : WaterModule
	{
		public PlanarReflection(Water water, PlanarReflection.Data data)
		{
			this._Water = water;
			this._Data = data;
			this._SystemSupportsHdr = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
			this.Validate();
			water.ProfilesManager.Changed.AddListener(new UnityAction<Water>(this.OnProfilesChanged));
			this.OnProfilesChanged(water);
		}

		public float Resolution
		{
			get
			{
				return this._Data.Resolution;
			}
			set
			{
				this._Data.Resolution = value;
				this.CalculateResolutionMultiplier();
			}
		}

		public float RetinaResolution
		{
			get
			{
				return this._Data.RetinaResolution;
			}
			set
			{
				this._Data.RetinaResolution = value;
				this.CalculateResolutionMultiplier();
			}
		}

		public bool ReflectSkybox
		{
			get
			{
				return this._Data.ReflectSkybox;
			}
			set
			{
				this._Data.ReflectSkybox = value;
			}
		}

		public bool RenderShadows
		{
			get
			{
				return this._Data.RenderShadows;
			}
			set
			{
				this._Data.RenderShadows = value;
			}
		}

		public LayerMask ReflectionMask
		{
			get
			{
				return this._Data.ReflectionMask;
			}
			set
			{
				this._Data.ReflectionMask = value;
			}
		}

		internal override void OnWaterPostRender(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			TemporaryRenderTexture temporaryRenderTexture;
			if (this._TemporaryTargets.TryGetValue(cameraComponent, out temporaryRenderTexture))
			{
				this._TemporaryTargets.Remove(cameraComponent);
				temporaryRenderTexture.Dispose();
			}
		}

		internal override void Start(Water water)
		{
		}

		internal override void Enable()
		{
		}

		internal override void Disable()
		{
		}

		internal override void Validate()
		{
			if (this._UtilitiesShader == null)
			{
				this._UtilitiesShader = Shader.Find("UltimateWater/Utilities/PlanarReflection - Utilities");
			}
			this._Data.Resolution = Mathf.Clamp01((float)Mathf.RoundToInt(this._Data.Resolution * 10f) * 0.1f);
			this._Data.RetinaResolution = Mathf.Clamp01((float)Mathf.RoundToInt(this._Data.RetinaResolution * 10f) * 0.1f);
			this.CalculateResolutionMultiplier();
		}

		internal override void Destroy()
		{
			this.ClearRenderTextures();
		}

		internal override void Update()
		{
			this.ClearRenderTextures();
		}

		internal override void OnWaterRender(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			if (!cameraComponent.enabled || !this._RenderPlanarReflections)
			{
				return;
			}
			if (!this._TemporaryTargets.TryGetValue(cameraComponent, out this._CurrentTarget))
			{
				Camera reflectionCamera = Reflection.GetReflectionCamera(cameraComponent);
				this.RenderReflection(cameraComponent, reflectionCamera);
				this.UpdateRenderProperties(reflectionCamera);
			}
		}

		private void CalculateResolutionMultiplier()
		{
			float num = (Screen.dpi > 220f) ? this._Data.RetinaResolution : this._Data.Resolution;
			if (this._FinalResolutionMultiplier != num)
			{
				this._FinalResolutionMultiplier = num;
				this.ClearRenderTextures();
			}
		}

		private void RenderReflection(Camera camera, Camera reflectionCamera)
		{
			reflectionCamera.cullingMask = this._Data.ReflectionMask;
			this.SetCameraSettings(camera, reflectionCamera);
			this._CurrentTarget = this.GetRenderTexture(camera.pixelWidth, camera.pixelHeight, reflectionCamera);
			this._TemporaryTargets[camera] = this._CurrentTarget;
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(this._CurrentTarget.Texture.width, this._CurrentTarget.Texture.height, 16, this._CurrentTarget.Texture.format, true, false, false);
			reflectionCamera.targetTexture = temporary;
			reflectionCamera.transform.eulerAngles = PlanarReflection.CalculateReflectionAngles(camera);
			reflectionCamera.transform.position = this.CalculateReflectionPosition(camera);
			float w = -this._Water.transform.position.y - 0.07f;
			Vector4 plane = new Vector4(0f, 1f, 0f, w);
			Matrix4x4 matrix4x = Matrix4x4.zero;
			matrix4x = Reflection.CalculateReflectionMatrix(matrix4x, plane);
			Vector3 position = matrix4x.MultiplyPoint(camera.transform.position);
			reflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix * matrix4x;
			Vector4 clipPlane = Reflection.CameraSpacePlane(reflectionCamera, this._Water.transform.position, new Vector3(0f, 1f, 0f), 0.07f, 1f);
			Matrix4x4 matrix4x2 = camera.projectionMatrix;
			matrix4x2 = Reflection.CalculateObliqueMatrix(matrix4x2, clipPlane);
			reflectionCamera.projectionMatrix = matrix4x2;
			reflectionCamera.transform.position = position;
			Vector3 eulerAngles = camera.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
			reflectionCamera.clearFlags = ((!this._Data.ReflectSkybox) ? CameraClearFlags.Color : CameraClearFlags.Skybox);
			if (this._Data.RenderShadows)
			{
				GL.invertCulling = true;
				reflectionCamera.Render();
				GL.invertCulling = false;
			}
			else
			{
				ShadowQuality shadows = QualitySettings.shadows;
				QualitySettings.shadows = ShadowQuality.Disable;
				GL.invertCulling = true;
				reflectionCamera.Render();
				GL.invertCulling = false;
				QualitySettings.shadows = shadows;
			}
			reflectionCamera.targetTexture = null;
			if (this._UtilitiesMaterial == null)
			{
				this._UtilitiesMaterial = new Material(this._UtilitiesShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			Graphics.Blit(temporary, this._CurrentTarget, this._UtilitiesMaterial, 0);
			temporary.Dispose();
		}

		private void UpdateRenderProperties(Camera reflectionCamera)
		{
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			propertyBlock.SetTexture(ShaderVariables.PlanarReflectionTex, this._CurrentTarget);
			propertyBlock.SetMatrix("_PlanarReflectionProj", Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(0.5f, 0.5f, 1f)) * reflectionCamera.projectionMatrix * reflectionCamera.worldToCameraMatrix);
			propertyBlock.SetFloat("_PlanarReflectionMipBias", -Mathf.Log(1f / this._FinalResolutionMultiplier, 2f));
		}

		private TemporaryRenderTexture GetRenderTexture(int width, int height, Camera reflectionCamera)
		{
			int width2 = Mathf.ClosestPowerOfTwo(Mathf.RoundToInt((float)width * this._FinalResolutionMultiplier));
			int height2 = Mathf.ClosestPowerOfTwo(Mathf.RoundToInt((float)height * this._FinalResolutionMultiplier));
			bool allowHDR = reflectionCamera.allowHDR;
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(width2, height2, 0, (!allowHDR || !this._SystemSupportsHdr || !WaterProjectSettings.Instance.AllowFloatingPointMipMaps) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGBHalf, true, false, true);
			temporary.Texture.filterMode = FilterMode.Trilinear;
			temporary.Texture.wrapMode = TextureWrapMode.Clamp;
			return temporary;
		}

		private void ClearRenderTextures()
		{
			Dictionary<Camera, TemporaryRenderTexture>.Enumerator enumerator = this._TemporaryTargets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<Camera, TemporaryRenderTexture> keyValuePair = enumerator.Current;
				keyValuePair.Value.Dispose();
			}
			enumerator.Dispose();
			this._TemporaryTargets.Clear();
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.ProfilesManager.Profiles;
			if (profiles == null)
			{
				return;
			}
			float num = 0f;
			for (int i = profiles.Length - 1; i >= 0; i--)
			{
				Water.WeightedProfile weightedProfile = profiles[i];
				WaterProfileData profile = weightedProfile.Profile;
				float weight = weightedProfile.Weight;
				num += profile.PlanarReflectionIntensity * weight;
			}
			this._RenderPlanarReflections = (num > 0f);
		}

		private void SetCameraSettings(Camera source, Camera destination)
		{
			destination.backgroundColor = new Color(0f, 0f, 0f, 0f);
			destination.fieldOfView = source.fieldOfView;
			destination.aspect = source.aspect;
			destination.allowHDR = (this._SystemSupportsHdr && source.allowHDR);
		}

		private Vector3 CalculateReflectionPosition(Camera camera)
		{
			Vector3 position = camera.transform.position;
			position.y = this._Water.transform.position.y - position.y;
			return position;
		}

		private static Vector3 CalculateReflectionAngles(Camera camera)
		{
			Vector3 eulerAngles = camera.transform.eulerAngles;
			return new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
		}

		private readonly PlanarReflection.Data _Data;

		private readonly Water _Water;

		private readonly bool _SystemSupportsHdr;

		private readonly Dictionary<Camera, TemporaryRenderTexture> _TemporaryTargets = new Dictionary<Camera, TemporaryRenderTexture>();

		private TemporaryRenderTexture _CurrentTarget;

		private float _FinalResolutionMultiplier;

		private bool _RenderPlanarReflections;

		private Material _UtilitiesMaterial;

		private Shader _UtilitiesShader;

		private const float _ClipPlaneOffset = 0.07f;

		[Serializable]
		public class Data
		{
			public LayerMask ReflectionMask = int.MaxValue;

			public bool ReflectSkybox = true;

			public bool RenderShadows = true;

			[Range(0f, 1f)]
			public float Resolution = 0.5f;

			[Tooltip("Allows you to use more rational resolution of planar reflections on screens with very high dpi. Planar reflections should be blurred anyway.")]
			[Range(0f, 1f)]
			public float RetinaResolution = 0.333f;
		}
	}
}
