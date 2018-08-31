using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public static class GaussianShader
	{
		public static int VerticalKernel
		{
			get
			{
				GaussianShader.Assign(GaussianShader.KernelType.Vertical);
				return GaussianShader._Kernel[1];
			}
		}

		public static int HorizontalKernel
		{
			get
			{
				GaussianShader.Assign(GaussianShader.KernelType.Horizontal);
				return GaussianShader._Kernel[0];
			}
		}

		public static float Term0
		{
			set
			{
				GaussianShader.Shader.SetFloat(GaussianShader._K0Name, value);
			}
		}

		public static float Term1
		{
			set
			{
				GaussianShader.Shader.SetFloat(GaussianShader._K1Name, value);
			}
		}

		public static float Term2
		{
			set
			{
				GaussianShader.Shader.SetFloat(GaussianShader._K2Name, value);
			}
		}

		public static RenderTexture VerticalInput
		{
			set
			{
				GaussianShader.Shader.SetTexture(GaussianShader.VerticalKernel, GaussianShader._InputName, value);
			}
		}

		public static RenderTexture HorizontalInput
		{
			set
			{
				GaussianShader.Shader.SetTexture(GaussianShader.HorizontalKernel, GaussianShader._InputName, value);
			}
		}

		public static RenderTexture VerticalOutput
		{
			set
			{
				GaussianShader.Shader.SetTexture(GaussianShader.VerticalKernel, GaussianShader._OutputName, value);
			}
		}

		public static RenderTexture HorizontalOutput
		{
			set
			{
				GaussianShader.Shader.SetTexture(GaussianShader.HorizontalKernel, GaussianShader._OutputName, value);
			}
		}

		public static ComputeShader Shader
		{
			get
			{
				return ShaderUtility.Instance.Get(ComputeShaderList.Gauss);
			}
		}

		public static void Dispatch(GaussianShader.KernelType type, int width, int height)
		{
			GaussianShader.Shader.Dispatch(GaussianShader._Kernel[(int)type], width / GaussianShader._ThreadGroupX[(int)type], height / GaussianShader._ThreadGroupY[(int)type], 1 / GaussianShader._ThreadGroupZ[(int)type]);
		}

		private static void Assign(GaussianShader.KernelType type)
		{
			if (GaussianShader._Kernel[(int)type] == -1)
			{
				GaussianShader._Kernel[(int)type] = GaussianShader.Shader.FindKernel(GaussianShader._KernelName[(int)type]);
				uint num;
				uint num2;
				uint num3;
				GaussianShader.Shader.GetKernelThreadGroupSizes(GaussianShader._Kernel[(int)type], out num, out num2, out num3);
				GaussianShader._ThreadGroupX[(int)type] = (int)num;
				GaussianShader._ThreadGroupY[(int)type] = (int)num2;
				GaussianShader._ThreadGroupZ[(int)type] = (int)num3;
			}
		}

		private static readonly string[] _KernelName = new string[]
		{
			"Gaussian_Horizontal",
			"Gaussian_Vertical"
		};

		private static readonly int[] _Kernel = new int[]
		{
			-1,
			-1
		};

		private static readonly int[] _ThreadGroupX = new int[2];

		private static readonly int[] _ThreadGroupY = new int[2];

		private static readonly int[] _ThreadGroupZ = new int[2];

		private static readonly string _InputName = "Input";

		private static readonly string _OutputName = "Output";

		private static readonly string _K0Name = "k0";

		private static readonly string _K1Name = "k1";

		private static readonly string _K2Name = "k2";

		public enum KernelType
		{
			Horizontal,
			Vertical
		}
	}
}
