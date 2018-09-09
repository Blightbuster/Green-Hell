using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public class WaterRenderer
	{
		public int MaskCount
		{
			get
			{
				return this._Masks.Count;
			}
		}

		public MaterialPropertyBlock PropertyBlock
		{
			get
			{
				return (this._PropertyBlock == null) ? (this._PropertyBlock = new MaterialPropertyBlock()) : this._PropertyBlock;
			}
		}

		public Transform ReflectionProbeAnchor
		{
			get
			{
				return this._ReflectionProbeAnchor;
			}
			set
			{
				this._ReflectionProbeAnchor = value;
			}
		}

		public void AddMask(WaterSimpleMask mask)
		{
			mask.Renderer.enabled = false;
			int renderQueuePriority = mask.RenderQueuePriority;
			for (int i = this._Masks.Count - 1; i >= 0; i--)
			{
				WaterSimpleMask waterSimpleMask = this._Masks[i];
				if (waterSimpleMask.RenderQueuePriority <= renderQueuePriority)
				{
					this._Masks.Insert(i + 1, mask);
					return;
				}
			}
			this._Masks.Insert(0, mask);
		}

		public void RemoveMask(WaterSimpleMask mask)
		{
			this._Masks.Remove(mask);
		}

		public void RenderEffects(WaterCamera waterCamera)
		{
			Camera cameraComponent = waterCamera.CameraComponent;
			if (!this._Water.isActiveAndEnabled || (cameraComponent.cullingMask & 1 << this._Water.gameObject.layer) == 0)
			{
				return;
			}
			if (!this._Water.Volume.Boundless && this._Water.Volume.HasRenderableAdditiveVolumes && !waterCamera.RenderVolumes)
			{
				return;
			}
			this._Water.OnWaterRender(waterCamera);
		}

		public void Render(Camera camera, WaterGeometryType geometryType, CommandBuffer commandBuffer = null, Shader shader = null)
		{
			if (!this._Water.isActiveAndEnabled || (camera.cullingMask & 1 << this._Water.gameObject.layer) == 0)
			{
				return;
			}
			WaterCamera waterCamera = WaterCamera.GetWaterCamera(camera, false);
			bool flag = waterCamera != null;
			if (!flag || (!this._Water.Volume.Boundless && this._Water.Volume.HasRenderableAdditiveVolumes && !waterCamera.RenderVolumes))
			{
				return;
			}
			if (this._Water.ShaderSet.ReceiveShadows)
			{
				Vector2 min = new Vector2(0f, 0f);
				Vector2 max = new Vector2(1f, 1f);
				waterCamera.ReportShadowedWaterMinMaxRect(min, max);
			}
			if (!this._UseSharedMask)
			{
				this.RenderMasks(camera, waterCamera, this._PropertyBlock);
			}
			Matrix4x4 matrix;
			Mesh[] transformedMeshes = this._Water.Geometry.GetTransformedMeshes(camera, out matrix, geometryType, false, waterCamera.ForcedVertexCount);
			if (commandBuffer == null)
			{
				Camera camera2 = (waterCamera.RenderMode == WaterRenderMode.DefaultQueue) ? camera : waterCamera._WaterRenderCamera;
				for (int i = 0; i < transformedMeshes.Length; i++)
				{
					Graphics.DrawMesh(transformedMeshes[i], matrix, this._Water.Materials.SurfaceMaterial, this._Water.gameObject.layer, camera2, 0, this._PropertyBlock, this._ShadowCastingMode, false, (!(this._ReflectionProbeAnchor != null)) ? this._Water.transform : this._ReflectionProbeAnchor, false);
					if (waterCamera.ContainingWater != null && waterCamera.Type == WaterCamera.CameraType.Normal)
					{
						Graphics.DrawMesh(transformedMeshes[i], matrix, this._Water.Materials.SurfaceBackMaterial, this._Water.gameObject.layer, camera2, 0, this._PropertyBlock, this._ShadowCastingMode, false, (!(this._ReflectionProbeAnchor != null)) ? this._Water.transform : this._ReflectionProbeAnchor, false);
					}
				}
			}
			else
			{
				Material variant = UtilityShaderVariants.Instance.GetVariant(shader, this._Water.Materials.UsedKeywords);
				for (int j = 0; j < transformedMeshes.Length; j++)
				{
					commandBuffer.DrawMesh(transformedMeshes[j], matrix, variant, 0, 0, this._PropertyBlock);
				}
			}
		}

		public void RenderVolumes(CommandBuffer commandBuffer, Shader shader, bool twoPass)
		{
			if (!this._Water.enabled)
			{
				return;
			}
			Material variant = UtilityShaderVariants.Instance.GetVariant(shader, this._Water.Materials.UsedKeywords);
			List<WaterVolumeAdd> volumesDirect = this._Water.Volume.GetVolumesDirect();
			WaterRenderer.RenderVolumes<WaterVolumeAdd>(commandBuffer, variant, volumesDirect, twoPass);
			List<WaterVolumeSubtract> subtractiveVolumesDirect = this._Water.Volume.GetSubtractiveVolumesDirect();
			WaterRenderer.RenderVolumes<WaterVolumeSubtract>(commandBuffer, variant, subtractiveVolumesDirect, twoPass);
		}

		public void RenderMasks(CommandBuffer commandBuffer)
		{
			for (int i = this._Masks.Count - 1; i >= 0; i--)
			{
				commandBuffer.DrawMesh(this._Masks[i].GetComponent<MeshFilter>().sharedMesh, this._Masks[i].transform.localToWorldMatrix, this._Masks[i].Renderer.sharedMaterial, 0, 0);
			}
		}

		public void PostRender(WaterCamera waterCamera)
		{
			if (this._Water != null)
			{
				this._Water.OnWaterPostRender(waterCamera);
			}
			this.ReleaseTemporaryBuffers();
		}

		public void OnSharedSubtractiveMaskRender(ref bool hasSubtractiveVolumes, ref bool hasAdditiveVolumes, ref bool hasFlatMasks)
		{
			if (!this._Water.enabled)
			{
				return;
			}
			List<WaterVolumeAdd> volumesDirect = this._Water.Volume.GetVolumesDirect();
			int count = volumesDirect.Count;
			for (int i = 0; i < count; i++)
			{
				volumesDirect[i].DisableRenderers();
			}
			List<WaterVolumeSubtract> subtractiveVolumesDirect = this._Water.Volume.GetSubtractiveVolumesDirect();
			int count2 = subtractiveVolumesDirect.Count;
			if (this._UseSharedMask)
			{
				for (int j = 0; j < count2; j++)
				{
					subtractiveVolumesDirect[j].EnableRenderers(false);
				}
				hasSubtractiveVolumes = (hasSubtractiveVolumes || this._Water.Volume.GetSubtractiveVolumesDirect().Count != 0);
				hasAdditiveVolumes = (hasAdditiveVolumes || count != 0);
				hasFlatMasks = (hasFlatMasks || this._Masks.Count != 0);
			}
			else
			{
				for (int k = 0; k < count2; k++)
				{
					subtractiveVolumesDirect[k].DisableRenderers();
				}
			}
		}

		public void OnSharedMaskAdditiveRender()
		{
			if (!this._Water.enabled)
			{
				return;
			}
			if (this._UseSharedMask)
			{
				List<WaterVolumeAdd> volumesDirect = this._Water.Volume.GetVolumesDirect();
				int count = volumesDirect.Count;
				for (int i = 0; i < count; i++)
				{
					volumesDirect[i].EnableRenderers(false);
				}
				List<WaterVolumeSubtract> subtractiveVolumesDirect = this._Water.Volume.GetSubtractiveVolumesDirect();
				int count2 = subtractiveVolumesDirect.Count;
				for (int j = 0; j < count2; j++)
				{
					subtractiveVolumesDirect[j].DisableRenderers();
				}
			}
		}

		public void OnSharedMaskPostRender()
		{
			if (!this._Water.enabled)
			{
				return;
			}
			List<WaterVolumeAdd> volumesDirect = this._Water.Volume.GetVolumesDirect();
			int count = volumesDirect.Count;
			for (int i = 0; i < count; i++)
			{
				volumesDirect[i].EnableRenderers(true);
			}
			List<WaterVolumeSubtract> subtractiveVolumesDirect = this._Water.Volume.GetSubtractiveVolumesDirect();
			int count2 = subtractiveVolumesDirect.Count;
			for (int j = 0; j < count2; j++)
			{
				subtractiveVolumesDirect[j].EnableRenderers(true);
			}
		}

		internal void Awake(Water water)
		{
			this._Water = water;
		}

		internal void OnValidate()
		{
			if (this._VolumeFrontShader == null)
			{
				this._VolumeFrontShader = Shader.Find("UltimateWater/Volumes/Front");
			}
			if (this._VolumeFrontFastShader == null)
			{
				this._VolumeFrontFastShader = Shader.Find("UltimateWater/Volumes/Front Simple");
			}
			if (this._VolumeBackShader == null)
			{
				this._VolumeBackShader = Shader.Find("UltimateWater/Volumes/Back");
			}
		}

		private static void RenderVolumes<T>(CommandBuffer commandBuffer, Material material, List<T> boundingVolumes, bool twoPass) where T : WaterVolumeBase
		{
			for (int i = boundingVolumes.Count - 1; i >= 0; i--)
			{
				T t = boundingVolumes[i];
				MeshRenderer[] volumeRenderers = t.VolumeRenderers;
				T t2 = boundingVolumes[i];
				MeshFilter[] volumeFilters = t2.VolumeFilters;
				if (volumeRenderers != null && volumeRenderers.Length != 0 && volumeRenderers[0].enabled)
				{
					if (!twoPass)
					{
						int shaderPass = (material.passCount != 1) ? 1 : 0;
						for (int j = 0; j < volumeRenderers.Length; j++)
						{
							commandBuffer.DrawMesh(volumeFilters[j].sharedMesh, volumeRenderers[j].transform.localToWorldMatrix, material, 0, shaderPass);
						}
					}
					else
					{
						for (int k = 0; k < volumeRenderers.Length; k++)
						{
							Mesh sharedMesh = volumeFilters[k].sharedMesh;
							commandBuffer.DrawMesh(sharedMesh, volumeRenderers[k].transform.localToWorldMatrix, material, 0, 0);
							commandBuffer.DrawMesh(sharedMesh, volumeRenderers[k].transform.localToWorldMatrix, material, 0, 1);
						}
					}
				}
			}
		}

		private void ReleaseTemporaryBuffers()
		{
			if (this._AdditiveMaskTexture != null)
			{
				RenderTexture.ReleaseTemporary(this._AdditiveMaskTexture);
				this._AdditiveMaskTexture = null;
			}
			if (this._SubtractiveMaskTexture != null)
			{
				RenderTexture.ReleaseTemporary(this._SubtractiveMaskTexture);
				this._SubtractiveMaskTexture = null;
			}
		}

		private void RenderMasks(Camera camera, WaterCamera waterCamera, MaterialPropertyBlock propertyBlock)
		{
			List<WaterVolumeSubtract> subtractiveVolumesDirect = this._Water.Volume.GetSubtractiveVolumesDirect();
			List<WaterVolumeAdd> volumesDirect = this._Water.Volume.GetVolumesDirect();
			if (waterCamera == null || !waterCamera.RenderVolumes || (subtractiveVolumesDirect.Count == 0 && volumesDirect.Count == 0 && this._Masks.Count == 0))
			{
				this.ReleaseTemporaryBuffers();
				return;
			}
			int waterTempLayer = WaterProjectSettings.Instance.WaterTempLayer;
			int waterCollidersLayer = WaterProjectSettings.Instance.WaterCollidersLayer;
			Camera effectsCamera = waterCamera.EffectsCamera;
			if (effectsCamera == null)
			{
				this.ReleaseTemporaryBuffers();
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			this.OnSharedSubtractiveMaskRender(ref flag, ref flag2, ref flag3);
			effectsCamera.CopyFrom(camera);
			effectsCamera.enabled = false;
			effectsCamera.GetComponent<WaterCamera>().enabled = false;
			effectsCamera.renderingPath = RenderingPath.Forward;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			effectsCamera.cullingMask = 1 << waterTempLayer;
			if (subtractiveVolumesDirect.Count != 0)
			{
				if (this._SubtractiveMaskTexture == null)
				{
					this._SubtractiveMaskTexture = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 24, (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
				}
				Graphics.SetRenderTarget(this._SubtractiveMaskTexture);
				int count = subtractiveVolumesDirect.Count;
				for (int i = 0; i < count; i++)
				{
					subtractiveVolumesDirect[i].SetLayer(waterTempLayer);
				}
				TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(camera.pixelWidth, camera.pixelHeight, 24, (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, true, false, false);
				effectsCamera.clearFlags = CameraClearFlags.Color;
				effectsCamera.backgroundColor = new Color(0f, 0f, 0.5f, 0f);
				effectsCamera.targetTexture = temporary;
				effectsCamera.RenderWithShader(this._VolumeFrontShader, "CustomType");
				GL.Clear(true, true, new Color(0f, 0f, 0f, 0f), 0f);
				Shader.SetGlobalTexture("_VolumesFrontDepth", temporary);
				effectsCamera.clearFlags = CameraClearFlags.Nothing;
				effectsCamera.targetTexture = this._SubtractiveMaskTexture;
				effectsCamera.RenderWithShader(this._VolumeBackShader, "CustomType");
				temporary.Dispose();
				for (int j = 0; j < count; j++)
				{
					subtractiveVolumesDirect[j].SetLayer(waterCollidersLayer);
				}
				propertyBlock.SetTexture(ShaderVariables.SubtractiveMask, this._SubtractiveMaskTexture);
			}
			if (volumesDirect.Count != 0)
			{
				this.OnSharedMaskAdditiveRender();
				if (this._AdditiveMaskTexture == null)
				{
					this._AdditiveMaskTexture = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 16, (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
				}
				Graphics.SetRenderTarget(this._AdditiveMaskTexture);
				GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
				int count2 = volumesDirect.Count;
				for (int k = 0; k < count2; k++)
				{
					volumesDirect[k].SetLayer(waterTempLayer);
					volumesDirect[k].EnableRenderers(false);
				}
				effectsCamera.clearFlags = CameraClearFlags.Nothing;
				effectsCamera.targetTexture = this._AdditiveMaskTexture;
				effectsCamera.RenderWithShader((!waterCamera.IsInsideAdditiveVolume) ? this._VolumeFrontFastShader : this._VolumeFrontShader, "CustomType");
				effectsCamera.clearFlags = CameraClearFlags.Nothing;
				effectsCamera.targetTexture = this._AdditiveMaskTexture;
				effectsCamera.RenderWithShader(this._VolumeBackShader, "CustomType");
				for (int l = 0; l < count2; l++)
				{
					volumesDirect[l].SetLayer(waterCollidersLayer);
				}
				propertyBlock.SetTexture(ShaderVariables.AdditiveMask, this._AdditiveMaskTexture);
			}
			this.OnSharedMaskPostRender();
			effectsCamera.targetTexture = null;
		}

		[FormerlySerializedAs("volumeFrontShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _VolumeFrontShader;

		[SerializeField]
		[FormerlySerializedAs("volumeFrontFastShader")]
		[HideInInspector]
		private Shader _VolumeFrontFastShader;

		[SerializeField]
		[FormerlySerializedAs("volumeBackShader")]
		[HideInInspector]
		private Shader _VolumeBackShader;

		[FormerlySerializedAs("reflectionProbeAnchor")]
		[SerializeField]
		private Transform _ReflectionProbeAnchor;

		[FormerlySerializedAs("shadowCastingMode")]
		[SerializeField]
		private ShadowCastingMode _ShadowCastingMode;

		[FormerlySerializedAs("useSharedMask")]
		[SerializeField]
		private bool _UseSharedMask = true;

		private Water _Water;

		private MaterialPropertyBlock _PropertyBlock;

		private RenderTexture _AdditiveMaskTexture;

		private RenderTexture _SubtractiveMaskTexture;

		private readonly List<WaterSimpleMask> _Masks = new List<WaterSimpleMask>();
	}
}
