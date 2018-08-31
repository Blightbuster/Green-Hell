using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class AIPathsManager : MonoBehaviour
	{
		public static AIPathsManager Get()
		{
			return AIPathsManager.s_Instance;
		}

		private void Awake()
		{
			AIPathsManager.s_Instance = this;
		}

		public void RequestPath(IPathRequester requester, Vector3 start, GameObject target, float radius = 0f)
		{
			this.RequestPath(requester, start, target.transform.position, radius);
		}

		public void RequestPath(IPathRequester requester, Vector3 start, Vector3 target, float radius = 0f)
		{
			PathRequest pathRequest = new PathRequest();
			pathRequest.m_Requester = requester;
			pathRequest.m_Radius = radius;
			pathRequest.m_StartPos = start;
			pathRequest.m_TargetPos = target;
			pathRequest.m_RequestTime = Time.time;
			pathRequest.m_NavMeshPath = new NavMeshPath();
			this.m_Requests.Add(pathRequest);
		}

		private void Update()
		{
			this.RemoveObsoleteRequests(ref this.m_Requests);
			this.RemoveObsoleteRequests(ref this.m_ActiveRequests);
			this.ProcessRequests();
		}

		private void RemoveObsoleteRequests(ref List<PathRequest> requests)
		{
			int i = 0;
			while (i < requests.Count)
			{
				PathRequest pathRequest = requests[i];
				if (Time.time - pathRequest.m_RequestTime >= pathRequest.m_MaxDuration)
				{
					this.OnPathNotFound(pathRequest);
				}
				else
				{
					i++;
				}
			}
		}

		private void ProcessRequests()
		{
			if (Time.time < this.m_NextProcessPathTime)
			{
				return;
			}
			while (this.m_Requests.Count > 0 && this.m_ActiveRequests.Count < this.m_MaxRequestsPerUpdate)
			{
				PathRequest pathRequest = this.m_Requests[0];
				NavMesh.CalculatePath(pathRequest.m_StartPos, pathRequest.m_TargetPos, AIManager.s_WalkableAreaMask, pathRequest.m_NavMeshPath);
				this.m_ActiveRequests.Add(pathRequest);
				this.m_Requests.Remove(pathRequest);
			}
			int i = 0;
			while (i < this.m_ActiveRequests.Count)
			{
				PathRequest pathRequest = this.m_ActiveRequests[i];
				if (pathRequest.m_Requester == null || pathRequest.m_Requester.GetAI() == null || !pathRequest.m_Requester.GetAI().gameObject.activeSelf || pathRequest.m_NavMeshPath.status == NavMeshPathStatus.PathInvalid)
				{
					this.OnPathNotFound(pathRequest);
				}
				else if (pathRequest.m_NavMeshPath.status == NavMeshPathStatus.PathComplete || pathRequest.m_NavMeshPath.status == NavMeshPathStatus.PathPartial)
				{
					if (pathRequest.m_NavMeshPath.corners.Length > 1)
					{
						this.OnPathFound(pathRequest);
					}
					else
					{
						this.OnPathNotFound(pathRequest);
					}
				}
				else
				{
					i++;
				}
			}
			this.m_NextProcessPathTime = Time.time * this.m_ProcessPathInterval;
		}

		private void OnPathFound(PathRequest request)
		{
			List<Vector3> path = new List<Vector3>(request.m_NavMeshPath.corners);
			if (request.m_Requester != null)
			{
				request.m_Requester.OnPathFound(path, request.m_NavMeshPath.status);
			}
			this.m_ActiveRequests.Remove(request);
		}

		private void OnPathNotFound(PathRequest request)
		{
			if (request.m_Requester != null)
			{
				request.m_Requester.OnPathNotFound();
			}
			this.m_ActiveRequests.Remove(request);
			this.m_Requests.Remove(request);
		}

		private List<PathRequest> m_Requests = new List<PathRequest>();

		private List<PathRequest> m_ActiveRequests = new List<PathRequest>();

		private int m_MaxRequestsPerUpdate = 10;

		private float m_ProcessPathInterval = 0.01f;

		private float m_NextProcessPathTime;

		private static AIPathsManager s_Instance;
	}
}
