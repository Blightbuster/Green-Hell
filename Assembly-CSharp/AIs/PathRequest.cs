using System;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class PathRequest
	{
		public float m_MaxDuration = 1f;

		public IPathRequester m_Requester;

		public float m_Radius = 1f;

		public Vector3 m_StartPos = Vector3.zero;

		public Vector3 m_TargetPos = Vector3.zero;

		public float m_RequestTime;

		public NavMeshPath m_NavMeshPath;
	}
}
