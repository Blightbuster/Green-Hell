using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicCullingSystem : MonoBehaviour
{
	private void Start()
	{
		this.rigidbody = base.GetComponent<Rigidbody>();
		this.group = new CullingGroup();
		this.group.targetCamera = Camera.main;
		this.spheres[0] = new BoundingSphere(base.transform.position, this.sphereSize);
		this.group.SetBoundingSpheres(this.spheres);
		this.group.SetBoundingSphereCount(1);
		this.group.onStateChanged = new CullingGroup.StateChanged(this.StateChangedMethod);
		this.group.SetBoundingDistances(new float[]
		{
			this.physicDistance
		});
		this.mainCamera = Camera.main;
		this.group.SetDistanceReferencePoint(Camera.main.transform);
		base.Invoke("CheckVisibility", 0.1f);
	}

	private void OnDrawGizmosSelected()
	{
		if (this.gizmo)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, this.physicDistance);
		}
	}

	private void CheckVisibility()
	{
		bool flag = false;
		if (this.group.GetDistance(0) == 0)
		{
			flag = true;
		}
		if (!flag)
		{
			this.StartMovement();
		}
	}

	public void Update()
	{
		if (this.mainCamera != Camera.main)
		{
			this.mainCamera = Camera.main;
		}
		this.group.SetDistanceReferencePoint(Camera.main.transform);
		this.spheres[0].position = base.transform.position;
	}

	private void StateChangedMethod(CullingGroupEvent evt)
	{
		bool flag = false;
		if (this.group.GetDistance(0) == 0)
		{
			flag = true;
		}
		if (flag)
		{
			this.StopMovement();
		}
		else
		{
			this.StartMovement();
		}
	}

	private void OnDisable()
	{
		if (this.group != null)
		{
			this.group.Dispose();
			this.group = null;
		}
	}

	private void StopMovement()
	{
		this.velocity = this.rigidbody.velocity;
		this.angularVelocity = this.rigidbody.angularVelocity;
		this.rigidbody.isKinematic = false;
	}

	private void StartMovement()
	{
		this.rigidbody.isKinematic = true;
		this.rigidbody.velocity = this.velocity;
		this.rigidbody.angularVelocity = this.angularVelocity;
	}

	[Tooltip("Max view distance is referred from camera to terrain center point")]
	public float physicDistance = 10000f;

	private float sphereSize = 0.5f;

	private Rigidbody rigidbody;

	private CullingGroup group;

	private BoundingSphere[] spheres = new BoundingSphere[1000];

	private Camera mainCamera;

	[HideInInspector]
	public Vector3 velocity;

	[HideInInspector]
	public Vector3 angularVelocity;

	public bool gizmo = true;
}
