using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class PlayerPlannerModule : PlayerModule, IEventsReceiver
{
	private void Start()
	{
		this.ParseScript();
		EventsManager.RegisterReceiver(this);
	}

	public override void OnDestroy()
	{
		EventsManager.UnregisterReceiver(this);
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_PlannerTasks.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Task")
			{
				Type type = Type.GetType("PlannerTask" + key.GetVariable(0).SValue);
				if (type == null)
				{
					DebugUtils.Assert("PlayerPlannerModule::ParseScript unknown type " + key.GetVariable(0).SValue, true, DebugUtils.AssertType.Info);
				}
				else
				{
					PlannerTask plannerTask = Activator.CreateInstance(type) as PlannerTask;
					plannerTask.Parse(key);
					this.m_AllTasks.Add(plannerTask);
				}
			}
		}
	}

	public void AddPlannedTask(PlannerTask task)
	{
		this.m_PlannedTasks.Add(task);
		PlannerTab.Get().OnPlannedTaskAdded();
	}

	public void DeletePlannedTask(PlannerTask task)
	{
		this.m_PlannedTasks.Remove(task);
		PlannerTab.Get().OnPlannedTaskDeleted();
	}

	public void OnEvent(Enums.Event event_type, int val, int data)
	{
		if (event_type == Enums.Event.Build)
		{
			for (int i = 0; i < this.m_AllTasks.Count; i++)
			{
				PlannerTask plannerTask = this.m_AllTasks[i];
				if (this.m_PlannedTasks.Contains(plannerTask))
				{
					if (plannerTask.OnBuild((ItemID)data, true))
					{
						this.m_AllTasks[i].m_Fullfiled = true;
					}
				}
				else
				{
					plannerTask.OnBuild((ItemID)data, false);
				}
			}
			return;
		}
		if (event_type == Enums.Event.Eat)
		{
			for (int j = 0; j < this.m_AllTasks.Count; j++)
			{
				PlannerTask plannerTask2 = this.m_AllTasks[j];
				if (this.m_PlannedTasks.Contains(plannerTask2))
				{
					if (plannerTask2.OnEat((ItemID)data, true))
					{
						this.m_AllTasks[j].m_Fullfiled = true;
					}
				}
				else
				{
					plannerTask2.OnEat((ItemID)data, false);
				}
			}
			return;
		}
		if (event_type == Enums.Event.IgniteFire)
		{
			for (int k = 0; k < this.m_AllTasks.Count; k++)
			{
				PlannerTask plannerTask3 = this.m_AllTasks[k];
				if (this.m_PlannedTasks.Contains(plannerTask3))
				{
					if (plannerTask3.OnMakeFire(true))
					{
						this.m_AllTasks[k].m_Fullfiled = true;
					}
				}
				else
				{
					plannerTask3.OnMakeFire(false);
				}
			}
			return;
		}
		if (event_type == Enums.Event.HealWound)
		{
			for (int l = 0; l < this.m_AllTasks.Count; l++)
			{
				PlannerTask plannerTask4 = this.m_AllTasks[l];
				if (this.m_PlannedTasks.Contains(plannerTask4))
				{
					if (plannerTask4.OnHealWound(true))
					{
						this.m_AllTasks[l].m_Fullfiled = true;
					}
				}
				else
				{
					plannerTask4.OnHealWound(false);
				}
			}
		}
	}

	public void OnEvent(Enums.Event event_type, int val, int data, int data2)
	{
		if (event_type == Enums.Event.Sleep)
		{
			for (int i = 0; i < this.m_AllTasks.Count; i++)
			{
				PlannerTask plannerTask = this.m_AllTasks[i];
				if (this.m_PlannedTasks.Contains(plannerTask))
				{
					if (plannerTask.OnSleep(data2 > 0, true))
					{
						this.m_AllTasks[i].m_Fullfiled = true;
					}
				}
				else
				{
					plannerTask.OnSleep(data2 > 0, false);
				}
			}
		}
	}

	public void OnEvent(Enums.Event event_type, float val, int data)
	{
	}

	public void OnEvent(Enums.Event event_type, string val, int data)
	{
	}

	public void OnEvent(Enums.Event event_type, bool val, int data)
	{
	}

	[HideInInspector]
	public List<PlannerTask> m_AllTasks = new List<PlannerTask>();

	[HideInInspector]
	public List<PlannerTask> m_PlannedTasks = new List<PlannerTask>();
}
