using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class GoalMoveAroundEnemy : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_TempPath = new NavMeshPath();
			this.m_HumanMoveTo = (base.CreateAction(typeof(HumanMoveTo)) as HumanMoveTo);
			this.m_Taunt = (base.CreateAction(typeof(Taunt)) as Taunt);
			this.m_HumanRotateTo = (base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active || (this.m_AI.m_GoalsModule.m_PrevGoal != null && ((this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.HumanHitReaction && this.m_AI.m_HumanFightModule.m_LastHitReactionType == HitReaction.Type.StepBack) || (this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.HumanJumpBack && this.m_AI.m_GoalsModule.m_ActiveGoal == null)));
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Lenght = UnityEngine.Random.Range(3f, 8f);
			this.m_Range = (this.m_AI.transform.position - this.m_AI.m_EnemyModule.m_Enemy.transform.position).magnitude + this.ADD_RANGE;
			this.m_Direction = ((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? Direction.Left : Direction.Right);
			this.m_MoveBack = (UnityEngine.Random.Range(0f, 1f) > 0.5f);
			this.m_MovingBack = false;
			if (!this.SetupPath())
			{
				return;
			}
			this.m_AI.m_MoveStyle = AIMoveStyle.Run;
			this.SetupAction();
		}

		private void SetupAction()
		{
			base.StartAction(this.m_HumanMoveTo);
			this.m_HumanMoveTo.m_PerformStop = false;
		}

		protected virtual bool SetupPath()
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
				Vector3 vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position + normalized2D * this.m_Range + a * 5f;
				vector.y = MainLevel.GetTerrainY(vector);
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(vector, out navMeshHit, 1f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(this.m_AI.m_PathModule.m_Agent.nextPosition, navMeshHit.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
				{
					this.m_AI.m_PathModule.m_Agent.SetPath(this.m_TempPath);
					return true;
				}
				this.m_Range += this.ADD_RANGE;
			}
			this.m_AI.m_PerformEmergency = true;
			return false;
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanMoveTo))
			{
				if (this.m_MoveBack)
				{
					if (this.m_Range > this.m_AI.m_Params.m_JumpAttackRange)
					{
						if (!this.m_MovingBack)
						{
							this.m_Range += this.ADD_RANGE;
							if (this.m_Range >= this.MAX_RANGE)
							{
								this.m_MovingBack = true;
							}
						}
						else
						{
							this.m_Range -= this.ADD_RANGE;
						}
						if (this.SetupPath())
						{
							this.SetupAction();
							return;
						}
					}
				}
				else if (base.GetDuration() < this.m_Lenght)
				{
					this.m_Range += this.ADD_RANGE;
					if (this.SetupPath())
					{
						this.SetupAction();
						return;
					}
				}
				else if (Vector3.Angle((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 10f)
				{
					this.m_HumanRotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.transform.position, 10f);
					base.StartAction(this.m_HumanRotateTo);
					return;
				}
			}
			else if (action.GetType() == typeof(HumanRotateTo) && UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				base.StartAction(this.m_Taunt);
			}
		}

		protected float m_Lenght;

		private List<NavMeshPath> m_Paths = new List<NavMeshPath>();

		protected Direction m_Direction;

		private float MAX_RANGE = 5f;

		private float ADD_RANGE = 1f;

		private float m_Range;

		private const float MAX_ANGLE = 10f;

		private bool m_MoveBack;

		private bool m_MovingBack;

		private NavMeshPath m_TempPath;

		private HumanMoveTo m_HumanMoveTo;

		private Taunt m_Taunt;

		private HumanRotateTo m_HumanRotateTo;
	}
}
