﻿using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class KidRunnerMoveTo : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_KidRunner = (KidRunnerAI)this.m_AI;
			this.m_State = ((this.m_AI.m_GoalsModule.m_PreviousAction == null || this.m_AI.m_GoalsModule.m_PreviousAction.GetType() != typeof(KidRunnerMoveTo)) ? KidRunnerMoveTo.State.Start : KidRunnerMoveTo.State.Move);
			this.SetupAnims();
		}

		private void SetupAnims()
		{
			string text = "Run";
			Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
			Vector3 normalized2D2 = (this.m_KidRunner.m_CurrentTarget - this.m_AI.transform.position).GetNormalized2D();
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
			this.UpdateMoveState();
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.StartMoveEnd)
			{
				this.m_State = KidRunnerMoveTo.State.Move;
			}
			if (id == AnimEventID.StopMoveEnd)
			{
				this.m_State = KidRunnerMoveTo.State.Finish;
				return;
			}
			base.OnAnimEvent(id);
		}

		private void UpdateMoveState()
		{
			if (this.m_State != KidRunnerMoveTo.State.Move)
			{
				return;
			}
			if ((this.m_KidRunner.m_CurrentTarget - this.m_AI.transform.position).magnitude <= this.m_AI.GetPathPassDistance())
			{
				this.m_State = (this.m_PerformStop ? KidRunnerMoveTo.State.Stop : KidRunnerMoveTo.State.Finish);
			}
		}

		private void UpdateWantedDir()
		{
			if (this.m_State == KidRunnerMoveTo.State.Move)
			{
				Vector3 normalized2D = (this.m_KidRunner.m_CurrentTarget - this.m_AI.transform.position).GetNormalized2D();
				this.m_AI.m_TransformModule.m_WantedDirection = normalized2D;
			}
		}

		public override string GetAnimName()
		{
			switch (this.m_State)
			{
			case KidRunnerMoveTo.State.Start:
				return this.m_StartAnimName;
			case KidRunnerMoveTo.State.Move:
				return this.m_MoveAnimName;
			case KidRunnerMoveTo.State.Stop:
				return this.m_StopAnimName;
			default:
				return string.Empty;
			}
		}

		protected override bool ShouldFinish()
		{
			return false;
		}

		private KidRunnerMoveTo.State m_State;

		private string m_StartAnimName = string.Empty;

		private string m_MoveAnimName = string.Empty;

		private string m_StopAnimName = string.Empty;

		public bool m_PerformStop = true;

		private KidRunnerAI m_KidRunner;

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
