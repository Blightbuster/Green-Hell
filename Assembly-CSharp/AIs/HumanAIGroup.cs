using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class HumanAIGroup : MonoBehaviour
	{
		public virtual void Initialize()
		{
			this.InitObjects();
			UnityEngine.Object.Destroy(base.gameObject.GetComponent<MeshRenderer>());
			UnityEngine.Object.Destroy(base.gameObject.GetComponent<MeshFilter>());
			HumanAIGroup.s_AIGroups.Add(this);
		}

		protected virtual void SetupState()
		{
			this.SetState(HumanAIGroup.State.Calm);
		}

		protected virtual void InitObjects()
		{
			List<GameObject> list = new List<GameObject>();
			foreach (HumanAI humanAI in base.gameObject.GetComponentsInChildren<HumanAI>())
			{
				if (humanAI.transform.parent != base.transform && !list.Contains(humanAI.transform.parent.gameObject))
				{
					list.Add(humanAI.transform.parent.gameObject);
				}
				humanAI.transform.parent = base.transform;
				this.AddAI(humanAI);
				humanAI.gameObject.SetActive(this.m_Active);
			}
			foreach (GameObject gameObject in list)
			{
				gameObject.transform.parent = null;
				UnityEngine.Object.Destroy(gameObject);
			}
			for (int j = 0; j < base.transform.childCount; j++)
			{
				GameObject gameObject2 = base.transform.GetChild(j).gameObject;
				if (!this.m_AllObjects.Contains(gameObject2))
				{
					Item component = gameObject2.GetComponent<Item>();
					if (component)
					{
						component.m_Group = this;
						component.ItemsManagerUnregister();
					}
					gameObject2.SetActive(this.m_Active);
					this.m_AllObjects.Add(gameObject2);
				}
			}
			if (this.m_Members.Count == 0)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				DebugUtils.Assert("HumanAIGroup does not contains any AI!", true, DebugUtils.AssertType.Info);
			}
		}

		public void SetupConstructionsToDestroy(FirecampGroup group)
		{
			if (group != null)
			{
				group.GetAllConstructions(ref this.m_ConstructionsToDestroy);
			}
		}

		private void OnDestroy()
		{
			HumanAIGroup.s_AIGroups.Remove(this);
		}

		public virtual bool IsWave()
		{
			return false;
		}

		public virtual bool IsPatrol()
		{
			return false;
		}

		public virtual bool IsDestroyers()
		{
			return false;
		}

		public void OnAIDie(HumanAI ai)
		{
			this.RemovedAI(ai, false);
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.m_EnemyModule.SetEnemy(Player.Get());
			}
		}

		public virtual void AddAI(HumanAI ai)
		{
			ai.gameObject.SetActive(true);
			ai.m_Group = this;
			this.m_Members.Add(ai);
			this.m_AllObjects.Add(ai.gameObject);
		}

		public void RemovedAI(HumanAI ai, bool from_destroy)
		{
			this.m_Members.Remove(ai);
			this.m_AllObjects.Remove(ai.gameObject);
			ai.m_Group = null;
			if (!ai.m_IsBeingDestroyed)
			{
				ai.transform.parent = null;
			}
			if (this.m_Members.Count == 0)
			{
				this.Deactivate(from_destroy);
			}
		}

		public void RemoveObject(GameObject obj)
		{
			this.m_AllObjects.Remove(obj);
		}

		public void Activate()
		{
			base.gameObject.SetActive(true);
			foreach (GameObject gameObject in this.m_AllObjects)
			{
				gameObject.SetActive(true);
			}
			if (!this.m_ChallengeGroup && this.m_FromBalance)
			{
				int currentGroupMembersCount = EnemyAISpawnManager.Get().GetCurrentGroupMembersCount();
				while (this.m_Members.Count > currentGroupMembersCount)
				{
					HumanAI humanAI = this.m_Members[UnityEngine.Random.Range(0, this.m_Members.Count)];
					this.RemovedAI(humanAI, false);
					UnityEngine.Object.Destroy(humanAI.gameObject);
				}
			}
			this.m_Active = true;
			this.OnActivate();
		}

		protected virtual void OnActivate()
		{
			this.SetupState();
		}

		protected virtual void OnDeactivate()
		{
			EnemyAISpawnManager.Get().OnDeactivateGroup(this);
			this.SetState(HumanAIGroup.State.None);
		}

		protected void Deactivate(bool from_destroy)
		{
			this.OnDeactivate();
			if (this.m_Members.Count == 0)
			{
				if (!from_destroy)
				{
					foreach (GameObject gameObject in this.m_AllObjects)
					{
						gameObject.transform.parent = null;
					}
				}
				this.m_AllObjects.Clear();
			}
			else
			{
				foreach (GameObject gameObject2 in this.m_AllObjects)
				{
					gameObject2.SetActive(false);
				}
			}
			base.gameObject.SetActive(false);
			this.m_Active = false;
		}

		public HumanAI GetClosestMember(out float distance)
		{
			Vector3 position = Player.Get().transform.position;
			HumanAI result = null;
			distance = float.MaxValue;
			foreach (HumanAI humanAI in this.m_Members)
			{
				float num = humanAI.transform.position.Distance(position);
				if (num < distance)
				{
					result = humanAI;
					distance = num;
				}
			}
			return result;
		}

		public Construction GetClosestConstructionToDestroy(Vector3 position)
		{
			float num = float.MaxValue;
			Construction result = null;
			foreach (Construction construction in this.m_ConstructionsToDestroy)
			{
				if (!(construction == null) && !ItemInfo.IsTrap(construction.GetInfoID()))
				{
					float num2 = construction.transform.position.Distance(position);
					if (num2 < num)
					{
						num = num2;
						result = construction;
					}
				}
			}
			return result;
		}

		public void StartWave(FirecampGroup group)
		{
			this.SetState(HumanAIGroup.State.StartWave);
			this.SetupConstructionsToDestroy(group);
		}

		protected virtual void Update()
		{
			if (Player.Get().IsDead())
			{
				return;
			}
			this.UpdateActivity();
			if (!this.m_Active)
			{
				return;
			}
			this.UpdateState();
			this.UpdateConstructionsToDestroy();
		}

		protected virtual void UpdateActivity()
		{
			if (this.m_ForceActive)
			{
				return;
			}
			if (this.m_ChallengeGroup)
			{
				return;
			}
			float maxValue = float.MaxValue;
			if (this.GetClosestMember(out maxValue) && maxValue >= EnemyAISpawnManager.s_DeactivationDist)
			{
				this.Deactivate(false);
			}
		}

		protected void SetState(HumanAIGroup.State state)
		{
			if (this.m_State == state)
			{
				return;
			}
			this.OnExitState();
			this.m_State = state;
			this.OnEnterState();
		}

		protected virtual void OnExitState()
		{
			HumanAIGroup.State state = this.m_State;
			if (state == HumanAIGroup.State.Calm)
			{
				this.OnExitCalmState();
			}
		}

		protected virtual void OnEnterState()
		{
			switch (this.m_State)
			{
			case HumanAIGroup.State.None:
				this.OnEnterNoneState();
				return;
			case HumanAIGroup.State.Calm:
				this.OnEnterCalmState();
				return;
			case HumanAIGroup.State.Upset:
				this.OnEnterUpsetState();
				return;
			case HumanAIGroup.State.StartWave:
				this.OnEnterStartWaveState();
				return;
			case HumanAIGroup.State.Attack:
				this.OnEnterAttackState();
				return;
			default:
				return;
			}
		}

		protected virtual void OnEnterNoneState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.None);
			}
		}

		protected virtual void OnEnterCalmState()
		{
			this.m_LastCalmClip = null;
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.Rest);
			}
		}

		private void OnEnterUpsetState()
		{
			int index = UnityEngine.Random.Range(0, this.m_Members.Count);
			this.m_Members[index].SetState(HumanAI.State.Upset);
		}

		private void OnEnterAttackState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.Attack);
			}
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.AISight);
		}

		private void OnEnterStartWaveState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.StartWave);
			}
		}

		private bool ShouldSetCalmState()
		{
			if (Player.Get().IsInSafeZone())
			{
				return true;
			}
			if (this.m_State != HumanAIGroup.State.Attack)
			{
				bool result = true;
				using (List<HumanAI>.Enumerator enumerator = this.m_Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.GetState() != HumanAI.State.Rest)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
			if (this.m_ChallengeGroup)
			{
				using (List<HumanAI>.Enumerator enumerator = this.m_Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.m_SightModule.m_VisiblePlayers.Count > 0)
						{
							return false;
						}
					}
				}
				using (List<HumanAI>.Enumerator enumerator = this.m_Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.m_EnemySenseModule.m_Enemies.Count > 0)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool ShouldSetUpsetState()
		{
			if (this.m_State == HumanAIGroup.State.Attack)
			{
				return false;
			}
			using (List<HumanAI>.Enumerator enumerator = this.m_Members.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_HearingModule.m_Noise != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool ShouldSetAttackState()
		{
			if (this.m_State == HumanAIGroup.State.Attack)
			{
				return false;
			}
			using (List<HumanAI>.Enumerator enumerator = this.m_Members.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_EnemyModule.m_Enemy)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual void UpdateState()
		{
			if (this.ShouldSetAttackState())
			{
				this.SetState(HumanAIGroup.State.Attack);
			}
			else if (this.ShouldSetUpsetState())
			{
				this.SetState(HumanAIGroup.State.Upset);
			}
			else if (this.ShouldSetCalmState())
			{
				this.SetState(HumanAIGroup.State.Calm);
			}
			this.UpdateSounds();
		}

		public void Save(int index)
		{
			SaveGame.SaveVal("HAGroupCount" + index, this.m_Members.Count);
			for (int i = 0; i < this.m_Members.Count; i++)
			{
				SaveGame.SaveVal("HAGroupID" + i.ToString() + "_" + index.ToString(), this.m_Members[i].m_ID.ToString());
				SaveGame.SaveVal("HAGroupPos" + i.ToString() + "_" + index.ToString(), this.m_Members[i].transform.position);
				SaveGame.SaveVal("HAGroupRot" + i.ToString() + "_" + index.ToString(), this.m_Members[i].transform.rotation);
			}
		}

		public void Load(int index)
		{
			for (int i = 0; i < this.m_Members.Count; i++)
			{
				if (this.m_Members[i] != null)
				{
					this.m_AllObjects.Remove(this.m_Members[i].gameObject);
					UnityEngine.Object.Destroy(this.m_Members[i].gameObject);
				}
			}
			this.m_Members.Clear();
			int num = SaveGame.LoadIVal("HAGroupCount" + index);
			for (int j = 0; j < num; j++)
			{
				AI.AIID aiid = (AI.AIID)Enum.Parse(typeof(AI.AIID), SaveGame.LoadSVal("HAGroupID" + j.ToString() + "_" + index.ToString()));
				Vector3 position = SaveGame.LoadV3Val("HAGroupPos" + j.ToString() + "_" + index.ToString());
				Quaternion rotation = SaveGame.LoadQVal("HAGroupRot" + j.ToString() + "_" + index.ToString());
				HumanAI component = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab(aiid.ToString()), position, rotation, base.transform).GetComponent<HumanAI>();
				this.AddAI(component);
			}
			this.Deactivate(false);
		}

		private void UpdateSounds()
		{
			HumanAIGroup.State state = this.m_State;
			if (state == HumanAIGroup.State.Calm)
			{
				this.UpdateCalmSound();
			}
		}

		private void OnExitCalmState()
		{
			for (int i = 0; i < this.m_Members.Count; i++)
			{
				this.m_Members[i].m_HumanAISoundModule.StopSound(this.m_LastCalmClip);
			}
		}

		protected virtual void UpdateCalmSound()
		{
			if (this.m_Members.Count == 0)
			{
				return;
			}
			if (Time.time >= this.m_NextCalmSoundTime)
			{
				HumanAI humanAI = this.m_Members[UnityEngine.Random.Range(0, this.m_Members.Count)];
				this.m_LastCalmClip = humanAI.m_HumanAISoundModule.PlaySound(HumanAISoundModule.SoundType.Sing);
				if (this.m_LastCalmClip)
				{
					this.m_NextCalmSoundTime = Time.time + this.m_LastCalmClip.length + UnityEngine.Random.Range(this.m_MinCalmSoundInterval, this.m_MaxCalmSoundInterval);
				}
			}
		}

		private void UpdateConstructionsToDestroy()
		{
			if (this.m_ConstructionsToDestroy.Count == 0)
			{
				return;
			}
			foreach (HumanAI humanAI in this.m_Members)
			{
				if (humanAI.m_SelectedConstruction)
				{
					if (humanAI.m_EnemyModule.m_Enemy && humanAI.m_EnemyModule.m_Enemy.transform.position.Distance(humanAI.transform.position) < 6f)
					{
						humanAI.m_SelectedConstruction = null;
					}
				}
				else
				{
					Being enemy = humanAI.m_EnemyModule.m_Enemy;
					if (!enemy || enemy.transform.position.Distance(humanAI.transform.position) > 15f)
					{
						humanAI.m_SelectedConstruction = this.GetClosestConstructionToDestroy(humanAI.transform.position);
					}
				}
			}
		}

		[HideInInspector]
		public List<HumanAI> m_Members = new List<HumanAI>();

		protected List<GameObject> m_AllObjects = new List<GameObject>();

		[NonSerialized]
		public HumanAIGroup.State m_State;

		public static List<HumanAIGroup> s_AIGroups = new List<HumanAIGroup>();

		[HideInInspector]
		[NonSerialized]
		public bool m_Active;

		public bool m_ChallengeGroup;

		public bool m_FromBalance = true;

		protected float m_NextCalmSoundTime;

		protected float m_MinCalmSoundInterval = 3f;

		protected float m_MaxCalmSoundInterval = 6f;

		protected AudioClip m_LastCalmClip;

		[HideInInspector]
		public bool m_ForceActive;

		private List<Construction> m_ConstructionsToDestroy = new List<Construction>();

		public enum State
		{
			None,
			Calm,
			Upset,
			StartWave,
			Attack
		}
	}
}
