using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class AISpawner : MonoBehaviour
	{
		private void Awake()
		{
			MeshRenderer component = base.GetComponent<MeshRenderer>();
			if (!component)
			{
				base.enabled = false;
				return;
			}
			component.enabled = false;
			this.m_Bounds = component.bounds;
			this.m_Prefab = GreenHellGame.Instance.GetPrefab(this.m_ID.ToString());
			if (!this.m_Prefab)
			{
				DebugUtils.Assert("Can't find AI prefab - " + this.m_ID.ToString() + ". AIArea will be removed!", true, DebugUtils.AssertType.Info);
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			Collider component2 = base.gameObject.GetComponent<Collider>();
			if (component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			this.m_AIBounds = General.GetComponentDeepChild<SkinnedMeshRenderer>(this.m_Prefab).bounds;
		}

		private void Start()
		{
			Vector3 size = base.gameObject.GetComponent<BoxCollider>().size;
			this.m_MinDistance = AIManager.Get().m_AIActivationRange * 0.7f;
			this.m_MaxDistance = AIManager.Get().m_AIActivationRange * 0.9f;
			for (int i = 0; i < 99; i++)
			{
				Vector3 spawnPosition = this.GetSpawnPosition();
				if (spawnPosition != Vector3.zero)
				{
					this.m_Positions.Add(spawnPosition);
				}
				if (this.m_Positions.Count >= Mathf.Max(this.m_NightCount, this.m_DayCount) * 2)
				{
					break;
				}
			}
			AIManager.Get().RegisterSpawner(this);
		}

		private void OnDestroy()
		{
			AIManager.Get().UnregisterSpawner(this);
		}

		public bool IsInside(Vector3 position)
		{
			return this.m_Bounds.Contains(position);
		}

		private int GetWantedAICount()
		{
			if (!MainLevel.Instance.IsNight())
			{
				return this.m_DayCount - this.m_BlockedCount;
			}
			return this.m_NightCount;
		}

		public void UpdateSpawner()
		{
			this.UpdateBlocked();
			this.UpdateSpawn();
			this.UpdateAICount();
		}

		private void UpdateBlocked()
		{
			if (this.m_BlockedCount > 0)
			{
				this.m_BlockedDuration += Time.deltaTime;
				if (this.m_BlockedDuration >= this.m_ResetTime)
				{
					this.m_BlockedCount--;
					this.m_BlockedDuration = 0f;
				}
			}
		}

		private void UpdateAICount()
		{
			int wantedAICount = this.GetWantedAICount();
			if (this.m_AICount <= wantedAICount)
			{
				return;
			}
			AI ai = null;
			float num = 0f;
			foreach (AI ai2 in this.m_AIs)
			{
				if (ai2.m_InvisibleDuration > num)
				{
					num = ai2.m_InvisibleDuration;
					ai = ai2;
				}
			}
			if (ai && ai.m_InvisibleDuration > 5f)
			{
				UnityEngine.Object.Destroy(ai.gameObject);
			}
		}

		private void UpdateSpawn()
		{
			if (!this.CanSpawn())
			{
				return;
			}
			for (int i = this.m_PositionIndex; i < this.m_Positions.Count; i++)
			{
				float num = Vector3.Distance(Player.Get().transform.position, this.m_Positions[i]);
				if (num < this.m_MaxDistance && num > this.m_MinDistance)
				{
					this.SpawnObject(this.m_Positions[i]);
					this.m_PositionIndex++;
					if (this.m_PositionIndex >= this.m_Positions.Count)
					{
						this.m_PositionIndex = 0;
					}
					return;
				}
			}
		}

		private bool CanSpawn()
		{
			return !(AIManager.Get() == null) && !ScenarioManager.Get().IsDreamOrPreDream() && (!AI.IsCat(this.m_ID) || EnemyAISpawnManager.Get().CanSpawnPredator()) && DifficultySettings.IsAIIDEnabled(this.m_ID) && this.m_AICount < this.GetWantedAICount() && (!this.m_SpawnersGroup || this.m_SpawnersGroup.CanSpawnAI());
		}

		public Vector3 GetRandomPositionInside()
		{
			Vector3 position = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 1f, UnityEngine.Random.Range(-0.5f, 0.5f));
			return base.gameObject.transform.TransformPoint(position);
		}

		private Vector3 GetSpawnPosition()
		{
			int num = 50;
			for (int i = 0; i < num; i++)
			{
				Vector3 randomPositionInside = this.GetRandomPositionInside();
				randomPositionInside.y = MainLevel.GetTerrainY(randomPositionInside);
				if (this.m_Bounds.Contains(randomPositionInside))
				{
					int num2 = AIManager.s_WalkableAreaMask;
					if (this.m_ID == AI.AIID.BlackCaiman || this.m_ID == AI.AIID.Crab)
					{
						num2 |= AIManager.s_WaterAreaMask;
					}
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(randomPositionInside, out navMeshHit, 2f, num2) && this.IsPoisitionAvailable(navMeshHit.position))
					{
						return navMeshHit.position;
					}
				}
			}
			return Vector3.zero;
		}

		private bool IsPoisitionAvailable(Vector3 pos)
		{
			if (pos == Vector3.zero)
			{
				return false;
			}
			int num = Physics.OverlapBoxNonAlloc(pos + Vector3.up * (this.m_AIBounds.size.y / 2f), this.m_AIBounds.size, AISpawner.s_OverlapCollidersTmp, Quaternion.identity);
			for (int i = 0; i < num; i++)
			{
				if (!AISpawner.s_OverlapCollidersTmp[i].isTrigger && !AISpawner.s_OverlapCollidersTmp[i].GetComponent<TerrainCollider>() && !(AISpawner.s_OverlapCollidersTmp[i].gameObject == base.gameObject))
				{
					return false;
				}
			}
			return true;
		}

		private void SpawnObject(Vector3 position)
		{
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-180, 180), 0f);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_Prefab, position, rotation);
			gameObject.name = string.Format("{0} {1}", this.m_Prefab.name, base.transform.childCount);
			Vector3 vector = gameObject.transform.localScale;
			vector *= UnityEngine.Random.Range(this.m_MinScale, this.m_MaxScale);
			gameObject.transform.localScale = vector;
			AI component = gameObject.GetComponent<AI>();
			component.m_Spawner = this;
			if (this.m_SpawnersGroup)
			{
				this.m_SpawnersGroup.OnSpawnAI(component);
			}
			this.m_AIs.Add(component);
			this.m_AICount++;
		}

		public void OnDestroyAI(AI ai)
		{
			if (this.m_SpawnersGroup)
			{
				this.m_SpawnersGroup.OnDestroyAI(ai);
			}
			if (ai.IsDead())
			{
				this.m_BlockedCount++;
			}
			this.m_AIs.Remove(ai);
			this.m_AICount--;
		}

		private GameObject m_Prefab;

		private Bounds m_AIBounds;

		public AI.AIID m_ID = AI.AIID.None;

		public int m_DayCount;

		public int m_NightCount;

		public float m_ResetTime = 1200f;

		[Range(0.5f, 2f)]
		public float m_MinScale = 0.8f;

		[Range(0.5f, 2f)]
		public float m_MaxScale = 1.2f;

		private List<Vector3> m_Positions = new List<Vector3>();

		private float m_MinDistance;

		private float m_MaxDistance;

		[HideInInspector]
		public Bounds m_Bounds;

		private int m_AICount;

		private int m_BlockedCount;

		private float m_BlockedDuration;

		[HideInInspector]
		public AISpawnersGroup m_SpawnersGroup;

		private List<AI> m_AIs = new List<AI>();

		private int m_PositionIndex;

		private static Collider[] s_OverlapCollidersTmp = new Collider[20];
	}
}
