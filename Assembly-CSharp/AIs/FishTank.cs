using System;
using System.Collections.Generic;
using CJTools;
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
			this.m_ColliderBounds = this.m_BoxCollider.bounds;
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
			if (MainLevel.GetTerrainY(base.transform.position) >= this.m_BoxCollider.bounds.max.y)
			{
				base.enabled = false;
				DebugUtils.Assert(false, "[FishTank::Start] Fish tank " + base.name + " is under terrain!", true, DebugUtils.AssertType.Info);
				return;
			}
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
			bool flag;
			if (this.m_Bounds.Contains(Player.Get().transform.position))
			{
				flag = true;
			}
			else
			{
				num = this.m_Bounds.ClosestPoint(Player.Get().transform.position).Distance(Player.Get().transform.position);
				flag = (num <= AIManager.Get().m_FishDeactivationRange);
			}
			if (flag && !base.gameObject.activeSelf)
			{
				this.Activate();
			}
			else if (!flag && base.gameObject.activeSelf)
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
				Vector3 randomPointInTankSpace = this.GetRandomPointInTankSpace();
				GameObject gameObject = (this.m_Prefabs.Count > 0) ? this.m_Prefabs[UnityEngine.Random.Range(0, this.m_Prefabs.Count)] : null;
				AI component = gameObject.GetComponent<AI>();
				if (!(component != null) || DifficultySettings.IsAIIDEnabled(component.m_ID))
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, randomPointInTankSpace, Quaternion.identity);
					gameObject2.transform.localScale *= UnityEngine.Random.Range(this.m_MinScale, this.m_MaxScale);
					Fish component2 = gameObject2.GetComponent<Fish>();
					component2.enabled = true;
					component2.SetTank(this);
					component2.SetPrefab(gameObject);
					this.m_Fishes.Add(component2);
					Collider component3 = component2.GetComponent<Collider>();
					Physics.IgnoreCollision(Player.Get().m_Collider, component3, !component2.IsStringray());
				}
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
			return MainLevel.GetTerrainY(base.transform.position) >= base.transform.position.y;
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
				if ((this.m_Fishes.Count <= 1 || !(fish == this.m_LastFishAttractedByHook)) && fish.m_Params.m_Baits.Contains(this.m_FishingRod.m_Hook.m_Bait.m_Info.m_ID) && fish.m_Params.m_FishingRods.Contains(this.m_FishingRod.m_FishingRodItem.m_Info.m_ID) && Vector3.Dot(fish.transform.forward, (this.m_FishingRod.m_Hook.transform.position - fish.transform.position).normalized) >= 0.5f)
				{
					list.Add(fish);
					if (!this.chance_map.ContainsKey((int)fish.m_ID))
					{
						this.chance_map.Add((int)fish.m_ID, fish.m_Params.m_BitingChance);
						num += fish.m_Params.m_BitingChance;
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
			return this.m_ColliderBounds.Contains(point);
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

		public Vector3 GetRandomPointInTankSpace()
		{
			Vector3 vector = Vector3.zero;
			int num = 99;
			for (int i = 0; i < num; i++)
			{
				vector.x = UnityEngine.Random.Range(-0.5f, 0.5f);
				vector.y = UnityEngine.Random.Range(-0.5f, 0.5f);
				vector.z = UnityEngine.Random.Range(-0.5f, 0.5f);
				vector = base.gameObject.transform.TransformPoint(vector);
				if (vector.y >= MainLevel.GetTerrainY(vector))
				{
					return vector;
				}
			}
			return base.transform.position;
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
		private Bounds m_Bounds;

		private Bounds m_ColliderBounds;

		public static List<FishTank> s_FishTanks = new List<FishTank>();

		private Dictionary<int, float> chance_map = new Dictionary<int, float>();
	}
}
