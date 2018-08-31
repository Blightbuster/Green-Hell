using System;
using UnityEngine;

namespace UltimateWater
{
	public class ResolutionAttribute : PropertyAttribute
	{
		public ResolutionAttribute(int recommendedResolution, params int[] resolutions)
		{
			this._RecommendedResolution = recommendedResolution;
			this._Resolutions = resolutions;
		}

		public int RecommendedResolution
		{
			get
			{
				return this._RecommendedResolution;
			}
		}

		public int[] Resolutions
		{
			get
			{
				return this._Resolutions;
			}
		}

		private readonly int _RecommendedResolution;

		private readonly int[] _Resolutions;
	}
}
