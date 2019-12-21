using System;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class Injury
{
	public Injury(InjuryType type, InjuryPlace place, BIWoundSlot slot, InjuryState state, int poison_level = 0, Injury parent_injury = null, DamageInfo damage_info = null)
	{
		this.m_Type = type;
		this.m_Place = place;
		slot.SetInjury(this);
		this.m_Slot = slot;
		this.m_StartTimeInMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		this.m_PoisonLevel = poison_level;
		this.m_TimeToInfect = this.m_DefaultTimeToInfect;
		this.m_ParentInjury = parent_injury;
		if (damage_info != null && damage_info.m_Damager != null)
		{
			AI component = damage_info.m_Damager.GetComponent<AI>();
			if (component != null)
			{
				this.m_AIDamager = component.m_ID;
			}
		}
		this.m_State = state;
		this.UpdateHealthDecreasePerSec();
		this.SetWoundMaterial(this.m_Slot.m_Wound);
		if (slot.m_AdditionalMeshes != null)
		{
			for (int i = 0; i < slot.m_AdditionalMeshes.Count; i++)
			{
				this.SetWoundMaterial(slot.m_AdditionalMeshes[i]);
			}
		}
		this.SetAdditionalInjury(this);
		Injury.s_NumInjuries++;
	}

	~Injury()
	{
		Injury.s_NumInjuries--;
	}

	private void SetAdditionalInjury(Injury parent_injury)
	{
		if (this.m_Type != InjuryType.Worm)
		{
			return;
		}
		BIWoundSlot biwoundSlot = null;
		if (this.m_Slot.transform.name == "Wound00")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_L_Wound01");
		}
		else if (this.m_Slot.transform.name == "Wound05")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_L_Wound02");
		}
		else if (this.m_Slot.transform.name == "Wound06")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_L_Wound00");
		}
		else if (this.m_Slot.transform.name == "Wound09")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_R_Wound01");
		}
		else if (this.m_Slot.transform.name == "Wound11")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_R_Wound02");
		}
		else if (this.m_Slot.transform.name == "Wound12")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Hand_R_Wound00");
		}
		else if (this.m_Slot.transform.name == "Wound16")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_L_Wound01");
		}
		else if (this.m_Slot.transform.name == "Wound21")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_L_Wound02");
		}
		else if (this.m_Slot.transform.name == "Wound24")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_L_Wound00");
		}
		else if (this.m_Slot.transform.name == "Wound29")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_R_Wound01");
		}
		else if (this.m_Slot.transform.name == "Wound31")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_R_Wound02");
		}
		else if (this.m_Slot.transform.name == "Wound37")
		{
			biwoundSlot = BodyInspectionController.Get().GetWoundSlot(this.m_Place, "Leg_R_Wound00");
		}
		if (biwoundSlot != null)
		{
			PlayerInjuryModule.Get().AddInjury(InjuryType.WormHole, this.m_Place, biwoundSlot, InjuryState.WormInside, 0, parent_injury, null);
		}
	}

	private void SetWoundMaterial(GameObject obj)
	{
		Injury.SetWoundMaterial(obj, this.m_State, this.m_Type);
	}

	public static void SetWoundMaterial(GameObject obj, InjuryState state, InjuryType type)
	{
		if (obj == null)
		{
			return;
		}
		Renderer componentDeepChild = General.GetComponentDeepChild<Renderer>(obj);
		switch (type)
		{
		case InjuryType.SmallWoundAbrassion:
			if (state == InjuryState.Open)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_abrasion", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Infected)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_abrasion_infected", typeof(Material)) as Material);
				return;
			}
			break;
		case InjuryType.SmallWoundScratch:
			if (state == InjuryState.Open)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_scratch", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Infected)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_scratch_infected", typeof(Material)) as Material);
				return;
			}
			break;
		case InjuryType.Laceration:
			if (state == InjuryState.Bleeding)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Open)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Infected)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_infected", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Closed)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_closed", typeof(Material)) as Material);
				return;
			}
			break;
		case InjuryType.LacerationCat:
			if (state == InjuryState.Bleeding)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_cat", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Open)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_cat", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Infected)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_cat_infected", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Closed)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_laceration_cat_closed", typeof(Material)) as Material);
				return;
			}
			break;
		case InjuryType.Rash:
			componentDeepChild.material = (Resources.Load("Decals/wound_rash", typeof(Material)) as Material);
			return;
		case InjuryType.Worm:
		case InjuryType.Leech:
		case InjuryType.LeechHole:
			break;
		case InjuryType.WormHole:
			if (state == InjuryState.WormInside)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_worm_hole_closed", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Open)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_worm_leech_hole", typeof(Material)) as Material);
				return;
			}
			if (state == InjuryState.Infected)
			{
				componentDeepChild.material = (Resources.Load("Decals/wound_worm_hole_infected", typeof(Material)) as Material);
			}
			break;
		case InjuryType.VenomBite:
			componentDeepChild.material = (Resources.Load("Decals/wound_venom_bite", typeof(Material)) as Material);
			return;
		case InjuryType.SnakeBite:
			componentDeepChild.material = (Resources.Load("Decals/wound_snake_bite", typeof(Material)) as Material);
			return;
		default:
			return;
		}
	}

	public InjuryType GetInjuryType()
	{
		return this.m_Type;
	}

	public InjuryPlace GetInjuryPlace()
	{
		return this.m_Place;
	}

	public bool GetDiagnosed()
	{
		return this.m_Diagnosed;
	}

	public void SetDiagnosed(bool set)
	{
		this.m_Diagnosed = set;
	}

	public BIWoundSlot GetSlot()
	{
		return this.m_Slot;
	}

	public void StartHealing()
	{
		this.m_Healing = true;
		this.m_HealingStartTime = MainLevel.Instance.GetCurrentTimeMinutes();
	}

	public void RemoveBandage()
	{
		if (this.m_Bandage != null)
		{
			if (this.m_Bandage.GetComponent<Item>() != null && (this.m_Bandage.GetComponent<Item>().m_Info.m_ID == ItemID.Larva || this.m_Bandage.GetComponent<Item>().m_Info.m_ID == ItemID.Ants || this.m_Bandage.GetComponent<Item>().m_Info.m_ID == ItemID.Maggot))
			{
				UnityEngine.Object.Destroy(this.m_Bandage);
				return;
			}
			this.m_Bandage.SetActive(false);
			this.m_Bandage = null;
		}
	}

	public float GetHealingDuration()
	{
		if (this.m_Type == InjuryType.Laceration || this.m_Type == InjuryType.LacerationCat)
		{
			if (this.m_State == InjuryState.Bleeding)
			{
				return Injury.s_HealingLacerationBleedingDurationInMinutes - this.m_HealingTimeDec;
			}
			if (this.m_State == InjuryState.Closed)
			{
				return Injury.s_HealingLacerationClosedDurationInMinutes - this.m_HealingTimeDec;
			}
			if (this.m_State == InjuryState.Infected)
			{
				return Injury.s_HealingLacerationInfectedDurationInMinutes - this.m_HealingTimeDec;
			}
			if (this.m_State == InjuryState.Open)
			{
				return Injury.s_HealingLacerationOpenDurationInMinutes - this.m_HealingTimeDec;
			}
		}
		else
		{
			if (this.m_Type == InjuryType.Rash)
			{
				return Injury.s_HealingRashDurationInMinutes - this.m_HealingTimeDec;
			}
			if (this.m_Type == InjuryType.SmallWoundAbrassion)
			{
				if (this.m_State == InjuryState.Open)
				{
					return Injury.s_HealingAbrasionOpenDurationInMinutes - this.m_HealingTimeDec;
				}
				if (this.m_State == InjuryState.Infected)
				{
					return Injury.s_HealingAbrasionInfectedDurationInMinutes - this.m_HealingTimeDec;
				}
			}
			else if (this.m_Type == InjuryType.SmallWoundScratch)
			{
				if (this.m_State == InjuryState.Open)
				{
					return Injury.s_HealingScratchOpenDurationInMinutes - this.m_HealingTimeDec;
				}
				if (this.m_State == InjuryState.Infected)
				{
					return Injury.s_HealingScratchInfectedDurationInMinutes - this.m_HealingTimeDec;
				}
			}
			else
			{
				if (this.m_Type == InjuryType.VenomBite)
				{
					return Injury.s_HealingVenomBiteDurationInMinutes - this.m_HealingTimeDec;
				}
				if (this.m_Type == InjuryType.SnakeBite)
				{
					return Injury.s_HealingSnakeBiteDurationInMinutes - this.m_HealingTimeDec;
				}
				if (this.m_Type == InjuryType.WormHole)
				{
					if (this.m_State == InjuryState.Open)
					{
						return Injury.s_HealingWormHoleOpenDurationInMinutes - this.m_HealingTimeDec;
					}
					if (this.m_State == InjuryState.Infected)
					{
						return Injury.s_HealingWormHoleInfectedDurationInMinutes - this.m_HealingTimeDec;
					}
				}
			}
		}
		return Injury.m_HealingDefaultDuration - this.m_HealingTimeDec;
	}

	public void Update()
	{
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (Player.Get().IsInWater())
		{
			this.m_TimeToInfect -= Time.deltaTime * 5f;
		}
		if (this.m_Type == InjuryType.SmallWoundAbrassion || this.m_Type == InjuryType.SmallWoundScratch || this.m_Type == InjuryType.WormHole)
		{
			if (this.m_State == InjuryState.Open)
			{
				if (currentTimeMinutes < this.m_HealingStartTime)
				{
					if (currentTimeMinutes - this.m_StartTimeInMinutes > this.m_TimeToInfect)
					{
						this.Infect();
					}
				}
				else if (currentTimeMinutes - this.m_HealingStartTime > this.GetHealingDuration())
				{
					if (this.m_HealingResultInjuryState == InjuryState.Infected)
					{
						this.Infect();
					}
					else
					{
						PlayerInjuryModule.Get().HealInjury(this);
					}
				}
			}
			else if (this.m_State == InjuryState.Infected && currentTimeMinutes - this.m_HealingStartTime > this.GetHealingDuration())
			{
				this.Disinfect();
			}
		}
		else if (this.m_Type == InjuryType.Leech)
		{
			if (currentTimeMinutes > this.m_HealingStartTime)
			{
				PlayerInjuryModule.Get().HealInjury(this);
			}
		}
		else if (this.m_Type == InjuryType.VenomBite || this.m_Type == InjuryType.SnakeBite)
		{
			if (currentTimeMinutes - this.m_StartTimeInMinutes > Injury.s_PoisonAutoDebufTime)
			{
				this.PoisonDebuff(1);
				this.m_StartTimeInMinutes = currentTimeMinutes;
			}
			if (this.m_PoisonLevel == 0)
			{
				this.RemoveBandage();
				this.m_HealingStartTime = float.MaxValue;
				PlayerInjuryModule.Get().HealInjury(this);
			}
			if (currentTimeMinutes - this.m_HealingStartTime > this.GetHealingDuration())
			{
				this.PoisonDebuff(this.m_Slot.m_PoisonDebuff);
				this.RemoveBandage();
				this.m_HealingStartTime = float.MaxValue;
				if (this.m_PoisonLevel == 0)
				{
					PlayerInjuryModule.Get().HealInjury(this);
				}
			}
		}
		else if (this.m_Type == InjuryType.Laceration || this.m_Type == InjuryType.LacerationCat)
		{
			if (this.m_State == InjuryState.Bleeding)
			{
				if (!this.m_Healing)
				{
					DamageInfo damageInfo = new DamageInfo();
					float num = 1f;
					if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
					{
						num = Player.GetSleepTimeFactor();
						damageInfo.m_Damage = num * 0.2f;
					}
					else
					{
						damageInfo.m_Damage = Time.deltaTime * 0.2f * num;
					}
					damageInfo.m_FromInjury = true;
					damageInfo.m_PlayDamageSound = false;
					Player.Get().TakeDamage(damageInfo);
				}
				if (this.m_State == InjuryState.Bleeding && this.m_HealingResultInjuryState == InjuryState.None && currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationBleedingDurationInMinutes - this.m_HealingTimeDec)
				{
					PlayerInjuryModule.Get().HealInjury(this);
				}
				if (this.m_HealingResultInjuryState == InjuryState.Infected && currentTimeMinutes - this.m_HealingStartTime > 20f)
				{
					this.Infect();
				}
			}
			else if (this.m_State == InjuryState.Open)
			{
				if (this.m_HealingResultInjuryState == InjuryState.None)
				{
					if (currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationOpenDurationInMinutes - this.m_HealingTimeDec)
					{
						PlayerInjuryModule.Get().HealInjury(this);
					}
				}
				else if (currentTimeMinutes < this.m_HealingStartTime)
				{
					if (currentTimeMinutes - this.m_StartTimeInMinutes > this.m_TimeToInfect)
					{
						this.Infect();
					}
				}
				else if (this.m_HealingResultInjuryState == InjuryState.Infected && currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationOpenDurationInMinutes - this.m_HealingTimeDec && this.m_Bandage != null)
				{
					this.Infect();
				}
			}
			else if (this.m_State == InjuryState.Infected)
			{
				if (currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationBleedingDurationInMinutes)
				{
					this.Disinfect();
				}
			}
			else if (this.m_State == InjuryState.Closed && currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationBleedingDurationInMinutes)
			{
				PlayerInjuryModule.Get().HealInjury(this);
			}
			if (currentTimeMinutes - this.m_HealingStartTime > Injury.s_HealingLacerationBleedingDurationInMinutes)
			{
				if (this.m_HealingResultInjuryState == InjuryState.Open)
				{
					this.OpenWound();
				}
			}
			else if (currentTimeMinutes - this.m_HealingStartTime > 0f && this.m_HealingResultInjuryState == InjuryState.Closed)
			{
				this.CloseWound();
			}
		}
		else if (this.m_Type == InjuryType.Rash)
		{
			if (currentTimeMinutes - this.m_StartTimeInMinutes > this.GetHealingDuration())
			{
				PlayerInjuryModule.Get().HealInjury(this);
			}
		}
		else if (this.m_Type == InjuryType.Worm && currentTimeMinutes - this.m_HealingStartTime > 0f)
		{
			PlayerInjuryModule.Get().OpenChildrenInjuries(this);
			PlayerInjuryModule.Get().HealInjury(this);
		}
		if (this.m_Type == InjuryType.Leech && Time.time > this.m_EffectLastTime + this.m_EffectCooldown)
		{
			this.m_EffectLastTime = Time.time;
		}
	}

	private void SetState(InjuryState state)
	{
		this.m_State = state;
		this.m_StartTimeInMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
	}

	public void Infect()
	{
		this.SetState(InjuryState.Infected);
		this.SetWoundMaterial(this.m_Slot.m_Wound.gameObject);
		this.m_HealingStartTime = float.MaxValue;
		this.UpdateHealthDecreasePerSec();
		this.RemoveBandage();
		Disease disease = PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.Fever);
		if (disease != null && disease.IsActive())
		{
			disease.IncreaseLevel(1);
		}
		else
		{
			PlayerDiseasesModule.Get().RequestDisease(ConsumeEffect.Fever, 0f, 1);
		}
		PlayerInjuryModule.Get().UnlockKnownInjuryState(InjuryState.Infected);
	}

	public void Disinfect()
	{
		BodyInspectionController.Get().DestroyMaggots(this.m_Slot);
		this.SetState(InjuryState.Open);
		this.m_HealingStartTime = float.MaxValue;
		this.m_TimeToInfect = this.m_DefaultTimeToInfect;
		this.SetWoundMaterial(this.m_Slot.m_Wound.gameObject);
		this.UpdateHealthDecreasePerSec();
		this.RemoveBandage();
	}

	public void OpenWound()
	{
		this.SetState(InjuryState.Open);
		this.m_HealingStartTime = float.MaxValue;
		this.m_TimeToInfect = this.m_DefaultTimeToInfect;
		if (this.m_Slot != null && this.m_Slot.m_Wound != null)
		{
			this.SetWoundMaterial(this.m_Slot.m_Wound.gameObject);
		}
		this.UpdateHealthDecreasePerSec();
		this.RemoveBandage();
	}

	public void CloseWound()
	{
		this.SetState(InjuryState.Closed);
		this.SetWoundMaterial(this.m_Slot.m_Wound.gameObject);
		this.m_HealingStartTime = MainLevel.Instance.GetCurrentTimeMinutes();
		this.UpdateHealthDecreasePerSec();
		this.RemoveBandage();
		this.m_HealingResultInjuryState = InjuryState.None;
	}

	private void UpdateHealthDecreasePerSec()
	{
		switch (this.m_Type)
		{
		case InjuryType.SmallWoundAbrassion:
		case InjuryType.SmallWoundScratch:
		case InjuryType.WormHole:
			if (this.m_State == InjuryState.Infected)
			{
				this.m_HealthDecreasePerSec = Injury.s_InfectedWoundHealthDecPerSec;
				return;
			}
			this.m_HealthDecreasePerSec = Injury.s_WoundHealthDecPerSec;
			return;
		case InjuryType.Laceration:
		case InjuryType.LacerationCat:
			this.m_HealthDecreasePerSec = Injury.s_BiteHealthDecPerSec;
			return;
		case InjuryType.Rash:
			this.m_HealthDecreasePerSec = Injury.s_BlainHealthDecPerSec;
			break;
		case InjuryType.Worm:
			this.m_HealthDecreasePerSec = Injury.s_WormHealthDecPerSec;
			return;
		case InjuryType.Leech:
			this.m_HealthDecreasePerSec = Injury.s_LeechHealthDecPerSec;
			return;
		case InjuryType.LeechHole:
			break;
		case InjuryType.VenomBite:
		case InjuryType.SnakeBite:
			this.m_HealthDecreasePerSec = Injury.s_PoisonedWoundHealthDecPerSec;
			return;
		default:
			return;
		}
	}

	public void Save(int index)
	{
		SaveGame.SaveVal("InjuryDiag" + index, this.m_Diagnosed);
		SaveGame.SaveVal("InjuryStart" + index, this.m_StartTimeInMinutes);
		SaveGame.SaveVal("InjuryBand" + index, this.m_Bandage ? this.m_Bandage.name : string.Empty);
		SaveGame.SaveVal("InjuryHeal" + index, this.m_Healing);
		SaveGame.SaveVal("InjuryHealStart" + index, this.m_HealingStartTime);
		SaveGame.SaveVal("HealingResultInjuryState" + index, (int)this.m_HealingResultInjuryState);
		SaveGame.SaveVal("PIMDamagerAIID" + index, (int)this.m_AIDamager);
	}

	public void Load(int index)
	{
		this.m_Diagnosed = SaveGame.LoadBVal("InjuryDiag" + index);
		this.m_StartTimeInMinutes = SaveGame.LoadFVal("InjuryStart" + index);
		string text = SaveGame.LoadSVal("InjuryBand" + index);
		if (text != string.Empty)
		{
			Transform transform = Player.Get().gameObject.transform.FindDeepChild(text);
			if (transform)
			{
				transform.gameObject.SetActive(true);
				this.m_Bandage = transform.gameObject;
			}
			else
			{
				BodyInspectionController.Get().AttachMaggots(true);
			}
		}
		this.m_Healing = SaveGame.LoadBVal("InjuryHeal" + index);
		this.m_HealingStartTime = SaveGame.LoadFVal("InjuryHealStart" + index);
		this.m_HealingResultInjuryState = (InjuryState)SaveGame.LoadIVal("HealingResultInjuryState" + index);
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate6)
		{
			this.m_AIDamager = (AI.AIID)SaveGame.LoadIVal("PIMDamagerAIID" + index);
		}
		this.UpdateHealthDecreasePerSec();
	}

	public void PoisonDebuff(int level)
	{
		this.m_PoisonLevel -= level;
		if (this.m_PoisonLevel < 0)
		{
			this.m_PoisonLevel = 0;
		}
	}

	public bool IsWound()
	{
		return this.m_Type == InjuryType.Laceration || this.m_Type == InjuryType.LacerationCat || this.m_Type == InjuryType.Rash || this.m_Type == InjuryType.SmallWoundAbrassion || this.m_Type == InjuryType.SmallWoundScratch || this.m_Type == InjuryType.SnakeBite || this.m_Type == InjuryType.VenomBite || this.m_Type == InjuryType.SnakeBite || this.m_Type == InjuryType.WormHole;
	}

	public bool IsHealing()
	{
		return MainLevel.Instance.GetCurrentTimeMinutes() > this.m_HealingStartTime;
	}

	public void CheckInfectionFromDirt()
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		Disease disease = PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.DirtSickness);
		if (num < (float)disease.m_Level * 0.2f)
		{
			this.m_HealingResultInjuryState = InjuryState.Infected;
		}
	}

	public InjuryType m_Type = InjuryType.None;

	public InjuryPlace m_Place = InjuryPlace.None;

	public InjuryState m_State = InjuryState.Open;

	public BIWoundSlot m_Slot;

	private bool m_Diagnosed;

	public float m_HealthDecreasePerSec = 0.2f;

	public static float s_BiteHealthDecPerSec = 0.05f;

	public static float s_BurnHealthDecPerSec = 0.05f;

	public static float s_WoundHealthDecPerSec = 0f;

	public static float s_InfectedWoundHealthDecPerSec = 0.2f;

	public static float s_LeechHealthDecPerSec = 0f;

	public static float s_WormHealthDecPerSec = 0f;

	public static float s_BlainHealthDecPerSec = 0f;

	public static float s_PoisonedWoundHealthDecPerSec = 0.3f;

	public static float s_CleanWoundHealthDecPerSec = 0f;

	public static float s_PoisonedWoundHydrationDecPerSec = 0.01f;

	public int m_PoisonLevel;

	public float m_StartTimeInMinutes;

	private float m_DefaultTimeToInfect = 500f;

	public float m_TimeToInfect = 500f;

	public bool m_Healing;

	public float m_HealingStartTime = float.MaxValue;

	public static float m_HealingDefaultDuration = 20f;

	public static float s_HealingLacerationBleedingDurationInMinutes = 50f;

	public static float s_HealingLacerationClosedDurationInMinutes = 35f;

	public static float s_HealingLacerationInfectedDurationInMinutes = 75f;

	public static float s_HealingLacerationOpenDurationInMinutes = 45f;

	public static float s_HealingRashDurationInMinutes = 120f;

	public static float s_HealingAbrasionOpenDurationInMinutes = 60f;

	public static float s_HealingAbrasionInfectedDurationInMinutes = 75f;

	public static float s_HealingScratchOpenDurationInMinutes = 60f;

	public static float s_HealingScratchInfectedDurationInMinutes = 75f;

	public static float s_HealingVenomBiteDurationInMinutes = 55f;

	public static float s_HealingSnakeBiteDurationInMinutes = 55f;

	public static float s_HealingWormHoleOpenDurationInMinutes = 55f;

	public static float s_HealingWormHoleInfectedDurationInMinutes = 65f;

	public GameObject m_Bandage;

	public static float s_LeechCooldownInMinutes = 1098f;

	public static float s_PoisonAutoDebufTime = 60f;

	private float m_EffectLastTime = float.MinValue;

	private float m_EffectCooldown = 30f;

	public float m_HealingTimeDec;

	public InjuryState m_HealingResultInjuryState = InjuryState.Open;

	public Injury m_ParentInjury;

	public static int s_NumInjuries = 0;

	public AI.AIID m_AIDamager = AI.AIID.None;
}
