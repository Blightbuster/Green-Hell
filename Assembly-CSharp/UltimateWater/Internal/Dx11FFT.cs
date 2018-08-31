using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public sealed class Dx11FFT : GpuFFT
	{
		public Dx11FFT(ComputeShader shader, int resolution, bool highPrecision, bool twoChannels) : base(resolution, highPrecision, twoChannels, true)
		{
			this._Shader = shader;
			this._KernelIndex = this._NumButterflies - 5 << 1;
			if (twoChannels)
			{
				this._KernelIndex += 10;
			}
		}

		public override void SetupMaterials()
		{
		}

		public override void ComputeFFT(Texture tex, RenderTexture target)
		{
			TemporaryRenderTexture temporary = this._RenderTexturesSet.GetTemporary();
			if (!target.IsCreated())
			{
				target.enableRandomWrite = true;
				target.Create();
			}
			this._Shader.SetTexture(this._KernelIndex, "_ButterflyTex", base.Butterfly);
			this._Shader.SetTexture(this._KernelIndex, "_SourceTex", tex);
			this._Shader.SetTexture(this._KernelIndex, "_TargetTex", temporary);
			this._Shader.Dispatch(this._KernelIndex, 1, tex.height, 1);
			this._Shader.SetTexture(this._KernelIndex + 1, "_ButterflyTex", base.Butterfly);
			this._Shader.SetTexture(this._KernelIndex + 1, "_SourceTex", temporary);
			this._Shader.SetTexture(this._KernelIndex + 1, "_TargetTex", target);
			this._Shader.Dispatch(this._KernelIndex + 1, 1, tex.height, 1);
			temporary.Dispose();
		}

		protected override void FillButterflyTexture(Texture2D butterfly, int[][] indices, Vector2[][] weights)
		{
			for (int i = 0; i < this._NumButterflies; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					int num = (j != 0) ? this._Resolution : 0;
					for (int k = 0; k < this._Resolution; k++)
					{
						int num2 = this._NumButterflies - i - 1;
						int num3 = k << 1;
						Color color;
						color.r = (float)(indices[num2][num3] + num);
						color.g = (float)(indices[num2][num3 + 1] + num);
						color.b = weights[i][k].x;
						color.a = weights[i][k].y;
						butterfly.SetPixel(num + k, i, color);
					}
				}
			}
		}

		private readonly ComputeShader _Shader;

		private readonly int _KernelIndex;
	}
}
