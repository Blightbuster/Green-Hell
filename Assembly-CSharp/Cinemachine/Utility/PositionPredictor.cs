using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	internal class PositionPredictor
	{
		public float Smoothing
		{
			get
			{
				return this.mSmoothing;
			}
			set
			{
				if (value != this.mSmoothing)
				{
					this.mSmoothing = value;
					int maxKernelRadius = Mathf.Max(10, Mathf.FloorToInt(value * 1.5f));
					this.m_Velocity = new GaussianWindow1D_Vector3(this.mSmoothing, maxKernelRadius);
					this.m_Accel = new GaussianWindow1D_Vector3(this.mSmoothing, maxKernelRadius);
				}
			}
		}

		public bool IsEmpty
		{
			get
			{
				return this.m_Velocity.IsEmpty();
			}
		}

		public void Reset()
		{
			this.m_Velocity.Reset();
			this.m_Accel.Reset();
		}

		public void AddPosition(Vector3 pos)
		{
			if (this.IsEmpty)
			{
				this.m_Velocity.AddValue(Vector3.zero);
			}
			else
			{
				Vector3 b = this.m_Velocity.Value();
				Vector3 vector = (pos - this.m_Position) / Time.deltaTime;
				this.m_Velocity.AddValue(vector);
				this.m_Accel.AddValue(vector - b);
			}
			this.m_Position = pos;
		}

		public Vector3 PredictPosition(float lookaheadTime)
		{
			int num = Mathf.Min(Mathf.RoundToInt(lookaheadTime / Time.deltaTime), 6);
			float d = lookaheadTime / (float)num;
			Vector3 vector = this.m_Position;
			Vector3 vector2 = this.m_Velocity.IsEmpty() ? Vector3.zero : this.m_Velocity.Value();
			Vector3 vector3 = this.m_Accel.IsEmpty() ? Vector3.zero : this.m_Accel.Value();
			for (int i = 0; i < num; i++)
			{
				vector += vector2 * d;
				Vector3 vector4 = vector2 + vector3 * d;
				vector3 = Quaternion.FromToRotation(vector2, vector4) * vector3;
				vector2 = vector4;
			}
			return vector;
		}

		private Vector3 m_Position;

		private const float kSmoothingDefault = 10f;

		private float mSmoothing = 10f;

		private GaussianWindow1D_Vector3 m_Velocity = new GaussianWindow1D_Vector3(10f, 10);

		private GaussianWindow1D_Vector3 m_Accel = new GaussianWindow1D_Vector3(10f, 10);
	}
}
