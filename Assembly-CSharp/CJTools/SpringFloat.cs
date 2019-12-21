using System;

namespace CJTools
{
	public struct SpringFloat
	{
		public void Init(float start_val, float omega)
		{
			this.target = start_val;
			this.current = start_val;
			this.velocity = 0f;
			this.omega = omega;
		}

		public void Update(float dt)
		{
			float num = this.velocity - (this.current - this.target) * (this.omega * this.omega * dt);
			float num2 = 1f + this.omega * dt;
			this.velocity = num / (num2 * num2);
			this.current += this.velocity * dt;
		}

		public static implicit operator float(SpringFloat s)
		{
			return s.current;
		}

		public void Reset()
		{
			this.Init(0f, this.omega);
		}

		public float current;

		public float target;

		public float velocity;

		public float omega;
	}
}
