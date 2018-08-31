using System;
using System.Collections.Generic;
using CJTools;
using UltimateWater;
using UnityEngine;

namespace AIs
{
	public class FishTank : MonoBehaviour, ITriggerThrough
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
			this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
			if (this.m_Prefabs.Count == 0)
			{
				DebugUtils.Assert(false, "[FishTank::CreateFish] Fish Prefab of spawner " + base.name + " is not set!", true, DebugUtils.AssertType.Info);
			}
			base.gameObject.SetActive(false);
			this.m_CurrentFishesCount = this.m_FishCount;
			this.m_Bounds = component.bounds;
			FishTank.s_FishTanks.Add(this);
		}

		private void Start()
		{
			float terrainY = MainLevel.GetTerrainY(base.transform.position);
			if (terrainY >= this.m_BoxCollider.bounds.max.y)
			{
				base.enabled = false;
				DebugUtils.Assert(false, "[FishTank::Start] Fish tank " + base.name + " is under terrain!", true, DebugUtils.AssertType.Info);
				return;
			}
			this.FindWater();
		}

		private void FindWater()
		{
			this.m_Water = Water.FindWater(base.transform.position, 0.1f);
		}

		private void OnDestroy()
		{
			FishTank.s_FishTanks.Remove(this);
		}

		public void ConstantUpdate()
		{
			if (this.m_Empty)
			{
				this.m_EmptyDuration += Time.deltaTime;
			}
			this.UpdateActivity();
		}

		private void UpdateActivity()
		{
			if (Time.time < this.m_NextUpdateActivityTime)
			{
				return;
			}
			float num = 0f;
			bool flag = this.m_Bounds.Contains(Player.Get().transform.position);
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				Vector3 vector = this.m_Bounds.ClosestPoint(Player.Get().transform.position);
				num = vector.Distance(Player.Get().transform.position);
				flag2 = (num <= AIManager.Get().m_FishDeactivationRange);
			}
			if (flag2 && !base.gameObject.activeSelf)
			{
				this.Activate();
				if (this.m_Water == null)
				{
					this.FindWater();
				}
			}
			else if (!flag2 && base.gameObject.activeSelf)
			{
				this.Deactivate();
			}
			this.m_NextUpdateActivityTime = Time.time + CJTools.Math.GetProportionalClamp(0.1f, 5f, num, AIManager.Get().m_FishDeactivationRange, AIManager.Get().m_FishDeactivationRange * 5f);
		}

		private void Reset()
		{
			this.m_CurrentFishesCount = this.m_FishCount;
			this.m_Empty = false;
			this.m_EmptyDuration = 0f;
		}

		private void Activate()
		{
			if (this.m_CurrentFishesCount == 0 && this.m_EmptyDuration > this.m_ResetInterval)
			{
				this.Reset();
			}
			base.gameObject.SetActive(true);
		}

		private void Deactivate()
		{
			base.gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			this.CreateFishes();
		}

		private void OnDisable()
		{
			this.DestroyFishes();
		}

		private void CreateFishes()
		{
			int num = 0;
			while (num < this.m_FishCount && this.m_Fishes.Count < this.m_CurrentFishesCount)
			{
				Vector3 randomPointInTankSpace = this.GetRandomPointInTankSpace(true, 0.5f);
				GameObject gameObject = (this.m_Prefabs.Count <= 0) ? null : this.m_Prefabs[UnityEngine.Random.Range(0, this.m_Prefabs.Count)];
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, randomPointInTankSpace, Quaternion.identity);
				gameObject2.transform.localScale *= UnityEngine.Random.Range(this.m_MinScale, this.m_MaxScale);
				Fish component = gameObject2.GetComponent<Fish>();
				component.enabled = true;
				component.SetTank(this);
				component.SetPrefab(gameObject);
				this.m_Fishes.Add(component);
				Collider component2 = component.GetComponent<Collider>();
				Physics.IgnoreCollision(Player.Get().GetComponent<Collider>(), component2, true);
				num++;
			}
		}

		private void DestroyFishes()
		{
			foreach (Fish fish in this.m_Fishes)
			{
				UnityEngine.Object.Destroy(fish.gameObject);
			}
			this.m_Fishes.Clear();
		}

		private bool IsTankUnderTerrain()
		{
			float terrainY = MainLevel.GetTerrainY(base.transform.position);
			return terrainY >= base.transform.position.y;
		}

		public void StartFishing(FishingRod fishing_rod)
		{
			this.m_FishingRod = fishing_rod;
		}

		public void StopFishing()
		{
			if (this.m_FishAttractedByHook)
			{
				this.m_FishAttractedByHook.SetHook(null);
				this.m_FishAttractedByHook = null;
				this.m_LastFishAttractedByHook = null;
			}
			this.m_FishingRod = null;
		}

		private void Update()
		{
			this.UpdateFishAttractedByHook();
		}

		private void UpdateFishAttractedByHook()
		{
			if (!this.m_FishingRod || !this.m_FishingRod.m_Hook || !this.m_FishingRod.m_Hook.m_Bait)
			{
				this.ResetFishAttractedByHook(this.m_FishAttractedByHook);
				return;
			}
			if (this.m_FishAttractedByHook)
			{
				return;
			}
			if (this.m_Fishes.Count <= 0)
			{
				return;
			}
			float num = 0f;
			List<Fish> list = new List<Fish>();
			this.chance_map.Clear();
			foreach (Fish fish in this.m_Fishes)
			{
				if (!(fish == this.m_LastFishAttractedByHook))
				{
					if (fish.m_Params.m_Baits.Contains(this.m_FishingRod.m_Hook.m_Bait.m_Info.m_ID))
					{
						if (fish.m_Params.m_FishingRods.Contains(this.m_FishingRod.m_FishingRodItem.m_Info.m_ID))
						{
							list.Add(fish);
							if (!this.chance_map.ContainsKey((int)fish.m_ID))
							{
								this.chance_map.Add((int)fish.m_ID, fish.m_Params.m_BitingChance);
								num += fish.m_Params.m_BitingChance;
							}
						}
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			AI.AIID aiid = AI.AIID.None;
			float num2 = UnityEngine.Random.Range(0f, num);
			num = 0f;
			using (Dictionary<int, float>.KeyCollection.Enumerator enumerator2 = this.chance_map.Keys.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					AI.AIID aiid2 = (AI.AIID)enumerator2.Current;
					if (num2 >= num && num2 <= num + this.chance_map[(int)aiid2])
					{
						aiid = aiid2;
						break;
					}
					num += this.chance_map[(int)aiid2];
				}
			}
			int i = 0;
			while (i < list.Count)
			{
				if (list[i].m_ID != aiid)
				{
					list.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			this.m_FishAttractedByHook = list[UnityEngine.Random.Range(0, list.Count)];
			this.m_FishAttractedByHook.SetHook(this.m_FishingRod.m_Hook);
		}

		public void ResetFishAttractedByHook(Fish fish)
		{
			if (this.m_FishAttractedByHook == fish)
			{
				this.m_FishAttractedByHook = null;
				this.m_LastFishAttractedByHook = fish;
			}
		}

		public int GetFishesCount()
		{
			return this.m_Fishes.Count;
		}

		public bool IsPointInside(Vector3 point)
		{
			return this.m_BoxCollider.bounds.Contains(point);
		}

		public Fish GetFish(int index)
		{
			if (index < 0 || index > this.GetFishesCount())
			{
				return null;
			}
			return this.m_Fishes[index];
		}

		public void RemoveFish(Fish fish)
		{
			this.ResetFishAttractedByHook(fish);
			this.m_Fishes.Remove(fish);
			this.m_CurrentFishesCount--;
			if (this.m_CurrentFishesCount == 0)
			{
				this.m_Empty = true;
				this.m_EmptyDuration = 0f;
			}
		}

		public void OnDestroyFish(Fish fish)
		{
			this.ResetFishAttractedByHook(fish);
			this.m_Fishes.Remove(fish);
		}

		public Vector3 GetRandomPointInTankSpace(bool check_terrain = true, float range = 0.5f)
		{
			Vector3 vector = Vector3.zero;
			int num = 10000;
			bool flag = false;
			int num2 = 0;
			while (num2 < num && !flag)
			{
				vector.x = UnityEngine.Random.Range(-range, range);
				vector.y = UnityEngine.Random.Range(-range, range);
				vector.z = UnityEngine.Random.Range(-range, range);
				vector = base.gameObject.transform.TransformPoint(vector);
				if (check_terrain)
				{
					float terrainY = MainLevel.GetTerrainY(vector);
					flag = (terrainY < vector.y);
				}
				else
				{
					flag = true;
				}
				num2++;
			}
			if (!flag)
			{
				return base.transform.position;
			}
			return vector;
		}

		public void Save(int index)
		{
			SaveGame.SaveVal("AIFTEmpty" + index, this.m_Empty);
			SaveGame.SaveVal("AIFTEmptyDur" + index, this.m_EmptyDuration);
			SaveGame.SaveVal("AIFTFishCount" + index, this.m_CurrentFishesCount);
			SaveGame.SaveVal("AIFTActive" + index, base.gameObject.activeSelf);
		}

		public void Load(int index)
		{
			this.DestroyFishes();
			this.m_Empty = SaveGame.LoadBVal("AIFTEmpty" + index);
			this.m_EmptyDuration = SaveGame.LoadFVal("AIFTEmptyDur" + index);
			this.m_CurrentFishesCount = SaveGame.LoadIVal("AIFTFishCount" + index);
			base.gameObject.SetActive(SaveGame.LoadBVal("AIFTActive" + index));
			if (base.gameObject.activeSelf)
			{
				this.CreateFishes();
			}
		}

		public List<GameObject> m_Prefabs = new List<GameObject>();

		private List<Fish> m_Fishes = new List<Fish>();

		public int m_FishCount = 1;

		private int m_CurrentFishesCount;

		public float m_MinScale = 1f;

		public float m_MaxScale = 1f;

		private FishingRod m_FishingRod;

		private Fish m_FishAttractedByHook;

		private Fish m_LastFishAttractedByHook;

		public float m_MaxSpeed = 2f;

		public float m_RotationSpeed = 2f;

		[HideInInspector]
		public BoxCollider m_BoxCollider;

		private float m_NextUpdateActivityTime;

		public float m_ResetInterval;

		private float m_EmptyDuration;

		private bool m_Empty;

		[HideInInspector]
		public Water m_Water;

		private Bounds m_Bounds;

		public static List<FishTank> s_FishTanks = new List<FishTank>();

		private Dictionary<int, float> chance_map = new Dictionary<int, float>();
	}
}
