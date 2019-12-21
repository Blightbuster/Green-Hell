using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggersManager : MonoBehaviour, ISaveLoad
{
	public static TriggersManager Get()
	{
		return TriggersManager.s_Instance;
	}

	private void Awake()
	{
		TriggersManager.s_Instance = this;
	}

	private void OnEnable()
	{
		base.hideFlags = HideFlags.HideAndDontSave;
	}

	public int GetTriggersCount()
	{
		return Trigger.s_AllTriggers.Count;
	}

	public HashSet<Trigger> GetActiveTriggers()
	{
		return Trigger.s_ActiveTriggers;
	}

	public int GetActiveTriggersCount()
	{
		return Trigger.s_ActiveTriggers.Count;
	}

	public bool IsTriggered(GameObject obj)
	{
		Trigger trigger = obj ? obj.GetComponent<Trigger>() : null;
		return trigger && trigger.WasTriggered();
	}

	public void ResetTrigger(GameObject obj)
	{
		Trigger trigger = obj ? obj.GetComponent<Trigger>() : null;
		if (trigger)
		{
			trigger.ResetTrigger();
		}
	}

	public void Save()
	{
		foreach (Trigger trigger in Trigger.s_AllTriggers)
		{
			trigger.Save();
		}
	}

	public void Load()
	{
		foreach (Trigger trigger in Trigger.s_AllTriggers)
		{
			trigger.Load();
		}
	}

	private static TriggersManager s_Instance;
}
