using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalHunterStrafe : GoalHunter
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HumanStrafe = (base.CreateAction(typeof(HumanStrafe)) as HumanStrafe);
		}

		public override bool ShouldPerform()
		{
			if (!this.m_HunterAI.IsProperPosToBowAttack())
			{
				return false;
			}
			if (this.m_Active)
			{
				return this.m_Active;
			}
			return this.m_HunterAI.m_GoalsModule.m_PrevGoal != null && this.m_HunterAI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.HunterBowAttack;
		}

		protected override void Prepare()
		{
			base.Prepare();
			Direction direction = (UnityEngine.Random.Range(0f, 1f) <= 0.5f) ? Direction.Right : Direction.Left;
			if (!this.m_AI.m_PathModule.CalcPath((direction != Direction.Left) ? PathModule.PathType.StrafeRight : PathModule.PathType.StrafeLeft))
			{
				direction = ((direction != Direction.Right) ? Direction.Right : Direction.Left);
				if (!this.m_AI.m_PathModule.CalcPath((direction != Direction.Left) ? PathModule.PathType.StrafeRight : PathModule.PathType.StrafeLeft))
				{
					base.Deactivate();
					return;
				}
			}
			this.m_HumanStrafe.SetDirection(direction);
			base.StartAction(this.m_HumanStrafe);
		}

		private HumanStrafe m_HumanStrafe;
	}
}
