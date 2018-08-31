using System;
using UnityEngine;

public class ImperialFurPhysics : MonoBehaviour
{
	private void Start()
	{
		this.rigidBody = base.gameObject.GetComponent<Rigidbody>();
		this.material = base.gameObject.GetComponent<Renderer>().material;
		this.thisTransform = base.transform;
		this.oldPosition = this.thisTransform.position;
		if (this.rigidBody == null && this.useRigidbody)
		{
			Debug.LogWarning("No Rigidbody attached to fur object. Defaulting to non-Rigidbody simulation");
			this.useRigidbody = false;
		}
	}

	private void Update()
	{
		if (this.useRigidbody)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (this.physicsEnabled && !this.useRigidbody)
		{
			Vector3 a = this.oldPosition - this.thisTransform.position;
			vector = a / Time.deltaTime;
			this.oldPosition = this.thisTransform.position;
			vector *= this.forceScale;
		}
		this.CalculateAdditionalForce(vector);
	}

	private void FixedUpdate()
	{
		if (!this.useRigidbody)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (this.physicsEnabled)
		{
			vector = -this.rigidBody.velocity;
			vector *= this.forceScale;
		}
		this.CalculateAdditionalForce(vector);
	}

	private void CalculateAdditionalForce(Vector3 force)
	{
		if (this.usePhysicsGravity)
		{
			force += Physics.gravity * this.gravityScale;
		}
		force += this.AdditionalGravity;
		if (this.windEnabled)
		{
			force += ImperialFurWind.windForce;
		}
		force = Vector3.ClampMagnitude(force, 1f);
		this.forceSmooth = Vector3.Lerp(this.forceSmooth, force, Time.deltaTime * this.forceDamping);
		this.material.SetVector("Displacement", base.transform.InverseTransformDirection(this.forceSmooth));
	}

	public void UpdatePhysics()
	{
		this.material.SetVector("Displacement", base.transform.InverseTransformDirection(this.forceSmooth));
	}

	public void UpdateMaterial()
	{
		this.material = base.gameObject.GetComponent<Renderer>().material;
	}

	public bool useRigidbody = true;

	public bool usePhysicsGravity = true;

	public bool physicsEnabled = true;

	public bool windEnabled;

	public Vector3 AdditionalGravity;

	public float forceScale = 1f;

	public float gravityScale = 0.25f;

	public float forceDamping = 3f;

	private Material material;

	private Rigidbody rigidBody;

	private Transform thisTransform;

	private Vector3 oldPosition;

	private Vector3 forceSmooth = Vector3.zero;
}
