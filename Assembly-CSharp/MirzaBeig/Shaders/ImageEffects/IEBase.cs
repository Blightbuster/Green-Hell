using System;
using UnityEngine;

namespace MirzaBeig.Shaders.ImageEffects
{
	[ExecuteInEditMode]
	[Serializable]
	public class IEBase : MonoBehaviour
	{
		protected Material material
		{
			get
			{
				if (!this._material)
				{
					this._material = new Material(this.shader);
					this._material.hideFlags = HideFlags.HideAndDontSave;
				}
				return this._material;
			}
		}

		protected Shader shader { get; set; }

		protected Camera camera
		{
			get
			{
				if (!this._camera)
				{
					this._camera = base.GetComponent<Camera>();
				}
				return this._camera;
			}
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
		}

		protected void blit(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, this.material);
		}

		private void OnDisable()
		{
			if (this._material)
			{
				UnityEngine.Object.DestroyImmediate(this._material);
			}
		}

		private Material _material;

		private Camera _camera;
	}
}
