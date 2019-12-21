using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HumanFollowPatrolPath : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			this.m_HumanAI = (HumanAI)ai;
		}

		public override void Start()
		{
			base.Start();
			this.m_State = HumanFollowPatrolPath.State.Start;
			this.SetupAnims();
		}

		private void SetupAnims()
		{
			string text = string.Empty;
			switch (this.m_AI.m_MoveStyle)
			{
			case AIMoveStyle.Walk:
				text = "Walk";
				break;
			case AIMoveStyle.Run:
				text = "Run";
				break;
			case AIMoveStyle.Sneak:
				text = "Sneak";
				break;
			case AIMoveStyle.Swim:
				text = "Swim";
				break;
			case AIMoveStyle.Trot:
				text = "Trot";
				break;
			case AIMoveStyle.Jump:
				text = "Jump";
				break;
			}
			Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
			Vector3 normalized2D2 = (this.m_AI.transform.position - this.m_AI.m_PatrolModule.m_CurrentPathPoint.transform.position).GetNormalized2D();
			float num = normalized2D.AngleSigned(normalized2D2, Vector3.up);
			float num2 = Mathf.Abs(num);
			if (num2 <= 45f)
			{
				this.m_StartAnimName = "Start" + text + "Forward";
			}
			else if (num2 <= 135f)
			{
				this.m_StartAnimName = ((num >= 0f) ? ("Start" + text + "Right_90") : ("Start" + text + "Left_90"));
			}
			else
			{
				this.m_StartAnimName = ((num >= 0f) ? ("Start" + text + "Right_180") : ("Start" + text + "Left_180"));
			}
			this.m_MoveAnimName = text;
			this.m_StopAnimName = "Stop" + text;
		}

		public override void Update()
		{
			base.Update();
			this.UpdateWantedDir();
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.StartMoveEnd)
			{
				this.m_State = HumanFollowPatrolPath.State.Move;
				this.m_AI.m_AnimationModule.m_StartFromRandomFrame = true;
			}
			if (id == AnimEventID.StopMoveEnd)
			{
				this.m_State = HumanFollowPatrolPath.State.Finish;
				return;
			}
			base.OnAnimEvent(id);
		}

		private void UpdateMoveState()
		{
			if (this.m_State != HumanFollowPatrolPath.State.Move)
			{
				return;
			}
			if (this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance())
			{
				this.m_State = (this.m_PerformStop ? HumanFollowPatrolPath.State.Stop : HumanFollowPatrolPath.State.Finish);
			}
		}

		private void UpdateWantedDir()
		{
			if (this.m_State == HumanFollowPatrolPath.State.Move)
			{
				Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
				this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
			}
		}

		public override string GetAnimName()
		{
			switch (this.m_State)
			{
			case HumanFollowPatrolPath.State.Start:
				return this.m_StartAnimName;
			case HumanFollowPatrolPath.State.Move:
				return this.m_MoveAnimName;
			case HumanFollowPatrolPath.State.Stop:
				return this.m_StopAnimName;
			default:
				return string.Empty;
			}
		}

		protected override bool ShouldFinish()
		{
			return false;
		}

		private HumanFollowPatrolPath.State m_State;

		private string m_StartAnimName = string.Empty;

		private string m_MoveAnimName = string.Empty;

		private string m_StopAnimName = string.Empty;

		public bool m_PerformStop = true;

		private HumanAI m_HumanAI;

		private enum State
		{
			None,
			Start,
			Move,
			Stop,
			Finish
		}
	}
}
