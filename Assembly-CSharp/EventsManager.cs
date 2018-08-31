using System;
using System.Collections.Generic;
using Enums;

public static class EventsManager
{
	public static void RegisterReceiver(IEventsReceiver receiver)
	{
		EventsManager.m_ReceiversToAdd.Add(receiver);
	}

	public static void UnregisterReceiver(IEventsReceiver receiver)
	{
		EventsManager.m_ReceiversToRemove.Remove(receiver);
	}

	private static void CheckReceivers()
	{
		for (int i = 0; i < EventsManager.m_ReceiversToAdd.Count; i++)
		{
			EventsManager.m_Receivers.Add(EventsManager.m_ReceiversToAdd[i]);
		}
		for (int j = 0; j < EventsManager.m_ReceiversToRemove.Count; j++)
		{
			EventsManager.m_Receivers.Remove(EventsManager.m_ReceiversToRemove[j]);
		}
		EventsManager.m_ReceiversToAdd.Clear();
		EventsManager.m_ReceiversToRemove.Clear();
	}

	public static void OnEvent(Event event_type, int val)
	{
		EventsManager.OnEvent(event_type, val, -1);
	}

	public static void OnEvent(Event event_type, int val, int data)
	{
		EventsManager.CheckReceivers();
		foreach (IEventsReceiver eventsReceiver in EventsManager.m_Receivers)
		{
			eventsReceiver.OnEvent(event_type, val, data);
		}
		EventsManager.CheckReceivers();
	}

	public static void OnEvent(Event event_type, int val, int data, int data2)
	{
		EventsManager.CheckReceivers();
		foreach (IEventsReceiver eventsReceiver in EventsManager.m_Receivers)
		{
			eventsReceiver.OnEvent(event_type, val, data, data2);
		}
		EventsManager.CheckReceivers();
	}

	public static void OnEvent(Event event_type, float val)
	{
		EventsManager.OnEvent(event_type, val, -1);
	}

	public static void OnEvent(Event event_type, float val, int data)
	{
		EventsManager.CheckReceivers();
		foreach (IEventsReceiver eventsReceiver in EventsManager.m_Receivers)
		{
			eventsReceiver.OnEvent(event_type, val, data);
		}
		EventsManager.CheckReceivers();
	}

	public static void OnEvent(Event event_type, string val)
	{
		EventsManager.OnEvent(event_type, val, -1);
	}

	public static void OnEvent(Event event_type, string val, int data)
	{
		EventsManager.CheckReceivers();
		foreach (IEventsReceiver eventsReceiver in EventsManager.m_Receivers)
		{
			eventsReceiver.OnEvent(event_type, val, data);
		}
		EventsManager.CheckReceivers();
	}

	public static void OnEvent(Event event_type, bool val)
	{
		EventsManager.OnEvent(event_type, val, -1);
	}

	public static void OnEvent(Event event_type, bool val, int data = -1)
	{
		EventsManager.CheckReceivers();
		foreach (IEventsReceiver eventsReceiver in EventsManager.m_Receivers)
		{
			eventsReceiver.OnEvent(event_type, val, data);
		}
		EventsManager.CheckReceivers();
	}

	public static List<IEventsReceiver> m_Receivers = new List<IEventsReceiver>();

	public static List<IEventsReceiver> m_ReceiversToAdd = new List<IEventsReceiver>();

	public static List<IEventsReceiver> m_ReceiversToRemove = new List<IEventsReceiver>();
}
