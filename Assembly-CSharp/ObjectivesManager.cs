using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivesManager : MonoBehaviour, ISaveLoad
{
	public List<Objective> GetObjectives()
	{
		return this.m_Objectives;
	}

	public static ObjectivesManager Get()
	{
		return ObjectivesManager.s_Instance;
	}

	private void Awake()
	{
		ObjectivesManager.s_Instance = this;
	}

	private void Start()
	{
		this.ParseScript();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse(ObjectivesManager.m_ScriptName, true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Objective")
			{
				Objective item = new Objective(key.GetVariable(0).SValue, key.GetVariable(1).SValue);
				this.m_Objectives.Add(item);
			}
		}
	}

	public void ActivateObjective(string obj_name, bool show_obj)
	{
		Objective objective = this.FindObjectiveByName(obj_name);
		if (objective == null)
		{
			DebugUtils.Assert("[ObjectivesManager:ActivateObjective] Can't find objective - " + obj_name, true, DebugUtils.AssertType.Info);
			return;
		}
		this.ActivateObjective(objective, show_obj);
	}

	private void ActivateObjective(Objective obj, bool show_obj)
	{
		if (obj == null)
		{
			DebugUtils.Assert("[ObjectivesManager:ActivateObjective] Can't activate objective!", true, DebugUtils.AssertType.Info);
			return;
		}
		obj.SetState(ObjectiveState.Active);
		this.OnObjectiveActivated(obj, show_obj);
	}

	public void DeactivateAllActiveObjectives()
	{
		while (this.m_ActiveObjectives.Count > 0)
		{
			this.DeactivateObjective(this.m_ActiveObjectives[0]);
		}
	}

	public void DeactivateObjective(string obj_name)
	{
		Objective obj = this.FindObjectiveByName(obj_name);
		this.DeactivateObjective(obj);
	}

	protected virtual void DeactivateObjective(Objective obj)
	{
		if (obj == null)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		obj.SetState(ObjectiveState.Completed);
		this.OnObjectiveCompleted(obj);
	}

	public void RegisterObserver(IObjectivesManagerObserver obs)
	{
		this.m_Observers.Add(obs);
	}

	public void UnRegisterObserver(IObjectivesManagerObserver obs)
	{
		this.m_Observers.Remove(obs);
	}

	private void OnObjectiveActivated(Objective obj, bool show_obj)
	{
		if (this.m_ActiveObjectives.Contains(obj))
		{
			return;
		}
		if (show_obj && SaveGame.m_State == SaveGame.State.None)
		{
			for (int i = 0; i < this.m_Observers.Count; i++)
			{
				this.m_Observers[i].OnObjectiveActivated(obj);
			}
		}
		this.m_ActiveObjectives.Add(obj);
	}

	protected void OnObjectiveCompleted(Objective obj)
	{
		for (int i = 0; i < this.m_Observers.Count; i++)
		{
			this.m_Observers[i].OnObjectiveCompleted(obj);
		}
		if (!this.m_CompletedObjectives.Contains(obj))
		{
			this.m_CompletedObjectives.Add(obj);
		}
		this.m_ActiveObjectives.Remove(obj);
	}

	private void OnObjectiveRemoved(Objective obj)
	{
		for (int i = 0; i < this.m_Observers.Count; i++)
		{
			this.m_Observers[i].OnObjectiveRemoved(obj);
		}
	}

	private Objective FindObjectiveByName(string name)
	{
		for (int i = 0; i < this.m_Objectives.Count; i++)
		{
			if (this.m_Objectives[i].m_Name == name)
			{
				return this.m_Objectives[i];
			}
		}
		return null;
	}

	public bool IsObjectiveCompleted(string name)
	{
		for (int i = 0; i < this.m_Objectives.Count; i++)
		{
			if (this.m_Objectives[i].m_Name == name)
			{
				return this.m_Objectives[i].IsCompleted();
			}
		}
		return false;
	}

	public virtual void Save()
	{
		SaveGame.SaveVal("ActiveObjectivesCount", this.m_ActiveObjectives.Count);
		for (int i = 0; i < this.m_ActiveObjectives.Count; i++)
		{
			SaveGame.SaveVal("ActiveObjective" + i.ToString(), this.m_ActiveObjectives[i].m_Name);
		}
		SaveGame.SaveVal("CompletedObjectivesCount", this.m_CompletedObjectives.Count);
		for (int j = 0; j < this.m_CompletedObjectives.Count; j++)
		{
			SaveGame.SaveVal("CompletedObjective" + j.ToString(), this.m_CompletedObjectives[j].m_Name);
		}
	}

	public virtual void Load()
	{
		this.m_ActiveObjectives.Clear();
		int num = SaveGame.LoadIVal("ActiveObjectivesCount");
		for (int i = 0; i < num; i++)
		{
			this.ActivateObjective(SaveGame.LoadSVal("ActiveObjective" + i.ToString()), true);
		}
		this.m_CompletedObjectives.Clear();
		int num2 = SaveGame.LoadIVal("CompletedObjectivesCount");
		for (int j = 0; j < num2; j++)
		{
			this.DeactivateObjective(SaveGame.LoadSVal("CompletedObjective" + j.ToString()));
		}
	}

	protected List<Objective> m_Objectives = new List<Objective>();

	public List<Objective> m_ActiveObjectives = new List<Objective>();

	public List<Objective> m_CompletedObjectives = new List<Objective>();

	private List<IObjectivesManagerObserver> m_Observers = new List<IObjectivesManagerObserver>();

	private static string m_ScriptName = "Objectives/Objectives.txt";

	private static ObjectivesManager s_Instance = null;
}
