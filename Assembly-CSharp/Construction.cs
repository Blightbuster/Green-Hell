using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Construction : Item, IAttachable, IPlaceToAttach
{
	public Construction()
	{
		this.m_PlacesToAttach = new List<Transform>();
		this.m_PlacesToAttachToNames = new List<string>();
	}

	private List<Transform> m_PlacesToAttach { get; set; }

	private List<string> m_PlacesToAttachToNames { get; set; }

	protected override void OnEnable()
	{
		base.OnEnable();
		Construction.s_Constructions.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Construction.s_Constructions.Remove(this);
	}

	protected override void Start()
	{
		base.Start();
		this.SetPlacesToAttach();
		this.SetPlacesToAttachTo();
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_01");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_02");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_04");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_05");
		this.m_DestroySounds.Add("Sounds/Constructions/construction_deconstruct_crash_06");
		this.m_ConstructionInfo = (ConstructionInfo)this.m_Info;
		ConstructionSlot[] componentsInChildren = base.gameObject.GetComponentsInChildren<ConstructionSlot>();
		this.m_ConstructionSlots = new List<ConstructionSlot>(componentsInChildren);
		foreach (ConstructionSlot constructionSlot in this.m_ConstructionSlots)
		{
			constructionSlot.m_ParentConstruction = this;
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
		return false;
	}

	public override void Save(int index)
	{
		base.Save(index);
		for (int i = 0; i < this.m_ConstructionSlots.Count; i++)
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
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		IFirecampAttach[] components = base.gameObject.GetComponents<IFirecampAttach>();
		if (components.Length > 0)
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
		if (GreenHellGame.s_GameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate4)
		{
			for (int k = 0; k < this.m_ConstructionSlots.Count; k++)
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
					foreach (Construction construction in Construction.s_Constructions)
					{
						if (!(construction == this))
						{
							if ((a - construction.transform.position).sqrMagnitude < 0.01f)
							{
								constructionSlot.SetConstruction(construction);
								break;
							}
						}
					}
				}
			}
		}
	}

	public override bool TakeDamage(DamageInfo damage_info)
	{
		if (this.m_ConstructionInfo.m_HitsCountToDestroy == 0)
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
		this.m_CurrentHitsCount++;
		if (this.m_CurrentHitsCount >= this.m_ConstructionInfo.m_HitsCountToDestroy)
		{
			this.DestroyMe();
		}
		else
		{
			this.m_LastDamageTime = Time.time;
			base.HitShake();
		}
		return true;
	}

	public virtual void DestroyMe()
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab(this.m_Info.m_ID.ToString() + "Ghost");
		ConstructionGhost component = prefab.GetComponent<ConstructionGhost>();
		foreach (GhostStep ghostStep in component.m_Steps)
		{
			foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
			{
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
				{
					ItemsManager.Get().CreateItem((ItemID)Enum.Parse(typeof(ItemID), ghostSlot.m_ItemName), true, base.transform.TransformPoint(ghostSlot.transform.localPosition), Quaternion.identity);
				}
			}
		}
		foreach (ConstructionSlot constructionSlot in this.m_ConstructionSlots)
		{
			if (constructionSlot.m_Construction && constructionSlot.m_DestroyWithSnapParent)
			{
				constructionSlot.m_Construction.DestroyMe();
			}
		}
		GameObject gameObject = new GameObject();
		OneShotSoundObject oneShotSoundObject = gameObject.AddComponent<OneShotSoundObject>();
		gameObject.name = "OneShotSoundObject from Construction";
		oneShotSoundObject.m_SoundNameWithPath = this.m_DestroySounds[UnityEngine.Random.Range(0, this.m_DestroySounds.Count)];
		oneShotSoundObject.gameObject.transform.position = base.transform.position;
		ItemsManager.Get().m_WasConstructionDestroyed = true;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Info.m_Health != this.m_Info.m_MaxHealth && Time.time - this.m_LastDamageTime > this.m_ResetHitsCountToDestroyTime)
		{
			this.m_CurrentHitsCount = 0;
		}
	}

	public bool IsConstructionConnected(Construction construction)
	{
		foreach (Construction x in this.m_ConnectedConstructions)
		{
			if (x == construction)
			{
				return true;
			}
		}
		return false;
	}

	private List<string> m_DestroySounds = new List<string>();

	private string m_DestroyHintName = "Destroy_Construction";

	private float m_LastDamageTime;

	public float m_ResetHitsCountToDestroyTime = 30f;

	private int m_CurrentHitsCount;

	private ConstructionInfo m_ConstructionInfo;

	[HideInInspector]
	public List<ConstructionSlot> m_ConstructionSlots;

	[HideInInspector]
	public List<Construction> m_ConnectedConstructions = new List<Construction>();

	public static List<Construction> s_Constructions = new List<Construction>();
}
