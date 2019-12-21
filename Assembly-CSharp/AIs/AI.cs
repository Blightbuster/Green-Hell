using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class AI : Being, INoiseReceiver, IAnimationEventsReceiver
	{
		public static bool IsHuntingTarget(AI.AIID id)
		{
			return id == AI.AIID.Agouti || id == AI.AIID.Peccary || id == AI.AIID.Capybara || id == AI.AIID.Tapir || id == AI.AIID.Armadillo || id == AI.AIID.GoldenLionTamarin || id == AI.AIID.Atelinae || id == AI.AIID.GreenIguana;
		}

		public static bool IsHuman(AI.AIID id)
		{
			return id == AI.AIID.Savage || id == AI.AIID.Hunter || id == AI.AIID.Thug || id == AI.AIID.Spearman || id == AI.AIID.Regular;
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

		public bool IsCaiman()
		{
			return this.m_ID == AI.AIID.BlackCaiman;
		}

		public bool IsEnemy()
		{
			return this.IsHuman() || this.IsCaiman() || this.IsCat();
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
			this.m_WaterBoxModule = base.gameObject.GetComponent<WaterBoxModule>();
			this.m_KidRunnerSoundModule = base.gameObject.GetComponent<KidRunnerSoundModule>();
			this.m_Animator = base.gameObject.GetComponent<Animator>();
			this.m_BoxCollider = base.GetComponent<BoxCollider>();
			float num = Mathf.Max(base.gameObject.transform.localScale.x, base.gameObject.transform.localScale.z);
			this.m_Radius = Mathf.Max(this.m_BoxCollider.size.x, this.m_BoxCollider.size.z) * num * 0.5f;
			this.m_Renderer = General.GetComponentDeepChild<SkinnedMeshRenderer>(base.gameObject);
			DebugUtils.Assert(this.m_Renderer != null, "Can't find renderer - " + this.m_ID.ToString(), true, DebugUtils.AssertType.Info);
			this.SetupColliderBoxes();
			this.m_Head = base.transform.FindDeepChild((this.m_ID == AI.AIID.Jaguar) ? "Jaguar_Head" : "Head");
			this.m_WaterMask = 1 << NavMesh.GetAreaFromName("Water");
		}

		protected override void Start()
		{
			this.m_Params = AIManager.Get().m_AIParamsMap[(int)this.m_ID];
			if (this.m_Params.m_BigAnimal)
			{
				if (this.m_ID == AI.AIID.BlackCaiman)
				{
					this.m_SoundModule = base.gameObject.AddComponent<BlackCaimanSoundModule>();
				}
				else
				{
					this.m_SoundModule = base.gameObject.AddComponent<BigAnimalSoundModule>();
				}
			}
			else if (this.m_ID == AI.AIID.Mouse || this.m_ID == AI.AIID.CaneToad)
			{
				this.m_SoundModule = base.gameObject.AddComponent<AISoundModule>();
			}
			if (this.m_SoundModule && this.m_Trap && (this.m_Trap.m_Effect == Trap.Effect.Block || this.m_Trap.m_Info.m_ID == ItemID.Snare_Trap))
			{
				this.m_SoundModule.RequestSound(AISoundType.Panic);
			}
			AIManager.Get().RegisterAI(this);
			base.Start();
			this.m_StartExecuted = true;
			this.OnEnable();
			if (this.IsCat())
			{
				EnemyAISpawnManager.Get().OnActivatePredator(this);
			}
			AudioSource[] components = base.GetComponents<AudioSource>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].rolloffMode = AudioRolloffMode.Linear;
			}
			this.UpdateSwimming();
			if (this.m_StartAttractor)
			{
				this.SetAttractor(this.m_StartAttractor);
			}
			if (this.m_ID == AI.AIID.PoisonDartFrog || this.m_ID == AI.AIID.CaneToad)
			{
				this.m_BoxCollider.isTrigger = true;
			}
		}

		public override void InitializeModules()
		{
			if (this.m_ModulesInitialized)
			{
				return;
			}
			base.GetComponents<BeingModule>(this.m_Modules);
			if (this.m_HearingModule)
			{
				this.m_HearingModule.Initialize(this);
			}
			if (this.m_SightModule)
			{
				this.m_SightModule.Initialize(this);
			}
			if (this.m_EnemySenseModule)
			{
				this.m_EnemySenseModule.Initialize(this);
			}
			if (this.m_EnemyModule)
			{
				this.m_EnemyModule.Initialize(this);
			}
			if (this.m_HealthModule)
			{
				this.m_HealthModule.Initialize(this);
			}
			if (this.m_MoraleModule)
			{
				this.m_MoraleModule.Initialize(this);
			}
			if (this.m_HostileStateModule)
			{
				this.m_HostileStateModule.Initialize(this);
			}
			if (this.m_DamageModule)
			{
				this.m_DamageModule.Initialize(this);
			}
			if (this.m_HumanFightModule)
			{
				this.m_HumanFightModule.Initialize(this);
			}
			if (this.m_HumanLookModule)
			{
				this.m_HumanLookModule.Initialize(this);
			}
			if (this.m_PathModule)
			{
				this.m_PathModule.Initialize(this);
			}
			if (this.m_PatrolModule)
			{
				this.m_PatrolModule.Initialize(this);
			}
			if (this.m_SnakeSoundModule)
			{
				this.m_SnakeSoundModule.Initialize(this);
			}
			if (this.m_HumanAISoundModule)
			{
				this.m_HumanAISoundModule.Initialize(this);
			}
			if (this.m_ArachnidSoundModule)
			{
				this.m_ArachnidSoundModule.Initialize(this);
			}
			if (this.m_AnimationModule)
			{
				this.m_AnimationModule.Initialize(this);
			}
			if (this.m_BodyRotationModule)
			{
				this.m_BodyRotationModule.Initialize(this);
			}
			if (this.m_TransformModule)
			{
				this.m_TransformModule.Initialize(this);
			}
			if (this.m_VisModule)
			{
				this.m_VisModule.Initialize(this);
			}
			if (this.m_SoundModule)
			{
				this.m_SoundModule.Initialize(this);
			}
			if (this.m_GoalsModule)
			{
				this.m_GoalsModule.Initialize(this);
			}
			if (this.m_KidRunnerSoundModule)
			{
				this.m_KidRunnerSoundModule.Initialize(this);
			}
			this.m_ModulesInitialized = true;
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
			if (!this.IsHuman() && !this.IsCat() && !this.IsCaiman() && !this.IsStringray())
			{
				Physics.IgnoreCollision(Player.Get().m_Collider, this.m_BoxCollider);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			AIManager.Get().OnDisableAI(this);
			NoiseManager.UnregisterReceiver(this);
			if (this.IsCat())
			{
				EnemyAISpawnManager.Get().OnDeactivatePredator(this);
			}
		}

		public virtual bool IsKidRunner()
		{
			return false;
		}

		public virtual bool IsHuman()
		{
			return false;
		}

		public virtual bool IsHunter()
		{
			return false;
		}

		public virtual bool IsStringray()
		{
			return false;
		}

		public override bool CanBeImpaledOnSpear()
		{
			return this.m_ID == AI.AIID.Crab;
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
			return type != AIGoalType.JumpBack && ((type != AIGoalType.ReactOnHit && type != AIGoalType.HumanPunchBack) || this.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f) && base.transform.position.Distance(this.m_EnemyModule.m_Enemy.transform.position) <= this.m_Params.m_JumpBackRange && UnityEngine.Random.Range(0f, 1f) <= this.m_GoalsModule.m_JumpBackGoal.m_Probability;
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
					return;
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
			foreach (RagdollBone ragdollBone in this.m_RagdollBones)
			{
				ragdollBone.m_Rigidbody.isKinematic = false;
				ragdollBone.m_Rigidbody.detectCollisions = true;
				ragdollBone.m_Collider.isTrigger = false;
				ragdollBone.m_Collider.enabled = true;
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
				if (this.m_CollidersTemp[i].gameObject.CompareTag(this.m_LiquidSourceName))
				{
					this.m_InWater = true;
					return;
				}
			}
		}

		public bool IsSwimming()
		{
			return this.m_IsSwimming;
		}

		private void UpdateSwimming()
		{
			if (!this.m_Params.m_CanSwim)
			{
				return;
			}
			float maxDistance = 5f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(base.transform.position, out navMeshHit, maxDistance, this.m_WaterMask))
			{
				this.m_IsSwimming = (navMeshHit.mask == this.m_WaterMask);
				this.m_SwimmingPos = navMeshHit.position;
				if (this.IsSwimming() && this.m_WaterBoxModule)
				{
					float waterLevel = this.m_WaterBoxModule.GetWaterLevel();
					if (waterLevel < -10000f)
					{
						this.m_IsSwimming = false;
						return;
					}
					if (this.m_TransformModule.m_TempTerrainPos != Vector3.zero && waterLevel - this.m_TransformModule.m_TempTerrainPos.y < 0.9f)
					{
						this.m_IsSwimming = false;
						return;
					}
				}
			}
			else
			{
				this.m_IsSwimming = false;
			}
		}

		protected override void Update()
		{
			if (HUDChallengeResult.Get().enabled)
			{
				return;
			}
			this.UpdateModules();
			if (QualitySettings.GetQualityLevel() == 4)
			{
				this.UpdateInWater();
			}
			this.UpdateSwimming();
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
			float num2 = Mathf.Max(base.gameObject.transform.localScale.x, base.gameObject.transform.localScale.z);
			this.m_Radius = Mathf.Max(this.m_BoxCollider.size.x, this.m_BoxCollider.size.z) * num2 * 0.5f;
		}

		private void UpdateInvisibleDuration()
		{
			if (!this.m_Renderer.isVisible)
			{
				this.m_InvisibleDuration += Time.deltaTime;
				return;
			}
			this.m_InvisibleDuration = 0f;
		}

		private void UpdateBleedingDamage()
		{
			if (this.m_BleedingDamage == 0f || !this.m_HealthModule)
			{
				return;
			}
			this.m_HealthModule.DecreaseHealth(this.m_BleedingDamage * Time.deltaTime);
		}

		public void OnStickWeapon(Weapon weapon)
		{
			this.m_BleedingDamage = Mathf.Max(this.m_BleedingDamage, ((WeaponInfo)weapon.m_Info).m_DamageOverTime);
			this.m_BleedingDamage = Mathf.Max(this.m_BleedingDamage, 0f);
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			this.LateUpdateModules();
			this.UpdateBleedingDamage();
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
			if (this.m_KidRunnerSoundModule)
			{
				this.m_KidRunnerSoundModule.OnUpdate();
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
			if (this.m_KidRunnerSoundModule)
			{
				this.m_KidRunnerSoundModule.OnLateUpdate();
			}
		}

		private void UpdateLeavesPusher()
		{
			LeavesPusher.Get().Push(base.gameObject, this.m_Radius, new Vector3?(this.m_BoxCollider.bounds.center - base.transform.position));
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
			foreach (RagdollBone ragdollBone in this.m_RagdollBones)
			{
				deadBody.m_RagdollBones.Add(ragdollBone.m_Rigidbody);
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
				foreach (Behaviour behaviour in base.GetComponents<Behaviour>())
				{
					Type type = behaviour.GetType();
					if (Array.IndexOf<Type>(AI.s_DontRemoveComponents, type) == -1)
					{
						if (behaviour is IReplicatedBehaviour)
						{
							behaviour.enabled = false;
						}
						else
						{
							this.DestroyComponentWithEvent(behaviour);
						}
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
			if (this.IsCat())
			{
				EnemyAISpawnManager.Get().OnDeactivatePredator(this);
			}
		}

		public void SetAttractor(AIAttractor attr)
		{
			this.m_Attractor = attr;
			this.m_Attractor.m_Occupied = true;
		}

		public virtual void ReleaseAttractor()
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
					Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
					Material material = null;
					for (int i = 0; i < componentsDeepChild.Length; i++)
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

		public RagdollBone GetHeadRagdollBone()
		{
			foreach (RagdollBone ragdollBone in this.m_RagdollBones)
			{
				if (ragdollBone.m_BoneType == RagdollBone.BoneType.Human_Head)
				{
					return ragdollBone;
				}
			}
			return null;
		}

		public RagdollBone GetClosestRagdollBone(Vector3 point)
		{
			float num = float.MaxValue;
			RagdollBone result = null;
			foreach (RagdollBone ragdollBone in this.m_RagdollBones)
			{
				bool enabled = ragdollBone.m_Collider.enabled;
				ragdollBone.m_Collider.enabled = true;
				float sqrMagnitude = (ragdollBone.m_Collider.ClosestPoint(point) - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = ragdollBone;
					num = sqrMagnitude;
				}
				ragdollBone.m_Collider.enabled = enabled;
			}
			return result;
		}

		public override bool TakeDamage(DamageInfo info)
		{
			RagdollBone closestRagdollBone = this.GetClosestRagdollBone(info.m_Position);
			if (closestRagdollBone)
			{
				info.m_Damage *= closestRagdollBone.GetDamageMultiplier(false);
			}
			this.m_LastDamageInfo = info;
			bool flag = base.TakeDamage(info);
			if (flag && info.m_Damager && info.m_Damager.GetComponent<ReplicatedLogicalPlayer>())
			{
				PlayerStateModule.Get().OnGiveDamageToAI(this, info);
			}
			return flag;
		}

		public float GetPathPassDistance()
		{
			if (this.IsKidRunner())
			{
				return this.m_Radius;
			}
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

		public bool IsVisible()
		{
			return this.m_Renderer.isVisible;
		}

		private void OnTriggerEnter(Collider other)
		{
			WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
			if (component && this.m_WaterBoxModule)
			{
				this.m_WaterBoxModule.OnEnterWater(component);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
			if (component && this.m_WaterBoxModule)
			{
				this.m_WaterBoxModule.OnExitWater(component);
			}
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
		public WaterBoxModule m_WaterBoxModule;

		[HideInInspector]
		public KidRunnerSoundModule m_KidRunnerSoundModule;

		[HideInInspector]
		public AIAttractor m_Attractor;

		public AIAttractor m_StartAttractor;

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
		public List<RagdollBone> m_RagdollBones = new List<RagdollBone>();

		[HideInInspector]
		public bool m_PerformEmergency;

		private bool m_InWater;

		private int m_WaterMask = -1;

		private bool m_IsSwimming;

		[HideInInspector]
		public Vector3 m_SwimmingPos = Vector3.zero;

		public string m_PresetName = string.Empty;

		public ItemID m_AddHarvestingItem = ItemID.None;

		[HideInInspector]
		public AI.SoundPreset m_SoundPreset = AI.SoundPreset.None;

		[HideInInspector]
		public float m_InvisibleDuration;

		[HideInInspector]
		public DamageInfo m_LastDamageInfo;

		[HideInInspector]
		public float m_BleedingDamage;

		private string m_LiquidSourceName = "LiquidSource";

		private Collider[] m_CollidersTemp = new Collider[10];

		private bool m_HasBody;

		private static readonly Type[] s_DontRemoveComponents = new Type[]
		{
			typeof(Transform),
			typeof(BoxCollider),
			typeof(DeadBody),
			typeof(AudioSource),
			typeof(VisModule),
			typeof(GuidComponent),
			typeof(ReplicationComponent),
			typeof(Relevance),
			typeof(LODGroup),
			typeof(HealthModule)
		};

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
			Spearman,
			Thug,
			Savage,
			ArmadilloThreeBanded,
			CaimanLizard,
			Crab,
			AngelFish,
			DiscusFish,
			KidRunner,
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
