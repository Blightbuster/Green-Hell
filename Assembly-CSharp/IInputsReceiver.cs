using System;

public interface IInputsReceiver
{
	void OnInputAction(InputsManager.InputAction action);

	bool CanReceiveAction();
}
