using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public abstract class GpuFFT
	{
		protected GpuFFT(int resolution, bool highPrecision, bool twoChannels, bool usesUAV)
		{
			this._Resolution = resolution;
			this._HighPrecision = highPrecision;
			this._NumButterflies = (int)(Mathf.Log((float)resolution) / Mathf.Log(2f));
			this._NumButterfliesPow2 = Mathf.NextPowerOfTwo(this._NumButterflies);
			this._TwoChannels = twoChannels;
			this._UsesUav = usesUAV;
			this.RetrieveRenderTexturesSet();
			this.CreateTextures();
		}

		public Texture2D Butterfly
		{
			get
			{
				return this._Butterfly;
			}
		}

		public int Resolution
		{
			get
			{
				return this._Resolution;
			}
		}

		public abstract void SetupMaterials();

		public abstract void ComputeFFT(Texture tex, RenderTexture target);

		public virtual void Dispose()
		{
			if (this._Butterfly != null)
			{
				this._Butterfly.Destroy();
				this._Butterfly = null;
			}
		}

		private void CreateTextures()
		{
			this.CreateButterflyTexture();
		}

		private void RetrieveRenderTexturesSet()
		{
			RenderTextureFormat format = (!this._TwoChannels) ? ((!this._HighPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat) : ((!this._HighPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat);
			this._RenderTexturesSet = RenderTexturesCache.GetCache(this._Resolution << 1, this._Resolution << 1, 0, format, true, this._UsesUav, false);
		}

		protected virtual void FillButterflyTexture(Texture2D butterfly, int[][] indices, Vector2[][] weights)
		{
			float num = (float)(this._Resolution << 1);
			for (int i = 0; i < this._NumButterflies; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					int num2 = (j != 0) ? this._Resolution : 0;
					for (int k = 0; k < this._Resolution; k++)
					{
						int num3 = this._NumButterflies - i - 1;
						int num4 = k << 1;
						Color color;
						color.r = ((float)(indices[num3][num4] + num2) + 0.5f) / num;
						color.g = ((float)(indices[num3][num4 + 1] + num2) + 0.5f) / num;
						color.b = weights[i][k].x;
						color.a = weights[i][k].y;
						butterfly.SetPixel(num2 + k, i, color);
					}
				}
			}
		}

		private void CreateButterflyTexture()
		{
			this._Butterfly = new Texture2D(this._Resolution << 1, this._NumButterfliesPow2, (!this._HighPrecision) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat, false, true)
			{
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			int[][] indices;
			Vector2[][] weights;
			ButterflyFFTUtility.ComputeButterfly(this._Resolution, this._NumButterflies, out indices, out weights);
			this.FillButterflyTexture(this._Butterfly, indices, weights);
			this._Butterfly.Apply();
		}

		private Texture2D _Butterfly;

		protected RenderTexturesCache _RenderTexturesSet;

		protected int _Resolution;

		protected int _NumButterflies;

		protected int _NumButterfliesPow2;

		protected bool _TwoChannels;

		private readonly bool _HighPrecision;

		private readonly bool _UsesUav;
	}
}
