using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AIAttractor : MonoBehaviour
	{
		private void Awake()
		{
			BoxCollider boxCollider = base.gameObject.GetComponent<BoxCollider>();
			if (!boxCollider)
			{
				boxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
			boxCollider.size = Vector3.one * this.m_Range;
			boxCollider.isTrigger = true;
			Rigidbody rigidbody = base.gameObject.GetComponent<Rigidbody>();
			if (!rigidbody)
			{
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.isKinematic = true;
			Renderer component = base.gameObject.GetComponent<Renderer>();
			if (component)
			{
				component.enabled = false;
			}
		}

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

		public float m_Range = 1f;
	}
}
