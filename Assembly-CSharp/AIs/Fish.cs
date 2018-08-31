using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class Fish : AI
	{
		protected override void Start()
		{
			base.Start();
			this.SetupMouth();
			this.SetState(Fish.State.Swim);
		}

		protected virtual void SetupMouth()
		{
			this.m_Mouth = base.transform.FindDeepChild("Mouth").gameObject;
			DebugUtils.Assert(this.m_Mouth, true);
			this.m_MouthShift = Vector3.Distance(base.transform.position, this.m_Mouth.transform.position);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.m_Tank)
			{
				this.m_Tank.OnDestroyFish(this);
			}
		}

		public override bool IsFish()
		{
			return true;
		}

		public GameObject GetPrefab()
		{
			return this.m_Prefab;
		}

		public void SetPrefab(GameObject prefab)
		{
			this.m_Prefab = prefab;
		}

		public override HitCollisionType GetHitCollisionType()
		{
			return HitCollisionType.Collider;
		}

		public void SetTank(FishTank tank)
		{
			this.m_Tank = tank;
			this.SetupTarget();
		}

		public void SetHook(FishingHook hook)
		{
			this.m_Hook = hook;
			this.SetState(Fish.State.SwimToHook);
		}

		protected virtual void SetupTarget()
		{
			if (!this.m_Tank)
			{
				return;
			}
			switch (this.m_FishState)
			{
			case Fish.State.Swim:
				this.m_Target = this.m_Tank.GetRandomPointInTankSpace(true, 0.5f);
				return;
			case Fish.State.SwimToHook:
				this.m_Target = this.m_Hook.transform.position;
				return;
			case Fish.State.RunAwayFromNoise:
			{
				Vector3 vector = (base.gameObject.transform.position - this.m_Noise.m_Position).GetNormalized2D();
				vector = Quaternion.Euler(0f, UnityEngine.Random.value * 30f, 0f) * vector;
				this.m_Target = base.transform.position + vector * 2f;
				if (!this.IsPointInsideTank(this.m_Target))
				{
					this.m_Target = this.m_Tank.GetRandomPointInTankSpace(true, 0.5f);
				}
				return;
			}
			case Fish.State.SwimToPlayer:
				this.m_Target = Player.Get().transform.position;
				return;
			}
			this.m_Target = base.transform.position;
		}

		public override void OnDie()
		{
			Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
			UnityEngine.Object.Destroy(component);
			this.m_Animator.enabled = false;
		}

		public override bool CanBeImpaledOnSpear()
		{
			return true;
		}

		public override void OnImpaleOnSpear()
		{
			base.OnImpaleOnSpear();
			Skill.Get<SpearFishingSkill>().OnSkillAction();
			if (this.m_Tank)
			{
				this.m_Tank.RemoveFish(this);
			}
		}

		public void OnHitByItem(Item hit_item, Vector3 hit_pos)
		{
			Item item = ItemsManager.Get().CreateItem(this.m_ID.ToString() + "_Body", false);
			DeadFish deadFish = item.gameObject.AddComponent<DeadFish>();
			deadFish.m_KillItem = hit_item;
			deadFish.m_HitPos = hit_pos;
			deadFish.m_Item = item;
			deadFish.m_Tank = this.m_Tank;
			deadFish.m_ID = this.m_ID;
			Animator componentInChildren = item.GetComponentInChildren<Animator>();
			if (componentInChildren)
			{
				componentInChildren.SetBool("Backpack", true);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void Catch()
		{
			this.SetState(Fish.State.Caught);
		}

		protected void SetState(Fish.State state)
		{
			this.OnExitState();
			this.m_FishState = state;
			this.OnEnterState();
		}

		protected virtual void OnExitState()
		{
		}

		protected virtual void OnEnterState()
		{
			this.m_StartStateTime = Time.time;
			switch (this.m_FishState)
			{
			case Fish.State.Swim:
			case Fish.State.SwimToHook:
			case Fish.State.RunAwayFromNoise:
			case Fish.State.SwimToPlayer:
				this.SetupTarget();
				break;
			case Fish.State.Caught:
				this.ForgetHook();
				break;
			}
		}

		protected override void Update()
		{
			if (!this.m_Tank)
			{
				return;
			}
			this.CheckNoise();
			this.UpdateState();
			this.UpdateSwimming();
		}

		protected virtual void CheckNoise()
		{
			if (!this.m_HearingModule || !this.m_HearingModule.enabled)
			{
				return;
			}
			this.m_HearingModule.OnUpdate();
			if (this.m_Noise != this.m_HearingModule.m_Noise)
			{
				this.m_Noise = this.m_HearingModule.m_Noise;
				if (this.m_Noise != null)
				{
					this.SetState(Fish.State.RunAwayFromNoise);
				}
			}
		}

		protected virtual void UpdateState()
		{
			switch (this.m_FishState)
			{
			case Fish.State.Swim:
				this.UpdateSwimState();
				break;
			case Fish.State.SwimToHook:
				this.UpdateSwimToHookState();
				break;
			case Fish.State.Bite:
				this.UpdateBiteState();
				break;
			case Fish.State.RunAwayFromNoise:
				this.UpdateRunAwayFromNoiseState();
				break;
			}
		}

		protected virtual void UpdateSwimState()
		{
		}

		private void UpdateSwimToHookState()
		{
			if (!this.m_Hook)
			{
				this.SetState(Fish.State.Swim);
				return;
			}
			float num = Vector3.Distance(this.m_Mouth.transform.position, this.m_Hook.transform.position);
			if (num < this.m_MinDistToHookToBite)
			{
				this.m_Hook.GetFishingRod().SetBiting(true);
				this.m_Hook.SetFish(this);
				this.m_LastDistToHook = float.MaxValue;
				this.SetupBitingDuration();
				this.SetState(Fish.State.Bite);
			}
			else if (num > this.m_LastDistToHook)
			{
				this.ForgetHook();
			}
			else
			{
				this.m_LastDistToHook = num;
			}
		}

		private void SetupBitingDuration()
		{
			this.m_BitingDuration = UnityEngine.Random.Range(this.m_Params.m_MinBitingDuration, this.m_Params.m_MaxBitingDuration) * Skill.Get<FishingSkill>().GetBiteDurationMul();
		}

		private void UpdateBiteState()
		{
			if (Time.time - this.m_StartStateTime >= this.m_BitingDuration)
			{
				if (this.m_Hook && UnityEngine.Random.Range(0f, 1f) <= this.m_Params.m_EatBaitChance)
				{
					this.m_Hook.DeleteBait();
				}
				this.ForgetHook();
			}
		}

		private void UpdateRunAwayFromNoiseState()
		{
			if (this.m_Noise == null)
			{
				this.SetState(Fish.State.Swim);
			}
		}

		private void ForgetHook()
		{
			if (this.m_Hook)
			{
				this.m_Hook.SetFish(null);
				this.m_Hook.GetFishingRod().SetBiting(false);
				this.SetHook(null);
				this.m_Tank.ResetFishAttractedByHook(this);
			}
		}

		protected bool IsPointInsideTank(Vector3 point)
		{
			return this.m_Tank && point.y > MainLevel.GetTerrainY(point) && this.m_Tank.IsPointInside(point);
		}

		public override bool CheckActivityByDistance()
		{
			return false;
		}

		private void UpdateSwimming()
		{
			Vector3 vector = this.m_Target - base.transform.position;
			base.gameObject.transform.localRotation = Quaternion.Slerp(base.gameObject.transform.localRotation, Quaternion.LookRotation(vector.normalized), Time.deltaTime * this.GetRotationSpeed());
			float magnitude = vector.magnitude;
			float proportionalClamp = CJTools.Math.GetProportionalClamp(this.GetMinSpeed(), this.GetMaxSpeed(), magnitude, 0.1f, 3f);
			if (magnitude <= 0.1f)
			{
				this.SetupTarget();
			}
			base.gameObject.transform.position += base.gameObject.transform.forward * Time.deltaTime * proportionalClamp;
			if (!this.IsPointInsideTank(base.transform.position))
			{
				base.transform.position = this.m_Tank.m_BoxCollider.ClosestPointOnBounds(base.transform.position);
				base.gameObject.transform.localRotation = Quaternion.Slerp(base.gameObject.transform.localRotation, Quaternion.LookRotation(vector.normalized), Time.deltaTime * this.GetRotationSpeed());
			}
			this.UpdateAnimatorSpeed(proportionalClamp);
		}

		protected virtual void UpdateAnimatorSpeed(float speed)
		{
			this.m_Animator.speed = CJTools.Math.GetProportionalClamp(0.3f, 2f, speed, this.GetMinSpeed(), this.GetMaxSpeed());
		}

		protected virtual float GetMinSpeed()
		{
			return 0.05f;
		}

		protected virtual float GetMaxSpeed()
		{
			return this.m_Tank.m_MaxSpeed * ((this.m_FishState != Fish.State.RunAwayFromNoise) ? 1f : 1.5f);
		}

		protected virtual float GetRotationSpeed()
		{
			return this.m_Tank.m_RotationSpeed;
		}

		private GameObject m_Prefab;

		protected Fish.State m_FishState;

		protected GameObject m_Mouth;

		private float m_MouthShift;

		[HideInInspector]
		public FishTank m_Tank;

		private FishingHook m_Hook;

		protected float m_StartStateTime;

		protected float m_MinDistToHookToBite = 0.1f;

		private float m_BitingDuration;

		private float m_HearingRange = 5f;

		protected Noise m_Noise;

		private float m_LastDistToHook = float.MaxValue;

		protected Vector3 m_Target = Vector3.zero;

		protected enum State
		{
			Swim,
			SwimToHook,
			Bite,
			Caught,
			RunAwayFromNoise,
			SwimToPlayer,
			BitePlayer,
			Idle,
			HitReaction
		}
	}
}
