using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class PunchBack : AIAction
	{
		public override void Start()
		{
			base.Start();
			if (this.m_AI.m_VisModule)
			{
				this.m_AI.m_VisModule.OnStartAttack();
			}
			this.m_Animation = "PunchBack" + this.m_Direction.ToString();
		}

		public void SetDirection(Direction direction)
		{
			this.m_Direction = direction;
			this.m_Animation = "PunchBack" + this.m_Direction.ToString();
		}

		protected override void SetupTransitionDuration()
		{
			this.m_AI.m_AnimationModule.m_TransitionDuration = 0.1f;
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		protected override void Stop()
		{
			base.Stop();
			this.m_AI.m_LastAttackTime = Time.time;
		}

		private Direction m_Direction;
	}
}
