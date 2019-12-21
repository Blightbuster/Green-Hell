using System;
using UnityEngine;

namespace CJTools
{
	public struct SpringVec4
	{
		public void Init(Vector4 start_val, float omega)
		{
			this.target = start_val;
			this.current = start_val;
			this.velocity = Vector4.zero;
			this.omega = omega;
		}

		public void Update(float dt)
		{
			Vector4 a = this.velocity - (this.current - this.target) * (this.omega * this.omega * dt);
			float num = 1f + this.omega * dt;
			this.velocity = a / (num * num);
			this.current += this.velocity * dt;
		}

		public static implicit operator Vector4(SpringVec4 s)
		{
			return s.current;
		}

		public Vector4 current;

		public Vector4 target;

		public Vector4 velocity;

		public float omega;
	}
}
