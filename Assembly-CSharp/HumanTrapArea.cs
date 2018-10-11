using System;
using UnityEngine;

public class HumanTrapArea : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		this.m_Trap.OnEnter(other.gameObject);
	}

	public BoxCollider m_Area;

	public HumanTrap m_Trap;
}
