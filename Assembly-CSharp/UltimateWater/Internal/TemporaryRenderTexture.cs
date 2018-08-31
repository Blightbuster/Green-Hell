using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public struct TemporaryRenderTexture : IDisposable
	{
		internal TemporaryRenderTexture(RenderTexturesCache renderTexturesCache)
		{
			this._Cache = renderTexturesCache;
			this._RenderTexture = renderTexturesCache.GetTemporaryDirect();
		}

		public RenderTexture Texture
		{
			get
			{
				return this._RenderTexture;
			}
		}

		public void Dispose()
		{
			if (this._RenderTexture == null)
			{
				return;
			}
			this._Cache.ReleaseTemporaryDirect(this._RenderTexture);
			this._RenderTexture = null;
		}

		public static implicit operator RenderTexture(TemporaryRenderTexture that)
		{
			return that.Texture;
		}

		private RenderTexture _RenderTexture;

		private readonly RenderTexturesCache _Cache;
	}
}
