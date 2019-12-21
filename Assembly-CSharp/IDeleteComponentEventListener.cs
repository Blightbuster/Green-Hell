using System;
using UnityEngine;

public interface IDeleteComponentEventListener
{
	void OnComponentDestroy(Component component);
}
