using System;
using UnityEngine;

namespace AIs
{
	public class HumanAnimationModule : AnimationModule
	{
		protected override string GetStatesDataScript()
		{
			return "SavageAnimatorData";
		}

		public override void OnUpdate()
		{
			if (this.m_WantedAnim.Length <= 0)
			{
				return;
			}
			if (this.m_CurrentAnim != this.m_WantedAnim)
			{
				this.m_CurrentAnim = this.m_WantedAnim;
				float fixedTimeOffset = 0f;
				if (this.m_StartFromRandomFrame)
				{
					fixedTimeOffset = UnityEngine.Random.Range(0f, this.m_StatesData[this.m_CurrentAnim].m_Duration);
					this.m_StartFromRandomFrame = false;
				}
				if (this.m_PrevAnim == this.m_CurrentAnim && this.m_StatesData[this.m_CurrentAnim].m_Loop)
				{
					this.m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;
					return;
				}
				this.m_AI.m_Animator.CrossFadeInFixedTime(this.m_CurrentAnim, this.m_TransitionDuration, -1, fixedTimeOffset);
				this.m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;
				this.m_PrevAnim = this.m_CurrentAnim;
			}
			AnimatorStateInfo currentAnimatorStateInfo = this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0);
			if (this.m_ForcedSpeed >= 0f)
			{
				this.m_AI.m_Animator.speed = this.m_ForcedSpeed;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_WalkHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_WalkSpeedMul;
			}
			else if (currentAnimatorStateInfo.shortNameHash == this.m_RunHash)
			{
				this.m_AI.m_Animator.speed = this.m_AI.m_Params.m_RunSpeedMul;
			}
			else
			{
				this.m_AI.m_Animator.speed = 1f;
			}
		}
	}
}
