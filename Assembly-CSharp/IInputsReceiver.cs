using System;

public interface IInputsReceiver
{
	void OnInputAction(InputActionData action_data);

	bool CanReceiveAction();

	bool CanReceiveActionPaused();
}
