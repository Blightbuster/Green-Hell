using System;
using UnityEngine;

namespace AIs
{
	public class HumanRotateTo : AIAction
	{
		public void SetupParams(Vector3 target, float max_angle)
		{
			this.m_Target = target;
			this.m_MaxAngle = max_angle;
			this.SetupAngleAnim();
		}

		private void SetupAngleAnim()
		{
			this.m_Animation = "Rotate";
			Vector3 normalized2D = (this.m_Target - this.m_AI.transform.position).GetNormalized2D();
			Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
			float num = normalized2D2.AngleSigned(normalized2D, Vector3.up);
			this.m_Animation += ((num < 0f) ? "Left_" : "Right_");
			float num2 = Mathf.Abs(num);
			if (num2 <= 67f)
			{
				this.m_Animation += "45";
			}
			else if (num2 <= 112f)
			{
				this.m_Animation += "90";
			}
			else if (num2 <= 157f)
			{
				this.m_Animation += "135";
			}
			else
			{
				this.m_Animation += "180";
			}
		}

		public override void Update()
		{
			base.Update();
			Vector3 normalized2D = (this.m_Target - this.m_AI.transform.position).GetNormalized2D();
			AnimatorStateInfo currentAnimatorStateInfo = this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.IsName(this.GetAnimName()) && currentAnimatorStateInfo.normalizedTime > 0.5f)
			{
				this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized2D), Time.deltaTime);
			}
		}

		protected override bool ShouldFinish()
		{
			Vector3 normalized2D = (this.m_Target - this.m_AI.transform.position).GetNormalized2D();
			float num = Vector3.Angle(normalized2D, this.m_AI.transform.forward.GetNormalized2D());
			return num <= this.m_MaxAngle;
		}

		private Vector3 m_Target = Vector3.zero;

		private float m_MaxAngle;
	}
}
