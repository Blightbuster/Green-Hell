using System;
using UnityEngine;

public class MeleeAttackPredictionData
{
	public static int s_NumSteps = 10;

	public Vector3 m_DamagerStart = Vector3.zero;

	public Vector3 m_DamagerEnd = Vector3.zero;

	public float m_NormalizedTime;
}
