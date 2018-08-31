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
			BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
			Vector3 size = component.size;
			this.m_MinDistance = AIManager.Get().m_AIActivationRange * 0.7f;
			this.m_MaxDistance = AIManager.Get().m_AIActivationRange * 0.9f;
			int num = Mathf.Max(this.m_DayCount, this.m_NightCount);
			for (int i = 0; i < num; i++)
			{
				AISpawnData aispawnData = new AISpawnData();
				aispawnData.m_Position = this.GetSpawnPosition();
				if (aispawnData.m_Position != Vector3.zero)
				{
					this.m_AIDatas.Add(aispawnData);
				}
			}
			AIManager.Get().RegisterSpawner(this);
		}

		private void OnDestroy()
		{
			for (int i = 0; i < this.m_AIDatas.Count; i++)
			{
				if (this.m_AIDatas[i].m_AI)
				{
					this.m_AIDatas[i].m_AI.m_Spawner = null;
				}
			}
			AIManager.Get().UnregisterSpawner(this);
		}

		public bool IsInside(Vector3 position)
		{
			return this.m_Bounds.Contains(position);
		}

		private bool CanSpawn()
		{
			if (AIManager.Get() == null)
			{
				return false;
			}
			if (this.m_ID == AI.AIID.Jaguar && !BalanceSystem.Get().CanSpawnJaguar())
			{
				return false;
			}
			int num = (!MainLevel.Instance.IsNight()) ? this.m_DayCount : this.m_NightCount;
			return this.m_AICount < num && (!this.m_SpawnersGroup || this.m_SpawnersGroup.CanSpawnAI());
		}

		private bool CanSpawn(AISpawnData data)
		{
			if (data.m_AI != null)
			{
				return false;
			}
			if (data.m_TimeToNextSpawn > 0f)
			{
				return false;
			}
			float num = Vector3.Distance(Player.Get().transform.position, data.m_Position);
			return num <= this.m_MaxDistance && num >= this.m_MinDistance;
		}

		private void Update()
		{
			for (int i = 0; i < this.m_AIDatas.Count; i++)
			{
				this.m_AIDatas[i].m_TimeToNextSpawn -= Time.deltaTime;
			}
			if (!this.CanSpawn())
			{
				return;
			}
			for (int j = 0; j < this.m_AIDatas.Count; j++)
			{
				if (this.CanSpawn(this.m_AIDatas[j]))
				{
					this.SpawnObject(this.m_AIDatas[j]);
				}
			}
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
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(randomPositionInside, out navMeshHit, 2f, AIManager.s_WalkableAreaMask) && this.IsPoisitionAvailable(navMeshHit.position))
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
			float num = Mathf.Max(this.m_AIBounds.size.x, this.m_AIBounds.size.z);
			for (int i = 0; i < this.m_AIDatas.Count; i++)
			{
				if (pos.Distance(this.m_AIDatas[i].m_Position) < num)
				{
					return false;
				}
			}
			Vector3 center = pos + Vector3.up * (this.m_AIBounds.size.y / 2f);
			Collider[] array = Physics.OverlapBox(center, this.m_AIBounds.size, Quaternion.identity);
			for (int j = 0; j < array.Length; j++)
			{
				if (!array[j].isTrigger)
				{
					if (!(array[j].gameObject.tag == "Sectr_trigger"))
					{
						if (!array[j].GetComponent<TerrainCollider>())
						{
							if (!(array[j].gameObject == base.gameObject))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		private void SpawnObject(AISpawnData data)
		{
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-180, 180), 0f);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_Prefab, data.m_Position, rotation);
			gameObject.name = string.Format("{0} {1}", this.m_Prefab.name, base.transform.childCount);
			Vector3 vector = gameObject.transform.localScale;
			vector *= UnityEngine.Random.Range(this.m_MinScale, this.m_MaxScale);
			gameObject.transform.localScale = vector;
			AI component = gameObject.GetComponent<AI>();
			component.m_Spawner = this;
			data.m_AI = component;
			if (this.m_SpawnersGroup)
			{
				this.m_SpawnersGroup.OnSpawnAI(component);
			}
			this.m_AICount++;
		}

		public void OnDestroyAI(AI ai)
		{
			if (this.m_SpawnersGroup)
			{
				this.m_SpawnersGroup.OnDestroyAI(ai);
			}
			for (int i = 0; i < this.m_AIDatas.Count; i++)
			{
				AISpawnData aispawnData = this.m_AIDatas[i];
				if (aispawnData.m_AI == ai)
				{
					aispawnData.m_AI = null;
					aispawnData.m_TimeToNextSpawn = ((!ai.IsDead()) ? 0f : this.m_ResetTime);
					break;
				}
			}
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

		private List<AISpawnData> m_AIDatas = new List<AISpawnData>();

		private float m_MinDistance;

		private float m_MaxDistance;

		[HideInInspector]
		public Bounds m_Bounds;

		private int m_AICount;

		[HideInInspector]
		public AISpawnersGroup m_SpawnersGroup;
	}
}
