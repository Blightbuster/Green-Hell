using System;
using UnityEngine;

namespace AIs
{
	public class GoalCantReachEnemy : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
			this.m_Idle.m_ForceVersion = 3;
			this.m_Idle.SetupParams(float.MaxValue);
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.m_PathModule.m_CurrPathType == PathModule.PathType.AnimalMoveToEnemy && !this.m_AI.m_PathModule.m_PathValid;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_Idle);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_AI.m_ID == AI.AIID.Jaguar && this.m_AI.m_EnemyModule.m_Enemy)
			{
				Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
				Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
				float num = Vector3.Angle(normalized2D, normalized2D2);
				if (num > 25f)
				{
					this.m_RotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.gameObject, true);
					base.StartAction(this.m_RotateTo);
					base.AddToPlan(this.m_Idle);
				}
			}
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		private Vector3 m_LastEnemyPos = Vector3.zero;

		private Idle m_Idle;

		private RotateTo m_RotateTo;
	}
}
