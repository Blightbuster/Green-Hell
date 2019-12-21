using System;
using System.Collections.Generic;
using UnityEngine;

public class PITimeCommander : PICommander
{
	private void OnEnable()
	{
		this.m_StartTime = Time.time;
	}

	private void OnDisable()
	{
		this.ResetParams();
	}

	private void Update()
	{
		float weight;
		if (Time.time > this.m_DisableWithFadeStartTime)
		{
			float time = (Time.time - this.m_DisableWithFadeStartTime) / this.m_DisableWithFadeDuration;
			weight = this.m_BackwardWeightCurve.Evaluate(time);
		}
		else
		{
			float time2 = (Time.time - this.m_StartTime) / this.m_Duration;
			weight = this.m_ForwardWeightCurve.Evaluate(time2);
		}
		for (int i = 0; i < this.m_Interpolators.Count; i++)
		{
			this.m_Interpolators[i].SetWeight(weight);
		}
	}

	protected override void ResetParams()
	{
		base.ResetParams();
		for (int i = 0; i < this.m_Interpolators.Count; i++)
		{
			if (this.m_Interpolators[i] != null)
			{
				this.m_Interpolators[i].SetWeight(0f);
			}
		}
	}

	private float m_StartTime = float.MinValue;

	public float m_Duration = 1f;

	public AnimationCurve m_ForwardWeightCurve = new AnimationCurve();

	public AnimationCurve m_BackwardWeightCurve = new AnimationCurve();

	public List<ParametersInterpolator> m_Interpolators = new List<ParametersInterpolator>();
}
