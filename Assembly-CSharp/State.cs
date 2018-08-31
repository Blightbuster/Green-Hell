using System;
using System.Collections.Generic;

public class State
{
	public State(Delegator del)
	{
		this.m_Del = del;
	}

	public void Load(Key key)
	{
		this.m_Name = key.GetVariable(0).SValue;
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "OnEnter")
			{
				this.m_OnEnterMethods.Add(key2.GetVariable(0).SValue);
			}
			else if (key2.GetName() == "OnExit")
			{
				this.m_OnExitMethods.Add(key2.GetVariable(0).SValue);
			}
			else if (key2.GetName() == "Update")
			{
				this.m_UpdateMethods.Add(key2.GetVariable(0).SValue);
			}
		}
	}

	public string GetName()
	{
		return this.m_Name;
	}

	public void SetName(string name)
	{
		this.m_Name = name;
	}

	public void SetupTransitions(StateMachine machine)
	{
		List<StateTransition> transitions = machine.GetTransitions();
		for (int i = 0; i < transitions.Count; i++)
		{
			List<State> fromStates = transitions[i].GetFromStates();
			for (int j = 0; j < fromStates.Count; j++)
			{
				if (fromStates[j] == this)
				{
					this.m_Transitions.Add(transitions[i]);
				}
			}
		}
	}

	public List<StateTransition> GetTransitions()
	{
		return this.m_Transitions;
	}

	public void OnEnter()
	{
		for (int i = 0; i < this.m_OnEnterMethods.Count; i++)
		{
			this.m_Del.CallVoid(this.m_OnEnterMethods[i]);
		}
	}

	public void OnExit()
	{
		for (int i = 0; i < this.m_OnExitMethods.Count; i++)
		{
			this.m_Del.CallVoid(this.m_OnExitMethods[i]);
		}
	}

	public void Update(StateMachine machine)
	{
		for (int i = 0; i < this.m_UpdateMethods.Count; i++)
		{
			this.m_Del.CallVoid(this.m_UpdateMethods[i]);
		}
		List<StateTransition> transitions = this.GetTransitions();
		for (int j = 0; j < transitions.Count; j++)
		{
			if (transitions[j].CanBeApplied())
			{
				machine.ApplyTransition(transitions[j]);
				return;
			}
		}
	}

	private string m_Name;

	private List<StateTransition> m_Transitions = new List<StateTransition>();

	private Delegator m_Del;

	private List<string> m_OnEnterMethods = new List<string>();

	private List<string> m_OnExitMethods = new List<string>();

	private List<string> m_UpdateMethods = new List<string>();
}
