using System;
using System.Collections.Generic;

public class StateGroup
{
	public void Load(Key key, StateMachine machine)
	{
		this.m_Name = key.GetVariable(0).SValue;
		int num = 1;
		string text = key.GetVariable(num).SValue;
		while (text.Length > 0)
		{
			State stateByName = machine.GetStateByName(text);
			if (stateByName != null)
			{
				this.m_States.Add(stateByName);
			}
			num++;
			CJVariable variable = key.GetVariable(num);
			text = ((variable == null) ? string.Empty : variable.SValue);
		}
	}

	public string GetName()
	{
		return this.m_Name;
	}

	public List<State> GetStates()
	{
		return this.m_States;
	}

	private string m_Name = string.Empty;

	private List<State> m_States = new List<State>();
}
