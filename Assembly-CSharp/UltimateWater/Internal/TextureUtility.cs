using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public static class TextureUtility
	{
		public static void Release(ref RenderTexture texture)
		{
			if (texture != null)
			{
				texture.Release();
				texture.Destroy();
			}
			texture = null;
		}

		public static void Release(ref Texture2D texture)
		{
			if (texture != null)
			{
				texture.Destroy();
			}
			texture = null;
		}

		public static void Swap<T>(ref T left, ref T right)
		{
			T t = left;
			left = right;
			right = t;
		}

		public static void Clear(this RenderTexture texture, bool clearDepth = false, bool clearColor = true)
		{
			texture.Clear(Color.clear, clearDepth, clearColor);
		}

		public static void Clear(this RenderTexture texture, Color color, bool clearDepth = false, bool clearColor = true)
		{
			Graphics.SetRenderTarget(texture);
			GL.Clear(clearDepth, clearColor, color);
			Graphics.SetRenderTarget(null);
		}

		public static void Resize(this RenderTexture texture, int width, int height)
		{
			if (texture == null)
			{
				Debug.LogWarning("Trying to resize null RenderTexture");
				return;
			}
			if (!texture.IsCreated())
			{
				Debug.LogWarning("Trying to resize not created RenderTexture");
				return;
			}
			if (width <= 0 || height <= 0)
			{
				Debug.LogWarning("Trying to resize to invalid(<=0) width or height ");
				return;
			}
			if (texture.width == width && texture.height == height)
			{
				return;
			}
			texture.Release();
			texture.width = width;
			texture.height = height;
			texture.Create();
		}

		public static bool Verify(this RenderTexture texture, bool clear = true)
		{
			if (texture == null)
			{
				Debug.LogWarning("Trying to resolve null RenderTexture");
				return false;
			}
			if (texture.IsCreated())
			{
				return false;
			}
			texture.Create();
			if (clear)
			{
				texture.Clear(Color.clear, false, true);
			}
			return true;
		}

		public static RenderTexture CreateRenderTexture(this RenderTexture template)
		{
			return template.GetDesc().CreateRenderTexture();
		}

		public static RenderTexture CreateRenderTexture(this TextureUtility.RenderTextureDesc desc)
		{
			RenderTexture renderTexture = new RenderTexture(desc.Width, desc.Height, desc.Depth, desc.Format, desc.ColorSpace)
			{
				name = desc.Name,
				volumeDepth = desc.VolumeDepth,
				antiAliasing = desc.Antialiasing,
				hideFlags = desc.Flags,
				filterMode = desc.Filter,
				wrapMode = desc.Wrap,
				autoGenerateMips = desc.GenerateMipmaps,
				useMipMap = desc.UseMipmaps,
				mipMapBias = desc.MipmapBias
			};
			if (desc.EnableRandomWrite)
			{
				renderTexture.Release();
				renderTexture.enableRandomWrite = true;
				renderTexture.Create();
			}
			else
			{
				renderTexture.Create();
			}
			return renderTexture;
		}

		public static RenderTexture CreateTemporary(this RenderTexture template)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(template.width, template.height, template.depth, template.format, (!template.sRGB) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB, template.antiAliasing);
			temporary.autoGenerateMips = template.autoGenerateMips;
			temporary.wrapMode = template.wrapMode;
			temporary.filterMode = template.filterMode;
			temporary.volumeDepth = template.volumeDepth;
			temporary.anisoLevel = template.anisoLevel;
			temporary.mipMapBias = template.mipMapBias;
			bool flag = false;
			bool flag2 = temporary.useMipMap != template;
			bool flag3 = temporary.enableRandomWrite && !template.enableRandomWrite;
			flag = (flag || flag2);
			flag = (flag || flag3);
			if (flag)
			{
				temporary.Release();
				if (flag2)
				{
					temporary.useMipMap = template.useMipMap;
				}
				if (flag3)
				{
					temporary.enableRandomWrite = template.enableRandomWrite;
				}
				temporary.Create();
			}
			temporary.Create();
			return temporary;
		}

		public static void ReleaseTemporary(this RenderTexture texture)
		{
			if (texture != null)
			{
				RenderTexture.ReleaseTemporary(texture);
			}
		}

		public static TextureUtility.RenderTextureDesc GetDesc(this RenderTexture source)
		{
			return new TextureUtility.RenderTextureDesc(source);
		}

		public struct RenderTextureDesc
		{
			public RenderTextureDesc(string name)
			{
				this.Width = 0;
				this.Height = 0;
				this.Depth = 0;
				this.Format = RenderTextureFormat.Default;
				this.ColorSpace = RenderTextureReadWrite.Linear;
				this.Name = name;
				this.VolumeDepth = 0;
				this.Antialiasing = 1;
				this.Flags = HideFlags.DontSave;
				this.Filter = FilterMode.Bilinear;
				this.Wrap = TextureWrapMode.Clamp;
				this.EnableRandomWrite = false;
				this.GenerateMipmaps = false;
				this.UseMipmaps = false;
				this.MipmapBias = 0f;
			}

			public RenderTextureDesc(RenderTexture source)
			{
				this.Name = "RenderTexture";
				this.Width = source.width;
				this.Height = source.height;
				this.Depth = source.depth;
				this.VolumeDepth = source.volumeDepth;
				this.Antialiasing = source.antiAliasing;
				this.Format = source.format;
				this.Flags = source.hideFlags;
				this.Filter = source.filterMode;
				this.Wrap = source.wrapMode;
				this.ColorSpace = ((!source.sRGB) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
				this.EnableRandomWrite = source.enableRandomWrite;
				this.GenerateMipmaps = source.autoGenerateMips;
				this.UseMipmaps = source.useMipMap;
				this.MipmapBias = source.mipMapBias;
			}

			public string Name;

			public int Width;

			public int Height;

			public int Depth;

			public int VolumeDepth;

			public int Antialiasing;

			public RenderTextureFormat Format;

			public HideFlags Flags;

			public FilterMode Filter;

			public TextureWrapMode Wrap;

			public RenderTextureReadWrite ColorSpace;

			public bool EnableRandomWrite;

			public bool GenerateMipmaps;

			public bool UseMipmaps;

			public float MipmapBias;
		}
	}
}
