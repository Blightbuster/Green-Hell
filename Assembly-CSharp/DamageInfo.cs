using System;
using Enums;
using UnityEngine;

public class DamageInfo
{
	public GameObject m_Damager;

	public Item m_DamageItem;

	public float m_Damage;

	public Vector3 m_Position = Vector3.zero;

	public Vector3 m_Normal = Vector3.zero;

	public bool m_PlayDamageSound = true;

	public Vector3 m_HitDir = Vector3.zero;

	public DamageType m_DamageType;

	public int m_PoisonLevel;

	public bool m_Blocked;

	public bool m_CriticalHit;
}
