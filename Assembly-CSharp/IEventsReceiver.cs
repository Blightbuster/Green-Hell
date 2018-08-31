using System;
using Enums;

public interface IEventsReceiver
{
	void OnEvent(Event event_type, int val, int data);

	void OnEvent(Event event_type, float val, int data);

	void OnEvent(Event event_type, string val, int data);

	void OnEvent(Event event_type, bool val, int data);

	void OnEvent(Event event_type, int val, int data, int data2);
}
