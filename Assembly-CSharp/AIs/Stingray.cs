using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class Stingray : Fish
	{
		protected override void Start()
		{
			base.Start();
		}

		protected override void SetupMouth()
		{
		}

		public override bool IsStringray()
		{
			return true;
		}

		protected override void OnExitState()
		{
			base.OnExitState();
			if (this.m_FishState == Fish.State.Idle)
			{
				this.m_LastIdleTime = Time.time;
			}
		}

		protected override void OnEnterState()
		{
			base.OnEnterState();
			Fish.State fishState = this.m_FishState;
			switch (fishState)
			{
			case Fish.State.RunAwayFromNoise:
				break;
			default:
				if (fishState != Fish.State.Swim)
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
					return;
				}
				break;
			case Fish.State.Idle:
				this.m_Animator.SetTrigger(this.m_IdleHash);
				return;
			case Fish.State.HitReaction:
				this.m_Animator.SetTrigger(this.m_HitReactionHash);
				return;
			}
			this.m_Animator.SetTrigger(this.m_SwimHash);
		}

		protected override void UpdateState()
		{
			Fish.State fishState = this.m_FishState;
			if (fishState != Fish.State.Swim)
			{
				if (fishState != Fish.State.Idle)
				{
					base.UpdateState();
				}
				else
				{
					this.UodateIdleState();
				}
			}
			else
			{
				this.UpdateSwimState();
			}
		}

		private void UodateIdleState()
		{
			if (Time.time - this.m_StartStateTime > 10f)
			{
				base.SetState(Fish.State.Swim);
			}
		}

		protected override void UpdateSwimState()
		{
			base.UpdateSwimState();
			this.UpdateBlend();
			if (Time.time - this.m_LastIdleTime > 60f && base.transform.position.y - MainLevel.GetTerrainY(base.transform.position) <= 0f)
			{
				base.SetState(Fish.State.Idle);
			}
		}

		private void UpdateBlend()
		{
			Vector3 normalized2D = base.transform.forward.GetNormalized2D();
			Vector3 normalized2D2 = (this.m_Target - base.transform.position).GetNormalized2D();
			float b = normalized2D2.AngleSigned(normalized2D, Vector3.up);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, 45f, -45f);
			this.m_Blend += (proportionalClamp - this.m_Blend) * Time.deltaTime * 5f;
			this.m_Blend = Mathf.Clamp01(this.m_Blend);
			this.m_Animator.SetFloat("Blend", this.m_Blend);
		}

		public override void OnImpaleOnSpear()
		{
			base.OnImpaleOnSpear();
			base.SetState(Fish.State.HitReaction);
		}

		protected override void UpdateAnimatorSpeed(float speed)
		{
			this.m_Animator.speed = 1f;
		}

		protected override float GetMinSpeed()
		{
			return 0.2f;
		}

		protected override float GetMaxSpeed()
		{
			return 0.2f;
		}

		private int m_SwimHash = Animator.StringToHash("Swim");

		private int m_IdleHash = Animator.StringToHash("Idle");

		private int m_HitReactionHash = Animator.StringToHash("HitReaction");

		private const float DEFAULT_BLEND_MUL = 5f;

		private float m_Blend = 0.5f;

		private const float MIN_TANK_SIZE = 10f;

		private float m_LastIdleTime;
	}
}
