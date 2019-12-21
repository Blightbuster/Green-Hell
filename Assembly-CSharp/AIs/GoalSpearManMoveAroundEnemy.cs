using System;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class GoalSpearManMoveAroundEnemy : GoalMoveAroundEnemy
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_TempPath = new NavMeshPath();
			this.m_HumanMoveTo = (base.CreateAction(typeof(HumanMoveTo)) as HumanMoveTo);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return base.GetDuration() < this.m_Lenght;
			}
			return this.m_AI.m_GoalsModule.m_PrevGoal != null && this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.Attack;
		}

		protected override bool SetupPath()
		{
			Vector3 a = Vector3.zero;
			Vector3 normalized2D = (this.m_AI.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position).GetNormalized2D();
			if (this.m_Direction == Direction.Right)
			{
				a = Vector3.Cross(Vector3.up, normalized2D);
			}
			else
			{
				a = Vector3.Cross(normalized2D, Vector3.up);
			}
			for (int i = 0; i < 10; i++)
			{
				Vector3 vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position + normalized2D * this.m_AI.m_Params.m_AttackRange + a * 2f;
				vector.y = MainLevel.GetTerrainY(vector);
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(vector, out navMeshHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(this.m_AI.m_PathModule.m_Agent.nextPosition, navMeshHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_AI.m_PathModule.m_Agent.SetPath(this.m_TempPath);
					return true;
				}
			}
			this.m_AI.m_PerformEmergency = true;
			return false;
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (this.m_Active && this.SetupPath())
			{
				base.StartAction(this.m_HumanMoveTo);
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
		}

		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.LookRotation(normalized, this.m_AI.transform.up);
		}

		private HumanMoveTo m_HumanMoveTo;

		private NavMeshPath m_TempPath;
	}
}
