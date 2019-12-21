using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	internal class GaussianWindow1D_Quaternion : GaussianWindow1d<Quaternion>
	{
		public GaussianWindow1D_Quaternion(float sigma, int maxKernelRadius = 10) : base(sigma, maxKernelRadius)
		{
		}

		protected override Quaternion Compute(int windowPos)
		{
			Quaternion rhs = new Quaternion(0f, 0f, 0f, 0f);
			Quaternion quaternion = this.mData[this.mCurrentPos];
			Quaternion lhs = Quaternion.Inverse(quaternion);
			for (int i = 0; i < base.KernelSize; i++)
			{
				float num = this.mKernel[i];
				Quaternion quaternion2 = lhs * this.mData[windowPos];
				if (Quaternion.Dot(Quaternion.identity, quaternion2) < 0f)
				{
					num = -num;
				}
				rhs.x += quaternion2.x * num;
				rhs.y += quaternion2.y * num;
				rhs.z += quaternion2.z * num;
				rhs.w += quaternion2.w * num;
				if (++windowPos == base.KernelSize)
				{
					windowPos = 0;
				}
			}
			return quaternion * rhs;
		}
	}
}
