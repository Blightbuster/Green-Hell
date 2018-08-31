using System;
using UnityEngine;

namespace AIs
{
	public class GoalSnakeLookAtEnemy : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			return this.m_AI.m_EnemyModule.m_Enemy != null;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.StartIdle();
		}

		private void StartIdle()
		{
			base.StartAction(this.m_Idle);
			this.m_Idle.SetupParams(float.MaxValue);
			this.m_Rotation = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			Vector3 normalized2D = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
			if (!this.m_Rotation)
			{
				float f = Vector3.Angle(this.m_AI.transform.forward.GetNormalized2D(), normalized2D);
				if (Mathf.Abs(f) > RotateTo.MAX_ANGLE)
				{
					base.StartAction(this.m_RotateTo);
					this.m_RotateTo.SetupParams(Player.Get().gameObject, false);
					this.m_Rotation = true;
				}
			}
			else
			{
				this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(RotateTo))
			{
				this.StartIdle();
			}
		}

		private bool m_Rotation;

		private Idle m_Idle;

		private RotateTo m_RotateTo;
	}
}
