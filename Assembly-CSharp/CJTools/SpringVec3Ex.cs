using System;
using UnityEngine;

namespace CJTools
{
	public struct SpringVec3Ex
	{
		public void Init(Vector3 start_val, Vector3 omega)
		{
			this.target = start_val;
			this.current = start_val;
			this.velocity = Vector3.zero;
			this.omega = omega;
		}

		public void Update(float dt)
		{
			Vector3 vector = this.velocity - Vector3.Scale(this.current - this.target, Vector3.Scale(this.omega, this.omega * dt));
			Vector3 vector2 = Vector3.one + this.omega * dt;
			this.velocity.x = vector.x / (vector2.x * vector2.x);
			this.velocity.y = vector.y / (vector2.y * vector2.y);
			this.velocity.z = vector.z / (vector2.z * vector2.z);
			this.current += this.velocity * dt;
		}

		public void Force(float x, float y, float z)
		{
			this.target.x = x;
			this.current.x = x;
			this.target.y = y;
			this.current.y = y;
			this.target.z = z;
			this.current.z = z;
			this.velocity = Vector3.zero;
		}

		public void Force(Vector3 v)
		{
			this.target = v;
			this.current = v;
			this.velocity = Vector3.zero;
		}

		public void Reset()
		{
			this.Init(Vector3.zero, this.omega);
		}

		public static implicit operator Vector3(SpringVec3Ex s)
		{
			return s.current;
		}

		public Vector3 current;

		public Vector3 target;

		public Vector3 velocity;

		public Vector3 omega;
	}
}
