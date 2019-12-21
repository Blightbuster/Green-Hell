using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class ObjectFollowPath : MonoBehaviour
{
	private void Start()
	{
		this.m_VPath.Add(base.transform.position);
		foreach (Transform transform in this.m_Path)
		{
			this.m_VPath.Add(transform.position);
		}
		this.m_PositionObject = new GameObject();
		this.m_PositionObject.transform.rotation = Quaternion.LookRotation((this.m_Path[this.m_PathPointIndex].position - base.transform.position).normalized);
		this.m_PositionObject.transform.position = base.transform.position;
	}

	private void Update()
	{
		float num = this.m_MaxDistToPlayer + 2f;
		float num2 = Player.Get().transform.position.Distance(base.transform.position);
		if (this.m_State != ObjectFollowPath.State.MoveForward && num2 < this.m_MaxDistToPlayer)
		{
			this.m_State = ObjectFollowPath.State.MoveForward;
			this.m_PathPointIndex++;
		}
		else if (this.m_State != ObjectFollowPath.State.MoveBackward && this.m_State != ObjectFollowPath.State.None && num2 > num)
		{
			this.m_State = ObjectFollowPath.State.MoveBackward;
			this.m_PathPointIndex--;
		}
		this.UpdateCurrentPathPoint();
		this.UpdateOnPath();
		if (this.m_PathPointIndex == this.m_VPath.Count - 1)
		{
			float num3 = 0f;
			CJTools.Math.ProjectPointOnSegment(this.m_VPath[this.m_PathPointIndex - 1], this.m_VPath[this.m_PathPointIndex], base.transform.position, out num3);
			if (num3 >= 0.95f)
			{
				this.m_Finish = true;
			}
		}
		if (this.m_MaxDistToPlayer == 0f && !this.m_Finish)
		{
			this.m_CurrentSpeed = this.m_Speed;
		}
		else
		{
			float num4 = 0f;
			if (!this.m_Finish)
			{
				if (this.m_State == ObjectFollowPath.State.MoveForward)
				{
					if (num2 < this.m_MaxDistToPlayer)
					{
						num4 = this.m_Speed;
					}
				}
				else if (this.m_State == ObjectFollowPath.State.MoveBackward && num2 > num)
				{
					if (this.m_PathPointIndex == 0)
					{
						float num5 = 0f;
						CJTools.Math.ProjectPointOnSegment(this.m_VPath[this.m_PathPointIndex], this.m_VPath[this.m_PathPointIndex + 1], base.transform.position, out num5);
						Debug.Log(num5);
					}
					num4 = this.m_Speed;
				}
			}
			this.m_CurrentSpeed += (num4 - this.m_CurrentSpeed) * Time.deltaTime * 2f;
		}
		if (!this.m_Finish && this.m_PathPointIndex < this.m_VPath.Count && this.m_PathPointIndex >= 0)
		{
			this.m_PositionObject.transform.rotation = Quaternion.Slerp(this.m_PositionObject.transform.rotation, Quaternion.LookRotation((this.m_VPath[this.m_PathPointIndex] - this.m_PositionObject.transform.position).normalized), Time.deltaTime * this.m_Speed * 2f);
		}
		this.m_PositionObject.transform.position += this.m_PositionObject.transform.forward * this.m_CurrentSpeed * Time.deltaTime;
		base.transform.position = this.m_PositionObject.transform.position;
	}

	private void UpdateCurrentPathPoint()
	{
		if (this.m_Finish)
		{
			return;
		}
		if (this.m_PathPointIndex >= this.m_VPath.Count)
		{
			return;
		}
		float num = 0f;
		if (this.m_State == ObjectFollowPath.State.MoveForward)
		{
			CJTools.Math.ProjectPointOnSegment(this.m_VPath[this.m_PathPointIndex - 1], this.m_VPath[this.m_PathPointIndex], base.transform.position, out num);
			if (num >= 1f)
			{
				this.m_PathPointIndex++;
				return;
			}
		}
		else if (this.m_PathPointIndex > 0 && this.m_State == ObjectFollowPath.State.MoveBackward)
		{
			CJTools.Math.ProjectPointOnSegment(this.m_VPath[this.m_PathPointIndex + 1], this.m_VPath[this.m_PathPointIndex], base.transform.position, out num);
			if (num >= 1f)
			{
				this.m_PathPointIndex--;
			}
		}
	}

	private void UpdateOnPath()
	{
		if (this.m_Finish)
		{
			return;
		}
		if (this.m_PathPointIndex >= this.m_VPath.Count)
		{
			return;
		}
		float num = this.m_TargetShift;
		Vector3 vector = this.m_VPath[this.m_PathPointIndex] - base.transform.position;
		if (vector.magnitude >= num)
		{
			this.m_CurrentTarget = base.transform.position + vector.normalized * num;
			return;
		}
		num -= vector.magnitude;
		for (int i = this.m_PathPointIndex; i < this.m_VPath.Count - 1; i++)
		{
			Vector3 vector2 = this.m_VPath[i + 1] - this.m_VPath[i];
			if (vector2.magnitude >= num)
			{
				this.m_CurrentTarget = this.m_VPath[i] + vector2.normalized * num;
				return;
			}
			num -= vector2.magnitude;
		}
	}

	public float m_Speed;

	private float m_CurrentSpeed;

	public float m_MaxDistToPlayer;

	private bool m_Finish;

	public List<Transform> m_Path = new List<Transform>();

	private List<Vector3> m_VPath = new List<Vector3>();

	private int m_PathPointIndex = 1;

	private Vector3 m_CurrentTarget = Vector3.zero;

	private Vector3 m_PointOnPath = Vector3.zero;

	public float m_TargetShift = 1f;

	private GameObject m_PositionObject;

	private ObjectFollowPath.State m_State;

	private enum State
	{
		None,
		MoveForward,
		MoveBackward
	}
}
