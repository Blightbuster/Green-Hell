using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalHumanMoveToEnemy : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanMoveTo = (base.CreateAction(typeof(HumanMoveTo)) as HumanMoveTo);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && enemy.transform.position.Distance(this.m_AI.transform.position) > this.m_AI.m_Params.m_AttackRange;
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (!this.m_AI.m_EnemyModule.m_Enemy)
			{
				base.Deactivate();
				return;
			}
			this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
			this.m_AI.m_MoveStyle = (((this.m_AI.m_Params.m_CanRun && this.m_HumanAI.GetState() == HumanAI.State.Attack) || this.m_HumanAI.GetState() == HumanAI.State.StartWave) ? AIMoveStyle.Run : AIMoveStyle.Walk);
			this.Setup();
			base.StartAction(this.m_HumanMoveTo);
		}

		private void Setup()
		{
			if (!this.m_AI.m_EnemyModule.m_Enemy || !this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveToEnemy))
			{
				base.Deactivate();
				return;
			}
			this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.m_AI.m_EnemyModule.m_Enemy)
			{
				base.Deactivate();
				return;
			}
			if (this.m_LastEnemyPos.Distance(this.m_AI.m_EnemyModule.m_Enemy.transform.position) >= 1f)
			{
				this.Setup();
			}
		}

		private Vector3 m_LastEnemyPos = Vector3.zero;

		private const float MAX_ANGLE = 5f;

		private HumanMoveTo m_HumanMoveTo;
	}
}
