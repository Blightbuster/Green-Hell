using System;
using UnityEngine;

public class PhysicsDampener : MonoBehaviour
{
	private void Awake()
	{
		this.m_Rigidbody = new CachedComponent<Rigidbody>(base.gameObject);
	}

	private void FixedUpdate()
	{
		if (this.m_Rigidbody.Get() == null)
		{
			return;
		}
		Vector3 velocity = this.m_Rigidbody.Get().velocity;
		if (this.m_MaxYUpVelocity >= 0f && velocity.y > this.m_MaxYUpVelocity)
		{
			velocity.y = this.m_MaxYUpVelocity;
			this.m_Rigidbody.Get().velocity = velocity;
		}
		if (this.m_MaxVelocityMagnitude >= 0f && velocity.magnitude > this.m_MaxVelocityMagnitude)
		{
			this.m_Rigidbody.Get().velocity = velocity.normalized * this.m_MaxVelocityMagnitude;
		}
	}

	public float m_MaxYUpVelocity = -1f;

	public float m_MaxVelocityMagnitude = -1f;

	private CachedComponent<Rigidbody> m_Rigidbody;
}
