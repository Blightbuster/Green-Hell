using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalMoveToEnemy : GoalMove
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && (this.m_AI.m_Params.m_CanLeaveSpawner || !(this.m_AI.m_Spawner != null) || this.m_AI.m_Spawner.IsInside(enemy.transform.position)) && enemy.transform.position.Distance(this.m_AI.transform.position) > this.m_AI.m_Params.m_AttackRange && this.m_AI.m_PathModule.CalcPath(PathModule.PathType.AnimalMoveToEnemy);
		}

		protected override void Prepare()
		{
			if (this.m_AI.IsCat())
			{
				Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
				Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
				if (Vector3.Angle(normalized2D, normalized2D2) > RotateTo.MAX_ANGLE)
				{
					this.m_RotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.gameObject, true);
					base.AddToPlan(this.m_RotateTo);
				}
			}
			base.Prepare();
		}

		private void SetupPath()
		{
			if (!this.m_AI.m_PathModule.CalcPath(PathModule.PathType.AnimalMoveToEnemy))
			{
				base.Deactivate();
				return;
			}
			this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			if (this.m_AI.m_HostileStateModule && this.m_AI.m_HostileStateModule.m_State == HostileStateModule.State.Upset)
			{
				return AIMoveStyle.Sneak;
			}
			if (this.m_AI.m_Params.m_CanRun)
			{
				return AIMoveStyle.Run;
			}
			if (this.m_AI.m_Params.m_CanSneak)
			{
				return AIMoveStyle.Sneak;
			}
			return AIMoveStyle.Walk;
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_LastEnemyPos.Distance(this.m_AI.m_EnemyModule.m_Enemy.transform.position) >= 1f)
			{
				this.SetupPath();
			}
		}

		private Vector3 m_LastEnemyPos = Vector3.zero;

		private RotateTo m_RotateTo;
	}
}
