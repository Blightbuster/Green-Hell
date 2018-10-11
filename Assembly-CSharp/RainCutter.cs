using System;
using UnityEngine;

public class RainCutter : MonoBehaviour
{
	private void Awake()
	{
		this.m_Collider = base.gameObject.GetComponent<BoxCollider>();
		this.m_Collider.isTrigger = true;
	}

	private void OnDestroy()
	{
		RainManager.Get().UnregisterRainCutter(this);
	}

	private void Update()
	{
		if (RainManager.Get())
		{
			RainManager.Get().RegisterRainCutter(this);
			this.m_Bounds = new Bounds(this.m_Collider.bounds.center, this.m_Collider.bounds.size);
			base.enabled = false;
		}
	}

	public bool IsInside(Vector3 point)
	{
		return this.m_Bounds.Contains(point);
	}

	private BoxCollider m_Collider;

	private Bounds m_Bounds;
}
