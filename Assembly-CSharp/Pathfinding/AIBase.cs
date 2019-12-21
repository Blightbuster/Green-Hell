using System;
using Pathfinding.RVO;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public abstract class AIBase : VersionedMonoBehaviour
	{
		protected bool usingGravity { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			this.seeker = base.GetComponent<Seeker>();
			this.controller = base.GetComponent<CharacterController>();
			this.rigid = base.GetComponent<Rigidbody>();
			this.rigid2D = base.GetComponent<Rigidbody2D>();
			this.rvoController = base.GetComponent<RVOController>();
			this.tr = base.transform;
		}

		protected virtual void Update()
		{
			if (this.rigid == null && this.rigid2D == null)
			{
				this.usingGravity = !(this.gravity == Vector3.zero);
				this.MovementUpdate(Time.deltaTime);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (!(this.rigid == null) || !(this.rigid2D == null))
			{
				this.usingGravity = (!(this.gravity == Vector3.zero) && (this.rigid == null || this.rigid.isKinematic) && (this.rigid2D == null || this.rigid2D.isKinematic));
				this.MovementUpdate(Time.fixedDeltaTime);
			}
		}

		protected abstract void MovementUpdate(float deltaTime);

		protected void ApplyGravity(float deltaTime)
		{
			if (this.usingGravity)
			{
				float num;
				this.velocity2D += this.movementPlane.ToPlane(deltaTime * (float.IsNaN(this.gravity.x) ? Physics.gravity : this.gravity), out num);
				this.verticalVelocity += num;
				return;
			}
			this.verticalVelocity = 0f;
		}

		protected Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime)
		{
			if (this.rvoController != null && this.rvoController.enabled)
			{
				return this.movementPlane.ToPlane(this.rvoController.CalculateMovementDelta(this.movementPlane.ToWorld(position, 0f), deltaTime));
			}
			return Vector2.ClampMagnitude(this.velocity2D * deltaTime, distanceToEndOfPath);
		}

		protected virtual void RotateTowards(Vector2 direction, float maxDegrees)
		{
			if (direction != Vector2.zero)
			{
				Quaternion quaternion = Quaternion.LookRotation(this.movementPlane.ToWorld(direction, 0f), this.movementPlane.ToWorld(Vector2.zero, 1f));
				if (this.rotationIn2D)
				{
					quaternion *= Quaternion.Euler(90f, 0f, 0f);
				}
				this.tr.rotation = Quaternion.RotateTowards(this.tr.rotation, quaternion, maxDegrees);
			}
		}

		protected void Move(Vector3 position3D, Vector3 deltaPosition)
		{
			bool flag = false;
			if (this.controller != null && this.controller.enabled)
			{
				this.tr.position = position3D;
				this.controller.Move(deltaPosition);
				position3D = this.tr.position;
				if (this.controller.isGrounded)
				{
					this.verticalVelocity = 0f;
				}
			}
			else
			{
				float lastElevation;
				this.movementPlane.ToPlane(position3D, out lastElevation);
				position3D += deltaPosition;
				if (this.usingGravity)
				{
					position3D = this.RaycastPosition(position3D, lastElevation);
				}
				flag = true;
			}
			Vector3 vector = this.ClampToNavmesh(position3D);
			if ((vector - position3D).sqrMagnitude > 1.00000011E-06f)
			{
				Vector2 vector2 = this.movementPlane.ToPlane(vector - position3D);
				this.velocity2D -= vector2 * Vector2.Dot(vector2, this.velocity2D) / vector2.sqrMagnitude;
				if (this.rvoController != null && this.rvoController.enabled)
				{
					this.rvoController.SetCollisionNormal(vector2);
				}
				position3D = vector;
				flag = true;
			}
			if (flag)
			{
				if (this.rigid != null)
				{
					this.rigid.MovePosition(position3D);
					return;
				}
				if (this.rigid2D != null)
				{
					this.rigid2D.MovePosition(position3D);
					return;
				}
				this.tr.position = position3D;
			}
		}

		protected virtual Vector3 ClampToNavmesh(Vector3 position)
		{
			return position;
		}

		protected Vector3 RaycastPosition(Vector3 position, float lastElevation)
		{
			float num;
			this.movementPlane.ToPlane(position, out num);
			float num2 = this.centerOffset + Mathf.Max(0f, lastElevation - num);
			Vector3 vector = this.movementPlane.ToWorld(Vector2.zero, num2);
			RaycastHit raycastHit;
			if (Physics.Raycast(position + vector, -vector, out raycastHit, num2, this.groundMask, QueryTriggerInteraction.Ignore))
			{
				this.verticalVelocity = 0f;
				return raycastHit.point;
			}
			return position;
		}

		protected virtual void OnDrawGizmos()
		{
			if (!Application.isPlaying && this.controller == null)
			{
				this.controller = base.GetComponent<CharacterController>();
				this.rigid = base.GetComponent<Rigidbody>();
				this.rigid2D = base.GetComponent<Rigidbody2D>();
			}
			if ((this.rigid == null || this.rigid.isKinematic) && (this.rigid2D == null || this.rigid2D.isKinematic) && !(this.gravity == Vector3.zero) && (this.controller == null || !this.controller.enabled))
			{
				Gizmos.color = AIBase.GizmoColorRaycast;
				Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.up * this.centerOffset);
				Gizmos.DrawLine(base.transform.position - base.transform.right * 0.1f, base.transform.position + base.transform.right * 0.1f);
				Gizmos.DrawLine(base.transform.position - base.transform.forward * 0.1f, base.transform.position + base.transform.forward * 0.1f);
			}
		}

		public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

		public LayerMask groundMask = -1;

		public float centerOffset = 1f;

		public bool rotationIn2D;

		protected Vector2 velocity2D;

		protected float verticalVelocity;

		protected Seeker seeker;

		protected Transform tr;

		protected Rigidbody rigid;

		protected Rigidbody2D rigid2D;

		protected CharacterController controller;

		protected RVOController rvoController;

		protected IMovementPlane movementPlane = GraphTransform.identityTransform;

		protected static readonly Color GizmoColorRaycast = new Color(0.4627451f, 0.807843149f, 0.4392157f);
	}
}
