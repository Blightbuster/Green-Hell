using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class UtilityShaderVariants
	{
		private UtilityShaderVariants()
		{
			this._Materials = new Dictionary<int, Material>();
		}

		public static UtilityShaderVariants Instance
		{
			get
			{
				return (UtilityShaderVariants._Instance == null) ? (UtilityShaderVariants._Instance = new UtilityShaderVariants()) : UtilityShaderVariants._Instance;
			}
		}

		public Material GetVariant(Shader shader, string keywords)
		{
			int key = shader.GetInstanceID() ^ keywords.GetHashCode();
			Material material;
			if (!this._Materials.TryGetValue(key, out material))
			{
				material = new Material(shader)
				{
					hideFlags = HideFlags.DontSave,
					shaderKeywords = keywords.Split(new char[]
					{
						' '
					})
				};
				this._Materials[key] = material;
			}
			return material;
		}

		private readonly Dictionary<int, Material> _Materials;

		private static UtilityShaderVariants _Instance;
	}
}
