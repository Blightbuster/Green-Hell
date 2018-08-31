using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Dynamic/Water Interactive")]
	[RequireComponent(typeof(Renderer))]
	public sealed class WaterInteractive : MonoBehaviour, IWavesInteractive, IDynamicWaterEffects
	{
		public void Render(CommandBuffer commandBuffer)
		{
			if (!this._Renderer.enabled)
			{
				return;
			}
			commandBuffer.DrawRenderer(this._Renderer, this._Material);
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		private void Awake()
		{
			this._Material = ShaderUtility.Instance.CreateMaterial(ShaderList.Velocity, HideFlags.None);
			if (this._Material.IsNullReference(this))
			{
				return;
			}
			this._Renderer = base.GetComponent<Renderer>();
			if (this._Renderer.IsNullReference(this))
			{
				return;
			}
			this._Previous = base.transform.localToWorldMatrix;
		}

		private void OnEnable()
		{
			DynamicWater.AddRenderer<WaterInteractive>(this);
		}

		private void OnDisable()
		{
			DynamicWater.RemoveRenderer<WaterInteractive>(this);
		}

		private void FixedUpdate()
		{
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			this._Material.SetMatrix("_PreviousWorld", this._Previous);
			this._Material.SetMatrix("_CurrentWorld", localToWorldMatrix);
			this._Material.SetFloat("_Data", this.Multiplier / Time.fixedDeltaTime);
			this._Previous = localToWorldMatrix;
		}

		[Tooltip("How much velocity modifies wave amplitude")]
		public float Multiplier = 1f;

		private Matrix4x4 _Previous;

		private Renderer _Renderer;

		private Material _Material;
	}
}
