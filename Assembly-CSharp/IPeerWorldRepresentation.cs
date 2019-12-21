using System;
using UnityEngine;

public interface IPeerWorldRepresentation
{
	Vector3 GetWorldPosition();

	bool IsReplicated();
}
