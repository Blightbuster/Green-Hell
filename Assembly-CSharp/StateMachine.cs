using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
	public void Init(TextAsset asset, Delegator del)
	{
		this.m_States = new List<State>();
		this.m_StateGroups = new List<StateGroup>();
		this.m_Transitions = new List<StateTransition>();
		this.m_Del = del;
		this.Load(asset);
		this.SetupStates();
	}

	private void Load(TextAsset asset)
	{
		TextAssetParser textAssetParser = new TextAssetParser();
		textAssetParser.Parse(asset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "State")
			{
				this.CreateState(key, false);
			}
			else if (key.GetName() == "DefaultState")
			{
				this.CreateState(key, true);
			}
			else if (key.GetName() == "StateGroup")
			{
				this.CreateStateGroup(key);
			}
			else if (key.GetName() == "Transition")
			{
				this.CreateTransition(key);
			}
		}
	}

	private void CreateState(Key key, bool def = false)
	{
		State state = new State(this.m_Del);
		state.Load(key);
		this.m_States.Add(state);
		if (def)
		{
			if (this.m_CurrentState != null)
			{
				DebugUtils.Assert("[StateMachine::CreateState] Error, more than one state is set as default!", true, DebugUtils.AssertType.Info);
				return;
			}
			this.m_CurrentState = state;
		}
	}

	private void CreateStateGroup(Key key)
	{
		StateGroup stateGroup = new StateGroup();
		stateGroup.Load(key, this);
		this.m_StateGroups.Add(stateGroup);
	}

	private void CreateTransition(Key key)
	{
		StateTransition stateTransition = new StateTransition(this.m_Del);
		stateTransition.Load(key, this);
		this.m_Transitions.Add(stateTransition);
	}

	private void SetupStates()
	{
		for (int i = 0; i < this.m_States.Count; i++)
		{
			this.m_States[i].SetupTransitions(this);
		}
	}

	public State GetStateByName(string name)
	{
		for (int i = 0; i < this.m_States.Count; i++)
		{
			if (this.m_States[i].GetName() == name)
			{
				return this.m_States[i];
			}
		}
		return null;
	}

	public StateGroup GetStatesGroupByName(string name)
	{
		for (int i = 0; i < this.m_StateGroups.Count; i++)
		{
			if (this.m_StateGroups[i].GetName() == name)
			{
				return this.m_StateGroups[i];
			}
		}
		return null;
	}

	public List<StateTransition> GetTransitions()
	{
		return this.m_Transitions;
	}

	public List<StateGroup> GetGroups()
	{
		return this.m_StateGroups;
	}

	public void Update()
	{
		if (this.m_CurrentState == null)
		{
			return;
		}
		this.m_CurrentState.Update(this);
	}

	private State GetNextState(StateTransition transition)
	{
		if (transition == null)
		{
			return null;
		}
		return transition.GetToState();
	}

	public void ApplyTransition(StateTransition transition)
	{
		if (transition == null)
		{
			return;
		}
		State nextState = this.GetNextState(transition);
		DebugUtils.Assert(nextState != null, "[StateMachine::ApplyTransition] Can't set next state!", true, DebugUtils.AssertType.Info);
		if (nextState == null)
		{
			return;
		}
		this.m_CurrentState.OnExit();
		this.m_CurrentState = nextState;
		this.m_StartStateTime = Time.time;
		Debug.Log(this.m_CurrentState.GetName());
		this.m_CurrentState.OnEnter();
	}

	public void SetState(string state_name)
	{
		State stateByName = this.GetStateByName(state_name);
		if (stateByName != null)
		{
			this.m_CurrentState = stateByName;
		}
	}

	public float GetCurrentStateDuration()
	{
		return Time.time - this.m_StartStateTime;
	}

	private Delegator m_Del;

	private State m_CurrentState;

	private List<State> m_States;

	private List<StateGroup> m_StateGroups;

	private List<StateTransition> m_Transitions;

	private float m_StartStateTime;
}
