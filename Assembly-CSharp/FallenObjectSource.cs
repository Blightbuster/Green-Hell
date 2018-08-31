using System;
using UnityEngine;

public class FallenObjectSource : MonoBehaviour
{
	private void Start()
	{
		FallenObjectsManager.Get().OnFallenObjectSourceAdded(base.gameObject);
	}

	private void OnDestroy()
	{
		FallenObjectsManager.Get().OnFallenObjectSourceRemoved(base.gameObject);
	}
}
