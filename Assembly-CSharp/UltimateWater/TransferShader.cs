using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public static class TransferShader
	{
		public static int VerticalKernel
		{
			get
			{
				TransferShader.Assign(TransferShader.KernelType.Vertical);
				return TransferShader._Kernel[1];
			}
		}

		public static int HorizontalKernel
		{
			get
			{
				TransferShader.Assign(TransferShader.KernelType.Horizontal);
				return TransferShader._Kernel[0];
			}
		}

		public static RenderTexture HorizontalSourceA
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.HorizontalKernel, "SourceA", value);
			}
		}

		public static RenderTexture HorizontalSourceB
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.HorizontalKernel, "SourceB", value);
			}
		}

		public static RenderTexture HorizontalDestinationA
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.HorizontalKernel, "DestinationA", value);
			}
		}

		public static RenderTexture HorizontalDestinationB
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.HorizontalKernel, "DestinationB", value);
			}
		}

		public static RenderTexture VerticalSourceA
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.VerticalKernel, "SourceA", value);
			}
		}

		public static RenderTexture VerticalSourceB
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.VerticalKernel, "SourceB", value);
			}
		}

		public static RenderTexture VerticalDestinationA
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.VerticalKernel, "DestinationA", value);
			}
		}

		public static RenderTexture VerticalDestinationB
		{
			set
			{
				TransferShader.Shader.SetTexture(TransferShader.VerticalKernel, "DestinationB", value);
			}
		}

		public static int From
		{
			set
			{
				TransferShader.Shader.SetInt("From", value);
			}
		}

		public static int To
		{
			set
			{
				TransferShader.Shader.SetInt("To", value);
			}
		}

		public static float Width
		{
			set
			{
				TransferShader.Shader.SetFloat("InvWidth", 1f / value);
			}
		}

		public static float Height
		{
			set
			{
				TransferShader.Shader.SetFloat("InvHeight", 1f / value);
			}
		}

		public static ComputeShader Shader
		{
			get
			{
				return ShaderUtility.Instance.Get(ComputeShaderList.Transfer);
			}
		}

		public static void Dispatch(TransferShader.KernelType type, int width, int height)
		{
			TransferShader.Shader.Dispatch(TransferShader._Kernel[(int)type], width / TransferShader._ThreadGroupX[(int)type], height / TransferShader._ThreadGroupY[(int)type], 1 / TransferShader._ThreadGroupZ[(int)type]);
		}

		public static void DispatchVertical(int height)
		{
			TransferShader.Dispatch(TransferShader.KernelType.Vertical, height, 1);
		}

		public static void DistpachHorizontal(int width)
		{
			TransferShader.Dispatch(TransferShader.KernelType.Horizontal, 1, width);
		}

		private static void Assign(TransferShader.KernelType type)
		{
			if (TransferShader._Kernel[(int)type] == -1)
			{
				TransferShader._Kernel[(int)type] = TransferShader.Shader.FindKernel(TransferShader._KernelName[(int)type]);
				uint num;
				uint num2;
				uint num3;
				TransferShader.Shader.GetKernelThreadGroupSizes(TransferShader._Kernel[(int)type], out num, out num2, out num3);
				TransferShader._ThreadGroupX[(int)type] = (int)num;
				TransferShader._ThreadGroupY[(int)type] = (int)num2;
				TransferShader._ThreadGroupZ[(int)type] = (int)num3;
			}
		}

		private static readonly string[] _KernelName = new string[]
		{
			"VerticalTransfer",
			"HorizontalTransfer"
		};

		private static readonly int[] _ThreadGroupX = new int[2];

		private static readonly int[] _ThreadGroupY = new int[2];

		private static readonly int[] _ThreadGroupZ = new int[2];

		private const string _SourceAName = "SourceA";

		private const string _SourceBName = "SourceB";

		private const string _DestinationAName = "DestinationA";

		private const string _DestinationBName = "DestinationB";

		private const string _InvWidthName = "InvWidth";

		private const string _InvHeightName = "InvHeight";

		private const string _FromName = "From";

		private const string _ToName = "To";

		private static readonly int[] _Kernel = new int[]
		{
			-1,
			-1
		};

		public enum KernelType
		{
			Horizontal,
			Vertical,
			Unknown
		}
	}
}
