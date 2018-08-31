using System;
using UnityEngine;

namespace MirzaBeig.Shaders.ImageEffects
{
	[ExecuteInEditMode]
	[Serializable]
	public class Sharpen : IEBase
	{
		private void Awake()
		{
			base.shader = Shader.Find("Hidden/Mirza Beig/Image Effects/Sharpen");
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.material.SetFloat("_strength", this.strength);
			base.material.SetFloat("_edgeMult", this.edgeMult);
			base.blit(source, destination);
		}

		[Range(-2f, 2f)]
		public float strength = 0.5f;

		[Range(0f, 8f)]
		public float edgeMult = 0.2f;
	}
}
