using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater.Internal
{
	[RequireComponent(typeof(Renderer))]
	public sealed class WaterSimpleMask : MonoBehaviour
	{
		public Renderer Renderer { get; private set; }

		public int RenderQueuePriority
		{
			get
			{
				return this._RenderQueuePriority;
			}
		}

		public Water Water
		{
			get
			{
				return this._Water;
			}
			set
			{
				if (this._Water == value)
				{
					return;
				}
				base.enabled = false;
				this._Water = value;
				base.enabled = true;
			}
		}

		private void OnValidate()
		{
			this.SetObjectLayer();
		}

		private void OnEnable()
		{
			this.Renderer = base.GetComponent<Renderer>();
			this.Renderer.enabled = false;
			this.Renderer.material.SetFloat("_WaterId", (float)(1 << this._Water.WaterId));
			this.SetObjectLayer();
			if (this.Renderer == null)
			{
				throw new InvalidOperationException("WaterSimpleMask is attached to an object without any renderer.");
			}
			this._Water.Renderer.AddMask(this);
			this._Water.WaterIdChanged += this.OnWaterIdChanged;
		}

		private void OnDisable()
		{
			this._Water.WaterIdChanged -= this.OnWaterIdChanged;
			this._Water.Renderer.RemoveMask(this);
		}

		private void SetObjectLayer()
		{
			if (base.gameObject.layer != WaterProjectSettings.Instance.WaterTempLayer)
			{
				base.gameObject.layer = WaterProjectSettings.Instance.WaterTempLayer;
			}
		}

		private void OnWaterIdChanged()
		{
			Renderer component = base.GetComponent<Renderer>();
			component.material.SetFloat("_WaterId", (float)(1 << this._Water.WaterId));
		}

		[FormerlySerializedAs("water")]
		[SerializeField]
		private Water _Water;

		[FormerlySerializedAs("renderQueuePriority")]
		[SerializeField]
		private int _RenderQueuePriority;

		private const string _WaterIdName = "_WaterId";
	}
}
