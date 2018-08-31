using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	internal class GaussianWindow1D_Vector3 : GaussianWindow1d<Vector3>
	{
		public GaussianWindow1D_Vector3(float sigma, int maxKernelRadius = 10) : base(sigma, maxKernelRadius)
		{
		}

		protected override Vector3 Compute(int windowPos)
		{
			Vector3 a = Vector3.zero;
			for (int i = 0; i < base.KernelSize; i++)
			{
				a += this.mData[windowPos] * this.mKernel[i];
				if (++windowPos == base.KernelSize)
				{
					windowPos = 0;
				}
			}
			return a / this.mKernelSum;
		}
	}
}
