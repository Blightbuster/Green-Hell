using System;
using UnityEngine;

public class TrapTrigger : MonoBehaviour, ITriggerThrough
{
	private void Awake()
	{
		base.gameObject.GetComponent<Collider>().isTrigger = true;
		base.gameObject.AddComponent<Rigidbody>().isKinematic = true;
		this.m_Owner = base.gameObject.GetComponentInParent<ITrapTriggerOwner>();
	}

	private void OnTriggerEnter(Collider other)
	{
		this.m_Owner.OnEnterTrigger(other.gameObject);
	}

	private ITrapTriggerOwner m_Owner;
}
