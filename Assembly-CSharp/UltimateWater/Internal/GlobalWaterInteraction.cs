using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater.Internal
{
	public class GlobalWaterInteraction : MonoBehaviour, ILocalDisplacementMaskRenderer, IDynamicWaterEffects
	{
		public Vector2 WorldUnitsOffset
		{
			get
			{
				return this._WorldUnitsOffset;
			}
			set
			{
				this._WorldUnitsOffset = value;
			}
		}

		public Vector2 WorldUnitsSize
		{
			get
			{
				return this._WorldUnitsSize;
			}
			set
			{
				this._WorldUnitsSize = value;
			}
		}

		public void RenderLocalMask(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			float farClipPlane = overlays.Camera.CameraComponent.farClipPlane;
			Vector3 position = overlays.Camera.transform.position;
			position.y = overlays.DynamicWater.Water.transform.position.y;
			this._InteractionMaskRenderer.transform.position = position;
			this._InteractionMaskRenderer.transform.localScale = new Vector3(farClipPlane, farClipPlane, farClipPlane);
			this._InteractionMaskMaterial.SetVector("_OffsetScale", new Vector4(0.5f + this._WorldUnitsOffset.x / this._WorldUnitsSize.x, 0.5f + this._WorldUnitsOffset.y / this._WorldUnitsSize.y, 1f / this._WorldUnitsSize.x, 1f / this._WorldUnitsSize.y));
			commandBuffer.DrawMesh(this._InteractionMaskRenderer.GetComponent<MeshFilter>().sharedMesh, this._InteractionMaskRenderer.transform.localToWorldMatrix, this._InteractionMaskMaterial, 0, 0);
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
			this.OnValidate();
			this.CreateMaskRenderer();
			base.gameObject.layer = WaterProjectSettings.Instance.WaterTempLayer;
		}

		private void OnEnable()
		{
			DynamicWater.AddRenderer<GlobalWaterInteraction>(this);
		}

		private void OnDisable()
		{
			DynamicWater.RemoveRenderer<GlobalWaterInteraction>(this);
		}

		private void OnValidate()
		{
			if (this._MaskDisplayShader == null)
			{
				this._MaskDisplayShader = Shader.Find("UltimateWater/Utility/ShorelineMaskRenderSimple");
			}
		}

		private void CreateMaskRenderer()
		{
			MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = Quads.BipolarXZ;
			this._InteractionMaskMaterial = new Material(this._MaskDisplayShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._InteractionMaskMaterial.SetTexture("_MainTex", this._IntensityMask);
			this._InteractionMaskRenderer = base.gameObject.AddComponent<MeshRenderer>();
			this._InteractionMaskRenderer.sharedMaterial = this._InteractionMaskMaterial;
			this._InteractionMaskRenderer.enabled = false;
			base.transform.localRotation = Quaternion.identity;
		}

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("maskDisplayShader")]
		private Shader _MaskDisplayShader;

		[FormerlySerializedAs("intensityMask")]
		[SerializeField]
		private Texture2D _IntensityMask;

		[FormerlySerializedAs("worldUnitsOffset")]
		[SerializeField]
		private Vector2 _WorldUnitsOffset;

		[FormerlySerializedAs("worldUnitsSize")]
		[SerializeField]
		private Vector2 _WorldUnitsSize;

		private MeshRenderer _InteractionMaskRenderer;

		private Material _InteractionMaskMaterial;
	}
}
