using System;
using UnityEngine;

public class TinyPhysicsObject : MonoBehaviour
{
	private void Awake()
	{
		this.m_CreationTime = Time.time;
	}

	private void Update()
	{
		if (Time.time - this.m_CreationTime > 10f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private float m_CreationTime = float.MinValue;

	private const float m_LifeTime = 10f;
}
