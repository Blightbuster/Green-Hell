using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIGoal
	{
		public virtual void Initialize(AI ai)
		{
			this.m_AI = ai;
			DebugUtils.Assert(this.m_AI, true);
			string text = base.GetType().ToString();
			text = text.Remove(0, 8);
			this.m_Type = (AIGoalType)Enum.Parse(typeof(AIGoalType), text);
		}

		public virtual bool ShouldPerform()
		{
			return false;
		}

		public void Activate()
		{
			this.m_Active = true;
			this.OnActivate();
		}

		protected virtual void OnActivate()
		{
			this.ResetPlan();
			this.m_CurrentAction = null;
			this.Prepare();
			this.m_ActivationTime = Time.time;
			if (this.m_AI.m_SoundModule != null)
			{
				this.m_AI.m_SoundModule.OnGoalActivate(this.m_Type);
			}
			this.m_WasDamage = false;
		}

		public void Deactivate()
		{
			this.m_Active = false;
			this.OnDeactivate();
		}

		protected virtual void OnDeactivate()
		{
			this.ResetPlan();
			this.m_CurrentAction = null;
			this.m_ActivationTime = -1f;
			this.m_AI.m_GoalsModule.OnDeactivateGoal(this);
			this.m_WasDamage = false;
		}

		protected AIAction StartAction(AIAction action)
		{
			this.m_Plan.Clear();
			this.m_CurrentAction = action;
			this.m_CurrentAction.Start();
			this.m_Plan.Add(this.m_CurrentAction);
			return this.m_CurrentAction;
		}

		protected AIAction CreateAction(Type type)
		{
			AIAction aiaction = Activator.CreateInstance(type) as AIAction;
			aiaction.Initialize(this.m_AI, this);
			return aiaction;
		}

		protected virtual void Prepare()
		{
		}

		protected void AddToPlan(AIAction action)
		{
			action.Reset();
			this.m_Plan.Add(action);
		}

		protected void ResetPlan()
		{
			for (int i = 0; i < this.m_Plan.Count; i++)
			{
				this.m_Plan[i].Reset();
			}
			this.m_Plan.Clear();
			this.m_CurrentAction = null;
		}

		protected int GetPlanCount()
		{
			return this.m_Plan.Count;
		}

		protected AIAction GetAction(int index)
		{
			return (this.GetPlanCount() <= index) ? null : this.m_Plan[index];
		}

		protected float GetDuration()
		{
			return (this.m_ActivationTime < 0f) ? 0f : (Time.time - this.m_ActivationTime);
		}

		public virtual void OnUpdate()
		{
			this.UpdatePlan();
		}

		private void UpdatePlan()
		{
			if (this.m_Plan.Count == 0)
			{
				this.Deactivate();
				return;
			}
			if (this.m_CurrentAction == null)
			{
				this.m_CurrentAction = this.m_Plan[0];
			}
			if (this.m_CurrentAction.IsNone())
			{
				this.m_CurrentAction.Start();
			}
			this.m_CurrentAction.UpdateState();
			if (this.m_CurrentAction == null)
			{
				return;
			}
			if (this.m_CurrentAction.IsFailed())
			{
				this.Deactivate();
			}
			else if (this.m_CurrentAction.IsFinished())
			{
				this.m_Plan.Remove(this.m_CurrentAction);
				this.m_CurrentAction = null;
				if (this.m_Plan.Count == 0)
				{
					this.Deactivate();
				}
			}
			else
			{
				this.m_CurrentAction.Update();
			}
		}

		public virtual void OnLateUpdate()
		{
		}

		public virtual bool ShouldLookAtPlayer()
		{
			return false;
		}

		public virtual bool ShouldRotateToPlayer()
		{
			return false;
		}

		public virtual void OnStopAction(AIAction action)
		{
		}

		public virtual void OnAnimEvent(AnimEventID id)
		{
			this.m_CurrentAction.OnAnimEvent(id);
		}

		public AIGoalType m_Type = AIGoalType.None;

		protected AI m_AI;

		private List<AIAction> m_Plan = new List<AIAction>(10);

		private AIAction m_CurrentAction;

		protected float m_ActivationTime = -1f;

		protected bool m_Active;

		public float m_Probability = 1f;

		public int m_Priority;

		public bool m_WasDamage;
	}
}
