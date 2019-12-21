using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanThrowStone : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_ThrowStone = (base.CreateAction(typeof(ThrowStone)) as ThrowStone);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			if (!this.m_AI.m_EnemyModule.m_Enemy)
			{
				return false;
			}
			float num = this.m_AI.m_EnemyModule.m_Enemy.transform.position.Distance(this.m_AI.transform.position);
			if (num > 15f || num < 6f || this.m_AI.m_GoalsModule.m_PrevGoal == this)
			{
				return false;
			}
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.GetHeadTransform().position - this.m_AI.GetHeadTransform().position).normalized;
			Debug.DrawLine(this.m_AI.GetHeadTransform().position + normalized, this.m_AI.m_EnemyModule.m_Enemy.GetHeadTransform().position - normalized, Color.blue);
			RaycastHit raycastHit;
			return Physics.Raycast(this.m_AI.GetHeadTransform().position + normalized, normalized, out raycastHit) && (raycastHit.collider.gameObject.IsPlayer() || raycastHit.collider.gameObject == Camera.main.gameObject);
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (Vector3.Angle((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 10f)
			{
				HumanRotateTo humanRotateTo = base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo;
				humanRotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.transform.position, 10f);
				base.StartAction(humanRotateTo);
				return;
			}
			base.StartAction(this.m_ThrowStone);
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_ThrowStone);
			}
		}

		private const float MAX_ANGLE = 10f;

		private ThrowStone m_ThrowStone;
	}
}
