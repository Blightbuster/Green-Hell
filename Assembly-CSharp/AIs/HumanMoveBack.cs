using System;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class HumanMoveBack : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_State = ((this.m_AI.m_GoalsModule.m_PreviousAction.GetType() == typeof(HumanMoveTo)) ? HumanMoveBack.State.Move : HumanMoveBack.State.Start);
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
			Vector3 normalized2D2 = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.transform.position).GetNormalized2D();
			float num = normalized2D.AngleSigned(normalized2D2, Vector3.up);
			float num2 = Mathf.Abs(num);
			if (num2 <= 45f)
			{
				this.m_StartAnimName = "Start" + text + "Back";
			}
			else if (num2 <= 135f)
			{
				this.m_StartAnimName = ((num < 0f) ? ("Start" + text + "BackLeft_90") : ("Start" + text + "BackRight_90"));
			}
			else
			{
				this.m_StartAnimName = ((num < 0f) ? ("Start" + text + "BackLeft_180") : ("Start" + text + "BackRight_180"));
			}
			this.m_MoveAnimName = text + "Back";
			this.m_StopAnimName = "Stop" + text + "Back";
		}

		public override void Update()
		{
			base.Update();
			this.UpdateWantedDir();
			this.UpdateMoveState();
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.StartMoveEnd)
			{
				this.m_State = HumanMoveBack.State.Move;
			}
			if (id == AnimEventID.StopMoveEnd)
			{
				this.m_State = HumanMoveBack.State.Finish;
			}
			else
			{
				base.OnAnimEvent(id);
			}
		}

		private void UpdateMoveState()
		{
			if (this.m_State != HumanMoveBack.State.Move)
			{
				return;
			}
			if (this.m_AI.m_PathModule.m_Agent.remainingDistance <= this.m_AI.GetPathPassDistance())
			{
				this.m_State = ((!this.m_PerformStop) ? HumanMoveBack.State.Finish : HumanMoveBack.State.Stop);
			}
		}

		private void UpdateWantedDir()
		{
			this.m_AI.m_TransformModule.m_WantedDirection = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
		}

		public override string GetAnimName()
		{
			HumanMoveBack.State state = this.m_State;
			if (state == HumanMoveBack.State.Start)
			{
				return this.m_StartAnimName;
			}
			if (state == HumanMoveBack.State.Move)
			{
				return this.m_MoveAnimName;
			}
			if (state != HumanMoveBack.State.Stop)
			{
				return string.Empty;
			}
			return this.m_StopAnimName;
		}

		protected override bool ShouldFinish()
		{
			return !this.m_AI.m_PathModule.m_Agent.pathPending && (this.m_AI.m_PathModule.m_Agent.pathStatus == NavMeshPathStatus.PathInvalid || this.m_State == HumanMoveBack.State.Finish);
		}

		private HumanMoveBack.State m_State;

		private string m_StartAnimName = string.Empty;

		private string m_MoveAnimName = string.Empty;

		private string m_StopAnimName = string.Empty;

		public bool m_PerformStop = true;

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
