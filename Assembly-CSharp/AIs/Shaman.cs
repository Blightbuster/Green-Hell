using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class Shaman : Being, IAnimationEventsReceiver
	{
		protected override void Start()
		{
			base.Start();
			this.m_Animator = base.GetComponent<Animator>();
			this.m_Player = Player.Get();
			this.SetupAgent();
			this.InitItem();
			base.transform.rotation = Quaternion.LookRotation(Player.Get().transform.position - base.transform.position, Vector3.up);
			DebugUtils.Assert(this.m_AttackClips.Length != 0, "[Shaman:Start] Missing attack clips!", true, DebugUtils.AssertType.Info);
			this.m_AudioSource = base.gameObject.GetComponent<AudioSource>();
			this.m_AudioSource.clip = this.m_AttackClips[UnityEngine.Random.Range(0, this.m_AttackClips.Length)];
			this.m_AudioSource.Play();
			this.SetState(Shaman.State.Spawn);
		}

		private void SetupAgent()
		{
			this.m_Agent = base.gameObject.GetComponent<NavMeshAgent>();
			this.m_Agent.updatePosition = true;
			this.m_Agent.updateRotation = false;
			this.m_MoveSpeed = this.m_Agent.speed;
			this.m_Agent.speed = 0f;
		}

		private void InitItem()
		{
			this.m_Item = ItemsManager.Get().CreateItem(ItemID.Axe, false, Vector3.zero, Quaternion.identity);
			this.m_Item.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			this.m_Item.gameObject.GetComponent<Collider>().isTrigger = true;
			this.m_Item.enabled = false;
			this.AttachItem();
		}

		private void AttachItem()
		{
			Transform transform = base.gameObject.transform.FindDeepChild("RHolder");
			Quaternion rhs = Quaternion.Inverse(this.m_Item.m_Holder.localRotation);
			Vector3 b = this.m_Item.m_Holder.parent.position - this.m_Item.m_Holder.position;
			this.m_Item.gameObject.transform.rotation = transform.rotation;
			this.m_Item.gameObject.transform.rotation *= rhs;
			this.m_Item.gameObject.transform.position = transform.position;
			this.m_Item.gameObject.transform.position += b;
			this.m_Item.gameObject.transform.parent = transform.transform;
		}

		private void SetState(Shaman.State state)
		{
			this.OnExitState();
			this.m_State = state;
			this.OnEnterState();
		}

		private void OnExitState()
		{
			this.ResetAnimator();
		}

		private void OnEnterState()
		{
			this.m_EnterStateTime = Time.time;
			switch (this.m_State)
			{
			case Shaman.State.MoveToPlayer:
				this.m_Animator.SetTrigger(this.m_MoveHash);
				this.m_Agent.speed = this.m_MoveSpeed;
				return;
			case Shaman.State.Attack:
				this.m_Animator.SetTrigger((ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Slow) ? this.m_AttackHash : this.m_AttackFastHash);
				this.m_Agent.speed = this.m_MoveSpeed;
				return;
			case Shaman.State.Idle:
				this.m_Animator.SetTrigger(this.m_IdleHash);
				return;
			case Shaman.State.Spawn:
				this.m_Animator.SetTrigger(this.m_IdleHash);
				return;
			default:
				return;
			}
		}

		private void ResetAnimator()
		{
			this.m_Animator.ResetTrigger(this.m_MoveHash);
			this.m_Animator.ResetTrigger(this.m_AttackHash);
			this.m_Animator.ResetTrigger(this.m_IdleHash);
		}

		private bool ShouldSetupDestination()
		{
			if (ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Slow)
			{
				return this.m_State == Shaman.State.MoveToPlayer && this.m_Agent.destination.Distance(this.m_Player.transform.position) > 0.2f;
			}
			return this.m_Agent.destination.Distance(this.m_Player.transform.position) > 0.2f;
		}

		private void SetupDestination()
		{
			this.m_Agent.SetDestination(this.m_Player.transform.position);
		}

		protected override void Update()
		{
			base.Update();
			this.UpdateMoveSpeed();
			this.UpdateState();
			this.UpdateDirection();
		}

		private void UpdateMoveSpeed()
		{
			if (ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Slow)
			{
				this.m_Animator.SetFloat(this.m_MoveSpeedHash, (this.m_State == Shaman.State.MoveToPlayer) ? this.m_Agent.velocity.magnitude : 0f);
				return;
			}
			this.m_Animator.SetFloat(this.m_MoveSpeedHash, this.m_Agent.velocity.magnitude);
		}

		private void UpdateState()
		{
			switch (this.m_State)
			{
			case Shaman.State.MoveToPlayer:
				if (this.ShouldSetupDestination())
				{
					this.SetupDestination();
				}
				if (base.transform.position.Distance(this.m_Player.transform.position) <= this.m_Agent.stoppingDistance)
				{
					this.SetState(Shaman.State.Attack);
					return;
				}
				break;
			case Shaman.State.Attack:
				if (this.ShouldSetupDestination())
				{
					this.SetupDestination();
					return;
				}
				break;
			case Shaman.State.Idle:
				if (ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Fast && Time.time - this.m_EnterStateTime >= 0.5f && base.transform.position.Distance(this.m_Player.transform.position) > this.m_Agent.stoppingDistance + 1f)
				{
					this.SetState(Shaman.State.MoveToPlayer);
					return;
				}
				if (Time.time - this.m_EnterStateTime >= 2f)
				{
					if (base.transform.position.Distance(this.m_Player.transform.position) <= this.m_Agent.stoppingDistance)
					{
						this.SetState(Shaman.State.Attack);
						return;
					}
					this.SetState(Shaman.State.MoveToPlayer);
					return;
				}
				break;
			case Shaman.State.Spawn:
				if (Time.time - this.m_EnterStateTime >= 1f)
				{
					this.SetState(Shaman.State.MoveToPlayer);
				}
				break;
			default:
				return;
			}
		}

		private void UpdateDirection()
		{
			Vector3 forward = Vector3.zero;
			if (this.m_Agent.path.corners.Length > 2)
			{
				forward = (this.m_Agent.path.corners[1] - this.m_Agent.path.corners[0]).GetNormalized2D();
			}
			else
			{
				forward = (this.m_Player.transform.position - base.transform.position).GetNormalized2D();
			}
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(forward), 10f * Time.deltaTime);
		}

		public override bool TakeDamage(DamageInfo info)
		{
			base.TakeDamage(info);
			UnityEngine.Object.Destroy(base.gameObject);
			ShamanManager.Get().OnKillShaman();
			return true;
		}

		public bool IsActive()
		{
			return true;
		}

		public virtual bool ForceReceiveAnimEvent()
		{
			return false;
		}

		public void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.ShamanAttackEnd)
			{
				if (ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Fast)
				{
					if (base.transform.position.Distance(this.m_Player.transform.position) <= this.m_Agent.stoppingDistance)
					{
						this.SetState(Shaman.State.Attack);
						return;
					}
					this.SetState(Shaman.State.MoveToPlayer);
					return;
				}
				else if (ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Slow)
				{
					if (this.m_AttacksCount == 0 && base.transform.position.Distance(this.m_Player.transform.position) <= this.m_Agent.stoppingDistance && UnityEngine.Random.Range(0f, 1f) > this.m_DoubleAttackChance)
					{
						this.SetState(Shaman.State.Attack);
						this.m_AttacksCount++;
						return;
					}
					this.SetState(Shaman.State.Idle);
					this.m_AttacksCount = 0;
					return;
				}
			}
			else if (id == AnimEventID.ShamanDamage)
			{
				this.TryGiveDamage();
			}
		}

		private void TryGiveDamage()
		{
			if (base.transform.position.Distance(this.m_Player.transform.position) > this.m_Agent.stoppingDistance)
			{
				return;
			}
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damager = base.gameObject;
			damageInfo.m_Damage = ((ShamanManager.Get().m_AttackVersion == ShamanManager.AttackVersion.Fast) ? 5f : 20f);
			this.m_Player.TakeDamage(damageInfo);
			CJDebug.Log("TryGiveDamage");
		}

		public override bool CanReceiveDamageOfType(DamageType damage_type)
		{
			return true;
		}

		private void DebugRender()
		{
			if (!GreenHellGame.DEBUG)
			{
				return;
			}
			for (int i = 1; i < this.m_Agent.path.corners.Length; i++)
			{
				Vector3 vector = this.m_Agent.path.corners[i - 1];
				vector.y = MainLevel.GetTerrainY(vector);
				Vector3 vector2 = this.m_Agent.path.corners[i];
				vector2.y = MainLevel.GetTerrainY(vector2);
				Debug.DrawLine(vector, vector2, Color.blue);
			}
		}

		private Shaman.State m_State;

		private float m_EnterStateTime;

		public float m_DoubleAttackChance = 0.5f;

		private int m_AttacksCount;

		private float m_MoveSpeed;

		private Player m_Player;

		private NavMeshAgent m_Agent;

		private Item m_Item;

		private AudioSource m_AudioSource;

		public AudioClip[] m_AttackClips;

		private Animator m_Animator;

		private int m_MoveHash = Animator.StringToHash("Move");

		private int m_IdleHash = Animator.StringToHash("Idle");

		private int m_AttackHash = Animator.StringToHash("Attack");

		private int m_AttackFastHash = Animator.StringToHash("AttackFast");

		private int m_MoveSpeedHash = Animator.StringToHash("MoveSpeed");

		private enum State
		{
			None,
			MoveToPlayer,
			Attack,
			Idle,
			Spawn
		}
	}
}
