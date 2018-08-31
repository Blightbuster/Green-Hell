using System;
using UnityEngine;

public class BSPlayerTrigger : MonoBehaviour
{
	public void SetBalanceSystem(BalanceSystem system)
	{
		this.m_BalanceSystem = system;
	}

	private void OnTriggerEnter(Collider other)
	{
		this.m_BalanceSystem.OnObjectTriggerEnter(other.gameObject);
	}

	private void OnTriggerExit(Collider other)
	{
		this.m_BalanceSystem.OnObjectTriggerExit(other.gameObject);
	}

	private BalanceSystem m_BalanceSystem;
}
