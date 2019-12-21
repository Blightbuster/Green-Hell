using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Construction : Item, IAttachable, IPlaceToAttach
{
	private List<Transform> m_PlacesToAttach { get; set; }

	private List<string> m_PlacesToAttachToNames { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Construction.s_AllConstructions.Add(this);
		this.m_ConstructionSlots = base.gameObject.GetComponentsInChildren<ConstructionSlot>();
		ConstructionSlot[] constructionSlots = this.m_ConstructionSlots;
		for (int i = 0; i < constructionSlots.Length; i++)
		{
			constructionSlots[i].m_ParentConstruction = this;
		}
		this.m_ReplConstructionSlots = new Construction.SReplConstructionSlot[this.m_ConstructionSlots.Length];
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_01");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_02");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_04");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_05");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_06");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Construction.s_AllConstructions.Remove(this);
		BalanceSystem20.Get().OnDestroyConstruction(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Construction.s_EnabledConstructions.Add(this);
		FirecampGroupsManager.Get().OnCreateConstruction(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Construction.s_EnabledConstructions.Remove(this);
		if (FirecampGroupsManager.Get() != null)
		{
			FirecampGroupsManager.Get().OnDestroyConstruction(this);
		}
	}

	protected override void Start()
	{
		base.Start();
		this.SetPlacesToAttach();
		this.SetPlacesToAttachTo();
		this.m_ConstructionInfo = (ConstructionInfo)this.m_Info;
		for (int i = 0; i < this.m_ConstructionSlots.Length; i++)
		{
			ConstructionSlot constructionSlot = this.m_ConstructionSlots[i];
			foreach (Construction construction in Construction.s_AllConstructions)
			{
				if (!(construction == this) && construction.m_Initialized && constructionSlot.m_MatchingItemIDs.Contains(construction.GetInfoID()) && (constructionSlot.transform.position - construction.transform.position).sqrMagnitude < 0.01f)
				{
					constructionSlot.SetConstruction(construction);
					break;
				}
			}
		}
		if (this.m_DisableFrame)
		{
			foreach (Construction construction2 in this.m_ConnectedConstructions)
			{
				ConstructionSlot[] constructionSlots = construction2.m_ConstructionSlots;
				for (int j = 0; j < constructionSlots.Length; j++)
				{
					if (constructionSlots[j].m_Construction == this)
					{
						construction2.ItemsManagerUnregister();
						construction2.gameObject.SetActive(false);
					}
				}
			}
		}
	}

	public Construction()
	{
		this.m_PlacesToAttach = new List<Transform>();
		this.m_PlacesToAttachToNames = new List<string>();
	}

	public Construction GetConstruction()
	{
		return this;
	}

	public virtual bool IsFirecamp()
	{
		return false;
	}

	public void SetUpperLevel(bool set, int level)
	{
		this.m_UpperLevel = set;
		this.m_Level = level;
		this.OnSetUpperLevel(set);
	}

	private void OnSetUpperLevel(bool set)
	{
		if (set)
		{
			if (this.m_UpperLevelShowElement)
			{
				this.m_UpperLevelShowElement.SetActive(true);
			}
			if (this.m_UpperLevelHideElement)
			{
				this.m_UpperLevelHideElement.SetActive(false);
				return;
			}
		}
		else
		{
			if (this.m_UpperLevelShowElement)
			{
				this.m_UpperLevelShowElement.SetActive(false);
			}
			if (this.m_UpperLevelHideElement)
			{
				this.m_UpperLevelHideElement.SetActive(true);
			}
		}
	}

	public List<Transform> GetPlacesToAttach()
	{
		return this.m_PlacesToAttach;
	}

	public List<string> GetPlacesToAttachTo()
	{
		return this.m_PlacesToAttachToNames;
	}

	private void SetPlacesToAttach()
	{
		ConstructionInfo constructionInfo = (ConstructionInfo)this.m_Info;
		for (int i = 0; i < constructionInfo.m_PlaceToAttachNames.Count; i++)
		{
			string name = constructionInfo.m_PlaceToAttachNames[i];
			Transform transform = base.gameObject.transform.FindDeepChild(name);
			if (transform != null)
			{
				this.m_PlacesToAttach.Add(transform);
			}
		}
	}

	private void SetPlacesToAttachTo()
	{
		ConstructionInfo constructionInfo = (ConstructionInfo)this.m_Info;
		for (int i = 0; i < constructionInfo.m_PlaceToAttachToNames.Count; i++)
		{
			this.m_PlacesToAttachToNames.Add(constructionInfo.m_PlaceToAttachToNames[i]);
		}
	}

	public virtual bool HasRestInfluence()
	{
		return true;
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog)
		{
			DialogsManager.Get().IsAnyDialogPlaying();
			return false;
		}
		return false;
	}

	public override void Save(int index)
	{
		base.Save(index);
		for (int i = 0; i < this.m_ConstructionSlots.Length; i++)
		{
			ConstructionSlot constructionSlot = this.m_ConstructionSlots[i];
			SaveGame.SaveVal(string.Concat(new object[]
			{
				"ConstructionSlot",
				index,
				constructionSlot.name,
				i
			}), constructionSlot.m_Construction != null);
			if (constructionSlot.m_Construction != null)
			{
				SaveGame.SaveVal(string.Concat(new object[]
				{
					"ConstructionSlotPos",
					index,
					constructionSlot.name,
					i
				}), constructionSlot.m_Construction.transform.position);
			}
		}
		SaveGame.SaveVal("ConstructionUpperLevel" + index, this.m_UpperLevel);
		SaveGame.SaveVal("ConstructionLevel" + index, this.m_Level);
		SaveGame.SaveVal("ConstructionBelow" + index, this.m_ConstructionBelow ? this.m_ConstructionBelow.transform.position : Vector3.zero);
		SaveGame.SaveVal("ConstructionBelowName" + index, this.m_ConstructionBelow ? this.m_ConstructionBelow.name : "None");
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_UpperLevel = SaveGame.LoadBVal("ConstructionUpperLevel" + index);
		this.m_Level = SaveGame.LoadIVal("ConstructionLevel" + index);
		this.m_ConstructionBelowPos = SaveGame.LoadV3Val("ConstructionBelow" + index);
		this.m_ConstructionBelowName = SaveGame.LoadSVal("ConstructionBelowName" + index);
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		IFirecampAttach[] components = base.gameObject.GetComponents<IFirecampAttach>();
		if (components.Length != 0)
		{
			Bounds bounds = new Bounds(base.transform.TransformPoint(this.m_BoxCollider.center), this.m_BoxCollider.size);
			for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
			{
				Firecamp firecamp = Firecamp.s_Firecamps[i];
				if (!(firecamp.gameObject == base.gameObject))
				{
					Bounds bounds2 = new Bounds(firecamp.transform.TransformPoint(firecamp.m_BoxCollider.center), firecamp.m_BoxCollider.size);
					if (bounds.Intersects(bounds2))
					{
						for (int j = 0; j < components.Length; j++)
						{
							components[j].SetFirecamp(firecamp);
						}
					}
				}
			}
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate4)
		{
			for (int k = 0; k < this.m_ConstructionSlots.Length; k++)
			{
				ConstructionSlot constructionSlot = this.m_ConstructionSlots[k];
				if (SaveGame.LoadBVal(string.Concat(new object[]
				{
					"ConstructionSlot",
					index,
					constructionSlot.name,
					k
				})))
				{
					Vector3 a = SaveGame.LoadV3Val(string.Concat(new object[]
					{
						"ConstructionSlotPos",
						index,
						constructionSlot.name,
						k
					}));
					foreach (Construction construction in Construction.s_AllConstructions)
					{
						if (!(construction == this) && (a - construction.transform.position).sqrMagnitude < 0.01f && constructionSlot.m_MatchingItemIDs.Contains(construction.GetInfoID()))
						{
							constructionSlot.SetConstruction(construction);
							break;
						}
					}
				}
			}
		}
		foreach (Construction construction2 in Construction.s_AllConstructions)
		{
			if (!(construction2 == this) && (this.m_ConstructionBelowPos - construction2.transform.position).sqrMagnitude < 0.01f && this.m_ConstructionBelowName == construction2.name)
			{
				this.m_ConstructionBelow = construction2;
				break;
			}
		}
		this.SetUpperLevel(this.m_UpperLevel, this.m_Level);
	}

	public override bool TakeDamage(DamageInfo damage_info)
	{
		if (this.m_ConstructionInfo.m_HitsCountToDestroy == 0)
		{
			return false;
		}
		if (damage_info.m_DamageItem && damage_info.m_DamageItem.m_Info.IsArrow())
		{
			return false;
		}
		if (!ItemsManager.Get().m_WasConstructionDestroyed && this.m_CurrentHitsCount == 0)
		{
			Hint hint = HintsManager.Get().FindHint(this.m_DestroyHintName);
			if (!HUDHint.Get().IsHintActive(hint) && Time.time - hint.m_LastShowTime > 5f)
			{
				hint.m_ShowedNTimes = 0;
				HintsManager.Get().ShowHint(this.m_DestroyHintName, 5f);
			}
		}
		this.ReplRequestOwnership(false);
		this.ReplSetDirty();
		this.m_CurrentHitsCount++;
		if (this.m_CurrentHitsCount >= this.m_ConstructionInfo.m_HitsCountToDestroy)
		{
			this.DestroyMe(true);
		}
		else
		{
			this.m_LastDamageTime = Time.time;
			base.HitShake();
		}
		return true;
	}

	public void OnOtherConstructionDestroyed(Construction construction)
	{
		if (construction == this.m_ConstructionBelow)
		{
			this.DestroyMe(true);
		}
	}

	public virtual void DestroyMe(bool check_connected = true)
	{
		if (this.m_Destroying)
		{
			return;
		}
		foreach (Construction construction in Construction.s_AllConstructions)
		{
			if (!(construction == this) && construction != null)
			{
				construction.OnOtherConstructionDestroyed(this);
			}
		}
		this.ResolveSlots();
		if (!Replicator.IsAnyObjectBeingDeserialized())
		{
			this.ReplRequestOwnership(false);
		}
		FoodProcessor component = base.gameObject.GetComponent<FoodProcessor>();
		if (component)
		{
			component.OnDestroyConstruction();
		}
		this.m_Destroying = true;
		GameObject prefab = GreenHellGame.Instance.GetPrefab(this.m_Info.m_ID.ToString() + "Ghost");
		if (prefab && this.ReplIsOwner())
		{
			foreach (GhostStep ghostStep in prefab.GetComponent<ConstructionGhost>().m_Steps)
			{
				foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
				{
					if (!ghostSlot.m_ItemName.ToLower().Contains("mud") && UnityEngine.Random.Range(0f, 1f) < 0.5f)
					{
						Vector3 position = (this.m_Info.m_ID == ItemID.Campfire && ghostSlot.gameObject.GetComponent<BoxCollider>()) ? ghostSlot.gameObject.GetComponent<BoxCollider>().center : ghostSlot.transform.localPosition;
						ItemsManager.Get().CreateItem((ItemID)Enum.Parse(typeof(ItemID), ghostSlot.m_ItemName), true, base.transform.TransformPoint(position), Quaternion.identity);
					}
				}
			}
		}
		foreach (ConstructionSlot constructionSlot in this.m_ConstructionSlots)
		{
			if (constructionSlot.m_Construction && constructionSlot.m_DestroyWithSnapParent && constructionSlot.m_MatchingItemIDs.Contains(constructionSlot.m_Construction.GetInfoID()))
			{
				constructionSlot.m_Construction.DestroyMe(true);
			}
		}
		if (this.m_DestroyWithFrame && check_connected)
		{
			foreach (Construction construction2 in this.m_ConnectedConstructions)
			{
				ConstructionSlot[] constructionSlots = construction2.m_ConstructionSlots;
				for (int i = 0; i < constructionSlots.Length; i++)
				{
					if (constructionSlots[i].m_Construction == this)
					{
						construction2.DestroyMe(false);
						break;
					}
				}
			}
		}
		if (this.m_DestroyWithChilds)
		{
			foreach (ConstructionSlot constructionSlot2 in this.m_ConstructionSlots)
			{
				if (constructionSlot2.m_Construction)
				{
					constructionSlot2.m_Construction.DestroyMe(false);
				}
			}
		}
		if (this.m_DestroySounds.Count > 0)
		{
			GameObject gameObject = new GameObject();
			OneShotSoundObject oneShotSoundObject = gameObject.AddComponent<OneShotSoundObject>();
			gameObject.name = "OneShotSoundObject from Construction";
			oneShotSoundObject.m_SoundNameWithPath = this.m_DestroySounds[UnityEngine.Random.Range(0, this.m_DestroySounds.Count)];
			oneShotSoundObject.gameObject.transform.position = base.transform.position;
		}
		ConstructionGhostManager.Get().OnDestoryConstruction(this);
		ItemsManager.Get().m_WasConstructionDestroyed = true;
		this.OnDestroyConstruction();
		if (!Replicator.IsAnyObjectBeingDeserialized())
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void OnDestroyConstruction()
	{
		int i = 0;
		while (i < this.m_Observers.Count)
		{
			this.m_Observers[i].OnDestroyConstruction(this);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Info.m_Health != this.m_Info.m_MaxHealth && Time.time - this.m_LastDamageTime > this.m_ResetHitsCountToDestroyTime)
		{
			this.m_CurrentHitsCount = 0;
			if (this.ReplIsOwner())
			{
				this.ReplSetDirty();
			}
		}
		if (!this.m_RegisteredInBalanceSystem)
		{
			BalanceSystem20.Get().OnCreateConstruction(this);
			this.m_RegisteredInBalanceSystem = true;
		}
	}

	public bool IsConstructionConnected(Construction construction)
	{
		using (List<Construction>.Enumerator enumerator = this.m_ConnectedConstructions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == construction)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanBeOutlined()
	{
		return false;
	}

	public void RegisterObserver(IConstructionObserver observer)
	{
		this.m_Observers.Add(observer);
	}

	public void UnregisterObserver(IConstructionObserver observer)
	{
		this.m_Observers.Remove(observer);
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		base.ReplOnChangedOwner(was_owner);
		if (!was_owner)
		{
			this.ResolveSlots();
		}
	}

	public override void ReplOnSpawned()
	{
		base.ReplOnSpawned();
		if (this.m_ConstructionInfo == null)
		{
			this.m_ConstructionInfo = (ConstructionInfo)this.m_Info;
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		base.OnReplicationSerialize(writer, initial_state);
		writer.Write(this.m_CurrentHitsCount);
		foreach (ConstructionSlot constructionSlot in this.m_ConstructionSlots)
		{
			writer.Write((constructionSlot.m_Construction != null) ? constructionSlot.m_Construction.gameObject : null);
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		base.OnReplicationDeserialize(reader, initial_state);
		this.m_ReplCurrentHitsCount = reader.ReadInt32();
		DebugUtils.Assert(this.m_ReplConstructionSlots.Length == this.m_ConstructionSlots.Length, true);
		for (int i = 0; i < this.m_ReplConstructionSlots.Length; i++)
		{
			this.m_ReplConstructionSlots[i].obj = reader.ReadReplicatedGameObject();
			this.m_ReplConstructionSlots[i].is_resolved = false;
		}
	}

	public override void OnReplicationResolve()
	{
		base.OnReplicationResolve();
		if (this.m_CurrentHitsCount < this.m_ReplCurrentHitsCount)
		{
			this.m_CurrentHitsCount = this.m_ReplCurrentHitsCount;
			if (this.m_CurrentHitsCount < this.m_ConstructionInfo.m_HitsCountToDestroy)
			{
				this.m_LastDamageTime = Time.time;
				base.HitShake();
			}
			else
			{
				this.DestroyMe(true);
			}
		}
		this.ResolveSlots();
	}

	private void ResolveSlots()
	{
		for (int i = 0; i < this.m_ReplConstructionSlots.Length; i++)
		{
			if (!this.m_ReplConstructionSlots[i].is_resolved)
			{
				GameObject gameObject = this.m_ReplConstructionSlots[i].obj.ResolveGameObject();
				Construction construction = (gameObject != null) ? gameObject.GetComponent<Construction>() : null;
				if (construction)
				{
					this.m_ConstructionSlots[i].SetConstruction(construction);
				}
				this.m_ReplConstructionSlots[i].is_resolved = (this.m_ReplConstructionSlots[i].obj.IsNull() || gameObject != null);
			}
		}
	}

	private List<string> m_DestroySounds = new List<string>();

	private string m_DestroyHintName = "Destroy_Construction";

	private float m_LastDamageTime;

	public float m_ResetHitsCountToDestroyTime = 30f;

	private int m_CurrentHitsCount;

	private int m_ReplCurrentHitsCount;

	private ConstructionInfo m_ConstructionInfo;

	public bool m_DestroyWithFrame;

	public bool m_DestroyWithChilds;

	public bool m_DisableFrame;

	[HideInInspector]
	public ConstructionSlot[] m_ConstructionSlots;

	private Construction.SReplConstructionSlot[] m_ReplConstructionSlots;

	[HideInInspector]
	public List<Construction> m_ConnectedConstructions = new List<Construction>();

	public static List<Construction> s_EnabledConstructions = new List<Construction>();

	public static List<Construction> s_AllConstructions = new List<Construction>();

	private List<IConstructionObserver> m_Observers = new List<IConstructionObserver>();

	public GameObject m_UpperLevelHideElement;

	public GameObject m_UpperLevelShowElement;

	private bool m_RegisteredInBalanceSystem;

	protected bool m_UpperLevel;

	public int m_Level;

	private Vector3 m_ConstructionBelowPos = Vector3.zero;

	private string m_ConstructionBelowName = "None";

	public Construction m_ConstructionBelow;

	private bool m_Destroying;

	private struct SReplConstructionSlot
	{
		public ReplicatedGameObject obj;

		public bool is_resolved;
	}
}
