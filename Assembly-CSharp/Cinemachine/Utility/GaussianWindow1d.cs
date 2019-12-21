using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	internal abstract class GaussianWindow1d<T>
	{
		public float Sigma { get; private set; }

		public int KernelSize
		{
			get
			{
				return this.mKernel.Length;
			}
		}

		private void GenerateKernel(float sigma, int maxKernelRadius)
		{
			int num = Math.Min(maxKernelRadius, Mathf.FloorToInt(Mathf.Abs(sigma) * 2.5f));
			this.mKernel = new float[2 * num + 1];
			this.mKernelSum = 0f;
			if (num == 0)
			{
				this.mKernelSum = (this.mKernel[0] = 1f);
			}
			else
			{
				for (int i = -num; i <= num; i++)
				{
					this.mKernel[i + num] = (float)(Math.Exp((double)((float)(-(float)(i * i)) / (2f * sigma * sigma))) / Math.Sqrt(6.2831853071795862 * (double)sigma));
					this.mKernelSum += this.mKernel[i + num];
				}
			}
			this.Sigma = sigma;
		}

		protected abstract T Compute(int windowPos);

		public GaussianWindow1d(float sigma, int maxKernelRadius = 10)
		{
			this.GenerateKernel(sigma, maxKernelRadius);
			this.mCurrentPos = 0;
		}

		public void Reset()
		{
			this.mData = null;
		}

		public bool IsEmpty()
		{
			return this.mData == null;
		}

		public void AddValue(T v)
		{
			if (this.mData == null)
			{
				this.mData = new T[this.KernelSize];
				for (int i = 0; i < this.KernelSize; i++)
				{
					this.mData[i] = v;
				}
				this.mCurrentPos = Mathf.Min(1, this.KernelSize - 1);
			}
			this.mData[this.mCurrentPos] = v;
			int num = this.mCurrentPos + 1;
			this.mCurrentPos = num;
			if (num == this.KernelSize)
			{
				this.mCurrentPos = 0;
			}
		}

		public T Filter(T v)
		{
			if (this.KernelSize < 3)
			{
				return v;
			}
			this.AddValue(v);
			return this.Value();
		}

		public T Value()
		{
			return this.Compute(this.mCurrentPos);
		}

		protected T[] mData;

		protected float[] mKernel;

		protected float mKernelSum;

		protected int mCurrentPos;
	}
}
