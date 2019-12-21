using System;
using UnityEngine;

namespace IESLights
{
	public struct EXRData
	{
		public EXRData(Color[] pixels, int width, int height)
		{
			this.Pixels = pixels;
			this.Width = (uint)width;
			this.Height = (uint)height;
		}

		public Color[] Pixels;

		public uint Width;

		public uint Height;
	}
}
