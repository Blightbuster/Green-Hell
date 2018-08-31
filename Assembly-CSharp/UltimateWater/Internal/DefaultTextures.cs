using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class DefaultTextures : ApplicationSingleton<DefaultTextures>
	{
		public static Texture2D Get(Color color)
		{
			DefaultTextures instance = ApplicationSingleton<DefaultTextures>.Instance;
			if (instance == null)
			{
				return null;
			}
			Texture2D texture2D;
			if (DefaultTextures._Cache.TryGetValue(color, out texture2D))
			{
				return texture2D;
			}
			texture2D = DefaultTextures.CreateColorTexure(color, "[UWS] DefaultTextures - " + color);
			DefaultTextures._Cache.Add(color, texture2D);
			return texture2D;
		}

		protected override void OnDestroy()
		{
			foreach (KeyValuePair<Color, Texture2D> keyValuePair in DefaultTextures._Cache)
			{
				keyValuePair.Value.Destroy();
			}
			DefaultTextures._Cache.Clear();
			base.OnDestroy();
		}

		private static Texture2D CreateColorTexure(Color color, string name)
		{
			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false)
			{
				name = name,
				hideFlags = HideFlags.DontSave
			};
			texture2D.SetPixel(0, 0, color);
			texture2D.SetPixel(1, 0, color);
			texture2D.SetPixel(0, 1, color);
			texture2D.SetPixel(1, 1, color);
			texture2D.Apply(false, true);
			return texture2D;
		}

		private static readonly Dictionary<Color, Texture2D> _Cache = new Dictionary<Color, Texture2D>();
	}
}
