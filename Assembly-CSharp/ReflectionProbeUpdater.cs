using System;
using System.Collections;
using UnityEngine;

public class ReflectionProbeUpdater : MonoBehaviour, ICaveSensorObservable
{
	private void OnEnable()
	{
		CaveSensor.s_ICaveSensorObservables.Add(this);
	}

	private void OnDisable()
	{
		CaveSensor.s_ICaveSensorObservables.Remove(this);
	}

	private void Start()
	{
		this.m_ReflectionProbe = base.gameObject.GetComponent<ReflectionProbe>();
		DebugUtils.Assert(this.m_ReflectionProbe, true);
		if (this.m_ReflectionProbe != null)
		{
			this.m_ReflectionProbe.RenderProbe();
			base.StartCoroutine(this.UpdateReflectionProbe(this.m_UpdateInterval));
		}
		this.m_DefaultIntensity = this.m_ReflectionProbe.intensity;
	}

	public IEnumerator UpdateReflectionProbe(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (CaveSensor.s_NumSensorsInside != 0)
		{
			this.m_ReflectionProbe.intensity = 0f;
		}
		else
		{
			this.m_ReflectionProbe.intensity = this.m_DefaultIntensity;
		}
		this.m_ReflectionProbe.RenderProbe();
		base.StartCoroutine(this.UpdateReflectionProbe(this.m_UpdateInterval));
		yield break;
	}

	public void RenderProbe()
	{
		if (CaveSensor.s_NumSensorsInside != 0)
		{
			this.m_ReflectionProbe.intensity = 0f;
		}
		else
		{
			this.m_ReflectionProbe.intensity = this.m_DefaultIntensity;
		}
		if (this.m_ReflectionProbe != null)
		{
			this.m_ReflectionProbe.RenderProbe();
		}
	}

	public void OnUpdateObservable()
	{
		this.RenderProbe();
	}

	private ReflectionProbe m_ReflectionProbe;

	public float m_UpdateInterval = 10f;

	private float m_DefaultIntensity;
}
