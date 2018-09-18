using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[RequireComponent(typeof(Renderer))]
	public class WaterSurfaceOverlayRenderer : MonoBehaviour, ILocalDisplacementRenderer, ILocalDisplacementMaskRenderer, ILocalFoamRenderer, IDynamicWaterEffects
	{
		public Material DisplacementAndNormalMaterial
		{
			get
			{
				return this._DisplacementAndNormalMaterial;
			}
		}

		public Material DisplacementMaskMaterial
		{
			get
			{
				return this._DisplacementMaskMaterial;
			}
		}

		public Material FoamMaterial
		{
			get
			{
				return this._FoamMaterial;
			}
		}

		public Renderer Renderer
		{
			get
			{
				return this._RendererComponent;
			}
		}

		public void RenderLocalDisplacement(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			commandBuffer.DrawRenderer(this._RendererComponent, this._DisplacementAndNormalMaterial);
		}

		public void RenderLocalMask(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			commandBuffer.DrawRenderer(this._RendererComponent, this._DisplacementMaskMaterial);
		}

		public void RenderLocalFoam(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			commandBuffer.DrawRenderer(this._RendererComponent, this._FoamMaterial);
		}

		public void Enable()
		{
			throw new NotImplementedException();
		}

		public void Disable()
		{
			throw new NotImplementedException();
		}

		private void Awake()
		{
			this._RendererComponent = base.GetComponent<Renderer>();
		}

		private void OnEnable()
		{
			if (this._DisplacementAndNormalMaterial != null)
			{
				DynamicWater.AddRenderer<ILocalDisplacementRenderer>(this);
			}
			if (this._DisplacementMaskMaterial != null)
			{
				DynamicWater.AddRenderer<ILocalDisplacementMaskRenderer>(this);
			}
			if (this._FoamMaterial != null)
			{
				DynamicWater.AddRenderer<ILocalFoamRenderer>(this);
			}
		}

		private void OnDisable()
		{
			DynamicWater.RemoveRenderer<ILocalDisplacementRenderer>(this);
			DynamicWater.RemoveRenderer<ILocalDisplacementMaskRenderer>(this);
			DynamicWater.RemoveRenderer<ILocalFoamRenderer>(this);
		}

		[SerializeField]
		[FormerlySerializedAs("displacementAndNormalMaterial")]
		private Material _DisplacementAndNormalMaterial;

		[FormerlySerializedAs("displacementMaskMaterial")]
		[SerializeField]
		private Material _DisplacementMaskMaterial;

		[SerializeField]
		[FormerlySerializedAs("foamMaterial")]
		private Material _FoamMaterial;

		private Renderer _RendererComponent;
	}
}
