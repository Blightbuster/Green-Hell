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
	}

	public void TakeDamage(float damage)
	{
		this.m_Info.m_Health -= damage;
		if (this.m_Info.m_Health <= 0f)
		{
			this.DestroyMe();
		}
	}

	private void DestroyMe()
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
		GameObject gameObject = new GameObject();
		OneShotSoundObject oneShotSoundObject = gameObject.AddComponent<OneShotSoundObject>();
		gameObject.name = "OneShotSoundObject from Construction";
		oneShotSoundObject.m_SoundNameWithPath = this.m_DestroySounds[UnityEngine.Random.Range(0, this.m_DestroySounds.Count)];
		oneShotSoundObject.gameObject.transform.position = base.transform.position;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private List<string> m_DestroySounds = new List<string>();

	public static List<Construction> s_Constructions = new List<Construction>();
}
