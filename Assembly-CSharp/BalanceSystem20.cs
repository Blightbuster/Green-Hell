using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BalanceSystem20 : ReplicatedBehaviour
{
	public static BalanceSystem20 Get()
	{
		return BalanceSystem20.s_Instance;
	}

	private void Start()
	{
		BalanceSystem20.s_Instance = this;
		this.Initialize();
	}

	public void Initialize()
	{
		this.m_Groups = new Dictionary<string, BalanceSystem20.GroupProps>();
		this.m_SpawnData = new Dictionary<string, List<BSItemData>>();
		this.m_ObjectsInArea = new Dictionary<string, List<BalanceSystemObject>>();
		this.m_QuadTree = null;
		this.ParseScript();
		this.ParseParametersScript();
		this.InitializePlayerTrigger();
		foreach (string key in this.m_Groups.Keys)
		{
			this.m_ObjectsInArea[key] = new List<BalanceSystemObject>();
		}
		this.InitializeQuadTrees();
		this.m_PlayerConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
		this.m_PlayerInjuryModule = Player.Get().GetComponent<PlayerInjuryModule>();
		this.m_PlayerDiseasesModule = Player.Get().GetComponent<PlayerDiseasesModule>();
		this.m_PlayerSanityModule = Player.Get().GetComponent<PlayerSanityModule>();
	}

	private string GetFilePostfix()
	{
		if (ChallengesManager.Get().m_ActiveChallengeName.Length > 0)
		{
			return ChallengesManager.Get().m_ActiveChallengeName;
		}
		if (ChallengesManager.Get().m_ChallengeToActivate.Length > 0)
		{
			return ChallengesManager.Get().m_ChallengeToActivate;
		}
		switch (DifficultySettings.ActivePreset.m_BaseDifficulty)
		{
		case GameDifficulty.Easy:
			return "Easy";
		case GameDifficulty.Normal:
			return "Normal";
		case GameDifficulty.Hard:
			return "Hard";
		default:
			return "Normal";
		}
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		string text = "Balance/Balance_";
		text += this.GetFilePostfix();
		text += ".txt";
		int num = 0;
		scriptParser.Parse(text, true);
		if (scriptParser.GetKeysCount() == 0 && (ChallengesManager.Get().m_ActiveChallengeName.Length > 0 || ChallengesManager.Get().m_ChallengeToActivate.Length > 0))
		{
			text = "Balance/Balance_";
			text += "Hard";
			text += ".txt";
			scriptParser.Parse(text, true);
		}
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Prefab")
			{
				BSItemData bsitemData = new BSItemData();
				bsitemData.m_PrefabName = key.GetVariable(0).SValue;
				bsitemData.m_Prefab = GreenHellGame.Instance.GetPrefab(bsitemData.m_PrefabName);
				DebugUtils.Assert(bsitemData.m_Prefab.GetComponent<Item>() != null, true);
				string[] array = key.GetVariable(2).SValue.Split(new char[]
				{
					';'
				});
				foreach (string text2 in array)
				{
					if (!this.m_Groups.ContainsKey(text2))
					{
						this.m_Groups.Add(text2, new BalanceSystem20.GroupProps(text2, 3, num++));
					}
					if (key.GetVariable(1).SValue == "Spawn")
					{
						if (!this.m_SpawnData.ContainsKey(text2))
						{
							this.m_SpawnData.Add(text2, new List<BSItemData>());
						}
						this.m_SpawnData[text2].Add(bsitemData);
					}
					else if (key.GetVariable(1).SValue == "Attachment")
					{
						if (!this.m_AttachmentData.ContainsKey(text2))
						{
							this.m_AttachmentData.Add(text2, new List<BSItemData>());
						}
						this.m_AttachmentData[text2].Add(bsitemData);
					}
				}
				for (int k = 0; k < key.GetKeysCount(); k++)
				{
					Key key2 = key.GetKey(k);
					if (key2.GetName() == "ItemID")
					{
						bsitemData.m_ItemID = (ItemID)Enum.Parse(typeof(ItemID), key2.GetVariable(0).SValue);
					}
					else if (key2.GetName() == "BaseChance")
					{
						bsitemData.m_BaseChance = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "IncRate")
					{
						bsitemData.m_IncRate = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "Func")
					{
						bsitemData.m_Func = null;
					}
					else if (key2.GetName() == "WalkRange")
					{
						bsitemData.m_WalkRange = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "WalkRangeValue")
					{
						bsitemData.m_WalkRangeValue = key2.GetVariable(0).FValue;
					}
					else if (key2.GetName() == "HaveItem")
					{
						array = key2.GetVariable(0).SValue.Split(new char[]
						{
							';'
						});
						for (int l = 0; l < array.Length; l++)
						{
							bsitemData.m_HaveItemID.Add((int)Enum.Parse(typeof(ItemID), array[l]));
						}
					}
					else if (key2.GetName() == "HaveItemCount")
					{
						bsitemData.m_HaveItemCount = key2.GetVariable(0).IValue;
					}
					else if (key2.GetName() == "HaveItemNegativeEffect")
					{
						bsitemData.m_HaveItemNegativeEffect = key2.GetVariable(0).FValue;
					}
				}
				bsitemData.m_ChanceAccu = bsitemData.m_BaseChance;
				if (bsitemData.m_ItemID == ItemID.None)
				{
					EnumUtils<ItemID>.TryGetValue(bsitemData.m_PrefabName, out bsitemData.m_ItemID);
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
			else if (key.GetName() == "BalanceSpawnerCooldown")
			{
				BalanceSystem20.s_BalanceSpawnerCooldown = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BalanceSpawnerNoSpawnCooldown")
			{
				BalanceSystem20.s_BalanceSpawnerNoSpawnCooldown = key.GetVariable(0).FValue;
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

	private void InitializeQuadTrees()
	{
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		Bounds bounds = default(Bounds);
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			if (i == 0)
			{
				bounds.min = (bounds.max = terrain.GetPosition());
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
		this.m_DisabledSpawnersQuadTree = new QuadTreeBalanceSystemDisabledSpawners(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
	}

	public void InitializeDisabledSpawnersQuadTree()
	{
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		Bounds bounds = default(Bounds);
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			if (i == 0)
			{
				bounds.min = (bounds.max = terrain.GetPosition());
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
		this.m_DisabledSpawnersQuadTree = new QuadTreeBalanceSystemDisabledSpawners(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
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
		this.UpdateLists();
		if (!this.IsDeserializingNetworkData() && !this.m_CurrentTriggers.Contains(obj))
		{
			this.m_CurrentTriggers.Add(obj);
		}
	}

	public void OnBalanceSpawnerEnter(GameObject spawner_obj, BalanceSpawner bs)
	{
		if (this.m_DisabledSpawnersQuadTree.GetObjectInPos(spawner_obj.transform.position))
		{
			return;
		}
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(spawner_obj.transform.position);
		if (objectInPos != null)
		{
			if (objectInPos.m_BalanceSpawner == null)
			{
				objectInPos.m_BalanceSpawner = spawner_obj;
			}
			if (objectInPos.m_ActiveChildrenMask != 0 && objectInPos.m_GameObject == null && objectInPos.m_ItemID != ItemID.None)
			{
				this.SpawnObject(spawner_obj, bs, objectInPos);
			}
			if (!this.m_ObjectsInArea[objectInPos.m_Group.name].Contains(objectInPos))
			{
				this.m_ObjectsInArea[objectInPos.m_Group.name].Add(objectInPos);
			}
			return;
		}
		if (bs != null && (Time.time - bs.m_LastSpawnObjectTime < BalanceSystem20.s_BalanceSpawnerCooldown || Time.time - bs.m_LastNoSpawnObjectTime < BalanceSystem20.s_BalanceSpawnerNoSpawnCooldown) && !this.IsDeserializingNetworkData())
		{
			return;
		}
		this.SpawnObject(spawner_obj, bs, null);
	}

	private void SpawnObject(GameObject spawner_obj, BalanceSpawner bs, BalanceSystemObject bso = null)
	{
		string empty = string.Empty;
		BSItemData objectToSpawn = this.GetObjectToSpawn(ref empty, bso);
		GameObject gameObject = null;
		if (objectToSpawn != null)
		{
			gameObject = objectToSpawn.m_Prefab;
			this.OnObjectSpawned(objectToSpawn, bs);
		}
		else
		{
			bs.m_LastNoSpawnObjectTime = Time.time;
		}
		if (gameObject != null)
		{
			objectToSpawn.m_LastSpawnTime = Time.time;
			GameObject gameObject2 = (bso != null) ? bso.m_GameObject : null;
			if (gameObject2 == null && spawner_obj != null)
			{
				gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, spawner_obj.transform.position, spawner_obj.transform.rotation);
				Item component = gameObject2.GetComponent<Item>();
				if (component)
				{
					component.m_CanSaveNotTriggered = false;
					Item[] componentsInChildren = component.GetComponentsInChildren<Item>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].m_CanSaveNotTriggered = false;
					}
				}
			}
			BalanceSystemObject balanceSystemObject = bso ?? new BalanceSystemObject();
			balanceSystemObject.m_GameObject = gameObject2;
			balanceSystemObject.m_Group = this.m_Groups[empty];
			balanceSystemObject.m_BalanceSpawner = bs.gameObject;
			balanceSystemObject.m_Position = bs.transform.position;
			balanceSystemObject.m_ItemID = objectToSpawn.m_ItemID;
			this.m_QuadTree.InsertObject(balanceSystemObject);
			if (!this.m_ObjectsInArea[empty].Contains(balanceSystemObject))
			{
				this.m_ObjectsInArea[empty].Add(balanceSystemObject);
			}
			if (empty == "Sanity")
			{
				CJObject[] array = (gameObject2 != null) ? gameObject2.GetComponentsInChildren<CJObject>() : null;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].m_Hallucination = true;
				}
			}
			else
			{
				bs.m_LastSpawnObjectTime = Time.time;
			}
			this.ApplyActiveChildrenMask(balanceSystemObject);
			if (!this.IsDeserializingNetworkData() && bso == null)
			{
				ReplicatedBalanceObjects.OnObjectChanged(balanceSystemObject);
			}
		}
	}

	public void OnBalanceAttachmentSpawnerEnter(GameObject obj, BalanceAttachmentSpawner bs)
	{
		if (Time.time - bs.m_LastSpawnObjectTime < BalanceSystem20.s_BalanceSpawnerCooldown && !this.IsDeserializingNetworkData())
		{
			return;
		}
		BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(obj.transform.position);
		if (objectInPos != null)
		{
			if (objectInPos.m_GameObject == null)
			{
				if (!bs.m_StaticSystem)
				{
					this.TryToAttach(bs, objectInPos);
					return;
				}
				if (!objectInPos.m_AllChildrenDestroyed)
				{
					Item item = bs.Attach(objectInPos.m_ItemID, objectInPos.m_ChildNum, objectInPos.m_ActiveChildrenMask);
					objectInPos.m_GameObject = item.gameObject;
					objectInPos.m_BalanceSpawner = bs.gameObject;
					this.m_ObjectsInArea[objectInPos.m_Group.name].Add(objectInPos);
					return;
				}
				if (Time.time - objectInPos.m_LastSpawnObjectTime > BalanceSystem20.s_BalanceSpawnerCooldown)
				{
					this.TryToAttach(bs, objectInPos);
					return;
				}
			}
			else if (!this.m_ObjectsInArea[objectInPos.m_Group.name].Contains(objectInPos))
			{
				this.m_ObjectsInArea[objectInPos.m_Group.name].Add(objectInPos);
				return;
			}
		}
		else
		{
			if (bs != null && (Time.time - bs.m_LastSpawnObjectTime < BalanceSystem20.s_BalanceSpawnerCooldown || Time.time - bs.m_LastNoSpawnObjectTime < BalanceSystem20.s_BalanceSpawnerNoSpawnCooldown))
			{
				return;
			}
			if (this.IsDeserializingNetworkData())
			{
				return;
			}
			this.TryToAttach(bs, null);
		}
	}

	private void TryToAttach(BalanceAttachmentSpawner bs, BalanceSystemObject bso)
	{
		if (this.IsDeserializingNetworkData() && bso != null)
		{
			Item item = bs.Attach(bso.m_ItemID, bso.m_ChildNum, bso.m_ActiveChildrenMask);
			if (item)
			{
				bso.m_GameObject = item.gameObject;
				return;
			}
		}
		else
		{
			string empty = string.Empty;
			BSItemData bsitemData = (bso != null) ? this.GetObjectToAttach(ref empty, new List<string>
			{
				EnumUtils<ItemID>.GetName(bso.m_ItemID)
			}) : this.GetObjectToAttach(ref empty, bs.m_ItemIDNamesList);
			if (bsitemData != null)
			{
				int childNum = -1;
				GameObject gameObject = bs.TryToAttach(bsitemData.m_ItemID, out childNum);
				if (gameObject != null)
				{
					this.OnObjectSpawned(bsitemData, bs);
					bsitemData.m_LastSpawnTime = Time.time;
					BalanceSystemObject balanceSystemObject = bso ?? new BalanceSystemObject();
					balanceSystemObject.m_Group = this.m_Groups[empty];
					balanceSystemObject.m_ChildNum = childNum;
					balanceSystemObject.m_ItemID = bsitemData.m_ItemID;
					balanceSystemObject.m_BalanceSpawner = bs.gameObject;
					balanceSystemObject.m_GameObject = gameObject;
					balanceSystemObject.m_Position = bs.transform.position;
					balanceSystemObject.m_AllChildrenDestroyed = false;
					this.m_ObjectsInArea[empty].Add(balanceSystemObject);
					this.m_QuadTree.InsertObject(balanceSystemObject);
					if (bso == null)
					{
						this.DetachActiveRigidbodies(gameObject);
						this.SetupActiveChildrenMask(gameObject, ref balanceSystemObject.m_ActiveChildrenMask);
					}
					else
					{
						this.ApplyActiveChildrenMask(balanceSystemObject);
					}
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
					if (!this.IsDeserializingNetworkData())
					{
						ReplicatedBalanceObjects.OnObjectChanged(balanceSystemObject);
						return;
					}
				}
			}
			else
			{
				bs.m_LastNoSpawnObjectTime = Time.time;
			}
		}
	}

	public void OnObjectTriggerExit(GameObject obj)
	{
		this.RemoveObjectFromArea(obj);
		if (!this.IsDeserializingNetworkData())
		{
			this.m_CurrentTriggers.Remove(obj);
		}
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

	private BSItemData GetObjectToSpawn(ref string group, BalanceSystemObject bso = null)
	{
		group = string.Empty;
		if (bso != null && bso.m_ItemID != ItemID.None)
		{
			BSItemData bsitemData = null;
			foreach (string text in this.m_SpawnData.Keys)
			{
				if (!(text == "Sanity"))
				{
					for (int i = 0; i < this.m_SpawnData[text].Count; i++)
					{
						if (bso.m_ItemID == this.m_SpawnData[text][i].m_ItemID)
						{
							group = text;
							bsitemData = this.m_SpawnData[text][i];
							break;
						}
					}
				}
			}
			if (bsitemData != null)
			{
				return bsitemData;
			}
		}
		if (PlayerSanityModule.Get().m_ItemHallucinationsEnabled && this.m_ObjectsInArea["Sanity"].Count < PlayerSanityModule.Get().GetWantedItemsHallucinationsCount())
		{
			group = "Sanity";
			return this.GetRandomObject(this.m_SpawnData[group], ItemID.None);
		}
		foreach (string text2 in this.m_SpawnData.Keys)
		{
			if (!(text2 == "Sanity"))
			{
				BSItemData randomObject = this.GetRandomObject(this.m_SpawnData[text2], ItemID.None);
				if (randomObject != null)
				{
					group = text2;
					return randomObject;
				}
			}
		}
		return null;
	}

	private void OnObjectSpawned(BSItemData data, BalanceSpawner bs)
	{
		data.m_LastSpawnPos = bs.transform.position;
		data.m_ChanceAccu = 0f;
	}

	private BSItemData GetRandomObject(List<BSItemData> list, ItemID matching_item = ItemID.None)
	{
		if (list.Count == 0)
		{
			return null;
		}
		float time = Time.time;
		for (int i = 0; i < list.Count; i++)
		{
			if (matching_item == ItemID.None || matching_item == list[i].m_ItemID)
			{
				BSItemData bsitemData = list[i];
				float chance = bsitemData.m_Chance;
				if (UnityEngine.Random.Range(1f, 2f) < chance)
				{
					if (matching_item == ItemID.None)
					{
						return bsitemData;
					}
					if (matching_item == bsitemData.m_ItemID)
					{
						return bsitemData;
					}
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
			BSItemData randomObject = this.GetRandomObject(this.m_AttachmentData[group], ItemID.None);
			if (randomObject != null && item_id_names.Contains(randomObject.m_ItemID.ToString()))
			{
				return randomObject;
			}
		}
		foreach (string text in this.m_AttachmentData.Keys)
		{
			if (!(text == "Sanity"))
			{
				for (int i = 0; i < item_id_names.Count; i++)
				{
					BSItemData randomObject2 = this.GetRandomObject(this.m_AttachmentData[text], (ItemID)Enum.Parse(typeof(ItemID), item_id_names[i]));
					if (randomObject2 != null)
					{
						group = text;
						return randomObject2;
					}
				}
			}
		}
		return null;
	}

	private void Update()
	{
		if (Debug.isDebugBuild)
		{
			this.UpdateInputs();
		}
		if (!this.m_BlockListsUpdate)
		{
			this.UpdateLists();
		}
	}

	private void UpdateLists()
	{
		this.UpdateList(this.m_SpawnData);
		this.UpdateList(this.m_AttachmentData);
	}

	private void UpdateList(Dictionary<string, List<BSItemData>> dict)
	{
		Vector3 position = Player.Get().transform.position;
		foreach (KeyValuePair<string, List<BSItemData>> keyValuePair in dict)
		{
			foreach (BSItemData bsitemData in keyValuePair.Value)
			{
				bsitemData.m_ChanceAccu += bsitemData.m_IncRate * Time.deltaTime;
				bsitemData.m_Chance = bsitemData.m_ChanceAccu;
				if (bsitemData.m_ItemID == ItemID.Banana_High_Flower || bsitemData.m_ItemID == ItemID.Banana_Medium_Flower || bsitemData.m_ItemID == ItemID.Banana_Low_Flower)
				{
					int num = 0 + 1;
				}
				if (bsitemData.m_WalkRange != 0f)
				{
					float num2 = bsitemData.m_LastSpawnPos.Distance(position) / bsitemData.m_WalkRange;
					bsitemData.m_Chance += num2 * bsitemData.m_WalkRangeValue;
				}
				if (bsitemData.m_Func != null)
				{
					bsitemData.m_Func();
				}
				if (bsitemData.m_HaveItemID.Count > 0)
				{
					int num3 = 0;
					for (int i = 0; i < bsitemData.m_HaveItemID.Count; i++)
					{
						num3 += InventoryBackpack.Get().GetItemsCount((ItemID)bsitemData.m_HaveItemID[i]);
					}
					float num4 = (float)(num3 / bsitemData.m_HaveItemCount) * bsitemData.m_HaveItemNegativeEffect;
					bsitemData.m_Chance -= num4;
				}
			}
			Dictionary<string, List<BSItemData>>.Enumerator enumerator;
			keyValuePair = enumerator.Current;
			keyValuePair.Value.Sort(BalanceSystem20.s_Comparer);
		}
	}

	public void OnItemDestroyed(CJObject balance_object)
	{
		bool main_obj_destroyed;
		GameObject gameObject;
		if (!balance_object.IsItem() && balance_object.transform.parent)
		{
			main_obj_destroyed = false;
			gameObject = balance_object.transform.parent.gameObject;
		}
		else
		{
			main_obj_destroyed = true;
			gameObject = balance_object.gameObject;
		}
		bool flag = false;
		foreach (string group in this.m_Groups.Keys)
		{
			if (this.ItemDestroyed(group, gameObject, main_obj_destroyed))
			{
				flag = true;
				break;
			}
		}
		if (!flag && gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			foreach (string group2 in this.m_Groups.Keys)
			{
				if (this.ItemDestroyed(group2, gameObject, false))
				{
					break;
				}
			}
		}
	}

	private bool ItemDestroyed(string group, GameObject go, bool main_obj_destroyed)
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
					int activeChildrenMask = balanceSystemObject.m_ActiveChildrenMask;
					if (component.IsAttachmentSpawner())
					{
						DestroyIfNoChildren component2 = gameObject.GetComponent<DestroyIfNoChildren>();
						if (component2 == null)
						{
							Debug.Log("obj_in_area name: " + gameObject.name);
							DebugUtils.Assert(DebugUtils.AssertType.Info);
							continue;
						}
						if (component2.m_NumChildren == 0)
						{
							balanceSystemObject.m_AllChildrenDestroyed = true;
						}
						if (component2.CheckNoChildren())
						{
							balanceSystemObject.m_ActiveChildrenMask = 0;
						}
						else
						{
							this.SetupActiveChildrenMask(gameObject, ref balanceSystemObject.m_ActiveChildrenMask);
						}
						if (main_obj_destroyed)
						{
							this.m_ObjectsInArea[group].Remove(balanceSystemObject);
						}
						else
						{
							i++;
						}
					}
					else
					{
						Item item = null;
						this.m_TempItemList.Clear();
						go.GetComponents<Item>(this.m_TempItemList);
						if (this.m_TempItemList.Count > 0)
						{
							item = this.m_TempItemList[0];
						}
						if (item && item.m_DestroyingOnlyScript)
						{
							i++;
						}
						else
						{
							if (main_obj_destroyed)
							{
								this.m_QuadTree.RemoveObject(balanceSystemObject);
								this.m_ObjectsInArea[group].Remove(balanceSystemObject);
							}
							else
							{
								i++;
							}
							if (go.GetComponent<DestroyablePlant>())
							{
								balanceSystemObject.m_ActiveChildrenMask = 0;
							}
							else
							{
								this.SetupActiveChildrenMask(gameObject, ref balanceSystemObject.m_ActiveChildrenMask);
							}
						}
					}
					if (main_obj_destroyed && balanceSystemObject.m_ActiveChildrenMask == 0)
					{
						ReplicatedBalanceObjects.OnObjectDestroyed(balanceSystemObject);
					}
					else if (activeChildrenMask != balanceSystemObject.m_ActiveChildrenMask)
					{
						ReplicatedBalanceObjects.OnObjectChanged(balanceSystemObject);
					}
					return true;
				}
				i++;
			}
			else
			{
				i++;
			}
		}
		return false;
	}

	private void SetupActiveChildrenMask(GameObject obj, ref int mask)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if ((mask & 1 << i) != 0)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			int num3 = 0;
			if (num > 0)
			{
				int num4 = 0;
				while (num4 <= j && num4 + num3 <= num)
				{
					if ((mask & 1 << num4 + num3) == 0 && obj.transform.GetChild(j).gameObject.activeSelf)
					{
						num3++;
					}
					else
					{
						num4++;
					}
				}
			}
			if (obj.transform.GetChild(j).gameObject.activeSelf)
			{
				num2 |= 1 << j + num3;
			}
		}
		mask = num2;
	}

	private void ApplyActiveChildrenMask(BalanceSystemObject bso)
	{
		for (int i = 0; i < bso.m_GameObject.transform.childCount; i++)
		{
			Transform child = bso.m_GameObject.transform.GetChild(i);
			if ((bso.m_ActiveChildrenMask & 1 << i) == 0)
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	private void UpdateInputs()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			this.m_Debug = !this.m_Debug;
		}
	}

	public void BlockListsUpdate()
	{
		this.m_BlockListsUpdate = true;
	}

	public void UnblockListsUpdate()
	{
		this.m_BlockListsUpdate = false;
	}

	public void SetItemsLastSpawnToPlayerPos()
	{
		Dictionary<string, List<BSItemData>>.Enumerator enumerator = this.m_SpawnData.GetEnumerator();
		Vector3 position = Player.Get().transform.position;
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, List<BSItemData>> keyValuePair = enumerator.Current;
			foreach (BSItemData bsitemData in keyValuePair.Value)
			{
				bsitemData.m_LastSpawnPos = position;
			}
		}
		Dictionary<string, List<BSItemData>>.Enumerator enumerator3 = this.m_AttachmentData.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			KeyValuePair<string, List<BSItemData>> keyValuePair = enumerator3.Current;
			foreach (BSItemData bsitemData2 in keyValuePair.Value)
			{
				bsitemData2.m_LastSpawnPos = position;
			}
		}
	}

	public void OnCreateConstruction(Construction con)
	{
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(con.gameObject);
		Bounds bounds = new Bounds(con.transform.position, Vector3.zero);
		foreach (Renderer renderer in componentsDeepChild)
		{
			bounds.Encapsulate(renderer.bounds);
		}
		int num = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, BalanceSystem20.s_OverlapCollidersTmp, Quaternion.identity, int.MinValue);
		for (int j = 0; j < num; j++)
		{
			BalanceSpawner component = BalanceSystem20.s_OverlapCollidersTmp[j].GetComponent<BalanceSpawner>();
			if (component && !component.IsAttachmentSpawner())
			{
				this.m_DisabledSpawnersQuadTree.InsertObject(component);
			}
		}
	}

	public void OnDestroyConstruction(Construction con)
	{
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(con.gameObject);
		Bounds bounds = new Bounds(con.transform.position, Vector3.zero);
		foreach (Renderer renderer in componentsDeepChild)
		{
			bounds.Encapsulate(renderer.bounds);
		}
		List<BalanceSpawner> objectsInBounds = this.m_DisabledSpawnersQuadTree.GetObjectsInBounds(bounds);
		for (int j = 0; j < objectsInBounds.Count; j++)
		{
			this.m_DisabledSpawnersQuadTree.RemoveObject(objectsInBounds[j]);
		}
	}

	public void OnFullLoadEnd()
	{
		foreach (string key in this.m_Groups.Keys)
		{
			this.m_ObjectsInArea[key] = new List<BalanceSystemObject>();
		}
		this.InitializeQuadTrees();
	}

	public BalanceSystemObject GetBalanceSystemObject(Vector3 pos)
	{
		return this.m_QuadTree.GetObjectInPos(pos);
	}

	public BalanceSystem20.GroupProps GetGroupByIndex(int idx)
	{
		foreach (BalanceSystem20.GroupProps groupProps in this.m_Groups.Values)
		{
			if (groupProps.index == idx)
			{
				return groupProps;
			}
		}
		return null;
	}

	private void SpawnObjectsIfNeeded()
	{
		foreach (GameObject gameObject in this.m_CurrentTriggers)
		{
			if (gameObject != null)
			{
				BalanceSystemObject objectInPos = this.m_QuadTree.GetObjectInPos(gameObject.transform.position);
				if (objectInPos != null && objectInPos.m_GameObject == null)
				{
					this.OnObjectTriggerEnter(gameObject);
				}
			}
		}
	}

	private void DetachActiveRigidbodies(GameObject go)
	{
		for (int i = 0; i < go.transform.childCount; i++)
		{
			Transform child = go.transform.GetChild(i);
			if (child.gameObject.ReplIsReplicable() && child.GetComponent<Rigidbody>())
			{
				CJObject component = child.gameObject.GetComponent<CJObject>();
				if (component && component.enabled)
				{
					this.OnItemDestroyed(component);
					child.parent = null;
				}
			}
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		if (initial_state)
		{
			this.m_QuadTree.SerializeAllObjects(writer);
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		if (initial_state)
		{
			foreach (string key in this.m_Groups.Keys)
			{
				this.m_ObjectsInArea[key].Clear();
			}
			this.m_QuadTree.Clear();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BalanceSystemObject balanceSystemObject = new BalanceSystemObject();
				reader.ReadVector3();
				balanceSystemObject.Deserialize(reader);
				this.m_QuadTree.InsertObject(balanceSystemObject, balanceSystemObject.m_Position);
			}
			this.SpawnObjectsIfNeeded();
		}
	}

	public void OnBalanceSystemObjectReplReceived(BalanceSystemObject readonly_temp_obj, bool destroyed = false)
	{
		BalanceSystemObject balanceSystemObject = this.m_QuadTree.GetObjectInPos(readonly_temp_obj.m_Position);
		if (destroyed)
		{
			if (balanceSystemObject != null && balanceSystemObject.m_GameObject != null)
			{
				balanceSystemObject.m_AllChildrenDestroyed = true;
				UnityEngine.Object.Destroy(balanceSystemObject.m_GameObject);
				return;
			}
		}
		else
		{
			if (balanceSystemObject != null)
			{
				int activeChildrenMask = balanceSystemObject.m_ActiveChildrenMask;
				if (readonly_temp_obj.m_ActiveChildrenMask != activeChildrenMask)
				{
					GameObject gameObject = balanceSystemObject.m_GameObject;
					if (gameObject != null)
					{
						for (int i = 0; i < gameObject.transform.childCount; i++)
						{
							GameObject gameObject2 = gameObject.transform.GetChild(i).gameObject;
							if (gameObject2.activeSelf)
							{
								bool flag = (activeChildrenMask & 1 << i) != 0;
								bool flag2 = (readonly_temp_obj.m_ActiveChildrenMask & 1 << i) != 0;
								if (flag != flag2 && !flag2)
								{
									UnityEngine.Object.Destroy(gameObject2);
								}
							}
						}
					}
					else
					{
						balanceSystemObject.CopyReplValues(readonly_temp_obj);
					}
				}
				this.SpawnObjectsIfNeeded();
				return;
			}
			balanceSystemObject = new BalanceSystemObject();
			balanceSystemObject.CopyReplValues(readonly_temp_obj);
			this.m_QuadTree.InsertObject(balanceSystemObject, balanceSystemObject.m_Position);
			this.SpawnObjectsIfNeeded();
		}
	}

	private bool IsDeserializingNetworkData()
	{
		if (ReplTools.IsPlayingAlone())
		{
			return false;
		}
		if (!this.ReplIsBeingDeserialized(false))
		{
			ReplicatedBalanceObjects local = ReplicatedBalanceObjects.GetLocal();
			return local != null && local.ReplIsBeingDeserialized(false);
		}
		return true;
	}

	private Dictionary<string, BalanceSystem20.GroupProps> m_Groups = new Dictionary<string, BalanceSystem20.GroupProps>();

	private Dictionary<string, List<BSItemData>> m_SpawnData = new Dictionary<string, List<BSItemData>>();

	private Dictionary<string, List<BalanceSystemObject>> m_ObjectsInArea = new Dictionary<string, List<BalanceSystemObject>>();

	private Dictionary<string, List<BSItemData>> m_AttachmentData = new Dictionary<string, List<BSItemData>>();

	private List<GameObject> m_CurrentTriggers = new List<GameObject>(5);

	private QuadTreeBalanceSystem m_QuadTree;

	private QuadTreeBalanceSystemDisabledSpawners m_DisabledSpawnersQuadTree;

	private PlayerConditionModule m_PlayerConditionModule;

	private PlayerInjuryModule m_PlayerInjuryModule;

	private PlayerDiseasesModule m_PlayerDiseasesModule;

	private PlayerSanityModule m_PlayerSanityModule;

	private static BalanceSystem20 s_Instance = null;

	private bool m_Debug;

	private static CompareListByChance s_Comparer = new CompareListByChance();

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

	public static float s_BalanceSpawnerCooldown = 600f;

	public static float s_BalanceSpawnerNoSpawnCooldown = 600f;

	[HideInInspector]
	public float m_WeightedAverage = 1f;

	[HideInInspector]
	public bool m_BlockListsUpdate;

	private List<Item> m_TempItemList = new List<Item>(10);

	private static Collider[] s_OverlapCollidersTmp = new Collider[200];

	public class GroupProps
	{
		public GroupProps(string n, int p, int i)
		{
			this.name = n;
			this.priority = p;
			this.index = i;
		}

		public string name;

		public int priority;

		public int index;
	}
}
