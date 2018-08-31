using System;
using UnityEngine;

public class SensorDamage : SensorBase
{
	protected override void Awake()
	{
		base.Awake();
		this.m_DamageInfo = new DamageInfo();
		this.m_DamageInfo.m_Damager = base.gameObject;
		this.SetupInterval();
	}

	private void SetupInterval()
	{
		this.m_Interval = UnityEngine.Random.Range(this.m_DamageIntervalMin, this.m_DamageIntervalMax);
	}

	protected override void Update()
	{
		if (!this.m_IsInside)
		{
			return;
		}
		if (Time.time - this.m_LastDamageTime < this.m_Interval)
		{
			return;
		}
		this.m_DamageInfo.m_Damage = UnityEngine.Random.Range(this.m_DamageMin, this.m_DamageMax);
		Player.Get().TakeDamage(this.m_DamageInfo);
		this.m_LastDamageTime = Time.time;
		this.SetupInterval();
	}

	public float m_DamageIntervalMin;

	public float m_DamageIntervalMax;

	public float m_DamageMin;

	public float m_DamageMax;

	private float m_LastDamageTime;

	private float m_Interval;

	private DamageInfo m_DamageInfo;
}
