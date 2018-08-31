using System;
using Enums;

namespace AIs
{
	public class Strafe : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "Strafe" + this.m_Direction.ToString();
		}

		public void SetupParams(float length, Direction direction)
		{
			this.m_Length = length;
			this.m_Direction = direction;
			this.m_Animation = "Strafe" + this.m_Direction.ToString();
		}

		protected override bool ShouldFinish()
		{
			return base.GetDuration() >= this.m_Length;
		}

		private float m_Length;

		public Direction m_Direction;
	}
}
