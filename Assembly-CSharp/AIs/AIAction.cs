using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIAction
	{
		public virtual void Initialize(AI ai, AIGoal goal)
		{
			this.m_AI = ai;
			DebugUtils.Assert(this.m_AI, true);
			this.m_Goal = goal;
		}

		public bool IsNone()
		{
			return this.m_State == AIAction.State.None;
		}

		public bool IsInProgress()
		{
			return this.m_State == AIAction.State.InProgress;
		}

		public bool IsFinished()
		{
			return this.m_State == AIAction.State.Finished;
		}

		public bool IsFailed()
		{
			return this.m_State == AIAction.State.Failed;
		}

		public virtual void OnAnimEvent(AnimEventID id)
		{
		}

		public virtual void Reset()
		{
			this.m_State = AIAction.State.None;
			this.m_StartTime = 0f;
		}

		private void Finish()
		{
			this.m_State = AIAction.State.Finished;
			this.Stop();
		}

		private void Fail()
		{
			this.m_State = AIAction.State.Failed;
			this.Stop();
		}

		protected virtual void Stop()
		{
			this.m_Goal.OnStopAction(this);
		}

		public virtual void SetupParams()
		{
		}

		public virtual void Start()
		{
			this.m_State = AIAction.State.InProgress;
			this.m_StartTime = Time.time;
			this.SetupTransitionDuration();
			this.m_AI.m_GoalsModule.m_PreviousAction = this.m_AI.m_GoalsModule.m_CurrentAction;
			this.m_AI.m_GoalsModule.m_CurrentAction = this;
			this.m_AI.m_AnimationModule.ForceReset();
		}

		protected virtual void SetupTransitionDuration()
		{
			this.m_AI.m_AnimationModule.m_TransitionDuration = AnimationModule.DEFAULT_TRANSITION_DURATION;
		}

		protected float GetDuration()
		{
			if (this.IsFinished() || this.IsFailed())
			{
				return -1f;
			}
			return Time.time - this.m_StartTime;
		}

		public virtual void Update()
		{
			this.UpdateAnimation();
		}

		public void UpdateState()
		{
			if (this.ShouldFinish())
			{
				this.Finish();
				return;
			}
			if (this.ShouldFail())
			{
				this.Fail();
			}
		}

		protected virtual bool ShouldFail()
		{
			return false;
		}

		protected virtual bool ShouldFinish()
		{
			return false;
		}

		public virtual string GetAnimName()
		{
			return this.m_Animation;
		}

		protected virtual void UpdateAnimation()
		{
			this.m_AI.m_AnimationModule.SetWantedAnim(this.GetAnimName());
		}

		protected bool IsAnimFinishing()
		{
			return this.m_AI.m_AnimationModule.IsAnimFinishing(this.GetAnimName());
		}

		protected AI m_AI;

		private AIAction.State m_State;

		protected float m_StartTime = -1f;

		private AIGoal m_Goal;

		protected string m_Animation = string.Empty;

		private enum State
		{
			None,
			InProgress,
			Finished,
			Failed
		}
	}
}
