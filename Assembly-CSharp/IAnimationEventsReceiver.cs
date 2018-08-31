using System;
using Enums;

public interface IAnimationEventsReceiver
{
	void OnAnimEvent(AnimEventID id);

	bool IsActive();

	bool ForceReceiveAnimEvent();
}
