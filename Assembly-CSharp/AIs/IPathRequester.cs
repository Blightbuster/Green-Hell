using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public interface IPathRequester
	{
		AI GetAI();

		void OnPathFound(List<Vector3> path, NavMeshPathStatus status);

		void OnPathNotFound();
	}
}
