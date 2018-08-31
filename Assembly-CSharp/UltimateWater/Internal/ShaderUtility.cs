using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class ShaderUtility : ScriptableObjectSingleton
	{
		public static ShaderUtility Instance
		{
			get
			{
				return (!(ShaderUtility._Instance != null)) ? (ShaderUtility._Instance = ScriptableObjectSingleton.LoadSingleton<ShaderUtility>()) : ShaderUtility._Instance;
			}
		}

		public Shader Get(ShaderList type)
		{
			if (this._ShaderList == null)
			{
				this.InitializeShaderList();
			}
			this.AddReference(this._ShaderList[(int)type]);
			return this._ShaderList[(int)type];
		}

		public ComputeShader Get(ComputeShaderList type)
		{
			if (this._ComputeShaderList == null)
			{
				this.InitializeComputeShaderList();
			}
			this.AddReference(this._ComputeShaderList[(int)type]);
			return this._ComputeShaderList[(int)type];
		}

		public Material CreateMaterial(ShaderList type, HideFlags flags = HideFlags.None)
		{
			return new Material(this.Get(type))
			{
				hideFlags = flags
			};
		}

		public void Use(ShaderList shader)
		{
			if (this._ShaderList == null)
			{
				this.InitializeShaderList();
			}
			this.AddReference(this._ShaderList[(int)shader]);
		}

		public void Use(ComputeShaderList shader)
		{
			if (this._ComputeShaderList == null)
			{
				this.InitializeComputeShaderList();
			}
			this.AddReference(this._ComputeShaderList[(int)shader]);
		}

		private void InitializeShaderList()
		{
			this._ShaderList = new Dictionary<int, Shader>
			{
				{
					0,
					Shader.Find("UltimateWater/Depth/Depth Copy")
				},
				{
					1,
					Shader.Find("UltimateWater/Depth/Water Depth")
				},
				{
					2,
					Shader.Find("UltimateWater/Volumes/Front")
				},
				{
					3,
					Shader.Find("UltimateWater/Volumes/Back")
				},
				{
					4,
					Shader.Find("UltimateWater/Volumes/Front Simple")
				},
				{
					5,
					Shader.Find("UltimateWater/Dynamic/Depth")
				},
				{
					6,
					Shader.Find("UltimateWater/Dynamic/Velocity")
				},
				{
					7,
					Shader.Find("UltimateWater/Dynamic/Simulation")
				},
				{
					8,
					Shader.Find("UltimateWater/Dynamic/Translate")
				},
				{
					9,
					Shader.Find("UltimateWater/Underwater/Screen-Space Mask")
				},
				{
					10,
					Shader.Find("UltimateWater/Underwater/Base IME")
				},
				{
					11,
					Shader.Find("UltimateWater/Underwater/Compose Underwater Mask")
				},
				{
					12,
					Shader.Find("UltimateWater/IME/Water Drops Mask")
				},
				{
					13,
					Shader.Find("UltimateWater/IME/Water Drops Normal")
				},
				{
					15,
					Shader.Find("UltimateWater/Raindrops/Final")
				},
				{
					14,
					Shader.Find("UltimateWater/Raindrops/Fade")
				},
				{
					16,
					Shader.Find("UltimateWater/Raindrops/PreciseParticle")
				},
				{
					17,
					Shader.Find("UltimateWater/Refraction/Collect Light")
				},
				{
					18,
					Shader.Find("UltimateWater/Refraction/Transmission")
				},
				{
					19,
					Shader.Find("UltimateWater/Deferred/GBuffer0Mix")
				},
				{
					20,
					Shader.Find("UltimateWater/Deferred/GBuffer123Mix")
				},
				{
					21,
					Shader.Find("UltimateWater/Deferred/FinalColorMix")
				},
				{
					22,
					Shader.Find("Hidden/UltimateWater-Internal-DeferredReflections")
				},
				{
					23,
					Shader.Find("Hidden/UltimateWater-Internal-DeferredShading")
				},
				{
					24,
					Shader.Find("UltimateWater/Utility/ShorelineMaskRender")
				},
				{
					25,
					Shader.Find("UltimateWater/Utility/ShorelineMaskRenderSimple")
				},
				{
					26,
					Shader.Find("UltimateWater/Utility/Noise")
				},
				{
					27,
					Shader.Find("UltimateWater/Utility/ShadowEnforcer")
				},
				{
					28,
					Shader.Find("UltimateWater/Utility/MergeDisplacements")
				}
			};
		}

		private void InitializeComputeShaderList()
		{
			this._ComputeShaderList = new Dictionary<int, ComputeShader>
			{
				{
					0,
					Resources.Load<ComputeShader>("Systems/Ultimate Water System/Shaders/Ripples - Simulation")
				},
				{
					1,
					Resources.Load<ComputeShader>("Systems/Ultimate Water System/Shaders/Ripples - Setup")
				},
				{
					2,
					Resources.Load<ComputeShader>("Systems/Ultimate Water System/Shaders/Gauss")
				},
				{
					3,
					Resources.Load<ComputeShader>("Systems/Ultimate Water System/Shaders/Ripples - Transfer")
				}
			};
		}

		private void AddReference(UnityEngine.Object obj)
		{
		}

		private Dictionary<int, Shader> _ShaderList;

		private Dictionary<int, ComputeShader> _ComputeShaderList;

		[SerializeField]
		private List<UnityEngine.Object> _References;

		private static ShaderUtility _Instance;
	}
}
