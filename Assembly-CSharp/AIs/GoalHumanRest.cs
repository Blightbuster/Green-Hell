using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalHumanRest : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Idle = (base.CreateAction(typeof(Idle)) as Idle);
			this.m_StartCrouch = (base.CreateAction(typeof(StartCrouch)) as StartCrouch);
			this.m_StartCrouch = (base.CreateAction(typeof(StartCrouch)) as StartCrouch);
			this.m_CrouchIdle = (base.CreateAction(typeof(CrouchIdle)) as CrouchIdle);
			this.m_StopCrouch = (base.CreateAction(typeof(StopCrouch)) as StopCrouch);
			this.m_HumanMoveTo = (base.CreateAction(typeof(HumanMoveTo)) as HumanMoveTo);
			this.m_HumanRotateTo = (base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo);
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_AI.m_MoveStyle = AIMoveStyle.Walk;
			if (this.m_AI.transform.position.Distance(this.m_HumanAI.m_StartPosition) <= this.m_Range)
			{
				base.StartAction(this.m_StartCrouch);
				return;
			}
			this.UpdateAction();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateAction();
		}

		private void UpdateAction()
		{
			if (this.m_AI.transform.position.Distance(this.m_HumanAI.m_StartPosition) > this.m_Range)
			{
				if (this.IsAction(typeof(CrouchIdle)))
				{
					base.StartAction(this.m_StopCrouch);
					return;
				}
				if (!this.IsAction(typeof(HumanMoveTo)) && !this.IsAction(typeof(HumanRotateTo)) && !this.IsAction(typeof(Idle)))
				{
					if (!this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveTo, this.m_HumanAI.m_StartPosition, this.m_Range))
					{
						base.StartAction(this.m_Idle);
						return;
					}
					base.StartAction(this.m_HumanMoveTo);
				}
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanMoveTo) || action.GetType() == typeof(HumanRotateTo))
			{
				if (Vector3.Angle(this.m_HumanAI.m_StartForward.GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 5f)
				{
					this.m_HumanRotateTo.SetupParams(this.m_HumanAI.transform.position + this.m_HumanAI.m_StartForward.GetNormalized2D(), 5f);
					base.StartAction(this.m_HumanRotateTo);
					return;
				}
				base.StartAction(this.m_Idle);
				return;
			}
			else
			{
				if (action.GetType() == typeof(Idle))
				{
					base.StartAction(this.m_StartCrouch);
					return;
				}
				if (action.GetType() == typeof(StartCrouch))
				{
					base.StartAction(this.m_CrouchIdle);
				}
				return;
			}
		}

		private float m_Range = 0.5f;

		private const float MAX_ANGLE = 5f;

		private Idle m_Idle;

		private StartCrouch m_StartCrouch;

		private CrouchIdle m_CrouchIdle;

		private StopCrouch m_StopCrouch;

		private HumanMoveTo m_HumanMoveTo;

		private HumanRotateTo m_HumanRotateTo;
	}
}
