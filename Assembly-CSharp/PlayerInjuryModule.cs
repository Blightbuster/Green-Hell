using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class PlayerInjuryModule : PlayerModule, ISaveLoad
{
	public static PlayerInjuryModule Get()
	{
		return PlayerInjuryModule.s_Instance;
	}

	private void Awake()
	{
		PlayerInjuryModule.s_Instance = this;
	}

	public float GetLeechNextTime()
	{
		return this.m_LeechNextTime2;
	}

	public void SetLeechNextTime(float time)
	{
		this.m_LeechNextTime2 = time;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	private void Start()
	{
		this.LoadScript();
		this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		DebugUtils.Assert(this.m_ConditionModule, true);
		this.m_BodyInspectionController = Player.Get().GetComponent<BodyInspectionController>();
		DebugUtils.Assert(this.m_BodyInspectionController, true);
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_InjuryTreatments", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Treatment")
			{
				InjuryType key2 = (InjuryType)Enum.Parse(typeof(InjuryType), key.GetVariable(0).SValue);
				InjuryTreatment injuryTreatment = new InjuryTreatment();
				string svalue = key.GetVariable(1).SValue;
				if (svalue != null)
				{
					foreach (string text in svalue.Split(new char[]
					{
						';'
					}))
					{
						string[] array2 = text.Split(new char[]
						{
							'*'
						});
						ItemID item = (ItemID)Enum.Parse(typeof(ItemID), array2[0]);
						injuryTreatment.AddItem(item, (array2.Length <= 1) ? 1 : int.Parse(array2[1]));
					}
				}
				this.m_Treatments[(int)key2] = injuryTreatment;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		this.UpdateActiveInjuries();
	}

	private void UpdateActiveInjuries()
	{
		this.m_SanityDictionary.Clear();
		PlayerSanityModule playerSanityModule = PlayerSanityModule.Get();
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			Injury injury = this.m_Injuries[i];
			if (!injury.m_Healing)
			{
				this.m_DamageInfo.m_Damager = base.gameObject;
				float num = 1f;
				if (SleepController.Get().IsActive())
				{
					num = Player.GetSleepTimeFactor();
					this.m_DamageInfo.m_Damage = injury.m_HealthDecreasePerSec * num;
				}
				else
				{
					this.m_DamageInfo.m_Damage = injury.m_HealthDecreasePerSec * Time.deltaTime * num;
				}
				this.m_DamageInfo.m_PlayDamageSound = false;
				this.m_Player.TakeDamage(this.m_DamageInfo);
				int num2 = 0;
				if (this.m_SanityDictionary.TryGetValue((int)injury.m_Type, out num2))
				{
					Dictionary<int, int> sanityDictionary;
					int type;
					(sanityDictionary = this.m_SanityDictionary)[type = (int)injury.m_Type] = sanityDictionary[type] + 1;
				}
				else
				{
					this.m_SanityDictionary.Add((int)injury.m_Type, 1);
				}
			}
			injury.Update();
		}
		foreach (KeyValuePair<int, int> keyValuePair in this.m_SanityDictionary)
		{
			switch (keyValuePair.Key)
			{
			case 0:
			{
				PlayerSanityModule playerSanityModule2 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn = PlayerSanityModule.SanityEventType.SmallWoundAbrassion;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair2 = enumerator.Current;
				playerSanityModule2.OnEvent(evn, keyValuePair2.Value);
				break;
			}
			case 1:
			{
				PlayerSanityModule playerSanityModule3 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn2 = PlayerSanityModule.SanityEventType.SmallWoundScratch;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair3 = enumerator.Current;
				playerSanityModule3.OnEvent(evn2, keyValuePair3.Value);
				break;
			}
			case 2:
			{
				PlayerSanityModule playerSanityModule4 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn3 = PlayerSanityModule.SanityEventType.Laceration;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair4 = enumerator.Current;
				playerSanityModule4.OnEvent(evn3, keyValuePair4.Value);
				break;
			}
			case 3:
			{
				PlayerSanityModule playerSanityModule5 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn4 = PlayerSanityModule.SanityEventType.LacerationCat;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair5 = enumerator.Current;
				playerSanityModule5.OnEvent(evn4, keyValuePair5.Value);
				break;
			}
			case 4:
			{
				PlayerSanityModule playerSanityModule6 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn5 = PlayerSanityModule.SanityEventType.Rash;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair6 = enumerator.Current;
				playerSanityModule6.OnEvent(evn5, keyValuePair6.Value);
				break;
			}
			case 5:
			{
				PlayerSanityModule playerSanityModule7 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn6 = PlayerSanityModule.SanityEventType.Worm;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair7 = enumerator.Current;
				playerSanityModule7.OnEvent(evn6, keyValuePair7.Value);
				break;
			}
			case 6:
			{
				PlayerSanityModule playerSanityModule8 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn7 = PlayerSanityModule.SanityEventType.WormHole;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair8 = enumerator.Current;
				playerSanityModule8.OnEvent(evn7, keyValuePair8.Value);
				break;
			}
			case 7:
			{
				PlayerSanityModule playerSanityModule9 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn8 = PlayerSanityModule.SanityEventType.Leech;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair9 = enumerator.Current;
				playerSanityModule9.OnEvent(evn8, keyValuePair9.Value);
				break;
			}
			case 8:
			{
				PlayerSanityModule playerSanityModule10 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn9 = PlayerSanityModule.SanityEventType.LeechHole;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair10 = enumerator.Current;
				playerSanityModule10.OnEvent(evn9, keyValuePair10.Value);
				break;
			}
			case 9:
			{
				PlayerSanityModule playerSanityModule11 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn10 = PlayerSanityModule.SanityEventType.VenomBite;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair11 = enumerator.Current;
				playerSanityModule11.OnEvent(evn10, keyValuePair11.Value);
				break;
			}
			case 10:
			{
				PlayerSanityModule playerSanityModule12 = playerSanityModule;
				PlayerSanityModule.SanityEventType evn11 = PlayerSanityModule.SanityEventType.SnakeBite;
				Dictionary<int, int>.Enumerator enumerator;
				KeyValuePair<int, int> keyValuePair12 = enumerator.Current;
				playerSanityModule12.OnEvent(evn11, keyValuePair12.Value);
				break;
			}
			}
		}
	}

	public void CheckLeeches()
	{
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			return;
		}
		if (PlayerConditionModule.Get().GetParameterLossBlocked() || Time.time - MainLevel.Instance.m_LevelStartTime < 20f)
		{
			return;
		}
		if (!this.m_LeechNextTimeInitialized)
		{
			this.m_LeechNextTime2 = Injury.s_LeechCooldownInMinutes + MainLevel.Instance.GetCurrentTimeMinutes();
			this.m_LeechNextTimeInitialized = true;
		}
		if (MainLevel.Instance.GetCurrentTimeMinutes() < this.m_LeechNextTime2)
		{
			return;
		}
		InjuryType injuryType;
		if (this.m_Player.IsInWater())
		{
			if (this.m_LeechChanceInsideWater < UnityEngine.Random.Range(0f, 1f))
			{
				return;
			}
			injuryType = InjuryType.Leech;
		}
		else
		{
			if (this.m_LeechChanceOutsideOfWater < UnityEngine.Random.Range(0f, 1f))
			{
				return;
			}
			injuryType = InjuryType.Leech;
		}
		InjuryPlace place;
		if (this.m_Player.IsInWater() && !SwimController.Get().IsActive())
		{
			place = (InjuryPlace)UnityEngine.Random.Range(2, 4);
		}
		else
		{
			place = (InjuryPlace)UnityEngine.Random.Range(0, 4);
		}
		int num = UnityEngine.Random.Range(1, 4);
		if (RainManager.Get().m_WeatherInterpolated >= 1f)
		{
			num = UnityEngine.Random.Range(3, 6);
		}
		for (int i = 0; i < num; i++)
		{
			BIWoundSlot freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(place, injuryType, true);
			if (freeWoundSlot == null)
			{
				return;
			}
			this.AddInjury(injuryType, place, freeWoundSlot, InjuryState.Open, 0, null);
			this.m_LeechNextTime2 = Injury.s_LeechCooldownInMinutes + MainLevel.Instance.GetCurrentTimeMinutes();
		}
	}

	public void ScenarioAddInjury(string type, string place, string state)
	{
		InjuryType injuryType = (InjuryType)Enum.Parse(typeof(InjuryType), type);
		InjuryPlace place2 = (InjuryPlace)Enum.Parse(typeof(InjuryPlace), place);
		InjuryState injuryState = (InjuryState)Enum.Parse(typeof(InjuryState), state);
		BIWoundSlot freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(place2, injuryType, true);
		Injury injury = null;
		if (freeWoundSlot != null)
		{
			injury = this.AddInjury(injuryType, place2, freeWoundSlot, injuryState, 0, null);
		}
		if (injuryState != InjuryState.Open)
		{
			if (injuryState != InjuryState.Infected)
			{
				if (injuryState == InjuryState.Closed)
				{
					injury.CloseWound();
				}
			}
			else
			{
				injury.Infect();
			}
		}
		else
		{
			injury.OpenWound();
		}
	}

	public Injury AddInjury(InjuryType type, InjuryPlace place, BIWoundSlot slot, InjuryState state, int poison_level = 0, Injury parent_injury = null)
	{
		if (!slot || PlayerConditionModule.Get().GetParameterLossBlocked())
		{
			return null;
		}
		if (type == InjuryType.Leech && this.GetAllInjuries(type).Count == 0 && PlayerSanityModule.Get())
		{
			PlayerSanityModule.Get().ResetEventCooldown(PlayerSanityModule.SanityEventType.Leech);
		}
		Debug.Log("AddInjury");
		Injury injury = new Injury(type, place, slot, state, poison_level, parent_injury);
		this.m_Injuries.Add(injury);
		this.OnAddInjury(type);
		return injury;
	}

	public int GetInjuriesCount()
	{
		return this.m_Injuries.Count;
	}

	public Injury GetInjury(int index)
	{
		return (index < 0 || index >= this.m_Injuries.Count) ? null : this.m_Injuries[index];
	}

	public List<Injury> GetInjuriesList()
	{
		return this.m_Injuries;
	}

	public List<Injury> GetAllInjuries(InjuryType type)
	{
		this.m_InjuriesTempList.Clear();
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryType() == type)
			{
				this.m_InjuriesTempList.Add(this.m_Injuries[i]);
			}
		}
		return this.m_InjuriesTempList;
	}

	public List<Injury> GetAllInjuriesOfState(InjuryState state)
	{
		this.m_InjuryStateTempList.Clear();
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].m_State == state)
			{
				this.m_InjuryStateTempList.Add(this.m_Injuries[i]);
			}
		}
		return this.m_InjuryStateTempList;
	}

	public InjuryTreatment GetTreatment(InjuryType type)
	{
		InjuryTreatment result = null;
		this.m_Treatments.TryGetValue((int)type, out result);
		return result;
	}

	public void HealInjury(Injury injury)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].m_ParentInjury == injury)
			{
				this.m_Injuries[i].m_ParentInjury = null;
			}
		}
		BodyInspectionController.Get().DestroyAnts(injury.GetSlot());
		BodyInspectionController.Get().DestroyMaggots(injury.GetSlot());
		injury.GetSlot().SetInjury(null);
		injury.RemoveBandage();
		this.m_Injuries.Remove(injury);
		EventsManager.OnEvent(Enums.Event.HealWound, 1);
	}

	public void TryHealInjury(Injury injury)
	{
		if (!this.CanHealInjury(injury))
		{
			return;
		}
		injury.GetSlot().SetInjury(null);
		this.m_Injuries.Remove(injury);
		InjuryTreatment treatment = this.GetTreatment(injury.GetInjuryType());
		DebugUtils.Assert(treatment != null, true);
		Dictionary<int, int> allItems = treatment.GetAllItems();
		using (Dictionary<int, int>.KeyCollection.Enumerator enumerator = allItems.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ItemID itemID = (ItemID)enumerator.Current;
				InventoryBackpack.Get().RemoveItem(itemID, allItems[(int)itemID]);
			}
		}
		EventsManager.OnEvent(Enums.Event.HealWound, 1);
	}

	public bool CanHealInjury(Injury injury)
	{
		if (injury == null)
		{
			return false;
		}
		InjuryTreatment treatment = this.GetTreatment(injury.GetInjuryType());
		DebugUtils.Assert(treatment != null, true);
		Dictionary<int, int> allItems = treatment.GetAllItems();
		using (Dictionary<int, int>.KeyCollection.Enumerator enumerator = allItems.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ItemID itemID = (ItemID)enumerator.Current;
				if (InventoryBackpack.Get().GetItemsCount(itemID) < allItems[(int)itemID])
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (info.m_Blocked)
		{
			return;
		}
		if (info.m_Damage > 5f)
		{
			DamageType damageType = info.m_DamageType;
			InjuryType injuryType;
			switch (damageType)
			{
			case DamageType.Cut:
			case DamageType.Thrust:
				if (info.m_CriticalHit)
				{
					injuryType = InjuryType.Laceration;
				}
				else
				{
					injuryType = InjuryType.SmallWoundScratch;
				}
				break;
			default:
				if (damageType != DamageType.Claws)
				{
					if (damageType != DamageType.Insects)
					{
						if (damageType != DamageType.Fall)
						{
							if (damageType != DamageType.Critical)
							{
								if (damageType != DamageType.SnakePoison)
								{
									injuryType = InjuryType.SmallWoundAbrassion;
								}
								else
								{
									injuryType = InjuryType.SnakeBite;
								}
							}
							else
							{
								injuryType = InjuryType.Laceration;
							}
						}
						else
						{
							injuryType = InjuryType.SmallWoundAbrassion;
						}
					}
					else
					{
						injuryType = InjuryType.Rash;
					}
				}
				else
				{
					injuryType = InjuryType.LacerationCat;
				}
				break;
			case DamageType.Melee:
				injuryType = InjuryType.SmallWoundAbrassion;
				break;
			case DamageType.VenomPoison:
				injuryType = InjuryType.VenomBite;
				break;
			}
			Vector2 zero = Vector2.zero;
			zero.x = Vector3.Dot(base.transform.right, info.m_HitDir);
			zero.y = Vector3.Dot(base.transform.up, info.m_HitDir);
			float num = Vector2.Angle(Vector2.up, zero);
			if (zero.x < 0f)
			{
				num *= -1f;
			}
			BIWoundSlot freeWoundSlot;
			if (info.m_DamageType == DamageType.Insects && GreenHellGame.TWITCH_DEMO)
			{
				freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.LHand, injuryType, true);
			}
			else if (num < 45f && num > -45f)
			{
				freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.LLeg, injuryType, true);
				if (freeWoundSlot == null)
				{
					freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RLeg, injuryType, true);
				}
			}
			else if (num >= 45f && num < 135f)
			{
				freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.LHand, injuryType, true);
			}
			else if (num >= 135f || num < -135f)
			{
				freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.LHand, injuryType, true);
				if (freeWoundSlot == null)
				{
					freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RHand, injuryType, true);
				}
			}
			else
			{
				freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(InjuryPlace.RHand, injuryType, true);
			}
			if (freeWoundSlot != null)
			{
				InjuryState state = InjuryState.Open;
				if (injuryType == InjuryType.Laceration || injuryType == InjuryType.Laceration)
				{
					state = InjuryState.Bleeding;
				}
				else if (injuryType == InjuryType.WormHole)
				{
					state = InjuryState.WormInside;
				}
				this.AddInjury(injuryType, freeWoundSlot.m_InjuryPlace, freeWoundSlot, state, info.m_PoisonLevel, null);
			}
			else if (info.m_DamageType == DamageType.VenomPoison)
			{
				for (int i = 0; i < this.m_Injuries.Count; i++)
				{
					if (this.m_Injuries[i].m_Type == InjuryType.VenomBite)
					{
						this.m_Injuries[i].m_PoisonLevel += info.m_PoisonLevel;
						break;
					}
				}
			}
			else if (info.m_DamageType == DamageType.SnakePoison)
			{
				for (int j = 0; j < this.m_Injuries.Count; j++)
				{
					if (this.m_Injuries[j].m_Type == InjuryType.SnakeBite)
					{
						this.m_Injuries[j].m_PoisonLevel += info.m_PoisonLevel;
						break;
					}
				}
			}
		}
	}

	public void OnLanding(Vector3 speed)
	{
	}

	public bool IsAnyWound()
	{
		return this.m_Injuries.Count > 0;
	}

	public void Save()
	{
		SaveGame.SaveVal("InjuriesCount", this.m_Injuries.Count);
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			Injury injury = this.m_Injuries[i];
			SaveGame.SaveVal("InjuryType" + i, (int)injury.m_Type);
			SaveGame.SaveVal("InjuryPlace" + i, (int)injury.m_Place);
			SaveGame.SaveVal("InjuryState" + i, (int)injury.m_State);
			SaveGame.SaveVal("InjurySlot" + i, injury.m_Slot.name);
			SaveGame.SaveVal("InjuryPoisonLevel" + i, injury.m_PoisonLevel);
			injury.Save(i);
		}
	}

	public void Load()
	{
		this.ResetInjuries();
		int num = SaveGame.LoadIVal("InjuriesCount");
		for (int i = 0; i < num; i++)
		{
			InjuryType injuryType = (InjuryType)SaveGame.LoadIVal("InjuryType" + i);
			InjuryPlace place = (InjuryPlace)SaveGame.LoadIVal("InjuryPlace" + i);
			InjuryState injuryState = (InjuryState)SaveGame.LoadIVal("InjuryState" + i);
			if (injuryType != InjuryType.WormHole || injuryState != InjuryState.WormInside)
			{
				BIWoundSlot woundSlot = BodyInspectionController.Get().GetWoundSlot(place, SaveGame.LoadSVal("InjurySlot" + i));
				int poison_level = SaveGame.LoadIVal("InjuryPoisonLevel" + i);
				Injury injury = this.AddInjury(injuryType, place, woundSlot, injuryState, poison_level, null);
				if (injuryState == InjuryState.Infected)
				{
					injury.Infect();
				}
				else if (injuryState == InjuryState.Closed)
				{
					injury.CloseWound();
				}
				injury.Load(i);
			}
		}
	}

	public void ResetInjuries()
	{
		int i = 0;
		while (i < this.m_Injuries.Count)
		{
			this.HealInjury(this.m_Injuries[i]);
		}
	}

	public void AddWorm(string place)
	{
		InjuryPlace place2 = (InjuryPlace)Enum.Parse(typeof(InjuryPlace), place);
		BIWoundSlot freeWoundSlot = this.m_BodyInspectionController.GetFreeWoundSlot(place2, InjuryType.Worm, true);
		this.AddInjury(InjuryType.Worm, place2, freeWoundSlot, InjuryState.Open, 0, null);
	}

	public bool HasWorm()
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryType() == InjuryType.Worm)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLeech()
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryType() == InjuryType.Leech)
			{
				return true;
			}
		}
		return false;
	}

	public void OnEat(ConsumableInfo info)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			this.m_Injuries[i].PoisonDebuff(info.m_PoisonDebuff);
		}
	}

	public void OnDrink(LiquidData data)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			this.m_Injuries[i].PoisonDebuff(data.m_PoisonDebuff);
		}
	}

	public void SleptOnGround()
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			this.m_Injuries[i].m_TimeToInfect = 0f;
		}
	}

	public bool ScenarioHasInjury(string injury_type)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryType().ToString() == injury_type)
			{
				return true;
			}
		}
		return false;
	}

	public bool ScenarioHasInjuryAt(string limb)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryPlace().ToString() == limb)
			{
				return true;
			}
		}
		return false;
	}

	public bool ScenarioHasBandageOnInjuryAt(string limb)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].GetInjuryPlace().ToString() == limb)
			{
				return this.m_Injuries[i].m_Bandage != null;
			}
		}
		return false;
	}

	public int GetPosionLevel()
	{
		int num = 0;
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].IsWound() && (this.m_Injuries[i].m_Type == InjuryType.SnakeBite || this.m_Injuries[i].m_Type == InjuryType.VenomBite))
			{
				num += this.m_Injuries[i].m_PoisonLevel;
			}
		}
		return num;
	}

	public int GetNumWoundsOfState(InjuryState state)
	{
		int num = 0;
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].IsWound() && this.m_Injuries[i].m_State == state)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumWoundsOfType(InjuryType type)
	{
		int num = 0;
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].IsWound() && this.m_Injuries[i].m_Type == type)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumWounds()
	{
		int num = 0;
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].IsWound())
			{
				num++;
			}
		}
		return num;
	}

	public bool AllWoundsHealing()
	{
		if (this.GetNumWounds() == 0)
		{
			return false;
		}
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].IsWound() && !this.m_Injuries[i].IsHealing())
			{
				return false;
			}
		}
		return true;
	}

	public int GetNumHealingWounds()
	{
		int num = 0;
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			Injury injury = this.m_Injuries[i];
			if (injury.IsWound() && injury.IsHealing())
			{
				num++;
			}
		}
		return num;
	}

	public void UnlockKnownInjury(InjuryType injury_type)
	{
		if (injury_type == InjuryType.WormHole)
		{
			return;
		}
		if (!this.m_KnownInjuries.Contains(injury_type))
		{
			this.m_KnownInjuries.Add(injury_type);
			HUDInfoLog hudinfoLog = (HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog));
			string title = GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewEntry");
			string text = GreenHellGame.Instance.GetLocalization().Get(injury_type.ToString());
			hudinfoLog.AddInfo(title, text);
		}
	}

	public void UnlockKnownInjuryFromScenario(string injury_type_name)
	{
		InjuryType injury_type = (InjuryType)Enum.Parse(typeof(InjuryType), injury_type_name);
		this.UnlockKnownInjury(injury_type);
	}

	public void UnlockKnownInjuryTreatment(NotepadKnownInjuryTreatment injury_treatment)
	{
		if (!this.m_KnownInjuryTreatments.Contains(injury_treatment))
		{
			this.m_KnownInjuryTreatments.Add(injury_treatment);
		}
	}

	public void UnlockKnownInjuryTreatmentFromScenario(string injury_type_name)
	{
		NotepadKnownInjuryTreatment injury_treatment = (NotepadKnownInjuryTreatment)Enum.Parse(typeof(NotepadKnownInjuryTreatment), injury_type_name);
		this.UnlockKnownInjuryTreatment(injury_treatment);
	}

	public bool IsInjuryUnlocked(InjuryType injury_type)
	{
		return this.m_KnownInjuries.Contains(injury_type);
	}

	public void UnlockAllKnownInjuries()
	{
		Array values = Enum.GetValues(typeof(InjuryType));
		for (int i = 0; i < values.Length; i++)
		{
			InjuryType item = (InjuryType)i;
			if (!this.m_KnownInjuries.Contains(item))
			{
				this.m_KnownInjuries.Add(item);
			}
		}
	}

	public bool IsInjuryTreatmentUnlocked(NotepadKnownInjuryTreatment injury_treatment)
	{
		return this.m_KnownInjuryTreatments.Contains(injury_treatment);
	}

	public void UnlockKnownInjuryState(InjuryState injury_state)
	{
		if (!this.m_KnownInjuryState.Contains(injury_state))
		{
			this.m_KnownInjuryState.Add(injury_state);
			HUDInfoLog hudinfoLog = (HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog));
			string title = GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewEntry");
			string text = GreenHellGame.Instance.GetLocalization().Get(injury_state.ToString());
			hudinfoLog.AddInfo(title, text);
		}
	}

	public void UnlockKnownInjuryStateFromScenario(string injury_state_name)
	{
		InjuryState injury_state = (InjuryState)Enum.Parse(typeof(InjuryState), injury_state_name);
		this.UnlockKnownInjuryState(injury_state);
	}

	public void UnlockKnownInjuryStateTreatment(NotepadKnownInjuryStateTreatment injury_state_treatment)
	{
		if (!this.m_KnownInjuryStateTreatments.Contains(injury_state_treatment))
		{
			this.m_KnownInjuryStateTreatments.Add(injury_state_treatment);
		}
	}

	public void UnlockKnownInjuryStateTreatmentFromScenario(string injury_state_name)
	{
		NotepadKnownInjuryStateTreatment injury_state_treatment = (NotepadKnownInjuryStateTreatment)Enum.Parse(typeof(NotepadKnownInjuryStateTreatment), injury_state_name);
		this.UnlockKnownInjuryStateTreatment(injury_state_treatment);
	}

	public bool IsInjuryStateUnlocked(InjuryState injury_state)
	{
		return this.m_KnownInjuryState.Contains(injury_state);
	}

	public bool IsInjuryStateTreatmentUnlocked(NotepadKnownInjuryStateTreatment injury_state_treatment)
	{
		return this.m_KnownInjuryStateTreatments.Contains(injury_state_treatment);
	}

	public void UnlockAllInjuryState()
	{
		Array values = Enum.GetValues(typeof(InjuryState));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownInjuryState((InjuryState)values.GetValue(i));
		}
	}

	public void UnlockAllInjuryStateTreatment()
	{
		Array values = Enum.GetValues(typeof(NotepadKnownInjuryStateTreatment));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownInjuryStateTreatment((NotepadKnownInjuryStateTreatment)values.GetValue(i));
		}
	}

	private void OnAddInjury(InjuryType type)
	{
		this.UnlockKnownInjury(type);
	}

	public void OpenChildrenInjuries(Injury injury)
	{
		for (int i = 0; i < this.m_Injuries.Count; i++)
		{
			if (this.m_Injuries[i].m_ParentInjury == injury)
			{
				this.m_Injuries[i].OpenWound();
			}
		}
	}

	[HideInInspector]
	public List<InjuryType> m_KnownInjuries = new List<InjuryType>();

	[HideInInspector]
	public List<NotepadKnownInjuryTreatment> m_KnownInjuryTreatments = new List<NotepadKnownInjuryTreatment>();

	[HideInInspector]
	public List<InjuryState> m_KnownInjuryState = new List<InjuryState>();

	[HideInInspector]
	public List<NotepadKnownInjuryStateTreatment> m_KnownInjuryStateTreatments = new List<NotepadKnownInjuryStateTreatment>();

	public List<Injury> m_Injuries = new List<Injury>();

	private Dictionary<int, InjuryTreatment> m_Treatments = new Dictionary<int, InjuryTreatment>();

	private PlayerConditionModule m_ConditionModule;

	private BodyInspectionController m_BodyInspectionController;

	private static PlayerInjuryModule s_Instance;

	[HideInInspector]
	public Dictionary<int, GameObject> m_WoundSlotsToWoundGameObjectMap = new Dictionary<int, GameObject>();

	private Material m_WoundMaterial;

	private float m_LeechNextTime2 = 610f;

	private bool m_LeechNextTimeInitialized;

	private DamageInfo m_DamageInfo = new DamageInfo();

	private Dictionary<int, int> m_SanityDictionary = new Dictionary<int, int>(20);

	private float m_LeechChanceOutsideOfWater = 0.5f;

	private float m_LeechChanceInsideWater = 0.5f;

	private List<Injury> m_InjuriesTempList = new List<Injury>();

	private List<Injury> m_InjuryStateTempList = new List<Injury>();
}
