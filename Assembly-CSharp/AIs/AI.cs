using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class AI : Being, INoiseReceiver, IAnimationEventsReceiver
	{
		public static bool IsHuman(AI.AIID id)
		{
			return id == AI.AIID.Savage || id == AI.AIID.Hunter || id == AI.AIID.Thug || id == AI.AIID.SpearMan || id == AI.AIID.Regular;
		}

		public static bool IsSnake(AI.AIID id)
		{
			return id == AI.AIID.SouthAmericanRattlesnake || id == AI.AIID.GreenAnaconda || id == AI.AIID.BoaConstrictor;
		}

		public static bool IsArmadillo(AI.AIID id)
		{
			return id == AI.AIID.Armadillo || id == AI.AIID.ArmadilloThreeBanded;
		}

		public static bool IsTurtle(AI.AIID id)
		{
			return id == AI.AIID.RedFootedTortoise || id == AI.AIID.MudTurtle;
		}

		public static bool IsCat(AI.AIID id)
		{
			return id == AI.AIID.Puma || id == AI.AIID.Jaguar;
		}

		public static bool IsCat(AI ai)
		{
			return ai.m_ID == AI.AIID.Puma || ai.m_ID == AI.AIID.Jaguar;
		}

		public bool IsCat()
		{
			return this.m_ID == AI.AIID.Puma || this.m_ID == AI.AIID.Jaguar;
		}

		protected override void Awake()
		{
			base.Awake();
			this.m_AnimationModule = base.gameObject.GetComponent<AnimationModule>();
			this.m_DamageModule = base.gameObject.GetComponent<DamageModule>();
			this.m_EnemyModule = base.gameObject.GetComponent<EnemyModule>();
			this.m_GoalsModule = base.gameObject.GetComponent<GoalsModule>();
			this.m_HealthModule = base.gameObject.GetComponent<HealthModule>();
			this.m_HearingModule = base.gameObject.GetComponent<HearingModule>();
			this.m_MoraleModule = base.gameObject.GetComponent<MoraleModule>();
			this.m_PathModule = base.gameObject.GetComponent<PathModule>();
			this.m_TransformModule = base.gameObject.GetComponent<TransformModule>();
			this.m_VisModule = base.gameObject.GetComponent<VisModule>();
			this.m_EnemySenseModule = base.gameObject.GetComponent<EnemySenseModule>();
			this.m_HostileStateModule = base.gameObject.GetComponent<HostileStateModule>();
			this.m_BodyRotationModule = base.gameObject.GetComponent<BodyRotationModule>();
			this.m_SnakeSoundModule = base.gameObject.GetComponent<SnakeSoundModule>();
			this.m_ArachnidSoundModule = base.gameObject.GetComponent<ArachnidSoundModule>();
			this.m_SightModule = base.gameObject.GetComponent<SightModule>();
			this.m_HumanFightModule = base.gameObject.GetComponent<HumanFightModule>();
			this.m_HumanLookModule = base.gameObject.GetComponent<HumanLookModule>();
			this.m_PatrolModule = base.gameObject.GetComponent<PatrolModule>();
			this.m_HumanAISoundModule = base.gameObject.GetComponent<HumanAISoundModule>();
			this.m_Animator = base.gameObject.GetComponent<Animator>();
			this.m_BoxCollider = base.GetComponent<BoxCollider>();
			float num = Mathf.Max(base.gameObject.transform.localScale.x, base.gameObject.transform.localScale.z);
			this.m_Radius = Mathf.Max(this.m_BoxCollider.size.x, this.m_BoxCollider.size.z) * num * 0.5f;
			this.m_Renderer = General.GetComponentDeepChild<SkinnedMeshRenderer>(base.gameObject);
			DebugUtils.Assert(this.m_Renderer != null, "Can't find renderer - " + this.m_ID.ToString(), true, DebugUtils.AssertType.Info);
			this.SetupColliderBoxes();
			this.SetupRagdollBones();
			this.m_Head = base.transform.FindDeepChild((this.m_ID != AI.AIID.Jaguar) ? "Head" : "Jaguar_Head");
		}

		protected override void Start()
		{
			this.m_Params = AIManager.Get().m_AIParamsMap[(int)this.m_ID];
			if (this.m_Params.m_BigAnimal)
			{
				this.m_SoundModule = base.gameObject.AddComponent<BigAnimalSoundModule>();
			}
			else if (this.m_ID == AI.AIID.Mouse || this.m_ID == AI.AIID.CaneToad)
			{
				this.m_SoundModule = base.gameObject.AddComponent<AISoundModule>();
			}
			if (this.m_SoundModule && this.m_Trap)
			{
				this.m_SoundModule.RequestSound(AISoundType.Panic);
			}
			AIManager.Get().RegisterAI(this);
			base.Start();
			this.m_StartExecuted = true;
			this.OnEnable();
			if (this.m_ID == AI.AIID.Jaguar)
			{
				BalanceSystem.Get().OnJaguarActivated();
			}
			AudioSource[] components = base.GetComponents<AudioSource>();
			foreach (AudioSource audioSource in components)
			{
				audioSource.rolloffMode = AudioRolloffMode.Linear;
			}
		}

		public override void InitializeModules()
		{
			if (this.m_ModulesInitialized)
			{
				return;
			}
			if (this.m_HearingModule)
			{
				this.m_HearingModule.Initialize();
			}
			if (this.m_SightModule)
			{
				this.m_SightModule.Initialize();
			}
			if (this.m_EnemySenseModule)
			{
				this.m_EnemySenseModule.Initialize();
			}
			if (this.m_EnemyModule)
			{
				this.m_EnemyModule.Initialize();
			}
			if (this.m_HealthModule)
			{
				this.m_HealthModule.Initialize();
			}
			if (this.m_MoraleModule)
			{
				this.m_MoraleModule.Initialize();
			}
			if (this.m_HostileStateModule)
			{
				this.m_HostileStateModule.Initialize();
			}
			if (this.m_DamageModule)
			{
				this.m_DamageModule.Initialize();
			}
			if (this.m_HumanFightModule)
			{
				this.m_HumanFightModule.Initialize();
			}
			if (this.m_HumanLookModule)
			{
				this.m_HumanLookModule.Initialize();
			}
			if (this.m_PathModule)
			{
				this.m_PathModule.Initialize();
			}
			if (this.m_PatrolModule)
			{
				this.m_PatrolModule.Initialize();
			}
			if (this.m_SnakeSoundModule)
			{
				this.m_SnakeSoundModule.Initialize();
			}
			if (this.m_HumanAISoundModule)
			{
				this.m_HumanAISoundModule.Initialize();
			}
			if (this.m_ArachnidSoundModule)
			{
				this.m_ArachnidSoundModule.Initialize();
			}
			if (this.m_AnimationModule)
			{
				this.m_AnimationModule.Initialize();
			}
			if (this.m_BodyRotationModule)
			{
				this.m_BodyRotationModule.Initialize();
			}
			if (this.m_TransformModule)
			{
				this.m_TransformModule.Initialize();
			}
			if (this.m_VisModule)
			{
				this.m_VisModule.Initialize();
			}
			if (this.m_SoundModule)
			{
				this.m_SoundModule.Initialize();
			}
			if (this.m_GoalsModule)
			{
				this.m_GoalsModule.Initialize();
			}
			this.m_ModulesInitialized = true;
		}

		private void SetupRagdollBones()
		{
			List<Rigidbody> componentsDeepChild = General.GetComponentsDeepChild<Rigidbody>(base.gameObject);
			for (int i = 0; i < componentsDeepChild.Count; i++)
			{
				componentsDeepChild[i].isKinematic = true;
				Collider component = componentsDeepChild[i].GetComponent<Collider>();
				RagdollBone ragdollBone = component.gameObject.AddComponent<RagdollBone>();
				ragdollBone.m_Parent = base.gameObject;
				component.enabled = false;
				component.isTrigger = true;
				this.m_RagdollBones.Add(component, componentsDeepChild[i]);
			}
		}

		public GameObject GetObject()
		{
			return base.gameObject;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			AIManager.Get().UnregisterAI(this);
			if (this.m_Spawner != null)
			{
				this.m_Spawner.OnDestroyAI(this);
				this.m_Spawner = null;
			}
		}

		protected override void OnEnable()
		{
			if (!this.m_StartExecuted)
			{
				return;
			}
			base.OnEnable();
			AIManager.Get().OnEnableAI(this);
			NoiseManager.RegisterReceiver(this);
			if (!this.IsHuman() && !this.IsCat())
			{
				Physics.IgnoreCollision(Player.Get().GetComponent<Collider>(), this.m_BoxCollider);
				this.m_BoxCollider.isTrigger = true;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			AIManager.Get().OnDisableAI(this);
			NoiseManager.UnregisterReceiver(this);
			this.m_Visible = false;
		}

		public virtual bool IsHuman()
		{
			return false;
		}

		public virtual bool IsHunter()
		{
			return false;
		}

		public virtual bool IsFish()
		{
			return false;
		}

		public virtual bool IsStringray()
		{
			return false;
		}

		private bool CanJumpBack()
		{
			if (this.m_GoalsModule.m_JumpBackGoal == null)
			{
				return false;
			}
			AIGoal activeGoal = this.m_GoalsModule.m_ActiveGoal;
			if (activeGoal == null)
			{
				return false;
			}
			AIGoalType type = activeGoal.m_Type;
			if (type == AIGoalType.JumpBack)
			{
				return false;
			}
			if ((type == AIGoalType.ReactOnHit || type == AIGoalType.HumanPunchBack) && this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f)
			{
				return false;
			}
			float num = base.transform.position.Distance(Player.Get().transform.position);
			return num <= this.m_Params.m_JumpBackRange && UnityEngine.Random.Range(0f, 1f) <= this.m_GoalsModule.m_JumpBackGoal.m_Probability;
		}

		private bool TryJumpBack()
		{
			if (!this.CanJumpBack())
			{
				return false;
			}
			this.m_GoalsModule.ActivateGoal(AIGoalType.JumpBack);
			return true;
		}

		public virtual void OnPlayerStartAttack()
		{
			if (this.IsCat())
			{
				this.TryJumpBack();
			}
		}

		public override void OnImpaleOnSpear()
		{
			base.OnImpaleOnSpear();
			List<BeingModule> list = new List<BeingModule>();
			base.GetComponents<BeingModule>(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = false;
			}
			Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
			base.enabled = false;
		}

		private void SetupColliderBoxes()
		{
			if (this.m_ColliderBoxesScript.Length == 0)
			{
				BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
				if (component)
				{
					OBB obb = new OBB();
					obb.half_sizes = component.bounds.extents;
					obb.start = base.gameObject.transform.InverseTransformPoint(component.bounds.center);
					obb.transform = base.transform;
					this.m_ColliderBoxes.Add(obb);
				}
			}
			else
			{
				this.ParseColliderBoxesScript(this.m_ColliderBoxesScript);
			}
		}

		private void ParseColliderBoxesScript(string script_name)
		{
			ScriptParser scriptParser = new ScriptParser();
			scriptParser.Parse("ColliderBoxes/" + script_name + ".txt", true);
			for (int i = 0; i < scriptParser.GetKeysCount(); i++)
			{
				Key key = scriptParser.GetKey(i);
				if (key.GetName() == "Bone")
				{
					OBB obb = new OBB();
					Vector3 vector = new Vector3
					{
						x = key.GetVariable(1).FValue,
						y = key.GetVariable(2).FValue,
						z = key.GetVariable(3).FValue
					};
					obb.half_sizes = vector;
					vector.x = key.GetVariable(4).FValue;
					vector.y = key.GetVariable(5).FValue;
					vector.z = key.GetVariable(6).FValue;
					obb.start = vector;
					Transform transform = base.gameObject.transform.FindDeepChild(key.GetVariable(0).SValue);
					obb.transform = transform;
					this.m_ColliderBoxes.Add(obb);
				}
			}
		}

		public override bool IsAI()
		{
			return true;
		}

		public override float GetHealth()
		{
			DebugUtils.Assert(this.m_HealthModule != null, "ERRO - m_HealthModule = null!", true, DebugUtils.AssertType.Info);
			return this.m_HealthModule.GetHealth();
		}

		public virtual void OnNoise(Noise noise)
		{
			if (!this.m_ModulesInitialized)
			{
				return;
			}
			if (this.m_HearingModule)
			{
				this.m_HearingModule.OnNoise(noise);
			}
		}

		public void StartRagdoll()
		{
			this.m_BoxCollider.isTrigger = true;
			foreach (Collider collider in this.m_RagdollBones.Keys)
			{
				this.m_RagdollBones[collider].isKinematic = false;
				collider.isTrigger = false;
				collider.enabled = true;
			}
		}

		public override bool IsInWater()
		{
			return this.m_InWater;
		}

		private void UpdateInWater()
		{
			this.m_InWater = false;
			int num = Physics.OverlapBoxNonAlloc(base.transform.TransformPoint(this.m_BoxCollider.center), this.m_BoxCollider.size, this.m_CollidersTemp, Quaternion.identity);
			for (int i = 0; i < num; i++)
			{
				Collider collider = this.m_CollidersTemp[i];
				if (collider.gameObject.CompareTag(this.m_LiquidSourceName))
				{
					this.m_InWater = true;
					return;
				}
			}
		}

		protected override void Update()
		{
			if (HUDChallengeResult.Get().enabled)
			{
				return;
			}
			this.UpdateModules();
			int qualityLevel = QualitySettings.GetQualityLevel();
			if (qualityLevel == 4)
			{
				this.UpdateInWater();
			}
			if (this.m_Trap)
			{
				this.m_Trap.UpdateEffect();
			}
			if (this.IsCat())
			{
				float num = 0.2f;
				Vector3 size = this.m_BoxCollider.size;
				size.y = Mathf.Abs(this.m_Head.transform.position.y - base.transform.position.y) + num;
				this.m_BoxCollider.size = size;
				Vector3 center = this.m_BoxCollider.center;
				center.y = size.y * 0.5f;
				this.m_BoxCollider.center = center;
			}
			this.UpdateInvisibleDuration();
		}

		private void UpdateInvisibleDuration()
		{
			if (!this.m_Renderer.isVisible)
			{
				this.m_InvisibleDuration += Time.deltaTime;
			}
			else
			{
				this.m_InvisibleDuration = 0f;
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			this.LateUpdateModules();
		}

		private void UpdateModules()
		{
			if (this.m_HearingModule && this.m_HearingModule.enabled)
			{
				this.m_HearingModule.OnUpdate();
			}
			if (this.m_SightModule && this.m_SightModule.enabled)
			{
				this.m_SightModule.OnUpdate();
			}
			if (this.m_EnemySenseModule && this.m_EnemySenseModule.enabled)
			{
				this.m_EnemySenseModule.OnUpdate();
			}
			if (this.m_EnemyModule && this.m_EnemyModule.enabled)
			{
				this.m_EnemyModule.OnUpdate();
			}
			if (this.m_HealthModule && this.m_HealthModule.enabled)
			{
				this.m_HealthModule.OnUpdate();
			}
			if (this.m_MoraleModule && this.m_MoraleModule.enabled)
			{
				this.m_MoraleModule.OnUpdate();
			}
			if (this.m_HostileStateModule && this.m_HostileStateModule.enabled)
			{
				this.m_HostileStateModule.OnUpdate();
			}
			if (this.m_DamageModule && this.m_DamageModule.enabled)
			{
				this.m_DamageModule.OnUpdate();
			}
			if (this.m_HumanFightModule && this.m_HumanFightModule.enabled)
			{
				this.m_HumanFightModule.Update();
			}
			if (this.m_HumanLookModule && this.m_HumanLookModule.enabled)
			{
				this.m_HumanLookModule.Update();
			}
			if (this.m_GoalsModule && this.m_GoalsModule.enabled)
			{
				this.m_GoalsModule.OnUpdate();
			}
			if (this.m_PathModule && this.m_PathModule.enabled)
			{
				this.m_PathModule.OnUpdate();
			}
			if (this.m_PatrolModule && this.m_PatrolModule.enabled)
			{
				this.m_PatrolModule.OnUpdate();
			}
			if (this.m_SnakeSoundModule && this.m_SnakeSoundModule.enabled)
			{
				this.m_SnakeSoundModule.OnUpdate();
			}
			if (this.m_HumanAISoundModule && this.m_HumanAISoundModule.enabled)
			{
				this.m_HumanAISoundModule.OnUpdate();
			}
			if (this.m_ArachnidSoundModule && this.m_ArachnidSoundModule.enabled)
			{
				this.m_ArachnidSoundModule.OnUpdate();
			}
			if (this.m_AnimationModule && this.m_AnimationModule.enabled)
			{
				this.m_AnimationModule.OnUpdate();
			}
			if (this.m_BodyRotationModule && this.m_BodyRotationModule.enabled)
			{
				this.m_BodyRotationModule.OnUpdate();
			}
			if (this.m_TransformModule && this.m_TransformModule.enabled)
			{
				this.m_TransformModule.OnUpdate();
			}
			if (this.m_VisModule && this.m_VisModule.enabled)
			{
				this.m_VisModule.OnUpdate();
			}
			if (this.m_SoundModule && this.m_SoundModule.enabled)
			{
				this.m_SoundModule.OnUpdate();
			}
		}

		private void LateUpdateModules()
		{
			if (this.m_HearingModule && this.m_HearingModule.enabled)
			{
				this.m_HearingModule.OnLateUpdate();
			}
			if (this.m_SightModule && this.m_SightModule.enabled)
			{
				this.m_SightModule.OnLateUpdate();
			}
			if (this.m_EnemySenseModule && this.m_EnemySenseModule.enabled)
			{
				this.m_EnemySenseModule.OnLateUpdate();
			}
			if (this.m_EnemyModule && this.m_EnemyModule.enabled)
			{
				this.m_EnemyModule.OnLateUpdate();
			}
			if (this.m_HealthModule && this.m_HealthModule.enabled)
			{
				this.m_HealthModule.OnLateUpdate();
			}
			if (this.m_MoraleModule && this.m_MoraleModule.enabled)
			{
				this.m_MoraleModule.OnLateUpdate();
			}
			if (this.m_HostileStateModule && this.m_HostileStateModule.enabled)
			{
				this.m_HostileStateModule.OnLateUpdate();
			}
			if (this.m_DamageModule && this.m_DamageModule.enabled)
			{
				this.m_DamageModule.OnLateUpdate();
			}
			if (this.m_HumanFightModule && this.m_HumanFightModule.enabled)
			{
				this.m_HumanFightModule.OnLateUpdate();
			}
			if (this.m_HumanLookModule && this.m_HumanLookModule.enabled)
			{
				this.m_HumanLookModule.OnLateUpdate();
			}
			if (this.m_GoalsModule && this.m_GoalsModule.enabled)
			{
				this.m_GoalsModule.OnLateUpdate();
			}
			if (this.m_PathModule && this.m_PathModule.enabled)
			{
				this.m_PathModule.OnLateUpdate();
			}
			if (this.m_PatrolModule && this.m_PatrolModule.enabled)
			{
				this.m_PatrolModule.OnLateUpdate();
			}
			if (this.m_SnakeSoundModule && this.m_SnakeSoundModule.enabled)
			{
				this.m_SnakeSoundModule.OnLateUpdate();
			}
			if (this.m_HumanAISoundModule && this.m_HumanAISoundModule.enabled)
			{
				this.m_HumanAISoundModule.OnLateUpdate();
			}
			if (this.m_ArachnidSoundModule && this.m_ArachnidSoundModule.enabled)
			{
				this.m_ArachnidSoundModule.OnLateUpdate();
			}
			if (this.m_AnimationModule && this.m_AnimationModule.enabled)
			{
				this.m_AnimationModule.OnLateUpdate();
			}
			if (this.m_BodyRotationModule && this.m_BodyRotationModule.enabled)
			{
				this.m_BodyRotationModule.OnLateUpdate();
			}
			if (this.m_TransformModule && this.m_TransformModule.enabled)
			{
				this.m_TransformModule.OnLateUpdate();
			}
			if (this.m_VisModule && this.m_VisModule.enabled)
			{
				this.m_VisModule.OnLateUpdate();
			}
			if (this.m_SoundModule && this.m_SoundModule.enabled)
			{
				this.m_SoundModule.OnLateUpdate();
			}
		}

		private void UpdateLeavesPusher()
		{
			LeavesPusher.Get().Push(this.m_BoxCollider.bounds.center, this.m_Radius);
		}

		public override HitCollisionType GetHitCollisionType()
		{
			return HitCollisionType.Bones;
		}

		public override List<OBB> GetColliderBoxes()
		{
			return this.m_ColliderBoxes;
		}

		public override bool IsDead()
		{
			return !this.m_HealthModule || this.m_HealthModule.m_IsDead;
		}

		public void AddDeadBodyComponent()
		{
			DeadBody deadBody = base.gameObject.AddComponent<DeadBody>();
			deadBody.m_AddHarvestingItem = this.m_AddHarvestingItem;
			deadBody.m_AIID = this.m_ID;
			deadBody.m_Trap = this.m_Trap;
			deadBody.m_RagdollBones = new List<Rigidbody>();
			foreach (Collider key in this.m_RagdollBones.Keys)
			{
				deadBody.m_RagdollBones.Add(this.m_RagdollBones[key]);
			}
			deadBody.m_AI = this;
		}

		public virtual void OnDie()
		{
			this.m_HasBody = GreenHellGame.Instance.GetPrefab(this.m_ID.ToString() + "_Body");
			if (!this.m_HasBody)
			{
				this.AddDeadBodyComponent();
			}
			List<BeingModule> list = new List<BeingModule>();
			base.GetComponents<BeingModule>(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].OnDie();
			}
			if (this.m_Params.m_BigAnimal || this.m_Params.m_Human)
			{
				Component[] components = base.GetComponents(typeof(Component));
				for (int j = 0; j < components.Length; j++)
				{
					Type type = components[j].GetType();
					if (type != typeof(Transform) && type != typeof(BoxCollider) && type != typeof(DeadBody) && type != typeof(AudioSource) && type != typeof(VisModule))
					{
						UnityEngine.Object.Destroy(components[j]);
					}
				}
				this.m_BoxCollider.center = base.transform.InverseTransformPoint(this.m_Renderer.bounds.center);
				this.m_BoxCollider.size = this.m_Renderer.bounds.size;
				this.StartRagdoll();
			}
			if (this.m_Spawner != null)
			{
				this.m_Spawner.OnDestroyAI(this);
				this.m_Spawner = null;
			}
		}

		public void SetAttractor(AIAttractor attr)
		{
			this.m_Attractor = attr;
			this.m_Attractor.m_Occupied = true;
		}

		public void ReleaseAttractor()
		{
			this.m_Attractor.m_Occupied = false;
			this.m_Attractor = null;
		}

		public float GetAttackRange(Being enemy)
		{
			return this.m_Radius + enemy.m_Radius + this.m_Params.m_AttackRange;
		}

		public override string GetName()
		{
			return base.name;
		}

		public override bool IsRunning()
		{
			return this.m_MoveStyle == AIMoveStyle.Run;
		}

		public override bool IsWalking()
		{
			return this.m_MoveStyle == AIMoveStyle.Walk;
		}

		public virtual bool CheckActivityByDistance()
		{
			return true;
		}

		public virtual bool DestroyOnLeaveActivationRange()
		{
			return !this.m_Trap;
		}

		public override bool CanReceiveDamageOfType(DamageType damage_type)
		{
			return true;
		}

		public bool IsActive()
		{
			return base.enabled;
		}

		public virtual bool ForceReceiveAnimEvent()
		{
			return false;
		}

		public void OnAnimEvent(AnimEventID id)
		{
			if (id == AnimEventID.DieEnd && this.m_HasBody)
			{
				Item item = ItemsManager.Get().CreateItem(this.m_ID.ToString() + "_Body", false);
				item.transform.position = base.transform.position;
				item.transform.rotation = base.transform.rotation;
				if (this.m_ID == AI.AIID.PoisonDartFrog)
				{
					List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
					Material material = null;
					for (int i = 0; i < componentsDeepChild.Count; i++)
					{
						material = componentsDeepChild[i].material;
					}
					item.ApplyMaterial(material);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public Vector3 GetClosestPointOnMesh(Vector3 point, Vector3 dir = default(Vector3))
		{
			Mesh mesh = new Mesh();
			this.m_Renderer.BakeMesh(mesh);
			Vector3 a = this.m_Renderer.transform.InverseTransformPoint(point);
			float num = float.PositiveInfinity;
			Vector3 position = Vector3.zero;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				vector = mesh.vertices[i];
				float sqrMagnitude = (a - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					position = vector;
				}
			}
			return this.m_Renderer.transform.TransformPoint(position);
		}

		public Transform GetClosestBone(Vector3 point)
		{
			float num = float.MaxValue;
			Transform result = null;
			for (int i = 0; i < this.m_Renderer.bones.Length; i++)
			{
				float sqrMagnitude = (this.m_Renderer.bones[i].position - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = this.m_Renderer.bones[i];
					num = sqrMagnitude;
				}
			}
			return result;
		}

		public Transform GetClosestRagdollBone(Vector3 point)
		{
			float num = float.MaxValue;
			Transform result = null;
			foreach (Collider collider in this.m_RagdollBones.Keys)
			{
				float sqrMagnitude = (collider.transform.position - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = collider.transform;
					num = sqrMagnitude;
				}
			}
			return result;
		}

		public float GetPathPassDistance()
		{
			float num = this.m_Radius;
			if (this.m_Params.m_Human)
			{
				num += Player.Get().m_Radius + 1f;
			}
			if (this.m_ID == AI.AIID.BlackCaiman)
			{
				num = Mathf.Min(num, 0.3f);
			}
			if (this.m_ID == AI.AIID.PoisonDartFrog)
			{
				num = Mathf.Min(num, 1f);
			}
			return num;
		}

		public virtual void SwitchWeapon()
		{
		}

		public virtual void OnPlayerDie()
		{
		}

		[HideInInspector]
		public AI.AIID m_ID = AI.AIID.None;

		[HideInInspector]
		public Animator m_Animator;

		private List<OBB> m_ColliderBoxes = new List<OBB>();

		[HideInInspector]
		public HealthModule m_HealthModule;

		[HideInInspector]
		public EnemyModule m_EnemyModule;

		[HideInInspector]
		public HearingModule m_HearingModule;

		[HideInInspector]
		public AnimationModule m_AnimationModule;

		[HideInInspector]
		public GoalsModule m_GoalsModule;

		[HideInInspector]
		public TransformModule m_TransformModule;

		[HideInInspector]
		public MoraleModule m_MoraleModule;

		[HideInInspector]
		public DamageModule m_DamageModule;

		[HideInInspector]
		public VisModule m_VisModule;

		[HideInInspector]
		public PathModule m_PathModule;

		[HideInInspector]
		public EnemySenseModule m_EnemySenseModule;

		[HideInInspector]
		public HostileStateModule m_HostileStateModule;

		[HideInInspector]
		public BodyRotationModule m_BodyRotationModule;

		[HideInInspector]
		public SnakeSoundModule m_SnakeSoundModule;

		[HideInInspector]
		public ArachnidSoundModule m_ArachnidSoundModule;

		[HideInInspector]
		public SightModule m_SightModule;

		[HideInInspector]
		public AISoundModule m_SoundModule;

		[HideInInspector]
		public HumanFightModule m_HumanFightModule;

		[HideInInspector]
		public HumanLookModule m_HumanLookModule;

		[HideInInspector]
		public PatrolModule m_PatrolModule;

		[HideInInspector]
		public HumanAISoundModule m_HumanAISoundModule;

		[HideInInspector]
		public AIAttractor m_Attractor;

		[HideInInspector]
		public AIParams m_Params;

		[HideInInspector]
		public Trap m_Trap;

		[HideInInspector]
		public AISpawner m_Spawner;

		[HideInInspector]
		public AIMoveStyle m_MoveStyle = AIMoveStyle.None;

		[HideInInspector]
		public float m_LastAttackTime;

		[HideInInspector]
		public BoxCollider m_BoxCollider;

		[HideInInspector]
		public SkinnedMeshRenderer m_Renderer;

		[HideInInspector]
		public bool m_StartExecuted;

		[HideInInspector]
		public Dictionary<Collider, Rigidbody> m_RagdollBones = new Dictionary<Collider, Rigidbody>();

		[HideInInspector]
		public bool m_PerformEmergency;

		private bool m_InWater;

		public string m_PresetName = string.Empty;

		public ItemID m_AddHarvestingItem = ItemID.None;

		[HideInInspector]
		public AI.SoundPreset m_SoundPreset = AI.SoundPreset.None;

		[HideInInspector]
		public float m_InvisibleDuration;

		private string m_LiquidSourceName = "LiquidSource";

		private Collider[] m_CollidersTemp = new Collider[10];

		private bool m_HasBody;

		public bool m_Visible = true;

		public enum AIID
		{
			None = -1,
			Puma,
			Jaguar,
			BlackPanther,
			Stalker,
			BlackCaiman,
			Piranha,
			GreenAnaconda,
			BoaConstrictor,
			SouthAmericanRattlesnake,
			GoliathBirdEater,
			BrasilianWanderingSpider,
			Scorpion,
			Mouse,
			Agouti,
			Peccary,
			Capybara,
			Tapir,
			Armadillo,
			GoldenLionTamarin,
			Atelinae,
			Prawn,
			Stingray,
			Arowana,
			PeacockBass,
			VampireFish,
			PoisonDartFrog,
			CaneToad,
			MudTurtle,
			RedFootedTortoise,
			GreenIguana,
			Centipede,
			Caterpillar,
			Beetle,
			Regular,
			Hunter,
			SpearMan,
			Thug,
			Savage,
			ArmadilloThreeBanded,
			CaimanLizard,
			Count
		}

		public enum SoundPreset
		{
			None = -1,
			Tribe0,
			Tribe1,
			Tribe2,
			Count
		}
	}
}
