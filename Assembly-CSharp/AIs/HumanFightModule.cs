using System;
using UnityEngine;

namespace AIs
{
	public class HumanFightModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_HumanAI = (HumanAI)this.m_AI;
			DebugUtils.Assert(this.m_HumanAI, true);
		}

		private bool CanJumpBack()
		{
			if (this.m_AI.m_GoalsModule.m_JumpBackGoal == null)
			{
				return false;
			}
			AIGoal activeGoal = this.m_AI.m_GoalsModule.m_ActiveGoal;
			if (activeGoal == null)
			{
				return false;
			}
			AIGoalType type = activeGoal.m_Type;
			return type != AIGoalType.HumanJumpBack && type != AIGoalType.HumanJumpAttack && ((type != AIGoalType.HumanHitReaction && type != AIGoalType.HumanPunchBack) || this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f) && base.transform.position.Distance(Player.Get().transform.position) <= this.m_AI.m_Params.m_JumpBackRange && (!(this.m_LastFailedJumpBackPos != Vector3.zero) || this.m_LastFailedJumpBackPos.Distance(base.transform.position) >= 5f) && Vector3.Dot(base.transform.forward, (Player.Get().transform.position - base.transform.position).normalized) >= 0.25f && UnityEngine.Random.Range(0f, 1f) <= this.m_AI.m_GoalsModule.m_JumpBackGoal.m_Probability;
		}

		public void OnPlayerStartAttack()
		{
			this.TryJumpBack();
		}

		private bool TryJumpBack()
		{
			if (!this.CanJumpBack())
			{
				return false;
			}
			this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanJumpBack);
			return true;
		}

		private bool CanPunchBack()
		{
			if (this.m_HumanAI.m_CurrentWeapon && this.m_HumanAI.m_CurrentWeapon.m_Info.IsBow())
			{
				return false;
			}
			float num = UnityEngine.Random.Range(0f, 1f);
			return this.m_AI.m_GoalsModule.m_PunchBackGoal.m_Probability > num && base.transform.position.Distance(Player.Get().transform.position) <= this.m_AI.m_Params.m_AttackRange;
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			if (this.m_AI.m_GoalsModule.m_ActiveGoal != null && this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.HumanPunchBack)
			{
				return;
			}
			if (this.CanPunchBack())
			{
				this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanPunchBack);
				return;
			}
			this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanHitReaction);
		}

		[NonSerialized]
		public HitReaction.Type m_LastHitReactionType = HitReaction.Type.None;

		public Vector3 m_LastFailedJumpBackPos = Vector3.zero;

		private HumanAI m_HumanAI;
	}
}
