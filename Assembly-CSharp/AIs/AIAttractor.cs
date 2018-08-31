using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIAttractor : MonoBehaviour
	{
		public bool CanAtract(AI ai)
		{
			return true;
		}

		private void OnEnable()
		{
			AIAttractorsManager.RegisterAttractor(this);
		}

		private void OnDisable()
		{
			AIAttractorsManager.UnregisterAttractor(this);
		}

		private void OnTriggerEnter(Collider other)
		{
			AI component = other.gameObject.GetComponent<AI>();
			if (!component)
			{
				return;
			}
			if (component.m_Attractor == this)
			{
				component.ReleaseAttractor();
			}
		}

		public AIMoveStyle m_MoveStyle = AIMoveStyle.None;

		[HideInInspector]
		public bool m_Occupied;

		public float m_Range = 20f;
	}
}
