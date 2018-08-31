using System;
using UnityEngine;

public class StaticObject : MonoBehaviour
{
	private void Start()
	{
		StaticObjectsManager.Get().OnStaticObjectAdded(base.gameObject);
	}

	private void OnDestroy()
	{
		this.m_IsBeingDestroyed = true;
		StaticObjectsManager.Get().OnStaticObjectRemoved(base.gameObject);
	}

	[HideInInspector]
	public bool m_IsBeingDestroyed;
}
