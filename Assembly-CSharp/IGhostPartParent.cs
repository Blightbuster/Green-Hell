using System;
using UnityEngine;

public interface IGhostPartParent
{
	Material GetActiveMaterial();

	Material GetHighlightedMaterial();

	void OnGhostFulfill(bool from_save);
}
