using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class StatsManager : MonoBehaviour, IEventsReceiver, ISaveLoad
{
	public static StatsManager Get()
	{
		return StatsManager.s_Instance;
	}

	private void Awake()
	{
		StatsManager.s_Instance = this;
		for (int i = 0; i < 18; i++)
		{
			this.m_Stats[i] = new CJVariable();
		}
		EventsManager.RegisterReceiver(this);
	}

	private void Reset()
	{
		for (int i = 0; i < 18; i++)
		{
			this.m_Stats[i].Reset();
		}
	}

	public void Save()
	{
		CJVariable cjvariable = null;
		for (int i = 0; i < 18; i++)
		{
			Enums.Event key = (Enums.Event)i;
			if (this.m_Stats.TryGetValue((int)key, out cjvariable))
			{
				switch (cjvariable.GetVariableType())
				{
				case CJVariable.TYPE.String:
					SaveGame.SaveVal("Stat" + key.ToString(), cjvariable.SValue);
					break;
				case CJVariable.TYPE.Int:
					SaveGame.SaveVal("Stat" + key.ToString(), cjvariable.IValue);
					break;
				case CJVariable.TYPE.Float:
					SaveGame.SaveVal("Stat" + key.ToString(), cjvariable.FValue);
					break;
				case CJVariable.TYPE.Bool:
					SaveGame.SaveVal("Stat" + key.ToString(), cjvariable.BValue);
					break;
				}
			}
		}
	}

	public void Load()
	{
		this.Reset();
		int ivalue = 0;
		float fvalue = 0f;
		string empty = string.Empty;
		bool bvalue = false;
		for (int i = 0; i < 18; i++)
		{
			Enums.Event stat_type = (Enums.Event)i;
			CJVariable statistic = this.GetStatistic(stat_type);
			if (statistic != null)
			{
				if (SaveGame.LoadVal("Stat" + stat_type.ToString(), out fvalue, false))
				{
					statistic.FValue = fvalue;
				}
				else if (SaveGame.LoadVal("Stat" + stat_type.ToString(), out ivalue, false))
				{
					statistic.IValue = ivalue;
				}
				else if (SaveGame.LoadVal("Stat" + stat_type.ToString(), out empty, false))
				{
					statistic.SValue = empty;
				}
				else if (SaveGame.LoadVal("Stat" + stat_type.ToString(), out bvalue, false))
				{
					statistic.BValue = bvalue;
				}
			}
		}
		this.m_PrevDayCheck = MainLevel.Instance.m_TODSky.Cycle.Day;
	}

	public bool StatisticGreater(string stat_name, string level)
	{
		Enums.Event stat_type = (Enums.Event)Enum.Parse(typeof(Enums.Event), stat_name);
		CJVariable statistic = this.GetStatistic(stat_type);
		if (statistic == null)
		{
			return false;
		}
		CJVariable.TYPE variableType = statistic.GetVariableType();
		if (variableType != CJVariable.TYPE.Int)
		{
			if (variableType == CJVariable.TYPE.Float)
			{
				float num = 0f;
				if (float.TryParse(level, out num))
				{
					return statistic.FValue > num;
				}
			}
		}
		else
		{
			int num2 = 0;
			if (int.TryParse(level, out num2))
			{
				return statistic.IValue > num2;
			}
		}
		return false;
	}

	public bool StatisticEqual(string stat_name, string level)
	{
		Enums.Event stat_type = (Enums.Event)Enum.Parse(typeof(Enums.Event), stat_name);
		CJVariable statistic = this.GetStatistic(stat_type);
		if (statistic == null)
		{
			return false;
		}
		CJVariable.TYPE variableType = statistic.GetVariableType();
		if (variableType != CJVariable.TYPE.Int)
		{
			if (variableType == CJVariable.TYPE.Float)
			{
				float num = 0f;
				if (float.TryParse(level, out num))
				{
					return statistic.FValue == num;
				}
			}
		}
		else
		{
			int num2 = 0;
			if (int.TryParse(level, out num2))
			{
				return statistic.IValue == num2;
			}
		}
		return false;
	}

	public bool StatisticLess(string stat_name, string level)
	{
		Enums.Event stat_type = (Enums.Event)Enum.Parse(typeof(Enums.Event), stat_name);
		CJVariable statistic = this.GetStatistic(stat_type);
		if (statistic == null)
		{
			return false;
		}
		CJVariable.TYPE variableType = statistic.GetVariableType();
		if (variableType != CJVariable.TYPE.Int)
		{
			if (variableType == CJVariable.TYPE.Float)
			{
				float num = 0f;
				if (float.TryParse(level, out num))
				{
					return statistic.FValue < num;
				}
			}
		}
		else
		{
			int num2 = 0;
			if (int.TryParse(level, out num2))
			{
				return statistic.IValue < num2;
			}
		}
		return false;
	}

	public CJVariable GetStatistic(Enums.Event stat_type)
	{
		CJVariable cjvariable = null;
		this.m_Stats.TryGetValue((int)stat_type, out cjvariable);
		DebugUtils.Assert(cjvariable != null, true);
		return cjvariable;
	}

	public void OnEvent(Enums.Event event_type, float val, int data)
	{
		CJVariable statistic = this.GetStatistic(event_type);
		statistic.FValue += val;
	}

	public void OnEvent(Enums.Event event_type, int val, int data)
	{
		CJVariable statistic = this.GetStatistic(event_type);
		statistic.IValue += val;
	}

	public void OnEvent(Enums.Event event_type, int val, int data, int data2)
	{
		CJVariable statistic = this.GetStatistic(event_type);
		statistic.IValue += val;
	}

	public void OnEvent(Enums.Event event_type, bool val, int data)
	{
		CJVariable statistic = this.GetStatistic(event_type);
		statistic.BValue = val;
	}

	public void OnEvent(Enums.Event event_type, string val, int data)
	{
		CJVariable statistic = this.GetStatistic(event_type);
		statistic.SValue = val;
	}

	private void Update()
	{
		if (this.m_PrevDayCheck >= 0 && MainLevel.Instance.m_TODSky.Cycle.Day > this.m_PrevDayCheck)
		{
			EventsManager.OnEvent(Enums.Event.DaysSurvived, 1);
		}
		this.m_PrevDayCheck = MainLevel.Instance.m_TODSky.Cycle.Day;
	}

	private Dictionary<int, CJVariable> m_Stats = new Dictionary<int, CJVariable>();

	private int m_PrevDayCheck = -1;

	private static StatsManager s_Instance;
}
