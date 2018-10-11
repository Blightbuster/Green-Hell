using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BIWoundSlot : ItemSlot
{
	public override bool IsBIWoundSlot()
	{
		return true;
	}

	public bool IsVisible()
	{
		bool flag = Vector3.Dot(Camera.main.transform.forward, this.m_Transform.right) > 0.3f;
		Camera main = Camera.main;
		Vector3 vector = main.WorldToScreenPoint(this.m_Transform.position);
		bool flag2 = vector.x > 0f && vector.x < (float)Screen.width && vector.y > 0f && vector.y < (float)Screen.height;
		bool flag3 = this.m_Injury != null && this.m_Injury.m_Bandage != null;
		return flag && flag2 && !flag3;
	}

	public void SetInjury(Injury injury)
	{
		if (injury != null)
		{
			if (this.m_Transform == null)
			{
				return;
			}
			if (injury.m_Type == InjuryType.Worm)
			{
				string path = "Prefabs/Items/Item/botfly";
				GameObject gameObject = Resources.Load(path) as GameObject;
				if (gameObject == null)
				{
					path = "Prefabs/TempPrefabs/Item/Item/botfly";
					gameObject = (Resources.Load(path) as GameObject);
				}
				if (gameObject == null)
				{
					return;
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, this.m_Transform.position, this.m_Transform.rotation);
				Item component = gameObject2.GetComponent<Item>();
				if (component != null)
				{
					component.m_CanSave = false;
				}
				gameObject2.layer = Player.Get().gameObject.layer;
				this.m_Wound = gameObject2;
				gameObject2.transform.parent = this.m_Transform;
				gameObject2.GetComponent<Parasite>().m_InBody = true;
				List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(gameObject2);
				for (int i = 0; i < componentsDeepChild.Count; i++)
				{
					componentsDeepChild[i].gameObject.layer = LayerMask.NameToLayer("Player");
				}
				gameObject2.SetActive(false);
			}
			else if (injury.m_Type == InjuryType.Leech)
			{
				string path2 = "Prefabs/Items/Item/Leech";
				GameObject gameObject3 = Resources.Load(path2) as GameObject;
				if (gameObject3 == null)
				{
					path2 = "Prefabs/TempPrefabs/Items/Item/Leech";
					gameObject3 = (Resources.Load(path2) as GameObject);
				}
				if (gameObject3 == null)
				{
					return;
				}
				GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(gameObject3, this.m_Transform.position, this.m_Transform.rotation);
				Item component2 = gameObject4.GetComponent<Item>();
				if (component2 != null)
				{
					component2.m_CanSave = false;
				}
				gameObject4.layer = Player.Get().gameObject.layer;
				UnityEngine.Random.InitState((int)(Time.unscaledTime * 4538.3f));
				this.m_Wound = gameObject4;
				gameObject4.transform.parent = this.m_Transform;
				Animator component3 = gameObject4.GetComponent<Animator>();
				component3.speed = UnityEngine.Random.Range(0.8f, 1.2f);
				gameObject4.GetComponent<Parasite>().m_InBody = true;
				Animator component4 = gameObject4.GetComponent<Animator>();
				component4.SetBool("Drink", true);
				List<Renderer> componentsDeepChild2 = General.GetComponentsDeepChild<Renderer>(gameObject4);
				for (int j = 0; j < componentsDeepChild2.Count; j++)
				{
					componentsDeepChild2[j].gameObject.layer = LayerMask.NameToLayer("Player");
				}
			}
			else if (injury.m_Type == InjuryType.VenomBite || injury.m_Type == InjuryType.SnakeBite || injury.m_Type == InjuryType.Laceration || injury.m_Type == InjuryType.LacerationCat || injury.m_Type == InjuryType.Rash || injury.m_Type == InjuryType.SmallWoundAbrassion || injury.m_Type == InjuryType.SmallWoundScratch || injury.m_Type == InjuryType.WormHole)
			{
				this.m_Injury = injury;
				this.m_Wound = this.m_Transform.gameObject;
				this.m_Wound.SetActive(true);
				if (injury.m_Type == InjuryType.Rash && this.m_AdditionalMeshes != null)
				{
					for (int k = 0; k < this.m_AdditionalMeshes.Count; k++)
					{
						this.m_AdditionalMeshes[k].SetActive(true);
					}
				}
			}
		}
		else
		{
			if (this.m_Wound != null)
			{
				Item component5 = this.m_Wound.GetComponent<Item>();
				if (component5 != null && component5.m_Info == null)
				{
					UnityEngine.Object.Destroy(component5.gameObject);
				}
				else if (component5 == null || (component5 != null && component5.m_Info.m_ID != ItemID.Leech && component5.m_Info.m_ID != ItemID.Botfly))
				{
					this.m_Wound.SetActive(false);
				}
				this.m_Wound = null;
			}
			if (this.m_AdditionalMeshes != null)
			{
				for (int l = 0; l < this.m_AdditionalMeshes.Count; l++)
				{
					this.m_AdditionalMeshes[l].SetActive(false);
				}
			}
		}
		this.m_Injury = injury;
	}

	public Injury GetInjury()
	{
		return this.m_Injury;
	}

	public override Vector3 GetScreenPoint()
	{
		Vector3 position = base.transform.position;
		Renderer component = base.gameObject.GetComponent<Renderer>();
		if (component != null)
		{
			position = component.bounds.center;
		}
		if (this.m_Camera)
		{
			return Camera.main.ViewportToScreenPoint(this.m_Camera.WorldToViewportPoint(position));
		}
		if (Camera.main)
		{
			return Camera.main.WorldToScreenPoint(position);
		}
		return Vector3.zero;
	}

	public override void InsertItem(Item item)
	{
		base.InsertItem(item);
		if (this.m_Injury.m_State == InjuryState.Infected)
		{
			if (item.m_Info.m_ID == ItemID.Maggots)
			{
				BodyInspectionController.Get().AttachMaggots(true);
			}
			else
			{
				this.m_Injury.m_HealingResultInjuryState = InjuryState.Open;
				BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
			}
		}
		else if (this.m_Injury.m_Type == InjuryType.Worm)
		{
			BodyInspectionController.Get().Deworm(item, this);
		}
		else if (this.m_Injury.m_Type == InjuryType.VenomBite || this.m_Injury.m_Type == InjuryType.SnakeBite || this.m_Injury.m_Type == InjuryType.SmallWoundAbrassion || this.m_Injury.m_Type == InjuryType.SmallWoundScratch || this.m_Injury.m_Type == InjuryType.Rash || this.m_Injury.m_Type == InjuryType.WormHole)
		{
			if (this.m_Injury.m_Type == InjuryType.VenomBite || this.m_Injury.m_Type == InjuryType.SnakeBite)
			{
				this.m_PoisonDebuff = item.m_Info.m_PoisonDebuff;
			}
			if (this.m_Injury.m_Type == InjuryType.WormHole && this.m_Injury.m_State == InjuryState.WormInside)
			{
				BodyInspectionController.Get().Deworm(item, this);
			}
			else if (item.m_Info.m_ID != ItemID.Leech)
			{
				BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
			}
		}
		else if (this.m_Injury.m_Type == InjuryType.Laceration || this.m_Injury.m_Type == InjuryType.LacerationCat)
		{
			if (item.m_Info.m_ID == ItemID.Ants)
			{
				if (this.m_Injury.m_State == InjuryState.Bleeding || this.m_Injury.m_State == InjuryState.Open)
				{
					BodyInspectionController.Get().AttachAnts();
					this.m_Injury.m_HealingResultInjuryState = InjuryState.Closed;
					BodyInspectionController.Get().HideAndShowLimb();
				}
			}
			else if (item.m_Info.m_ID == ItemID.Goliath_dressing || item.m_Info.m_ID == ItemID.ash_dressing || item.m_Info.m_ID == ItemID.Honey_Dressing)
			{
				if (this.m_Injury.m_State == InjuryState.Bleeding || this.m_Injury.m_State == InjuryState.Open)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.None;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
			}
			else if (item.m_Info.m_ID == ItemID.Goliath_dressing || item.m_Info.m_ID == ItemID.ash_dressing || item.m_Info.m_ID == ItemID.Honey_Dressing)
			{
				if (this.m_Injury.m_State == InjuryState.Bleeding)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.Open;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
				else if (this.m_Injury.m_State == InjuryState.Closed)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.None;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
			}
			else if (item.m_Info.m_ID == ItemID.Leaf_Bandage)
			{
				if (this.m_Injury.m_State == InjuryState.Bleeding)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.Infected;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
				else if (this.m_Injury.m_State == InjuryState.Closed)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.None;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
				else if (this.m_Injury.m_State == InjuryState.Open)
				{
					this.m_Injury.m_HealingResultInjuryState = InjuryState.None;
					BodyInspectionController.Get().StartBandage(this.m_InjuryPlace);
				}
			}
		}
	}

	protected override void OnInsertItem(Item item)
	{
		base.OnInsertItem(item);
		if (this.m_Injury.m_Type == InjuryType.VenomBite || this.m_Injury.m_Type == InjuryType.SnakeBite || this.m_Injury.m_Type == InjuryType.SmallWoundAbrassion || this.m_Injury.m_Type == InjuryType.SmallWoundScratch || this.m_Injury.m_Type == InjuryType.Rash || this.m_Injury.m_Type == InjuryType.SmallWoundScratch || this.m_Injury.m_Type == InjuryType.WormHole)
		{
			this.m_Injury.m_HealingTimeDec = item.m_Info.m_HealingTimeDec;
			UnityEngine.Object.Destroy(item.gameObject);
		}
		else if (this.m_Injury.m_State == InjuryState.Infected)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		else if (this.m_Injury.m_Type == InjuryType.Laceration || this.m_Injury.m_Type == InjuryType.LacerationCat)
		{
			this.m_Injury.m_HealingTimeDec = item.m_Info.m_HealingTimeDec;
			UnityEngine.Object.Destroy(item.gameObject);
		}
		BodyInspectionController.Get().m_ActiveSlot = this;
		this.UnlockInjuryTreatment(item);
	}

	private void UnlockInjuryTreatment(Item item)
	{
		if ((this.m_Injury.m_Type == InjuryType.SmallWoundAbrassion || this.m_Injury.m_Type == InjuryType.SmallWoundScratch) && item.m_Info.m_ID == ItemID.Ficus_Dressing)
		{
			PlayerInjuryModule.Get().UnlockKnownInjuryTreatment(NotepadKnownInjuryTreatment.SmallWoundFicusLeafDressing);
		}
	}

	public override bool CanInsertItem(Item item)
	{
		if (this.m_Injury == null)
		{
			return false;
		}
		if (this.m_Injury.m_ParentInjury != null)
		{
			return false;
		}
		if (this.m_Injury.m_State == InjuryState.Infected)
		{
			return !(this.m_Injury.m_Bandage != null) && this.m_Injury.m_Slot.m_Maggots == null && (item.m_Info.m_ID == ItemID.Maggots || item.m_Info.m_ID == ItemID.Honey_Dressing);
		}
		if (this.m_Injury.m_Type == InjuryType.SmallWoundAbrassion || this.m_Injury.m_Type == InjuryType.SmallWoundScratch || this.m_Injury.m_Type == InjuryType.WormHole)
		{
			if (this.m_Injury.m_Type == InjuryType.WormHole && this.m_Injury.m_State == InjuryState.WormInside && (item.m_Info.m_ID == ItemID.Fish_Bone || item.m_Info.m_ID == ItemID.Bone_Needle))
			{
				return true;
			}
			if (this.m_Injury.m_Bandage != null)
			{
				return false;
			}
			if (item.m_Info.m_Type == ItemType.Dressing)
			{
				return true;
			}
		}
		else if (this.m_Injury.m_Type == InjuryType.Worm || (this.m_Injury.m_Type == InjuryType.WormHole && this.m_Injury.m_State == InjuryState.WormInside))
		{
			if (item.m_Info.m_ID == ItemID.Fish_Bone || item.m_Info.m_ID == ItemID.Bone_Needle)
			{
				return true;
			}
		}
		else if (this.m_Injury.m_Type == InjuryType.VenomBite || this.m_Injury.m_Type == InjuryType.SnakeBite)
		{
			if (item.m_Info.m_ID == ItemID.Tabaco_Dressing || item.m_Info.m_ID == ItemID.lily_dressing)
			{
				return true;
			}
		}
		else if ((this.m_Injury.m_Type == InjuryType.Laceration || this.m_Injury.m_Type == InjuryType.LacerationCat) && (this.m_Injury.m_State == InjuryState.Bleeding || this.m_Injury.m_State == InjuryState.Open))
		{
			if (item.m_Info.m_ID == ItemID.Ants)
			{
				return true;
			}
			if (item.m_Info.m_ID == ItemID.ash_dressing || item.m_Info.m_ID == ItemID.Goliath_dressing || item.m_Info.m_ID == ItemID.Honey_Dressing || item.m_Info.m_ID == ItemID.Leaf_Bandage)
			{
				return true;
			}
		}
		if (this.m_Injury.m_Type == InjuryType.Rash)
		{
			if (this.m_Injury.m_Bandage != null)
			{
				return false;
			}
			if (item.m_Info.m_ID == ItemID.lily_dressing || item.m_Info.m_ID == ItemID.Honey_Dressing)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInjuryOfType(InjuryType injury_type)
	{
		for (int i = 0; i < this.m_InjuryType.Count; i++)
		{
			if (this.m_InjuryType[i] == injury_type)
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		if (this.m_Wound == null)
		{
			return;
		}
		for (int i = 0; i < this.m_InjuryType.Count; i++)
		{
			if (this.m_InjuryType[i] == InjuryType.Leech || this.m_InjuryType[i] == InjuryType.Worm)
			{
				this.m_Wound.transform.rotation = this.m_Transform.rotation;
				this.m_Wound.transform.position = this.m_Transform.position;
				this.m_Wound.transform.Rotate(this.m_Wound.transform.right, this.m_WoundRotationAngle, Space.World);
			}
		}
	}

	public Transform m_Transform;

	public GameObject m_Wound;

	public float m_WoundRotationAngle;

	public InjuryPlace m_InjuryPlace = InjuryPlace.None;

	public List<InjuryType> m_InjuryType = new List<InjuryType>();

	public Injury m_Injury;

	public List<GameObject> m_AdditionalMeshes;

	public List<GameObject> m_Maggots;

	public List<GameObject> m_Ants;

	public Vector3[] m_LacerationMaggotsVertices;

	public WeightList[] m_LacerationMaggotsWeightList;

	public Vector3[] m_LacerationCatMaggotsVertices;

	public WeightList[] m_LacerationCatMaggotsWeightList;

	public Vector3[] m_AbrassionMaggotsVertices;

	public WeightList[] m_AbrassionMaggotsWeightList;

	public Vector3[] m_ScratchMaggotsVertices;

	public WeightList[] m_ScratchMaggotsWeightList;

	public Vector3[] m_WormHoleMaggotsVertices;

	public WeightList[] m_WormHoleMaggotsWeightList;

	public Vector3[] m_LacerationAntsVertices;

	public WeightList[] m_LacerationAntsWeightList;

	public Vector3[] m_LacerationCatAntsVertices;

	public WeightList[] m_LacerationCatAntsWeightList;

	public int m_PoisonDebuff;
}
