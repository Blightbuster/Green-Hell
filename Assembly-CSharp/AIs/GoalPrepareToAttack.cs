using System;
using UnityEngine;

namespace AIs
{
	public class GoalPrepareToAttack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_PrepareToAttack = (base.CreateAction(typeof(PrepareToAttack)) as PrepareToAttack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_PrepareToAttack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateRotation();
		}

		private void UpdateRotation()
		{
			if (this.m_AI.m_ID != AI.AIID.BrasilianWanderingSpider)
			{
				return;
			}
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			Vector3 up = this.m_AI.m_EnemyModule.m_Enemy.transform.up;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized, up), Time.deltaTime / 0.1f);
		}

		private PrepareToAttack m_PrepareToAttack;
	}
}
