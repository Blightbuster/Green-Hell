using System;
using UnityEngine;

namespace CJTools
{
	public struct SpringVec3
	{
		public void Init(Vector3 start_val, float omega)
		{
			this.target = start_val;
			this.current = start_val;
			this.velocity = Vector3.zero;
			this.omega = omega;
		}

		public void Update(float dt)
		{
			Vector3 a = this.velocity - (this.current - this.target) * (this.omega * this.omega * dt);
			float num = 1f + this.omega * dt;
			this.velocity = a / (num * num);
			this.current += this.velocity * dt;
		}

		public static implicit operator Vector3(SpringVec3 s)
		{
			return s.current;
		}

		public void Reset()
		{
			this.Init(Vector3.zero, this.omega);
		}

		public void Force(Vector3 v)
		{
			this.target = v;
			this.current = v;
			this.velocity = Vector3.zero;
		}

		public Vector3 current;

		public Vector3 target;

		public Vector3 velocity;

		public float omega;
	}
}
