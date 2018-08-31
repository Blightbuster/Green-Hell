using System;
using UnityEngine;

public class LeavesPusherData
{
	public LeavesPusherData(GameObject go, Quaternion rot, Vector3 rotation_axis, float enter_radius_size, PushLeaves push_leaves, float def_shake_mul, float def_shake_time_mul)
	{
		this.m_Object = go;
		this.m_OriginalQuat = rot;
		this.m_RotationAxis = rotation_axis;
		this.m_EnterRadiusSize = enter_radius_size;
		this.m_PushLeaves = push_leaves;
		this.m_DefShake = def_shake_mul;
		this.m_DefShakeTime = def_shake_time_mul;
	}

	public GameObject m_Object;

	public Quaternion m_OriginalQuat;

	public Vector3 m_RotationAxis = Vector3.left;

	public float m_Angle;

	public float m_ShaderPropertyShake = 1f;

	public float m_ShaderPropertyShakeTime = 1f;

	public Vector3 m_HitAxis = Vector3.left;

	public float m_HitTime = -1f;

	public float m_EnterRadiusSize = -1f;

	public PushLeaves m_PushLeaves;

	public float m_DefShake;

	public float m_DefShakeTime;
}
