using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanHitReaction : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_HitReaction = (base.CreateAction(typeof(HitReaction)) as HitReaction);
		}

		protected override void Prepare()
		{
			base.Prepare();
			Vector3 normalized2D = (Player.Get().transform.position - this.m_HumanAI.transform.position).GetNormalized2D();
			float num = Vector3.Angle(this.m_HumanAI.transform.forward.GetNormalized2D(), normalized2D);
			float num2 = 40f;
			float num3 = 0.3f;
			HitReaction.Type type = HitReaction.Type.Middle;
			if (num <= num2 && num3 <= UnityEngine.Random.Range(0f, 1f))
			{
				type = HitReaction.Type.StepBack;
			}
			else if (FistFightController.Get().IsActive())
			{
				type = ((!FistFightController.Get().IsLeftPunch()) ? HitReaction.Type.Right : HitReaction.Type.Left);
			}
			else if (WeaponMeleeController.Get().IsActive())
			{
				if (WeaponMeleeController.Get().IsRightAttack())
				{
					type = HitReaction.Type.Right;
				}
				else if (WeaponMeleeController.Get().IsLeftAttack())
				{
					type = HitReaction.Type.Left;
				}
				else
				{
					type = HitReaction.Type.Middle;
				}
			}
			else if (num <= num2 && WeaponSpearController.Get().IsActive())
			{
				type = HitReaction.Type.StepBack;
			}
			if (type == this.m_AI.m_HumanFightModule.m_LastHitReactionType && this.m_AI.m_GoalsModule.m_PrevGoal != null && this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.HumanHitReaction)
			{
				switch (type)
				{
				case HitReaction.Type.Right:
				case HitReaction.Type.Left:
					type = HitReaction.Type.Middle;
					break;
				case HitReaction.Type.Middle:
					type = HitReaction.Type.StepBack;
					break;
				case HitReaction.Type.StepBack:
					type = HitReaction.Type.Middle;
					break;
				}
			}
			this.m_HitReaction.SetType(type);
			this.m_AI.m_HumanFightModule.m_LastHitReactionType = type;
			this.m_HitReaction.Initialize(this.m_AI, this);
			base.AddToPlan(this.m_HitReaction);
		}

		private HitReaction m_HitReaction;
	}
}
