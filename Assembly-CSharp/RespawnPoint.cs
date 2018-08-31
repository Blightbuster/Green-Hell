using System;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
	private void Start()
	{
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		DebugUtils.Assert(component, true);
		component.enabled = false;
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	private void OnEnable()
	{
		if (!MainLevel.Instance)
		{
			return;
		}
		RespawnManager.Get().RegisterRespawnPoint(this);
	}

	private void OnDisable()
	{
		if (!MainLevel.Instance)
		{
			return;
		}
		RespawnManager.Get().UnregisterRespawnPoint(this);
	}
}
