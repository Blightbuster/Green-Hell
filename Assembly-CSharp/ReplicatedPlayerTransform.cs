using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ReplicatedTransform))]
public class ReplicatedPlayerTransform : ReplicatedBehaviour
{
	private void Awake()
	{
		if (base.ReplIsOwner())
		{
			Collider component = base.GetComponent<Collider>();
			if (component)
			{
				component.enabled = false;
			}
			Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Renderer[] componentsInChildren2 = base.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			Transform child = base.transform.GetChild(0);
			if (child != null)
			{
				child.gameObject.SetActive(false);
			}
			GameObject gameObject = Player.Get().gameObject;
			DebugUtils.Assert(gameObject, true);
			this.m_LocalPlayerTransform = gameObject.GetComponent<Transform>();
		}
		this.m_NetTransform = base.GetComponent<Transform>();
	}

	private void Update()
	{
		if (base.ReplIsOwner())
		{
			this.m_NetTransform.SetPositionAndRotation(this.m_LocalPlayerTransform.position, this.m_LocalPlayerTransform.rotation);
		}
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		GameObject gameObject = Player.Get().gameObject;
		DebugUtils.Assert(gameObject, true);
		this.m_LocalPlayerTransform = gameObject.GetComponent<Transform>();
	}

	private Transform m_LocalPlayerTransform;

	private Transform m_NetTransform;
}
