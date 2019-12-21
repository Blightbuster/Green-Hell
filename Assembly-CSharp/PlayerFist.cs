using System;
using AIs;
using UnityEngine;

public class PlayerFist : MonoBehaviour
{
	private void Awake()
	{
		SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = 0.4f;
		sphereCollider.center = Vector3.zero;
		sphereCollider.isTrigger = true;
		sphereCollider.enabled = false;
		this.m_HandCollider = sphereCollider;
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
	}

	private void OnTriggerEnter(Collider other)
	{
		AI component = other.gameObject.GetComponent<AI>();
		if (component == null || component.IsStringray())
		{
			return;
		}
		FistFightController.Get().GiveDamage(component);
	}

	public Collider m_HandCollider;
}
