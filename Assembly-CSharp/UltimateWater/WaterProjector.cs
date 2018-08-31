using System;
using UltimateWater.Internal;
using UltimateWater.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	public class WaterProjector : MonoBehaviour, ILocalDiffuseRenderer, ILocalDisplacementRenderer, IDynamicWaterEffects
	{
		public void RenderLocalDiffuse(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			if (this._Type != WaterProjector.Type.Diffuse)
			{
				return;
			}
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				commandBuffer.DrawRenderer(this._Renderers[i], this._Renderers[i].sharedMaterial);
			}
		}

		public void RenderLocalDisplacement(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			if (this._Type != WaterProjector.Type.Displacement || this._Displacement == null)
			{
				return;
			}
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				commandBuffer.DrawRenderer(this._Renderers[i], this._Displacement);
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
			if (this._UseChildrenRenderers)
			{
				this._Renderers = base.GetComponentsInChildren<Renderer>();
			}
			else
			{
				Renderer component = base.GetComponent<Renderer>();
				if (component != null)
				{
					this._Renderers = new Renderer[]
					{
						component
					};
				}
			}
			if (this._Renderers == null)
			{
				Debug.LogError("[WaterProjector] : no renderers found");
				base.enabled = false;
				return;
			}
			for (int i = 0; i < this._Renderers.Length; i++)
			{
				this._Renderers[i].enabled = false;
			}
			if (this._Type == WaterProjector.Type.Displacement && this._Displacement == null)
			{
				this._Displacement = Resources.Load<Material>("Systems/Ultimate Water System/Materials/Overlay (Displacements)");
			}
		}

		private void OnEnable()
		{
			this.Unregister();
			this.Register();
		}

		private void OnDisable()
		{
			this.Unregister();
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

		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				if (this._Type == WaterProjector.Type.Displacement && this._Displacement == null)
				{
					this._Displacement = Resources.Load<Material>("Systems/Ultimate Water System/Materials/Overlay (Displacements)");
				}
				return;
			}
			this.Unregister();
			this.Register();
		}

		private void Register()
		{
			DynamicWater.AddRenderer<WaterProjector>(this);
		}

		private void Unregister()
		{
			DynamicWater.RemoveRenderer<WaterProjector>(this);
		}

		private string Warning()
		{
			bool flag = true;
			if (this._UseChildrenRenderers)
			{
				Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
				if (componentsInChildren == null || componentsInChildren.Length == 0)
				{
					flag = false;
				}
			}
			else if (base.GetComponent<Renderer>() == null)
			{
				flag = false;
			}
			return (!flag) ? "error: no renderers found,\nadd renderer to GameObject or its children" : string.Empty;
		}

		[SerializeField]
		private WaterProjector.Type _Type;

		[SerializeField]
		private Material _Displacement;

		[SerializeField]
		private bool _UseChildrenRenderers;

		private Renderer[] _Renderers;

		[SerializeField]
		[InspectorWarning("Warning", InspectorWarningAttribute.InfoType.Error)]
		private string _Warning;

		public enum Type
		{
			Diffuse,
			Displacement
		}
	}
}
