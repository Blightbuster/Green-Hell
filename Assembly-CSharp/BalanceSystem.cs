using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BalanceSystem : MonoBehaviour, ISaveLoad
{
	public static BalanceSystem Get()
	{
		return BalanceSystem.s_Instance;
	}

	private void Start()
	{
		BalanceSystem.s_Instance = this;
		this.Initialize();
		GameDifficulty gameDifficulty = GreenHellGame.Instance.m_GameDifficulty;
		if (gameDifficulty != GameDifficulty.Easy)
		{
			if (gameDifficulty != GameDifficulty.Normal)
			{
				if (gameDifficulty == GameDifficulty.Hard)
				{
					this.m_MinHumanAISpawnCount = 1;
					this.m_MaxHumanAISpawnCount = 3;
				}
			}
			else
			{
				this.m_MinHumanAISpawnCount = 1;
				this.m_MaxHumanAISpawnCount = 3;
			}
		}
		else
		{
			this.m_MinHumanAISpawnCount = 1;
			this.m_MaxHumanAISpawnCount = 3;
		}
		this.SetupSpawnCooldown();
		this.SetupCoooldowns();
	}

	private void SetupSpawnCooldown()
	{
		GameDifficulty gameDifficulty = GreenHellGame.Instance.m_GameDifficulty;
		if (gameDifficulty != GameDifficulty.Easy)
		{
			if (gameDifficulty != GameDifficulty.Normal)
			{
				if (gameDifficulty == GameDifficulty.Hard)
				{
					this.m_AISpawnCooldown = 1200f;
				}
			}
			else
			{
				this.m_AISpawnCooldown = 1200f;
			}
		}
		else
		{
			this.m_AISpawnCooldown = 1200f;
		}
	}

	private void SetupCoooldowns()
	{
		GameDifficulty gameDifficulty = GreenHellGame.Instance.m_GameDifficulty;
		if (gameDifficulty != GameDifficulty.Easy)
		{
			if (gameDifficulty != GameDifficulty.Normal)
			{
				if (gameDifficulty == GameDifficulty.Hard)
				{
					this.m_TimeToNextSpawnHumanAIGroup = 60f;
					this.m_TimeToNextSpawnHumanAIWave = 1200f;
					this.m_TimeToNextSpawnJaguar = 300f;
				}
			}
			else
			{
				this.m_TimeToNextSpawnHumanAIGroup = 600f;
				this.m_TimeToNextSpawnHumanAIWave = 2400f;
				this.m_TimeToNextSpawnJaguar = 900f;
			}
		}
		else
		{
			this.m_TimeToNextSpawnHumanAIGroup = 1200f;
			this.m_TimeToNextSpawnHumanAIWave = 2400f;
			this.m_TimeToNextSpawnJaguar = 2100f;
		}
	}

	public void UnblockHumanAISpawn(bool reset_colldown)
	{
		this.m_HumanAISpawnBlocked = false;
		if (reset_colldown)
		{
			this.m_TimeToNextIncreaseHumanAISpawnCount = this.m_IncreaseHumanAISpawnCountInterval;
			this.SetupCoooldowns();
		}
	}

	private void Initialize()
	{
		this.ParseScript();
		this.ParseParametersScript();
		this.m_CurrentHumanAISpawnCount = this.m_MinHumanAISpawnCount;
		this.m_TimeToNextIncreaseHumanAISpawnCount = this.m_IncreaseHumanAISpawnCountInterval;
		this.m_TimeToNextSpawnJaguar = this.m_JaguarCooldown;
		this.InitializePlayerTrigger();
		foreach (string key in this.m_Groups.Keys)
		{
			this.m_ObjectsInArea[key] = new List<BalanceSystemObject>();
		}
		this.InitializeQuadTree();
		this.m_PlayerConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		this.m_PlayerInjuryModule = Player.Get().GetComponent<PlayerInjuryModule>();
		this.m_PlayerDiseasesModule = Player.Get().GetComponent<PlayerDiseasesModule>();
		this.m_PlayerSanityModule = Player.Get().GetComponent<PlayerSanityModule>();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Balance/Balance.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Prefab")
			{
				BSItemData bsitemData = new BSItemData();
				bsitemData.m_PrefabName = key.GetVariable(0).SValue;
				bsitemData.m_Prefab = GreenHellGame.Instance.GetPrefab(bsitemData.m_PrefabName);
				Item component = bsitemData.m_Prefab.GetComponent<Item>();
				DebugUtils.Assert(component != null, true);
				string[] array = key.GetVariable(2).SValue.Split(new char[]
				{
					';'
				});
				foreach (string key2 in array)
				{
					if (!this.m_Groups.ContainsKey(key2))
					{
						this.m_Groups.Add(key2, 3);
					}
					if (key.GetVariable(1).SValue == "Spawn")
					{
						if (!this.m_SpawnData.ContainsKey(key2))
						{
							this.m_SpawnData.Add(key2, new List<BSItemData>());
						}
						this.m_SpawnData[key2].Add(bsitemData);
					}
					else if (key.GetVariable(1).SValue == "Attachment")
					{
						if (!this.m_AttachmentData.ContainsKey(key2))
						{
							this.m_AttachmentData.Add(key2, new List<BSItemData>());
						}
						this.m_AttachmentData[key2].Add(bsitemData);
					}
				}
				for (int k = 0; k < key.GetKeysCount(); k++)
				{
					Key key3 = key.GetKey(k);
					if (key3.GetName() == "ItemID")
					{
						bsitemData.m_ItemID = (ItemID)Enum.Parse(typeof(ItemID), key3.GetVariable(0).SValue);
					}
					else if (key3.GetName() == "Chance")
					{
						bsitemData.m_Chance = key3.GetVariable(0).FValue;
					}
					else if (key3.GetName() == "Condition")
					{
						bsitemData.m_Condition = (BSCondition)Enum.Parse(typeof(BSCondition), key3.GetVariable(0).SValue);
					}
					else if (key3.GetName() == "ConditionValue")
					{
						bsitemData.m_ConditionValue = key3.GetVariable(0).FValue;
					}
					else if (key3.GetName() == "ConditionChance")
					{
						bsitemData.m_ConditionChance = key3.GetVariable(0).FValue;
					}
					else if (key3.GetName() == "Cooldown")
					{
						bsitemData.m_Cooldown = key3.GetVariable(0).FValue;
					}
					else if (key3.GetName() == "CooldownChance")
					{
						bsitemData.m_CooldownChance = key3.GetVariable(0).FValue;
					}
				}
			}
		}
	}

	private void ParseParametersScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Balance/BalanceParameters.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "HPWeight")
			{
				this.m_HPWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HydrationWeight")
			{
				this.m_HydrationWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "EnergyWeight")
			{
				this.m_EnergyWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "ProteinsWeight")
			{
				this.m_ProteinsWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FatWeight")
			{
				this.m_FatWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "CarbsWeight")
			{
				this.m_CarbsWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HaveItemWeight")
			{
				this.m_HaveItemWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HaveFireWeight")
			{
				this.m_HaveFireWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "SanityWeight")
			{
				this.m_SanityWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "InfectedWoundWeight")
			{
				this.m_InfectedWoundWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "SnakeBiteLvlWeight")
			{
				this.m_SnakeBiteLvlWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FoodPoisonLvlWeight")
			{
				this.m_FoodPoisonLvlWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BleedingWeight")
			{
				this.m_BleedingWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "CutWoundWeight")
			{
				this.m_CutWoundWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "InsectWoundWeight")
			{
				this.m_InsectWoundWeight = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MaxNumGroupPriority")
			{
				if (!this.m_Groups.ContainsKey(key.GetVariable(0).SValue))
				{
					DebugUtils.Assert("Can't find group - " + key.GetVariable(0).SValue, true, DebugUtils.AssertType.Info);
				}
				else
				{
					this.m_Groups[key.GetVariable(0).SValue] = key.GetVariable(1).IValue;
				}
			}
			else if (key.GetName() == "BalanceSpawnerCooldown")
			{
				BalanceSystem.s_BalanceSpawnerCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BalanceSpawnerCheckCooldown")
			{
				BalanceSystem.s_BalanceSpawnerCheckCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HumanAIStaticCooldown")
			{
				this.m_HumanAIStaticCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "HumanAIWaveCooldown")
			{
				this.m_HumanAIWaveCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "MinHumanAISpawnCount")
			{
				this.m_MinHumanAISpawnCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "MaxHumanAISpawnCount")
			{
				this.m_MaxHumanAISpawnCount = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "IncreaseHumanAISpawnCountInterval")
			{
				this.m_IncreaseHumanAISpawnCountInterval = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "JaguarCooldown")
			{
				this.m_JaguarCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BSConditionCooldown")
			{
				BSCondition key2 = (BSCondition)Enum.Parse(typeof(BSCondition), key.GetVariable(0).SValue);
				float fvalue = key.GetVariable(1).FValue;
				this.m_BSConditionCooldown.Add((int)key2, fvalue);
				this.m_BSConditionNextTime.Add((int)key2, float.MinValue);
			}
		}
	}

	private void InitializePlayerTrigger()
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab("BSPlayerTrigger");
		if (prefab == null)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
		if (gameObject == null)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		Player player = Player.Get();
		gameObject.transform.position = player.transform.position;
		gameObject.transform.rotation = player.transform.rotation;
		gameObject.transform.parent = player.transform;
		gameObject.GetComponent<BSPlayerTrigger>().SetBalanceSystem(this);
	}

	private void InitializeQuadTree()
	{
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		Bounds bounds = default(Bounds);
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			if (i == 0)
			{
				Vector3 position = terrain.GetPosition();
				bounds.max = position;
				bounds.min = position;
			}
			else
			{
				bounds.Encapsulate(terrain.GetPosition());
			}
			if (!(terrain.terrainData == null))
			{
				bounds.Encapsulate(terrain.terrainData.size);
			}
		}
		this.m_QuadTree = new QuadTreeBalanceSystem(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
	}

	public void OnObjectTriggerEnter(GameObject obj)
	{
		BalanceSpawner component = obj.GetComponent<BalanceSpawner>();
		if (component == null)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		if (component.IsAttachmentSpawner())
		{
			this.OnBalanceAttachmentSpawnerEnter(obj, obj.GetComponent<BalanceAttachmentSpawner>());
		}
		else
		{
			this.OnBalanceSpawnerEnter(obj, component);
		}
	}

	public void OnBalanceSpawnerEnter(GameObject obj, BalanceSpawner bs)
	{
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(obj.transform.position);
		if (objectInPos == null)
		{
			if (bs != null && Time.time - bs.m_LastSpawnObjectTime < BalanceSystem.s_BalanceSpawnerCooldown)
			{
				return;
			}
			string empty = string.Empty;
			BSItemData objectToSpawn = this.GetObjectToSpawn(ref empty);
			GameObject gameObject = null;
			if (objectToSpawn != null)
			{
				gameObject = objectToSpawn.m_Prefab;
			}
			if (gameObject != null)
			{
				objectToSpawn.m_LastSpawnTime = Time.time;
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, obj.transform.position, obj.transform.rotation);
				Item component = gameObject2.GetComponent<Item>();
				if (component)
				{
					component.m_CanSave = false;
				}
				BalanceSystemObject balanceSystemObject = new BalanceSystemObject();
				balanceSystemObject.m_GameObject = gameObject2;
				balanceSystemObject.m_Group = empty;
				balanceSystemObject.m_BalanceSpawner = bs.gameObject;
				this.m_QuadTree.InsertObject(balanceSystemObject);
				this.m_ObjectsInArea[empty].Add(balanceSystemObject);
				if (empty == "Sanity")
				{
					CJObject[] componentsInChildren = gameObject2.GetComponentsInChildren<CJObject>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].m_Hallucination = true;
					}
				}
				else
				{
					bs.m_LastSpawnObjectTime = Time.time;
				}
				bs.m_LastCheckSpawnTime = Time.time;
			}
		}
		else
		{
			this.m_ObjectsInArea[objectInPos.m_Group].Add(objectInPos);
		}
	}

	public void OnBalanceAttachmentSpawnerEnter(GameObject obj, BalanceAttachmentSpawner bs)
	{
		if (Time.time - bs.m_LastSpawnObjectTime < BalanceSystem.s_BalanceSpawnerCooldown)
		{
			return;
		}
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(obj.transform.position);
		if (objectInPos != null)
		{
			if (objectInPos.m_GameObject == null)
			{
				if (bs.m_StaticSystem)
				{
					if (!objectInPos.m_AllChildrenDestroyed)
					{
						Item item = bs.Attach(objectInPos.m_ItemID, objectInPos.m_ChildNum, objectInPos.m_ActiveChildrenMask);
						objectInPos.m_GameObject = item.gameObject;
						objectInPos.m_BalanceSpawner = bs.gameObject;
						this.m_ObjectsInArea[objectInPos.m_Group].Add(objectInPos);
					}
					else if (Time.time - objectInPos.m_LastSpawnObjectTime > BalanceSystem.s_BalanceSpawnerCooldown)
					{
						this.TryToAttach(bs, objectInPos);
					}
				}
				else
				{
					this.TryToAttach(bs, objectInPos);
				}
			}
		}
		else
		{
			this.TryToAttach(bs, null);
		}
	}

	private void TryToAttach(BalanceAttachmentSpawner bs, BalanceSystemObject bso)
	{
		string empty = string.Empty;
		BSItemData objectToAttach = this.GetObjectToAttach(ref empty, bs.m_ItemIDNamesList);
		if (objectToAttach != null)
		{
			int childNum = -1;
			GameObject gameObject = bs.TryToAttach(objectToAttach.m_ItemID, out childNum);
			if (gameObject != null)
			{
				objectToAttach.m_LastSpawnTime = Time.time;
				BalanceSystemObject balanceSystemObject = (bso != null) ? bso : new BalanceSystemObject();
				balanceSystemObject.m_Group = empty;
				balanceSystemObject.m_ChildNum = childNum;
				balanceSystemObject.m_ItemID = objectToAttach.m_ItemID;
				balanceSystemObject.m_BalanceSpawner = bs.gameObject;
				balanceSystemObject.m_GameObject = gameObject;
				balanceSystemObject.m_Position = bs.transform.position;
				balanceSystemObject.m_AllChildrenDestroyed = false;
				this.m_ObjectsInArea[empty].Add(balanceSystemObject);
				this.m_QuadTree.InsertObject(balanceSystemObject);
				if (empty == "Sanity")
				{
					CJObject[] componentsInChildren = gameObject.GetComponentsInChildren<CJObject>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].m_Hallucination = true;
					}
				}
				else
				{
					balanceSystemObject.m_LastSpawnObjectTime = Time.time;
				}
			}
		}
	}

	public void OnObjectTriggerExit(GameObject obj)
	{
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(obj.transform.position);
		this.RemoveObjectFromArea(obj);
	}

	private void RemoveObjectFromArea(GameObject obj)
	{
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(obj.transform.position);
		if (objectInPos != null)
		{
			foreach (string key in this.m_Groups.Keys)
			{
				this.m_ObjectsInArea[key].Remove(objectInPos);
			}
		}
	}

	private BSItemData GetObjectToSpawn(ref string group)
	{
		group = string.Empty;
		if (PlayerSanityModule.Get().m_ItemHallucinationsEnabled && this.m_ObjectsInArea["Sanity"].Count < PlayerSanityModule.Get().GetWantedItemsHallucinationsCount())
		{
			group = "Sanity";
			return this.GetRandomObject(this.m_SpawnData[group]);
		}
		foreach (string text in this.m_Groups.Keys)
		{
			if (!(text == "Sanity"))
			{
				if (this.m_ObjectsInArea[text].Count < this.m_Groups[text])
				{
					group = text;
					return this.GetRandomObject(this.m_SpawnData[group]);
				}
			}
		}
		return null;
	}

	private BSItemData GetObjectToAttach(ref string group, List<string> item_id_names)
	{
		group = string.Empty;
		if (PlayerSanityModule.Get().m_ItemHallucinationsEnabled && this.m_ObjectsInArea["Sanity"].Count < PlayerSanityModule.Get().GetWantedItemsHallucinationsCount() && this.m_AttachmentData.ContainsKey("Sanity"))
		{
			group = "Sanity";
			BSItemData randomObject = this.GetRandomObject(this.m_AttachmentData[group]);
			if (randomObject != null && item_id_names.Contains(randomObject.m_ItemID.ToString()))
			{
				return randomObject;
			}
		}
		foreach (string text in this.m_Groups.Keys)
		{
			if (!(text == "Sanity"))
			{
				if (this.m_ObjectsInArea[text].Count < this.m_Groups[text])
				{
					group = text;
					BSItemData randomObject2 = this.GetRandomObject(this.m_AttachmentData[group]);
					if (randomObject2 != null && item_id_names.Contains(randomObject2.m_ItemID.ToString()))
					{
						return randomObject2;
					}
				}
			}
		}
		return null;
	}

	private BSItemData GetRandomObject(List<BSItemData> list)
	{
		if (list.Count == 0)
		{
			return null;
		}
		this.m_TempList.Clear();
		this.m_TempConditionList.Clear();
		float time = Time.time;
		for (int i = 0; i < list.Count; i++)
		{
			BSItemData bsitemData = list[i];
			float num = 0f;
			if (time - bsitemData.m_LastSpawnTime > bsitemData.m_Cooldown)
			{
				num += bsitemData.m_Chance;
			}
			else
			{
				num += bsitemData.m_CooldownChance;
			}
			float num2 = 0f;
			bool flag = this.m_BSConditionNextTime.TryGetValue((int)bsitemData.m_Condition, out num2);
			if (this.IsBSCondition(bsitemData.m_Condition, bsitemData.m_ConditionValue) && flag && Time.time > num2)
			{
				num += bsitemData.m_ConditionChance;
				this.m_TempConditionList.Add((int)bsitemData.m_Condition);
			}
			if (UnityEngine.Random.Range(0f, 1f) < num)
			{
				this.m_TempList.Add(bsitemData);
			}
		}
		if (this.m_TempList.Count > 0)
		{
			BSItemData bsitemData2 = this.m_TempList[UnityEngine.Random.Range(0, this.m_TempList.Count)];
			if (this.m_TempConditionList.Contains((int)bsitemData2.m_Condition))
			{
				this.SetBSConditionCooldown((int)bsitemData2.m_Condition);
			}
			return bsitemData2;
		}
		return null;
	}

	private void SetBSConditionCooldown(int bs_cond)
	{
		float num = 0f;
		if (this.m_BSConditionCooldown.TryGetValue(bs_cond, out num))
		{
			float num2 = 0f;
			if (this.m_BSConditionNextTime.TryGetValue(bs_cond, out num2))
			{
				this.m_BSConditionNextTime[bs_cond] = Time.time + num;
			}
		}
	}

	private void Update()
	{
		if (Debug.isDebugBuild)
		{
			this.UpdateInputs();
		}
		this.UpdateParameters();
		this.UpdateHumanAICooldown();
		this.UpdateHumanAISpawnCount();
		this.UpdateJaguarCooldown();
	}

	private void UpdateParameters()
	{
		float num = this.m_PlayerConditionModule.GetHP() / this.m_PlayerConditionModule.GetMaxHP();
		float num2 = this.m_PlayerConditionModule.GetHydration() / this.m_PlayerConditionModule.GetMaxHydration();
		float num3 = this.m_PlayerConditionModule.GetEnergy() / this.m_PlayerConditionModule.GetMaxEnergy();
		float num4 = this.m_PlayerConditionModule.GetNutritionProtein() / this.m_PlayerConditionModule.GetMaxNutritionProtein();
		float num5 = this.m_PlayerConditionModule.GetNutritionFat() / this.m_PlayerConditionModule.GetMaxNutritionFat();
		float num6 = this.m_PlayerConditionModule.GetMaxNutritionCarbo() / this.m_PlayerConditionModule.GetMaxNutritionCarbo();
		float num7 = (float)InventoryBackpack.Get().GetFoodItemsCount();
		float num8 = (!Firecamp.IsAnyBurning()) ? 0f : 1f;
		float num9 = (float)PlayerSanityModule.Get().m_Sanity / 100f;
		float num10 = (float)this.m_PlayerInjuryModule.GetNumWoundsOfState(InjuryState.Infected);
		float num11 = (float)this.m_PlayerInjuryModule.GetPosionLevel();
		float num12 = (float)this.m_PlayerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).m_Level;
		float num13 = (float)this.m_PlayerInjuryModule.GetNumWoundsOfState(InjuryState.Bleeding);
		float num14 = (float)this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.Laceration) + (float)this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.LacerationCat);
		float num15 = (float)this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.Rash);
		this.m_WeightedAverage = (num * this.m_HPWeight + num2 * this.m_HydrationWeight + num3 * this.m_EnergyWeight + num4 * this.m_ProteinsWeight + num5 * this.m_FatWeight + num6 * this.m_CarbsWeight + num7 * this.m_HaveItemWeight + num8 * this.m_HaveFireWeight + num9 * this.m_SanityWeight - (num10 * this.m_InfectedWoundWeight + num11 * this.m_SnakeBiteLvlWeight + num12 * this.m_FoodPoisonLvlWeight + num13 * this.m_BleedingWeight + num14 * this.m_CutWoundWeight + num15 * this.m_InsectWoundWeight)) / (this.m_HPWeight + this.m_HydrationWeight + this.m_EnergyWeight + this.m_ProteinsWeight + this.m_FatWeight + this.m_CarbsWeight + this.m_HaveItemWeight + this.m_HaveFireWeight + this.m_SanityWeight + (this.m_InfectedWoundWeight + this.m_SnakeBiteLvlWeight + this.m_FoodPoisonLvlWeight + this.m_BleedingWeight + this.m_CutWoundWeight + this.m_InsectWoundWeight));
	}

	private bool IsBSCondition(BSCondition cond, float val)
	{
		if (cond == BSCondition.Fat)
		{
			return val > this.m_PlayerConditionModule.GetNutritionFat() / this.m_PlayerConditionModule.GetMaxNutritionFat();
		}
		if (cond == BSCondition.Carbo)
		{
			return val > this.m_PlayerConditionModule.GetNutritionCarbo() / this.m_PlayerConditionModule.GetMaxNutritionCarbo();
		}
		if (cond == BSCondition.Proteins)
		{
			return val > this.m_PlayerConditionModule.GetNutritionProtein() / this.m_PlayerConditionModule.GetMaxNutritionProtein();
		}
		if (cond == BSCondition.Infected)
		{
			return this.m_PlayerInjuryModule.GetNumWoundsOfState(InjuryState.Infected) > 0;
		}
		if (cond == BSCondition.Bleeding)
		{
			return this.m_PlayerInjuryModule.GetNumWoundsOfState(InjuryState.Bleeding) > 0;
		}
		if (cond == BSCondition.Cut)
		{
			return this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.Laceration) > 0 || this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.LacerationCat) > 0;
		}
		if (cond == BSCondition.FoodPoison)
		{
			return this.m_PlayerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).m_Level > 0;
		}
		if (cond == BSCondition.Sanity)
		{
			return val > (float)this.m_PlayerSanityModule.m_Sanity;
		}
		if (cond == BSCondition.Poison)
		{
			return this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.SnakeBite) > 0 || this.m_PlayerInjuryModule.GetNumWoundsOfType(InjuryType.VenomBite) > 0;
		}
		if (cond == BSCondition.Hydration)
		{
			return val > this.m_PlayerConditionModule.GetHydration() / this.m_PlayerConditionModule.GetMaxHydration();
		}
		if (cond == BSCondition.Fever)
		{
			return this.m_PlayerDiseasesModule.GetDisease(ConsumeEffect.Fever).m_Level > 0;
		}
		if (cond == BSCondition.HasFire)
		{
			ItemsManager.Get().IsAnyFirecampBurning();
		}
		return false;
	}

	public void OnItemDestroyed(Item item)
	{
		GameObject gameObject = item.gameObject;
		foreach (string group in this.m_Groups.Keys)
		{
			this.ItemDestroyed(group, gameObject);
		}
	}

	private void ItemDestroyed(string group, GameObject go)
	{
		int i = 0;
		while (i < this.m_ObjectsInArea[group].Count)
		{
			BalanceSystemObject balanceSystemObject = this.m_ObjectsInArea[group][i];
			GameObject gameObject = balanceSystemObject.m_GameObject;
			if (gameObject == go)
			{
				if (balanceSystemObject.m_BalanceSpawner != null)
				{
					BalanceSpawner component = balanceSystemObject.m_BalanceSpawner.GetComponent<BalanceSpawner>();
					bool flag = component.IsAttachmentSpawner();
					if (flag)
					{
						DestroyIfNoChildren component2 = gameObject.GetComponent<DestroyIfNoChildren>();
						if (component2 == null)
						{
							DebugUtils.Assert(DebugUtils.AssertType.Info);
						}
						if (component2.m_NumChildren == 0)
						{
							balanceSystemObject.m_AllChildrenDestroyed = true;
						}
						if (component2.CheckNoChildren())
						{
							balanceSystemObject.m_ActiveChildrenMask = -1;
						}
						else
						{
							this.SetupActiveChildrenMask(gameObject, out balanceSystemObject.m_ActiveChildrenMask);
						}
						this.m_ObjectsInArea[group].Remove(balanceSystemObject);
					}
					else
					{
						Item item = null;
						this.m_TempItemList.Clear();
						go.GetComponents<Item>(this.m_TempItemList);
						if (this.m_TempList.Count > 0)
						{
							item = this.m_TempItemList[0];
						}
						if (item && item.m_DestroyingOnlyScript)
						{
							i++;
						}
						else
						{
							this.m_QuadTree.RemoveObject(balanceSystemObject);
							this.m_ObjectsInArea[group].Remove(balanceSystemObject);
						}
					}
				}
				else
				{
					i++;
				}
			}
			else
			{
				i++;
			}
		}
	}

	private void SetupActiveChildrenMask(GameObject obj, out int mask)
	{
		int num = 0;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			if (obj.transform.GetChild(i).gameObject.activeSelf)
			{
				num |= 1 << i;
			}
		}
		mask = num;
	}

	private void UpdateInputs()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			this.m_Debug = !this.m_Debug;
		}
	}

	public bool CanSpawnHumanAIGroup()
	{
		return !this.m_HumanAISpawnBlocked && this.m_TimeToNextSpawnHumanAIGroup <= 0f;
	}

	public bool CanSpawnHumanAIWave()
	{
		return !this.m_HumanAISpawnBlocked && this.m_TimeToNextSpawnHumanAIWave <= 0f;
	}

	private void UpdateHumanAICooldown()
	{
		float num = 1f;
		float deltaTime = Time.deltaTime;
		this.m_TimeToNextSpawnHumanAIGroup -= deltaTime * num;
		this.m_TimeToNextSpawnHumanAIWave -= deltaTime * num;
	}

	private void UpdateHumanAISpawnCount()
	{
		if (MainLevel.Instance.m_Tutorial)
		{
			return;
		}
		if (this.m_CurrentHumanAISpawnCount == this.m_MaxHumanAISpawnCount)
		{
			return;
		}
		this.m_TimeToNextIncreaseHumanAISpawnCount -= Time.deltaTime;
		if (this.m_TimeToNextIncreaseHumanAISpawnCount <= 0f)
		{
			this.m_CurrentHumanAISpawnCount++;
			this.m_TimeToNextIncreaseHumanAISpawnCount = this.m_IncreaseHumanAISpawnCountInterval;
		}
	}

	public int GetCurrentHumanAISpawnCount()
	{
		int num = this.m_MaxHumanAISpawnCount;
		if (this.m_WeightedAverage < 0.4f)
		{
			num -= 2;
		}
		else if (this.m_WeightedAverage < 0.8f)
		{
			num--;
		}
		return Mathf.Min(this.m_CurrentHumanAISpawnCount, num);
	}

	private void UpdateJaguarCooldown()
	{
		if (MainLevel.Instance.m_Tutorial)
		{
			return;
		}
		float num = 1f;
		float deltaTime = Time.deltaTime;
		this.m_TimeToNextSpawnJaguar -= deltaTime * num;
	}

	public bool CanSpawnJaguar()
	{
		return this.m_TimeToNextSpawnJaguar <= 0f;
	}

	public void OnJaguarActivated()
	{
		this.m_TimeToNextSpawnJaguar = this.m_JaguarCooldown;
	}

	public void BlockHumanAISpawn()
	{
		this.m_HumanAISpawnBlocked = true;
	}

	public void OnHumanAIGroupActivated()
	{
		if (this.m_LastHumanAIGroupKilled)
		{
			this.m_TimeToNextSpawnHumanAIGroup = this.m_AISpawnCooldown;
		}
		else
		{
			this.m_TimeToNextSpawnHumanAIGroup = this.m_AISpawnCooldown * 0.5f;
		}
	}

	public void OnHumanAIWaveActivated()
	{
		if (this.m_LastHumanAIGroupKilled)
		{
			this.m_TimeToNextSpawnHumanAIWave = this.m_AISpawnCooldown;
		}
		else
		{
			this.m_TimeToNextSpawnHumanAIWave = this.m_AISpawnCooldown * 0.5f;
		}
	}

	public void OnHumanAIGroupDeactivated(bool killed)
	{
		this.m_LastHumanAIGroupKilled = killed;
		if (this.m_LastHumanAIGroupKilled)
		{
			this.m_TimeToNextSpawnHumanAIGroup = this.m_AISpawnCooldown;
			this.m_TimeToNextSpawnHumanAIWave = this.m_AISpawnCooldown;
		}
		else
		{
			this.m_TimeToNextSpawnHumanAIGroup = Mathf.Min(this.m_TimeToNextSpawnHumanAIGroup, this.m_AISpawnCooldown * 0.5f);
		}
	}

	public void OnHumanAIWaveDeactivated(bool killed)
	{
		this.m_LastHumanAIGroupKilled = killed;
		this.m_TimeToNextSpawnHumanAIWave = ((!killed) ? Mathf.Min(this.m_TimeToNextSpawnHumanAIWave, this.m_AISpawnCooldown * 0.5f) : this.m_AISpawnCooldown);
	}

	public void Save()
	{
		SaveGame.SaveVal("TimeToNextSpawnHumanAIStatic", this.m_TimeToNextSpawnHumanAIGroup);
		SaveGame.SaveVal("TimeToNextSpawnHumanAIWave", this.m_TimeToNextSpawnHumanAIWave);
		SaveGame.SaveVal("CurrentHumanAISpawnCount", this.m_CurrentHumanAISpawnCount);
		SaveGame.SaveVal("TimeToNextIncreaseHumanAISpawnCount", this.m_TimeToNextIncreaseHumanAISpawnCount);
		SaveGame.SaveVal("TimeToNextSpawnJaguar", this.m_TimeToNextSpawnJaguar);
	}

	public void Load()
	{
		SaveGame.LoadVal("TimeToNextSpawnHumanAIStatic", out this.m_TimeToNextSpawnHumanAIGroup, false);
		SaveGame.LoadVal("TimeToNextSpawnHumanAIWave", out this.m_TimeToNextSpawnHumanAIWave, false);
		this.m_CurrentHumanAISpawnCount = SaveGame.LoadIVal("CurrentHumanAISpawnCount");
		this.m_TimeToNextIncreaseHumanAISpawnCount = SaveGame.LoadFVal("TimeToNextIncreaseHumanAISpawnCount");
		this.m_TimeToNextSpawnJaguar = SaveGame.LoadFVal("TimeToNextSpawnJaguar");
		this.SetupSpawnCooldown();
	}

	public void LogCooldowns()
	{
		Debug.Log("m_TimeToNextSpawnHumanAIGroup - " + this.m_TimeToNextSpawnHumanAIGroup);
		Debug.Log("m_TimeToNextSpawnHumanAIWave - " + this.m_TimeToNextSpawnHumanAIWave);
		Debug.Log("m_CurrentHumanAISpawnCount - " + this.m_CurrentHumanAISpawnCount);
		Debug.Log("m_TimeToNextIncreaseHumanAISpawnCount - " + this.m_TimeToNextIncreaseHumanAISpawnCount);
		Debug.Log("m_TimeToNextSpawnJaguar - " + this.m_TimeToNextSpawnJaguar);
		Debug.Log("m_AISpawnCooldown - " + this.m_AISpawnCooldown);
		Debug.Log("m_HumanAISpawnBlocked - " + this.m_HumanAISpawnBlocked.ToString());
	}

	public void OnFullLoadEnd()
	{
		foreach (string key in this.m_Groups.Keys)
		{
			this.m_ObjectsInArea[key] = new List<BalanceSystemObject>();
		}
		this.InitializeQuadTree();
	}

	private Dictionary<string, int> m_Groups = new Dictionary<string, int>();

	private Dictionary<string, List<BSItemData>> m_SpawnData = new Dictionary<string, List<BSItemData>>();

	private Dictionary<string, List<BalanceSystemObject>> m_ObjectsInArea = new Dictionary<string, List<BalanceSystemObject>>();

	private Dictionary<string, List<BSItemData>> m_AttachmentData = new Dictionary<string, List<BSItemData>>();

	private QuadTreeBalanceSystem m_QuadTree;

	private PlayerConditionModule m_PlayerConditionModule;

	private PlayerInjuryModule m_PlayerInjuryModule;

	private PlayerDiseasesModule m_PlayerDiseasesModule;

	private PlayerSanityModule m_PlayerSanityModule;

	private static BalanceSystem s_Instance;

	private bool m_Debug;

	private Dictionary<int, float> m_BSConditionCooldown = new Dictionary<int, float>();

	private Dictionary<int, float> m_BSConditionNextTime = new Dictionary<int, float>();

	private float m_AISpawnCooldown = 1200f;

	private float m_HPWeight = 1f;

	private float m_HydrationWeight = 1f;

	private float m_EnergyWeight = 1f;

	private float m_ProteinsWeight = 1f;

	private float m_FatWeight = 1f;

	private float m_CarbsWeight = 1f;

	private float m_HaveItemWeight = 1f;

	private float m_HaveFireWeight = 1f;

	private float m_SanityWeight = 1f;

	private float m_InfectedWoundWeight = 1f;

	private float m_SnakeBiteLvlWeight = 1f;

	private float m_FoodPoisonLvlWeight = 1f;

	private float m_BleedingWeight = 1f;

	private float m_CutWoundWeight = 1f;

	private float m_InsectWoundWeight = 1f;

	private int m_MinHumanAISpawnCount = 1;

	private int m_MaxHumanAISpawnCount = 3;

	private float m_IncreaseHumanAISpawnCountInterval = 3600f;

	private float m_TimeToNextIncreaseHumanAISpawnCount;

	private int m_CurrentHumanAISpawnCount = 1;

	[HideInInspector]
	public float m_HumanAIStaticCooldown = 600f;

	[HideInInspector]
	public float m_HumanAIWaveCooldown = 1800f;

	private float m_TimeToNextSpawnHumanAIGroup = 600f;

	private float m_TimeToNextSpawnHumanAIWave = 900f;

	private float m_JaguarCooldown = 600f;

	private float m_TimeToNextSpawnJaguar = 600f;

	public static float s_BalanceSpawnerCheckCooldown = 120f;

	public static float s_BalanceSpawnerCooldown = 600f;

	private float m_WeightedAverage = 1f;

	private List<BSItemData> m_TempList = new List<BSItemData>(20);

	private List<int> m_TempConditionList = new List<int>(20);

	private List<Item> m_TempItemList = new List<Item>(10);

	private bool m_LastHumanAIGroupKilled = true;

	private bool m_HumanAISpawnBlocked;
}
