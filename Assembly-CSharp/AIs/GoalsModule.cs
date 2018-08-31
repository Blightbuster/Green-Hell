using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalsModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.Deactivate();
			}
			this.m_Goals.Clear();
			if (!AIManager.Get().m_GoalParsers.ContainsKey((int)this.m_AI.m_ID))
			{
				DebugUtils.Assert("[GoalsModule:Initialize] ERROR, missing goals parser of ai " + this.m_AI.m_ID.ToString(), true, DebugUtils.AssertType.Info);
				return;
			}
			TextAssetParser textAssetParser = null;
			if (this.m_AI.m_PresetName != string.Empty)
			{
				textAssetParser = AIManager.Get().GetGoalParser(this.m_AI.m_PresetName);
			}
			if (textAssetParser == null)
			{
				textAssetParser = AIManager.Get().GetRandomGoalsParser(this.m_AI.m_ID);
			}
			for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
			{
				Key key = textAssetParser.GetKey(i);
				if (key.GetName() == "Goal")
				{
					AIGoal aigoal = this.CreateGoal(key.GetVariable(0).SValue);
					aigoal.m_Priority = key.GetVariable(1).IValue;
					aigoal.m_Probability = ((key.GetVariablesCount() <= 2) ? 1f : key.GetVariable(2).FValue);
					if (aigoal.m_Type == AIGoalType.HumanJumpBack || aigoal.m_Type == AIGoalType.JumpBack)
					{
						this.m_JumpBackGoal = aigoal;
					}
					else if (aigoal.m_Type == AIGoalType.HumanPunchBack || aigoal.m_Type == AIGoalType.PunchBack)
					{
						this.m_PunchBackGoal = aigoal;
					}
					else if (aigoal.m_Type == AIGoalType.HumanTaunt)
					{
						this.m_TauntGoal = aigoal;
					}
					this.m_Goals.Add(aigoal);
				}
				else
				{
					DebugUtils.Assert("[GoalsModule::Initialize] Unknown keyword - " + key.GetName(), true, DebugUtils.AssertType.Info);
				}
			}
			if (this.m_GoalToActivate != AIGoalType.None)
			{
				this.ActivateGoal(this.m_GoalToActivate);
				this.m_GoalToActivate = AIGoalType.None;
			}
		}

		private AIGoal CreateGoal(string goal_name)
		{
			Type type = Type.GetType("AIs.Goal" + goal_name);
			if (DebugUtils.Assert(type != null, "[GoalsModule::CreateGoal] ERROR - Can't create goal " + goal_name, true, DebugUtils.AssertType.Info))
			{
				return null;
			}
			AIGoal aigoal = Activator.CreateInstance(type) as AIGoal;
			aigoal.Initialize(this.m_AI);
			return aigoal;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_ActiveGoal != null && !this.m_ActiveGoal.ShouldPerform())
			{
				this.m_ActiveGoal.Deactivate();
			}
			this.SetupActiveGoal();
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.OnUpdate();
			}
		}

		public void SetupActiveGoal()
		{
			if (this.m_ForcedGoal != AIGoalType.None)
			{
				if (this.m_ActiveGoal == null || this.m_ActiveGoal.m_Type != this.m_ForcedGoal)
				{
					for (int i = 0; i < this.m_Goals.Count; i++)
					{
						AIGoal aigoal = this.m_Goals[i];
						if (aigoal.m_Type == this.m_ForcedGoal)
						{
							this.ActivateGoal(aigoal);
							return;
						}
					}
				}
				return;
			}
			int num = (this.m_ActiveGoal == null) ? int.MaxValue : this.m_ActiveGoal.m_Priority;
			for (int j = 0; j < this.m_Goals.Count; j++)
			{
				AIGoal aigoal = this.m_Goals[j];
				if (aigoal.m_Priority >= num)
				{
					break;
				}
				float num2 = (aigoal.m_Probability != 1f) ? UnityEngine.Random.Range(0f, 1f) : 1f;
				if (num2 <= aigoal.m_Probability)
				{
					if (aigoal.ShouldPerform())
					{
						this.ActivateGoal(aigoal);
						break;
					}
				}
			}
			if (this.m_ActiveGoal == null)
			{
				this.ActivateGoal(AIGoalType.Idle);
				if (this.m_ActiveGoal == null)
				{
					DebugUtils.Assert("[GoalsModule::SetupActiveGoal] Can't find proper goal - " + this.m_AI.name, true, DebugUtils.AssertType.Info);
					bool flag = false;
					if (flag)
					{
						this.SetupActiveGoal();
					}
				}
			}
		}

		public void ActivateGoal(AIGoalType type)
		{
			if (this.m_Goals.Count == 0)
			{
				this.m_GoalToActivate = type;
				return;
			}
			for (int i = 0; i < this.m_Goals.Count; i++)
			{
				if (this.m_Goals[i].m_Type == type)
				{
					this.ActivateGoal(this.m_Goals[i]);
					break;
				}
			}
		}

		private void ActivateGoal(AIGoal goal)
		{
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.Deactivate();
			}
			this.m_ActiveGoal = goal;
			this.m_ActiveGoal.Activate();
			this.OnActivateGoal(goal);
		}

		public void OnActivateGoal(AIGoal goal)
		{
			if (this.m_AI.m_BodyRotationModule)
			{
				this.m_AI.m_BodyRotationModule.m_LookAtPlayer = this.m_ActiveGoal.ShouldLookAtPlayer();
				this.m_AI.m_BodyRotationModule.m_RotateToPlayer = this.m_ActiveGoal.ShouldLookAtPlayer();
			}
		}

		public void OnDeactivateGoal(AIGoal goal)
		{
			this.m_PrevGoal = this.m_ActiveGoal;
			this.m_ActiveGoal = null;
			this.m_AI.m_AnimationModule.SetWantedAnim(string.Empty);
		}

		private void OnDisable()
		{
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.Deactivate();
			}
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.OnAnimEvent(id);
			}
		}

		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			if (this.m_ActiveGoal != null)
			{
				this.m_ActiveGoal.OnLateUpdate();
			}
		}

		private List<AIGoal> m_Goals = new List<AIGoal>();

		public AIGoal m_PrevGoal;

		public AIGoal m_ActiveGoal;

		public AIGoalType m_ForcedGoal = AIGoalType.None;

		[NonSerialized]
		public AIAction m_CurrentAction;

		[NonSerialized]
		public AIAction m_PreviousAction;

		[NonSerialized]
		public AIGoal m_JumpBackGoal;

		[NonSerialized]
		public AIGoal m_PunchBackGoal;

		[NonSerialized]
		public AIGoal m_TauntGoal;

		private AIGoalType m_GoalToActivate = AIGoalType.None;
	}
}
