using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class InsectsManager : MonoBehaviour
{
	public static InsectsManager Get()
	{
		return InsectsManager.s_Instance;
	}

	private void Awake()
	{
		InsectsManager.s_Instance = this;
		this.m_AvailableDummies.Add("Wound02");
		this.m_AvailableDummies.Add("Wound03");
		this.m_AvailableDummies.Add("Wound07");
		this.m_AvailableDummies.Add("Wound08");
		this.m_AvailableDummies.Add("Wound12");
		foreach (object obj in Enum.GetValues(typeof(InsectsManager.InsectType)))
		{
			InsectsManager.InsectType key = (InsectsManager.InsectType)obj;
			this.m_Prefabs.Add(key, Resources.Load("Prefabs/AI/" + key.ToString().ToLower()) as GameObject);
		}
	}

	public void Activate(InsectsManager.InsectType type)
	{
		this.m_Type = type;
		this.SpawnInsects();
	}

	private void SpawnInsects()
	{
		this.SpawnInsects(UnityEngine.Random.Range(this.m_MinCountPerHand, this.m_MaxCountPerHand + 1), Hand.Right);
		this.SpawnInsects(UnityEngine.Random.Range(this.m_MinCountPerHand, this.m_MaxCountPerHand + 1), Hand.Left);
	}

	private void SpawnInsects(int count, Hand hand)
	{
		GameObject original = this.m_Prefabs[this.m_Type];
		for (int i = 0; i < count; i++)
		{
			BIWoundSlot woundSlot = BodyInspectionController.Get().GetWoundSlot((hand == Hand.Right) ? InjuryPlace.RHand : InjuryPlace.LHand, InjuryType.Leech, null);
			if (!woundSlot)
			{
				break;
			}
			InsectData insectData = new InsectData();
			insectData.m_Insect = UnityEngine.Object.Instantiate<GameObject>(original, woundSlot.transform.position, woundSlot.transform.rotation);
			insectData.m_Slot = woundSlot;
			insectData.m_Hand = hand;
			this.m_Insects.Add(insectData);
		}
	}

	public void FlyAway(Hand hand)
	{
		if (!Camera.main)
		{
			return;
		}
		for (int i = 0; i < this.m_Insects.Count; i++)
		{
			if (!(this.m_Insects[i].m_Insect == null) && this.m_Insects[i].m_Hand == hand)
			{
				this.m_Insects[i].m_Insect.transform.GetChild(0).transform.localRotation = Quaternion.identity;
				this.m_Insects[i].m_FlyAwaySpeed = UnityEngine.Random.Range(this.m_MinFlyAwaySpeed, this.m_MaxFlyAwaySpeed);
				this.m_Insects[i].m_FlyAwayDirection = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Camera.main.transform.up) * Camera.main.transform.forward;
				this.m_Insects[i].m_FlyAwayDirection.y = Mathf.Max(this.m_Insects[i].m_FlyAwayDirection.y, 0.8f);
				this.m_Insects[i].m_FlyAwayDirection.Normalize();
				this.m_Insects[i].m_FlyAway = true;
			}
		}
	}

	public void UpdateInsects()
	{
		if (this.m_Insects.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.m_Insects.Count; i++)
		{
			if (this.m_Insects[i].m_FlyAway)
			{
				this.m_Insects[i].m_Insect.transform.position += this.m_Insects[i].m_FlyAwayDirection * this.m_Insects[i].m_FlyAwaySpeed * Time.deltaTime;
				this.m_Insects[i].m_Insect.transform.rotation = Quaternion.Slerp(this.m_Insects[i].m_Insect.transform.rotation, Quaternion.LookRotation(this.m_Insects[i].m_FlyAwayDirection, Vector3.up), Time.deltaTime * 3f);
				if (this.m_Insects[i].m_Insect.transform.position.Distance(Player.Get().transform.position) >= this.m_DistToDeactivate)
				{
					UnityEngine.Object.Destroy(this.m_Insects[i].m_Insect.gameObject);
					this.m_Insects.RemoveAt(i);
					i--;
				}
			}
			else
			{
				this.m_Insects[i].m_Insect.transform.rotation = this.m_Insects[i].m_Slot.m_Transform.rotation;
				this.m_Insects[i].m_Insect.transform.position = this.m_Insects[i].m_Slot.m_Transform.position;
			}
		}
	}

	private InsectsManager.InsectType m_Type = InsectsManager.InsectType.None;

	private Dictionary<InsectsManager.InsectType, GameObject> m_Prefabs = new Dictionary<InsectsManager.InsectType, GameObject>();

	public int m_MinCountPerHand;

	public int m_MaxCountPerHand;

	public float m_MinFlyAwaySpeed;

	public float m_MaxFlyAwaySpeed;

	private List<InsectData> m_Insects = new List<InsectData>();

	private List<string> m_AvailableDummies = new List<string>();

	public float m_DistToDeactivate;

	private static InsectsManager s_Instance;

	public enum InsectType
	{
		None = -1,
		Wasp,
		Ant,
		Count
	}
}
