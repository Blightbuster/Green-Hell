using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HumanStartMove : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.SetupAnim();
		}

		private void SetupAnim()
		{
			string str = string.Empty;
			switch (this.m_AI.m_MoveStyle)
			{
			case AIMoveStyle.Walk:
				str = "Walk";
				break;
			case AIMoveStyle.Run:
				str = "Run";
				break;
			case AIMoveStyle.Sneak:
				str = "Sneak";
				break;
			case AIMoveStyle.Swim:
				str = "Swim";
				break;
			case AIMoveStyle.Trot:
				str = "Trot";
				break;
			case AIMoveStyle.Jump:
				str = "Jump";
				break;
			}
			Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
			Vector3 normalized2D2 = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
			float num = normalized2D.AngleSigned(normalized2D2, Vector3.up);
			float num2 = Mathf.Abs(num);
			if (num2 <= 45f)
			{
				this.m_Animation = "Start" + str + "Forward";
				return;
			}
			if (num2 <= 135f)
			{
				this.m_Animation = ((num >= 0f) ? ("Start" + str + "Right_90") : ("Start" + str + "Left_90"));
				return;
			}
			this.m_Animation = ((num >= 0f) ? ("Start" + str + "Right_180") : ("Start" + str + "Left_180"));
		}

		public override void Update()
		{
			base.Update();
			this.UpdateWantedDir();
		}

		private void UpdateWantedDir()
		{
			AnimatorStateInfo currentAnimatorStateInfo = this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.IsName(this.m_Animation) && currentAnimatorStateInfo.normalizedTime > 0.3f)
			{
				Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
				this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
			}
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
