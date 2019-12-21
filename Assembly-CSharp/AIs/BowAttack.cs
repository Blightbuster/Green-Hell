using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class BowAttack : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Finish = false;
			this.m_Animation = "BowAttack";
		}

		protected override bool ShouldFinish()
		{
			return this.m_Finish;
		}

		protected override void Stop()
		{
			base.Stop();
			this.m_AI.m_LastAttackTime = Time.time;
		}

		public override void Update()
		{
			base.Update();
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized2D), Time.deltaTime * 2f);
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.BowShotEnd)
			{
				this.m_Finish = true;
			}
		}

		private bool m_Finish;
	}
}
