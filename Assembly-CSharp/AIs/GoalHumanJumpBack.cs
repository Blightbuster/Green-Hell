using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanJumpBack : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_JumpBack = (base.CreateAction(typeof(JumpBack)) as JumpBack);
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.StartAction(this.m_JumpBack);
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_HumanAI.transform.position).GetNormalized2D();
			float num = this.m_HumanAI.transform.forward.GetNormalized2D().AngleSigned(normalized2D, Vector3.up);
			if (Mathf.Abs(num) > 45f)
			{
				this.m_HumanAI.transform.Rotate(Vector3.up, (num > 0f) ? (num - 45f) : (num + 45f));
			}
			this.m_StartPos = this.m_AI.transform.position;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.m_AI.transform.position.Distance2D(this.m_StartPos) < 2f)
			{
				this.m_AI.m_HumanFightModule.m_LastFailedJumpBackPos = this.m_AI.transform.position;
			}
		}

		private const float MAX_ANGLE = 45f;

		private const float MIN_SUCCESS_DIST = 2f;

		private JumpBack m_JumpBack;

		private Vector3 m_StartPos = Vector3.zero;
	}
}
