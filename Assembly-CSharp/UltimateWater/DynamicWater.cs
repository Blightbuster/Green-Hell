using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public sealed class DynamicWater : WaterModule
	{
		public DynamicWater(Water water, DynamicWater.Data data)
		{
			this._Water = water;
			this._Data = data;
			DynamicWater.CreateCommandBuffers();
			this.Validate();
			this._MapLocalDisplacementsShader = Shader.Find("UltimateWater/Utility/Map Local Displacements");
		}

		public Water Water
		{
			get
			{
				return this._Water;
			}
		}

		public void ValidateWaterComponents()
		{
			this._OverlayRenderers = this._Water.GetComponentsInChildren<IOverlaysRenderer>();
			int[] array = new int[this._OverlayRenderers.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Type type = this._OverlayRenderers[i].GetType();
				object[] customAttributes = type.GetCustomAttributes(typeof(OverlayRendererOrderAttribute), true);
				if (customAttributes.Length != 0)
				{
					array[i] = ((OverlayRendererOrderAttribute)customAttributes[0]).Priority;
				}
			}
			Array.Sort<int, IOverlaysRenderer>(array, this._OverlayRenderers);
		}

		public static void AddRenderer<T>(T renderer) where T : IDynamicWaterEffects
		{
			DynamicWater._Renderers.Add<T>(renderer);
		}

		public static void RemoveRenderer<T>(T renderer) where T : IDynamicWaterEffects
		{
			DynamicWater._Renderers.Remove<T>(renderer);
		}

		public void RenderTotalDisplacementMap(WaterCamera camera, RenderTexture renderTexture)
		{
			Rect localMapsRect = camera.LocalMapsRect;
			Camera effectsCamera = camera.EffectsCamera;
			effectsCamera.enabled = false;
			effectsCamera.stereoTargetEye = StereoTargetEyeMask.None;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			effectsCamera.renderingPath = RenderingPath.VertexLit;
			effectsCamera.orthographic = true;
			effectsCamera.orthographicSize = localMapsRect.width * 0.5f;
			effectsCamera.farClipPlane = 2000f;
			effectsCamera.ResetProjectionMatrix();
			effectsCamera.clearFlags = CameraClearFlags.Nothing;
			effectsCamera.allowHDR = true;
			effectsCamera.transform.position = new Vector3(localMapsRect.center.x, 1000f, localMapsRect.center.y);
			effectsCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 1f));
			WaterCamera component = effectsCamera.GetComponent<WaterCamera>();
			component.enabled = true;
			component.GeometryType = WaterGeometryType.UniformGrid;
			component.ForcedVertexCount = renderTexture.width * renderTexture.height >> 2;
			component.RenderWaterWithShader("[Ultimate Water] Render Total Displacement Map", renderTexture, this._MapLocalDisplacementsShader, this._Water);
		}

		public DynamicWaterCameraData GetCameraOverlaysData(Camera camera, bool createIfNotExists = true)
		{
			DynamicWaterCameraData dynamicWaterCameraData;
			if (!this._Buffers.TryGetValue(camera, out dynamicWaterCameraData) && createIfNotExists)
			{
				dynamicWaterCameraData = (this._Buffers[camera] = new DynamicWaterCameraData(this, WaterCamera.GetWaterCamera(camera, false), this._Data.Antialiasing));
				DynamicWater.DrawDisplacementMaskRenderers(dynamicWaterCameraData);
				dynamicWaterCameraData.SwapFoamMaps();
				for (int i = 0; i < this._OverlayRenderers.Length; i++)
				{
					this._OverlayRenderers[i].RenderOverlays(dynamicWaterCameraData);
				}
			}
			if (dynamicWaterCameraData != null)
			{
				dynamicWaterCameraData._LastFrameUsed = Time.frameCount;
			}
			return dynamicWaterCameraData;
		}

		internal override void OnWaterRender(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			if (waterCamera.Type != WaterCamera.CameraType.Normal || !Application.isPlaying)
			{
				return;
			}
			if (!WaterProjectSettings.Instance.RenderInSceneView && WaterUtilities.IsSceneViewCamera(cameraComponent))
			{
				return;
			}
			Shader.SetGlobalVector("_TileSizesInv", this._Water.WindWaves.TileSizesInv);
			DynamicWaterCameraData cameraOverlaysData = this.GetCameraOverlaysData(cameraComponent, true);
			cameraOverlaysData.SwapFoamMaps();
			cameraOverlaysData.ClearOverlays();
			DynamicWater.DrawDisplacementMaskRenderers(cameraOverlaysData);
			DynamicWater.DrawDisplacementRenderers(cameraOverlaysData);
			for (int i = 0; i < this._OverlayRenderers.Length; i++)
			{
				this._OverlayRenderers[i].RenderOverlays(cameraOverlaysData);
			}
			if (this._Water.Foam != null)
			{
				this._Water.Foam.RenderOverlays(cameraOverlaysData);
			}
			for (int j = 0; j < this._OverlayRenderers.Length; j++)
			{
				this._OverlayRenderers[j].RenderFoam(cameraOverlaysData);
			}
			int localDiffuseMap = ShaderVariables.LocalDiffuseMap;
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			propertyBlock.SetTexture(ShaderVariables.LocalDisplacementMap, cameraOverlaysData.DynamicDisplacementMap);
			propertyBlock.SetTexture(ShaderVariables.LocalNormalMap, cameraOverlaysData.NormalMap);
			propertyBlock.SetTexture(ShaderVariables.TotalDisplacementMap, cameraOverlaysData.TotalDisplacementMap);
			propertyBlock.SetTexture(ShaderVariables.DisplacementsMask, cameraOverlaysData.DisplacementsMask);
			propertyBlock.SetTexture(localDiffuseMap, cameraOverlaysData.DiffuseMap);
			Shader.SetGlobalTexture("DIFFUSE", cameraOverlaysData.DiffuseMap);
			RenderTexture debugMap = cameraOverlaysData.GetDebugMap(false);
			if (debugMap != null)
			{
				propertyBlock.SetTexture("_LocalDebugMap", debugMap);
			}
			if (waterCamera.MainWater == this._Water)
			{
				Shader.SetGlobalTexture(ShaderVariables.TotalDisplacementMap, cameraOverlaysData.TotalDisplacementMap);
			}
			DynamicWater.DrawFoamRenderers(cameraOverlaysData);
			DynamicWater.DrawDiffuseRenderers(cameraOverlaysData);
		}

		internal override void Enable()
		{
			this.ValidateWaterComponents();
			if (this._MapLocalDisplacementsMaterial == null)
			{
				this._MapLocalDisplacementsMaterial = new Material(this._MapLocalDisplacementsShader)
				{
					hideFlags = HideFlags.DontSave
				};
			}
		}

		internal override void Disable()
		{
			foreach (KeyValuePair<Camera, DynamicWaterCameraData> keyValuePair in this._Buffers)
			{
				keyValuePair.Value.Dispose();
			}
			this._Buffers.Clear();
		}

		internal override void Destroy()
		{
			foreach (KeyValuePair<Camera, DynamicWaterCameraData> keyValuePair in this._Buffers)
			{
				keyValuePair.Value.Dispose();
			}
			this._Buffers.Clear();
		}

		internal override void Validate()
		{
		}

		internal override void Update()
		{
			int num = Time.frameCount - 3;
			Dictionary<Camera, DynamicWaterCameraData>.Enumerator enumerator = this._Buffers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<Camera, DynamicWaterCameraData> keyValuePair = enumerator.Current;
				if (keyValuePair.Value._LastFrameUsed < num)
				{
					KeyValuePair<Camera, DynamicWaterCameraData> keyValuePair2 = enumerator.Current;
					keyValuePair2.Value.Dispose();
					List<Camera> lostCameras = this._LostCameras;
					KeyValuePair<Camera, DynamicWaterCameraData> keyValuePair3 = enumerator.Current;
					lostCameras.Add(keyValuePair3.Key);
				}
			}
			enumerator.Dispose();
			for (int i = 0; i < this._LostCameras.Count; i++)
			{
				this._Buffers.Remove(this._LostCameras[i]);
			}
			this._LostCameras.Clear();
		}

		private static void DrawDisplacementRenderers(DynamicWaterCameraData overlays)
		{
			Camera cameraComponent = overlays.Camera.CameraComponent;
			DynamicWater._CustomEffectsBuffers[0] = overlays.DynamicDisplacementMap;
			DynamicWater._CustomEffectsBuffers[1] = overlays.NormalMap;
			GL.PushMatrix();
			GL.modelview = cameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(cameraComponent.projectionMatrix);
			DynamicWater._RenderDisplacementBuffer.Clear();
			DynamicWater._RenderDisplacementBuffer.SetRenderTarget(DynamicWater._CustomEffectsBuffers, overlays.DynamicDisplacementMap);
			List<ILocalDisplacementRenderer> localDisplacement = DynamicWater._Renderers.LocalDisplacement;
			for (int i = localDisplacement.Count - 1; i >= 0; i--)
			{
				localDisplacement[i].RenderLocalDisplacement(DynamicWater._RenderDisplacementBuffer, overlays);
			}
			Graphics.ExecuteCommandBuffer(DynamicWater._RenderDisplacementBuffer);
			GL.PopMatrix();
		}

		private static void DrawDisplacementMaskRenderers(DynamicWaterCameraData overlays)
		{
			Shader.SetGlobalTexture("_CameraDepthTexture", DefaultTextures.Get(Color.white));
			Camera cameraComponent = overlays.Camera.CameraComponent;
			GL.PushMatrix();
			GL.modelview = cameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(cameraComponent.projectionMatrix);
			DynamicWater._RenderDisplacementMaskBuffer.Clear();
			DynamicWater._RenderDisplacementMaskBuffer.SetRenderTarget(overlays.DisplacementsMask);
			List<ILocalDisplacementMaskRenderer> localDisplacementMask = DynamicWater._Renderers.LocalDisplacementMask;
			for (int i = localDisplacementMask.Count - 1; i >= 0; i--)
			{
				localDisplacementMask[i].RenderLocalMask(DynamicWater._RenderDisplacementMaskBuffer, overlays);
			}
			Graphics.ExecuteCommandBuffer(DynamicWater._RenderDisplacementMaskBuffer);
			GL.PopMatrix();
		}

		private static void DrawFoamRenderers(DynamicWaterCameraData overlays)
		{
			List<ILocalFoamRenderer> localFoam = DynamicWater._Renderers.LocalFoam;
			for (int i = localFoam.Count - 1; i >= 0; i--)
			{
				localFoam[i].Enable();
			}
			Shader.SetGlobalTexture("_CameraDepthTexture", DefaultTextures.Get(Color.white));
			Camera cameraComponent = overlays.Camera.CameraComponent;
			GL.PushMatrix();
			GL.modelview = cameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(cameraComponent.projectionMatrix);
			DynamicWater._RenderFoamBuffer.Clear();
			DynamicWater._RenderFoamBuffer.SetRenderTarget(overlays.FoamMap);
			for (int j = localFoam.Count - 1; j >= 0; j--)
			{
				localFoam[j].RenderLocalFoam(DynamicWater._RenderFoamBuffer, overlays);
			}
			Graphics.ExecuteCommandBuffer(DynamicWater._RenderFoamBuffer);
			GL.PopMatrix();
			for (int k = localFoam.Count - 1; k >= 0; k--)
			{
				localFoam[k].Disable();
			}
		}

		private static void DrawDiffuseRenderers(DynamicWaterCameraData overlays)
		{
			List<ILocalDiffuseRenderer> localDiffuse = DynamicWater._Renderers.LocalDiffuse;
			for (int i = localDiffuse.Count - 1; i >= 0; i--)
			{
				localDiffuse[i].Enable();
			}
			Shader.SetGlobalTexture("_CameraDepthTexture", DefaultTextures.Get(Color.white));
			Camera cameraComponent = overlays.Camera.CameraComponent;
			GL.PushMatrix();
			GL.modelview = cameraComponent.worldToCameraMatrix;
			GL.LoadProjectionMatrix(cameraComponent.projectionMatrix);
			DynamicWater._RenderDiffuseBuffer.Clear();
			DynamicWater._RenderDiffuseBuffer.SetRenderTarget(overlays.DiffuseMap);
			for (int j = localDiffuse.Count - 1; j >= 0; j--)
			{
				localDiffuse[j].RenderLocalDiffuse(DynamicWater._RenderDiffuseBuffer, overlays);
			}
			Graphics.ExecuteCommandBuffer(DynamicWater._RenderDiffuseBuffer);
			GL.PopMatrix();
			for (int k = localDiffuse.Count - 1; k >= 0; k--)
			{
				localDiffuse[k].Disable();
			}
		}

		private static void CreateCommandBuffers()
		{
			DynamicWater._RenderDisplacementBuffer = new CommandBuffer
			{
				name = "[Ultimate Water] Displacement Renderers"
			};
			DynamicWater._RenderDisplacementMaskBuffer = new CommandBuffer
			{
				name = "[Ultimate Water] Displacement Mask Renderers"
			};
			DynamicWater._RenderFoamBuffer = new CommandBuffer
			{
				name = "[Ultimate Water] Foam Renderers"
			};
			DynamicWater._RenderDiffuseBuffer = new CommandBuffer
			{
				name = "[Ultimate Water] Diffuse Renderers"
			};
		}

		private static void ReleaseCommandBuffers()
		{
			if (DynamicWater._RenderDisplacementBuffer != null)
			{
				DynamicWater._RenderDisplacementBuffer.Release();
			}
			if (DynamicWater._RenderDisplacementMaskBuffer != null)
			{
				DynamicWater._RenderDisplacementMaskBuffer.Release();
			}
			if (DynamicWater._RenderFoamBuffer != null)
			{
				DynamicWater._RenderFoamBuffer.Release();
			}
			if (DynamicWater._RenderDiffuseBuffer != null)
			{
				DynamicWater._RenderDiffuseBuffer.Release();
			}
		}

		private IOverlaysRenderer[] _OverlayRenderers;

		private Material _MapLocalDisplacementsMaterial;

		private static readonly DynamicWater.Renderers _Renderers = new DynamicWater.Renderers();

		public static List<IWavesInteractive> Interactions = new List<IWavesInteractive>();

		private readonly Water _Water;

		private readonly DynamicWater.Data _Data;

		private readonly Shader _MapLocalDisplacementsShader;

		private static CommandBuffer _RenderDisplacementBuffer;

		private static CommandBuffer _RenderDisplacementMaskBuffer;

		private static CommandBuffer _RenderFoamBuffer;

		private static CommandBuffer _RenderDiffuseBuffer;

		private readonly Dictionary<Camera, DynamicWaterCameraData> _Buffers = new Dictionary<Camera, DynamicWaterCameraData>();

		private readonly List<Camera> _LostCameras = new List<Camera>();

		private static readonly RenderTargetIdentifier[] _CustomEffectsBuffers = new RenderTargetIdentifier[2];

		[Serializable]
		public class Data
		{
			public int Antialiasing = 1;

			public LayerMask CustomEffectsLayerMask = -1;
		}

		private class Renderers
		{
			public void Add<T>(T renderer) where T : IDynamicWaterEffects
			{
				ILocalDisplacementRenderer localDisplacementRenderer = renderer as ILocalDisplacementRenderer;
				ILocalDisplacementMaskRenderer localDisplacementMaskRenderer = renderer as ILocalDisplacementMaskRenderer;
				ILocalFoamRenderer localFoamRenderer = renderer as ILocalFoamRenderer;
				ILocalDiffuseRenderer localDiffuseRenderer = renderer as ILocalDiffuseRenderer;
				IWavesInteractive wavesInteractive = renderer as IWavesInteractive;
				if (localDisplacementRenderer != null)
				{
					this.LocalDisplacement.Add(localDisplacementRenderer);
				}
				if (localDisplacementMaskRenderer != null)
				{
					this.LocalDisplacementMask.Add(localDisplacementMaskRenderer);
				}
				if (localFoamRenderer != null)
				{
					this.LocalFoam.Add(localFoamRenderer);
				}
				if (wavesInteractive != null)
				{
					DynamicWater.Interactions.Add(wavesInteractive);
				}
				if (localDiffuseRenderer != null)
				{
					this.LocalDiffuse.Add(localDiffuseRenderer);
				}
			}

			public void Remove<T>(T renderer) where T : IDynamicWaterEffects
			{
				ILocalDisplacementRenderer localDisplacementRenderer = renderer as ILocalDisplacementRenderer;
				ILocalDisplacementMaskRenderer localDisplacementMaskRenderer = renderer as ILocalDisplacementMaskRenderer;
				ILocalFoamRenderer localFoamRenderer = renderer as ILocalFoamRenderer;
				ILocalDiffuseRenderer localDiffuseRenderer = renderer as ILocalDiffuseRenderer;
				IWavesInteractive wavesInteractive = renderer as IWavesInteractive;
				if (localDisplacementRenderer != null)
				{
					this.LocalDisplacement.Remove(localDisplacementRenderer);
				}
				if (localDisplacementMaskRenderer != null)
				{
					this.LocalDisplacementMask.Remove(localDisplacementMaskRenderer);
				}
				if (localFoamRenderer != null)
				{
					this.LocalFoam.Remove(localFoamRenderer);
				}
				if (wavesInteractive != null)
				{
					DynamicWater.Interactions.Remove(wavesInteractive);
				}
				if (localDiffuseRenderer != null)
				{
					this.LocalDiffuse.Remove(localDiffuseRenderer);
				}
			}

			public readonly List<ILocalDisplacementRenderer> LocalDisplacement = new List<ILocalDisplacementRenderer>();

			public readonly List<ILocalDisplacementMaskRenderer> LocalDisplacementMask = new List<ILocalDisplacementMaskRenderer>();

			public readonly List<ILocalFoamRenderer> LocalFoam = new List<ILocalFoamRenderer>();

			public readonly List<ILocalDiffuseRenderer> LocalDiffuse = new List<ILocalDiffuseRenderer>();
		}
	}
}
