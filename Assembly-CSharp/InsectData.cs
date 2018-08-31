using System;
using Enums;
using UnityEngine;

public class InsectData
{
	public GameObject m_Insect;

	public BIWoundSlot m_Slot;

	public Hand m_Hand = Hand.Right;

	public Vector3 m_FlyAwayDirection = Vector3.zero;

	public float m_FlyAwaySpeed;

	public bool m_FlyAway;
}
