using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		private void Start()
		{
			this.m_Animator = base.GetComponent<Animator>();
			this.m_Rigidbody = base.GetComponent<Rigidbody>();
			this.m_Capsule = base.GetComponent<CapsuleCollider>();
			this.m_CapsuleHeight = this.m_Capsule.height;
			this.m_CapsuleCenter = this.m_Capsule.center;
			this.m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			this.m_OrigGroundCheckDistance = this.m_GroundCheckDistance;
		}

		public void Move(Vector3 move, bool crouch, bool jump)
		{
			if (move.magnitude > 1f)
			{
				move.Normalize();
			}
			move = base.transform.InverseTransformDirection(move);
			this.CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, this.m_GroundNormal);
			this.m_TurnAmount = Mathf.Atan2(move.x, move.z);
			this.m_ForwardAmount = move.z;
			this.ApplyExtraTurnRotation();
			if (this.m_IsGrounded)
			{
				this.HandleGroundedMovement(crouch, jump);
			}
			else
			{
				this.HandleAirborneMovement();
			}
			this.ScaleCapsuleForCrouching(crouch);
			this.PreventStandingInLowHeadroom();
			this.UpdateAnimator(move);
		}

		private void ScaleCapsuleForCrouching(bool crouch)
		{
			if (this.m_IsGrounded && crouch)
			{
				if (this.m_Crouching)
				{
					return;
				}
				this.m_Capsule.height = this.m_Capsule.height / 2f;
				this.m_Capsule.center = this.m_Capsule.center / 2f;
				this.m_Crouching = true;
			}
			else
			{
				Ray ray = new Ray(this.m_Rigidbody.position + Vector3.up * this.m_Capsule.radius * 0.5f, Vector3.up);
				float maxDistance = this.m_CapsuleHeight - this.m_Capsule.radius * 0.5f;
				if (Physics.SphereCast(ray, this.m_Capsule.radius * 0.5f, maxDistance))
				{
					this.m_Crouching = true;
					return;
				}
				this.m_Capsule.height = this.m_CapsuleHeight;
				this.m_Capsule.center = this.m_CapsuleCenter;
				this.m_Crouching = false;
			}
		}

		private void PreventStandingInLowHeadroom()
		{
			if (!this.m_Crouching)
			{
				Ray ray = new Ray(this.m_Rigidbody.position + Vector3.up * this.m_Capsule.radius * 0.5f, Vector3.up);
				float maxDistance = this.m_CapsuleHeight - this.m_Capsule.radius * 0.5f;
				if (Physics.SphereCast(ray, this.m_Capsule.radius * 0.5f, maxDistance))
				{
					this.m_Crouching = true;
				}
			}
		}

		private void UpdateAnimator(Vector3 move)
		{
			this.m_Animator.SetFloat("Forward", this.m_ForwardAmount, 0.1f, Time.deltaTime);
			this.m_Animator.SetFloat("Turn", this.m_TurnAmount, 0.1f, Time.deltaTime);
			this.m_Animator.SetBool("OnGround", this.m_IsGrounded);
			if (!this.m_IsGrounded)
			{
				this.m_Animator.SetFloat("Jump", this.m_Rigidbody.velocity.y);
			}
			float num = Mathf.Repeat(this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + this.m_RunCycleLegOffset, 1f);
			float value = (float)((num >= 0.5f) ? -1 : 1) * this.m_ForwardAmount;
			if (this.m_IsGrounded)
			{
				this.m_Animator.SetFloat("JumpLeg", value);
			}
			if (this.m_IsGrounded && move.magnitude > 0f)
			{
				this.m_Animator.speed = this.m_AnimSpeedMultiplier;
			}
			else
			{
				this.m_Animator.speed = 1f;
			}
		}

		private void HandleAirborneMovement()
		{
			Vector3 force = Physics.gravity * this.m_GravityMultiplier - Physics.gravity;
			this.m_Rigidbody.AddForce(force);
			this.m_GroundCheckDistance = ((this.m_Rigidbody.velocity.y >= 0f) ? 0.01f : this.m_OrigGroundCheckDistance);
		}

		private void HandleGroundedMovement(bool crouch, bool jump)
		{
			if (jump && !crouch && this.m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				this.m_Rigidbody.velocity = new Vector3(this.m_Rigidbody.velocity.x, this.m_JumpPower, this.m_Rigidbody.velocity.z);
				this.m_IsGrounded = false;
				this.m_Animator.applyRootMotion = false;
				this.m_GroundCheckDistance = 0.1f;
			}
		}

		private void ApplyExtraTurnRotation()
		{
			float num = Mathf.Lerp(this.m_StationaryTurnSpeed, this.m_MovingTurnSpeed, this.m_ForwardAmount);
			base.transform.Rotate(0f, this.m_TurnAmount * num * Time.deltaTime, 0f);
		}

		public void OnAnimatorMove()
		{
			if (this.m_IsGrounded && Time.deltaTime > 0f)
			{
				Vector3 velocity = this.m_Animator.deltaPosition * this.m_MoveSpeedMultiplier / Time.deltaTime;
				velocity.y = this.m_Rigidbody.velocity.y;
				this.m_Rigidbody.velocity = velocity;
			}
		}

		private void CheckGroundStatus()
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out raycastHit, this.m_GroundCheckDistance))
			{
				this.m_GroundNormal = raycastHit.normal;
				this.m_IsGrounded = true;
				this.m_Animator.applyRootMotion = true;
			}
			else
			{
				this.m_IsGrounded = false;
				this.m_GroundNormal = Vector3.up;
				this.m_Animator.applyRootMotion = false;
			}
		}

		[SerializeField]
		private float m_MovingTurnSpeed = 360f;

		[SerializeField]
		private float m_StationaryTurnSpeed = 180f;

		[SerializeField]
		private float m_JumpPower = 12f;

		[SerializeField]
		[Range(1f, 4f)]
		private float m_GravityMultiplier = 2f;

		[SerializeField]
		private float m_RunCycleLegOffset = 0.2f;

		[SerializeField]
		private float m_MoveSpeedMultiplier = 1f;

		[SerializeField]
		private float m_AnimSpeedMultiplier = 1f;

		[SerializeField]
		private float m_GroundCheckDistance = 0.1f;

		private Rigidbody m_Rigidbody;

		private Animator m_Animator;

		private bool m_IsGrounded;

		private float m_OrigGroundCheckDistance;

		private const float k_Half = 0.5f;

		private float m_TurnAmount;

		private float m_ForwardAmount;

		private Vector3 m_GroundNormal;

		private float m_CapsuleHeight;

		private Vector3 m_CapsuleCenter;

		private CapsuleCollider m_Capsule;

		private bool m_Crouching;
	}
}
