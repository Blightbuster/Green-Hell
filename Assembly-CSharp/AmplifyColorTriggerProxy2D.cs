using System;
using UnityEngine;

[AddComponentMenu("")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class AmplifyColorTriggerProxy2D : AmplifyColorTriggerProxyBase
{
	private void Start()
	{
		this.circleCollider = base.GetComponent<CircleCollider2D>();
		this.circleCollider.radius = 0.01f;
		this.circleCollider.isTrigger = true;
		this.rigidBody = base.GetComponent<Rigidbody2D>();
		this.rigidBody.gravityScale = 0f;
		this.rigidBody.isKinematic = true;
	}

	private void LateUpdate()
	{
		base.transform.position = this.Reference.position;
		base.transform.rotation = this.Reference.rotation;
	}

	private CircleCollider2D circleCollider;

	private Rigidbody2D rigidBody;
}
