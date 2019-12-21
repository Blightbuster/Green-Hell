using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class RainCutter : MonoBehaviour
{
	private void Awake()
	{
		this.m_Collider = base.gameObject.GetComponent<BoxCollider>();
		this.m_Collider.isTrigger = true;
		this.m_ScaleInitial = base.transform.localScale;
		this.m_Extent = base.transform.localScale.magnitude * 0.5f;
		RainCutter.s_AllRainCutters.Add(this);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		RainCutter.s_AllRainCutters.Remove(this);
	}

	public bool CheckInside(Vector3 point)
	{
		if (this.m_Extent < base.transform.position.Distance(point))
		{
			this.m_IsInside = false;
		}
		else
		{
			Vector3 vector = base.transform.InverseTransformPoint(point);
			this.m_IsInside = (Mathf.Abs(vector.x) < 0.5f && Mathf.Abs(vector.y) < 0.5f && Mathf.Abs(vector.z) < 0.5f);
			if (this.m_IsInside)
			{
				if (this.m_ScaleState != RainCutter.EScaleState.Enlarged)
				{
					base.transform.localScale = this.m_ScaleInitial * 1.02f;
					this.m_ScaleState = RainCutter.EScaleState.Enlarged;
				}
			}
			else if (this.m_ScaleState != RainCutter.EScaleState.Shrunken)
			{
				base.transform.localScale = this.m_ScaleInitial * 0.98f;
				this.m_ScaleState = RainCutter.EScaleState.Shrunken;
			}
		}
		this.m_LastCheckTime = Time.time;
		return this.m_IsInside;
	}

	public float GetInsideValue(Vector3 point)
	{
		float num = base.transform.position.Distance(point);
		if (num > this.m_Extent)
		{
			if (num > this.m_Extent + this.m_DistStartInterpolatingInside)
			{
				return 0f;
			}
			return CJTools.Math.GetProportionalClamp(0f, 0.99f, num, this.m_Extent + this.m_DistStartInterpolatingInside, this.m_Extent);
		}
		else
		{
			Vector3 vector = base.transform.InverseTransformPoint(point);
			if (Mathf.Abs(vector.x) >= 0.5f || Mathf.Abs(vector.y) >= 0.5f || Mathf.Abs(vector.z) >= 0.5f)
			{
				return 0.99f;
			}
			return 1f;
		}
	}

	private BoxCollider m_Collider;

	private float m_Extent;

	public RainCutterType m_Type;

	public float m_DistStartInterpolatingInside = 1f;

	private RainCutter.EScaleState m_ScaleState;

	private Vector3 m_ScaleInitial;

	private float m_LastCheckTime;

	private bool m_IsInside;

	public static List<RainCutter> s_AllRainCutters = new List<RainCutter>();

	private enum EScaleState
	{
		Normal,
		Enlarged,
		Shrunken
	}
}
