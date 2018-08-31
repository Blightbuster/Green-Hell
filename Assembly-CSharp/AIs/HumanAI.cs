using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HumanAI : AI
	{
		protected override void Awake()
		{
			base.Awake();
			this.m_Head = base.transform.FindDeepChild("head");
			Transform transform = base.transform.FindDeepChild("Body_final");
			this.m_Renderer = transform.GetComponent<SkinnedMeshRenderer>();
			this.m_SoundPreset = (AI.SoundPreset)UnityEngine.Random.Range(0, 3);
			Debug.Log("HumanAI");
		}

		protected override void Start()
		{
			this.m_StartPosition = base.transform.position;
			this.m_StartForward = base.transform.forward;
			base.Start();
			this.InitWeapons();
			this.InitHair();
			Debug.Log("HumanAI - " + ((!this.m_Group) ? "No group" : this.m_Group.GetType().ToString()));
			BalanceSystem.Get().LogCooldowns();
		}

		private void InitWeapons()
		{
			if (this.m_PrimaryWeaponPrefabs != null && this.m_PrimaryWeaponPrefabs.Count > 0)
			{
				this.m_PrimaryWeapon = this.CreateItem(this.m_PrimaryWeaponPrefabs[UnityEngine.Random.Range(0, this.m_PrimaryWeaponPrefabs.Count)]);
				this.m_PrimaryWeapon.m_CanSave = false;
				this.m_CurrentWeapon = this.m_PrimaryWeapon;
				this.m_WeaponType = HumanAI.WeaponType.Primary;
			}
			else
			{
				DebugUtils.Assert("ERROR - Primary weapon is not set - " + base.name, true, DebugUtils.AssertType.Info);
			}
			if (this.m_SecondaryWeaponPrefabs != null && this.m_SecondaryWeaponPrefabs.Count > 0)
			{
				this.m_SecondaryWeapon = this.CreateItem(this.m_SecondaryWeaponPrefabs[UnityEngine.Random.Range(0, this.m_SecondaryWeaponPrefabs.Count)]);
				this.m_SecondaryWeapon.gameObject.SetActive(false);
				this.m_SecondaryWeapon.m_CanSave = false;
			}
			this.ShowWeapon(true);
		}

		private Item CreateItem(GameObject prefab)
		{
			Item item = ItemsManager.Get().CreateItem(prefab, false, Vector3.zero, Quaternion.identity);
			item.gameObject.SetActive(true);
			item.StaticPhxRequestAdd();
			Hand hand = (!item.m_Info.IsBow()) ? Hand.Right : Hand.Left;
			Transform transform = base.transform.FindDeepChild((hand != Hand.Right) ? "LH_holder" : "RH_holder");
			item.transform.rotation = transform.rotation;
			item.transform.position = transform.position;
			item.transform.parent = transform;
			item.transform.localRotation = Quaternion.identity;
			item.transform.localPosition = Vector3.zero;
			item.enabled = false;
			item.gameObject.SetActive(false);
			return item;
		}

		private void InitHair()
		{
			if (this.m_HairPrefabs == null || this.m_HairPrefabs.Count == 0)
			{
				return;
			}
			Transform transform = base.transform.FindDeepChild("addon_11");
			DebugUtils.Assert(transform, "Can't find hair dummy - addon_11!", true, DebugUtils.AssertType.Info);
			GameObject original = this.m_HairPrefabs[UnityEngine.Random.Range(0, this.m_HairPrefabs.Count)];
			UnityEngine.Object.Instantiate<GameObject>(original, transform.position, transform.rotation, transform);
		}

		public override bool IsHuman()
		{
			return true;
		}

		public override bool CheckActivityByDistance()
		{
			return false;
		}

		public void ShowWeapon(bool show)
		{
			if (this.m_CurrentWeapon)
			{
				this.m_CurrentWeapon.gameObject.SetActive(show);
			}
		}

		public override void SwitchWeapon()
		{
			this.ShowWeapon(false);
			if (this.m_CurrentWeapon == this.m_PrimaryWeapon)
			{
				this.m_CurrentWeapon = this.m_SecondaryWeapon;
				this.m_WeaponType = HumanAI.WeaponType.Secondary;
			}
			else
			{
				this.m_CurrentWeapon = this.m_PrimaryWeapon;
				this.m_WeaponType = HumanAI.WeaponType.Primary;
			}
			this.ShowWeapon(true);
		}

		public override void OnDie()
		{
			base.OnDie();
			if (!this.m_Hallucination && this.m_CurrentWeapon)
			{
				this.m_CurrentWeapon.transform.parent = null;
				this.m_CurrentWeapon.enabled = true;
				this.m_CurrentWeapon.m_CanSave = true;
				this.m_CurrentWeapon.m_Rigidbody.velocity = Vector3.zero;
				this.m_CurrentWeapon.m_Rigidbody.angularVelocity = Vector3.zero;
				this.m_CurrentWeapon.m_Info.m_Health = this.m_CurrentWeapon.m_Info.m_MaxHealth * 0.15f + (float)StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue * 5f;
				this.m_CurrentWeapon.m_Info.m_Health = Mathf.Min(this.m_CurrentWeapon.m_Info.m_Health, this.m_CurrentWeapon.m_Info.m_MaxHealth);
				this.m_CurrentWeapon.StaticPhxRequestRemove();
			}
			if (this.m_Group)
			{
				this.m_Group.RemovedAI(this);
			}
		}

		public bool CanReceiveHit()
		{
			if (this.m_GoalsModule.m_ActiveGoal == null)
			{
				return true;
			}
			AIGoalType type = this.m_GoalsModule.m_ActiveGoal.m_Type;
			if (type == AIGoalType.HumanHitReaction)
			{
				return this.m_Animator.GetCurrentAnimatorStateInfo(0).length * this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f;
			}
			return type != AIGoalType.HumanJumpBack || this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f;
		}

		public override bool TakeDamage(DamageInfo info)
		{
			return this.CanReceiveHit() && base.TakeDamage(info);
		}

		public override void OnPlayerStartAttack()
		{
			if (this.m_HumanFightModule)
			{
				this.m_HumanFightModule.OnPlayerStartAttack();
			}
		}

		protected override void Update()
		{
			base.Update();
			this.UpdateWeapon();
			if (this.m_PerformEmergency)
			{
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanEmergency);
			}
			if (!this.m_Group && this.GetState() != HumanAI.State.Attack)
			{
				this.SetState(HumanAI.State.Attack);
			}
		}

		protected virtual void UpdateWeapon()
		{
			if (!this.m_CurrentWeapon)
			{
				return;
			}
			this.m_CurrentWeapon.transform.localRotation = Quaternion.identity;
			this.m_CurrentWeapon.transform.localPosition = Vector3.zero;
		}

		public HumanAI.State GetState()
		{
			return this.m_State;
		}

		public void SetState(HumanAI.State state)
		{
			if (this.m_State == state)
			{
				return;
			}
			this.m_State = state;
			this.OnEnterState();
		}

		private void OnEnterState()
		{
			switch (this.m_State)
			{
			case HumanAI.State.Rest:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanRest);
				break;
			case HumanAI.State.Patrol:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanFollowPatrolPath);
				break;
			case HumanAI.State.Upset:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanUpset);
				break;
			case HumanAI.State.StartWave:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanMoveToEnemy);
				break;
			case HumanAI.State.Attack:
				if (this.m_GoalsModule.m_TauntGoal != null && !this.IsHunter() && UnityEngine.Random.Range(0f, 1f) < this.m_GoalsModule.m_TauntGoal.m_Probability)
				{
					this.m_GoalsModule.ActivateGoal(AIGoalType.HumanTaunt);
				}
				else if (this.m_GoalsModule.m_ActiveGoal != null)
				{
					this.m_GoalsModule.m_ActiveGoal.Deactivate();
				}
				break;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.m_Group != null)
			{
				this.m_Group.RemovedAI(this);
			}
		}

		public override void OnPlayerDie()
		{
			this.m_GoalsModule.ActivateGoal(AIGoalType.HumanTaunt);
		}

		private HumanAI.State m_State = HumanAI.State.None;

		[HideInInspector]
		public Vector3 m_StartPosition = Vector3.zero;

		[HideInInspector]
		public Vector3 m_StartForward = Vector3.zero;

		[NonSerialized]
		public HumanAIGroup m_Group;

		[NonSerialized]
		public HumanAIPatrol m_Patrol;

		public List<GameObject> m_HairPrefabs;

		public List<GameObject> m_PrimaryWeaponPrefabs;

		public List<GameObject> m_SecondaryWeaponPrefabs;

		[NonSerialized]
		public HumanAI.WeaponType m_WeaponType = HumanAI.WeaponType.None;

		[NonSerialized]
		public Item m_PrimaryWeapon;

		[NonSerialized]
		public Item m_SecondaryWeapon;

		[NonSerialized]
		public Item m_CurrentWeapon;

		[HideInInspector]
		public Construction m_SelectedConstruction;

		public enum State
		{
			None = -1,
			Rest,
			Patrol,
			Loiter,
			Upset,
			StartWave,
			Attack
		}

		public enum WeaponType
		{
			None = -1,
			Primary,
			Secondary
		}
	}
}
