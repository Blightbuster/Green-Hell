using System;
using UnityEngine;

namespace UltimateWater
{
	[Serializable]
	public class WaterRipplesData
	{
		public int Iterations
		{
			get
			{
				return this._Iterations;
			}
		}

		public WaterRipplesData.ShaderModes ShaderMode
		{
			get
			{
				return this._ShaderMode;
			}
		}

		public LayerMask StaticDepthMask
		{
			get
			{
				return this._StaticDepthMask;
			}
		}

		public RenderTextureFormat SimulationFormat
		{
			get
			{
				return this._SimulationFormat;
			}
		}

		public RenderTextureFormat GatherFormat
		{
			get
			{
				return this._GatherFormat;
			}
		}

		public RenderTextureFormat DepthFormat
		{
			get
			{
				return this._DepthFormat;
			}
		}

		[SerializeField]
		[Tooltip("Static objects that interact with water (terrain, pillars, rocks)")]
		private LayerMask _StaticDepthMask;

		[Tooltip("How many simulation steps are performed per frame, the more iterations, the faster the water waves are, but it's expensive")]
		[SerializeField]
		private int _Iterations = 1;

		[Header("Texture formats")]
		[SerializeField]
		[Tooltip("Simulation data (only R channel is used)")]
		private RenderTextureFormat _SimulationFormat = RenderTextureFormat.RHalf;

		[Tooltip("Screen space simulation data gather (only R channel is used)")]
		[SerializeField]
		private RenderTextureFormat _GatherFormat = RenderTextureFormat.RGHalf;

		[Tooltip("Depth data (only R channel is used)")]
		[SerializeField]
		private RenderTextureFormat _DepthFormat = RenderTextureFormat.RHalf;

		[SerializeField]
		[Header("Shaders")]
		private WaterRipplesData.ShaderModes _ShaderMode = WaterRipplesData.ShaderModes.PixelShader;

		[Serializable]
		public enum ShaderModes
		{
			ComputeShader,
			PixelShader
		}
	}
}
