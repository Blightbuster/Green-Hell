using System;
using System.Collections.Generic;

public class ScenarioNode
{
	public void Load(Key key)
	{
		this.m_Name = key.GetVariable(0).SValue;
		this.m_State = ((!key.GetVariable(1).BValue) ? ScenarioNode.State.Inactive : ScenarioNode.State.None);
		this.m_ParentNames = key.GetVariable(6).SValue;
		if (this.m_ParentNames == ScenarioNode.NO_PARENTS)
		{
			this.m_ParentNames = string.Empty;
		}
		this.m_Loop = key.GetVariable(7).BValue;
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Element")
			{
				string svalue = key2.GetVariable(0).SValue;
				ScenarioSyntaxData scenarioSyntaxData = ScenarioManager.Get().EncodeContent(svalue);
				if (scenarioSyntaxData == null)
				{
					DebugUtils.Assert(string.Concat(new string[]
					{
						"[ScenarioNode:Load] Can't decode element - ",
						svalue,
						", node - ",
						this.m_Name,
						". Check spelling!"
					}), true, DebugUtils.AssertType.Info);
				}
				else
				{
					string[] array = scenarioSyntaxData.m_Encoded.Split(new char[]
					{
						':'
					});
					DebugUtils.Assert(array.Length > 0, true);
					ScenarioElement scenarioElement = this.CreateElement("Scenario" + array[0]);
					if (scenarioElement == null)
					{
						DebugUtils.Assert("[ScenarioNode:Load] Can't create element - Scenario" + array[0], true, DebugUtils.AssertType.Info);
					}
					else
					{
						scenarioElement.m_Content = svalue;
						scenarioElement.m_EncodedContent = scenarioSyntaxData.m_Encoded;
						array = svalue.Split(new char[]
						{
							':'
						});
						for (int j = 1; j < array.Length; j++)
						{
							ScenarioElement scenarioElement2 = scenarioElement;
							scenarioElement2.m_EncodedContent = scenarioElement2.m_EncodedContent + ":" + array[j];
						}
						scenarioElement.m_And = key2.GetVariable(1).BValue;
						scenarioElement.m_Node = this;
						this.m_Elements.Add(scenarioElement);
					}
				}
			}
		}
	}

	public ScenarioElement CreateElement(string class_name)
	{
		Type type = Type.GetType(class_name);
		if (type == null)
		{
			DebugUtils.Assert(class_name, true, DebugUtils.AssertType.Info);
			return null;
		}
		return Activator.CreateInstance(type) as ScenarioElement;
	}

	public void Setup()
	{
		foreach (ScenarioElement scenarioElement in this.m_Elements)
		{
			scenarioElement.Setup();
		}
	}

	public void Activate()
	{
		this.m_State = ScenarioNode.State.Active;
		foreach (ScenarioNode scenarioNode in this.m_Parents)
		{
			scenarioNode.m_State = ScenarioNode.State.Completed;
		}
	}

	public void Deactivate()
	{
		this.m_State = ScenarioNode.State.Inactive;
	}

	public void Complete()
	{
		this.m_State = ScenarioNode.State.Completed;
		foreach (ScenarioNode scenarioNode in this.m_Childs)
		{
			if (scenarioNode.m_State == ScenarioNode.State.None || scenarioNode.m_State == ScenarioNode.State.Inactive)
			{
				bool flag = true;
				for (int i = 0; i < scenarioNode.m_Parents.Count; i++)
				{
					if (scenarioNode.m_Parents[i].m_State != ScenarioNode.State.Completed)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					scenarioNode.Activate();
				}
			}
		}
		if (this.m_Loop)
		{
			foreach (ScenarioElement scenarioElement in this.m_Elements)
			{
				scenarioElement.Reset();
			}
			this.Activate();
		}
	}

	private void SetupActiveElements()
	{
		this.m_ActiveElements.Clear();
		foreach (ScenarioElement scenarioElement in this.m_Elements)
		{
			if (scenarioElement.IsState(ScenarioElement.State.None))
			{
				this.m_ActiveElements.Add(scenarioElement);
				scenarioElement.Activate();
				if (!scenarioElement.m_And)
				{
					break;
				}
			}
		}
		if (this.m_ActiveElements.Count == 0)
		{
			this.Complete();
		}
	}

	public List<ScenarioElement> GetAllElements()
	{
		return this.m_Elements;
	}

	public void Update()
	{
		if (this.m_ActiveElements.Count == 0)
		{
			this.SetupActiveElements();
		}
		this.UpdateCurrentElement();
	}

	private void UpdateCurrentElement()
	{
		if (this.m_ActiveElements.Count == 0)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < this.m_ActiveElements.Count; i++)
		{
			ScenarioElement scenarioElement = this.m_ActiveElements[i];
			scenarioElement.Update();
			if (flag && !scenarioElement.IsState(ScenarioElement.State.Completed))
			{
				flag = false;
			}
		}
		if (flag)
		{
			this.SetupActiveElements();
		}
	}

	public bool IsActive()
	{
		return this.m_State == ScenarioNode.State.Active;
	}

	public bool IsCompleted()
	{
		return this.m_State == ScenarioNode.State.Completed;
	}

	public void Save()
	{
		SaveGame.SaveVal(this.m_Name, (int)this.m_State);
		for (int i = 0; i < this.m_Elements.Count; i++)
		{
			this.m_Elements[i].Save(this, i);
		}
	}

	public void Load()
	{
		this.m_ActiveElements.Clear();
		this.m_State = (ScenarioNode.State)SaveGame.LoadIVal(this.m_Name);
		for (int i = 0; i < this.m_Elements.Count; i++)
		{
			this.m_Elements[i].Load(this, i);
			if (this.m_Elements[i].GetActionState() == ScenarioElement.State.InProgress)
			{
				this.m_ActiveElements.Add(this.m_Elements[i]);
			}
		}
	}

	public static string NO_PARENTS = "NoParents";

	public ScenarioNode.State m_State;

	private List<ScenarioElement> m_Elements = new List<ScenarioElement>();

	private List<ScenarioElement> m_ActiveElements = new List<ScenarioElement>();

	public string m_Name = string.Empty;

	public string m_ParentNames = string.Empty;

	public List<ScenarioNode> m_Parents = new List<ScenarioNode>();

	public List<ScenarioNode> m_Childs = new List<ScenarioNode>();

	public Scenario m_Scenario;

	public bool m_Loop;

	public enum State
	{
		None,
		Inactive,
		Active,
		Completed
	}
}
