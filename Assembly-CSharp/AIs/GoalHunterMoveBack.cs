using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalHunterMoveBack : GoalHunter
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanMoveBack = (base.CreateAction(typeof(HumanMoveBack)) as HumanMoveBack);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return this.m_Active;
			}
			return this.m_HumanAI.m_WeaponType == HumanAI.WeaponType.Secondary;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
			this.m_AI.m_MoveStyle = AIMoveStyle.Walk;
			this.Setup();
			base.StartAction(this.m_HumanMoveBack);
		}

		private void Setup()
		{
			if (!this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveAwayFromEnemy))
			{
				base.Deactivate();
				return;
			}
			this.m_LastEnemyPos = this.m_AI.m_EnemyModule.m_Enemy.transform.position;
		}

		private Vector3 m_LastEnemyPos = Vector3.zero;

		private const float MAX_ANGLE = 5f;

		private HumanMoveBack m_HumanMoveBack;
	}
}
