using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class RotateTo : AIAction
	{
		public override string GetAnimName()
		{
			if (this.m_SpecificAngle)
			{
				return this.m_SpecificAngleAnim;
			}
			return this.m_Animation;
		}

		private void SetupAngleAnim()
		{
			Vector3 wantedTargetPos = this.GetWantedTargetPos();
			if (wantedTargetPos == Vector3.zero)
			{
				return;
			}
			this.m_SpecificAngleAnim = "turn_";
			Vector3 normalized2D = (wantedTargetPos - this.m_AI.transform.position).GetNormalized2D();
			float num = this.m_AI.transform.forward.GetNormalized2D().AngleSigned(normalized2D, Vector3.up);
			this.m_SpecificAngleAnim += ((num >= 0f) ? "right_" : "left_");
			float num2 = Mathf.Abs(num);
			if (num2 <= 45f)
			{
				this.m_SpecificAngleAnim += "45";
				return;
			}
			if (num2 <= 90f)
			{
				this.m_SpecificAngleAnim += "90";
				return;
			}
			if (num2 <= 135f)
			{
				this.m_SpecificAngleAnim += "135";
				return;
			}
			this.m_SpecificAngleAnim += "180";
		}

		public void SetupParams(GameObject target, bool specific_angle = false)
		{
			this.m_TargetObj = target;
			this.m_SpecificAngle = specific_angle;
			if (this.m_SpecificAngle)
			{
				this.SetupAngleAnim();
			}
			this.SetupDirection();
			this.m_Animation = "Rotate" + this.m_Direction.ToString();
		}

		private void SetupDirection()
		{
			Vector3 wantedTargetPos = this.GetWantedTargetPos();
			if (wantedTargetPos == Vector3.zero)
			{
				return;
			}
			Vector3 normalized2D = (wantedTargetPos - this.m_AI.transform.position).GetNormalized2D();
			Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
			Vector3 vector = Vector3.Cross(normalized2D, normalized2D2);
			this.m_Direction = ((vector.y < 0f) ? Direction.Right : Direction.Left);
		}

		protected override bool ShouldFail()
		{
			return this.GetWantedTargetPos() == Vector3.zero;
		}

		protected override bool ShouldFinish()
		{
			if (this.m_SpecificAngle)
			{
				return base.IsAnimFinishing() && Time.time - this.m_StartTime > 0.4f;
			}
			Vector3 from = this.GetWantedTargetPos() - this.m_AI.transform.position;
			from.y = 0f;
			from.Normalize();
			Vector3 forward = this.m_AI.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			return Vector3.Angle(from, forward) <= RotateTo.MAX_ANGLE;
		}

		private Vector3 GetWantedTargetPos()
		{
			if (!(this.m_TargetObj != null))
			{
				return this.m_TargetPos;
			}
			return this.m_TargetObj.transform.position;
		}

		private Vector3 m_TargetPos = Vector3.zero;

		private GameObject m_TargetObj;

		private Direction m_Direction;

		private bool m_SpecificAngle;

		private string m_SpecificAngleAnim = string.Empty;

		public static float MAX_ANGLE = 10f;
	}
}
