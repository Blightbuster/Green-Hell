using System;
using UnityEngine;

public class CharacterControllerProxy : MonoBehaviour
{
	private void Awake()
	{
		DebugUtils.Assert(this.m_Controller != null, true);
		if (this.transform.parent != null)
		{
			this.m_Owner = this.transform.parent.gameObject;
			this.transform.parent = null;
			this.m_IsDetached = true;
		}
		else
		{
			this.m_Owner = base.gameObject;
		}
		Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(componentsInChildren[i], this.m_Controller);
		}
	}

	private void PreMove()
	{
		if (!this.m_IsDetached)
		{
			return;
		}
		if (this.m_Owner.transform.position.Distance(this.transform.position) > 2f)
		{
			this.transform.position = this.m_Owner.transform.position;
		}
	}

	private void PostMove(bool set_transform_to_owner)
	{
		if (!this.m_IsDetached)
		{
			return;
		}
		if (set_transform_to_owner && this.m_Owner != null)
		{
			this.m_Owner.transform.position = this.transform.position;
			this.m_Owner.transform.rotation = this.transform.rotation;
		}
	}

	public new Transform transform
	{
		get
		{
			return this.m_Controller.transform;
		}
	}

	public Vector3 velocity
	{
		get
		{
			return this.m_Controller.velocity;
		}
	}

	public bool isGrounded
	{
		get
		{
			return this.m_Controller.isGrounded;
		}
	}

	public CollisionFlags collisionFlags
	{
		get
		{
			return this.m_Controller.collisionFlags;
		}
	}

	public float radius
	{
		get
		{
			return this.m_Controller.radius;
		}
		set
		{
			this.m_Controller.radius = value;
		}
	}

	public float height
	{
		get
		{
			return this.m_Controller.height;
		}
		set
		{
			this.m_Controller.height = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return this.m_Controller.center;
		}
		set
		{
			this.m_Controller.center = value;
		}
	}

	public float slopeLimit
	{
		get
		{
			return this.m_Controller.slopeLimit;
		}
		set
		{
			this.m_Controller.slopeLimit = value;
		}
	}

	public float stepOffset
	{
		get
		{
			return this.m_Controller.stepOffset;
		}
		set
		{
			this.m_Controller.stepOffset = value;
		}
	}

	public float skinWidth
	{
		get
		{
			return this.m_Controller.skinWidth;
		}
		set
		{
			this.m_Controller.skinWidth = value;
		}
	}

	public float minMoveDistance
	{
		get
		{
			return this.m_Controller.minMoveDistance;
		}
		set
		{
			this.m_Controller.minMoveDistance = value;
		}
	}

	public bool detectCollisions
	{
		get
		{
			return this.m_Controller.detectCollisions;
		}
		set
		{
			this.m_Controller.detectCollisions = value;
		}
	}

	public bool enableOverlapRecovery
	{
		get
		{
			return this.m_Controller.enableOverlapRecovery;
		}
		set
		{
			this.m_Controller.enableOverlapRecovery = value;
		}
	}

	public CollisionFlags Move(Vector3 motion, bool set_transform_to_object = true)
	{
		this.PreMove();
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = true;
		CollisionFlags result = this.m_Controller.Move(motion);
		Physics.autoSyncTransforms = autoSyncTransforms;
		this.PostMove(set_transform_to_object);
		return result;
	}

	public bool SimpleMove(Vector3 speed, bool set_transform_to_object = true)
	{
		this.PreMove();
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = true;
		bool result = this.m_Controller.SimpleMove(speed);
		Physics.autoSyncTransforms = autoSyncTransforms;
		this.PostMove(set_transform_to_object);
		return result;
	}

	public new bool enabled
	{
		get
		{
			return this.m_Controller.enabled;
		}
		set
		{
			this.m_Controller.enabled = value;
		}
	}

	public Rigidbody attachedRigidbody
	{
		get
		{
			return this.m_Controller.attachedRigidbody;
		}
	}

	public bool isTrigger
	{
		get
		{
			return this.m_Controller.isTrigger;
		}
		set
		{
			this.m_Controller.isTrigger = value;
		}
	}

	public float contactOffset
	{
		get
		{
			return this.m_Controller.contactOffset;
		}
		set
		{
			this.m_Controller.contactOffset = value;
		}
	}

	public Bounds bounds
	{
		get
		{
			return this.m_Controller.bounds;
		}
	}

	public PhysicMaterial sharedMaterial
	{
		get
		{
			return this.m_Controller.sharedMaterial;
		}
		set
		{
			this.m_Controller.sharedMaterial = value;
		}
	}

	public PhysicMaterial material
	{
		get
		{
			return this.m_Controller.material;
		}
		set
		{
			this.m_Controller.material = value;
		}
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		return this.m_Controller.ClosestPoint(position);
	}

	public Vector3 ClosestPointOnBounds(Vector3 position)
	{
		return this.m_Controller.ClosestPointOnBounds(position);
	}

	public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
	{
		return this.m_Controller.Raycast(ray, out hitInfo, maxDistance);
	}

	public CharacterController m_Controller;

	public const float DIST_TO_REPOSITION = 2f;

	private GameObject m_Owner;

	private bool m_IsDetached;
}
