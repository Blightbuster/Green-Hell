using System;
using UnityEngine;

namespace AIs
{
	public class GoalRotateToEnemy : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			if (!enemy)
			{
				return false;
			}
			if (!this.m_AI.m_Params.m_CanLeaveSpawner && this.m_AI.m_Spawner != null && !this.m_AI.m_Spawner.IsInside(enemy.transform.position))
			{
				return false;
			}
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
			Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
			float num = Vector3.Angle(normalized2D, normalized2D2);
			return num > RotateTo.MAX_ANGLE;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_RotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.gameObject, this.m_AI.m_ID == AI.AIID.Jaguar);
			base.AddToPlan(this.m_RotateTo);
		}

		private RotateTo m_RotateTo;
	}
}
