using System;
using UnityEngine;

namespace CJTools
{
	public struct SpringVec2
	{
		public void Init(Vector2 start_val, float omega, bool is_angle)
		{
			this.target = start_val;
			this.current = start_val;
			this.velocity = Vector2.zero;
			this.omega = omega;
			this.is_angle = is_angle;
		}

		public void Update(float dt)
		{
			if (this.is_angle)
			{
				this.target.x = this.current.x + Mathf.DeltaAngle(this.current.x, this.target.x);
				this.target.y = this.current.y + Mathf.DeltaAngle(this.current.y, this.target.y);
			}
			Vector2 a = this.velocity - (this.current - this.target) * (this.omega * this.omega * dt);
			float num = 1f + this.omega * dt;
			this.velocity = a / (num * num);
			this.current += this.velocity * dt;
		}

		public static implicit operator Vector2(SpringVec2 s)
		{
			return s.current;
		}

		public static implicit operator Vector3(SpringVec2 s)
		{
			return new Vector3(s.current.x, s.current.y);
		}

		public void Reset()
		{
			this.Init(Vector2.zero, this.omega, this.is_angle);
		}

		public bool is_angle;

		public Vector2 current;

		public Vector2 target;

		public Vector2 velocity;

		public float omega;
	}
}
