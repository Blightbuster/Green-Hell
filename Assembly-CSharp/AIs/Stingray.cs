using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class Stingray : Fish
	{
		protected override void Start()
		{
			base.Start();
			this.m_BoxCollider.isTrigger = true;
		}

		protected override void SetupMouth()
		{
		}

		public override bool IsStringray()
		{
			return true;
		}

		protected override void OnExitState()
		{
			base.OnExitState();
			if (this.m_FishState == Fish.State.Idle)
			{
				this.m_LastIdleTime = Time.time;
			}
		}

		protected override void OnEnterState()
		{
			base.OnEnterState();
			Fish.State fishState = this.m_FishState;
			if (fishState != Fish.State.Swim)
			{
				switch (fishState)
				{
				case Fish.State.RunAwayFromNoise:
					goto IL_2E;
				case Fish.State.Idle:
					this.m_Animator.SetTrigger(this.m_IdleHash);
					return;
				case Fish.State.HitReaction:
					this.m_Animator.SetTrigger(this.m_HitReactionHash);
					return;
				}
				DebugUtils.Assert(DebugUtils.AssertType.Info);
				return;
			}
			IL_2E:
			this.m_Animator.SetTrigger(this.m_SwimHash);
		}

		protected override void UpdateState()
		{
			Fish.State fishState = this.m_FishState;
			if (fishState == Fish.State.Swim)
			{
				this.UpdateSwimState();
				return;
			}
			if (fishState != Fish.State.Idle)
			{
				base.UpdateState();
				return;
			}
			this.UodateIdleState();
		}

		private void UodateIdleState()
		{
			if (Time.time - this.m_StartStateTime > 10f)
			{
				base.SetState(Fish.State.Swim);
			}
		}

		protected override void UpdateSwimState()
		{
			base.UpdateSwimState();
			this.UpdateBlend();
			if (Time.time - this.m_LastIdleTime > 60f && base.transform.position.y - MainLevel.GetTerrainY(base.transform.position) <= 0f)
			{
				base.SetState(Fish.State.Idle);
			}
		}

		private void UpdateBlend()
		{
			Vector3 normalized2D = base.transform.forward.GetNormalized2D();
			float b = (this.m_Target - base.transform.position).GetNormalized2D().AngleSigned(normalized2D, Vector3.up);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, 45f, -45f);
			this.m_Blend += (proportionalClamp - this.m_Blend) * Time.deltaTime * 5f;
			this.m_Blend = Mathf.Clamp01(this.m_Blend);
			this.m_Animator.SetFloat("Blend", this.m_Blend);
		}

		public override void OnImpaleOnSpear()
		{
			base.OnImpaleOnSpear();
			base.SetState(Fish.State.HitReaction);
		}

		protected override void UpdateAnimatorSpeed(float speed)
		{
			this.m_Animator.speed = 1f;
		}

		protected override float GetMinSpeed()
		{
			return 0.2f + this.m_AdditionalSpeed;
		}

		protected override float GetMaxSpeed()
		{
			return 0.2f + this.m_AdditionalSpeed;
		}

		protected override bool IsPointInsideTank(Vector3 point)
		{
			point.y = ((point.y < this.m_Tank.m_BoxCollider.bounds.center.y) ? this.m_Tank.m_BoxCollider.bounds.center.y : point.y);
			return this.m_Tank && this.m_Tank.IsPointInside(point);
		}

		protected override void Update()
		{
			base.Update();
			if (this.m_AdditionalSpeed > 0f)
			{
				this.m_AdditionalSpeed -= Time.deltaTime * 0.5f;
				if (this.m_AdditionalSpeed < 0f)
				{
					this.m_AdditionalSpeed = 0f;
				}
			}
		}

		protected override void UpdateSwimming()
		{
			Vector3 vector = this.m_Target - base.transform.position;
			base.gameObject.transform.localRotation = Quaternion.Slerp(base.gameObject.transform.localRotation, Quaternion.LookRotation(vector.normalized), Time.deltaTime * this.GetRotationSpeed());
			float num = this.m_Target.Distance2D(base.transform.position);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.GetMinSpeed(), this.GetMaxSpeed(), num, 0.1f, 3f);
			RaycastHit raycastHit;
			if (num <= 0.1f)
			{
				this.SetupTarget();
			}
			else if (Physics.Linecast(base.transform.position + Vector3.up * 0.3f, this.m_Target, out raycastHit) && !raycastHit.collider.isTrigger && raycastHit.collider.gameObject != base.gameObject)
			{
				this.SetupTarget();
			}
			base.gameObject.transform.position += base.gameObject.transform.forward * Time.deltaTime * proportionalClamp;
			this.UpdateAnimatorSpeed(proportionalClamp);
			Debug.DrawLine(this.m_Target, this.m_Target + Vector3.up, Color.red);
			this.AllignToTerrain();
		}

		private void AllignToTerrain()
		{
			Vector3 position = base.transform.position;
			position.y = MainLevel.GetTerrainY(position);
			base.transform.position = position;
			if (base.IsVisible())
			{
				Vector3 normalized2D = base.transform.forward.GetNormalized2D();
				Vector3 vector = this.m_BoxCollider.bounds.center + normalized2D * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
				vector.y = MainLevel.GetTerrainY(vector);
				Vector3 vector2 = this.m_BoxCollider.bounds.center - normalized2D * this.m_BoxCollider.size.z + Vector3.up * this.m_BoxCollider.size.y;
				vector2.y = MainLevel.GetTerrainY(vector2);
				Vector3 vector3 = vector - vector2;
				Vector3 normalized2D2 = base.transform.right.GetNormalized2D();
				Vector3 vector4 = this.m_BoxCollider.bounds.center + normalized2D2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
				vector4.y = MainLevel.GetTerrainY(vector4);
				Vector3 vector5 = this.m_BoxCollider.bounds.center - normalized2D2 * this.m_BoxCollider.size.x + Vector3.up * this.m_BoxCollider.size.y;
				vector5.y = MainLevel.GetTerrainY(vector5);
				Vector3 vector6 = vector4 - vector5;
				Vector3 vector7 = Vector3.Cross(vector3.normalized, vector6.normalized);
				if (!vector7.IsZero())
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector3.normalized, vector7.normalized), 1f);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.IsPlayer())
			{
				this.m_AdditionalSpeed = 2f;
				Player.Get().GiveDamage(base.gameObject, null, this.m_Params.m_Damage, Vector3.up, this.m_Params.m_DamageType, 1, false);
			}
		}

		private int m_SwimHash = Animator.StringToHash("Swim");

		private int m_IdleHash = Animator.StringToHash("Idle");

		private int m_HitReactionHash = Animator.StringToHash("HitReaction");

		private const float DEFAULT_BLEND_MUL = 5f;

		private float m_Blend = 0.5f;

		private const float MIN_TANK_SIZE = 10f;

		private float m_LastIdleTime;

		private float m_AdditionalSpeed;
	}
}
