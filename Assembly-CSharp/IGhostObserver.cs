using System;

public interface IGhostObserver
{
	void OnCreateConstruction(ConstructionGhost ghost, Item created_item);
}
