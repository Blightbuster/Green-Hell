using System;

public class ScenarioElement
{
	public virtual void Setup()
	{
	}

	public ScenarioElement.State GetActionState()
	{
		return this.m_State;
	}

	public virtual void Activate()
	{
		this.m_State = ScenarioElement.State.InProgress;
	}

	public void Complete()
	{
		this.m_State = ScenarioElement.State.Completed;
	}

	public void Reset()
	{
		this.m_State = ScenarioElement.State.None;
	}

	public bool IsState(ScenarioElement.State state)
	{
		return this.m_State == state;
	}

	public void Update()
	{
		if (this.ShouldComplete())
		{
			this.Complete();
		}
	}

	protected virtual bool ShouldComplete()
	{
		return false;
	}

	public virtual bool IsCondition()
	{
		return false;
	}

	public virtual bool IsAction()
	{
		return false;
	}

	public virtual void Save(ScenarioNode node, int index)
	{
		SaveGame.SaveVal(node.m_Name + "_" + index, (int)this.m_State);
	}

	public virtual void Load(ScenarioNode node, int index)
	{
		this.m_State = (ScenarioElement.State)SaveGame.LoadIVal(node.m_Name + "_" + index);
	}

	public string m_Content = string.Empty;

	public string m_EncodedContent = string.Empty;

	public ScenarioElement.State m_State;

	protected string m_Param0 = string.Empty;

	protected string m_Param1 = string.Empty;

	public bool m_And;

	public ScenarioNode m_Node;

	public enum State
	{
		None,
		InProgress,
		Completed
	}
}
