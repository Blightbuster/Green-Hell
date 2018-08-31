using System;
using UnityEngine;

namespace AIs
{
	public class AIPathPoint : MonoBehaviour
	{
		private void Start()
		{
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = false;
			}
		}

		public AIPathPoint m_Next;

		[HideInInspector]
		public AIPathPoint m_Prev;

		[HideInInspector]
		public float m_Progress;
	}
}
