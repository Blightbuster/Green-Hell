using System;
using UnityEngine;

public class SensorMoveObject : SensorBase
{
	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		base.enabled = true;
	}

	protected override void Update()
	{
		base.Update();
		Vector3 vector = this.m_Target.transform.position - this.m_Object.transform.position;
		float num = this.m_Speed * Time.deltaTime;
		if (vector.magnitude < num)
		{
			this.m_Object.transform.position = this.m_Target.transform.position;
			this.m_Activated = false;
			base.enabled = false;
			base.gameObject.SetActive(false);
			return;
		}
		this.m_Object.transform.position = this.m_Object.transform.position + vector.normalized * num;
	}

	public GameObject m_Object;

	public GameObject m_Target;

	public float m_Speed = 1f;

	private bool m_Activated;
}
