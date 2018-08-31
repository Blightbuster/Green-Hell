using System;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class GoalHumanMoveAwayFromEnemy : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_TempPath = new NavMeshPath();
			this.m_HumanStartMove = (base.CreateAction(typeof(HumanStartMove)) as HumanStartMove);
			this.m_HumanMove = (base.CreateAction(typeof(HumanMove)) as HumanMove);
			this.m_HumanStopMove = (base.CreateAction(typeof(HumanStopMove)) as HumanStopMove);
		}

		public override bool ShouldPerform()
		{
			return !(this.m_AI.m_EnemyModule.m_Enemy == null) && (this.m_Active || Vector3.Distance(this.m_HumanAI.transform.position, this.m_AI.m_EnemyModule.m_Enemy.transform.position) < this.m_MinDistToEnemy);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_MoveStyle = AIMoveStyle.Run;
			this.m_Range = (this.m_AI.transform.position - Player.Get().transform.position).magnitude;
			this.m_Direction = ((UnityEngine.Random.Range(0f, 1f) <= 0.5f) ? Direction.Right : Direction.Left);
			this.m_MaxDuration = UnityEngine.Random.Range(4f, 7f);
			this.m_Duration = 0f;
			if (!this.SetupPath())
			{
				this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanThrowStone);
				return;
			}
			base.StartAction(this.m_HumanStartMove);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.m_Duration += Time.deltaTime;
			if (this.m_Duration >= this.m_MaxDuration)
			{
				this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HunterBowAttack);
			}
		}

		private void UpdateRange()
		{
			if (this.m_Range + 3f < 10f)
			{
				this.m_Range += 3f;
			}
			else
			{
				this.m_Range = UnityEngine.Random.Range(15f, 18f);
			}
		}

		private bool SetupPath()
		{
			this.UpdateRange();
			Vector3 a = Vector3.zero;
			Vector3 normalized2D = (this.m_AI.transform.position - Player.Get().transform.position).GetNormalized2D();
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
				Vector3 vector = Player.Get().transform.position + normalized2D * this.m_Range + a * 2f;
				vector.y = MainLevel.GetTerrainY(vector);
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(vector, out navMeshHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(this.m_AI.m_PathModule.m_Agent.nextPosition, navMeshHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_AI.m_PathModule.m_Agent.SetPath(this.m_TempPath);
					return true;
				}
				this.UpdateRange();
			}
			return false;
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanStartMove))
			{
				base.StartAction(this.m_HumanMove);
			}
			else if (action.GetType() == typeof(HumanMove))
			{
				if (Vector3.Distance(this.m_HumanAI.transform.position, this.m_AI.m_EnemyModule.m_Enemy.transform.position) > this.m_MinDistToEnemy)
				{
					base.StartAction(this.m_HumanStopMove);
				}
				else if (this.SetupPath())
				{
					base.StartAction(this.m_HumanMove);
				}
				else
				{
					base.StartAction(this.m_HumanStopMove);
				}
			}
		}

		private Direction m_Direction;

		private const float ADD_RANGE = 3f;

		private float m_MinDistToEnemy = 10f;

		private float m_Range;

		private float m_Duration;

		private float m_MaxDuration;

		private NavMeshPath m_TempPath;

		private HumanStartMove m_HumanStartMove;

		private HumanMove m_HumanMove;

		private HumanStopMove m_HumanStopMove;
	}
}
