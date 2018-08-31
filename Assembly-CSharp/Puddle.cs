using System;
using UnityEngine;

public class Puddle : LiquidSource
{
	protected override void Start()
	{
		base.Start();
		this.m_OrigPos = base.transform.position;
		this.m_RainPos = this.m_OrigPos;
		this.m_RainPos.y = this.m_MaxY;
	}

	protected override void Update()
	{
		base.Update();
		if (RainManager.Get().IsRain())
		{
			base.transform.position = base.transform.position + Vector3.up * this.m_Speed * Time.deltaTime;
			if (base.transform.position.y > this.m_RainPos.y)
			{
				base.transform.position = this.m_RainPos;
			}
		}
		else
		{
			base.transform.position = base.transform.position - Vector3.up * this.m_Speed * Time.deltaTime;
			if (base.transform.position.y < this.m_OrigPos.y)
			{
				base.transform.position = this.m_OrigPos;
			}
		}
	}

	private Vector3 m_OrigPos = Vector3.zero;

	private Vector3 m_RainPos = Vector3.zero;

	public float m_MaxY;

	public float m_Speed = 0.01f;
}
