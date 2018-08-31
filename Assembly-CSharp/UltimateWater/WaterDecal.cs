using System;
using UltimateWater.Internal;
using UltimateWater.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public class WaterDecal : MonoBehaviour, ILocalDisplacementRenderer, ILocalFoamRenderer, ILocalDiffuseRenderer, IDynamicWaterEffects
	{
		public Material DisplacementMaterial
		{
			get
			{
				return this._DisplacementMaterial;
			}
			set
			{
				Material displacementMaterial = this._DisplacementMaterial;
				this._DisplacementMaterial = value;
				if (displacementMaterial == null && this._DisplacementMaterial != null)
				{
					DynamicWater.AddRenderer<ILocalDisplacementRenderer>(this);
				}
				if (displacementMaterial != null && this._DisplacementMaterial == null)
				{
					DynamicWater.RemoveRenderer<ILocalDisplacementRenderer>(this);
				}
			}
		}

		public Material FoamMaterial
		{
			get
			{
				return this._FoamMaterial;
			}
			set
			{
				Material foamMaterial = this._FoamMaterial;
				this._FoamMaterial = value;
				if (foamMaterial == null && this._FoamMaterial != null)
				{
					DynamicWater.AddRenderer<ILocalFoamRenderer>(this);
				}
				if (foamMaterial != null && this._FoamMaterial == null)
				{
					DynamicWater.RemoveRenderer<ILocalFoamRenderer>(this);
				}
			}
		}

		public bool RenderDiffuse
		{
			get
			{
				return this._RenderDiffuse;
			}
			set
			{
				bool renderDiffuse = this._RenderDiffuse;
				this._RenderDiffuse = value;
				if (!renderDiffuse && this._RenderDiffuse)
				{
					DynamicWater.AddRenderer<ILocalFoamRenderer>(this);
				}
				if (renderDiffuse && !this._RenderDiffuse)
				{
					DynamicWater.RemoveRenderer<ILocalFoamRenderer>(this);
				}
			}
		}

		public void RenderLocalDisplacement(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				commandBuffer.DrawRenderer(this._Renderers[i], this._DisplacementMaterial);
			}
		}

		public void RenderLocalFoam(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				commandBuffer.DrawRenderer(this._Renderers[i], this._FoamMaterial);
			}
		}

		public void RenderLocalDiffuse(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				commandBuffer.DrawRenderer(this._Renderers[i], this._Renderers[i].material);
			}
		}

		public void Enable()
		{
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				this._Renderers[i].enabled = true;
			}
		}

		public void Disable()
		{
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				this._Renderers[i].enabled = false;
			}
		}

		private void Awake()
		{
			Renderer[] renderers;
			if (this._UseChildrenRenderers)
			{
				renderers = base.GetComponentsInChildren<Renderer>();
			}
			else
			{
				(renderers = new Renderer[1])[0] = base.GetComponent<Renderer>();
			}
			this._Renderers = renderers;
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				this._Renderers[i].enabled = false;
			}
		}

		private void OnEnable()
		{
			if (this._DisplacementMaterial != null)
			{
				DynamicWater.AddRenderer<ILocalDisplacementRenderer>(this);
			}
			if (this._FoamMaterial != null)
			{
				DynamicWater.AddRenderer<ILocalFoamRenderer>(this);
			}
			if (this._RenderDiffuse)
			{
				DynamicWater.AddRenderer<ILocalDiffuseRenderer>(this);
			}
		}

		private void OnDisable()
		{
			if (this._DisplacementMaterial != null)
			{
				DynamicWater.RemoveRenderer<ILocalDisplacementRenderer>(this);
			}
			if (this._FoamMaterial != null)
			{
				DynamicWater.RemoveRenderer<ILocalFoamRenderer>(this);
			}
			if (this._RenderDiffuse)
			{
				DynamicWater.RemoveRenderer<ILocalDiffuseRenderer>(this);
			}
		}

		private void OnValidate()
		{
			this.DisplacementMaterial = this._DisplacementMaterial;
			this.FoamMaterial = this._FoamMaterial;
			this.RenderDiffuse = this._RenderDiffuse;
		}

		private void OnDrawGizmos()
		{
			MeshFilter component = base.GetComponent<MeshFilter>();
			if (component == null || component.sharedMesh == null)
			{
				return;
			}
			Gizmos.color = new Color(0.5f, 0.2f, 1f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireMesh(component.sharedMesh);
		}

		private string Validation()
		{
			if (this._DisplacementMaterial == null && this._FoamMaterial == null && !this._RenderDiffuse)
			{
				return "Warnings:\n- Set at least one material";
			}
			return string.Empty;
		}

		[Tooltip("Used for rendering water displacements")]
		[SerializeField]
		private Material _DisplacementMaterial;

		[Tooltip("Used for rendering foam")]
		[SerializeField]
		private Material _FoamMaterial;

		[SerializeField]
		private bool _RenderDiffuse;

		[SerializeField]
		private bool _UseChildrenRenderers;

		private Renderer[] _Renderers;

		[SerializeField]
		[InspectorWarning("Validation", InspectorWarningAttribute.InfoType.Error)]
		private int _Validation;
	}
}
