using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Ultimate Water/Water Camera", -1)]
	[ExecuteInEditMode]
	public class WaterCamera : MonoBehaviour
	{
		public bool RenderWaterDepth
		{
			get
			{
				return this._RenderWaterDepth;
			}
			set
			{
				this._RenderWaterDepth = value;
			}
		}

		public bool RenderVolumes
		{
			get
			{
				return this._RenderVolumes;
			}
			set
			{
				this._RenderVolumes = value;
			}
		}

		public float BaseEffectsQuality
		{
			get
			{
				return this._BaseEffectsQuality;
			}
			set
			{
				this._BaseEffectsQuality = value;
			}
		}

		public WaterCamera.CameraType Type
		{
			get
			{
				return this._WaterCameraType;
			}
			set
			{
				this._WaterCameraType = value;
			}
		}

		public WaterGeometryType GeometryType
		{
			get
			{
				return this._GeometryType;
			}
			set
			{
				this._GeometryType = value;
			}
		}

		public Rect LocalMapsRect
		{
			get
			{
				return this._LocalMapsRect;
			}
		}

		public WaterRenderMode RenderMode
		{
			get
			{
				return this._RenderMode;
			}
			set
			{
				this._RenderMode = value;
				this.OnDisable();
				this.OnEnable();
			}
		}

		public Rect LocalMapsRectPrevious
		{
			get
			{
				return this._LocalMapsRectPrevious;
			}
		}

		public Vector4 LocalMapsShaderCoords
		{
			get
			{
				float num = 1f / this._LocalMapsRect.width;
				return new Vector4(-this._LocalMapsRect.xMin * num, -this._LocalMapsRect.yMin * num, num, this._LocalMapsRect.width);
			}
		}

		public int ForcedVertexCount
		{
			get
			{
				return this._ForcedVertexCount;
			}
			set
			{
				this._ForcedVertexCount = value;
			}
		}

		public Water ContainingWater
		{
			get
			{
				return (!(this._BaseCamera == null)) ? this._BaseCamera.ContainingWater : ((this._SubmersionState == SubmersionState.None) ? null : this._ContainingWater);
			}
		}

		public float WaterLevel
		{
			get
			{
				return this._WaterLevel;
			}
		}

		public SubmersionState SubmersionState
		{
			get
			{
				return this._SubmersionState;
			}
		}

		public Camera MainCamera
		{
			get
			{
				return this._MainCamera;
			}
		}

		public Camera CameraComponent
		{
			get
			{
				return this._CameraComponent;
			}
		}

		public Water MainWater
		{
			get
			{
				if (this._MainWater != null)
				{
					return this._MainWater;
				}
				List<Water> boundlessWaters = ApplicationSingleton<WaterSystem>.Instance.BoundlessWaters;
				if (boundlessWaters.Count != 0)
				{
					return boundlessWaters[0];
				}
				List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
				if (list.Count != 0)
				{
					return list[0];
				}
				return null;
			}
		}

		public static List<WaterCamera> EnabledWaterCameras
		{
			get
			{
				return WaterCamera._EnabledWaterCameras;
			}
		}

		public int BaseEffectWidth
		{
			get
			{
				return this._PixelWidth;
			}
		}

		public int BaseEffectHeight
		{
			get
			{
				return this._PixelHeight;
			}
		}

		public bool RenderFlatMasks
		{
			get
			{
				return this._RenderFlatMasks;
			}
		}

		public bool IsInsideSubtractiveVolume
		{
			get
			{
				return this._IsInsideSubtractiveVolume;
			}
		}

		public Camera EffectsCamera
		{
			get
			{
				if (this._WaterCameraType == WaterCamera.CameraType.Normal && this._EffectCamera == null)
				{
					this._EffectCamera = this.CreateEffectsCamera(WaterCamera.CameraType.Effect);
				}
				return this._EffectCamera;
			}
		}

		public Camera PlaneProjectorCamera
		{
			get
			{
				if (this._WaterCameraType == WaterCamera.CameraType.Normal && this._PlaneProjectorCamera == null)
				{
					this._PlaneProjectorCamera = this.CreateEffectsCamera(WaterCamera.CameraType.Effect);
				}
				return this._PlaneProjectorCamera;
			}
		}

		public WaterCamera.WaterCameraEvent SubmersionStateChanged
		{
			get
			{
				return (this._SubmersionStateChanged == null) ? (this._SubmersionStateChanged = new WaterCamera.WaterCameraEvent()) : this._SubmersionStateChanged;
			}
		}

		public bool IsInsideAdditiveVolume
		{
			get
			{
				return this._IsInsideAdditiveVolume;
			}
		}

		public LightWaterEffects EffectsLight
		{
			get
			{
				return this._EffectsLight;
			}
			set
			{
				this._EffectsLight = value;
			}
		}

		public WaterCameraSubmersion CameraSubmersion
		{
			get
			{
				return this._CameraSubmersion;
			}
		}

		public void SetCustomWaterRenderList(List<Water> waters)
		{
			this._CustomWaterRenderList = waters;
		}

		public static WaterCamera GetWaterCamera(Camera camera, bool forceAdd = false)
		{
			WaterCamera waterCamera;
			if (!WaterCamera._WaterCamerasCache.TryGetValue(camera, out waterCamera))
			{
				waterCamera = camera.GetComponent<WaterCamera>();
				if (waterCamera != null)
				{
					WaterCamera._WaterCamerasCache[camera] = waterCamera;
				}
				else if (forceAdd)
				{
					WaterCamera._WaterCamerasCache[camera] = camera.gameObject.AddComponent<WaterCamera>();
				}
				else
				{
					waterCamera = (WaterCamera._WaterCamerasCache[camera] = null);
				}
			}
			return waterCamera;
		}

		protected Material _DepthBlitCopyMaterial
		{
			get
			{
				return (!(this._DepthBlitCopyMaterialCache != null)) ? (this._DepthBlitCopyMaterialCache = new Material(this._DepthBlitCopyShader)
				{
					hideFlags = HideFlags.DontSave
				}) : this._DepthBlitCopyMaterialCache;
			}
		}

		private static CommandBuffer _UtilityCommandBuffer
		{
			get
			{
				return (WaterCamera._UtilityCommandBufferCache == null) ? (WaterCamera._UtilityCommandBufferCache = new CommandBuffer
				{
					name = "[PW Water] WaterCamera Utility"
				}) : WaterCamera._UtilityCommandBufferCache;
			}
		}

		internal Camera _WaterRenderCamera
		{
			get
			{
				return (!(this._WaterRenderCameraCache != null)) ? (this._WaterRenderCameraCache = this.CreateEffectsCamera(WaterCamera.CameraType.RenderHelper)) : this._WaterRenderCameraCache;
			}
		}

		public static event Action<WaterCamera> OnGlobalPreCull;

		public static event Action<WaterCamera> OnGlobalPostRender;

		public event Action<WaterCamera> RenderTargetResized;

		public event Action<WaterCamera> Destroyed;

		public event Action<WaterCamera> Disabled;

		protected void Awake()
		{
			this.OnValidate();
			if (SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) && this._DepthBlitCopyMaterial.passCount > 3)
			{
				this._BlendedDepthTexturesFormat = RenderTextureFormat.Depth;
			}
			else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) && this._BaseEffectsQuality > 0.2f)
			{
				this._BlendedDepthTexturesFormat = RenderTextureFormat.RFloat;
			}
			else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf))
			{
				this._BlendedDepthTexturesFormat = RenderTextureFormat.RHalf;
			}
			else
			{
				this._BlendedDepthTexturesFormat = RenderTextureFormat.R8;
			}
			this._Gbuffer0MixMaterial = new Material(this._Gbuffer0MixShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._Gbuffer123MixMaterial = new Material(this._Gbuffer123MixShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._FinalColorMixMaterial = new Material(this._FinalColorMixShader)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		protected void OnEnable()
		{
			this._CameraComponent = base.GetComponent<Camera>();
			WaterCamera._WaterCamerasCache[this._CameraComponent] = this;
			if (this._WaterCameraType == WaterCamera.CameraType.Normal)
			{
				WaterCamera._EnabledWaterCameras.Add(this);
				this._ImageEffects = base.GetComponents<IWaterImageEffect>();
				foreach (IWaterImageEffect waterImageEffect in this._ImageEffects)
				{
					waterImageEffect.OnWaterCameraEnabled();
				}
			}
			this.RemoveUtilityCommands();
			this.AddUtilityCommands();
			this._MaskModule.OnEnable(this);
			this._DepthModule.OnEnable(this);
			this._ForwardModule.OnEnable(this);
			this._CameraSubmersion.OnEnable(this);
		}

		protected void OnDisable()
		{
			if (this._WaterCameraType == WaterCamera.CameraType.Normal)
			{
				WaterCamera._EnabledWaterCameras.Remove(this);
			}
			this.ReleaseImageEffectTemporaryTextures();
			this.ReleaseTemporaryTextures();
			this.RemoveUtilityCommands();
			this.DisableEffects();
			if (this._EffectCamera != null)
			{
				this._EffectCamera.gameObject.Destroy();
				this._EffectCamera = null;
			}
			if (this._PlaneProjectorCamera != null)
			{
				this._PlaneProjectorCamera.gameObject.Destroy();
				this._PlaneProjectorCamera = null;
			}
			if (this._DepthBlitCopyMaterialCache != null)
			{
				this._DepthBlitCopyMaterialCache.Destroy();
				this._DepthBlitCopyMaterialCache = null;
			}
			this._ContainingWater = null;
			this._MaskModule.OnDisable(this);
			this._DepthModule.OnDisable(this);
			this._ForwardModule.OnDisable(this);
			this._CameraSubmersion.OnDisable();
			if (this.Disabled != null)
			{
				this.Disabled(this);
			}
		}

		protected void OnPreCull()
		{
			if (!this.ShouldRender())
			{
				return;
			}
			if (WaterCamera.OnGlobalPreCull != null)
			{
				WaterCamera.OnGlobalPreCull(this);
			}
			if (this._WaterCameraType == WaterCamera.CameraType.RenderHelper)
			{
				WaterCamera component = this._MainCamera.GetComponent<WaterCamera>();
				if (component != null)
				{
					component.RenderWaterDirect();
				}
				return;
			}
			if (this._WaterCameraType == WaterCamera.CameraType.Normal)
			{
				bool flipY = WaterUtilities.IsSceneViewCamera(this._CameraComponent) || (VersionCompatibility.Version >= 560 && this._RenderMode == WaterRenderMode.DefaultQueue && this._CameraComponent.allowHDR);
				this.SetPlaneProjectorMatrix(this._RenderMode != WaterRenderMode.DefaultQueue, flipY);
				this.ToggleEffects();
				this.PrepareToRender();
				WaterCamera.SetFallbackTextures();
			}
			if (this._EffectsEnabled)
			{
				this.SetLocalMapCoordinates();
			}
			this.RenderWaterEffects();
			if (this._EffectsLight != null)
			{
				this._EffectsLight.PrepareRenderingOnCamera(this);
			}
			if (this._RenderMode == WaterRenderMode.DefaultQueue)
			{
				this.RenderWaterDirect();
			}
			if (!this._EffectsEnabled)
			{
				Shader.DisableKeyword("WATER_BUFFERS_ENABLED");
				return;
			}
			Shader.EnableKeyword("WATER_BUFFERS_ENABLED");
			if (this._RenderVolumes)
			{
				this._MaskModule.Process(this);
			}
			if (this._RenderWaterDepth || this._RenderMode == WaterRenderMode.ImageEffectDeferred)
			{
				this._DepthModule.Process(this);
			}
			if (this._ImageEffects != null && Application.isPlaying)
			{
				for (int i = 0; i < this._ImageEffects.Length; i++)
				{
					this._ImageEffects[i].OnWaterCameraPreCull();
				}
			}
			if (this._ShadowedWaterRect.xMin < this._ShadowedWaterRect.xMax)
			{
				this.RenderShadowEnforcers();
			}
			if (this._RenderMode != WaterRenderMode.DefaultQueue)
			{
				List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
				for (int j = list.Count - 1; j >= 0; j--)
				{
					list[j].Volume.DisableRenderers();
				}
				WaterMaterials.ValidateGlobalWaterDataLookupTex();
			}
		}

		protected void OnPostRender()
		{
			if (!this.ShouldRender())
			{
				return;
			}
			this.ReleaseTemporaryTextures();
			List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].Renderer.PostRender(this);
			}
			if (this._EffectsLight != null)
			{
				this._EffectsLight.CleanRenderingOnCamera();
			}
			if (WaterCamera.OnGlobalPostRender != null)
			{
				WaterCamera.OnGlobalPostRender(this);
			}
		}

		protected void Update()
		{
			if (!this.ShouldRender())
			{
				return;
			}
			WaterRenderMode renderMode = this._RenderMode;
			if (renderMode == WaterRenderMode.ImageEffectDeferred)
			{
				this.ReleaseImageEffectTemporaryTextures();
			}
		}

		private void OnDestroy()
		{
			WaterCamera._WaterCamerasCache.Remove(base.GetComponent<Camera>());
			if (this.Destroyed != null)
			{
				this.Destroyed(this);
				this.Destroyed = null;
			}
		}

		private void OnValidate()
		{
			this._CameraSubmersion.OnValidate();
			this._MaskModule.OnValidate(this);
			this._DepthModule.OnValidate(this);
			this._ForwardModule.OnValidate(this);
			if (this._DepthBlitCopyShader == null)
			{
				this._DepthBlitCopyShader = Shader.Find("UltimateWater/Depth/Depth Copy");
			}
			if (this._ShadowEnforcerShader == null)
			{
				this._ShadowEnforcerShader = Shader.Find("UltimateWater/Utility/ShadowEnforcer");
			}
			if (this._Gbuffer0MixShader == null)
			{
				this._Gbuffer0MixShader = Shader.Find("UltimateWater/Deferred/GBuffer0Mix");
			}
			if (this._Gbuffer123MixShader == null)
			{
				this._Gbuffer123MixShader = Shader.Find("UltimateWater/Deferred/GBuffer123Mix");
			}
			if (this._FinalColorMixShader == null)
			{
				this._FinalColorMixShader = Shader.Find("UltimateWater/Deferred/FinalColorMix");
			}
			if (this._DeferredReflections == null)
			{
				this._DeferredReflections = Shader.Find("Hidden/UltimateWater-Internal-DeferredReflections");
			}
			if (this._DeferredShading == null)
			{
				this._DeferredShading = Shader.Find("Hidden/UltimateWater-Internal-DeferredShading");
			}
			if (this._MergeDisplacementsShader == null)
			{
				this._MergeDisplacementsShader = Shader.Find("UltimateWater/Utility/MergeDisplacements");
			}
			if (this._WaterCameraIME == null)
			{
				this._WaterCameraIME = base.GetComponent<WaterCameraIME>();
				if (this._WaterCameraIME == null)
				{
					this._WaterCameraIME = base.gameObject.AddComponent<WaterCameraIME>();
				}
			}
			if (this._RenderMode == WaterRenderMode.ImageEffectDeferred)
			{
				this._RenderWaterDepth = true;
			}
			this._CameraComponent = base.GetComponent<Camera>();
			this.RemoveUtilityCommands();
			if (base.enabled)
			{
				this.AddUtilityCommands();
			}
			this._WaterCameraIME.enabled = (base.enabled && (this._RenderMode == WaterRenderMode.ImageEffectDeferred || this._RenderMode == WaterRenderMode.ImageEffectForward));
		}

		private void OnDrawGizmosSelected()
		{
			if (this._EditSubmersion)
			{
				this._CameraSubmersion.OnDrawGizmos();
			}
		}

		internal void ReportShadowedWaterMinMaxRect(Vector2 min, Vector2 max)
		{
			if (this._ShadowedWaterRect.xMin > min.x)
			{
				this._ShadowedWaterRect.xMin = min.x;
			}
			if (this._ShadowedWaterRect.yMin > min.y)
			{
				this._ShadowedWaterRect.yMin = min.y;
			}
			if (this._ShadowedWaterRect.xMax < max.x)
			{
				this._ShadowedWaterRect.xMax = max.x;
			}
			if (this._ShadowedWaterRect.yMax < max.y)
			{
				this._ShadowedWaterRect.yMax = max.y;
			}
		}

		internal void RenderWaterWithShader(string commandName, RenderTexture target, Shader shader, Water water)
		{
			CommandBuffer utilityCommandBuffer = WaterCamera._UtilityCommandBuffer;
			utilityCommandBuffer.Clear();
			utilityCommandBuffer.SetRenderTarget(target);
			water.Renderer.Render(this._CameraComponent, this._GeometryType, utilityCommandBuffer, shader);
			GL.PushMatrix();
			GL.modelview = this._CameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(this._CameraComponent.projectionMatrix);
			Graphics.ExecuteCommandBuffer(utilityCommandBuffer);
			GL.PopMatrix();
		}

		internal void RenderWaterWithShader(string commandName, RenderTexture target, Shader shader, bool surfaces, bool volumes, bool volumesTwoPass)
		{
			CommandBuffer utilityCommandBuffer = WaterCamera._UtilityCommandBuffer;
			utilityCommandBuffer.Clear();
			utilityCommandBuffer.SetRenderTarget(target);
			this.AddWaterRenderCommands(utilityCommandBuffer, shader, surfaces, volumes, volumesTwoPass);
			GL.PushMatrix();
			GL.modelview = this._CameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(this._CameraComponent.projectionMatrix);
			Graphics.ExecuteCommandBuffer(utilityCommandBuffer);
			GL.PopMatrix();
		}

		internal void AddWaterRenderCommands(CommandBuffer commandBuffer, Shader shader, bool surfaces, bool volumes, bool volumesTwoPass)
		{
			List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
			if (volumes)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					list[i].Renderer.RenderVolumes(commandBuffer, shader, volumesTwoPass);
				}
			}
			if (surfaces)
			{
				for (int j = list.Count - 1; j >= 0; j--)
				{
					list[j].Renderer.Render(this._CameraComponent, this._GeometryType, commandBuffer, shader);
				}
			}
		}

		internal void AddWaterMasksRenderCommands(CommandBuffer commandBuffer)
		{
			List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].Renderer.RenderMasks(commandBuffer);
			}
		}

		private void ReleaseTemporaryTextures()
		{
			if (this._Gbuffer0Tex != null)
			{
				RenderTexture.ReleaseTemporary(this._Gbuffer0Tex);
				this._Gbuffer0Tex = null;
			}
			if (this._DepthTex2 != null)
			{
				RenderTexture.ReleaseTemporary(this._DepthTex2);
				this._DepthTex2 = null;
			}
		}

		private void CopyFrom(WaterCamera waterCamera)
		{
			this._LocalMapsRect = waterCamera._LocalMapsRect;
			this._LocalMapsRectPrevious = waterCamera._LocalMapsRectPrevious;
			this._GeometryType = waterCamera._GeometryType;
		}

		private void RenderWaterDirect()
		{
			List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].Renderer.Render(this._CameraComponent, this._GeometryType, null, null);
			}
		}

		private void RenderWaterEffects()
		{
			List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].Renderer.RenderEffects(this);
			}
		}

		internal void OnRenderImageCallback(RenderTexture source, RenderTexture destination)
		{
			if (!this.ShouldRender())
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (this._RenderMode != WaterRenderMode.DefaultQueue)
			{
				List<Water> list = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					list[i].Volume.EnableRenderers(false);
				}
			}
			WaterRenderMode renderMode = this._RenderMode;
			if (renderMode != WaterRenderMode.ImageEffectForward)
			{
				if (renderMode != WaterRenderMode.ImageEffectDeferred)
				{
					Graphics.Blit(source, destination);
				}
				else
				{
					RenderTexture temporary = RenderTexture.GetTemporary(Mathf.RoundToInt((float)this._CameraComponent.pixelWidth * this._SuperSampling) + 1, Mathf.RoundToInt((float)this._CameraComponent.pixelHeight * this._SuperSampling), 32, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
					temporary.filterMode = FilterMode.Point;
					source.filterMode = FilterMode.Point;
					Shader.SetGlobalTexture(ShaderVariables.RefractTex, source);
					this.RenderWaterDeferred(temporary, destination);
					RenderTexture.ReleaseTemporary(temporary);
				}
			}
			else
			{
				this._ForwardModule.Render(this, source, destination);
			}
			if (this._RenderMode != WaterRenderMode.DefaultQueue)
			{
				List<Water> list2 = (this._CustomWaterRenderList == null) ? ApplicationSingleton<WaterSystem>.Instance.Waters : this._CustomWaterRenderList;
				for (int j = list2.Count - 1; j >= 0; j--)
				{
					list2[j].Volume.DisableRenderers();
				}
			}
		}

		private void ReleaseImageEffectTemporaryTextures()
		{
			if (this._DeferredRenderTarget != null)
			{
				RenderTexture.ReleaseTemporary(this._DeferredRenderTarget);
				this._DeferredRenderTarget = null;
			}
		}

		private void AddUtilityCommands()
		{
			if (this._ImageEffectCommands == null && this._RenderMode == WaterRenderMode.ImageEffectDeferred)
			{
				this._ImageEffectCommands = new CommandBuffer
				{
					name = "[PW Water] Set Buffers"
				};
				this._ImageEffectCommands.SetGlobalTexture(ShaderVariables.Gbuffer0, BuiltinRenderTextureType.GBuffer0);
				this._ImageEffectCommands.SetGlobalTexture(ShaderVariables.Gbuffer1, BuiltinRenderTextureType.GBuffer1);
				this._ImageEffectCommands.SetGlobalTexture(ShaderVariables.Gbuffer2, BuiltinRenderTextureType.GBuffer2);
				this._ImageEffectCommands.SetGlobalTexture(ShaderVariables.Gbuffer3, BuiltinRenderTextureType.Reflections);
				this._ImageEffectCommands.SetGlobalTexture(ShaderVariables.WaterlessDepthTexture, BuiltinRenderTextureType.ResolvedDepth);
				this._CameraComponent.RemoveCommandBuffer(CameraEvent.AfterLighting, this._ImageEffectCommands);
				this._CameraComponent.AddCommandBuffer(CameraEvent.AfterLighting, this._ImageEffectCommands);
			}
		}

		private void RemoveUtilityCommands()
		{
			this.RemoveCommandBuffer(CameraEvent.AfterLighting, "[PW Water] Set Buffers");
			if (this._ImageEffectCommands != null)
			{
				this._ImageEffectCommands.Dispose();
				this._ImageEffectCommands = null;
			}
		}

		private void RemoveCommandBuffer(CameraEvent cameraEvent, string bufferName)
		{
			CommandBuffer[] commandBuffers = this._CameraComponent.GetCommandBuffers(cameraEvent);
			for (int i = commandBuffers.Length - 1; i >= 0; i--)
			{
				if (commandBuffers[i].name == bufferName)
				{
					this._CameraComponent.RemoveCommandBuffer(cameraEvent, commandBuffers[i]);
					return;
				}
			}
		}

		private void RenderWaterDeferred(RenderTexture temp, RenderTexture target)
		{
			Camera waterRenderCamera = this._WaterRenderCamera;
			waterRenderCamera.CopyFrom(this._CameraComponent);
			Water mainWater = this.MainWater;
			if (this._RenderMode == WaterRenderMode.ImageEffectDeferred)
			{
				this._FinalColorMixMaterial.SetMatrix(ShaderVariables.UnityMatrixVPInverse, Matrix4x4.Inverse(GL.GetGPUProjectionMatrix(this._CameraComponent.projectionMatrix, true) * this._CameraComponent.worldToCameraMatrix));
				this._Gbuffer123MixMaterial.SetFloat(ShaderVariables.DepthClipMultiplier, -1f);
				if (this._Gbuffer0Tex == null)
				{
					this._Gbuffer0Tex = RenderTexture.GetTemporary(this._CameraComponent.pixelWidth + 1, this._CameraComponent.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
					this._Gbuffer0Tex.filterMode = FilterMode.Point;
				}
				if (this._DepthTex2 == null)
				{
					this._DepthTex2 = RenderTexture.GetTemporary(this._CameraComponent.pixelWidth + 1, this._CameraComponent.pixelHeight, (this._BlendedDepthTexturesFormat != RenderTextureFormat.Depth) ? 0 : 32, this._BlendedDepthTexturesFormat, RenderTextureReadWrite.Linear);
					this._DepthTex2.filterMode = FilterMode.Point;
				}
				CommandBuffer utilityCommandBuffer = WaterCamera._UtilityCommandBuffer;
				utilityCommandBuffer.Clear();
				utilityCommandBuffer.name = "[PW Water] Blend Deferred Results";
				Material depthBlitCopyMaterial = this._DepthBlitCopyMaterial;
				utilityCommandBuffer.SetRenderTarget(this._DepthTex2);
				utilityCommandBuffer.DrawMesh(Quads.BipolarXInversedY, Matrix4x4.identity, depthBlitCopyMaterial, 0, (this._BlendedDepthTexturesFormat != RenderTextureFormat.Depth) ? 2 : 5);
				utilityCommandBuffer.Blit(BuiltinRenderTextureType.GBuffer0, this._Gbuffer0Tex, this._Gbuffer0MixMaterial, 0);
				utilityCommandBuffer.SetRenderTarget(WaterCamera._DeferredTargets, BuiltinRenderTextureType.Reflections);
				utilityCommandBuffer.DrawMesh(Quads.BipolarXY, Matrix4x4.identity, this._Gbuffer123MixMaterial, 0);
				utilityCommandBuffer.SetRenderTarget(target);
				utilityCommandBuffer.SetGlobalTexture("_WaterColorTex", BuiltinRenderTextureType.CameraTarget);
				utilityCommandBuffer.DrawMesh(Quads.BipolarXInversedY, Matrix4x4.identity, this._FinalColorMixMaterial, 0, 0, (!(mainWater != null)) ? null : mainWater.Renderer.PropertyBlock);
				utilityCommandBuffer.SetGlobalTexture("_CameraDepthTexture", this._DepthTex2);
				utilityCommandBuffer.SetGlobalTexture("_CameraGBufferTexture0", this._Gbuffer0Tex);
				utilityCommandBuffer.SetGlobalTexture("_CameraGBufferTexture1", BuiltinRenderTextureType.GBuffer1);
				utilityCommandBuffer.SetGlobalTexture("_CameraGBufferTexture2", BuiltinRenderTextureType.GBuffer2);
				utilityCommandBuffer.SetGlobalTexture("_CameraGBufferTexture3", BuiltinRenderTextureType.Reflections);
				utilityCommandBuffer.SetGlobalTexture("_CameraReflectionsTexture", BuiltinRenderTextureType.Reflections);
				waterRenderCamera.AddCommandBuffer(CameraEvent.AfterEverything, utilityCommandBuffer);
			}
			Shader customShader = GraphicsSettings.GetCustomShader(BuiltinShaderType.DeferredReflections);
			Shader customShader2 = GraphicsSettings.GetCustomShader(BuiltinShaderType.DeferredShading);
			BuiltinShaderMode shaderMode = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredReflections);
			BuiltinShaderMode shaderMode2 = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredShading);
			GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredReflections, this._DeferredReflections);
			GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, this._DeferredShading);
			GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredReflections, BuiltinShaderMode.UseCustom);
			GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);
			if (mainWater != null)
			{
				Shader.SetGlobalVector("_MainWaterWrapSubsurfaceScatteringPack", mainWater.Renderer.PropertyBlock.GetVector("_WrapSubsurfaceScatteringPack"));
			}
			WaterCamera component = waterRenderCamera.GetComponent<WaterCamera>();
			component.CopyFrom(this);
			waterRenderCamera.enabled = false;
			waterRenderCamera.clearFlags = CameraClearFlags.Color;
			waterRenderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			waterRenderCamera.depthTextureMode = DepthTextureMode.Depth;
			waterRenderCamera.renderingPath = RenderingPath.DeferredShading;
			waterRenderCamera.allowHDR = true;
			waterRenderCamera.targetTexture = temp;
			waterRenderCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			waterRenderCamera.Render();
			waterRenderCamera.targetTexture = null;
			GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredReflections, customShader);
			GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, customShader2);
			GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredReflections, shaderMode);
			GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, shaderMode2);
			Shader.SetGlobalTexture("_CameraDepthTexture", this._DepthTex2);
			if (this._RenderMode == WaterRenderMode.ImageEffectDeferred)
			{
				waterRenderCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, WaterCamera._UtilityCommandBufferCache);
			}
		}

		private void EnableEffects()
		{
			if (this._WaterCameraType != WaterCamera.CameraType.Normal)
			{
				return;
			}
			this._PixelWidth = Mathf.RoundToInt((float)this._CameraComponent.pixelWidth);
			this._PixelHeight = Mathf.RoundToInt((float)this._CameraComponent.pixelHeight);
			if (WaterProjectSettings.Instance.SinglePassStereoRendering)
			{
				this._PixelWidth = Mathf.CeilToInt((float)(this._PixelWidth << 1) / 256f) * 256;
			}
			this._EffectsEnabled = true;
			if (this._RenderWaterDepth || this._RenderVolumes)
			{
				this._CameraComponent.depthTextureMode |= DepthTextureMode.Depth;
			}
		}

		private void DisableEffects()
		{
			this._EffectsEnabled = false;
		}

		protected Camera CreateEffectsCamera(WaterCamera.CameraType type)
		{
			GameObject gameObject = new GameObject(base.name + " Water Effects Camera")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			Camera camera = gameObject.AddComponent<Camera>();
			camera.enabled = false;
			camera.useOcclusionCulling = false;
			WaterCamera waterCamera = gameObject.AddComponent<WaterCamera>();
			waterCamera._WaterCameraType = type;
			waterCamera._MainCamera = this._CameraComponent;
			waterCamera._BaseCamera = this;
			WaterCamera._EnabledWaterCameras.Remove(waterCamera);
			return camera;
		}

		private void RenderShadowEnforcers()
		{
			if (this._ShadowsEnforcerMesh == null)
			{
				this._ShadowsEnforcerMesh = new Mesh
				{
					name = "Water Shadow Enforcer",
					hideFlags = HideFlags.DontSave,
					vertices = new Vector3[4]
				};
				this._ShadowsEnforcerMesh.SetIndices(new int[]
				{
					0,
					1,
					2,
					3
				}, MeshTopology.Quads, 0);
				this._ShadowsEnforcerMesh.UploadMeshData(true);
				this._ShadowsEnforcerMaterial = new Material(this._ShadowEnforcerShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
			Bounds bounds = default(Bounds);
			float shadowDistance = QualitySettings.shadowDistance;
			Vector3 point = this._CameraComponent.ViewportPointToRay(new Vector3(this._ShadowedWaterRect.xMin, this._ShadowedWaterRect.yMin, 1f)).GetPoint(shadowDistance * 1.5f);
			Vector3 point2 = this._CameraComponent.ViewportPointToRay(new Vector3(this._ShadowedWaterRect.xMax, this._ShadowedWaterRect.yMax, 1f)).GetPoint(shadowDistance * 1.5f);
			WaterCamera.SetBoundsMinMaxComponentWise(ref bounds, point, point2);
			bounds.Encapsulate(this._CameraComponent.ViewportPointToRay(new Vector3(this._ShadowedWaterRect.xMin, this._ShadowedWaterRect.yMax, 1f)).GetPoint(shadowDistance * 0.3f));
			bounds.Encapsulate(this._CameraComponent.ViewportPointToRay(new Vector3(this._ShadowedWaterRect.xMax, this._ShadowedWaterRect.yMin, 1f)).GetPoint(shadowDistance * 0.3f));
			this._ShadowsEnforcerMesh.bounds = bounds;
			Graphics.DrawMesh(this._ShadowsEnforcerMesh, Matrix4x4.identity, this._ShadowsEnforcerMaterial, 0);
		}

		private void PrepareToRender()
		{
			this._ShadowedWaterRect = new Rect(1f, 1f, -1f, -1f);
			float radius = 4f * this._CameraComponent.nearClipPlane * Mathf.Tan(0.5f * this._CameraComponent.fieldOfView * 0.0174532924f);
			Water water = Water.FindWater(base.transform.position, radius, this._CustomWaterRenderList, out this._IsInsideSubtractiveVolume, out this._IsInsideAdditiveVolume);
			if (water != this._ContainingWater)
			{
				if (this._ContainingWater != null && this._SubmersionState != SubmersionState.None)
				{
					this._SubmersionState = SubmersionState.None;
					this.SubmersionStateChanged.Invoke(this);
				}
				this._ContainingWater = water;
				this._SubmersionState = SubmersionState.None;
				if (water != null && water.Volume.Boundless)
				{
					this._CameraSubmersion.Create();
				}
			}
			else
			{
				SubmersionState state = this._CameraSubmersion.State;
				if (state != this._SubmersionState)
				{
					this._SubmersionState = state;
					this.SubmersionStateChanged.Invoke(this);
				}
			}
		}

		private static void SetFallbackTextures()
		{
			Shader.SetGlobalTexture(ShaderVariables.UnderwaterMask, DefaultTextures.Get(Color.clear));
			Shader.SetGlobalTexture(ShaderVariables.DisplacementsMask, DefaultTextures.Get(Color.white));
		}

		private void ToggleEffects()
		{
			bool flag = WaterSystem.IsWaterPossiblyVisible();
			if (!this._EffectsEnabled)
			{
				if (flag)
				{
					this.EnableEffects();
				}
			}
			else if (!flag)
			{
				this.DisableEffects();
			}
			int num = Mathf.RoundToInt((float)this._CameraComponent.pixelWidth);
			int num2 = Mathf.RoundToInt((float)this._CameraComponent.pixelHeight);
			if (WaterProjectSettings.Instance.SinglePassStereoRendering)
			{
				num = Mathf.CeilToInt((float)(num << 1) / 256f) * 256;
			}
			if (this._EffectsEnabled && (num != this._PixelWidth || num2 != this._PixelHeight))
			{
				this.DisableEffects();
				this.EnableEffects();
				if (this.RenderTargetResized != null)
				{
					this.RenderTargetResized(this);
				}
			}
		}

		private void SetPlaneProjectorMatrix(bool isRenderTarget, bool flipY)
		{
			Camera planeProjectorCamera = this.PlaneProjectorCamera;
			Shader.SetGlobalMatrix("_WaterProjectorPreviousVP", this._LastPlaneProjectorMatrix);
			planeProjectorCamera.CopyFrom(this._CameraComponent);
			planeProjectorCamera.renderingPath = RenderingPath.Forward;
			planeProjectorCamera.ResetProjectionMatrix();
			planeProjectorCamera.projectionMatrix = this._CameraComponent.projectionMatrix;
			Matrix4x4 proj = (!flipY) ? this._CameraComponent.projectionMatrix : (this._CameraComponent.projectionMatrix * Matrix4x4.Scale(new Vector3(1f, -1f, 1f)));
			this._LastPlaneProjectorMatrix = GL.GetGPUProjectionMatrix(proj, isRenderTarget) * this._CameraComponent.worldToCameraMatrix;
			Shader.SetGlobalMatrix("_WaterProjectorVP", this._LastPlaneProjectorMatrix);
		}

		private void SetLocalMapCoordinates()
		{
			int num = Mathf.NextPowerOfTwo(this._CameraComponent.pixelWidth + this._CameraComponent.pixelHeight >> 1);
			float num2 = 0f;
			float num3 = 0f;
			List<Water> waters = ApplicationSingleton<WaterSystem>.Instance.Waters;
			for (int i = waters.Count - 1; i >= 0; i--)
			{
				Water water = waters[i];
				num2 += water.MaxVerticalDisplacement;
				float y = water.transform.position.y;
				if (num3 < y)
				{
					num3 = y;
				}
			}
			Vector3 position = this._CameraComponent.transform.position;
			Vector3 forward = this._CameraComponent.transform.forward;
			float f = Mathf.Min(1f, forward.y + 1f);
			float num4 = Mathf.Abs(position.y) * (1f + 7f * Mathf.Sqrt(f));
			float num5 = num2 * 2.5f;
			float num6 = (num4 <= num5) ? num5 : num4;
			if (num6 < 20f)
			{
				num6 = 20f;
			}
			Vector3 vector = new Vector3(position.x + forward.x * num6 * 0.4f, 0f, position.z + forward.z * num6 * 0.4f);
			this._LocalMapsRectPrevious = this._LocalMapsRect;
			float num7 = num6 / (float)num;
			this._LocalMapsRect = new Rect(vector.x - num6 + num7, vector.z - num6 + num7, 2f * num6, 2f * num6);
			float num8 = 1f / this._LocalMapsRectPrevious.width;
			Shader.SetGlobalVector(ShaderVariables.LocalMapsCoordsPrevious, new Vector4(-this._LocalMapsRectPrevious.xMin * num8, -this._LocalMapsRectPrevious.yMin * num8, num8, this._LocalMapsRectPrevious.width));
			float num9 = 1f / this._LocalMapsRect.width;
			Shader.SetGlobalVector(ShaderVariables.LocalMapsCoords, new Vector4(-this._LocalMapsRect.xMin * num9, -this._LocalMapsRect.yMin * num9, num9, this._LocalMapsRect.width));
		}

		private bool ShouldRender()
		{
			bool flag = true;
			flag &= (base.enabled && Application.isPlaying);
			flag &= WaterSystem.IsWaterPossiblyVisible();
			return flag & (!WaterUtilities.IsSceneViewCamera(this._CameraComponent) || WaterProjectSettings.Instance.RenderInSceneView);
		}

		private static void SetBoundsMinMaxComponentWise(ref Bounds bounds, Vector3 a, Vector3 b)
		{
			if (a.x > b.x)
			{
				float x = b.x;
				b.x = a.x;
				a.x = x;
			}
			if (a.y > b.y)
			{
				float y = b.y;
				b.y = a.y;
				a.y = y;
			}
			if (a.z > b.z)
			{
				float z = b.z;
				b.z = a.z;
				a.z = z;
			}
			bounds.SetMinMax(a, b);
		}

		[HideInInspector]
		public Camera ReflectionCamera;

		[FormerlySerializedAs("depthBlitCopyShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _DepthBlitCopyShader;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("shadowEnforcerShader")]
		private Shader _ShadowEnforcerShader;

		[FormerlySerializedAs("gbuffer0MixShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _Gbuffer0MixShader;

		[FormerlySerializedAs("gbuffer123MixShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _Gbuffer123MixShader;

		[FormerlySerializedAs("finalColorMixShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _FinalColorMixShader;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("deferredReflections")]
		private Shader _DeferredReflections;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("deferredShading")]
		private Shader _DeferredShading;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("mergeDisplacementsShader")]
		private Shader _MergeDisplacementsShader;

		[FormerlySerializedAs("renderMode")]
		[SerializeField]
		private WaterRenderMode _RenderMode;

		[FormerlySerializedAs("geometryType")]
		[SerializeField]
		private WaterGeometryType _GeometryType;

		[FormerlySerializedAs("renderWaterDepth")]
		[SerializeField]
		private bool _RenderWaterDepth = true;

		[Range(0.2f, 1f)]
		[Tooltip("Water has a pretty smooth shape so it's often safe to render it's depth in a lower resolution than the rest of the scene. Although the default value is 1.0, you may probably safely use 0.5 and gain some minor performance boost. If you will encounter any artifacts in masking or image effects, set it back to 1.0.")]
		[FormerlySerializedAs("baseEffectsQuality")]
		[SerializeField]
		private float _BaseEffectsQuality = 1f;

		[FormerlySerializedAs("superSampling")]
		[SerializeField]
		private float _SuperSampling = 1f;

		[FormerlySerializedAs("renderVolumes")]
		[SerializeField]
		private bool _RenderVolumes = true;

		[SerializeField]
		[FormerlySerializedAs("renderFlatMasks")]
		private bool _RenderFlatMasks = true;

		[FormerlySerializedAs("forcedVertexCount")]
		[SerializeField]
		private int _ForcedVertexCount;

		[FormerlySerializedAs("submersionStateChanged")]
		[SerializeField]
		private WaterCamera.WaterCameraEvent _SubmersionStateChanged;

		[SerializeField]
		[Tooltip("Optional. Deferred rendering mode will try to match profile parameters of this water object as well as possible. It affects only some minor parameters and you may generally ignore this setting. May be removed in the future.")]
		[FormerlySerializedAs("mainWater")]
		private Water _MainWater;

		[SerializeField]
		[FormerlySerializedAs("effectsLight")]
		private LightWaterEffects _EffectsLight;

		private RenderTexture _Gbuffer0Tex;

		private RenderTexture _DepthTex2;

		private WaterCamera _BaseCamera;

		private Camera _EffectCamera;

		private Camera _MainCamera;

		private Camera _PlaneProjectorCamera;

		protected Camera _CameraComponent;

		private Material _DepthBlitCopyMaterialCache;

		private RenderTextureFormat _BlendedDepthTexturesFormat;

		private WaterCamera.CameraType _WaterCameraType;

		private bool _EffectsEnabled;

		private IWaterImageEffect[] _ImageEffects;

		private Rect _LocalMapsRect;

		private Rect _LocalMapsRectPrevious;

		private Rect _ShadowedWaterRect;

		private int _PixelWidth;

		private int _PixelHeight;

		private Mesh _ShadowsEnforcerMesh;

		private Material _ShadowsEnforcerMaterial;

		internal Water _ContainingWater;

		private float _WaterLevel;

		private SubmersionState _SubmersionState;

		private bool _IsInsideSubtractiveVolume;

		private bool _IsInsideAdditiveVolume;

		private Matrix4x4 _LastPlaneProjectorMatrix;

		private WaterCameraIME _WaterCameraIME;

		private List<Water> _CustomWaterRenderList;

		private static CommandBuffer _UtilityCommandBufferCache;

		private static readonly Dictionary<Camera, WaterCamera> _WaterCamerasCache = new Dictionary<Camera, WaterCamera>();

		private static readonly List<WaterCamera> _EnabledWaterCameras = new List<WaterCamera>();

		private static readonly RenderTargetIdentifier[] _DeferredTargets = new RenderTargetIdentifier[]
		{
			BuiltinRenderTextureType.GBuffer1,
			BuiltinRenderTextureType.GBuffer2,
			BuiltinRenderTextureType.Reflections
		};

		private Camera _WaterRenderCameraCache;

		private CommandBuffer _ImageEffectCommands;

		private RenderTexture _DeferredRenderTarget;

		private Material _Gbuffer0MixMaterial;

		private Material _Gbuffer123MixMaterial;

		private Material _FinalColorMixMaterial;

		private readonly MaskModule _MaskModule = new MaskModule();

		private readonly DepthModule _DepthModule = new DepthModule();

		private readonly ForwardModule _ForwardModule = new ForwardModule();

		[SerializeField]
		private WaterCameraSubmersion _CameraSubmersion = new WaterCameraSubmersion();

		public bool _EditSubmersion;

		public enum CameraType
		{
			Normal,
			Effect,
			RenderHelper
		}

		[Serializable]
		public class WaterCameraEvent : UnityEvent<WaterCamera>
		{
		}
	}
}
