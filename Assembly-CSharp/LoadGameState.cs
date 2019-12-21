using System;

public enum LoadGameState
{
	None,
	PreloadScheduled,
	PreloadCompleted,
	ScenePreparation,
	FullLoadScheduled,
	FullLoadWaitingForScenario,
	FullLoadCompleted
}
