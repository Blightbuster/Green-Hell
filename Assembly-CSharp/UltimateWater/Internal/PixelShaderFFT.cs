using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public sealed class PixelShaderFFT : GpuFFT
	{
		public PixelShaderFFT(Shader fftShader, int resolution, bool highPrecision, bool twoChannels) : base(resolution, highPrecision, twoChannels, false)
		{
			this._Material = new Material(fftShader)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		public override void Dispose()
		{
			base.Dispose();
			if (this._Material == null)
			{
				UnityEngine.Object.Destroy(this._Material);
			}
		}

		public override void SetupMaterials()
		{
			this._Material.SetTexture(ShaderVariables.ButterflyTex, base.Butterfly);
		}

		public override void ComputeFFT(Texture tex, RenderTexture target)
		{
			using (this._RT1 = this._RenderTexturesSet.GetTemporary())
			{
				using (this._RT2 = this._RenderTexturesSet.GetTemporary())
				{
					this.ComputeFFT(tex, null, (!this._TwoChannels) ? 0 : 2);
					this.ComputeFFT(this._RT1, target, (!this._TwoChannels) ? 1 : 3);
				}
			}
		}

		private void ComputeFFT(Texture tex, RenderTexture target, int passIndex)
		{
			this._Material.SetFloat(ShaderVariables.ButterflyPass, 0.5f / (float)this._NumButterfliesPow2);
			Graphics.Blit(tex, this._RT2, this._Material, passIndex);
			this.SwapRT();
			for (int i = 1; i < this._NumButterflies; i++)
			{
				if (target != null && i == this._NumButterflies - 1)
				{
					this._Material.SetFloat(ShaderVariables.ButterflyPass, ((float)i + 0.5f) / (float)this._NumButterfliesPow2);
					Graphics.Blit(this._RT1, target, this._Material, (passIndex != 1) ? 5 : 4);
				}
				else
				{
					this._Material.SetFloat(ShaderVariables.ButterflyPass, ((float)i + 0.5f) / (float)this._NumButterfliesPow2);
					Graphics.Blit(this._RT1, this._RT2, this._Material, passIndex);
				}
				this.SwapRT();
			}
		}

		private void SwapRT()
		{
			TemporaryRenderTexture rt = this._RT1;
			this._RT1 = this._RT2;
			this._RT2 = rt;
		}

		private TemporaryRenderTexture _RT1;

		private TemporaryRenderTexture _RT2;

		private readonly Material _Material;
	}
}
