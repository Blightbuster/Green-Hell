using System;
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

	public int GetActiveTriggersCount()
	{
		return Trigger.s_ActiveTriggers.Count;
	}

	public Trigger GetTrigger(int i, bool only_active = false)
	{
		if (i >= this.GetActiveTriggersCount())
		{
			return null;
		}
		return Trigger.s_ActiveTriggers[i];
	}

	public bool IsTriggered(GameObject obj)
	{
		Trigger trigger = (!obj) ? null : obj.GetComponent<Trigger>();
		return trigger && trigger.WasTriggered();
	}

	public void ResetTrigger(GameObject obj)
	{
		Trigger trigger = (!obj) ? null : obj.GetComponent<Trigger>();
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
