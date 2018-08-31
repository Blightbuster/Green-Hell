using System;
using UnityEngine;

public class RainCutter : MonoBehaviour
{
	private void Awake()
	{
		this.m_Collider = base.gameObject.GetComponent<BoxCollider>();
		this.m_Collider.isTrigger = true;
	}

	private void OnEnable()
	{
		if (RainManager.Get())
		{
			RainManager.Get().RegisterRainCutter(this);
		}
		else
		{
			this.m_RequestRegister = true;
		}
	}

	private void OnDisable()
	{
		RainManager.Get().UnregisterRainCutter(this);
		this.m_RequestRegister = false;
	}

	private void Update()
	{
		if (this.m_RequestRegister && RainManager.Get())
		{
			RainManager.Get().RegisterRainCutter(this);
			this.m_RequestRegister = false;
		}
	}

	public bool IsInside(Vector3 point)
	{
		return this.m_Collider.bounds.Contains(point);
	}

	private BoxCollider m_Collider;

	private bool m_RequestRegister;
}
