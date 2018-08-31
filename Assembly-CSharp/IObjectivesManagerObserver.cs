using System;

public interface IObjectivesManagerObserver
{
	void OnObjectiveActivated(Objective obj);

	void OnObjectiveCompleted(Objective obj);

	void OnObjectiveRemoved(Objective obj);
}
