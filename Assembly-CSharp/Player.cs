using System;
using System.Collections.Generic;
using System.Linq;
using AIs;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Being, ISaveLoad, IInputsReceiver, IPeerWorldRepresentation
{
	public Dictionary<Transform, float> m_RotatedBodyBones { get; private set; } = new Dictionary<Transform, float>();

	public PlayerController GetController(PlayerControllerType type)
	{
		DebugUtils.Assert(type != PlayerControllerType.Unknown && type != PlayerControllerType.Count, true);
		return this.m_PlayerControllers[(int)type];
	}

	public static Player Get()
	{
		return Player.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_LEye = base.gameObject.transform.FindDeepChild("mixamorig:Eye.L");
		this.m_REye = base.gameObject.transform.FindDeepChild("mixamorig:Eye.R");
		this.m_ClimbingBlocks = base.gameObject.transform.FindDeepChild("climbing_blockInHands");
		Player.s_Instance = this;
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
		this.m_BodyInspectionController = base.GetComponent<BodyInspectionController>();
		this.m_MakeFireController = base.GetComponent<MakeFireController>();
		this.m_ConsciousnessController = base.GetComponent<ConsciousnessController>();
		this.m_DiarrheaController = base.GetComponent<DiarrheaController>();
		this.m_AudioModule = base.GetComponent<PlayerAudioModule>();
		this.m_SwimController = base.GetComponent<SwimController>();
		this.m_LookController = base.GetComponent<LookController>();
		this.m_FPPController = base.GetComponent<FPPController>();
		this.m_WatchController = base.GetComponent<WatchController>();
		this.m_NotepadController = base.GetComponent<NotepadController>();
		this.m_MapController = base.GetComponent<MapController>();
		this.m_TorchController = base.GetComponent<TorchController>();
		this.m_DeathController = base.GetComponent<DeathController>();
		this.m_WeaponController = base.GetComponent<WeaponController>();
		this.m_HitReactionController = base.GetComponent<HitReactionController>();
		this.m_ConstructionController = base.GetComponent<ConstructionController>();
		this.m_InsectsController = base.GetComponent<InsectsController>();
		this.m_SleepController = base.GetComponent<SleepController>();
		this.m_Collider = this.m_CharacterController.m_Controller;
		this.m_Watch = base.gameObject.transform.FindDeepChild("watch").gameObject;
		this.m_Animator = base.GetComponent<Animator>();
		this.ParseAdditiveAnimations();
		this.m_CharacterControllerLastOffset = this.m_CharacterController.center;
		this.m_CharacterControllerHeight = this.m_CharacterController.height;
		this.RegisterForPeer(ReplTools.GetLocalPeer());
	}

	private void OnAnimatorMove()
	{
	}

	protected override void Start()
	{
		base.Start();
		CameraManager.Get().SetTarget(this);
		this.SetupControllers();
		this.InitializeParams();
		this.SetModuleReferences();
		this.m_CharacterController.enabled = true;
		this.m_Radius = this.m_CharacterController.radius;
		this.SetupActiveController();
		this.m_LHand = base.gameObject.transform.FindDeepChild("LHolder");
		DebugUtils.Assert(this.m_LHand != null, "Missing Player LHolder", true, DebugUtils.AssertType.Info);
		this.m_LHand.gameObject.AddComponent<PlayerHolder>();
		this.m_RHand = base.gameObject.transform.FindDeepChild("RHolder");
		DebugUtils.Assert(this.m_RHand != null, "Missing Player RHolder", true, DebugUtils.AssertType.Info);
		this.m_RHand.gameObject.AddComponent<PlayerHolder>();
		this.m_Spine1 = base.gameObject.transform.FindDeepChild("mixamorig:Spine1");
		DebugUtils.Assert(this.m_Spine1 != null, "mixamorig:Spine1", true, DebugUtils.AssertType.Info);
		InputsManager.Get().RegisterReceiver(this);
		this.BlockMoves();
		this.BlockRotation();
		base.Invoke("UnblockMoves", this.m_InitDuration);
		base.Invoke("UnblockRotation", this.m_InitDuration);
		this.m_StartTime = Time.time;
	}

	private void InitializeParams()
	{
		this.m_Params = new PlayerParams();
	}

	public void Save()
	{
		SaveGame.SaveVal("PlayerPos", base.transform.position);
		SaveGame.SaveVal("PlayerLookDev", this.m_LookController.m_LookDev);
		SaveGame.SaveVal("PlayerDream", this.m_DreamToActivate);
		SaveGame.SaveVal("MapUnlocked", this.m_MapUnlocked);
		SaveGame.SaveVal("WatchUnlocked", this.m_WatchUnlocked);
		SaveGame.SaveVal("NotepadUnlocked", this.m_NotepadUnlocked);
		SaveGame.SaveVal("BackpackWasOpen", this.m_BackpackWasOpen);
		SaveGame.SaveVal("SleepBlocked", this.m_SleepBlocked);
		SaveGame.SaveVal("InspectionBlocked", this.m_InspectionBlocked);
		SaveGame.SaveVal("Sanity", PlayerSanityModule.Get().m_Sanity);
		SaveGame.SaveVal("RunBlocked", FPPController.Get().m_RunBlocked);
		SaveGame.SaveVal("ShowDivingMask", this.m_ShowDivingMask);
		SaveGame.SaveVal("ShowBiohazardMask", this.m_ShowBiohazardMask);
		SaveGame.SaveVal("WheelHUDBlocked", this.m_WheelHUDBlocked);
		SaveGame.SaveVal("EquippedItemSlot", InventoryBackpack.Get().m_EquippedItemSlot.name);
		SkillsManager.Get().Save();
		SaveGame.SaveVal("KnownInjuriesCount", PlayerInjuryModule.Get().m_KnownInjuries.Count);
		for (int i = 0; i < PlayerInjuryModule.Get().m_KnownInjuries.Count; i++)
		{
			SaveGame.SaveVal("KnownInjury" + i.ToString(), (int)PlayerInjuryModule.Get().m_KnownInjuries[i]);
		}
		SaveGame.SaveVal("KnownInjuryTreatmentsCount", PlayerInjuryModule.Get().m_KnownInjuryTreatments.Count);
		for (int j = 0; j < PlayerInjuryModule.Get().m_KnownInjuryTreatments.Count; j++)
		{
			SaveGame.SaveVal("InjuryTreatments" + j.ToString(), (int)PlayerInjuryModule.Get().m_KnownInjuryTreatments[j]);
		}
		SaveGame.SaveVal("KnownInjuryStateCount", PlayerInjuryModule.Get().m_KnownInjuryState.Count);
		for (int k = 0; k < PlayerInjuryModule.Get().m_KnownInjuryState.Count; k++)
		{
			SaveGame.SaveVal("KnownInjuryState" + k.ToString(), (int)PlayerInjuryModule.Get().m_KnownInjuryState[k]);
		}
		SaveGame.SaveVal("KnownInjuryStateTreatmentsCount", PlayerInjuryModule.Get().m_KnownInjuryStateTreatments.Count);
		for (int l = 0; l < PlayerInjuryModule.Get().m_KnownInjuryStateTreatments.Count; l++)
		{
			SaveGame.SaveVal("InjuryStateTreatments" + l.ToString(), (int)PlayerInjuryModule.Get().m_KnownInjuryStateTreatments[l]);
		}
		SaveGame.SaveVal("SymptomsCount", PlayerDiseasesModule.Get().m_KnownSymptoms.Count);
		for (int m = 0; m < PlayerDiseasesModule.Get().m_KnownSymptoms.Count; m++)
		{
			SaveGame.SaveVal("Symptoms" + m.ToString(), (int)PlayerDiseasesModule.Get().m_KnownSymptoms[m]);
		}
		SaveGame.SaveVal("SymptomTreatmentsCount", PlayerDiseasesModule.Get().m_KnownSymptomTreatments.Count);
		for (int n = 0; n < PlayerDiseasesModule.Get().m_KnownSymptomTreatments.Count; n++)
		{
			SaveGame.SaveVal("SymptomTreatment" + n.ToString(), (int)PlayerDiseasesModule.Get().m_KnownSymptomTreatments[n]);
		}
		SaveGame.SaveVal("DiseasesCount", PlayerDiseasesModule.Get().m_KnownDiseases.Count);
		for (int num = 0; num < PlayerDiseasesModule.Get().m_KnownDiseases.Count; num++)
		{
			SaveGame.SaveVal("Disease" + num.ToString(), (int)PlayerDiseasesModule.Get().m_KnownDiseases[num]);
		}
		SaveGame.SaveVal("DiseaseTreatmentsCount", PlayerDiseasesModule.Get().m_KnownDiseaseTreatments.Count);
		for (int num2 = 0; num2 < PlayerDiseasesModule.Get().m_KnownDiseaseTreatments.Count; num2++)
		{
			SaveGame.SaveVal("DiseaseTreatment" + num2.ToString(), (int)PlayerDiseasesModule.Get().m_KnownDiseaseTreatments[num2]);
		}
		SaveGame.SaveVal("climbing_blockInHands", this.m_ClimbingBlocks.gameObject.activeSelf);
		SaveGame.SaveVal("InfinityDiving", this.m_InfinityDiving);
		SaveGame.SaveVal("BatteryLevel", PlayerWalkieTalkieModule.Get().m_BatteryLevel);
		SaveGame.SaveVal("RespawnPosition", this.m_RespawnPosition);
	}

	public override void Load()
	{
		base.Load();
		this.ResetAnimatorParameters();
		this.m_LastLoadTime = Time.time;
		this.m_WeaponHiddenFromScenario = false;
		this.m_ScenarioPositionObject = null;
		if (FishingController.Get().IsActive())
		{
			FishingController.Get().Stop();
		}
		if (this.m_ActiveFightController)
		{
			this.m_ActiveFightController.Stop();
		}
		if (ItemController.Get().IsActive())
		{
			ItemController.Get().Stop();
		}
		if (SleepController.Get().IsActive())
		{
			SleepController.Get().Stop();
		}
		if (MakeFireController.Get().IsActive())
		{
			MakeFireController.Get().Stop();
		}
		if (WalkieTalkieController.Get().IsActive())
		{
			WalkieTalkieController.Get().Stop();
		}
		if (MudMixerController.Get().IsActive())
		{
			MudMixerController.Get().Stop();
		}
		PlayerSanityModule.Get().m_FeverSanityLoss = 0;
		WalkieTalkieController.Get().HideWalkieTalkieObject();
		HUDReadableItem.Get().Deactivate();
		foreach (GameObject gameObject in this.m_AttachedObjects)
		{
			if (gameObject)
			{
				gameObject.transform.parent = null;
			}
		}
		this.m_AttachedObjects.Clear();
		this.m_Animator.CrossFadeInFixedTime("Idle", 0f, 0);
		this.m_Animator.CrossFadeInFixedTime("Unarmed_Idle", 0f, 1);
		this.ResetControllerToStart();
		this.m_BlockMovesRequestsCount = 0;
		this.m_BlockRotationRequestsCount = 0;
		LookController.Get().ResetLookAtObject(null);
		this.Reposition(SaveGame.LoadV3Val("PlayerPos"), null);
		this.m_LookController.m_LookDev = SaveGame.LoadV2Val("PlayerLookDev");
		this.m_LookController.m_LookDev.y = 0f;
		this.m_LookController.m_WantedLookDev = this.m_LookController.m_LookDev;
		this.m_DreamToActivate = SaveGame.LoadSVal("PlayerDream");
		this.m_MapUnlocked = SaveGame.LoadBVal("MapUnlocked");
		this.m_WatchUnlocked = SaveGame.LoadBVal("WatchUnlocked");
		this.m_NotepadUnlocked = SaveGame.LoadBVal("NotepadUnlocked");
		this.m_BackpackWasOpen = SaveGame.LoadBVal("BackpackWasOpen");
		this.m_SleepBlocked = SaveGame.LoadBVal("SleepBlocked");
		this.m_InspectionBlocked = SaveGame.LoadBVal("InspectionBlocked");
		PlayerSanityModule.Get().m_Sanity = SaveGame.LoadIVal("Sanity");
		FPPController.Get().m_RunBlocked = SaveGame.LoadBVal("RunBlocked");
		this.m_ShowDivingMask = SaveGame.LoadBVal("ShowDivingMask");
		this.m_ShowBiohazardMask = SaveGame.LoadBVal("ShowBiohazardMask");
		this.m_WheelHUDBlocked = SaveGame.LoadBVal("WheelHUDBlocked");
		string name = SaveGame.LoadSVal("EquippedItemSlot");
		InventoryBackpack.Get().m_EquippedItemSlot = InventoryBackpack.Get().GetSlotByName(name, BackpackPocket.Left);
		SkillsManager.Get().Load();
		this.m_LastPosOnGround = base.transform.position;
		PlayerInjuryModule.Get().m_KnownInjuries.Clear();
		int num = SaveGame.LoadIVal("KnownInjuriesCount");
		for (int i = 0; i < num; i++)
		{
			PlayerInjuryModule.Get().m_KnownInjuries.Add((InjuryType)SaveGame.LoadIVal("KnownInjury" + i.ToString()));
		}
		PlayerInjuryModule.Get().m_KnownInjuryTreatments.Clear();
		num = SaveGame.LoadIVal("KnownInjuryTreatmentsCount");
		for (int j = 0; j < num; j++)
		{
			PlayerInjuryModule.Get().m_KnownInjuryTreatments.Add((NotepadKnownInjuryTreatment)SaveGame.LoadIVal("InjuryTreatments" + j.ToString()));
		}
		PlayerInjuryModule.Get().m_KnownInjuryState.Clear();
		num = SaveGame.LoadIVal("KnownInjuryStateCount");
		for (int k = 0; k < num; k++)
		{
			PlayerInjuryModule.Get().m_KnownInjuryState.Add((InjuryState)SaveGame.LoadIVal("KnownInjuryState" + k.ToString()));
		}
		PlayerInjuryModule.Get().m_KnownInjuryStateTreatments.Clear();
		num = SaveGame.LoadIVal("KnownInjuryStateTreatmentsCount");
		for (int l = 0; l < num; l++)
		{
			PlayerInjuryModule.Get().m_KnownInjuryStateTreatments.Add((NotepadKnownInjuryStateTreatment)SaveGame.LoadIVal("InjuryStateTreatments" + l.ToString()));
		}
		PlayerDiseasesModule.Get().m_KnownSymptoms.Clear();
		num = SaveGame.LoadIVal("SymptomsCount");
		for (int m = 0; m < num; m++)
		{
			PlayerDiseasesModule.Get().m_KnownSymptoms.Add((Enums.DiseaseSymptom)SaveGame.LoadIVal("Symptoms" + m.ToString()));
		}
		PlayerDiseasesModule.Get().m_KnownSymptomTreatments.Clear();
		num = SaveGame.LoadIVal("SymptomTreatmentsCount");
		for (int n = 0; n < num; n++)
		{
			PlayerDiseasesModule.Get().m_KnownSymptomTreatments.Add((NotepadKnownSymptomTreatment)SaveGame.LoadIVal("SymptomTreatment" + n.ToString()));
		}
		PlayerDiseasesModule.Get().m_KnownDiseases.Clear();
		num = SaveGame.LoadIVal("DiseasesCount");
		for (int num2 = 0; num2 < num; num2++)
		{
			PlayerDiseasesModule.Get().m_KnownDiseases.Add((ConsumeEffect)SaveGame.LoadIVal("Disease" + num2.ToString()));
		}
		PlayerDiseasesModule.Get().m_KnownDiseaseTreatments.Clear();
		num = SaveGame.LoadIVal("DiseaseTreatmentsCount");
		for (int num3 = 0; num3 < num; num3++)
		{
			PlayerDiseasesModule.Get().m_KnownDiseaseTreatments.Add((NotepadKnownDiseaseTreatment)SaveGame.LoadIVal("DiseaseTreatment" + num3.ToString()));
		}
		DeathController.Get().ResetDeathType();
		this.m_ClimbingBlocks.gameObject.SetActive(SaveGame.LoadBVal("climbing_blockInHands"));
		this.m_InfinityDiving = SaveGame.LoadBVal("InfinityDiving");
		PlayerWalkieTalkieModule.Get().m_BatteryLevel = SaveGame.LoadFVal("BatteryLevel");
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionMasterShelters1_3)
		{
			this.m_RespawnPosition = SaveGame.LoadV3Val("RespawnPosition");
		}
	}

	public override bool IsPlayer()
	{
		return true;
	}

	public bool BackpackWasOpen()
	{
		return this.m_BackpackWasOpen;
	}

	protected override void Update()
	{
		if (!MainLevel.Instance.m_LevelStarted)
		{
			return;
		}
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		Physics.autoSyncTransforms = false;
		base.Update();
		if (this.m_HasKilledTribeInLastFrame)
		{
			if (this.m_HasKilledTribeFrameCounter == 1)
			{
				this.m_HasKilledTribeInLastFrame = false;
				this.m_HasKilledTribeFrameCounter = 0;
			}
			else
			{
				this.m_HasKilledTribeFrameCounter++;
			}
		}
		for (int i = 0; i < 44; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].ControllerUpdate();
			}
		}
		if (this.m_ControllerToStart != PlayerControllerType.Unknown && !this.IsControllerStartBlocked())
		{
			this.StartControllerInternal();
		}
		if (this.m_UpdateHands)
		{
			this.UpdateHands();
		}
		this.UpdateVelocity();
		this.UpdateInAir();
		this.UpdateInWater();
		this.UpdateSwim();
		this.UpdatePassOut();
		this.UpdateDeath();
		this.UpdateLeavesPusher();
		this.UpdateInputs();
		this.UpdateWatchObject();
		this.UpdateTraveledDistStat();
		this.UpdateWeapon();
		WalkieTalkieController walkieTalkieController = WalkieTalkieController.Get();
		if (walkieTalkieController != null)
		{
			walkieTalkieController.UpdateWTActivity();
		}
		this.UpdateFreeHandsLadder();
		if (this.m_ResetDreamParams)
		{
			this.m_ResetDreamParamsFrame++;
			if (this.m_ResetDreamParamsFrame > 3)
			{
				this.ResetDreamParams();
				this.m_ResetDreamParams = false;
				this.m_ResetDreamParamsFrame = 0;
			}
		}
		this.DebugUpdate();
		this.m_MapActivityChanged = false;
		this.m_NotepadActivityChanged = false;
		this.UpdateShaderParameters();
		this.UpdateFishingRodAnimatorState();
		Physics.autoSyncTransforms = autoSyncTransforms;
	}

	private void UpdateFishingRodAnimatorState()
	{
		Item currentItem = this.GetCurrentItem(Hand.Right);
		if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Fishing_Rod)
		{
			this.m_Animator.SetBool(this.m_FishingRodInHand, true);
			return;
		}
		this.m_Animator.SetBool(this.m_FishingRodInHand, false);
	}

	private void UpdateScenarioPositionObject()
	{
		if (!this.m_ScenarioPositionObject)
		{
			return;
		}
		Vector3 vector = (this.m_ScenarioPositionObject.transform.position - base.transform.position).To2D();
		Vector3 a = vector.GetNormalized2D() * Mathf.Min(vector.magnitude, FPPController.Get().m_WalkSpeed) + Physics.gravity * Time.fixedDeltaTime;
		this.m_CharacterController.Move(a * Time.fixedDeltaTime, true);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		this.UpdateShake();
		if (this.m_FPPController.IsActive())
		{
			this.m_FPPController.UpdateBodyRotation();
		}
		this.UpdateBonesRotation();
		for (int i = 0; i < 44; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].ControllerLateUpdate();
			}
		}
		this.UpdateAdditiveAnimations();
		this.UpdateInLift();
		this.UpdateAim();
		InsectsManager.Get().UpdateInsects();
		BodyInspectionController.Get().UpdateMaggots();
		BodyInspectionController.Get().UpdateAnts();
		this.UpdateScenarioPositionObject();
		if (this.m_OpenBackpackSheduled)
		{
			Inventory3DManager.Get().Activate();
			this.m_OpenBackpackSheduled = false;
		}
	}

	public PlayerParams GetParams()
	{
		return this.m_Params;
	}

	public T GetParam<T>(string param_name)
	{
		return this.m_Params.GetParam<T>(param_name);
	}

	public void ResetBlockMoves()
	{
		this.m_BlockMovesRequestsCount = 0;
	}

	public void BlockMoves()
	{
		this.m_BlockMovesRequestsCount++;
	}

	public void UnblockMoves()
	{
		if (this.m_BlockMovesRequestsCount > 0)
		{
			this.m_BlockMovesRequestsCount--;
		}
	}

	public bool GetMovesBlocked()
	{
		return this.m_BlockMovesRequestsCount > 0 || LoadingScreen.Get().m_Active || HUDStartSurvivalSplash.Get().m_Active || InputsManager.Get().m_TextInputActive;
	}

	public void ResetBlockRotation()
	{
		this.m_BlockRotationRequestsCount = 0;
	}

	public void BlockRotation()
	{
		bool rotationBlocked = this.GetRotationBlocked();
		this.m_BlockRotationRequestsCount++;
		if (!rotationBlocked)
		{
			PlayerController[] components = base.GetComponents<PlayerController>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnBlockRotation();
			}
		}
	}

	public void UnblockRotation()
	{
		if (this.GetRotationBlocked())
		{
			this.m_BlockRotationRequestsCount--;
			if (!this.GetRotationBlocked())
			{
				PlayerController[] components = base.GetComponents<PlayerController>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].OnUnblockRotation();
				}
			}
		}
	}

	public bool GetRotationBlocked()
	{
		return this.m_BlockRotationRequestsCount > 0 || HUDStartSurvivalSplash.Get().m_Active;
	}

	public void DropItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (this.m_CurrentItem[i] == item)
			{
				this.DetachItemFromHand(this.m_CurrentItem[i]);
				this.SetWantedItem((Hand)i, null, true);
				this.m_CurrentItem[i] = null;
				this.SetupActiveController();
				break;
			}
		}
		if (item.m_Info.IsHeavyObject() && !item.m_Info.m_DestroyOnDrop)
		{
			PlayerAudioModule.Get().PlayHODropSound();
		}
	}

	private void ApplyForceToHeavyObjectAfterDrop(Item item)
	{
		Rigidbody componentDeepChild = General.GetComponentDeepChild<Rigidbody>(item.gameObject);
		Vector3 vector = this.m_CharacterController.transform.right;
		vector *= 1.4f;
		componentDeepChild.velocity += vector;
	}

	public float GetMaxStamina()
	{
		return this.m_ConditionModule.GetMaxStamina();
	}

	public float GetStamina()
	{
		return this.m_ConditionModule.GetStamina();
	}

	public float GetMaxEnergy()
	{
		return this.m_ConditionModule.GetMaxEnergy();
	}

	public float GetEnergy()
	{
		return this.m_ConditionModule.GetEnergy();
	}

	public float GetMaxNutritionFat()
	{
		return this.m_ConditionModule.GetMaxNutritionFat();
	}

	public float GetNutritionFat()
	{
		return this.m_ConditionModule.GetNutritionFat();
	}

	public float GetMaxNutritionCarbo()
	{
		return this.m_ConditionModule.GetMaxNutritionCarbo();
	}

	public float GetNutritionCarbo()
	{
		return this.m_ConditionModule.GetNutritionCarbo();
	}

	public float GetMaxNutritionProtein()
	{
		return this.m_ConditionModule.GetMaxNutritionProtein();
	}

	public float GetNutritionProtein()
	{
		return this.m_ConditionModule.GetNutritionProtein();
	}

	public float GetMaxHydration()
	{
		return this.m_ConditionModule.GetMaxHydration();
	}

	public float GetHydration()
	{
		return this.m_ConditionModule.GetHydration();
	}

	public bool IsStaminaDepleted()
	{
		return this.m_ConditionModule.IsStaminaDepleted();
	}

	public void DecreaseStamina(StaminaDecreaseReason reason)
	{
		this.m_ConditionModule.DecreaseStamina(reason, 1f);
	}

	public float GetStaminaDecrease(StaminaDecreaseReason reason)
	{
		return this.m_ConditionModule.GetStaminaDecrease(reason);
	}

	public void DecreaseStamina(float value)
	{
		this.m_ConditionModule.DecreaseStamina(value);
	}

	public void DecreaseEnergy(EnergyDecreaseReason reason)
	{
		this.m_ConditionModule.DecreaseEnergy(reason);
	}

	public Item GetWantedItem(Hand hand)
	{
		return this.m_WantedItem[(int)hand];
	}

	public void SetWantedItem(Item item, bool immediate = true)
	{
		if (item == null)
		{
			DebugUtils.Assert("Use SetWantedItem method with Hand parameter!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.SetWantedItem(item.m_Info.IsBow() ? Hand.Left : Hand.Right, item, immediate);
	}

	public void SetWantedItem(Hand hand, Item item, bool immediate = true)
	{
		this.m_WantedItem[(int)hand] = item;
		this.SetupActiveController();
		InventoryBackpack.Get().RemoveItem(item, false);
		if (item)
		{
			item.StaticPhxRequestReset();
		}
		if (immediate)
		{
			this.m_UpdateHands = true;
		}
	}

	public void UpdateHands()
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.m_WantedItem[i] == null && this.m_WantedItem[i] != this.m_CurrentItem[i])
			{
				this.DetachItemFromHand(this.m_CurrentItem[i]);
				if (this.m_SlotToEquip != null)
				{
					InventoryBackpack.Get().InsertItem(this.m_CurrentItem[i], InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, true);
				}
				else
				{
					InventoryBackpack.Get().m_EquippedItem == this.m_CurrentItem[i];
				}
				this.m_CurrentItem[i].m_InPlayersHand = false;
				this.m_CurrentItem[i].m_ShownInInventory = true;
				this.m_CurrentItem[i] = null;
				this.OnItemChanged(null, (Hand)i);
			}
		}
		Item equippedItem = null;
		for (int j = 0; j < 2; j++)
		{
			if (this.m_WantedItem[j] != null && this.m_WantedItem[j] != this.m_CurrentItem[j])
			{
				if (this.m_CurrentItem[j])
				{
					this.DetachItemFromHand(this.m_CurrentItem[j]);
					this.m_CurrentItem[j].m_InPlayersHand = false;
					this.m_CurrentItem[j].m_ShownInInventory = true;
					if (this.m_SlotToEquip != null)
					{
						InventoryBackpack.Get().InsertItem(this.m_CurrentItem[j], InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, true);
					}
				}
				equippedItem = this.m_WantedItem[j];
				this.m_CurrentItem[j] = this.m_WantedItem[j];
				this.m_CurrentItem[j].m_InPlayersHand = true;
				this.AttachItemToHand((Hand)j, this.m_CurrentItem[j]);
				InventoryBackpack.Get().RemoveItem(this.m_CurrentItem[j], false);
				this.OnItemChanged(this.m_CurrentItem[j], (Hand)j);
			}
		}
		if (this.m_SlotToEquip)
		{
			InventoryBackpack.Get().m_EquippedItemSlot = this.m_SlotToEquip;
			InventoryBackpack.Get().m_EquippedItem = equippedItem;
			this.m_SlotToEquip = null;
		}
		this.m_UpdateHands = false;
	}

	public void AttachItemToHand(Hand hand, Item item)
	{
		Transform transform = (hand == Hand.Left) ? this.m_LHand : this.m_RHand;
		Quaternion rhs = Quaternion.Inverse(item.m_Holder.localRotation);
		item.gameObject.transform.rotation = transform.rotation;
		item.gameObject.transform.rotation *= rhs;
		Vector3 b = item.m_Holder.parent.position - item.m_Holder.position;
		item.gameObject.transform.position = transform.position;
		item.gameObject.transform.position += b;
		item.gameObject.transform.parent = transform.transform;
		item.OnItemAttachedToHand();
		Physics.IgnoreCollision(this.m_Collider, item.m_Collider, true);
		AttachmentSynchronizer component = item.GetComponent<AttachmentSynchronizer>();
		if (component)
		{
			component.Attach(base.gameObject, (hand == Hand.Left) ? "LHolder" : "RHolder");
		}
	}

	private void DetachItemFromHand(Item item)
	{
		if (item.transform.parent == null || (item.transform.parent != this.m_LHand.transform && item.transform.parent != this.m_RHand.transform))
		{
			return;
		}
		if (item.m_Info.IsHeavyObject())
		{
			((HeavyObject)item).DetachHeavyObjects();
		}
		if (!item.m_Info.m_CanBeAddedToInventory)
		{
			item.ItemsManagerRegister(false);
		}
		item.transform.parent = null;
		item.OnItemDetachedFromHand();
		if (item.m_Info.m_DestroyOnDrop)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		if (this.m_Collider)
		{
			Physics.IgnoreCollision(this.m_Collider, item.m_Collider, false);
		}
	}

	public bool HasItemEquiped(string item_name)
	{
		Item currentItem = this.GetCurrentItem(Hand.Right);
		Item currentItem2 = this.GetCurrentItem(Hand.Left);
		return (currentItem && ItemsManager.Get().ItemIDToString((int)currentItem.GetInfoID()) == item_name) || (currentItem2 && ItemsManager.Get().ItemIDToString((int)currentItem2.GetInfoID()) == item_name);
	}

	public bool HaveItem(ItemID id)
	{
		return (this.GetCurrentItem(Hand.Right) && this.GetCurrentItem(Hand.Right).GetInfoID() == id) || (this.GetCurrentItem(Hand.Left) && this.GetCurrentItem(Hand.Left).GetInfoID() == id) || InventoryBackpack.Get().Contains(id);
	}

	public bool ScenarioHaveItem(string item_name)
	{
		ItemID itemID = (ItemID)ItemsManager.Get().StringToItemID(item_name);
		return (this.GetCurrentItem(Hand.Right) && this.GetCurrentItem(Hand.Right).GetInfoID() == itemID) || (this.GetCurrentItem(Hand.Left) && this.GetCurrentItem(Hand.Left).GetInfoID() == itemID) || InventoryBackpack.Get().Contains(itemID);
	}

	public bool HaveItemType(string item_type)
	{
		ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), item_type);
		return (this.GetCurrentItem(Hand.Right) && this.GetCurrentItem(Hand.Right).m_Info.m_Type == itemType) || (this.GetCurrentItem(Hand.Left) && this.GetCurrentItem(Hand.Left).m_Info.m_Type == itemType) || InventoryBackpack.Get().Contains(itemType);
	}

	public Item GetCurrentItem()
	{
		if (this.m_CurrentItem[1] != null)
		{
			return this.m_CurrentItem[1];
		}
		if (this.m_CurrentItem[0] != null)
		{
			return this.m_CurrentItem[0];
		}
		return null;
	}

	public Item GetCurrentItem(Hand hand)
	{
		return this.m_CurrentItem[(int)hand];
	}

	public void SetCurrentItem(Hand hand, Item item)
	{
		this.m_CurrentItem[(int)hand] = item;
		if (item && item.gameObject.scene != MainLevel.Instance.m_LevelScene)
		{
			SceneManager.MoveGameObjectToScene(item.gameObject, MainLevel.Instance.m_LevelScene);
		}
	}

	public bool IsLookingAtObject(GameObject obj, float dist)
	{
		if (!Camera.main)
		{
			return false;
		}
		Vector3 position = Camera.main.transform.position;
		Vector3 forward = Camera.main.transform.forward;
		int num = Physics.RaycastNonAlloc(position, forward, this.m_RaycastHitsTmp, dist);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this.m_RaycastHitsTmp[i];
			if (raycastHit.collider.gameObject == obj)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateInAir()
	{
		if (Time.deltaTime == 0f)
		{
			return;
		}
		if (Time.time - this.m_StartTime < this.m_InitDuration)
		{
			return;
		}
		if (this.m_CharacterController.isGrounded && this.m_IsInAir && !base.GetComponent<LadderController>().IsActive())
		{
			this.OnLand(this.m_LastPosOnGround.y - base.gameObject.transform.position.y);
		}
		if (this.m_CharacterController.isGrounded || this.m_SwimController.IsActive() || HarvestingAnimalController.Get().IsActive() || HarvestingSmallAnimalController.Get().IsActive() || MudMixerController.Get().IsActive() || FreeHandsLadderController.Get().IsActive())
		{
			this.m_LastPosOnGround = base.transform.position;
			this.m_LastTimeOnGround = Time.time;
		}
		this.m_IsInAir = (!this.m_CharacterController.isGrounded && !this.m_SwimController.IsActive());
	}

	private void OnLand(float fall_height)
	{
		if (LoadingScreen.Get() && LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (fall_height <= this.m_MinFallingHeight && Time.time - this.m_LastTimeOnGround < this.m_MinFallingDuration)
		{
			return;
		}
		if (fall_height > this.m_MinFallingHeightToDealDamage)
		{
			this.TakeDamage(new DamageInfo
			{
				m_Damage = (fall_height - this.m_MinFallingHeightToDealDamage) * 5f,
				m_Damager = base.gameObject,
				m_HitDir = Vector3.up,
				m_DamageType = DamageType.Fall
			});
		}
		this.m_AudioModule.PlayFeetLandingSound(1f, false);
		if (Mathf.Abs(fall_height) > 1f)
		{
			this.m_AudioModule.PlayLandingSound();
		}
	}

	private void SetModuleReferences()
	{
		this.m_ConditionModule = base.GetComponent<PlayerConditionModule>();
		this.m_DiseasesModule = base.GetComponent<PlayerDiseasesModule>();
		this.m_InjuryModule = base.GetComponent<PlayerInjuryModule>();
	}

	public void OnMenuScreenShow(Type type)
	{
		this.StopController(PlayerControllerType.BodyInspection);
	}

	public void OnMenuScreenHide()
	{
	}

	private void UpdateInWater()
	{
		this.m_InWater = WaterBoxManager.Get().IsInWater(this.m_Collider.bounds.center, ref this.m_WaterLevel);
		this.m_InSwimWater = WaterBoxManager.Get().IsInSwimWater(this.m_Collider.bounds.center, ref this.m_WaterLevel);
	}

	public override bool IsInWater()
	{
		return this.m_InWater;
	}

	public bool IsInSwimWater()
	{
		return this.m_InSwimWater;
	}

	public float GetWaterLevel()
	{
		return this.m_WaterLevel;
	}

	private void UpdateSwim()
	{
		bool flag = this.ShouldSwim();
		if (!this.m_SwimController.IsActive() && flag && !this.IsDead() && !this.m_DeathController.IsActive() && GreenHellGame.Instance.m_LoadGameState == LoadGameState.None)
		{
			this.StartController(PlayerControllerType.Swim);
		}
		if (this.m_SwimController.IsActive() && !flag)
		{
			this.StopController(PlayerControllerType.Swim);
		}
	}

	public bool ShouldSwim()
	{
		if (HarvestingAnimalController.Get().IsActive())
		{
			return false;
		}
		if (HarvestingSmallAnimalController.Get().IsActive())
		{
			return false;
		}
		if (this.m_SwimController.IsActive())
		{
			if (!this.IsInSwimWater())
			{
				return false;
			}
			Vector3 position = base.transform.position;
			position.y = this.m_WaterLevel;
			this.m_Ray.origin = position;
			this.m_Ray.direction = Vector3.down;
			int num = Physics.RaycastNonAlloc(this.m_Ray, this.m_Hits, Player.DEEP_WATER * 0.6f);
			for (int i = 0; i < num; i++)
			{
				if (!this.m_Hits[i].collider.isTrigger && this.m_Hits[i].collider.gameObject.layer != LayerMask.NameToLayer("SmallPlant") && this.m_Hits[i].collider.gameObject != base.gameObject && this.m_Hits[i].collider.gameObject != this.m_Collider.gameObject && this.GetLEyeTransform().position.y >= this.m_Hits[i].point.y)
				{
					return false;
				}
			}
			return true;
		}
		else
		{
			if (this.IsInSwimWater())
			{
				Vector3 position2 = base.transform.position;
				position2.y = this.m_WaterLevel;
				this.m_Ray.origin = position2;
				this.m_Ray.direction = Vector3.down;
				int num2 = Physics.RaycastNonAlloc(this.m_Ray, this.m_Hits, Player.DEEP_WATER * 0.7f);
				for (int j = 0; j < num2; j++)
				{
					if (!this.m_Hits[j].collider.isTrigger && this.m_Hits[j].collider.gameObject.layer != LayerMask.NameToLayer("SmallPlant") && this.m_Hits[j].collider.gameObject != base.gameObject && this.m_Hits[j].collider.gameObject != this.m_Collider.gameObject)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}

	public void StartFreeHandsClimbing(FreeHandsLadder ladder)
	{
		if (ladder == null)
		{
			return;
		}
		base.GetComponent<FreeHandsLadderController>().SetLadder(ladder);
		this.m_Climbing = true;
		this.StartController(PlayerControllerType.FreeHandsLadder);
	}

	public void StartClimbing(Ladder ladder)
	{
		if (ladder == null)
		{
			return;
		}
		base.GetComponent<LadderController>().SetLadder(ladder);
		this.m_Climbing = true;
		this.HideWeapon();
		this.StartController(PlayerControllerType.Ladder);
	}

	public void StopClimbing()
	{
		base.GetComponent<LadderController>().SetLadder(null);
		this.m_Climbing = false;
	}

	public bool IsClimbing()
	{
		return this.m_Climbing;
	}

	public void UpdateBonesRotation()
	{
		this.m_AffectedBones.Clear();
		this.m_BonesToRemove.Clear();
		PlayerController playerController = null;
		Vector3 right = base.gameObject.transform.right;
		Vector3 up = base.gameObject.transform.up;
		bool flag = false;
		for (int i = 0; i < this.m_PlayerControllers.Length; i++)
		{
			PlayerController playerController2 = this.m_PlayerControllers[i];
			if (playerController2.IsActive())
			{
				Dictionary<Transform, float> bodyRotationBonesParams = playerController2.GetBodyRotationBonesParams();
				if (bodyRotationBonesParams != null && bodyRotationBonesParams.Count > 0)
				{
					flag = true;
					DebugUtils.Assert(!playerController, true);
					playerController = playerController2;
					if (playerController != this.m_LastActiveBodyRotationController)
					{
						this.m_LastTimeBodyRotationControllerChange = Time.time;
					}
					foreach (KeyValuePair<Transform, float> keyValuePair in bodyRotationBonesParams)
					{
						Transform key = keyValuePair.Key;
						if (!this.m_AffectedBones.Contains(key))
						{
							this.m_AffectedBones.Add(key);
						}
						float num = bodyRotationBonesParams[key];
						float num2 = 0f;
						this.m_RotatedBodyBones.TryGetValue(key, out num2);
						float num3 = num2 + (num - num2) * Mathf.Clamp01(Time.time - this.m_LastTimeBodyRotationControllerChange);
						key.Rotate(right, num3, Space.World);
						if (this.m_ShakePower > 0f && this.m_ShakeSpeed > 0f)
						{
							float num4 = Mathf.PerlinNoise(Time.time * 0.5f * this.m_ShakeSpeed, Time.time * 0.25f * this.m_ShakeSpeed) - 0.5f;
							key.Rotate(right, num4 * this.m_ShakePower, Space.World);
							num4 = Mathf.PerlinNoise(Time.time * 0.25f * this.m_ShakeSpeed, Time.time * 0.5f * this.m_ShakeSpeed) - 0.5f;
							key.Rotate(up, num4 * this.m_ShakePower, Space.World);
							if (this.m_AdditionalShakePower > 0f)
							{
								num4 = Mathf.PerlinNoise(Time.time * 10f, Time.time * 5f) - 0.5f;
								key.Rotate(right, num4 * this.m_AdditionalShakePower, Space.World);
								num4 = Mathf.PerlinNoise(Time.time * 10f, Time.time * 5f) - 0.5f;
								key.Rotate(up, num4 * this.m_AdditionalShakePower, Space.World);
							}
						}
						this.m_RotatedBodyBones[key] = num3;
					}
					this.m_LastActiveBodyRotationController = playerController;
				}
			}
		}
		if (!flag && this.m_BonesRotated)
		{
			this.m_LastTimeBodyRotationControllerChange = Time.time;
		}
		Dictionary<Transform, float>.Enumerator enumerator2 = this.m_RotatedBodyBones.GetEnumerator();
		this.m_BonesToRotate.Clear();
		this.m_BonesToRotateValue.Clear();
		while (enumerator2.MoveNext())
		{
			KeyValuePair<Transform, float> keyValuePair = enumerator2.Current;
			Transform key2 = keyValuePair.Key;
			if (!this.m_AffectedBones.Contains(key2))
			{
				keyValuePair = enumerator2.Current;
				float num5 = keyValuePair.Value;
				num5 *= Mathf.Clamp01(1f - (Time.time - this.m_LastTimeBodyRotationControllerChange) / 0.3f);
				if (Mathf.Abs(num5) > 0.1f)
				{
					key2.Rotate(right, num5, Space.World);
					this.m_BonesToRotate.Add(key2);
					this.m_BonesToRotateValue.Add(num5);
				}
				else
				{
					this.m_BonesToRemove.Add(key2);
				}
			}
		}
		for (int j = 0; j < this.m_BonesToRotate.Count; j++)
		{
			this.m_RotatedBodyBones[this.m_BonesToRotate[j]] = this.m_BonesToRotateValue[j];
		}
		for (int k = 0; k < this.m_BonesToRemove.Count; k++)
		{
			this.m_RotatedBodyBones.Remove(this.m_BonesToRemove[k]);
		}
		this.m_BonesRotated = flag;
	}

	public void StartShake(float power, float speed, float duration)
	{
		this.m_WantedShakePower = power * Skill.Get<ArcherySkill>().GetAimShakeMul();
		this.m_ShakeSpeed = speed;
		this.m_SetShakePowerDuration = duration;
	}

	public void StopShake(float duration)
	{
		this.m_WantedShakePower = 0f;
		this.m_SetShakePowerDuration = duration;
		this.m_AdditionalShakePower = 0f;
	}

	private void UpdateShake()
	{
		if (this.m_ShakePower != this.m_WantedShakePower)
		{
			if (this.m_SetShakePowerDuration <= 0f)
			{
				this.m_ShakePower = this.m_WantedShakePower;
				return;
			}
			this.m_ShakePower += (this.m_WantedShakePower - this.m_ShakePower) * Time.deltaTime / this.m_SetShakePowerDuration;
		}
	}

	public void AddKnownItem(ItemID id)
	{
		if (!this.m_KnownItems.Contains(id))
		{
			this.m_KnownItems.Add(id);
		}
	}

	private void SetupControllers()
	{
		string type = string.Empty;
		for (int i = 0; i < 44; i++)
		{
			PlayerControllerType playerControllerType = (PlayerControllerType)i;
			type = playerControllerType.ToString() + "Controller";
			this.m_PlayerControllers[i] = (base.gameObject.GetComponent(type) as PlayerController);
		}
	}

	public PlayerControllerType m_LastActiveController { get; private set; } = PlayerControllerType.Unknown;

	public void StartController(PlayerControllerType controller)
	{
		if (this.m_HitReactionController.enabled && controller != PlayerControllerType.Death)
		{
			return;
		}
		if (this.m_ControllerToStart != controller)
		{
			this.m_ControllerToStart = controller;
			if (this.IsItemChangePending())
			{
				PlayerController playerController = this.m_PlayerControllers[(int)this.m_LastActiveController];
				bool flag = playerController != null && playerController.PlayUnequipAnimation();
				if (flag)
				{
					this.m_Animator.SetTrigger(this.m_ChangingControllerInAnimator);
				}
				this.m_Animator.SetBool(this.m_PlayUnequipInAnimator, flag);
			}
		}
	}

	public void ResetControllerToStart()
	{
		this.m_ControllerToStart = PlayerControllerType.Unknown;
	}

	private bool IsControllerStartBlocked()
	{
		return (!Inventory3DManager.Get() || !Inventory3DManager.Get().gameObject.activeSelf) && (!this.m_WeaponController || !this.m_WeaponController.m_AnimationStopped) && (this.m_Animator.GetBool(this.m_ChangingControllerInAnimator) || this.m_Animator.IsInTransition(1) || (this.m_WeaponController && this.m_WeaponController.IsAttack()));
	}

	public void StartControllerInternal()
	{
		if (this.m_ControllerToStart == PlayerControllerType.Unknown)
		{
			return;
		}
		PlayerControllerType controllerToStart = this.m_ControllerToStart;
		if (this.m_PlayerControllers[(int)this.m_ControllerToStart].enabled && this.IsItemChangePending())
		{
			Item item = this.m_WantedItem[0];
			Item item2 = this.m_WantedItem[1];
			ItemSlot slotToEquip = this.m_SlotToEquip;
			this.m_WantedItem[1] = null;
			this.m_WantedItem[0] = null;
			this.SetupActiveController();
			if (this.m_ControllerToStart != controllerToStart)
			{
				this.UpdateHands();
				this.SetActiveControllers(this.m_ControllerToStart);
			}
			this.m_WantedItem[1] = item2;
			this.m_WantedItem[0] = item;
			this.m_SlotToEquip = slotToEquip;
		}
		this.m_Animator.ResetTrigger(this.m_ChangingControllerInAnimator);
		this.m_Animator.SetBool(this.m_PlayUnequipInAnimator, false);
		this.UpdateHands();
		Debug.Log(controllerToStart.ToString());
		this.SetActiveControllers(controllerToStart);
		this.m_LastActiveController = controllerToStart;
		this.m_ControllerToStart = PlayerControllerType.Unknown;
	}

	private void SetActiveControllers(PlayerControllerType main_controller)
	{
		for (int i = 0; i < 44; i++)
		{
			if (i != (int)main_controller)
			{
				int num = this.m_PlayerControllerArray[i, (int)main_controller];
				bool enabled = num != 0 && (num != 1 || this.m_PlayerControllers[i].enabled);
				this.m_PlayerControllers[i].enabled = enabled;
			}
		}
		this.m_PlayerControllers[(int)main_controller].enabled = true;
	}

	public void StopController(PlayerControllerType controller)
	{
		bool enabled = this.m_PlayerControllers[(int)controller].enabled;
		this.m_PlayerControllers[(int)controller].enabled = false;
		if (enabled && this.m_PlayerControllers[(int)controller].SetupActiveControllerOnStop() && controller == this.m_LastActiveController)
		{
			if (this.m_ControllerToStart == PlayerControllerType.Unknown)
			{
				this.SetupActiveController();
			}
			this.StartControllerInternal();
		}
		if (controller == PlayerControllerType.Watch && MapController.Get().IsActive())
		{
			this.m_LastActiveController = PlayerControllerType.Map;
		}
	}

	public void SetupActiveController()
	{
		if (this.IsDead())
		{
			return;
		}
		if (this.m_ShouldStartWalkieTalkieController)
		{
			this.StartController(PlayerControllerType.WalkieTalkie);
			return;
		}
		bool flag = BodyInspectionController.Get().IsActive() || BoatController.Get().IsActive() || NotepadController.Get().IsActive() || MapController.Get().IsActive() || InsectsController.Get().IsActive() || InventoryController.Get().IsActive();
		PlayerControllerType controllerForItems = this.GetControllerForItems(this.m_WantedItem[1], this.m_WantedItem[0]);
		if (controllerForItems != PlayerControllerType.Unknown)
		{
			Item item = this.m_WantedItem[1];
			if (!Inventory3DManager.Get().IsActive() || !(item != null) || item.m_Info.m_ID != ItemID.Fishing_Rod)
			{
				this.StartController(controllerForItems);
			}
			return;
		}
		if (this.ShouldSwim())
		{
			if (!this.m_SwimController.IsActive() && GreenHellGame.Instance.m_LoadGameState == LoadGameState.None)
			{
				this.StartController(PlayerControllerType.Swim);
			}
			return;
		}
		if (flag)
		{
			return;
		}
		this.StartController(PlayerControllerType.FPP);
	}

	private PlayerControllerType GetControllerForItems(Item r_item, Item l_item)
	{
		if (l_item != null)
		{
			if (l_item.m_Info.IsWeapon())
			{
				if (((Weapon)l_item).m_Info.IsBow() && r_item == null)
				{
					return PlayerControllerType.Bow;
				}
			}
			else if (l_item.GetInfoID() == ItemID.Fish_Bone)
			{
				return PlayerControllerType.BodyInspectionMiniGame;
			}
		}
		if (r_item != null)
		{
			if (r_item.m_Info.IsWeapon())
			{
				WeaponType weaponType = ((Weapon)r_item).GetWeaponType();
				if (weaponType == WeaponType.Melee)
				{
					if (r_item.m_Info.m_ID == ItemID.Torch || r_item.m_Info.m_ID == ItemID.Weak_Torch || r_item.m_Info.m_ID == ItemID.Tobacco_Torch)
					{
						return PlayerControllerType.Torch;
					}
					return PlayerControllerType.WeaponMelee;
				}
				else
				{
					if (weaponType == WeaponType.Spear)
					{
						return PlayerControllerType.WeaponSpear;
					}
					if (weaponType == WeaponType.Bow)
					{
						return PlayerControllerType.Bow;
					}
					if (weaponType == WeaponType.Blowpipe)
					{
						return PlayerControllerType.Blowpipe;
					}
				}
			}
			else
			{
				if (r_item.m_Info.IsBowl())
				{
					return PlayerControllerType.Bowl;
				}
				if (r_item.m_Info.m_ID == ItemID.Water_In_Hands)
				{
					return PlayerControllerType.LiquidInHands;
				}
				if (r_item.GetComponent<FishingRod>())
				{
					return PlayerControllerType.Fishing;
				}
				if (r_item.GetComponent<FireTool>())
				{
					return PlayerControllerType.MakeFire;
				}
				if (r_item.m_Info.IsHeavyObject())
				{
					return PlayerControllerType.HeavyObject;
				}
			}
			if (r_item.GetInfoID() == ItemID.Fish_Bone)
			{
				return PlayerControllerType.BodyInspectionMiniGame;
			}
			if (r_item.GetInfoID() != ItemID.Boat_Stick)
			{
				if (r_item.GetInfoID() == ItemID.Fire)
				{
					this.HideWeapon();
				}
				return PlayerControllerType.Item;
			}
		}
		return PlayerControllerType.Unknown;
	}

	private bool IsItemChangePending()
	{
		return this.m_WantedItem[0] != this.m_CurrentItem[0] || this.m_WantedItem[1] != this.m_CurrentItem[1];
	}

	private void UpdatePassOut()
	{
		if (this.ShouldPassOut())
		{
			if (SwimController.Get().IsActive())
			{
				this.Die(DeathController.DeathType.UnderWater);
				return;
			}
			if (this.IsInWater() && this.GetWaterLevel() > base.transform.position.y + 0.6f)
			{
				this.Die(DeathController.DeathType.Normal);
				return;
			}
			this.HideWeapon();
			this.StartController(PlayerControllerType.Consciousness);
			this.StartControllerInternal();
		}
	}

	public bool ShouldPassOut()
	{
		return (GreenHellGame.DEBUG && Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.J)) || (!this.m_PlayerControllers[1].IsActive() && !this.m_PlayerControllers[3].IsActive() && !DialogsManager.Get().IsAnyDialogPlaying() && !CutscenesManager.Get().IsCutscenePlaying() && !this.m_CurrentLift && !SleepController.Get().IsSleeping() && this.m_ConditionModule.GetEnergy() <= this.m_ConsciousnessController.GetEnergyToPassOut());
	}

	public override bool IsDead()
	{
		return this.m_ConditionModule.GetHP() <= 0f;
	}

	public void UpdateDeath()
	{
		if (this.IsDead() && !this.m_PlayerControllers[3].IsActive() && !this.m_SleepController.IsActive() && !this.m_ConsciousnessController.IsActive())
		{
			if (DeathController.Get().m_DeathType == DeathController.DeathType.Normal && SwimController.Get().IsActive())
			{
				if (SwimController.Get().m_State == SwimState.Dive)
				{
					this.Die(DeathController.DeathType.UnderWater);
					return;
				}
				this.Die(DeathController.DeathType.OnWater);
				return;
			}
			else
			{
				this.Die(DeathController.Get().m_DeathType);
			}
		}
	}

	public void Die(DeathController.DeathType type = DeathController.DeathType.Normal)
	{
		this.SetWantedItem(Hand.Left, null, true);
		this.SetWantedItem(Hand.Right, null, true);
		DeathController.Get().m_DeathType = type;
		this.UpdateHUDDeathState();
		this.StartController(PlayerControllerType.Death);
		if (DifficultySettings.ActivePreset.m_PermaDeath)
		{
			SaveGame.Save();
		}
	}

	private void UpdateHUDDeathState()
	{
		HUDDeath.Get().ClearStateAll();
		if (PlayerConditionModule.Get().m_Oxygen <= 0f)
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Drowning);
		}
		if (PlayerConditionModule.Get().IsNutritionFatCriticalLevel() || PlayerConditionModule.Get().IsNutritionCarboCriticalLevel() || PlayerConditionModule.Get().IsNutritionProteinsCriticalLevel() || PlayerConditionModule.Get().IsHydrationCriticalLevel())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Macronutritients);
		}
		if (this.m_DiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).IsActive())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.FoodPoison);
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < this.m_InjuryModule.m_Injuries.Count; i++)
		{
			flag = true;
			Injury injury = this.m_InjuryModule.m_Injuries[i];
			if (injury.m_Type == InjuryType.VenomBite || injury.m_Type == InjuryType.SnakeBite)
			{
				flag2 = true;
			}
			if (injury.m_AIDamager == AI.AIID.Savage || injury.m_AIDamager == AI.AIID.Hunter || injury.m_AIDamager == AI.AIID.Spearman || injury.m_AIDamager == AI.AIID.Thug)
			{
				HUDDeath.Get().SetState(HUDDeath.DeathState.Tribe);
			}
			if (injury.m_AIDamager == AI.AIID.Jaguar || injury.m_AIDamager == AI.AIID.Puma)
			{
				HUDDeath.Get().SetState(HUDDeath.DeathState.Predator);
			}
			if (injury.m_State == InjuryState.Infected)
			{
				HUDDeath.Get().SetState(HUDDeath.DeathState.Infection);
			}
		}
		if (flag2)
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Poison);
		}
		if (this.m_DiseasesModule.GetDisease(ConsumeEffect.Fever).IsActive())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Fever);
		}
		if (this.m_DiseasesModule.GetDisease(ConsumeEffect.ParasiteSickness).IsActive())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.ParasiteSickness);
		}
		if (this.m_DiseasesModule.GetDisease(ConsumeEffect.Insomnia).IsActive())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Insomnia);
		}
		if (flag)
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Wounds);
		}
		if ((float)PlayerSanityModule.Get().m_Sanity < PlayerSanityModule.Get().GetAIHallucinationsSanityLevel())
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Sanity);
		}
		if (DeathController.Get().m_DeathType == DeathController.DeathType.Fall)
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Fall);
		}
		if (DeathController.Get().m_DeathType == DeathController.DeathType.Caiman)
		{
			HUDDeath.Get().SetState(HUDDeath.DeathState.Caiman);
		}
	}

	private void UpdateDiarrhea()
	{
		Diarrhea diarrhea = this.m_DiarrheaController.GetDiarrhea();
		if (diarrhea == null || (!diarrhea.IsEffect() && this.m_DiarrheaController.IsActive()))
		{
			this.StopController(PlayerControllerType.Diarrhea);
		}
		if (diarrhea != null && diarrhea.IsActive() && diarrhea.IsEffect() && !this.m_DiarrheaController.IsActive())
		{
			this.StartController(PlayerControllerType.Diarrhea);
		}
	}

	public FPPController GetFPPController()
	{
		return this.m_FPPController;
	}

	public LookController GetLookController()
	{
		return this.m_LookController;
	}

	public override float GetHealth()
	{
		return this.m_ConditionModule.GetEnergy();
	}

	public void ScenarioHeal()
	{
		PlayerConditionModule.Get().ResetParams();
		PlayerInjuryModule.Get().ResetInjuries();
		PlayerDiseasesModule.Get().HealAllDiseases();
	}

	private void UpdateLeavesPusher()
	{
		LeavesPusher.Get().Push(base.gameObject, 0.5f, new Vector3?(Vector3.up * 0.5f));
	}

	public void OnItemDestroyed(Item item)
	{
		if (item != null && item.Equals(this.GetCurrentItem(Hand.Right)))
		{
			item.enabled = true;
			this.DropItem(item);
			string chatter_name = "Player_tool_destroyed_" + UnityEngine.Random.Range(1, 5).ToString();
			ChatterManager.Get().Play(chatter_name, 0f);
		}
	}

	public override bool IsRunning()
	{
		return this.m_FPPController.IsRunning();
	}

	public override bool IsWalking()
	{
		return this.m_FPPController.IsWalking();
	}

	public override bool IsDuck()
	{
		return this.m_FPPController.IsDuck();
	}

	public Transform GetLHand()
	{
		return this.m_LHand;
	}

	public Transform GetRHand()
	{
		return this.m_RHand;
	}

	public Transform GetSpine1()
	{
		return this.m_Spine1;
	}

	public void OnLanding(Vector3 speed)
	{
		base.GetComponent<PlayerInjuryModule>().OnLanding(speed);
	}

	public float GetFoodPoison()
	{
		return this.m_DiseasesModule.GetFoodPoisonLevel();
	}

	public bool IsOnBoat()
	{
		return base.GetComponent<BoatController>().IsActive();
	}

	public bool CanStartBodyInspection()
	{
		return (!WeaponSpearController.Get().IsActive() || !(WeaponSpearController.Get().GetImpaledObject() != null)) && !MakeFireController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && (!CraftingController.Get().IsActive() || !CraftingController.Get().m_InProgress) && (!FishingController.Get().IsActive() || !FishingController.Get().IsFishingInProgress()) && (!this.m_IsInAir || NotepadController.Get().IsActive() || Inventory3DManager.Get().IsActive() || BodyInspectionController.Get().IsActive()) && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && !ScenarioManager.Get().IsDream() && !this.m_CurrentLift && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	public bool CanSleep()
	{
		return !this.m_SleepBlocked && !MakeFireController.Get().IsActive() && WeaponMeleeController.Get().CanBeInterrupted() && !this.m_IsInAir && !InsectsController.Get().IsActive() && !this.m_Animator.GetBool(Player.Get().m_CleanUpHash) && PlayerStateModule.Get().m_State != PlayerStateModule.State.Combat && !ScenarioManager.Get().IsDream() && !this.m_CurrentLift && !DialogsManager.Get().IsAnyDialogPlaying() && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	public void SetScenarioPositionObject(GameObject obj)
	{
		if (obj && !this.m_ScenarioPositionObject)
		{
			this.BlockMoves();
		}
		else if (!obj && this.m_ScenarioPositionObject)
		{
			this.UnblockMoves();
		}
		this.m_ScenarioPositionObject = obj;
	}

	public void ScenarioBlockHUDWheel()
	{
		this.m_WheelHUDBlocked = true;
	}

	public void ScenarioUnblockHUDWheel()
	{
		this.m_WheelHUDBlocked = false;
	}

	public bool CanInvokeWheelHUD()
	{
		return !this.m_WheelHUDBlocked && (!WeaponSpearController.Get().IsActive() || !(WeaponSpearController.Get().GetImpaledObject() != null)) && !SleepController.Get().IsSleeping() && !CraftingController.Get().m_InProgress && !CutscenesManager.Get().IsCutscenePlaying() && (!BodyInspectionController.Get().IsActive() || BodyInspectionController.Get().CanLeave()) && !InsectsController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !this.m_Animator.GetBool(this.m_CleanUpHash) && !MakeFireController.Get().IsActive() && (!this.GetCurrentItem(Hand.Right) || this.GetCurrentItem(Hand.Right).GetInfoID() != ItemID.Fire) && !this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && !HUDReadableItem.Get().enabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && !ScenarioManager.Get().IsDreamOrPreDream() && !HUDSelectDialog.Get().enabled;
	}

	public void StartDream(string dream_name)
	{
		if (DreamSpawner.Find(dream_name) == null)
		{
			return;
		}
		this.m_DreamDuration = -1f;
		this.m_DreamToActivate = dream_name;
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.StartDreamInternal), 1.5f, null);
	}

	public void StartDreamInternal()
	{
		this.m_CurrentLift = null;
		this.m_LastPosBeforeDream = base.gameObject.transform.position;
		this.m_LastRotBeforeDream = base.gameObject.transform.rotation;
		DreamSpawner dreamSpawner = DreamSpawner.Find(this.m_DreamToActivate);
		base.gameObject.transform.position = dreamSpawner.gameObject.transform.position;
		base.gameObject.transform.rotation = dreamSpawner.gameObject.transform.rotation;
		this.m_LastPosOnGround = base.gameObject.transform.position;
		this.m_DreamActive = true;
		this.m_FogModeToRestore = RenderSettings.fogMode;
		this.m_FogStartDistanceToRestore = RenderSettings.fogStartDistance;
		this.m_FogEndDistanceToRestore = RenderSettings.fogEndDistance;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = 4f;
		RenderSettings.fogEndDistance = 40f;
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 1.5f);
	}

	public void StopDream()
	{
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.StopDreamInternal), 1.5f, null);
		if (this.m_DreamDuration >= 0f)
		{
			SleepController.Get().WakeUp(true, false);
		}
	}

	private void StopDreamInternal()
	{
		base.gameObject.transform.position = this.m_LastPosBeforeDream;
		base.gameObject.transform.rotation = this.m_LastRotBeforeDream;
		this.m_LastPosOnGround = base.gameObject.transform.position;
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 1.5f);
		RenderSettings.fogMode = this.m_FogModeToRestore;
		RenderSettings.fogStartDistance = this.m_FogStartDistanceToRestore;
		RenderSettings.fogEndDistance = this.m_FogEndDistanceToRestore;
		this.m_ResetDreamParams = true;
	}

	public void ResetDreamParams()
	{
		this.m_DreamActive = false;
		this.m_DreamToActivate = string.Empty;
	}

	private void UpdateInputs()
	{
		if (GreenHellGame.DEBUG)
		{
			if (!MainLevel.Instance.IsPause() && Input.GetKeyDown(KeyCode.K) && Input.GetKey(KeyCode.RightControl))
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					ObjectivesManager.Get().ActivateObjective("Obj_FindKate", true);
				}
				else
				{
					ObjectivesManager.Get().DeactivateObjective("Obj_FindKate");
				}
			}
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha5))
			{
				this.StorePlayerTransforms();
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha6))
			{
				this.RestorePlayerTransforms();
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Quote))
			{
				base.GetComponent<BodyInspectionController>().DebugAttachLeeches();
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Comma))
			{
				base.GetComponent<BodyInspectionController>().DebugAttachWorms();
			}
			else if (Input.GetKeyDown(KeyCode.Backslash))
			{
				SortedDictionary<string, string>.Enumerator enumerator = GreenHellGame.Instance.GetLocalization().GetLocalizedtexts().GetEnumerator();
				while (enumerator.MoveNext())
				{
					Localization localization = GreenHellGame.Instance.GetLocalization();
					KeyValuePair<string, string> keyValuePair = enumerator.Current;
					string text = localization.Get(keyValuePair.Key, true);
					if (text.Contains("["))
					{
						Debug.Log(text);
					}
				}
			}
		}
		bool flag = this.m_PlayerControllers[28].IsActive();
		bool flag2 = this.CanShowWatch();
		if (flag && !flag2)
		{
			this.StopController(PlayerControllerType.Watch);
			flag = false;
		}
		if (!GreenHellGame.Instance.m_Settings.m_ToggleWatch)
		{
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Watch))
			{
				if (!flag && flag2)
				{
					this.StartController(PlayerControllerType.Watch);
					return;
				}
			}
			else if (flag)
			{
				this.StopController(PlayerControllerType.Watch);
			}
		}
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public void OnDropHeavyItem()
	{
		if (this.m_ShowNotepadSheduled)
		{
			this.StartController(PlayerControllerType.Notepad);
			this.m_NotepadActivityChanged = true;
			this.m_ShowNotepadSheduled = false;
			return;
		}
		if (this.m_ShowMapSheduled)
		{
			this.StartController(PlayerControllerType.Map);
			this.m_MapActivityChanged = true;
			this.m_ShowMapSheduled = false;
		}
	}

	public void OnHideMap()
	{
		if (this.m_ShowNotepadSheduled)
		{
			this.StartController(PlayerControllerType.Notepad);
			this.m_NotepadActivityChanged = true;
			this.m_ShowNotepadSheduled = false;
		}
	}

	public void OnHideNotepad()
	{
		if (this.m_ShowMapSheduled)
		{
			this.StartController(PlayerControllerType.Map);
			this.m_MapActivityChanged = true;
			this.m_ShowMapSheduled = false;
		}
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.ShowMap)
		{
			if (!this.m_MapActivityChanged && this.CanShowMap())
			{
				if (NotepadController.Get().IsActive())
				{
					if (this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.m_NotepadIdleHash)
					{
						NotepadController.Get().Hide();
						this.m_ShowMapSheduled = true;
						this.m_ShowNotepadSheduled = false;
					}
				}
				else if (HeavyObjectController.Get().IsActive())
				{
					this.m_ShowMapSheduled = true;
					this.m_ShowNotepadSheduled = false;
					HeavyObjectController.Get().Drop();
				}
				else if (!MapController.Get().IsActive())
				{
					Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
					if (currentItem != null && currentItem.m_Info.IsHeavyObject())
					{
						Player.Get().DropItem(currentItem);
					}
					this.StartController(PlayerControllerType.Map);
					this.m_MapActivityChanged = true;
				}
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.ShowNotepad)
		{
			if (!this.m_NotepadActivityChanged && this.CanShowNotepad())
			{
				if (MapController.Get().IsActive())
				{
					if (this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.m_MapIdleHash)
					{
						this.m_ShowNotepadSheduled = true;
						this.m_ShowMapSheduled = false;
						MapController.Get().Hide();
					}
				}
				else if (HeavyObjectController.Get().IsActive())
				{
					this.m_ShowNotepadSheduled = true;
					this.m_ShowMapSheduled = false;
					HeavyObjectController.Get().Drop();
				}
				else if (!NotepadController.Get().IsActive())
				{
					this.StartController(PlayerControllerType.Notepad);
					this.m_NotepadActivityChanged = true;
				}
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.QuickEquip0 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(0, BackpackPocket.Left));
		}
		else if (action_data.m_Action == InputsManager.InputAction.QuickEquip1 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(1, BackpackPocket.Left));
		}
		else if (action_data.m_Action == InputsManager.InputAction.QuickEquip2 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(2, BackpackPocket.Left));
		}
		else if (action_data.m_Action == InputsManager.InputAction.QuickEquip3 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(3, BackpackPocket.Left));
		}
		else if (action_data.m_Action == InputsManager.InputAction.PrevItemOrPage)
		{
			if (this.CanSelectNextPrevSlot())
			{
				this.SelectNextPrevSlot(false);
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.NextItemOrPage)
		{
			if (this.CanSelectNextPrevSlot())
			{
				this.SelectNextPrevSlot(true);
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.ThrowStone)
		{
			Item item = InventoryBackpack.Get().FindItem(ItemID.Stone);
			if (item && this.CanThrowStone())
			{
				this.HideWeapon();
				this.SetWantedItem(item, true);
				this.StopController(PlayerControllerType.Item);
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Watch && GreenHellGame.Instance.m_Settings.m_ToggleWatch)
		{
			if (this.m_PlayerControllers[28].IsActive())
			{
				this.StopController(PlayerControllerType.Watch);
			}
			else if (this.CanShowWatch())
			{
				this.StartController(PlayerControllerType.Watch);
			}
		}
		if ((action_data.m_Action == InputsManager.InputAction.HideMap || action_data.m_Action == InputsManager.InputAction.Quit || action_data.m_Action == InputsManager.InputAction.AdditionalQuit) && !this.m_MapActivityChanged && MapController.Get().IsActive() && MapController.Get().CanDisable())
		{
			MapController.Get().Hide();
			this.m_MapActivityChanged = true;
		}
		if ((action_data.m_Action == InputsManager.InputAction.HideNotepad || action_data.m_Action == InputsManager.InputAction.Quit || action_data.m_Action == InputsManager.InputAction.AdditionalQuit) && !this.m_NotepadActivityChanged && this.m_NotepadController.IsActive() && this.m_NotepadController.CanDisable())
		{
			this.m_NotepadController.Hide();
			this.m_NotepadActivityChanged = true;
		}
	}

	private bool CanEquipItem()
	{
		return !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && (!FishingController.Get().IsActive() || FishingController.Get().CanHideRod()) && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	private bool CanSelectNextPrevSlot()
	{
		return (!Inventory3DManager.Get().IsActive() || InventoryBackpack.Get().m_ActivePocket == BackpackPocket.Left) && (!Inventory3DManager.Get().IsActive() || !GreenHellGame.IsPadControllerActive()) && !HUDSelectDialog.Get().enabled && !NotepadController.Get().IsActive() && !MapController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !EatingController.Get().IsActive() && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && !CraftingController.Get().IsActive() && !ConsciousnessController.Get().IsActive() && (!FishingController.Get().IsActive() || !FishingController.Get().IsFishingInProgress()) && !GrapplingHookController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !MudMixerController.Get().IsActive() && !HUDReadableItem.Get().enabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	private void SelectNextPrevSlot(bool next)
	{
		int num = -1;
		for (int i = 0; i < 5; i++)
		{
			if (InventoryBackpack.Get().GetSlotByIndex(i, BackpackPocket.Left) == InventoryBackpack.Get().m_EquippedItemSlot)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			bool flag = false;
			if (!Inventory3DManager.Get().IsActive() && this.GetCurrentItem() == null)
			{
				int num2 = num;
				num2 += (next ? 1 : -1);
				if (num2 >= 5)
				{
					num2 = 0;
				}
				else if (num2 < 0)
				{
					num2 = 4;
				}
				for (int j = 0; j < 5; j++)
				{
					if (InventoryBackpack.Get().GetSlotByIndex(num2, BackpackPocket.Left).m_Item)
					{
						num = num2;
						flag = true;
						break;
					}
					num2 += (next ? 1 : -1);
					if (num2 >= 5)
					{
						num2 = 0;
					}
					else if (num2 < 0)
					{
						num2 = 4;
					}
				}
			}
			if (!flag)
			{
				num += (next ? 1 : -1);
			}
			if (num >= 5)
			{
				num = 0;
			}
			else if (num < 0)
			{
				num = 4;
			}
			if (!flag && num == 4 && !InventoryBackpack.Get().GetSlotByIndex(num, BackpackPocket.Left).m_Item)
			{
				num = (next ? 0 : 3);
			}
			ItemSlot slotByIndex = InventoryBackpack.Get().GetSlotByIndex(num, BackpackPocket.Left);
			if (slotByIndex != null)
			{
				this.Equip(slotByIndex);
			}
		}
	}

	public bool CanThrowStone()
	{
		return !Inventory3DManager.Get().gameObject.activeSelf && !this.m_DreamActive && !SleepController.Get().IsActive() && !this.m_MapController.IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !BodyInspectionController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !ConsciousnessController.Get().IsActive() && !HeavyObjectController.Get().IsActive() && !InsectsController.Get().IsActive() && !this.m_Animator.GetBool(this.m_CleanUpHash) && !CraftingController.Get().IsActive() && !MakeFireController.Get().IsActive() && (!this.GetCurrentItem(Hand.Right) || this.GetCurrentItem(Hand.Right).GetInfoID() != ItemID.Fire) && !this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && !CutscenesManager.Get().IsCutscenePlaying() && !ScenarioManager.Get().IsDream() && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && (!GreenHellGame.IsPadControllerActive() || !WeaponSpearController.Get().IsActive());
	}

	private void StorePlayerTransforms()
	{
		this.m_StoredPos = base.gameObject.transform.position;
		this.m_StoredRotation = base.gameObject.transform.rotation;
		this.m_StoredLookDev = this.m_LookController.m_LookDev;
	}

	private void RestorePlayerTransforms()
	{
		base.gameObject.transform.position = this.m_StoredPos;
		base.gameObject.transform.rotation = this.m_StoredRotation;
		this.m_LastPosOnGround = base.gameObject.transform.position;
		this.m_LookController.m_WantedLookDev = this.m_StoredLookDev;
	}

	public void GetInputActions(ref List<int> actions)
	{
		for (int i = 0; i < 44; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].GetInputActions(ref actions);
			}
		}
	}

	public void AddItemToInventory(string item_name)
	{
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), item_name);
		Item item = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
		item.StaticPhxRequestReset();
		InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
	}

	public void ShowDivingMask()
	{
		this.m_ShowDivingMask = true;
	}

	public void HideDivingMask()
	{
		this.m_ShowDivingMask = false;
	}

	public void ShowBiohazardMask()
	{
		this.m_ShowBiohazardMask = true;
	}

	public void HideBiohazardMask()
	{
		this.m_ShowBiohazardMask = false;
	}

	public bool CanStartCrafting()
	{
		return !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_InsectsController.IsActive() && !VomitingController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !HitReactionController.Get().IsActive() && !ConsciousnessController.Get().IsActive() && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && !ScenarioManager.Get().IsDream() && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	private bool CanShowWatch()
	{
		return this.m_WatchUnlocked && !this.m_ScenarioWatchBlocked && !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_NotepadController.IsActive() && !this.m_InsectsController.IsActive() && !VomitingController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CraftingController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !ConsciousnessController.Get().IsActive() && !BodyInspectionController.Get().IsActive() && !this.m_Animator.GetBool(this.m_CleanUpHash) && !WalkieTalkieController.Get().IsActive() && !ScenarioManager.Get().IsDreamOrPreDream() && !HUDReadableItem.Get().enabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && (!this.m_ActiveFightController || !this.m_ActiveFightController.IsAttack()) && !HeavyObjectController.Get().IsActive() && !InputsManager.Get().m_TextInputActive;
	}

	private void UpdateWatchObject()
	{
		if (this.m_Watch.activeSelf != this.m_WatchUnlocked)
		{
			this.m_Watch.SetActive(this.m_WatchUnlocked);
		}
	}

	public void UnlockWatch()
	{
		this.m_WatchUnlocked = true;
	}

	public void LockWatch()
	{
		this.m_WatchUnlocked = false;
	}

	public void ScenarioBlockWatch()
	{
		this.m_ScenarioWatchBlocked = true;
	}

	public void ScenarioUnblockWatch()
	{
		this.m_ScenarioWatchBlocked = false;
	}

	public void UnlockNotepad()
	{
		this.m_NotepadUnlocked = true;
	}

	public void LockNotepad()
	{
		this.m_NotepadUnlocked = false;
	}

	public bool CanShowNotepad()
	{
		return this.m_NotepadUnlocked && !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !VomitingController.Get().IsActive() && !InsectsController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && WeaponMeleeController.Get().CanBeInterrupted() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !ConsciousnessController.Get().IsActive() && !this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && (!FishingController.Get().IsActive() || FishingController.Get().m_State == FishingController.State.Idle) && !this.m_Animator.GetBool(this.m_CleanUpHash) && !BodyInspectionController.Get().IsBandagingInProgress() && !ScenarioManager.Get().IsDream() && !this.m_CurrentLift && !HUDReadableItem.Get().enabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && (!this.m_ActiveFightController || !this.m_ActiveFightController.IsAttack());
	}

	public void UnlockMap()
	{
		this.m_MapUnlocked = true;
	}

	public void LockMap()
	{
		this.m_MapUnlocked = false;
	}

	public bool CanShowMap()
	{
		return !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !VomitingController.Get().IsActive() && !InsectsController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !MudMixerController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !ConsciousnessController.Get().IsActive() && !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash) && MapTab.Get().GetUnlockedPagesCount() != 0 && !ScenarioManager.Get().IsDream() && !this.m_CurrentLift && !HUDReadableItem.Get().enabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && (!this.m_ActiveFightController || !this.m_ActiveFightController.IsAttack()) && (!GreenHellGame.IsPadControllerActive() || (!NotepadController.Get().IsActive() && !Inventory3DManager.Get().IsActive() && !HUDItem.Get().m_Active && !HUDSelectDialogNode.Get().enabled && !HUDSelectDialog.Get().enabled));
	}

	public bool IsWatchControllerActive()
	{
		return this.m_WatchController.IsActive();
	}

	public bool IsLookingAtMap()
	{
		return this.m_MapController.IsActive();
	}

	public bool IsLookingAtMapPage(string page_name)
	{
		return this.m_MapController.IsActive() && MapTab.Get().GetCurrentPageName().ToLower() == page_name.ToLower();
	}

	public void BlockSleeping()
	{
		this.m_SleepBlocked = true;
	}

	public void UnblockSleeping()
	{
		this.m_SleepBlocked = false;
	}

	public void BlockInspection()
	{
		this.m_InspectionBlocked = true;
	}

	public void UnblockInspection()
	{
		this.m_InspectionBlocked = false;
	}

	public void ThrowItem(Hand hand)
	{
		Item currentItem = this.GetCurrentItem(hand);
		if (!currentItem)
		{
			return;
		}
		this.SetWantedItem(hand, null, true);
		this.StartControllerInternal();
		this.ThrowItem(currentItem);
	}

	public void ThrowItem(Item item)
	{
		if (!item)
		{
			return;
		}
		Physics.IgnoreCollision(item.m_Collider, this.m_Collider);
		item.gameObject.SetActive(true);
		item.transform.parent = null;
		item.UpdatePhx();
		item.m_RequestThrow = true;
		item.m_Thrower = base.gameObject;
		this.DecreaseStamina(StaminaDecreaseReason.Throw);
		PlayerAudioModule.Get().PlayAttackSound(1f, false);
	}

	public void Kill()
	{
		this.m_ConditionModule.IncreaseHP(-1E+07f);
	}

	public void TenPointsOfDamage()
	{
		this.m_ConditionModule.IncreaseHP(-10f);
	}

	public void TwentyFivePointsOfDamage()
	{
		this.m_ConditionModule.IncreaseHP(-25f);
	}

	public void ScenarioGiveDamage(float damage)
	{
		base.GiveDamage(null, null, damage, Vector3.forward, DamageType.None, 0, false);
	}

	public override bool TakeDamage(DamageInfo info)
	{
		if (ScenarioManager.Get().IsDreamOrPreDream())
		{
			return false;
		}
		if (this.IsDead())
		{
			return false;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return false;
		}
		if (this.m_LastLoadTime > Time.time - 1f)
		{
			return false;
		}
		if (info.m_Damager)
		{
			AI component = info.m_Damager.GetComponent<AI>();
			if (component)
			{
				if (HUDItem.Get().m_Active)
				{
					HUDItem.Get().DelayedExecute();
				}
				if (info.m_AIType == AI.AIID.None)
				{
					info.m_AIType = component.m_ID;
				}
				if (component.IsHuman())
				{
					if (this.IsBlock() && (!info.m_DamageItem || !info.m_DamageItem.m_Info.IsArrow()) && Vector3.Angle((component.transform.position - base.transform.position).GetNormalized2D(), base.transform.forward.GetNormalized2D()) < 70f)
					{
						info.m_Blocked = true;
						Item currentItem = this.GetCurrentItem();
						if (currentItem)
						{
							DamageInfo damageInfo = new DamageInfo();
							ObjectMaterial objectMaterial = null;
							if (info.m_DamageItem != null)
							{
								objectMaterial = info.m_DamageItem.gameObject.GetComponent<ObjectMaterial>();
							}
							EObjectMaterial mat = (objectMaterial == null) ? EObjectMaterial.Unknown : objectMaterial.m_ObjectMaterial;
							damageInfo.m_Damage = currentItem.m_Info.m_DamageSelf * ObjectMaterial.GetDamageSelfMul(mat);
							currentItem.TakeDamage(damageInfo);
						}
					}
					if (!info.m_Blocked)
					{
						PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
					}
				}
				if (this.CanStartHitReaction())
				{
					if (this.m_HitReactionController.enabled)
					{
						this.m_HitReactionController.OnTakeDamage();
					}
					else
					{
						this.StartController(PlayerControllerType.HitReaction);
					}
				}
				if (component.IsHunter() && info.m_DamageItem && info.m_DamageItem.m_Info.IsArrow())
				{
					((HunterAI)component).OnHitPlayerByArrow();
				}
				PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.AIDamage);
				HUDItem.Get().Deactivate();
			}
			else if (info.m_DamageItem && info.m_DamageItem.IsSpikes())
			{
				if (this.CanStartHitReaction())
				{
					if (this.m_HitReactionController.enabled)
					{
						this.m_HitReactionController.OnTakeDamage();
					}
					else
					{
						this.StartController(PlayerControllerType.HitReaction);
					}
				}
				HUDItem.Get().Deactivate();
			}
		}
		for (int i = 0; i < 44; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].OnTakeDamage(info);
			}
		}
		return base.TakeDamage(info);
	}

	private bool CanStartHitReaction()
	{
		return !this.m_WeaponController.IsAttack() && !CraftingController.Get().IsActive() && !CraftingManager.Get().IsActive() && !SleepController.Get().IsActive() && (!WeaponSpearController.Get().IsActive() || (!WeaponSpearController.Get().m_ImpaledObject && !WeaponSpearController.Get().m_ItemBody)) && !SwimController.Get().IsActive();
	}

	public bool IsBlock()
	{
		return this.m_ActiveFightController && this.m_ActiveFightController.IsBlock();
	}

	private void DebugUpdate()
	{
		if (MenuInGameManager.Get().IsAnyScreenVisible())
		{
			return;
		}
		if (GreenHellGame.ROADSHOW_DEMO && Input.GetKeyDown(KeyCode.Backslash) && Time.time - this.m_LastAddInjuryTime > 1f)
		{
			PlayerInjuryModule component = base.gameObject.GetComponent<PlayerInjuryModule>();
			BIWoundSlot freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RHand, InjuryType.Worm, true);
			component.AddInjury(InjuryType.Worm, InjuryPlace.RHand, freeWoundSlot, InjuryState.Open, 0, null, null);
		}
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backslash) && Time.time - this.m_LastAddInjuryTime > 1f)
		{
			PlayerInjuryModule component2 = base.gameObject.GetComponent<PlayerInjuryModule>();
			BIWoundSlot freeWoundSlot2 = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RHand, InjuryType.Leech, true);
			component2.AddInjury(InjuryType.Leech, InjuryPlace.RHand, freeWoundSlot2, InjuryState.Open, 0, null, null);
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backspace) && Time.time - this.m_LastAddInjuryTime > 1f)
		{
			this.TakeDamage(new DamageInfo
			{
				m_Damage = 30f,
				m_DamageType = DamageType.Claws
			});
			this.m_LastAddInjuryTime = Time.time;
			return;
		}
		if (Input.GetKeyDown(KeyCode.V))
		{
			HintsManager.Get().ShowAllHints();
			return;
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			this.StartDream("Goldmine_dream_spawner");
			return;
		}
		if (Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.F))
		{
			this.SetWantedItem(Hand.Right, ItemsManager.Get().CreateItem(ItemID.Fire, false, Vector3.zero, Quaternion.identity), true);
			return;
		}
		if (!Input.GetKeyDown(KeyCode.F10))
		{
			if (Input.GetKeyDown(KeyCode.F11))
			{
				this.ItemsFromHandsPutToInventory();
			}
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf)
		{
			CraftingManager.Get().Deactivate();
			return;
		}
		CraftingManager.Get().Activate();
	}

	private void ParseAdditiveAnimations()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse(Player.s_AdditiveAnimationsScriptPath, true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			string key2 = string.Empty;
			if (key.GetName() == "AnimName")
			{
				key2 = key.GetVariable(0).SValue;
			}
			if (!this.m_Animations.ContainsKey(key2))
			{
				this.m_Animations[key2] = new Dictionary<Transform, List<SimpleTransform>>();
			}
			int j = 0;
			while (j < key.GetKeysCount())
			{
				Key key3 = key.GetKey(j);
				Transform transform = null;
				if (!(key3.GetName() == "Transform"))
				{
					goto IL_E7;
				}
				transform = base.gameObject.transform.FindDeepChild(key3.GetVariable(0).SValue);
				if (!(transform == null))
				{
					if (!this.m_Animations[key2].ContainsKey(transform))
					{
						this.m_Animations[key2][transform] = new List<SimpleTransform>();
						goto IL_E7;
					}
					goto IL_E7;
				}
				IL_1D6:
				j++;
				continue;
				IL_E7:
				for (int k = 0; k < key3.GetKeysCount(); k += 2)
				{
					Key key4 = key3.GetKey(k);
					Key key5 = key3.GetKey(k + 1);
					SimpleTransform item = default(SimpleTransform);
					if (key4.GetName() == "Quat")
					{
						item.m_Rot = new Quaternion(key4.GetVariable(1).FValue, key4.GetVariable(2).FValue, key4.GetVariable(3).FValue, key4.GetVariable(4).FValue);
					}
					if (key5.GetName() == "Pos")
					{
						item.m_Pos = new Vector3(key5.GetVariable(1).FValue, key5.GetVariable(2).FValue, key5.GetVariable(3).FValue);
					}
					this.m_Animations[key2][transform].Add(item);
				}
				goto IL_1D6;
			}
		}
	}

	private void StoreAdditiveAnimationStartFrame(Dictionary<Transform, List<SimpleTransform>> dict)
	{
		this.m_AdditiveAnimationStartFrame.Clear();
		Dictionary<Transform, List<SimpleTransform>>.Enumerator enumerator = dict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Transform, List<SimpleTransform>> keyValuePair = enumerator.Current;
			Transform key = keyValuePair.Key;
			SimpleTransform value;
			value.m_Pos = key.localPosition;
			value.m_Rot = key.localRotation;
			this.m_AdditiveAnimationStartFrame.Add(key, value);
		}
		enumerator.Dispose();
	}

	private void UpdateAdditiveAnimations()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(3);
		this.m_Animator.GetCurrentAnimatorClipInfo(3, this.m_AnimatorClipInfo);
		bool flag = false;
		if (!this.m_PostBlendAdditiveAnimation)
		{
			for (int i = 0; i < this.m_AnimatorClipInfo.Count; i++)
			{
				string name = this.m_AnimatorClipInfo[i].clip.name;
				if (this.m_Animations.ContainsKey(name))
				{
					Dictionary<Transform, List<SimpleTransform>> dictionary = this.m_Animations[name];
					if (!this.m_AdditiveAnimationActiveLastFrame)
					{
						this.StoreAdditiveAnimationStartFrame(dictionary);
					}
					float num = currentAnimatorStateInfo.normalizedTime * currentAnimatorStateInfo.length;
					int num2 = (int)Mathf.Floor(currentAnimatorStateInfo.normalizedTime * currentAnimatorStateInfo.length / Player.s_AdditiveAnimationSampleInterval);
					int num3 = num2 + 1;
					Dictionary<Transform, List<SimpleTransform>>.Enumerator enumerator = dictionary.GetEnumerator();
					while (enumerator.MoveNext())
					{
						KeyValuePair<Transform, List<SimpleTransform>> keyValuePair = enumerator.Current;
						Transform key = keyValuePair.Key;
						keyValuePair = enumerator.Current;
						List<SimpleTransform> value = keyValuePair.Value;
						Quaternion quaternion = Quaternion.Inverse(value[0].m_Rot);
						Vector3 pos = value[0].m_Pos;
						if (num2 >= value.Count)
						{
							num2 = value.Count - 1;
						}
						if (num3 >= value.Count)
						{
							num3 = value.Count - 1;
						}
						float num4 = (num - (float)num2 * Player.s_AdditiveAnimationSampleInterval) / Player.s_AdditiveAnimationSampleInterval;
						float b = 0.5f;
						float proportionalClamp = CJTools.Math.GetProportionalClamp(1f, 0f, currentAnimatorStateInfo.normalizedTime, b, 1f);
						key.localRotation = Quaternion.Slerp(this.m_AdditiveAnimationStartFrame[key].m_Rot, key.localRotation, 1f - proportionalClamp);
						key.localPosition = this.m_AdditiveAnimationStartFrame[key].m_Pos + (this.m_AdditiveAnimationStartFrame[key].m_Pos - key.localPosition) * (1f - proportionalClamp);
						Quaternion rhs = Quaternion.Slerp(value[num2].m_Rot, value[num3].m_Rot, num4);
						quaternion *= rhs;
						key.localRotation *= quaternion;
						Vector3 vector = value[num2].m_Pos + (value[num3].m_Pos - value[num2].m_Pos) * num4;
						vector -= pos;
						key.localPosition += vector;
						SimpleTransform value2;
						value2.m_Rot = key.localRotation;
						value2.m_Pos = key.localPosition;
						this.m_LastAdditiveAnimationFrame[key] = value2;
					}
					enumerator.Dispose();
					this.m_LastAdditiveAnimationTime = Time.time;
					this.m_AdditiveAnimationActiveLastFrame = true;
					flag = true;
				}
			}
		}
		if (!flag && this.m_AdditiveAnimationActiveLastFrame)
		{
			this.m_AdditiveAnimationActiveLastFrame = false;
			this.m_PostBlendAdditiveAnimation = true;
		}
		if (this.m_PostBlendAdditiveAnimation)
		{
			if (Time.time - this.m_LastAdditiveAnimationTime < Player.s_AdditiveAnimationPostBlendTime && this.m_LastAdditiveAnimationTime != 0f)
			{
				float num5 = (Time.time - this.m_LastAdditiveAnimationTime) / Player.s_AdditiveAnimationPostBlendTime;
				for (int j = 0; j < this.m_LastAdditiveAnimationFrame.Count; j++)
				{
					Transform key2 = this.m_LastAdditiveAnimationFrame.ElementAt(j).Key;
					Quaternion localRotation = Quaternion.Slerp(this.m_LastAdditiveAnimationFrame[key2].m_Rot, key2.localRotation, num5);
					key2.localRotation = localRotation;
					Vector3 localPosition = this.m_LastAdditiveAnimationFrame[key2].m_Pos + (key2.localPosition - this.m_LastAdditiveAnimationFrame[key2].m_Pos) * num5;
					key2.localPosition = localPosition;
				}
				return;
			}
			this.m_PostBlendAdditiveAnimation = false;
		}
	}

	public void GetGPSCoordinates(out int gps_lat, out int gps_long)
	{
		this.GetGPSCoordinates(base.transform.position, out gps_lat, out gps_long);
	}

	public void GetGPSCoordinates(Vector3 position, out int gps_lat, out int gps_long)
	{
		Vector3 position2 = MapTab.Get().m_WorldZeroDummy.position;
		Vector3 position3 = MapTab.Get().m_WorldOneDummy.position;
		float num = position3.x - position2.x;
		float num2 = position3.z - position2.z;
		float num3 = num / 35f;
		float num4 = num2 / 27f;
		Vector3 vector = MapTab.Get().m_WorldZeroDummy.InverseTransformPoint(position);
		gps_lat = Mathf.FloorToInt(vector.x / num3) + 20;
		gps_long = Mathf.FloorToInt(vector.z / num4) + 14;
	}

	public void OnPlayerChangeScene(GameObject spawner)
	{
		this.BlockMoves();
		this.m_MovesBlockedOnChangeScene = true;
		this.m_ChangeSceneSpawner = spawner;
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.ChangeSceneShowLoadingScreen), 1.5f, null);
	}

	private void ChangeSceneShowLoadingScreen()
	{
		LoadingScreen.Get().Show(LoadingScreenState.ChangeScene);
		base.gameObject.transform.position = this.m_ChangeSceneSpawner.gameObject.transform.position;
		base.gameObject.transform.rotation = this.m_ChangeSceneSpawner.gameObject.transform.rotation;
		this.m_LastPosOnGround = base.gameObject.transform.position;
		Vector3 forward = this.m_ChangeSceneSpawner.gameObject.transform.forward;
		forward.y = 0f;
		float num = Vector3.Angle(forward, Vector3.forward);
		Vector2 zero = Vector2.zero;
		if (Vector3.Dot(Vector3.right, forward) < 0f)
		{
			num *= -1f;
		}
		zero.x = num;
		Player.Get().m_FPPController.SetLookDev(zero);
	}

	private void UpdateInLift()
	{
		if (!this.m_CurrentLift)
		{
			return;
		}
		Vector3 position = base.transform.position;
		position.y = this.m_CurrentLift.transform.position.y - this.m_CurrentLift.transform.localScale.y * 0.5f;
		base.transform.position = position;
	}

	public void StartAim(Player.AimType type, float final_dist = 18f)
	{
		HUDCrosshair.Get().m_FinalDistance = final_dist;
		HUDCrosshair.Get().ShowCrosshair();
		HUDCrosshair.Get().SetAimPower(0f);
		this.m_AimPower = 0f;
		this.m_Aim = true;
		this.m_AimType = type;
		this.m_StartAimTime = Time.time;
		this.UpdateAim();
		PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.Aim);
	}

	public void StopAim()
	{
		if (!this.m_Aim)
		{
			return;
		}
		HUDCrosshair.Get().HideCrosshair();
		this.m_Aim = false;
		this.m_StopAimTime = Time.time;
		if (Camera.main != null)
		{
			this.m_StopAimCameraMtx.SetColumn(0, Camera.main.transform.right);
			this.m_StopAimCameraMtx.SetColumn(1, Camera.main.transform.up);
			this.m_StopAimCameraMtx.SetColumn(2, Camera.main.transform.forward);
			this.m_StopAimCameraMtx.SetColumn(3, Camera.main.transform.position);
		}
	}

	private void UpdateAim()
	{
		if (!this.m_Aim)
		{
			return;
		}
		float b = 0f;
		switch (this.m_AimType)
		{
		case Player.AimType.Blowpipe:
			b = Skill.Get<BlowgunSkill>().GetAimDuration();
			break;
		case Player.AimType.Bow:
			b = 0.5f;
			break;
		case Player.AimType.Fishing:
			b = 0f;
			break;
		case Player.AimType.Item:
			b = Skill.Get<ThrowingSkill>().GetAimDuration();
			break;
		case Player.AimType.SpearHunting:
			b = Skill.Get<SpearSkill>().GetAimDuration();
			break;
		case Player.AimType.SpearFishing:
			b = Skill.Get<SpearFishingSkill>().GetAimDuration();
			break;
		}
		this.m_AimPower = CJTools.Math.GetProportionalClamp(0f, 1f, Time.time - this.m_StartAimTime, 0f, b);
		HUDCrosshair.Get().SetAimPower(this.m_AimPower);
	}

	public void RandomTeleport(List<GameObject> targets, bool show_loading)
	{
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		this.Teleport(targets[UnityEngine.Random.Range(0, targets.Count)], show_loading);
	}

	public void Teleport(GameObject target, bool show_loading)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		if (!target)
		{
			return;
		}
		if (base.GetComponent<LadderController>().IsActive())
		{
			this.StopController(PlayerControllerType.Ladder);
		}
		base.gameObject.transform.position = target.gameObject.transform.position;
		base.gameObject.transform.rotation = target.gameObject.transform.rotation;
		this.m_CharacterController.transform.position = target.gameObject.transform.position;
		this.m_CharacterController.transform.rotation = target.gameObject.transform.rotation;
		this.m_LastPosOnGround = base.gameObject.transform.position;
		Vector3 forward = base.gameObject.transform.forward;
		forward.y = 0f;
		float num = Vector3.Angle(forward, Vector3.forward);
		Vector2 zero = Vector2.zero;
		if (Vector3.Dot(Vector3.right, forward) < 0f)
		{
			num *= -1f;
		}
		zero.x = num;
		Player.Get().m_FPPController.SetLookDev(zero);
		if (show_loading)
		{
			LoadingScreen.Get().Show(LoadingScreenState.Teleport);
		}
		this.m_TeleportTime = Time.unscaledTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
		if (component)
		{
			WaterBoxManager.Get().OnEnterWater(component);
		}
		FreeHandsLadderTrigger component2 = other.gameObject.GetComponent<FreeHandsLadderTrigger>();
		if (component2 && !this.m_FreeHandsLadders.Contains(component2))
		{
			this.m_FreeHandsLadders.Add(component2);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
		if (component)
		{
			WaterBoxManager.Get().OnExitWater(component);
		}
		FreeHandsLadderTrigger component2 = other.gameObject.GetComponent<FreeHandsLadderTrigger>();
		if (component2 && this.m_FreeHandsLadders.Contains(component2))
		{
			this.m_FreeHandsLadders.Remove(component2);
		}
	}

	public void DeleteHeavyObjectsInHands()
	{
		Item item = this.m_CurrentItem[1];
		if (item && item.m_Info.IsHeavyObject())
		{
			((HeavyObject)item).DeleteAttachedHeavyObjects();
			this.SetWantedItem(Hand.Right, null, true);
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public void DeleteAllItems()
	{
		Item item = this.m_CurrentItem[1];
		if (item)
		{
			this.SetWantedItem(Hand.Right, null, true);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		Item item2 = this.m_CurrentItem[0];
		if (item2)
		{
			this.SetWantedItem(Hand.Left, null, true);
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		InventoryBackpack.Get().DeleteAllItems();
		CraftingManager.Get().DeleteAllItems();
		TriggerController.Get().m_TriggerToExecute = null;
		for (int i = 0; i < this.m_StoredItems.Length; i++)
		{
			if (this.m_StoredItems[i] != null)
			{
				UnityEngine.Object.Destroy(this.m_StoredItems[i].gameObject);
				this.m_StoredItems[i] = null;
			}
		}
	}

	public void Equip(ItemSlot slot)
	{
		if (Time.time - this.m_LastEquipTime < 0.5f)
		{
			return;
		}
		if (BodyInspectionController.Get().IsActive())
		{
			return;
		}
		if (InventoryBackpack.Get().m_EquippedItemSlot == slot)
		{
			return;
		}
		if (WalkieTalkieController.Get().IsActive() && slot.m_Item != null && slot.m_Item.m_Info != null && slot.m_Item.m_Info.IsBow())
		{
			return;
		}
		if (TriggerController.Get().IsGrabInProgress())
		{
			return;
		}
		if (WatchController.Get().IsActive())
		{
			return;
		}
		if (MapController.Get().IsActive())
		{
			return;
		}
		if (NotepadController.Get().IsActive())
		{
			return;
		}
		if (SleepController.Get().IsActive())
		{
			return;
		}
		if (SwimController.Get().IsActive())
		{
			return;
		}
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (MakeFireController.Get().IsActive())
		{
			return;
		}
		if (ConstructionController.Get().IsActive())
		{
			return;
		}
		if (FishingController.Get().IsActive() && FishingController.Get().m_Fish)
		{
			return;
		}
		if (BowController.Get().IsActive() && BowController.Get().IsShot())
		{
			return;
		}
		if (ScenarioManager.Get().IsDream())
		{
			return;
		}
		if (VomitingController.Get().IsActive())
		{
			return;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return;
		}
		if (this.m_SlotToEquip == slot)
		{
			return;
		}
		if (this.m_Aim)
		{
			return;
		}
		if (ItemController.Get().IsActive() && ItemController.Get().m_State == ItemController.State.Throw)
		{
			return;
		}
		if (WeaponSpearController.Get().IsActive() && WeaponSpearController.Get().IsThrow())
		{
			return;
		}
		if (this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater))
		{
			return;
		}
		if (CraftingController.Get().IsActive())
		{
			return;
		}
		Item currentItem = this.GetCurrentItem();
		if (currentItem && currentItem.GetInfoID() == ItemID.Fire)
		{
			return;
		}
		Item item = this.m_CurrentItem[0];
		if (item == null || !item.m_RequestThrow)
		{
			Item item2 = this.m_CurrentItem[1];
			if (item2 == null || !item2.m_RequestThrow)
			{
				Item item3 = this.m_CurrentItem[0];
				if (item3 == null || !item3.m_Thrown)
				{
					Item item4 = this.m_CurrentItem[1];
					if (item4 == null || !item4.m_Thrown)
					{
						Item currentItem2 = this.GetCurrentItem();
						if (currentItem2 && currentItem2.m_Info.IsFishingRod())
						{
							this.HideWeapon();
						}
						if (this.m_ActiveFightController && this.m_ActiveFightController.IsBlock())
						{
							this.m_ActiveFightController.SetBlock(false);
						}
						Item item5 = slot.m_Item;
						this.m_SlotToEquip = slot;
						if (!Inventory3DManager.Get().gameObject.activeSelf)
						{
							this.SetWantedItem((item5 && item5.m_Info.IsBow()) ? Hand.Left : Hand.Right, item5, false);
							this.SetWantedItem((item5 && item5.m_Info.IsBow()) ? Hand.Right : Hand.Left, null, false);
						}
						else if (item5 != null && item5.m_Info.m_ID == ItemID.Fishing_Rod)
						{
							this.SetWantedItem(Hand.Right, item5, true);
							this.SetWantedItem(Hand.Left, null, false);
						}
						else
						{
							this.UpdateHands();
						}
						this.m_LastEquipTime = Time.time;
						if (this.m_Aim)
						{
							this.StopAim();
						}
						return;
					}
				}
			}
		}
	}

	public void AttachObjectToPlayersBone(GameObject obj, string bone_name)
	{
		Transform transform = base.transform.FindDeepChild(bone_name);
		DebugUtils.Assert(transform != null, true);
		if (transform == null)
		{
			return;
		}
		obj.transform.position = transform.position;
		obj.transform.rotation = transform.rotation;
		obj.transform.parent = transform;
		this.m_AttachedObjects.Add(obj);
	}

	public void DetachObjectFromPlayer(GameObject obj)
	{
		if (obj != null)
		{
			obj.transform.parent = null;
			this.m_AttachedObjects.Remove(obj);
		}
	}

	public void EnableCollisions()
	{
		this.m_CharacterController.detectCollisions = true;
		this.m_UseGravity = true;
	}

	public void DisableCollisions()
	{
		this.m_CharacterController.detectCollisions = false;
		this.m_UseGravity = false;
	}

	public void ItemsFromHandsPutToInventory()
	{
		Item currentItem = this.GetCurrentItem(Hand.Left);
		if (currentItem != null)
		{
			this.SetWantedItem(Hand.Left, null, true);
			InventoryBackpack.Get().InsertItem(currentItem, null, null, true, true, true, true, true);
		}
		currentItem = this.GetCurrentItem(Hand.Right);
		if (currentItem != null)
		{
			this.SetWantedItem(Hand.Right, null, true);
			InventoryBackpack.Get().InsertItem(currentItem, null, null, true, true, true, true, true);
		}
		InventoryBackpack.Get().m_EquippedItem = null;
	}

	public void ScenarioResetBodyRotationParams()
	{
		foreach (PlayerController playerController in this.m_PlayerControllers)
		{
			if (playerController.IsActive())
			{
				playerController.ResetBodyRotationBonesParams();
			}
		}
		LookController.Get().m_WantedLookDev.y = 0f;
	}

	public void ReplaceMaterial(string object_name, string replacer)
	{
		Material material = Resources.Load("Materials/Player/" + replacer) as Material;
		if (!material)
		{
			return;
		}
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			if (componentsDeepChild[i].gameObject.name == object_name)
			{
				componentsDeepChild[i].material = material;
			}
		}
	}

	private void PlayAnim(int hash, float transition_duration, int layer, float start_time = 0f)
	{
		this.m_Animator.CrossFadeInFixedTime(hash, transition_duration, layer, start_time);
	}

	private void UpdateTraveledDistStat()
	{
		if (this.m_TraveledDistStatLastPos != Vector3.zero)
		{
			float magnitude = (base.transform.position - this.m_TraveledDistStatLastPos).magnitude;
			if (magnitude > 0.01f && magnitude < 1f)
			{
				this.m_TraveledDistStatCurrShift += magnitude;
				if (this.m_TraveledDistStatCurrShift >= 1f)
				{
					EventsManager.OnEvent(Enums.Event.TraveledDist, 0.001f);
					this.m_TraveledDistStatCurrShift -= 1f;
				}
			}
		}
		this.m_TraveledDistStatLastPos = base.transform.position;
	}

	public float GetTeleportTime()
	{
		return this.m_TeleportTime;
	}

	private bool ShouldHideWeapon()
	{
		if (BodyInspectionController.Get().IsActive())
		{
			return true;
		}
		if (Inventory3DManager.Get().IsActive() && !CraftingManager.Get().IsActive())
		{
			Item item = this.GetCurrentItem();
			if (!item)
			{
				item = InventoryBackpack.Get().m_EquippedItem;
			}
			return !item || !item.m_Info.IsFishingRod() || InventoryBackpack.Get().m_ActivePocket == BackpackPocket.Left;
		}
		if (this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater))
		{
			return true;
		}
		if (this.m_Animator.GetBool(ItemController.Get().m_FireHash))
		{
			return true;
		}
		if (this.m_Animator.GetBool(this.m_CleanUpHash))
		{
			return true;
		}
		UnityEngine.Object currentItem = this.GetCurrentItem(Hand.Left);
		Item item2 = (InventoryBackpack.Get().m_EquippedItemSlot != null) ? InventoryBackpack.Get().m_EquippedItemSlot.m_Item : null;
		if ((currentItem != null && this.m_ShouldStartWalkieTalkieController) || (WalkieTalkieController.Get().IsActive() && item2 != null && item2.m_Info.IsBow()))
		{
			return true;
		}
		if (NotepadController.Get().IsActive())
		{
			return true;
		}
		if (MapController.Get().IsActive())
		{
			return true;
		}
		if (LadderController.Get().IsActive())
		{
			return true;
		}
		if (LiquidInHandsController.Get().IsActive())
		{
			return true;
		}
		if (SwimController.Get().IsActive())
		{
			return true;
		}
		if (HeavyObjectController.Get().IsActive())
		{
			return true;
		}
		if (BoatController.Get().IsActive())
		{
			return true;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return true;
		}
		if (GrapplingHookController.Get().IsActive())
		{
			return true;
		}
		if (HarvestingAnimalController.Get().IsActive())
		{
			return true;
		}
		if (HarvestingSmallAnimalController.Get().IsActive())
		{
			return true;
		}
		if (CraftingController.Get().IsActive())
		{
			return true;
		}
		if (MakeFireController.Get().IsActive())
		{
			return true;
		}
		if (DeathController.Get().IsActive())
		{
			return true;
		}
		if (ItemController.Get().IsActive() && ItemController.Get().m_StoneThrowing)
		{
			return true;
		}
		if (CutscenesManager.Get().IsCutscenePlaying())
		{
			return true;
		}
		if (ConstructionController.Get().IsActive())
		{
			return true;
		}
		if (VomitingController.Get().IsActive())
		{
			return true;
		}
		if (WatchController.Get().IsActive() && !FishingController.Get().IsActive())
		{
			return true;
		}
		if (SleepController.Get().IsActive())
		{
			return true;
		}
		if (MudMixerController.Get().IsActive())
		{
			return true;
		}
		if (this.m_WeaponHiddenFromScenario)
		{
			return true;
		}
		Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash);
		return false;
	}

	private void UpdateWeapon()
	{
		if (this.m_ControllerToStart != PlayerControllerType.Unknown)
		{
			return;
		}
		if (this.IsControllerStartBlocked())
		{
			return;
		}
		if (this.ShouldHideWeapon())
		{
			this.HideWeapon();
			return;
		}
		this.ShowWeapon();
	}

	public void HideWeaponFromScenario()
	{
		this.m_WeaponHiddenFromScenario = true;
		this.HideWeapon();
	}

	public void ShowWeaponFromScenario()
	{
		this.m_WeaponHiddenFromScenario = false;
		this.ShowWeapon();
	}

	public void HideWeapon()
	{
		Item currentItem = this.GetCurrentItem();
		if (currentItem && (currentItem.m_Info.IsWeapon() || currentItem.m_Info.IsFishingRod()))
		{
			if (currentItem.m_Info.IsFishingRod())
			{
				this.StopController(PlayerControllerType.Fishing);
			}
			this.SetWantedItem(currentItem.m_Info.IsBow() ? Hand.Left : Hand.Right, null, true);
			currentItem.m_ShownInInventory = true;
			InventoryBackpack.Get().InsertItem(currentItem, InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, false);
		}
	}

	public void ShowWeapon()
	{
		if (InventoryBackpack.Get().m_EquippedItemSlot && InventoryBackpack.Get().m_EquippedItemSlot.m_Item && this.m_ControllerToStart == PlayerControllerType.Unknown)
		{
			Item item = InventoryBackpack.Get().m_EquippedItemSlot.m_Item;
			InventoryBackpack.Get().RemoveItem(item, false);
			this.SetWantedItem(item, true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InputsManager.Get().UnregisterReceiver(this);
		this.UnregisterForPeer(ReplTools.GetLocalPeer());
	}

	public bool HasBlade()
	{
		if (InventoryBackpack.Get().ContainsBlade())
		{
			return true;
		}
		Item currentItem = this.GetCurrentItem(Hand.Right);
		return currentItem && currentItem.m_Info.IsKnife();
	}

	public static float GetSleepTimeFactor()
	{
		if (HUDSleeping.Get().GetState() != HUDSleepingState.Progress)
		{
			return Time.deltaTime;
		}
		TOD_Sky todsky = MainLevel.Instance.m_TODSky;
		TOD_Time todtime = MainLevel.Instance.m_TODTime;
		float num;
		if (todsky.Cycle.Hour > 6f && todsky.Cycle.Hour <= 18f)
		{
			num = todtime.m_DayLengthInMinutes / 12f * 60f;
		}
		else
		{
			num = todtime.m_NightLengthInMinutes / 12f * 60f;
		}
		return num * SleepController.Get().m_HoursDelta;
	}

	public static float GetUnconsciousTimeFactor()
	{
		TOD_Sky todsky = MainLevel.Instance.m_TODSky;
		TOD_Time todtime = MainLevel.Instance.m_TODTime;
		float num;
		if (todsky.Cycle.Hour > 6f && todsky.Cycle.Hour <= 18f)
		{
			num = todtime.m_DayLengthInMinutes / 12f * 60f;
		}
		else
		{
			num = todtime.m_NightLengthInMinutes / 12f * 60f;
		}
		return num * ConsciousnessController.Get().m_HoursDelta;
	}

	public void UpdateCharacterControllerSizeAndCenter()
	{
		Vector3 center = this.m_CharacterController.center;
		float num = this.m_CharacterController.height;
		this.m_CharacterControllerLastOffset = center;
		float num2 = -40f;
		if (this.m_SwimController.IsActive())
		{
			center.y = 1.35f;
			num = 0.9f;
		}
		else if (this.m_FPPController.IsDuck())
		{
			center.y = 0.65f;
			num = 1.3f;
		}
		else
		{
			center.y = 0.9f;
			num = 1.8f;
		}
		if (this.m_FPPController.IsActive() && !FreeHandsLadderController.Get().IsActive())
		{
			if (this.m_LookController.m_LookDev.y <= num2)
			{
				center.z = 0.35f;
			}
			else
			{
				center.z = CJTools.Math.GetProportionalClamp(0.35f, -0.35f, this.m_LookController.m_LookDev.y, num2, 80f);
			}
		}
		float height = this.m_CharacterController.height + (num - this.m_CharacterController.height) * Time.deltaTime * 2f;
		float y = this.m_CharacterController.center.y + (center.y - this.m_CharacterController.center.y) * Time.deltaTime * 2f;
		center.y = y;
		this.m_CharacterController.center = center;
		this.m_CharacterController.height = height;
		this.m_CharacterControllerDelta = center - this.m_CharacterControllerLastOffset;
		this.m_CharacterControllerDelta.y = 0f;
	}

	private void UpdateVelocity()
	{
		if (this.m_LastPos != Vector3.zero && Time.deltaTime > 0f)
		{
			this.m_Velocity = (base.transform.position - this.m_LastPos) / Time.deltaTime;
		}
		this.m_LastPos = base.transform.position;
	}

	public bool IsCameraUnderwater()
	{
		return this.IsInSwimWater() && CameraManager.Get().m_MainCamera.transform.position.y < this.GetWaterLevel();
	}

	public void SetDreamPPActive(bool active)
	{
		this.m_DreamPPActive = active;
	}

	public void SetSpeedMul(float mul)
	{
		this.m_SpeedMul = mul;
		this.m_Animator.speed = mul;
	}

	private void UpdateShaderParameters()
	{
		if (!this.m_HandsRenderer)
		{
			Transform transform = base.transform.FindDeepChild("Hands");
			this.m_HandsRenderer = transform.gameObject.GetComponent<Renderer>();
		}
		if (!this.m_LegsRenderer)
		{
			Transform transform2 = base.transform.FindDeepChild("Legs");
			this.m_LegsRenderer = transform2.gameObject.GetComponent<Renderer>();
		}
		float maxDirtiness = PlayerConditionModule.Get().m_MaxDirtiness;
		float num = Mathf.Clamp(PlayerConditionModule.Get().m_Dirtiness, 0f, maxDirtiness);
		float value = 1f - num / maxDirtiness;
		if (this.m_HandsRenderer)
		{
			this.m_HandsRenderer.material.SetFloat(this.m_ShaderAlbedoMaskPower, value);
		}
		if (this.m_LegsRenderer)
		{
			this.m_LegsRenderer.material.SetFloat(this.m_ShaderAlbedoMaskPower, value);
		}
		if (SwimController.Get().IsActive() || this.IsTakingShower() || (RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position)) || (this.m_Animator.GetBool(Player.Get().m_CleanUpHash) && this.m_RGHPowerValue < 1f))
		{
			this.m_RGHPowerValue += Time.deltaTime;
		}
		else if (this.m_RGHPowerValue > 0f)
		{
			this.m_RGHPowerValue -= Time.deltaTime * 0.1f;
		}
		this.m_RGHPowerValue = Mathf.Clamp01(this.m_RGHPowerValue);
		if (this.m_HandsRenderer)
		{
			this.m_HandsRenderer.material.SetFloat(this.m_ShaderRGHPower, this.m_RGHPowerValue);
		}
		if (this.m_LegsRenderer)
		{
			this.m_LegsRenderer.material.SetFloat(this.m_ShaderRGHPower, this.m_RGHPowerValue);
		}
	}

	public bool IsTakingShower()
	{
		for (int i = 0; i < Shower.s_Showers.Count; i++)
		{
			Shower shower = Shower.s_Showers[i];
			if (shower.m_IsPlayerInside && shower.m_Active)
			{
				return true;
			}
		}
		return false;
	}

	public void Reposition(Vector3 position, Vector3? forward = null)
	{
		base.transform.position = position;
		this.m_CharacterController.transform.position = position;
		if (forward != null)
		{
			base.transform.forward = forward.Value;
		}
		this.m_LastPosOnGround = position;
	}

	public Vector3 GetWorldPosition()
	{
		return base.transform.position;
	}

	public bool IsReplicated()
	{
		return false;
	}

	public void EnableFootstepSounds(bool enable)
	{
		this.m_FootestepsSoundsEnabled = enable;
	}

	public bool IsSpeakingInDialog()
	{
		DialogsManager dialogsManager = DialogsManager.Get();
		return dialogsManager != null && dialogsManager.IsPlayerSpeaking();
	}

	public bool HasKilledTribe()
	{
		return this.m_HasKilledTribeInLastFrame;
	}

	public bool CanStartWakieTalkieController()
	{
		return true;
	}

	public void DirtAddOnHarvest(float dirt)
	{
		this.m_ConditionModule.AddDirtiness(dirt);
	}

	private void ResetAnimatorParameters()
	{
		for (int i = 0; i < this.m_Animator.parameterCount; i++)
		{
			AnimatorControllerParameterType type = this.m_Animator.parameters[i].type;
			switch (type)
			{
			case AnimatorControllerParameterType.Float:
				this.m_Animator.SetFloat(this.m_Animator.parameters[i].nameHash, 0f);
				break;
			case (AnimatorControllerParameterType)2:
				break;
			case AnimatorControllerParameterType.Int:
				this.m_Animator.SetInteger(this.m_Animator.parameters[i].nameHash, 0);
				break;
			case AnimatorControllerParameterType.Bool:
				this.m_Animator.SetBool(this.m_Animator.parameters[i].nameHash, false);
				break;
			default:
				if (type == AnimatorControllerParameterType.Trigger)
				{
					this.m_Animator.ResetTrigger(this.m_Animator.parameters[i].nameHash);
				}
				break;
			}
		}
	}

	public void InfiniteDiving(bool infinity_diving)
	{
		this.m_InfinityDiving = infinity_diving;
	}

	private void OnItemChanged(Item item, Hand hand)
	{
		foreach (PlayerController playerController in this.m_PlayerControllers)
		{
			if (playerController.IsActive())
			{
				playerController.OnItemChanged(item, hand);
			}
		}
		InventoryBackpack.Get().CalculateCurrentWeight();
	}

	public void OnStartWashinghands()
	{
		PlayerAudioModule.Get().PlayWashingHandsSound();
	}

	private void UpdateFreeHandsLadder()
	{
		int i = 0;
		while (i < this.m_FreeHandsLadders.Count)
		{
			if (this.m_FreeHandsLadders[i] == null)
			{
				this.m_FreeHandsLadders.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
		if (this.m_FreeHandsLadders.Count > 0)
		{
			if (!FreeHandsLadderController.Get().IsActive())
			{
				FreeHandsLadderController.Get().SetLadder(this.m_FreeHandsLadders[0].transform.parent.GetComponent<FreeHandsLadder>());
				this.StartController(PlayerControllerType.FreeHandsLadder);
				return;
			}
		}
		else if (FreeHandsLadderController.Get().IsActive())
		{
			this.StopController(PlayerControllerType.FreeHandsLadder);
		}
	}

	private static Player s_Instance = null;

	private PlayerParams m_Params;

	[HideInInspector]
	public CharacterControllerProxy m_CharacterController;

	[HideInInspector]
	public PlayerAudioModule m_AudioModule;

	private int m_BlockMovesRequestsCount;

	private int m_BlockRotationRequestsCount;

	private Item[] m_WantedItem = new Item[2];

	private Item[] m_CurrentItem = new Item[2];

	private ItemSlot m_SlotToEquip;

	private bool m_UpdateHands;

	private Transform m_ClimbingBlocks;

	private Transform m_LHand;

	private Transform m_RHand;

	private Transform m_Spine1;

	[HideInInspector]
	public BodyInspectionController m_BodyInspectionController;

	[HideInInspector]
	public MakeFireController m_MakeFireController;

	private ConsciousnessController m_ConsciousnessController;

	private DiarrheaController m_DiarrheaController;

	[HideInInspector]
	public SwimController m_SwimController;

	[HideInInspector]
	public LookController m_LookController;

	[HideInInspector]
	public FPPController m_FPPController;

	[HideInInspector]
	public DeathController m_DeathController;

	[HideInInspector]
	public WeaponController m_WeaponController;

	[HideInInspector]
	public HitReactionController m_HitReactionController;

	[HideInInspector]
	public SleepController m_SleepController;

	private WatchController m_WatchController;

	private NotepadController m_NotepadController;

	private InsectsController m_InsectsController;

	private ConstructionController m_ConstructionController;

	private MapController m_MapController;

	[HideInInspector]
	public TorchController m_TorchController;

	[HideInInspector]
	public FightController m_ActiveFightController;

	[HideInInspector]
	public bool m_UseGravity = true;

	[HideInInspector]
	public bool m_IsInAir;

	[HideInInspector]
	public Vector3 m_LastPosOnGround = Vector3.zero;

	[HideInInspector]
	public float m_LastTimeOnGround;

	private float m_MinFallingHeight = 0.2f;

	private float m_MinFallingDuration = 0.5f;

	private float m_MinFallingHeightToDealDamage = 3f;

	private bool m_InWater;

	private bool m_InSwimWater;

	private float m_WaterLevel = float.MinValue;

	public static float DEEP_WATER = 1.8f;

	private bool m_Climbing;

	private PlayerController m_LastActiveBodyRotationController;

	private float m_LastTimeBodyRotationControllerChange;

	[HideInInspector]
	public string m_DreamToActivate = string.Empty;

	[HideInInspector]
	public bool m_DreamActive;

	[HideInInspector]
	public bool m_DreamPPActive;

	private Vector3 m_LastPosBeforeDream = Vector3.zero;

	private Quaternion m_LastRotBeforeDream = Quaternion.identity;

	public float m_DreamDuration;

	private Vector3 m_StoredPos = Vector3.zero;

	private Quaternion m_StoredRotation = Quaternion.identity;

	private Vector2 m_StoredLookDev = Vector2.zero;

	[HideInInspector]
	public Collider m_Collider;

	[HideInInspector]
	public float m_LastLoadTime = float.MinValue;

	private int[,] m_PlayerControllerArray = new int[,]
	{
		{
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			1,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			2,
			0,
			0,
			0,
			2,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			2,
			2,
			0,
			2,
			0,
			0,
			2,
			1,
			0,
			1,
			2,
			2,
			1,
			2,
			0,
			2,
			0,
			2,
			2,
			2,
			2,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			2,
			1,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			2,
			0,
			0,
			0,
			2,
			2,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			2,
			2,
			2,
			0,
			2,
			0,
			0,
			2,
			2,
			0,
			0,
			2,
			2,
			1,
			2,
			0,
			2,
			0,
			2,
			2,
			0,
			2,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			1,
			0,
			0,
			0,
			0,
			0,
			2,
			2,
			0,
			0,
			0,
			0,
			0,
			1,
			2,
			2,
			2,
			2,
			2,
			2,
			0,
			0,
			2,
			2,
			2,
			0,
			2,
			2,
			1,
			2,
			0,
			2,
			0,
			0,
			2,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			2,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			0,
			1,
			1,
			0,
			0,
			1,
			2,
			0,
			0,
			1,
			1,
			1,
			0,
			0,
			1,
			0,
			0,
			1,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			2,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			1,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			1,
			1,
			0,
			0,
			0,
			1,
			1,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			2,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			2,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,
			1,
			0,
			1,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			0,
			0,
			2,
			0,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			0,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2,
			1
		},
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			2
		}
	};

	private PlayerController[] m_PlayerControllers = new PlayerController[44];

	[HideInInspector]
	public List<ItemID> m_KnownItems = new List<ItemID>();

	private Item[] m_StoredItems = new Item[2];

	[HideInInspector]
	public bool m_ShowDivingMask;

	[HideInInspector]
	public bool m_ShowBiohazardMask;

	private FogMode m_FogModeToRestore = FogMode.ExponentialSquared;

	private float m_FogStartDistanceToRestore;

	private float m_FogEndDistanceToRestore;

	private bool m_ResetDreamParams;

	private int m_ResetDreamParamsFrame;

	private float m_ShakePower;

	private float m_WantedShakePower;

	private float m_ShakeSpeed;

	private float m_SetShakePowerDuration;

	[HideInInspector]
	public float m_AdditionalShakePower;

	public static string s_AdditiveAnimationsScriptPath = "Player/AdditiveAnims.txt";

	private Dictionary<string, Dictionary<Transform, List<SimpleTransform>>> m_Animations = new Dictionary<string, Dictionary<Transform, List<SimpleTransform>>>();

	[HideInInspector]
	public bool m_MapUnlocked;

	[HideInInspector]
	public bool m_WatchUnlocked;

	private GameObject m_Watch;

	[HideInInspector]
	public bool m_NotepadUnlocked;

	[HideInInspector]
	public bool m_SleepBlocked;

	[HideInInspector]
	public bool m_InspectionBlocked;

	private bool m_WheelHUDBlocked;

	[HideInInspector]
	public bool m_BackpackWasOpen;

	public static float s_AdditiveAnimationSampleInterval = 0.03333f;

	public static float s_AdditiveAnimationPostBlendTime = 0.23f;

	private float m_LastAdditiveAnimationTime;

	private Dictionary<Transform, SimpleTransform> m_LastAdditiveAnimationFrame = new Dictionary<Transform, SimpleTransform>();

	private Dictionary<Transform, SimpleTransform> m_AdditiveAnimationStartFrame = new Dictionary<Transform, SimpleTransform>();

	public bool m_PostBlendAdditiveAnimation;

	private bool m_MapActivityChanged;

	private bool m_NotepadActivityChanged;

	[HideInInspector]
	public SensorLift m_CurrentLift;

	[HideInInspector]
	public bool m_InfinityDiving;

	private Player.AimType m_AimType;

	[HideInInspector]
	public bool m_Aim;

	[HideInInspector]
	public float m_AimPower;

	private float m_StartAimTime;

	[HideInInspector]
	public float m_StopAimTime;

	[HideInInspector]
	public Matrix4x4 m_StopAimCameraMtx;

	[HideInInspector]
	public Animator m_Animator;

	[HideInInspector]
	public int m_CleanUpHash = Animator.StringToHash("CleanUp");

	private float m_StartTime;

	private float m_InitDuration = 2f;

	private Vector3 m_TraveledDistStatLastPos = Vector3.zero;

	private float m_TraveledDistStatCurrShift;

	private float m_LastEquipTime;

	[HideInInspector]
	public bool m_OpenBackpackSheduled;

	private GameObject m_ScenarioPositionObject;

	[HideInInspector]
	public bool m_HasKilledTribeInLastFrame;

	private int m_HasKilledTribeFrameCounter;

	private List<GameObject> m_AttachedObjects = new List<GameObject>();

	private float m_CharacterControllerHeight = 1.8f;

	private int m_ChangingControllerInAnimator = Animator.StringToHash("ChangingController");

	private int m_PlayUnequipInAnimator = Animator.StringToHash("PlayUnequip");

	private int m_NotepadIdleHash = Animator.StringToHash("NotepadIdle");

	private int m_MapIdleHash = Animator.StringToHash("MapIdle");

	public Vector3 m_RespawnPosition;

	private int m_FishingRodInHand = Animator.StringToHash("FishingRodInHand");

	private RaycastHit[] m_RaycastHitsTmp = new RaycastHit[20];

	private PlayerConditionModule m_ConditionModule;

	private PlayerDiseasesModule m_DiseasesModule;

	private PlayerInjuryModule m_InjuryModule;

	private Ray m_Ray;

	private RaycastHit[] m_Hits = new RaycastHit[10];

	private bool m_BonesRotated;

	private List<Transform> m_AffectedBones = new List<Transform>(100);

	private List<Transform> m_BonesToRemove = new List<Transform>(100);

	private List<Transform> m_BonesToRotate = new List<Transform>(100);

	private List<float> m_BonesToRotateValue = new List<float>(100);

	[HideInInspector]
	public PlayerControllerType m_ControllerToStart = PlayerControllerType.Unknown;

	[HideInInspector]
	private bool m_ShowMapSheduled;

	[HideInInspector]
	public bool m_ShowNotepadSheduled;

	private bool m_ScenarioWatchBlocked;

	private float m_LastAddInjuryTime;

	private bool m_AdditiveAnimationActiveLastFrame;

	private List<AnimatorClipInfo> m_AnimatorClipInfo = new List<AnimatorClipInfo>(300);

	private GameObject m_ChangeSceneSpawner;

	[HideInInspector]
	public bool m_MovesBlockedOnChangeScene;

	private float m_TeleportTime = float.MinValue;

	private List<FreeHandsLadderTrigger> m_FreeHandsLadders = new List<FreeHandsLadderTrigger>();

	[HideInInspector]
	public bool m_WeaponHiddenFromScenario;

	private Vector3 m_CharacterControllerLastOffset = Vector3.zero;

	public Vector3 m_CharacterControllerDelta = Vector3.zero;

	public const float m_CharacterColliderMaxOffsetZ = 0.35f;

	public Vector3 m_Velocity = Vector3.zero;

	private Vector3 m_LastPos = Vector3.zero;

	[HideInInspector]
	public float m_SpeedMul = 1f;

	private Renderer m_HandsRenderer;

	private Renderer m_LegsRenderer;

	private int m_ShaderAlbedoMaskPower = Shader.PropertyToID("_AlbedoMaskPower");

	private int m_ShaderRGHPower = Shader.PropertyToID("_RGHPower");

	private float m_RGHPowerValue;

	[HideInInspector]
	public bool m_FootestepsSoundsEnabled = true;

	public bool m_ShouldStartWalkieTalkieController;

	public enum AimType
	{
		None,
		Blowpipe,
		Bow,
		Fishing,
		Item,
		SpearHunting,
		SpearFishing
	}
}
