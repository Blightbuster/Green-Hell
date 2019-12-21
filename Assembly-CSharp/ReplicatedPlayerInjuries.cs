using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class ReplicatedPlayerInjuries : ReplicatedBehaviour
{
	private void Start()
	{
		this.Initialize();
	}

	private void Initialize()
	{
		if (this.m_Initialized)
		{
			return;
		}
		this.m_Initialized = true;
		this.m_WoundSlots = new List<ReplicatedPlayerInjuries.ReplicatedWoundSlot>(BodyInspectionController.Get().m_WoundSlots.Count);
		for (int i = 0; i < BodyInspectionController.Get().m_WoundSlots.Count; i++)
		{
			Transform transform = base.transform.FindDeepChild(BodyInspectionController.Get().m_WoundSlots[i].name);
			if (!(transform == null))
			{
				ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot = new ReplicatedPlayerInjuries.ReplicatedWoundSlot();
				replicatedWoundSlot.m_ReplicatedPlayerSlot = transform.gameObject;
				replicatedWoundSlot.m_LocalPlayerSlot = (base.ReplIsOwner() ? BodyInspectionController.Get().m_WoundSlots[i].gameObject : null);
				replicatedWoundSlot.m_SlotIndex = i;
				if (BodyInspectionController.Get().m_WoundSlots[i].m_AdditionalMeshes != null)
				{
					replicatedWoundSlot.m_ReplicatedPlayerAdditionalMeshes = new List<GameObject>(BodyInspectionController.Get().m_WoundSlots[i].m_AdditionalMeshes.Count);
					foreach (GameObject gameObject in BodyInspectionController.Get().m_WoundSlots[i].m_AdditionalMeshes)
					{
						Transform transform2 = base.transform.FindDeepChild(gameObject.name);
						DebugUtils.Assert(transform2 != null, true);
						if (transform2)
						{
							replicatedWoundSlot.m_ReplicatedPlayerAdditionalMeshes.Add(transform2.gameObject);
						}
					}
				}
				this.m_WoundSlots.Add(replicatedWoundSlot);
			}
		}
	}

	private int GetSlotIdUsingLocalPlayerSlot(GameObject local_player_slot)
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_LocalPlayerSlot == local_player_slot)
			{
				return i;
			}
		}
		return -1;
	}

	public override void OnReplicationPrepare()
	{
		base.OnReplicationPrepare();
		foreach (ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot in this.m_WoundSlots)
		{
			if (replicatedWoundSlot.m_LocalPlayerInjury != null && !PlayerInjuryModule.Get().m_Injuries.Contains(replicatedWoundSlot.m_LocalPlayerInjury))
			{
				replicatedWoundSlot.m_LocalPlayerInjury = null;
				replicatedWoundSlot.ClearInjury();
				this.ReplSetDirty();
			}
		}
		foreach (Injury injury in PlayerInjuryModule.Get().m_Injuries)
		{
			if (!this.m_PreviousInjuries.ContainsKey(injury))
			{
				int slotIdUsingLocalPlayerSlot = this.GetSlotIdUsingLocalPlayerSlot(injury.m_Slot.gameObject);
				this.m_PreviousInjuries.Add(injury, this.m_WoundSlots[slotIdUsingLocalPlayerSlot]);
				this.m_WoundSlots[slotIdUsingLocalPlayerSlot].m_LocalPlayerInjury = injury;
				this.m_WoundSlots[slotIdUsingLocalPlayerSlot].SetInjury(injury.m_Type, injury.m_State);
				this.ReplSetDirty();
			}
			else
			{
				ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot2 = this.m_PreviousInjuries[injury];
				if (!replicatedWoundSlot2.HasInjury(injury))
				{
					replicatedWoundSlot2.m_LocalPlayerInjury = injury;
					replicatedWoundSlot2.SetInjury(injury.m_Type, injury.m_State);
					this.ReplSetDirty();
				}
			}
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		base.OnReplicationSerialize(writer, initial_state);
		using (P2PNetworkWriterSeekHelper p2PNetworkWriterSeekHelper = new P2PNetworkWriterSeekHelper(writer))
		{
			writer.WritePackedUInt32(0u);
			uint num = 0u;
			foreach (ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot in this.m_WoundSlots)
			{
				if (replicatedWoundSlot.IsInjury())
				{
					writer.Write((int)replicatedWoundSlot.m_InjuryData.type);
					writer.Write((int)replicatedWoundSlot.m_InjuryData.state);
					writer.Write(replicatedWoundSlot.m_SlotIndex);
					num += 1u;
				}
			}
			p2PNetworkWriterSeekHelper.SeekToStoredPos();
			writer.WritePackedUInt32(num);
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		base.OnReplicationDeserialize(reader, initial_state);
		this.Initialize();
		foreach (ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot in this.m_WoundSlots)
		{
			replicatedWoundSlot.m_IsValid = false;
		}
		int num = (int)reader.ReadPackedUInt32();
		for (int i = 0; i < num; i++)
		{
			InjuryType injury_type = (InjuryType)reader.ReadInt32();
			InjuryState injury_state = (InjuryState)reader.ReadInt32();
			int num2 = reader.ReadInt32();
			this.m_WoundSlots[num2].m_IsValid = true;
			if (num2 >= 0 && num2 < this.m_WoundSlots.Count)
			{
				this.m_WoundSlots[num2].SetInjury(injury_type, injury_state);
			}
		}
	}

	public override void OnReplicationResolve()
	{
		base.OnReplicationResolve();
		foreach (ReplicatedPlayerInjuries.ReplicatedWoundSlot replicatedWoundSlot in this.m_WoundSlots)
		{
			if (!replicatedWoundSlot.m_IsValid)
			{
				replicatedWoundSlot.ClearInjury();
			}
		}
	}

	private bool m_Initialized;

	private List<ReplicatedPlayerInjuries.ReplicatedWoundSlot> m_WoundSlots;

	private Dictionary<Injury, ReplicatedPlayerInjuries.ReplicatedWoundSlot> m_PreviousInjuries = new Dictionary<Injury, ReplicatedPlayerInjuries.ReplicatedWoundSlot>();

	private class ReplicatedWoundSlot
	{
		public ReplicatedWoundSlot()
		{
			this.m_InjuryData.Clear();
		}

		public void SetInjury(InjuryType injury_type, InjuryState injury_state)
		{
			if (this.HasInjury(injury_type, injury_state))
			{
				return;
			}
			this.m_InjuryData = new ReplicatedPlayerInjuries.SReplicatedInjuryData(injury_type, injury_state);
			if (injury_type != InjuryType.None)
			{
				this.m_ReplicatedPlayerSlot.SetActive(true);
			}
			Injury.SetWoundMaterial(this.m_ReplicatedPlayerSlot, injury_state, injury_type);
			if (this.m_ReplicatedPlayerAdditionalMeshes != null)
			{
				for (int i = 0; i < this.m_ReplicatedPlayerAdditionalMeshes.Count; i++)
				{
					Injury.SetWoundMaterial(this.m_ReplicatedPlayerAdditionalMeshes[i], injury_state, injury_type);
				}
			}
			if (injury_type == InjuryType.Worm)
			{
				GameObject gameObject = Resources.Load("Prefabs/Items/Item/botfly") as GameObject;
				if (gameObject == null)
				{
					gameObject = (Resources.Load("Prefabs/TempPrefabs/Item/Item/botfly") as GameObject);
				}
				if (gameObject == null)
				{
					return;
				}
				this.m_AdditionalObjectInSlot = UnityEngine.Object.Instantiate<GameObject>(gameObject, this.m_ReplicatedPlayerSlot.transform.position, this.m_ReplicatedPlayerSlot.transform.rotation);
				Item component = this.m_AdditionalObjectInSlot.GetComponent<Item>();
				if (component != null)
				{
					component.m_CanSaveNotTriggered = false;
				}
				this.m_AdditionalObjectInSlot.layer = Player.Get().gameObject.layer;
				this.m_AdditionalObjectInSlot.transform.parent = this.m_ReplicatedPlayerSlot.transform;
				this.m_AdditionalObjectInSlot.GetComponent<Parasite>().m_InBody = true;
				Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(this.m_AdditionalObjectInSlot);
				for (int j = 0; j < componentsDeepChild.Length; j++)
				{
					componentsDeepChild[j].gameObject.layer = LayerMask.NameToLayer("Player");
				}
				this.m_AdditionalObjectInSlot.SetActive(false);
				return;
			}
			else
			{
				if (injury_type != InjuryType.Leech)
				{
					if (injury_type == InjuryType.VenomBite || injury_type == InjuryType.SnakeBite || injury_type == InjuryType.Laceration || injury_type == InjuryType.LacerationCat || injury_type == InjuryType.Rash || injury_type == InjuryType.SmallWoundAbrassion || injury_type == InjuryType.SmallWoundScratch || injury_type == InjuryType.WormHole)
					{
						this.m_ReplicatedPlayerSlot.SetActive(true);
						if (injury_type == InjuryType.Rash && this.m_ReplicatedPlayerAdditionalMeshes != null)
						{
							for (int k = 0; k < this.m_ReplicatedPlayerAdditionalMeshes.Count; k++)
							{
								this.m_ReplicatedPlayerAdditionalMeshes[k].SetActive(true);
							}
							return;
						}
					}
					else
					{
						Item component2 = this.m_ReplicatedPlayerSlot.GetComponent<Item>();
						if (component2 != null && component2.m_Info == null)
						{
							UnityEngine.Object.Destroy(component2.gameObject);
						}
						if (component2 != null && component2.m_Info != null && component2.m_Info.m_ID == ItemID.Leech)
						{
							UnityEngine.Object.Destroy(component2.gameObject);
						}
						else if (component2 == null || (component2 != null && component2.m_Info != null && component2.m_Info.m_ID != ItemID.Leech && component2.m_Info.m_ID != ItemID.Botfly))
						{
							this.m_ReplicatedPlayerSlot.SetActive(false);
						}
						if (this.m_ReplicatedPlayerAdditionalMeshes != null)
						{
							for (int l = 0; l < this.m_ReplicatedPlayerAdditionalMeshes.Count; l++)
							{
								this.m_ReplicatedPlayerAdditionalMeshes[l].SetActive(false);
							}
						}
						if (this.m_AdditionalObjectInSlot)
						{
							UnityEngine.Object.Destroy(this.m_AdditionalObjectInSlot);
							this.m_AdditionalObjectInSlot = null;
						}
					}
					return;
				}
				GameObject gameObject2 = Resources.Load("Prefabs/Items/Item/Leech") as GameObject;
				if (gameObject2 == null)
				{
					gameObject2 = (Resources.Load("Prefabs/TempPrefabs/Items/Item/Leech") as GameObject);
				}
				if (gameObject2 == null)
				{
					return;
				}
				this.m_AdditionalObjectInSlot = UnityEngine.Object.Instantiate<GameObject>(gameObject2, this.m_ReplicatedPlayerSlot.transform.position, this.m_ReplicatedPlayerSlot.transform.rotation);
				Item component3 = this.m_AdditionalObjectInSlot.GetComponent<Item>();
				if (component3 != null)
				{
					component3.m_CanSaveNotTriggered = false;
				}
				this.m_AdditionalObjectInSlot.layer = Player.Get().gameObject.layer;
				this.m_AdditionalObjectInSlot.GetComponent<Rigidbody>().useGravity = false;
				this.m_AdditionalObjectInSlot.transform.parent = this.m_ReplicatedPlayerSlot.transform;
				this.m_AdditionalObjectInSlot.GetComponent<Animator>().speed = UnityEngine.Random.Range(0.8f, 1.2f);
				this.m_AdditionalObjectInSlot.GetComponent<Parasite>().m_InBody = true;
				this.m_AdditionalObjectInSlot.GetComponent<Rigidbody>().detectCollisions = false;
				this.m_AdditionalObjectInSlot.GetComponent<Animator>().SetBool("Drink", true);
				Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(this.m_AdditionalObjectInSlot);
				for (int j = 0; j < componentsDeepChild.Length; j++)
				{
					componentsDeepChild[j].gameObject.layer = LayerMask.NameToLayer("Player");
				}
				return;
			}
		}

		public bool IsInjury()
		{
			return this.m_InjuryData.type != InjuryType.None;
		}

		public bool HasInjury(Injury injury)
		{
			return injury.m_Type == this.m_InjuryData.type && injury.m_State == this.m_InjuryData.state;
		}

		public bool HasInjury(InjuryType injury_type, InjuryState injury_state)
		{
			return this.m_InjuryData.type == injury_type && this.m_InjuryData.state == injury_state;
		}

		public void ClearInjury()
		{
			this.SetInjury(InjuryType.None, InjuryState.None);
		}

		public Injury m_LocalPlayerInjury;

		public GameObject m_LocalPlayerSlot;

		public bool m_IsValid;

		public int m_SlotIndex;

		public GameObject m_ReplicatedPlayerSlot;

		public ReplicatedPlayerInjuries.SReplicatedInjuryData m_InjuryData;

		public List<GameObject> m_ReplicatedPlayerAdditionalMeshes;

		public GameObject m_AdditionalObjectInSlot;
	}

	private struct SReplicatedInjuryData
	{
		public SReplicatedInjuryData(InjuryType type, InjuryState state)
		{
			this.type = type;
			this.state = state;
		}

		public bool IsEqual(Injury injury)
		{
			return this.type == injury.m_Type && this.state == injury.m_State;
		}

		public bool IsEqual(ref ReplicatedPlayerInjuries.SReplicatedInjuryData data)
		{
			return this.type == data.type && this.state == data.state;
		}

		public void Clear()
		{
			this.type = InjuryType.None;
			this.state = InjuryState.None;
		}

		public InjuryType type;

		public InjuryState state;
	}
}
