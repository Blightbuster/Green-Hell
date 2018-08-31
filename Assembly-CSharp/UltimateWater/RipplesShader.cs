using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public static class RipplesShader
	{
		public static int Kernel
		{
			get
			{
				if (RipplesShader._Kernel == -1)
				{
					RipplesShader._Kernel = RipplesShader.Shader.FindKernel("Simulation");
					uint threadGroupX;
					uint threadGroupY;
					uint threadGroupZ;
					RipplesShader.Shader.GetKernelThreadGroupSizes(RipplesShader.Kernel, out threadGroupX, out threadGroupY, out threadGroupZ);
					RipplesShader._ThreadGroupX = (int)threadGroupX;
					RipplesShader._ThreadGroupY = (int)threadGroupY;
					RipplesShader._ThreadGroupZ = (int)threadGroupZ;
				}
				return RipplesShader._Kernel;
			}
		}

		public static Vector2 Size
		{
			set
			{
				RipplesShader.Shader.SetVector("SizeInv", new Vector2(1f / value.x, 1f / value.y));
				RipplesShader.Shader.SetInts("Size", new int[]
				{
					(int)value.x,
					(int)value.y
				});
			}
		}

		public static ComputeShader Shader
		{
			get
			{
				return ShaderUtility.Instance.Get(ComputeShaderList.Simulation);
			}
		}

		public static void SetDepth(Texture value, Material material)
		{
			RipplesShader.SetTextureShaderVariable("Depth", value, material);
		}

		public static void SetPreviousDepth(Texture value, Material material)
		{
			RipplesShader.SetTextureShaderVariable("DepthT1", value, material);
		}

		public static void SetStaticDepth(Texture value, Material material)
		{
			RipplesShader.SetTextureShaderVariable("StaticDepth", value, material);
		}

		public static void SetPrimary(Texture value, Material material)
		{
			RipplesShader.SetTextureShaderVariable("MatrixT1", value, material);
		}

		public static void SetSecondary(Texture value, Material material)
		{
			RipplesShader.SetTextureShaderVariable("MatrixT2", value, material);
		}

		public static void SetDamping(float value, Material material)
		{
			RipplesShader.SetFloatShaderVariable("Damping", value, material);
		}

		public static void SetPropagation(float value, Material material)
		{
			RipplesShader.SetFloatShaderVariable("Propagation", value, material);
		}

		public static void SetGain(float value, Material material)
		{
			RipplesShader.SetFloatShaderVariable("Gain", value, material);
		}

		public static void SetHeightGain(float value, Material material)
		{
			RipplesShader.SetFloatShaderVariable("HeightGain", value, material);
		}

		public static void SetHeightOffset(float value, Material material)
		{
			RipplesShader.SetFloatShaderVariable("HeightOffset", value, material);
		}

		public static void Dispatch(int width, int height)
		{
			RipplesShader.Shader.Dispatch(RipplesShader.Kernel, width / RipplesShader._ThreadGroupX, height / RipplesShader._ThreadGroupY, 1 / RipplesShader._ThreadGroupZ);
		}

		private static void SetFloatShaderVariable(string name, float value, Material material)
		{
			WaterRipplesData.ShaderModes shaderMode = WaterQualitySettings.Instance.Ripples.ShaderMode;
			if (shaderMode != WaterRipplesData.ShaderModes.ComputeShader)
			{
				if (shaderMode == WaterRipplesData.ShaderModes.PixelShader)
				{
					material.SetFloat(name, value);
				}
			}
			else
			{
				RipplesShader.Shader.SetFloat(name, value);
			}
		}

		private static void SetTextureShaderVariable(string name, Texture value, Material material)
		{
			WaterRipplesData.ShaderModes shaderMode = WaterQualitySettings.Instance.Ripples.ShaderMode;
			if (shaderMode != WaterRipplesData.ShaderModes.ComputeShader)
			{
				if (shaderMode == WaterRipplesData.ShaderModes.PixelShader)
				{
					material.SetTexture(name, value);
				}
			}
			else
			{
				RipplesShader.Shader.SetTexture(RipplesShader.Kernel, name, value);
			}
		}

		private static int _ThreadGroupX;

		private static int _ThreadGroupY;

		private static int _ThreadGroupZ;

		private const string _KernelName = "Simulation";

		private const string _DepthName = "Depth";

		private const string _PreviousDepthName = "DepthT1";

		private const string _StaticDepthName = "StaticDepth";

		private const string _SizeInvName = "SizeInv";

		private const string _SizeName = "Size";

		private const string _PrimaryName = "MatrixT1";

		private const string _SecondaryName = "MatrixT2";

		private const string _PropagationName = "Propagation";

		private const string _DampingName = "Damping";

		private const string _GainName = "Gain";

		private const string _HeightGainName = "HeightGain";

		private const string _HeightOffsetName = "HeightOffset";

		private static int _Kernel = -1;
	}
}
