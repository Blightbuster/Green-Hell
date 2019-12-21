using System;

public class Objective
{
	public ObjectiveState GetState()
	{
		return this.m_State;
	}

	public void SetState(ObjectiveState state)
	{
		this.m_State = state;
	}

	public Objective(string name, string text_id)
	{
		this.m_Name = name;
		this.m_TextID = text_id;
	}

	public bool IsCompleted()
	{
		return this.m_State == ObjectiveState.Completed;
	}

	public string m_Name = string.Empty;

	public string m_TextID = string.Empty;

	private ObjectiveState m_State;
}
