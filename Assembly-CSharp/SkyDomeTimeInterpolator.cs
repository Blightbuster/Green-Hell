using System;
using CJTools;
using UnityEngine;

public class SkyDomeTimeInterpolator : MonoBehaviour
{
	private void Start()
	{
		this.m_TODTime.m_InterpolateTime = true;
		this.m_TODSky.Cycle.Hour = this.m_TimeA;
	}

	private void Update()
	{
		Vector3 a = this.m_ObjectA.transform.position;
		a -= this.m_ObjectB.transform.position;
		float num = (CJTools.Math.ProjectPointOnLine(this.m_ObjectA.transform.position, a.normalized, Player.Get().transform.position) - this.m_ObjectA.transform.position).magnitude / a.magnitude;
		num = Mathf.Clamp01(num);
		float num2 = this.m_TimeA + (this.m_TimeB - this.m_TimeA) * num;
		if (this.m_OnlyForwardUpdate && num2 < this.m_TODSky.Cycle.Hour)
		{
			num2 = this.m_TODSky.Cycle.Hour;
		}
		this.m_TODSky.Cycle.Hour = num2;
	}

	public TOD_Time m_TODTime;

	public TOD_Sky m_TODSky;

	public float m_TimeA = 1f;

	public float m_TimeB = 10f;

	public GameObject m_ObjectA;

	public GameObject m_ObjectB;

	public bool m_OnlyForwardUpdate;
}
