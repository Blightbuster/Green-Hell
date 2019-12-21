using System;
using UnityEngine;

public class WaterCollider : MonoBehaviour
{
	private void Awake()
	{
		this.m_LiquidSource = base.gameObject.GetComponent<LiquidSource>();
	}

	private void OnDestroy()
	{
		WaterBoxManager.Get().OnExitWater(this);
	}

	public float m_LeechChance;

	public bool m_PlayerCanSwimIn = true;

	public LiquidSource m_LiquidSource;
}
