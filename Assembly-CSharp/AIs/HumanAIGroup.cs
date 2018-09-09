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
			for (int i = 0; i < base.transform.childCount; i++)
			{
				GameObject gameObject = base.transform.GetChild(i).gameObject;
				HumanAI component = gameObject.GetComponent<HumanAI>();
				if (component)
				{
					this.AddAI(component);
				}
				else
				{
					Item component2 = gameObject.GetComponent<Item>();
					if (component2)
					{
						component2.m_Group = this;
						component2.ItemsManagerUnregister();
					}
					this.m_AllObjects.Add(gameObject);
				}
				gameObject.SetActive(this.m_Active);
			}
			if (this.m_Members.Count == 0)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				DebugUtils.Assert("HumanAIGroup does not contains any AI!", true, DebugUtils.AssertType.Info);
			}
		}

		private void OnDestroy()
		{
			HumanAIGroup.s_AIGroups.Remove(this);
			HumanAIGroupManager.Get().OnDestroyGroup(this);
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

		public virtual void AddAI(HumanAI ai)
		{
			ai.gameObject.SetActive(true);
			ai.m_Group = this;
			this.m_Members.Add(ai);
			this.m_AllObjects.Add(ai.gameObject);
		}

		public void RemovedAI(HumanAI ai)
		{
			this.m_Members.Remove(ai);
			this.m_AllObjects.Remove(ai.gameObject);
			ai.m_Group = null;
			ai.transform.parent = null;
			if (this.m_Members.Count == 0)
			{
				this.Deactivate();
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
				int currentHumanAISpawnCount = BalanceSystem.Get().GetCurrentHumanAISpawnCount();
				while (this.m_Members.Count > currentHumanAISpawnCount)
				{
					HumanAI humanAI = this.m_Members[UnityEngine.Random.Range(0, this.m_Members.Count)];
					this.RemovedAI(humanAI);
					UnityEngine.Object.Destroy(humanAI.gameObject);
				}
			}
			this.m_Active = true;
			this.OnActivate();
		}

		protected virtual void OnActivate()
		{
			if (this.IsWave())
			{
				BalanceSystem.Get().OnHumanAIWaveActivated();
			}
			else if (this.m_FromBalance)
			{
				BalanceSystem.Get().OnHumanAIGroupActivated();
			}
			this.SetupState();
		}

		protected virtual void OnDeactivate()
		{
			if (this.IsWave())
			{
				BalanceSystem.Get().OnHumanAIWaveDeactivated(this.m_Members.Count == 0);
			}
			else if (this.m_FromBalance)
			{
				BalanceSystem.Get().OnHumanAIGroupDeactivated(this.m_Members.Count == 0);
			}
			this.SetState(HumanAIGroup.State.None);
		}

		protected void Deactivate()
		{
			this.OnDeactivate();
			if (this.m_Members.Count == 0)
			{
				foreach (GameObject gameObject in this.m_AllObjects)
				{
					gameObject.transform.parent = null;
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
		}

		protected virtual void UpdateActivity()
		{
			if (this.m_ChallengeGroup)
			{
				return;
			}
			float maxValue = float.MaxValue;
			HumanAI closestMember = this.GetClosestMember(out maxValue);
			if (closestMember && maxValue >= HumanAIGroupManager.Get().m_DeactivationDistance)
			{
				this.Deactivate();
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
				break;
			case HumanAIGroup.State.Calm:
				this.OnEnterCalmState();
				break;
			case HumanAIGroup.State.Upset:
				this.OnEnterUpsetState();
				break;
			case HumanAIGroup.State.Attack:
				this.OnEnterAttackState();
				break;
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
			HumanAI humanAI = this.m_Members[index];
			humanAI.SetState(HumanAI.State.Upset);
		}

		private void OnEnterAttackState()
		{
			foreach (HumanAI humanAI in this.m_Members)
			{
				humanAI.SetState(HumanAI.State.Attack);
			}
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.AISight);
		}

		private bool ShouldSetCalmState()
		{
			if (this.m_State != HumanAIGroup.State.Attack)
			{
				bool result = true;
				foreach (HumanAI humanAI in this.m_Members)
				{
					if (humanAI.GetState() != HumanAI.State.Rest)
					{
						result = false;
						break;
					}
				}
				return result;
			}
			if (this.m_ChallengeGroup)
			{
				foreach (HumanAI humanAI2 in this.m_Members)
				{
					if (humanAI2.m_SightModule.m_PlayerVisible)
					{
						return false;
					}
				}
				foreach (HumanAI humanAI3 in this.m_Members)
				{
					if (humanAI3.m_EnemySenseModule.m_Enemy)
					{
						return false;
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
			foreach (HumanAI humanAI in this.m_Members)
			{
				if (humanAI.m_HearingModule.m_Noise != null)
				{
					return true;
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
			foreach (HumanAI humanAI in this.m_Members)
			{
				if (humanAI.m_SightModule.m_PlayerVisible)
				{
					return true;
				}
			}
			foreach (HumanAI humanAI2 in this.m_Members)
			{
				if (humanAI2.m_EnemySenseModule.m_Enemy)
				{
					return true;
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
				GameObject prefab = GreenHellGame.Instance.GetPrefab(aiid.ToString());
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation, base.transform);
				HumanAI component = gameObject.GetComponent<HumanAI>();
				this.AddAI(component);
			}
			this.Deactivate();
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
