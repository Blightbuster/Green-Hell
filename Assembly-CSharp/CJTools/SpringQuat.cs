using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CJTools
{
	public struct SpringQuat
	{
		private static Vector4 q2v(Quaternion q)
		{
			return new Vector4(q.x, q.y, q.z, q.w);
		}

		public Quaternion rotation
		{
			get
			{
				return this.current.q;
			}
			set
			{
				this.current.q = value;
			}
		}

		public void Init(Quaternion rotation, float omega)
		{
			this.current.v = (this.target.v = Vector4.zero);
			this.target.q = rotation;
			this.current.q = rotation;
			this.velocity = Vector4.zero;
			this.omega = omega;
		}

		public void Update(float dt)
		{
			if (Vector4.Dot(this.current.v, this.target.v) < 0f)
			{
				this.target.v = -this.target.v;
			}
			Vector4 a = this.velocity - (this.current.v - this.target.v) * (this.omega * this.omega * dt);
			float num = 1f + this.omega * dt;
			this.velocity = a / (num * num);
			this.current.v = (this.current.v + this.velocity * dt).normalized;
		}

		public static implicit operator Quaternion(SpringQuat m)
		{
			return m.rotation;
		}

		private SpringQuat.QVUnion current;

		private SpringQuat.QVUnion target;

		public Vector4 velocity;

		public float omega;

		[StructLayout(LayoutKind.Explicit)]
		private struct QVUnion
		{
			[FieldOffset(0)]
			public Vector4 v;

			[FieldOffset(0)]
			public Quaternion q;
		}
	}
}
