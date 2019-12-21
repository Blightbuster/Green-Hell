using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class HumanAI : AI, IReplicatedBehaviour
	{
		protected override void Awake()
		{
			base.Awake();
			this.m_Head = base.transform.FindDeepChild("head");
			this.m_BowHolder = base.transform.FindDeepChild("back_holder");
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
			if (this.m_ID != AI.AIID.KidRunner)
			{
				this.InitWeapons();
				this.InitHair();
				this.InitMaterials();
			}
			Debug.Log("HumanAI - " + (this.m_Group ? this.m_Group.GetType().ToString() : "No group"));
		}

		private void InitWeapons()
		{
			if (!this.ReplIsOwner())
			{
				return;
			}
			if (this.m_PrimaryWeaponPrefabs != null && this.m_PrimaryWeaponPrefabs.Count > 0)
			{
				this.m_PrimaryWeapon = this.CreateItem(this.m_PrimaryWeaponPrefabs[UnityEngine.Random.Range(0, this.m_PrimaryWeaponPrefabs.Count)], false);
				this.m_PrimaryWeapon.m_CanSaveNotTriggered = false;
				this.m_CurrentWeapon = this.m_PrimaryWeapon;
				this.m_WeaponType = HumanAI.WeaponType.Primary;
			}
			if (this.m_AdditionalWeaponPrefabs != null && this.m_AdditionalWeaponPrefabs.Count > 0)
			{
				this.m_AdditionalWeapon = this.CreateItem(this.m_AdditionalWeaponPrefabs[UnityEngine.Random.Range(0, this.m_AdditionalWeaponPrefabs.Count)], true);
				this.m_AdditionalWeapon.m_CanSaveNotTriggered = false;
				this.m_CurrentAdditionalWeapon = this.m_AdditionalWeapon;
				this.m_WeaponType = HumanAI.WeaponType.Additional;
			}
			if (this.m_SecondaryWeaponPrefabs != null && this.m_SecondaryWeaponPrefabs.Count > 0)
			{
				this.m_SecondaryWeapon = this.CreateItem(this.m_SecondaryWeaponPrefabs[UnityEngine.Random.Range(0, this.m_SecondaryWeaponPrefabs.Count)], false);
				this.m_SecondaryWeapon.gameObject.SetActive(false);
				this.m_SecondaryWeapon.m_CanSaveNotTriggered = false;
			}
			this.ShowWeapon(true);
		}

		private Item CreateItem(GameObject prefab, bool additional = false)
		{
			Item item = ItemsManager.Get().CreateItem(prefab, false, Vector3.zero, Quaternion.identity);
			item.gameObject.SetActive(true);
			item.StaticPhxRequestAdd();
			item.UpdatePhx();
			int num = (additional || item.m_Info.IsBow()) ? 0 : 1;
			string name = string.Empty;
			if (num == 1)
			{
				name = "RH_holder";
			}
			else
			{
				name = "LH_holder";
			}
			Transform transform = base.transform.FindDeepChild(name);
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
			UnityEngine.Object.Instantiate<GameObject>(this.m_HairPrefabs[UnityEngine.Random.Range(0, this.m_HairPrefabs.Count)], transform.position, transform.rotation, transform);
		}

		private void InitMaterials()
		{
			switch (AIManager.Get().GetBodyPaintingIndex())
			{
			case 0:
				this.m_Renderer.materials = AIManager.Get().m_Painting0;
				return;
			case 1:
				this.m_Renderer.materials = AIManager.Get().m_Painting1;
				return;
			case 2:
				this.m_Renderer.materials = AIManager.Get().m_Painting2;
				return;
			default:
				return;
			}
		}

		public override bool IsHuman()
		{
			return true;
		}

		public override bool IsHumanAI()
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
			if (this.m_CurrentAdditionalWeapon)
			{
				this.m_CurrentAdditionalWeapon.gameObject.SetActive(show);
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
			if (!this.m_Hallucination)
			{
				if (this.m_PrimaryWeapon)
				{
					this.m_PrimaryWeapon.gameObject.SetActive(true);
					this.m_PrimaryWeapon.transform.parent = null;
					this.m_PrimaryWeapon.enabled = true;
					this.m_PrimaryWeapon.m_CanSaveNotTriggered = true;
					this.m_PrimaryWeapon.m_Rigidbody.velocity = Vector3.zero;
					this.m_PrimaryWeapon.m_Rigidbody.angularVelocity = Vector3.zero;
					this.m_PrimaryWeapon.m_Info.m_Health = this.m_PrimaryWeapon.m_Info.m_MaxHealth * 0.15f + (float)StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue * 5f;
					this.m_PrimaryWeapon.m_Info.m_Health = Mathf.Min(this.m_PrimaryWeapon.m_Info.m_Health, this.m_PrimaryWeapon.m_Info.m_MaxHealth);
					this.m_PrimaryWeapon.StaticPhxRequestRemove();
					this.m_PrimaryWeapon.transform.localScale = this.m_PrimaryWeapon.m_LocalScaleOnEnable;
				}
				if (this.m_AdditionalWeapon)
				{
					this.m_AdditionalWeapon.gameObject.SetActive(true);
					this.m_AdditionalWeapon.transform.parent = null;
					this.m_AdditionalWeapon.enabled = true;
					this.m_AdditionalWeapon.m_CanSaveNotTriggered = true;
					this.m_AdditionalWeapon.m_Rigidbody.velocity = Vector3.zero;
					this.m_AdditionalWeapon.m_Rigidbody.angularVelocity = Vector3.zero;
					this.m_AdditionalWeapon.m_Info.m_Health = this.m_AdditionalWeapon.m_Info.m_MaxHealth * 0.15f + (float)StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue * 5f;
					this.m_AdditionalWeapon.m_Info.m_Health = Mathf.Min(this.m_AdditionalWeapon.m_Info.m_Health, this.m_AdditionalWeapon.m_Info.m_MaxHealth);
					this.m_AdditionalWeapon.StaticPhxRequestRemove();
					this.m_AdditionalWeapon.transform.localScale = this.m_AdditionalWeapon.m_LocalScaleOnEnable;
				}
				if (this.m_SecondaryWeapon)
				{
					this.m_SecondaryWeapon.gameObject.SetActive(true);
					this.m_SecondaryWeapon.transform.parent = null;
					this.m_SecondaryWeapon.enabled = true;
					this.m_SecondaryWeapon.m_CanSaveNotTriggered = true;
					this.m_SecondaryWeapon.m_Rigidbody.velocity = Vector3.zero;
					this.m_SecondaryWeapon.m_Rigidbody.angularVelocity = Vector3.zero;
					this.m_SecondaryWeapon.m_Info.m_Health = this.m_SecondaryWeapon.m_Info.m_MaxHealth * 0.15f + (float)StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue * 5f;
					this.m_SecondaryWeapon.m_Info.m_Health = Mathf.Min(this.m_SecondaryWeapon.m_Info.m_Health, this.m_SecondaryWeapon.m_Info.m_MaxHealth);
					this.m_SecondaryWeapon.StaticPhxRequestRemove();
					this.m_SecondaryWeapon.transform.localScale = this.m_SecondaryWeapon.m_LocalScaleOnEnable;
				}
			}
			if (this.m_Group)
			{
				this.m_Group.OnAIDie(this);
			}
			if (this.m_LastDamageInfo != null && this.m_LastDamageInfo.m_Damager && this.m_LastDamageInfo.m_Damager == Player.Get().gameObject)
			{
				Player.Get().m_HasKilledTribeInLastFrame = true;
			}
		}

		public bool CanReceiveHit(Item damage_item = null)
		{
			if (this.m_GoalsModule.m_ActiveGoal == null)
			{
				return true;
			}
			if (damage_item && damage_item.IsSpikes())
			{
				return true;
			}
			AIGoalType type = this.m_GoalsModule.m_ActiveGoal.m_Type;
			if (type == AIGoalType.HumanHitReaction && this.m_Animator)
			{
				return this.m_Animator.GetCurrentAnimatorStateInfo(0).length * this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f;
			}
			return type != AIGoalType.HumanJumpBack || !this.m_Animator || this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f;
		}

		public override bool TakeDamage(DamageInfo info)
		{
			return this.CanReceiveHit(info.m_DamageItem) && base.TakeDamage(info);
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
			this.UpdateWeapons();
			if (this.m_PerformEmergency)
			{
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanEmergency);
			}
			if (!this.m_Group && this.GetState() != HumanAI.State.Attack && !this.IsKidRunner())
			{
				this.SetState(HumanAI.State.Attack);
			}
		}

		private void UpdateWeapons()
		{
			if (this.m_PrimaryWeapon && this.m_WeaponType == HumanAI.WeaponType.Secondary && this.m_PrimaryWeapon.m_Info.IsBow())
			{
				this.m_PrimaryWeapon.gameObject.SetActive(true);
				this.m_PrimaryWeapon.transform.rotation = this.m_BowHolder.rotation;
				this.m_PrimaryWeapon.transform.position = this.m_BowHolder.position;
			}
			if (this.m_CurrentWeapon)
			{
				this.m_CurrentWeapon.transform.localRotation = Quaternion.identity;
				this.m_CurrentWeapon.transform.localPosition = Vector3.zero;
			}
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
				return;
			case HumanAI.State.Patrol:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanFollowPatrolPath);
				return;
			case HumanAI.State.Loiter:
				break;
			case HumanAI.State.Upset:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanUpset);
				return;
			case HumanAI.State.StartWave:
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanMoveToEnemy);
				return;
			case HumanAI.State.Attack:
				if (this.m_GoalsModule.m_TauntGoal != null && !this.IsHunter() && UnityEngine.Random.Range(0f, 1f) < this.m_GoalsModule.m_TauntGoal.m_Probability)
				{
					this.m_GoalsModule.ActivateGoal(AIGoalType.HumanTaunt);
					return;
				}
				if (this.m_GoalsModule.m_ActiveGoal != null)
				{
					this.m_GoalsModule.m_ActiveGoal.Deactivate();
				}
				break;
			default:
				return;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.m_Group != null)
			{
				this.m_Group.RemovedAI(this, true);
			}
		}

		public override void OnPlayerDie()
		{
			this.m_GoalsModule.ActivateGoal(AIGoalType.HumanTaunt);
		}

		public void OnReplicationPrepare()
		{
		}

		public void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
		{
		}

		public void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
		{
		}

		public void OnReplicationResolve()
		{
		}

		public void ReplOnChangedOwner(bool was_owner)
		{
		}

		public void ReplOnSpawned()
		{
		}

		public virtual void OnReplicationPrepare_CJGenerated()
		{
			if (this.m_WeaponType_Repl != this.m_WeaponType)
			{
				this.m_WeaponType_Repl = this.m_WeaponType;
				this.ReplSetDirty();
			}
			if (this.m_PrimaryWeapon_Repl != this.m_PrimaryWeapon)
			{
				this.m_PrimaryWeapon_Repl = this.m_PrimaryWeapon;
				this.ReplSetDirty();
			}
			if (this.m_AdditionalWeapon_Repl != this.m_AdditionalWeapon)
			{
				this.m_AdditionalWeapon_Repl = this.m_AdditionalWeapon;
				this.ReplSetDirty();
			}
			if (this.m_SecondaryWeapon_Repl != this.m_SecondaryWeapon)
			{
				this.m_SecondaryWeapon_Repl = this.m_SecondaryWeapon;
				this.ReplSetDirty();
			}
			if (this.m_CurrentWeapon_Repl != this.m_CurrentWeapon)
			{
				this.m_CurrentWeapon_Repl = this.m_CurrentWeapon;
				this.ReplSetDirty();
			}
			if (this.m_CurrentAdditionalWeapon_Repl != this.m_CurrentAdditionalWeapon)
			{
				this.m_CurrentAdditionalWeapon_Repl = this.m_CurrentAdditionalWeapon;
				this.ReplSetDirty();
			}
		}

		public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
		{
			writer.Write((int)this.m_WeaponType_Repl);
			writer.Write(this.m_PrimaryWeapon_Repl ? this.m_PrimaryWeapon_Repl.gameObject : null);
			writer.Write(this.m_PrimaryWeapon_Repl ? this.m_PrimaryWeapon_Repl.GetReplicationComponent().GetComponentIndex(this.m_PrimaryWeapon_Repl) : -1);
			writer.Write(this.m_AdditionalWeapon_Repl ? this.m_AdditionalWeapon_Repl.gameObject : null);
			writer.Write(this.m_AdditionalWeapon_Repl ? this.m_AdditionalWeapon_Repl.GetReplicationComponent().GetComponentIndex(this.m_AdditionalWeapon_Repl) : -1);
			writer.Write(this.m_SecondaryWeapon_Repl ? this.m_SecondaryWeapon_Repl.gameObject : null);
			writer.Write(this.m_SecondaryWeapon_Repl ? this.m_SecondaryWeapon_Repl.GetReplicationComponent().GetComponentIndex(this.m_SecondaryWeapon_Repl) : -1);
			writer.Write(this.m_CurrentWeapon_Repl ? this.m_CurrentWeapon_Repl.gameObject : null);
			writer.Write(this.m_CurrentWeapon_Repl ? this.m_CurrentWeapon_Repl.GetReplicationComponent().GetComponentIndex(this.m_CurrentWeapon_Repl) : -1);
			writer.Write(this.m_CurrentAdditionalWeapon_Repl ? this.m_CurrentAdditionalWeapon_Repl.gameObject : null);
			writer.Write(this.m_CurrentAdditionalWeapon_Repl ? this.m_CurrentAdditionalWeapon_Repl.GetReplicationComponent().GetComponentIndex(this.m_CurrentAdditionalWeapon_Repl) : -1);
		}

		public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
		{
			this.m_WeaponType_Repl = (HumanAI.WeaponType)reader.ReadInt32();
			GameObject gameObject = reader.ReadGameObject();
			int repl_behaviour_idx = reader.ReadInt32();
			this.m_PrimaryWeapon_Repl = (gameObject ? ((Item)gameObject.GetComponent<ReplicationComponent>().GetComponentFromIndex(repl_behaviour_idx)) : null);
			GameObject gameObject2 = reader.ReadGameObject();
			int repl_behaviour_idx2 = reader.ReadInt32();
			this.m_AdditionalWeapon_Repl = (gameObject2 ? ((Item)gameObject2.GetComponent<ReplicationComponent>().GetComponentFromIndex(repl_behaviour_idx2)) : null);
			GameObject gameObject3 = reader.ReadGameObject();
			int repl_behaviour_idx3 = reader.ReadInt32();
			this.m_SecondaryWeapon_Repl = (gameObject3 ? ((Item)gameObject3.GetComponent<ReplicationComponent>().GetComponentFromIndex(repl_behaviour_idx3)) : null);
			GameObject gameObject4 = reader.ReadGameObject();
			int repl_behaviour_idx4 = reader.ReadInt32();
			this.m_CurrentWeapon_Repl = (gameObject4 ? ((Item)gameObject4.GetComponent<ReplicationComponent>().GetComponentFromIndex(repl_behaviour_idx4)) : null);
			GameObject gameObject5 = reader.ReadGameObject();
			int repl_behaviour_idx5 = reader.ReadInt32();
			this.m_CurrentAdditionalWeapon_Repl = (gameObject5 ? ((Item)gameObject5.GetComponent<ReplicationComponent>().GetComponentFromIndex(repl_behaviour_idx5)) : null);
		}

		public virtual void OnReplicationResolve_CJGenerated()
		{
			this.m_WeaponType = this.m_WeaponType_Repl;
			this.m_PrimaryWeapon = this.m_PrimaryWeapon_Repl;
			this.m_AdditionalWeapon = this.m_AdditionalWeapon_Repl;
			this.m_SecondaryWeapon = this.m_SecondaryWeapon_Repl;
			this.m_CurrentWeapon = this.m_CurrentWeapon_Repl;
			this.m_CurrentAdditionalWeapon = this.m_CurrentAdditionalWeapon_Repl;
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

		public List<GameObject> m_AdditionalWeaponPrefabs;

		public List<GameObject> m_SecondaryWeaponPrefabs;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public HumanAI.WeaponType m_WeaponType = HumanAI.WeaponType.None;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public Item m_PrimaryWeapon;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public Item m_AdditionalWeapon;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public Item m_SecondaryWeapon;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public Item m_CurrentWeapon;

		[Replicate(new string[]
		{

		})]
		[NonSerialized]
		public Item m_CurrentAdditionalWeapon;

		[HideInInspector]
		public Construction m_SelectedConstruction;

		public Rigidbody m_KillerTrapJoint;

		private Transform m_BowHolder;

		private HumanAI.WeaponType m_WeaponType_Repl;

		private Item m_PrimaryWeapon_Repl;

		private Item m_AdditionalWeapon_Repl;

		private Item m_SecondaryWeapon_Repl;

		private Item m_CurrentWeapon_Repl;

		private Item m_CurrentAdditionalWeapon_Repl;

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
			Secondary,
			Additional
		}
	}
}
