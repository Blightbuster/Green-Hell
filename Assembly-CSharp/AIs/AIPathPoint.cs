using System;
using UnityEngine;

namespace AIs
{
	public class AIPathPoint : MonoBehaviour
	{
		private void Start()
		{
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		public AIPathPoint m_Next;

		[HideInInspector]
		public AIPathPoint m_Prev;

		[HideInInspector]
		public float m_Progress;
	}
}
