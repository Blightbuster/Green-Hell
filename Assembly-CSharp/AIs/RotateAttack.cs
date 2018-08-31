using System;
using UnityEngine;

namespace AIs
{
	public class RotateAttack : AIAction
	{
		public void SetupParams(float angle_signed)
		{
			base.SetupParams();
			this.m_Animation = ((angle_signed < 0f) ? "AttackLeft_" : "AttackRight_");
			float num = Mathf.Abs(angle_signed);
			if (num < 135f)
			{
				this.m_Animation += "90";
			}
			else
			{
				this.m_Animation += "180";
			}
		}

		public override void Start()
		{
			base.Start();
			if (this.m_AI.m_VisModule)
			{
				this.m_AI.m_VisModule.OnStartAttack();
			}
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
	}
}
