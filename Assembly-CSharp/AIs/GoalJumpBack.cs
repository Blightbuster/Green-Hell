using System;
using UnityEngine;

namespace AIs
{
	public class GoalJumpBack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_JumpBack = (base.CreateAction(typeof(JumpBack)) as JumpBack);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.StartAction(this.m_JumpBack);
			Vector3 normalized2D = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
			float num = this.m_AI.transform.forward.GetNormalized2D().AngleSigned(normalized2D, Vector3.up);
			if (Mathf.Abs(num) > 45f)
			{
				this.m_AI.transform.Rotate(Vector3.up, (num > 0f) ? (num - 45f) : (num + 45f));
			}
		}

		private const float MAX_ANGLE = 45f;

		private JumpBack m_JumpBack;
	}
}
