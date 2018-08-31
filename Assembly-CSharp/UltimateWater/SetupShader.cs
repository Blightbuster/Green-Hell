using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public static class SetupShader
	{
		public static int Kernel
		{
			get
			{
				if (SetupShader._Kernel == -1)
				{
					SetupShader._Kernel = SetupShader.Shader.FindKernel("Setup");
				}
				return SetupShader._Kernel;
			}
		}

		public static int Multi
		{
			get
			{
				if (SetupShader._Multi == -1)
				{
					SetupShader._Multi = SetupShader.Shader.FindKernel("MultiSetup");
				}
				return SetupShader._Multi;
			}
		}

		public static RenderTexture Primary
		{
			set
			{
				SetupShader.Shader.SetTexture(SetupShader.Kernel, "Previous", value);
			}
		}

		public static RenderTexture Secondary
		{
			set
			{
				SetupShader.Shader.SetTexture(SetupShader.Kernel, "Current", value);
			}
		}

		public static Vector2 Position
		{
			set
			{
				SetupShader.Shader.SetVector("Position", value);
			}
		}

		public static float Force
		{
			set
			{
				SetupShader.Shader.SetFloat("Force", value);
			}
		}

		public static int Scale
		{
			set
			{
				SetupShader.Shader.SetInt("_Scale", value);
			}
		}

		public static ComputeShader Shader
		{
			get
			{
				return ShaderUtility.Instance.Get(ComputeShaderList.Setup);
			}
		}

		public static void Dispatch()
		{
			SetupShader.Shader.Dispatch(SetupShader.Kernel, 1, 1, 1);
		}

		public const string KernelName = "Setup";

		public const string MultiName = "MultiSetup";

		public const string PrimaryName = "Previous";

		public const string SecondaryName = "Current";

		public const string PositionName = "Position";

		public const string ForceName = "Force";

		public const string ScaleName = "_Scale";

		private static int _Kernel = -1;

		private static int _Multi = -1;
	}
}
