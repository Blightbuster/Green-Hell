using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class RenderTexturesCache
	{
		public RenderTexturesCache(ulong hash, int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps)
		{
			this._Hash = hash;
			this._Width = width;
			this._Height = height;
			this._DepthBuffer = depthBuffer;
			this._Format = format;
			this._Linear = linear;
			this._Uav = uav;
			this._MipMaps = mipMaps;
			this._RenderTextures = new Queue<RenderTexture>();
		}

		public static RenderTexturesCache GetCache(int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps = false)
		{
			RenderTexturesCache.RenderTexturesUpdater.EnsureInstance();
			ulong num = 0UL;
			num |= (ulong)width;
			num |= (ulong)((ulong)height << 16);
			num |= (ulong)((ulong)((long)depthBuffer) << 29);
			num |= ((!linear) ? 0UL : 1UL) << 34;
			num |= ((!uav) ? 0UL : 1UL) << 35;
			num |= ((!mipMaps) ? 0UL : 1UL) << 36;
			num |= (ulong)((ulong)((long)format) << 37);
			RenderTexturesCache result;
			if (!RenderTexturesCache._Cache.TryGetValue(num, out result))
			{
				result = (RenderTexturesCache._Cache[num] = new RenderTexturesCache(num, width, height, depthBuffer, format, linear, uav, mipMaps));
			}
			return result;
		}

		public static TemporaryRenderTexture GetTemporary(int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps = false)
		{
			return RenderTexturesCache.GetCache(width, height, depthBuffer, format, linear, uav, mipMaps).GetTemporary();
		}

		public TemporaryRenderTexture GetTemporary()
		{
			return new TemporaryRenderTexture(this);
		}

		public RenderTexture GetTemporaryDirect()
		{
			RenderTexture renderTexture;
			if (this._RenderTextures.Count == 0)
			{
				renderTexture = new RenderTexture(this._Width, this._Height, this._DepthBuffer, this._Format, (!this._Linear) ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
				renderTexture.hideFlags = HideFlags.DontSave;
				renderTexture.name = "[UWS] RenderTexturesCache - Temporary#" + this._Hash;
				renderTexture.filterMode = FilterMode.Point;
				renderTexture.anisoLevel = 1;
				renderTexture.wrapMode = TextureWrapMode.Repeat;
				renderTexture.useMipMap = this._MipMaps;
				renderTexture.autoGenerateMips = this._MipMaps;
				if (this._Uav)
				{
					renderTexture.enableRandomWrite = true;
				}
			}
			else
			{
				renderTexture = this._RenderTextures.Dequeue();
			}
			if (this._Uav && !renderTexture.IsCreated())
			{
				renderTexture.Create();
			}
			if (this._RenderTextures.Count == 0)
			{
				this._LastFrameAllUsed = Time.frameCount;
			}
			return renderTexture;
		}

		public void ReleaseTemporaryDirect(RenderTexture renderTexture)
		{
			this._RenderTextures.Enqueue(renderTexture);
		}

		internal void Update(int frame)
		{
			if (frame - this._LastFrameAllUsed > 3 && this._RenderTextures.Count != 0)
			{
				RenderTexture obj = this._RenderTextures.Dequeue();
				obj.Destroy();
			}
		}

		internal void Release()
		{
			foreach (RenderTexture renderTexture in this._RenderTextures)
			{
				if (renderTexture != null)
				{
					renderTexture.Release();
					renderTexture.Destroy();
				}
			}
			this._RenderTextures.Clear();
		}

		private static readonly Dictionary<ulong, RenderTexturesCache> _Cache = new Dictionary<ulong, RenderTexturesCache>(UInt64EqualityComparer.Default);

		private readonly Queue<RenderTexture> _RenderTextures;

		private int _LastFrameAllUsed;

		private readonly ulong _Hash;

		private readonly int _Width;

		private readonly int _Height;

		private readonly int _DepthBuffer;

		private readonly RenderTextureFormat _Format;

		private readonly bool _Linear;

		private readonly bool _Uav;

		private readonly bool _MipMaps;

		[ExecuteInEditMode]
		public class RenderTexturesUpdater : MonoBehaviour
		{
			public static void EnsureInstance()
			{
				if (RenderTexturesCache.RenderTexturesUpdater._Instance != null)
				{
					return;
				}
				GameObject gameObject = new GameObject("Water.RenderTexturesCache")
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				RenderTexturesCache.RenderTexturesUpdater._Instance = gameObject.AddComponent<RenderTexturesCache.RenderTexturesUpdater>();
			}

			private void Update()
			{
				int frameCount = Time.frameCount;
				Dictionary<ulong, RenderTexturesCache>.Enumerator enumerator = RenderTexturesCache._Cache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<ulong, RenderTexturesCache> keyValuePair = enumerator.Current;
					keyValuePair.Value.Update(frameCount);
				}
				enumerator.Dispose();
			}

			private void OnApplicationQuit()
			{
				Dictionary<ulong, RenderTexturesCache>.Enumerator enumerator = RenderTexturesCache._Cache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<ulong, RenderTexturesCache> keyValuePair = enumerator.Current;
					keyValuePair.Value.Release();
				}
				enumerator.Dispose();
			}

			private static RenderTexturesCache.RenderTexturesUpdater _Instance;
		}
	}
}
