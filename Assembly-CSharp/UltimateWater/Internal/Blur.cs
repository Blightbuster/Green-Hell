using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater.Internal
{
	[Serializable]
	public class Blur
	{
		public int Iterations
		{
			get
			{
				return this._Iterations;
			}
			set
			{
				float totalSize = this.TotalSize;
				this._Iterations = value;
				this.TotalSize = totalSize;
			}
		}

		public float Size
		{
			get
			{
				return this._Size;
			}
			set
			{
				this._Size = value;
			}
		}

		public float TotalSize
		{
			get
			{
				return this._Size * (float)this._Iterations;
			}
			set
			{
				this._Size = value / (float)this._Iterations;
			}
		}

		public Material BlurMaterial
		{
			get
			{
				if (this._BlurMaterial == null)
				{
					if (this._BlurShader == null)
					{
						this.Validate();
					}
					this._BlurMaterial = new Material(this._BlurShader)
					{
						hideFlags = HideFlags.DontSave
					};
				}
				return this._BlurMaterial;
			}
			set
			{
				this._BlurMaterial = value;
			}
		}

		public int PassIndex
		{
			get
			{
				return this._PassIndex;
			}
			set
			{
				this._PassIndex = value;
			}
		}

		public virtual void Validate(string shaderName, string computeShaderName = null, int kernelIndex = 0)
		{
			this._BlurShader = Shader.Find(shaderName);
			if (computeShaderName != null)
			{
				this._BlurComputeShader = Resources.Load<ComputeShader>(computeShaderName);
				this._ComputeShaderKernelIndex = kernelIndex;
			}
			else
			{
				this._BlurComputeShader = null;
			}
		}

		public void Dispose()
		{
			if (this._BlurMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(this._BlurMaterial);
			}
			if (this._ShaderWeights != null)
			{
				this._ShaderWeights.Release();
				this._ShaderWeights = null;
			}
		}

		public void Validate()
		{
			this.Validate("UltimateWater/Utilities/Blur", "Shaders/Blurs", 0);
		}

		public void Apply(RenderTexture target)
		{
			if (this._Iterations == 0)
			{
				return;
			}
			this.ApplyPixelShader(target);
		}

		protected void ApplyComputeShader(RenderTexture target)
		{
			if (this._ShaderWeights == null)
			{
				this._ShaderWeights = new ComputeBuffer(4, 16);
				this._ShaderWeights.SetData(new Color[]
				{
					new Color(0.324f, 0.324f, 0.324f, 1f),
					new Color(0.232f, 0.232f, 0.232f, 1f),
					new Color(0.0855f, 0.0855f, 0.0855f, 1f),
					new Color(0.0205f, 0.0205f, 0.0205f, 1f)
				});
			}
			int width = target.width;
			int num = (width != 128) ? ((width != 256) ? (this._ComputeShaderKernelIndex + 4) : (this._ComputeShaderKernelIndex + 2)) : this._ComputeShaderKernelIndex;
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(target.width, target.height, 0, target.format, true, true, false);
			this._BlurComputeShader.SetBuffer(num, "weights", this._ShaderWeights);
			this._BlurComputeShader.SetTexture(num, "_MainTex", target);
			this._BlurComputeShader.SetTexture(num, "_Output", temporary);
			this._BlurComputeShader.Dispatch(num, 1, target.height, 1);
			num++;
			this._BlurComputeShader.SetBuffer(num, "weights", this._ShaderWeights);
			this._BlurComputeShader.SetTexture(num, "_MainTex", temporary);
			this._BlurComputeShader.SetTexture(num, "_Output", target);
			this._BlurComputeShader.Dispatch(num, target.width, 1, 1);
			temporary.Dispose();
		}

		protected void ApplyPixelShader(RenderTexture target)
		{
			Material blurMaterial = this.BlurMaterial;
			FilterMode filterMode = target.filterMode;
			target.filterMode = FilterMode.Bilinear;
			RenderTexture temporary = RenderTexture.GetTemporary(target.width, target.height, 0, target.format);
			temporary.filterMode = FilterMode.Bilinear;
			temporary.name = "[UWS] Blur - ApplyPixelShader temporary";
			for (int i = 0; i < this._Iterations; i++)
			{
				float num = this._Size * (1f + (float)i * 0.5f);
				blurMaterial.SetVector(ShaderVariables.Offset, new Vector4(num, 0f, 0f, 0f));
				Graphics.Blit(target, temporary, blurMaterial, this._PassIndex);
				blurMaterial.SetVector(ShaderVariables.Offset, new Vector4(0f, num, 0f, 0f));
				Graphics.Blit(temporary, target, blurMaterial, this._PassIndex);
			}
			target.filterMode = filterMode;
			RenderTexture.ReleaseTemporary(temporary);
		}

		[SerializeField]
		[FormerlySerializedAs("blurComputeShader")]
		[HideInInspector]
		protected ComputeShader _BlurComputeShader;

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("blurShader")]
		private Shader _BlurShader;

		[HideInInspector]
		[FormerlySerializedAs("computeShaderKernelIndex")]
		[SerializeField]
		private int _ComputeShaderKernelIndex;

		[SerializeField]
		[Range(0f, 10f)]
		[FormerlySerializedAs("iterations")]
		private int _Iterations = 1;

		[SerializeField]
		[FormerlySerializedAs("size")]
		private float _Size = 0.005f;

		private Material _BlurMaterial;

		private int _PassIndex;

		protected ComputeBuffer _ShaderWeights;
	}
}
