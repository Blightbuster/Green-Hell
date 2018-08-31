using System;
using System.Collections.Generic;

public class StateTransition
{
	public StateTransition(Delegator del)
	{
		this.m_Del = del;
	}

	public List<State> GetFromStates()
	{
		return this.m_FromStates;
	}

	public State GetToState()
	{
		return this.m_ToState;
	}

	public StateGroup GetFromStateGroup()
	{
		return this.m_FromStateGroup;
	}

	public void Load(Key key, StateMachine machine)
	{
		string svalue = key.GetVariable(0).SValue;
		State stateByName = machine.GetStateByName(svalue);
		if (stateByName != null)
		{
			this.m_FromStates.Add(stateByName);
		}
		else
		{
			StateGroup statesGroupByName = machine.GetStatesGroupByName(svalue);
			if (statesGroupByName == null)
			{
				DebugUtils.Assert("[StateTransition::Load] Can't find from state!", true, DebugUtils.AssertType.Info);
				return;
			}
			List<State> states = statesGroupByName.GetStates();
			for (int i = 0; i < states.Count; i++)
			{
				this.m_FromStates.Add(states[i]);
			}
		}
		this.m_ToState = machine.GetStateByName(key.GetVariable(1).SValue);
		for (int j = 0; j < key.GetKeysCount(); j++)
		{
			Key key2 = key.GetKey(j);
			if (key2.GetName() == "Cnd")
			{
				this.m_ConditionMethods.Add(key2.GetVariable(0).SValue);
			}
		}
	}

	public bool CanBeApplied()
	{
		if (this.m_Del == null)
		{
			DebugUtils.Assert("[StateTransition::CanBeApplied] Error, delegator is not set!", true, DebugUtils.AssertType.Info);
			return true;
		}
		for (int i = 0; i < this.m_ConditionMethods.Count; i++)
		{
			if (!this.m_Del.CallBool(this.m_ConditionMethods[i]))
			{
				return false;
			}
		}
		return true;
	}

	private Delegator m_Del;

	private List<State> m_FromStates = new List<State>();

	private StateGroup m_FromStateGroup;

	private State m_ToState;

	private List<string> m_ConditionMethods = new List<string>();
}
