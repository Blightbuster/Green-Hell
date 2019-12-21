using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HumanStopMove : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.SetupAnim();
		}

		private void SetupAnim()
		{
			if (this.m_AI.m_MoveStyle == AIMoveStyle.Walk)
			{
				this.m_Animation = "StopWalk";
				return;
			}
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
			Vector3 normalized2D2 = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
			float num = normalized2D.AngleSigned(normalized2D2, Vector3.up);
			float num2 = Mathf.Abs(num);
			if (num2 <= 45f)
			{
				this.m_Animation = "Stop" + str;
				return;
			}
			if (num2 <= 135f)
			{
				this.m_Animation = ((num >= 0f) ? ("Stop" + str + "Right_90") : ("Stop" + str + "Left_90"));
				return;
			}
			this.m_Animation = ((num >= 0f) ? ("Stop" + str + "Right_180") : ("Stop" + str + "Left_180"));
		}

		protected override bool ShouldFinish()
		{
			return this.m_Finish;
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.StopMoveEnd)
			{
				this.m_Finish = true;
			}
		}

		public override void Update()
		{
			base.Update();
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized2D), Time.deltaTime * 0.5f);
		}

		private bool m_Finish;
	}
}
