using System;
using System.Collections.Generic;
using System.Linq;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class Player : Being, ISaveLoad, IInputsReceiver
{
	public static Player Get()
	{
		return Player.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_LEye = base.gameObject.transform.FindDeepChild("mixamorig:Eye.L");
		this.m_REye = base.gameObject.transform.FindDeepChild("mixamorig:Eye.R");
		this.m_Head = base.gameObject.transform.FindDeepChild("mixamorig:Head");
		Player.s_Instance = this;
		this.m_CharacterController = base.GetComponent<CharacterController>();
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
		this.m_Watch = base.gameObject.transform.FindDeepChild("watch").gameObject;
		this.m_Animator = base.GetComponent<Animator>();
		this.ParseAdditiveAnimations();
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
		this.m_LFoot = base.gameObject.transform.FindDeepChild("mixamorig:Foot.L");
		DebugUtils.Assert(this.m_LFoot != null, "Missing Player mixamorig:Foot.L", true, DebugUtils.AssertType.Info);
		this.m_RFoot = base.gameObject.transform.FindDeepChild("mixamorig:Foot.R");
		DebugUtils.Assert(this.m_RFoot != null, "Missing Player mixamorig:Foot.R", true, DebugUtils.AssertType.Info);
		InputsManager.Get().RegisterReceiver(this);
		this.BlockMoves();
		this.BlockRotation();
		base.Invoke("UnblockMoves", this.m_InitDuration);
		base.Invoke("UnblockRotation", this.m_InitDuration);
		this.m_StartTime = Time.time;
		this.m_Collider = base.gameObject.GetComponent<Collider>();
	}

	private void InitializeParams()
	{
		this.m_Params = new PlayerParams();
		this.m_Params.ParseScript();
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
	}

	public void Load()
	{
		base.transform.position = SaveGame.LoadV3Val("PlayerPos");
		this.m_LastPosOnGround = base.gameObject.transform.position;
		this.m_LookController.m_LookDev = SaveGame.LoadV2Val("PlayerLookDev");
		this.m_LookController.m_LookDev.y = 0f;
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
		base.Update();
		for (int i = 0; i < 41; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].ControllerUpdate();
			}
		}
		if (this.m_ControllerToStart != PlayerControllerType.Unknown)
		{
			this.StartControllerInternal();
		}
		if (this.m_OpenBackpackSheduled)
		{
			Inventory3DManager.Get().Activate();
			this.m_OpenBackpackSheduled = false;
		}
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
		if (this.m_PlannerScheduled != PlannerMode.None)
		{
			HUDPlanner.Get().m_PlannerMode = this.m_PlannerScheduled;
			MenuNotepad.Get().m_ActiveTab = MenuNotepad.MenuNotepadTab.PlannerTab;
			this.StartController(PlayerControllerType.Notepad);
			this.m_PlannerScheduled = PlannerMode.None;
		}
		this.DebugUpdate();
		this.m_MapActivityChanged = false;
		this.m_NotepadActivityChanged = false;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		this.UpdateShake();
		this.UpdateBonesRotation();
		for (int i = 0; i < 41; i++)
		{
			if (this.m_PlayerControllers[i].IsActive())
			{
				this.m_PlayerControllers[i].ControllerLateUpdate();
			}
		}
		this.UpdateHands();
		this.UpdateAdditiveAnimations();
		this.UpdateInLift();
		this.UpdateAim();
		InsectsManager.Get().UpdateInsects();
		BodyInspectionController.Get().UpdateMaggots();
		BodyInspectionController.Get().UpdateAnts();
	}

	public PlayerParams GetParams()
	{
		return this.m_Params;
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
		return this.m_BlockMovesRequestsCount > 0 || LoadingScreen.Get().m_Active || HUDStartSurvivalSplash.Get().m_Active;
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
		if (item.m_Info.m_DestroyOnDrop)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		else if (item.m_Info.IsHeavyObject())
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

	public void SetWantedItem(Item item, bool immediate = true)
	{
		if (item == null)
		{
			DebugUtils.Assert("Use SetWantedItem method with Hand parameter!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.SetWantedItem((!item.m_Info.IsBow()) ? Hand.Right : Hand.Left, item, immediate);
	}

	public void SetWantedItem(Hand hand, Item item, bool immediate = true)
	{
		this.m_WantedItem[(int)hand] = item;
		InventoryBackpack.Get().RemoveItem(item, false);
		if (immediate)
		{
			this.UpdateHands();
		}
	}

	private void UpdateHands()
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.m_WantedItem[i] != null && (this.m_CurrentItem[i] == null || this.m_CurrentItem[i] != this.m_WantedItem[i]))
			{
				if (this.m_CurrentItem[i])
				{
					this.DetachItemFromHand(this.m_CurrentItem[i]);
				}
				this.AttachItemToHand((Hand)i, this.m_WantedItem[i]);
				this.m_CurrentItem[i] = this.m_WantedItem[i];
				this.SetupActiveController();
			}
			else if (this.m_WantedItem[i] == null && this.m_CurrentItem[i])
			{
				this.DetachItemFromHand(this.m_CurrentItem[i]);
				this.m_CurrentItem[i] = null;
				this.SetupActiveController();
			}
		}
	}

	public void AttachItemToHand(Hand hand, Item item)
	{
		Transform transform = (hand != Hand.Left) ? this.m_RHand : this.m_LHand;
		Quaternion rhs = Quaternion.Inverse(item.m_Holder.localRotation);
		item.gameObject.transform.rotation = transform.rotation;
		item.gameObject.transform.rotation *= rhs;
		Vector3 b = item.m_Holder.parent.position - item.m_Holder.position;
		item.gameObject.transform.position = transform.position;
		item.gameObject.transform.position += b;
		item.gameObject.transform.parent = transform.transform;
		item.OnItemAttachedToHand();
	}

	private void DetachItemFromHand(Item item)
	{
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
	}

	public bool HasItemEquiped(string item_name)
	{
		Item currentItem = this.GetCurrentItem(Hand.Right);
		Item currentItem2 = this.GetCurrentItem(Hand.Left);
		return (currentItem && ItemsManager.Get().ItemIDToString((int)currentItem.GetInfoID()) == item_name) || (currentItem2 && ItemsManager.Get().ItemIDToString((int)currentItem2.GetInfoID()) == item_name);
	}

	public bool HaveItem(string item_name)
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

	public bool IsLookingAtObject(GameObject obj, float dist)
	{
		if (!Camera.main)
		{
			return false;
		}
		Vector3 position = Camera.main.transform.position;
		Vector3 forward = Camera.main.transform.forward;
		foreach (RaycastHit raycastHit in Physics.RaycastAll(position, forward, dist))
		{
			if (raycastHit.collider.gameObject == obj)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateInAir()
	{
		if (Time.time - this.m_StartTime < this.m_InitDuration)
		{
			return;
		}
		if (this.m_CharacterController.isGrounded)
		{
			if (this.m_IsInAir && !base.GetComponent<LadderController>().IsActive())
			{
				this.OnLand(this.m_LastPosOnGround.y - base.gameObject.transform.position.y);
			}
			this.m_LastPosOnGround = base.transform.position;
			this.m_LastTimeOnGround = Time.time;
		}
		this.m_IsInAir = (!this.m_CharacterController.isGrounded && !this.m_SwimController.enabled);
	}

	private void OnLand(float fall_height)
	{
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
		this.m_InWater = WaterBoxManager.Get().IsInWater(base.gameObject.transform.position, ref this.m_WaterLevel);
	}

	public new bool IsInWater()
	{
		return this.m_InWater;
	}

	public float GetWaterLevel()
	{
		return this.m_WaterLevel;
	}

	private void UpdateSwim()
	{
		if (!this.m_SwimController.IsActive() && this.ShouldSwim() && !this.IsDead() && !this.m_DeathController.IsActive() && GreenHellGame.Instance.m_LoadGameState == LoadGameState.None)
		{
			this.StartController(PlayerControllerType.Swim);
		}
		if (this.m_SwimController.IsActive() && !this.ShouldSwim())
		{
			this.StopController(PlayerControllerType.Swim);
		}
	}

	public bool ShouldSwim()
	{
		if (this.m_SwimController.IsActive())
		{
			return !this.m_InWater || this.m_WaterLevel - base.gameObject.transform.position.y > Player.DEEP_WATER * 0.8f;
		}
		return this.m_InWater && this.m_WaterLevel - base.gameObject.transform.position.y > Player.DEEP_WATER * 1.1f;
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
			KeyValuePair<Transform, float> keyValuePair2 = enumerator2.Current;
			Transform key2 = keyValuePair2.Key;
			if (!this.m_AffectedBones.Contains(key2))
			{
				KeyValuePair<Transform, float> keyValuePair3 = enumerator2.Current;
				float num5 = keyValuePair3.Value;
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

	public void SetShake(float power, float speed, float duration)
	{
		this.m_ShakePower = power;
		this.m_StartShakePower = power;
		this.m_ShakeSpeed = speed;
		this.m_ShakeDuration = duration;
		this.m_StartShakeTime = Time.time;
	}

	private void UpdateShake()
	{
		this.m_ShakePower = CJTools.Math.GetProportionalClamp(0f, this.m_StartShakePower, Time.time - this.m_StartShakeTime, this.m_ShakeDuration, 0f);
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
		for (int i = 0; i < 41; i++)
		{
			PlayerControllerType playerControllerType = (PlayerControllerType)i;
			type = playerControllerType.ToString() + "Controller";
			this.m_PlayerControllers[i] = (base.gameObject.GetComponent(type) as PlayerController);
		}
	}

	public void StartController(PlayerControllerType controller)
	{
		if (this.m_HitReactionController.enabled && controller != PlayerControllerType.Death)
		{
			return;
		}
		this.m_ControllerToStart = controller;
	}

	public void ResetControllerToStart()
	{
		this.m_ControllerToStart = PlayerControllerType.Unknown;
	}

	public void StartControllerInternal()
	{
		if (this.m_ControllerToStart == PlayerControllerType.Unknown)
		{
			return;
		}
		Debug.Log(this.m_ControllerToStart.ToString());
		for (int i = 0; i < 41; i++)
		{
			if (i != (int)this.m_ControllerToStart)
			{
				int num = this.m_PlayerControllerArray[i, (int)this.m_ControllerToStart];
				bool enabled = num != 0 && (num != 1 || this.m_PlayerControllers[i].enabled);
				this.m_PlayerControllers[i].enabled = enabled;
			}
		}
		this.m_PlayerControllers[(int)this.m_ControllerToStart].enabled = true;
		this.m_ControllerToStart = PlayerControllerType.Unknown;
	}

	public void StopController(PlayerControllerType controller)
	{
		bool enabled = this.m_PlayerControllers[(int)controller].enabled;
		this.m_PlayerControllers[(int)controller].enabled = false;
		if (enabled && this.m_PlayerControllers[(int)controller].SetupActiveControllerOnStop())
		{
			this.SetupActiveController();
		}
	}

	public void SetupActiveController()
	{
		if (this.m_ControllerToStart != PlayerControllerType.Unknown)
		{
			return;
		}
		Item item = this.m_CurrentItem[0];
		Item item2 = this.m_CurrentItem[1];
		if (item != null)
		{
			if (item.m_Info.IsWeapon())
			{
				if (((Weapon)item).m_Info.IsBow() && item2 == null)
				{
					this.StartController(PlayerControllerType.Bow);
					return;
				}
			}
			else if (item.GetInfoID() == ItemID.Fish_Bone)
			{
				this.StartController(PlayerControllerType.BodyInspectionMiniGame);
				return;
			}
		}
		bool flag = BodyInspectionController.Get().IsActive() || BoatController.Get().IsActive() || NotepadController.Get().IsActive() || MapController.Get().IsActive() || InsectsController.Get().IsActive();
		if (item2 != null)
		{
			if (item2.m_Info.IsWeapon())
			{
				WeaponType weaponType = ((Weapon)item2).GetWeaponType();
				if (weaponType == WeaponType.Melee)
				{
					if (item2.m_Info.m_ID == ItemID.Torch || item2.m_Info.m_ID == ItemID.Weak_Torch || item2.m_Info.m_ID == ItemID.Tobacco_Torch)
					{
						this.StartController(PlayerControllerType.Torch);
						return;
					}
					this.StartController(PlayerControllerType.WeaponMelee);
					return;
				}
				else
				{
					if (weaponType == WeaponType.Spear)
					{
						this.StartController(PlayerControllerType.WeaponSpear);
						return;
					}
					if (weaponType == WeaponType.Bow)
					{
						this.StartController(PlayerControllerType.Bow);
						return;
					}
					if (weaponType == WeaponType.Blowpipe)
					{
						this.StartController(PlayerControllerType.Blowpipe);
						return;
					}
				}
			}
			else
			{
				if (item2.m_Info.m_ID == ItemID.Radio)
				{
					this.StartController(PlayerControllerType.WalkieTalkie);
					return;
				}
				if (item2.m_Info.IsBowl())
				{
					this.StartController(PlayerControllerType.Bowl);
					return;
				}
				if (item2.m_Info.m_ID == ItemID.Water_In_Hands)
				{
					this.StartController(PlayerControllerType.LiquidInHands);
					return;
				}
				if (item2.GetComponent<FishingRod>())
				{
					this.StartController(PlayerControllerType.Fishing);
					return;
				}
				if (item2.GetComponent<FireTool>())
				{
					this.StartController(PlayerControllerType.MakeFire);
					return;
				}
				if (item2.m_Info.IsHeavyObject())
				{
					this.StartController(PlayerControllerType.HeavyObject);
					return;
				}
			}
			if (item2.GetInfoID() == ItemID.Fish_Bone)
			{
				this.StartController(PlayerControllerType.BodyInspectionMiniGame);
				return;
			}
			if (item2.GetInfoID() != ItemID.Boat_Stick)
			{
				if (item2.GetInfoID() == ItemID.Fire)
				{
					this.HideWeapon();
				}
				this.StartController(PlayerControllerType.Item);
				return;
			}
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

	private void UpdatePassOut()
	{
		if (this.ShouldPassOut())
		{
			this.HideWeapon();
			this.StartController(PlayerControllerType.Consciousness);
			this.StartControllerInternal();
		}
	}

	public bool ShouldPassOut()
	{
		return (GreenHellGame.DEBUG && Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.J)) || (!this.m_PlayerControllers[1].IsActive() && !this.IsDead() && this.m_ConditionModule.GetEnergy() <= this.m_ConsciousnessController.GetEnergyToPassOut());
	}

	public override bool IsDead()
	{
		return this.m_ConditionModule.GetHP() <= 0f;
	}

	public void UpdateDeath()
	{
		if (this.IsDead() && !this.m_PlayerControllers[3].IsActive() && !this.m_SleepController.IsActive() && !this.m_ConsciousnessController.IsActive())
		{
			this.SetWantedItem(Hand.Left, null, true);
			this.SetWantedItem(Hand.Right, null, true);
			this.StartController(PlayerControllerType.Death);
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

	private void UpdateLeavesPusher()
	{
		LeavesPusher.Get().Push(base.gameObject.transform.position + Vector3.up * 0.5f, 0.5f);
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

	public void OnLanding(Vector3 speed)
	{
		PlayerInjuryModule component = base.GetComponent<PlayerInjuryModule>();
		component.OnLanding(speed);
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
		return (!WeaponSpearController.Get().IsActive() || !(WeaponSpearController.Get().GetImpaledObject() != null)) && !MakeFireController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && (!CraftingController.Get().IsActive() || !CraftingController.Get().m_InProgress);
	}

	public bool CanSleep()
	{
		return !this.m_SleepBlocked && !MakeFireController.Get().IsActive() && WeaponMeleeController.Get().CanBeInterrupted();
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
		return !this.m_WheelHUDBlocked && (!WeaponSpearController.Get().IsActive() || !(WeaponSpearController.Get().GetImpaledObject() != null)) && !SleepController.Get().IsSleeping() && !CraftingController.Get().m_InProgress && !CutscenesManager.Get().IsCutscenePlaying() && (!BodyInspectionController.Get().IsActive() || BodyInspectionController.Get().CanLeave()) && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f;
	}

	public void StartDream(string dream_name)
	{
		this.m_DreamDuration = -1f;
		this.m_DreamToActivate = dream_name;
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.StartDreamInternal), 1.5f, null);
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
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
	}

	public void StopDream()
	{
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.StopDreamInternal), 1.5f, null);
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
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
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

	public void ShowWalkieTalkie()
	{
		base.GetComponent<WalkieTalkieController>().m_RHandItemToRestore = this.GetCurrentItem(Hand.Right);
		Item item = InventoryBackpack.Get().FindItem(ItemID.Radio);
		if (!item)
		{
			item = ItemsManager.Get().CreateItem(ItemID.Radio, true, Vector3.zero, Quaternion.identity);
		}
		DebugUtils.Assert(item != null, true);
		this.SetWantedItem(Hand.Right, item, true);
	}

	public void HideWalkieTalkie()
	{
		base.GetComponent<WalkieTalkieController>().Stop();
	}

	private void UpdateInputs()
	{
		if (GreenHellGame.DEBUG)
		{
			if (!MainLevel.Instance.IsPause() && Input.GetKeyDown(KeyCode.K) && Input.GetKey(KeyCode.RightControl))
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					ObjectivesManager.Get().ActivateObjective("Obj_FindKate");
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
			else if (!MainLevel.Instance.IsPause() && Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.J))
			{
				if (!base.GetComponent<WalkieTalkieController>().IsActive())
				{
					this.ShowWalkieTalkie();
				}
				else
				{
					this.HideWalkieTalkie();
				}
			}
			else if (Input.GetKeyDown(KeyCode.Quote))
			{
				base.GetComponent<BodyInspectionController>().DebugAttachLeeches();
			}
			else if (Input.GetKeyDown(KeyCode.Comma))
			{
				base.GetComponent<BodyInspectionController>().DebugAttachWorms();
			}
			else if (Input.GetKeyDown(KeyCode.Backslash))
			{
				SortedDictionary<string, string> localizedtexts = GreenHellGame.Instance.GetLocalization().GetLocalizedtexts();
				SortedDictionary<string, string>.Enumerator enumerator = localizedtexts.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Localization localization = GreenHellGame.Instance.GetLocalization();
					KeyValuePair<string, string> keyValuePair = enumerator.Current;
					string text = localization.Get(keyValuePair.Key);
					if (text.Contains("["))
					{
						Debug.Log(text);
					}
				}
			}
		}
		bool flag = this.m_PlayerControllers[28].IsActive();
		bool flag2 = InputsManager.Get().IsActionActive(InputsManager.InputAction.Watch);
		if (flag2 && this.CanShowWatch())
		{
			if (!flag)
			{
				this.StartController(PlayerControllerType.Watch);
			}
		}
		else if (flag)
		{
			this.StopController(PlayerControllerType.Watch);
		}
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public void OnDropHeavyItem()
	{
		if (this.m_ShowNotepadSheduled)
		{
			this.StartController(PlayerControllerType.Notepad);
			this.m_NotepadActivityChanged = true;
			this.m_ShowNotepadSheduled = false;
		}
		else if (this.m_ShowMapSheduled)
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

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (action == InputsManager.InputAction.ShowMap)
		{
			if (!this.m_MapActivityChanged && this.CanShowMap())
			{
				if (NotepadController.Get().IsActive())
				{
					NotepadController.Get().Hide();
					this.m_ShowMapSheduled = true;
					this.m_ShowNotepadSheduled = false;
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
		else if (action == InputsManager.InputAction.ShowNotepad)
		{
			if (!this.m_NotepadActivityChanged && this.CanShowNotepad())
			{
				if (MapController.Get().IsActive())
				{
					this.m_ShowNotepadSheduled = true;
					this.m_ShowMapSheduled = false;
					MapController.Get().Hide();
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
		else if (action == InputsManager.InputAction.QuickEquip0 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(0, BackpackPocket.Left));
		}
		else if (action == InputsManager.InputAction.QuickEquip1 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(1, BackpackPocket.Left));
		}
		else if (action == InputsManager.InputAction.QuickEquip2 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(2, BackpackPocket.Left));
		}
		else if (action == InputsManager.InputAction.QuickEquip3 && this.CanEquipItem())
		{
			this.Equip(InventoryBackpack.Get().GetSlotByIndex(3, BackpackPocket.Left));
		}
		else if (action == InputsManager.InputAction.ThrowStone)
		{
			Item item = InventoryBackpack.Get().FindItem(ItemID.Stone);
			if (item && this.CanThrowStone())
			{
				this.HideWeapon();
				this.StartControllerInternal();
				this.SetWantedItem(item, true);
			}
		}
		if ((action == InputsManager.InputAction.HideMap || action == InputsManager.InputAction.Quit || action == InputsManager.InputAction.AdditionalQuit) && !this.m_MapActivityChanged && MapController.Get().IsActive() && MapController.Get().CanDisable())
		{
			MapController.Get().Hide();
			this.m_MapActivityChanged = true;
		}
		if ((action == InputsManager.InputAction.HideNotepad || action == InputsManager.InputAction.Quit || action == InputsManager.InputAction.AdditionalQuit) && !this.m_NotepadActivityChanged && this.m_NotepadController.IsActive() && this.m_NotepadController.CanDisable())
		{
			this.m_NotepadController.Hide();
			this.m_NotepadActivityChanged = true;
		}
	}

	private bool CanEquipItem()
	{
		return !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive();
	}

	private bool CanThrowStone()
	{
		return !Inventory3DManager.Get().gameObject.activeSelf && !this.m_DreamActive && !SleepController.Get().IsActive() && !this.m_MapController.IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !BodyInspectionController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !ConsciousnessController.Get().IsActive();
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
		for (int i = 0; i < 41; i++)
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
		return !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_InsectsController.IsActive() && !VomitingController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !HitReactionController.Get().IsActive() && !ConsciousnessController.Get().IsActive();
	}

	private bool CanShowWatch()
	{
		return this.m_WatchUnlocked && !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !this.m_NotepadController.IsActive() && !this.m_InsectsController.IsActive() && !VomitingController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CraftingController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !ConsciousnessController.Get().IsActive();
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
		return this.m_NotepadUnlocked && !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !VomitingController.Get().IsActive() && !InsectsController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && WeaponMeleeController.Get().CanBeInterrupted() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !ConsciousnessController.Get().IsActive() && !CraftingController.Get().IsActive();
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
		return this.m_MapUnlocked && !this.m_DreamActive && !this.IsDead() && !SleepController.Get().IsActive() && !MenuInGameManager.Get().IsAnyScreenVisible() && !this.m_SwimController.IsActive() && !VomitingController.Get().IsActive() && !InsectsController.Get().IsActive() && !this.m_Aim && Time.time - this.m_StopAimTime >= 0.5f && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !MakeFireController.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !ConsciousnessController.Get().IsActive() && !CraftingController.Get().IsActive();
	}

	public bool IsWatchControllerActive()
	{
		return this.m_WatchController.IsActive();
	}

	public bool IsMapControllerActive()
	{
		return this.m_MapController.IsActive();
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

	public void SchedulePlanner(PlannerMode mode)
	{
		this.m_PlannerScheduled = mode;
	}

	public void ThrowItem(Hand hand)
	{
		Item currentItem = this.GetCurrentItem(hand);
		if (!currentItem)
		{
			return;
		}
		this.SetWantedItem(hand, null, true);
		this.ThrowItem(currentItem);
	}

	public void ThrowItem(Item item)
	{
		if (!item)
		{
			return;
		}
		item.gameObject.SetActive(true);
		item.transform.parent = null;
		item.UpdatePhx();
		item.m_RequestThrow = true;
		item.m_Thrower = this;
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
		if (this.m_DreamActive)
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
		if (info.m_Damager)
		{
			AI component = info.m_Damager.GetComponent<AI>();
			if (component)
			{
				if (component.IsHuman())
				{
					if (this.IsBlock())
					{
						Vector3 normalized2D = (component.transform.position - base.transform.position).GetNormalized2D();
						if (Vector3.Angle(normalized2D, base.transform.forward.GetNormalized2D()) < 70f)
						{
							info.m_Blocked = true;
						}
					}
					if (!info.m_Blocked)
					{
						PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
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
				}
				PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.AIDamage);
				HUDItem.Get().Deactivate();
			}
		}
		return base.TakeDamage(info);
	}

	private bool CanStartHitReaction()
	{
		return !this.m_WeaponController.IsAttack() && !CraftingController.Get().IsActive() && !CraftingManager.Get().IsActive() && !SleepController.Get().IsActive() && (!WeaponSpearController.Get().IsActive() || (!WeaponSpearController.Get().m_ImpaledObject && !WeaponSpearController.Get().m_ItemBody));
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
			component.AddInjury(InjuryType.Worm, InjuryPlace.RHand, freeWoundSlot, InjuryState.Open, 0, null);
		}
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backslash) && Time.time - this.m_LastAddInjuryTime > 1f)
		{
			PlayerInjuryModule component2 = base.gameObject.GetComponent<PlayerInjuryModule>();
			BIWoundSlot freeWoundSlot2 = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RHand, InjuryType.Leech, true);
			component2.AddInjury(InjuryType.Leech, InjuryPlace.RHand, freeWoundSlot2, InjuryState.Open, 0, null);
		}
		else if (Input.GetKeyDown(KeyCode.Backspace) && Time.time - this.m_LastAddInjuryTime > 1f)
		{
			this.TakeDamage(new DamageInfo
			{
				m_Damage = 30f,
				m_DamageType = DamageType.Claws
			});
			this.m_LastAddInjuryTime = Time.time;
		}
		else if (Input.GetKeyDown(KeyCode.V))
		{
			this.m_ShowDivingMask = !this.m_ShowDivingMask;
		}
		else if (Input.GetKeyDown(KeyCode.B))
		{
			this.m_ShowBiohazardMask = !this.m_ShowBiohazardMask;
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			this.StartDream("Goldmine_dream_spawner");
		}
		else if (Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.F))
		{
			this.SetWantedItem(Hand.Right, ItemsManager.Get().CreateItem(ItemID.Fire, false, Vector3.zero, Quaternion.identity), true);
		}
		else if (Input.GetKeyDown(KeyCode.F10))
		{
			if (CraftingManager.Get().gameObject.activeSelf)
			{
				CraftingManager.Get().Deactivate();
			}
			else
			{
				CraftingManager.Get().Activate();
			}
		}
		else if (Input.GetKeyDown(KeyCode.F11))
		{
			this.ItemsFromHandsPutToInventory();
		}
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
					goto IL_F8;
				}
				transform = base.gameObject.transform.FindDeepChild(key3.GetVariable(0).SValue);
				if (!(transform == null))
				{
					if (!this.m_Animations[key2].ContainsKey(transform))
					{
						this.m_Animations[key2][transform] = new List<SimpleTransform>();
						goto IL_F8;
					}
					goto IL_F8;
				}
				IL_1ED:
				j++;
				continue;
				IL_F8:
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
				goto IL_1ED;
			}
		}
	}

	private void StoreAdditiveAnimationStartFrame(Dictionary<Transform, List<SimpleTransform>> dict)
	{
		this.m_AdditiveAnimationStartFrame.Clear();
		for (int i = 0; i < dict.Count; i++)
		{
			Transform key = dict.ElementAt(i).Key;
			SimpleTransform value;
			value.m_Pos = key.localPosition;
			value.m_Rot = key.localRotation;
			this.m_AdditiveAnimationStartFrame.Add(key, value);
		}
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
					float f = currentAnimatorStateInfo.normalizedTime * currentAnimatorStateInfo.length / Player.s_AdditiveAnimationSampleInterval;
					int num2 = (int)Mathf.Floor(f);
					int num3 = num2 + 1;
					for (int j = 0; j < dictionary.Count; j++)
					{
						Transform key = dictionary.ElementAt(j).Key;
						List<SimpleTransform> value = dictionary.ElementAt(j).Value;
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
				for (int k = 0; k < this.m_LastAdditiveAnimationFrame.Count; k++)
				{
					Transform key2 = this.m_LastAdditiveAnimationFrame.ElementAt(k).Key;
					Quaternion localRotation = Quaternion.Slerp(this.m_LastAdditiveAnimationFrame[key2].m_Rot, key2.localRotation, num5);
					key2.localRotation = localRotation;
					Vector3 localPosition = this.m_LastAdditiveAnimationFrame[key2].m_Pos + (key2.localPosition - this.m_LastAdditiveAnimationFrame[key2].m_Pos) * num5;
					key2.localPosition = localPosition;
				}
			}
			else
			{
				this.m_PostBlendAdditiveAnimation = false;
			}
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
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.ChangeSceneShowLoadingScreen), 1.5f, null);
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
		FPPController fppcontroller = Player.Get().m_FPPController;
		fppcontroller.SetLookDev(zero);
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

	public void StartAim(Player.AimType type)
	{
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
		this.m_StopAimCameraMtx.SetColumn(0, Camera.main.transform.right);
		this.m_StopAimCameraMtx.SetColumn(1, Camera.main.transform.up);
		this.m_StopAimCameraMtx.SetColumn(2, Camera.main.transform.forward);
		this.m_StopAimCameraMtx.SetColumn(3, Camera.main.transform.position);
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
			b = Skill.Get<ArcherySkill>().GetAimDuration();
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
		FPPController fppcontroller = Player.Get().m_FPPController;
		fppcontroller.SetLookDev(zero);
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
	}

	private void OnTriggerExit(Collider other)
	{
		WaterCollider component = other.gameObject.GetComponent<WaterCollider>();
		if (component)
		{
			WaterBoxManager.Get().OnExitWater(component);
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
		if (this.m_ActiveFightController && this.m_ActiveFightController.IsBlock())
		{
			this.m_ActiveFightController.SetBlock(false);
		}
		Item item = slot.m_Item;
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Item currentItem = this.GetCurrentItem(Hand.Right);
			Item currentItem2 = this.GetCurrentItem(Hand.Left);
			this.SetWantedItem(Hand.Right, null, true);
			this.SetWantedItem(Hand.Left, null, true);
			if (this.m_ControllerToStart != PlayerControllerType.Unknown)
			{
				this.StartControllerInternal();
			}
			ItemSlot equippedItemSlot = InventoryBackpack.Get().m_EquippedItemSlot;
			if (currentItem)
			{
				currentItem.m_ShownInInventory = true;
				InventoryBackpack.Get().InsertItem(currentItem, equippedItemSlot, null, true, true, true, true, true);
			}
			else if (currentItem2)
			{
				currentItem2.m_ShownInInventory = true;
				InventoryBackpack.Get().InsertItem(currentItem2, equippedItemSlot, null, true, true, true, true, true);
			}
			if (item)
			{
				InventoryBackpack.Get().RemoveItem(item, false);
				this.SetWantedItem(item, true);
			}
		}
		InventoryBackpack.Get().m_EquippedItemSlot = slot;
		InventoryBackpack.Get().m_EquippedItem = item;
		this.m_LastEquipTime = Time.time;
		if (this.m_Aim)
		{
			this.StopAim();
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
	}

	public void DetachObjectFromPlayer(GameObject obj)
	{
		obj.transform.parent = null;
	}

	public void EnableCollisions()
	{
		this.m_CharacterController.detectCollisions = true;
		base.GetComponent<Rigidbody>().useGravity = true;
	}

	public void DisableCollisions()
	{
		this.m_CharacterController.detectCollisions = false;
		base.GetComponent<Rigidbody>().useGravity = false;
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
		List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Count; i++)
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
		return Inventory3DManager.Get().IsActive() || BodyInspectionController.Get().IsActive() || this.m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) || this.m_Animator.GetBool(ItemController.Get().m_FireHash) || NotepadController.Get().IsActive() || MapController.Get().IsActive() || LadderController.Get().IsActive() || LiquidInHandsController.Get().IsActive() || SwimController.Get().IsActive() || HeavyObjectController.Get().IsActive() || BoatController.Get().IsActive() || ConsciousnessController.Get().IsActive() || GrapplingHookController.Get().IsActive() || HarvestingAnimalController.Get().IsActive() || HarvestingSmallAnimalController.Get().IsActive() || CraftingController.Get().IsActive() || MakeFireController.Get().IsActive() || DeathController.Get().IsActive() || (ItemController.Get().IsActive() && ItemController.Get().m_StoneThrowing) || CutscenesManager.Get().IsCutscenePlaying() || ConstructionController.Get().IsActive() || VomitingController.Get().IsActive() || WatchController.Get().IsActive();
	}

	private void UpdateWeapon()
	{
		if (this.ShouldHideWeapon())
		{
			this.HideWeapon();
		}
		else
		{
			this.ShowWeapon();
		}
	}

	public void HideWeapon()
	{
		Item currentItem = this.GetCurrentItem();
		if (currentItem && currentItem.m_Info.IsWeapon())
		{
			this.SetWantedItem((!currentItem.m_Info.IsBow()) ? Hand.Right : Hand.Left, null, true);
			currentItem.m_ShownInInventory = true;
			InventoryBackpack.Get().InsertItem(currentItem, InventoryBackpack.Get().m_EquippedItemSlot, null, true, true, true, true, false);
		}
	}

	private void ShowWeapon()
	{
		if (InventoryBackpack.Get().m_EquippedItemSlot.m_Item && this.m_ControllerToStart == PlayerControllerType.Unknown)
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
	}

	public bool HasBlade()
	{
		if (InventoryBackpack.Get().Contains(ItemID.Obsidian_Blade) || InventoryBackpack.Get().Contains(ItemID.Obsidian_Bone_Blade) || InventoryBackpack.Get().Contains(ItemID.Stick_Blade) || InventoryBackpack.Get().Contains(ItemID.Stone_Blade) || InventoryBackpack.Get().Contains(ItemID.Bone_Knife))
		{
			return true;
		}
		Item currentItem = this.GetCurrentItem(Hand.Right);
		return !(currentItem == null) && (currentItem.m_Info.m_ID == ItemID.Obsidian_Blade || currentItem.m_Info.m_ID == ItemID.Obsidian_Bone_Blade || currentItem.m_Info.m_ID == ItemID.Stick_Blade || currentItem.m_Info.m_ID == ItemID.Stone_Blade || currentItem.m_Info.m_ID == ItemID.Bone_Knife);
	}

	public static float GetSleepTimeFactor()
	{
		TOD_Sky todsky = MainLevel.Instance.m_TODSky;
		TOD_Time todtime = MainLevel.Instance.m_TODTime;
		bool flag = todsky.Cycle.Hour > 6f && todsky.Cycle.Hour <= 18f;
		float num;
		if (flag)
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
		bool flag = todsky.Cycle.Hour > 6f && todsky.Cycle.Hour <= 18f;
		float num;
		if (flag)
		{
			num = todtime.m_DayLengthInMinutes / 12f * 60f;
		}
		else
		{
			num = todtime.m_NightLengthInMinutes / 12f * 60f;
		}
		return num * ConsciousnessController.Get().m_HoursDelta;
	}

	private static Player s_Instance;

	private PlayerParams m_Params;

	[HideInInspector]
	public CharacterController m_CharacterController;

	[HideInInspector]
	public PlayerAudioModule m_AudioModule;

	private int m_BlockMovesRequestsCount;

	private int m_BlockRotationRequestsCount;

	private Item[] m_WantedItem = new Item[2];

	private Item[] m_CurrentItem = new Item[2];

	private Transform m_LHand;

	private Transform m_RHand;

	public Transform m_LFoot;

	public Transform m_RFoot;

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
	public bool m_IsInAir;

	[HideInInspector]
	public Vector3 m_LastPosOnGround = Vector3.zero;

	[HideInInspector]
	public float m_LastTimeOnGround;

	private float m_MinFallingHeight = 0.2f;

	private float m_MinFallingDuration = 0.5f;

	private float m_MinFallingHeightToDealDamage = 3f;

	private bool m_InWater;

	private float m_WaterLevel = float.MinValue;

	public static float DEEP_WATER = 1.8f;

	private bool m_Climbing;

	private Dictionary<Transform, float> m_RotatedBodyBones = new Dictionary<Transform, float>();

	private PlayerController m_LastActiveBodyRotationController;

	private float m_LastTimeBodyRotationControllerChange;

	[HideInInspector]
	public string m_DreamToActivate = string.Empty;

	[HideInInspector]
	public bool m_DreamActive;

	private Vector3 m_LastPosBeforeDream = Vector3.zero;

	private Quaternion m_LastRotBeforeDream = Quaternion.identity;

	public float m_DreamDuration;

	private Vector3 m_StoredPos = Vector3.zero;

	private Quaternion m_StoredRotation = Quaternion.identity;

	private Vector2 m_StoredLookDev = Vector2.zero;

	[HideInInspector]
	public Collider m_Collider;

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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0,
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0
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
			0,
			0,
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

	private PlayerController[] m_PlayerControllers = new PlayerController[41];

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

	private float m_StartShakePower;

	private float m_ShakeDuration;

	private float m_ShakeSpeed;

	private float m_StartShakeTime;

	private PlannerMode m_PlannerScheduled = PlannerMode.None;

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

	private Player.AimType m_AimType;

	[HideInInspector]
	public bool m_Aim;

	[HideInInspector]
	public float m_AimPower;

	private float m_StartAimTime;

	[HideInInspector]
	public float m_StopAimTime;

	[HideInInspector]
	public Matrix4x4 m_StopAimCameraMtx = default(Matrix4x4);

	[HideInInspector]
	public Animator m_Animator;

	private float m_StartTime;

	private float m_InitDuration = 2f;

	private Vector3 m_TraveledDistStatLastPos = Vector3.zero;

	private float m_TraveledDistStatCurrShift;

	private float m_LastEquipTime;

	[HideInInspector]
	public bool m_OpenBackpackSheduled;

	private PlayerConditionModule m_ConditionModule;

	private PlayerDiseasesModule m_DiseasesModule;

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

	private float m_LastAddInjuryTime;

	private bool m_AdditiveAnimationActiveLastFrame;

	private List<AnimatorClipInfo> m_AnimatorClipInfo = new List<AnimatorClipInfo>(300);

	private GameObject m_ChangeSceneSpawner;

	[HideInInspector]
	public bool m_MovesBlockedOnChangeScene;

	private float m_TeleportTime = float.MinValue;

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
