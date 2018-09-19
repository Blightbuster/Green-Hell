using System;
using System.Collections.Generic;
using UltimateWater.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[ExecuteInEditMode]
	public abstract class WaterVolumeBase : MonoBehaviour
	{
		public Water Water
		{
			get
			{
				return this._Water;
			}
		}

		public bool EnablePhysics
		{
			get
			{
				return this._AffectPhysics;
			}
		}

		public WaterVolumeRenderMode RenderMode
		{
			get
			{
				return this._RenderMode;
			}
		}

		public MeshRenderer[] VolumeRenderers { get; private set; }

		public MeshFilter[] VolumeFilters { get; private set; }

		protected virtual CullMode _CullMode
		{
			get
			{
				return CullMode.Back;
			}
		}

		public void AssignTo(Water water)
		{
			if (this._Water == water || water == null)
			{
				return;
			}
			this.DisposeRenderers();
			this.Unregister(water);
			this._Water = water;
			this.Register(water);
			if (this._RenderMode != WaterVolumeRenderMode.None && Application.isPlaying)
			{
				this.CreateRenderers();
			}
		}

		public void EnableRenderers(bool forBorderRendering)
		{
			if (this.VolumeRenderers == null)
			{
				return;
			}
			bool enabled = !forBorderRendering && this._Water.enabled;
			for (int i = 0; i < this.VolumeRenderers.Length; i++)
			{
				this.VolumeRenderers[i].enabled = enabled;
			}
		}

		public void DisableRenderers()
		{
			if (this.VolumeRenderers == null)
			{
				return;
			}
			for (int i = 0; i < this.VolumeRenderers.Length; i++)
			{
				this.VolumeRenderers[i].enabled = false;
			}
		}

		public bool IsPointInside(Vector3 point)
		{
			for (int i = 0; i < this._Colliders.Length; i++)
			{
				if (this._Colliders[i].IsPointInside(point))
				{
					return true;
				}
			}
			return false;
		}

		public static WaterVolumeBase GetWaterVolume<T>(Collider collider) where T : WaterVolumeBase
		{
			return WaterVolumeBase.GetWaterVolume(collider) as T;
		}

		public static WaterVolumeBase GetWaterVolume(Collider collider)
		{
			WaterVolumeBase waterVolumeBase;
			if (!WaterVolumeBase._ColliderToVolumeCache.TryGetValue(collider, out waterVolumeBase))
			{
				waterVolumeBase = collider.GetComponent<WaterVolumeBase>();
				if (waterVolumeBase != null)
				{
					WaterVolumeBase._ColliderToVolumeCache[collider] = waterVolumeBase;
				}
				else
				{
					waterVolumeBase = (WaterVolumeBase._ColliderToVolumeCache[collider] = null);
				}
			}
			return waterVolumeBase;
		}

		protected void OnEnable()
		{
			this._Colliders = base.GetComponents<Collider>();
			base.gameObject.layer = WaterProjectSettings.Instance.WaterCollidersLayer;
			this.Register(this._Water);
			if (this._RenderMode != WaterVolumeRenderMode.None && this._Water != null && Application.isPlaying)
			{
				this.CreateRenderers();
			}
		}

		protected void OnDisable()
		{
			this.DisposeRenderers();
			this.Unregister(this._Water);
		}

		private void Update()
		{
			if (this.VolumeRenderers == null)
			{
				return;
			}
			for (int i = 0; i < this.VolumeRenderers.Length; i++)
			{
				this.VolumeRenderers[i].SetPropertyBlock(this._Water.Renderer.PropertyBlock);
			}
		}

		protected void OnValidate()
		{
			this._Colliders = base.GetComponents<Collider>();
			for (int i = 0; i < this._Colliders.Length; i++)
			{
				if (!this._Colliders[i].isTrigger)
				{
					this._Colliders[i].isTrigger = true;
				}
			}
			if (this._Water == null)
			{
				this._Water = base.GetComponentInChildren<Water>();
			}
		}

		protected void Reset()
		{
			if (this._Water == null)
			{
				this._Water = Utilities.GetWaterReference();
			}
		}

		protected abstract void Register(Water water);

		protected abstract void Unregister(Water water);

		internal void SetLayer(int layer)
		{
			if (this.VolumeRenderers == null)
			{
				return;
			}
			for (int i = 0; i < this.VolumeRenderers.Length; i++)
			{
				this.VolumeRenderers[i].gameObject.layer = layer;
			}
		}

		private void DisposeRenderers()
		{
			if (this.VolumeRenderers == null)
			{
				return;
			}
			for (int i = 0; i < this.VolumeRenderers.Length; i++)
			{
				if (this.VolumeRenderers[i] != null)
				{
					UnityEngine.Object.Destroy(this.VolumeRenderers[i].gameObject);
				}
			}
			this.VolumeRenderers = null;
			this.VolumeFilters = null;
		}

		protected virtual void CreateRenderers()
		{
			int num = this._Colliders.Length;
			this.VolumeRenderers = new MeshRenderer[num];
			this.VolumeFilters = new MeshFilter[num];
			Material sharedMaterial = (this._CullMode != CullMode.Back) ? this._Water.Materials.VolumeBackMaterial : this._Water.Materials.VolumeMaterial;
			for (int i = 0; i < num; i++)
			{
				Collider collider = this._Colliders[i];
				BoxCollider boxCollider;
				GameObject gameObject;
				SphereCollider sphereCollider;
				MeshCollider meshCollider;
				if ((boxCollider = (collider as BoxCollider)) != null)
				{
					WaterVolumeBase.HandleBoxCollider(out gameObject, boxCollider);
				}
				else if ((sphereCollider = (collider as SphereCollider)) != null)
				{
					WaterVolumeBase.HandleSphereCollider(out gameObject, sphereCollider);
				}
				else if ((meshCollider = (collider as MeshCollider)) != null)
				{
					this.HandleMeshCollider(out gameObject, meshCollider);
				}
				else
				{
					CapsuleCollider capsuleCollider;
					if (!((capsuleCollider = (collider as CapsuleCollider)) != null))
					{
						throw new InvalidOperationException("Unsupported collider type.");
					}
					WaterVolumeBase.HandleCapsuleCollider(out gameObject, capsuleCollider);
				}
				gameObject.hideFlags = HideFlags.DontSave;
				gameObject.name = "Volume Renderer";
				gameObject.layer = WaterProjectSettings.Instance.WaterLayer;
				gameObject.transform.SetParent(base.transform, false);
				UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				component.sharedMaterial = sharedMaterial;
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
				component.lightProbeUsage = LightProbeUsage.Off;
				component.enabled = true;
				component.SetPropertyBlock(this._Water.Renderer.PropertyBlock);
				this.VolumeRenderers[i] = component;
				this.VolumeFilters[i] = component.GetComponent<MeshFilter>();
			}
		}

		private static void HandleBoxCollider(out GameObject obj, BoxCollider boxCollider)
		{
			obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj.transform.localScale = boxCollider.size;
			obj.transform.localPosition = boxCollider.center;
		}

		private void HandleMeshCollider(out GameObject obj, MeshCollider meshCollider)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			if (sharedMesh == null)
			{
				throw new InvalidOperationException("MeshCollider used to mask water doesn't have a mesh assigned.");
			}
			obj = new GameObject
			{
				hideFlags = HideFlags.DontSave
			};
			MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = sharedMesh;
			obj.AddComponent<MeshRenderer>();
		}

		private static void HandleSphereCollider(out GameObject obj, SphereCollider sphereCollider)
		{
			float num = sphereCollider.radius * 2f;
			obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			obj.transform.localScale = new Vector3(num, num, num);
			obj.transform.localPosition = sphereCollider.center;
		}

		private static void HandleCapsuleCollider(out GameObject obj, CapsuleCollider capsuleCollider)
		{
			float num = capsuleCollider.height * 0.5f;
			float num2 = capsuleCollider.radius * 2f;
			obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			obj.transform.localPosition = capsuleCollider.center;
			int direction = capsuleCollider.direction;
			if (direction != 0)
			{
				if (direction != 1)
				{
					if (direction == 2)
					{
						obj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
						obj.transform.localScale = new Vector3(num2, num2, num);
					}
				}
				else
				{
					obj.transform.localScale = new Vector3(num2, num, num2);
				}
			}
			else
			{
				obj.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				obj.transform.localScale = new Vector3(num, num2, num2);
			}
		}

		protected string Validation()
		{
			string text = string.Empty;
			if (this._Water == null)
			{
				text += "warning: assign Water reference";
			}
			return text;
		}

		[SerializeField]
		private Water _Water;

		[SerializeField]
		private WaterVolumeRenderMode _RenderMode = WaterVolumeRenderMode.Basic;

		[SerializeField]
		private bool _AffectPhysics = true;

		private Collider[] _Colliders;

		private static readonly Dictionary<Collider, WaterVolumeBase> _ColliderToVolumeCache = new Dictionary<Collider, WaterVolumeBase>();

		[InspectorWarning("Validation", InspectorWarningAttribute.InfoType.Warning)]
		[SerializeField]
		private string _Validation;
	}
}
