using System;
using System.Collections.Generic;
using UnityEngine;

public class FallenObjectsManager : MonoBehaviour
{
	public static FallenObjectsManager Get()
	{
		return FallenObjectsManager.s_Instance;
	}

	private void Awake()
	{
		FallenObjectsManager.s_Instance = this;
		this.ParseScript();
		this.Initialize();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("FallenObjects/FallenObjects.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "SourceObject")
			{
				FallenObjectData fallenObjectData = new FallenObjectData();
				fallenObjectData.m_SourceTag = key.GetVariable(0).SValue;
				fallenObjectData.m_FallenPrefabName = key.GetVariable(1).SValue;
				fallenObjectData.m_QuantityMin = key.GetVariable(2).IValue;
				fallenObjectData.m_QuantityMax = key.GetVariable(3).IValue;
				fallenObjectData.m_MinGenRadius = key.GetVariable(4).FValue;
				fallenObjectData.m_MaxGenRadius = key.GetVariable(5).FValue;
				fallenObjectData.m_NoRespawn = key.GetVariable(6).BValue;
				fallenObjectData.m_Chance = key.GetVariable(7).FValue;
				fallenObjectData.m_Cooldown = key.GetVariable(8).FValue;
				List<FallenObjectData> list = null;
				if (!this.m_Data.TryGetValue(fallenObjectData.m_SourceTag, out list))
				{
					this.m_Data[fallenObjectData.m_SourceTag] = new List<FallenObjectData>();
				}
				this.m_Data[fallenObjectData.m_SourceTag].Add(fallenObjectData);
			}
		}
	}

	public void Initialize()
	{
		this.m_InitializeInternalRequested = true;
		this.InitializeInternal();
	}

	private void InitializeInternal()
	{
		for (int i = 0; i < this.m_Generators.Count; i++)
		{
			for (int j = 0; j < this.m_Generators[i].m_GeneratorData.m_Data.Count; j++)
			{
				for (int k = 0; k < this.m_Generators[i].m_GeneratorData.m_Data[j].m_GeneratedObjects.Count; k++)
				{
					GameObject gameObject = this.m_Generators[i].m_GeneratorData.m_Data[j].m_GeneratedObjects[k];
					if (gameObject != null)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
		}
		this.m_Generators.Clear();
		if (!GreenHellGame.Instance.m_FromSave)
		{
			this.GenerateObjects(this.m_Generators.Count);
		}
		this.m_InitializeInternalRequested = false;
		this.m_Initialized = true;
		MainLevel.Instance.OnFallenObjectsManagerInitialized();
	}

	private void Update()
	{
		if (MainLevel.Instance.IsPause())
		{
			return;
		}
		if (this.m_InitializeInternalRequested)
		{
			this.InitializeInternal();
		}
		else
		{
			this.GenerateObjects(10);
		}
	}

	private void GenerateObjects(int num_objects)
	{
		if (this.m_Generators.Count == 0)
		{
			return;
		}
		Vector3 position = Player.Get().gameObject.transform.position;
		position.y = 0f;
		int i = 0;
		while (i < num_objects)
		{
			if (this.m_CurrentObjectIdx >= this.m_Generators.Count)
			{
				this.m_CurrentObjectIdx = 0;
			}
			FallenObjectGenerator fallenObjectGenerator = this.m_Generators[this.m_CurrentObjectIdx];
			if (fallenObjectGenerator == null)
			{
				this.m_Generators.RemoveAt(this.m_CurrentObjectIdx);
			}
			else if (fallenObjectGenerator.m_GeneratorData.m_Object)
			{
				Vector3 position2 = fallenObjectGenerator.m_GeneratorData.m_Object.transform.position;
				position2.y = 0f;
				if (position.Distance(position2) <= FallenObjectsManager.s_MaxDistToPlayer)
				{
					for (int j = 0; j < fallenObjectGenerator.m_GeneratorData.m_Data.Count; j++)
					{
						bool flag = false;
						FallenObjectData fallenObjectData = fallenObjectGenerator.m_GeneratorData.m_Data[j];
						if (fallenObjectData.m_ObjectsSpawnNextTime < 0f)
						{
							fallenObjectData.m_ObjectsSpawnNextTime = MainLevel.Instance.GetCurrentTimeMinutes() + fallenObjectData.m_Cooldown;
							flag = true;
						}
						int k = 0;
						while (k < fallenObjectData.m_GeneratedObjects.Count)
						{
							if (fallenObjectData.m_GeneratedObjects[k] == null)
							{
								fallenObjectData.m_GeneratedObjects.RemoveAt(k);
							}
							else
							{
								k++;
							}
						}
						if (fallenObjectData.m_GeneratedObjects.Count <= 0)
						{
							if (!fallenObjectData.m_AlreadySpawned || !fallenObjectData.m_NoRespawn)
							{
								if (flag || MainLevel.Instance.GetCurrentTimeMinutes() >= fallenObjectData.m_ObjectsSpawnNextTime)
								{
									bool flag2 = this.CreateFallenObjects(fallenObjectGenerator.m_GeneratorData.m_Object, fallenObjectData, fallenObjectGenerator.m_GeneratorData, j, fallenObjectGenerator);
									fallenObjectData.m_ObjectsSpawnNextTime = MainLevel.Instance.GetCurrentTimeMinutes() + fallenObjectData.m_Cooldown;
									fallenObjectData.m_AlreadySpawned = true;
									if (flag2)
									{
										this.m_CurrentObjectIdx++;
										return;
									}
								}
							}
						}
					}
				}
			}
			i++;
			this.m_CurrentObjectIdx++;
		}
	}

	private bool CreateFallenObjects(GameObject go, FallenObjectData data, FallenObjectGeneratorData generator_data, int data_index, FallenObjectGenerator generator)
	{
		if (UnityEngine.Random.Range(0f, 1f) > data.m_Chance)
		{
			return false;
		}
		GameObject prefab = GreenHellGame.Instance.GetPrefab(data.m_FallenPrefabName);
		if (!prefab)
		{
			DebugUtils.Assert("[FallenObjectsManager:CreateFallenObjects] Can't load prefab - " + data.m_FallenPrefabName, true, DebugUtils.AssertType.Info);
			return false;
		}
		bool result = false;
		int num = UnityEngine.Random.Range(data.m_QuantityMin, data.m_QuantityMax + 1);
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = this.GenerateRandomPointAround(go.transform.position, data.m_MinGenRadius, data.m_MaxGenRadius);
			Vector3 origin = vector;
			origin.y += 3f;
			RaycastHit raycastHit;
			if (Physics.Raycast(new Ray
			{
				origin = origin,
				direction = Vector3.down
			}, out raycastHit))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
				if (!gameObject)
				{
					DebugUtils.Assert("[FallenObjectsManager:CreateFallenObjects] Can't Instantiate prefab - " + prefab.name, true, DebugUtils.AssertType.Info);
				}
				else
				{
					vector = raycastHit.point;
					BoxCollider component = gameObject.GetComponent<BoxCollider>();
					float y = component.bounds.min.y;
					float y2 = gameObject.transform.position.y;
					float num2 = y2 - y;
					vector.y += num2;
					gameObject.transform.position = vector;
					Quaternion quaternion = gameObject.transform.rotation;
					quaternion *= Quaternion.Euler(Vector3.up * UnityEngine.Random.Range(0f, 360f));
					gameObject.transform.rotation = quaternion;
					Vector3 forward = gameObject.transform.forward - Vector3.Dot(gameObject.transform.forward, raycastHit.normal) * raycastHit.normal;
					gameObject.transform.rotation = Quaternion.LookRotation(forward, raycastHit.normal);
					data.m_GeneratedObjects.Add(gameObject);
					Trigger component2 = gameObject.GetComponent<Trigger>();
					DebugUtils.Assert(component2 != null, "[FallenObjectsManager:CreateFallenObjects] Generated object is not item!", true, DebugUtils.AssertType.Info);
					component2.m_FallenObject = true;
					component2.m_FallenObjectCreationTime = Time.time;
					component2.m_FallenObjectGenerator = generator;
					result = true;
				}
			}
		}
		return result;
	}

	private Vector3 GenerateRandomPointAround(Vector3 point, float min_dist, float max_dist)
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		float num2 = UnityEngine.Random.Range(0f, 1f);
		float num3 = min_dist + num * (max_dist - min_dist);
		float f = 6.28318548f * num2;
		float x = point.x + num3 * Mathf.Cos(f);
		float z = point.z + num3 * Mathf.Sin(f);
		return new Vector3(x, point.y, z);
	}

	public void AddObject(GameObject obj, int gen_index, int data_index)
	{
		FallenObjectGenerator fallenObjectGenerator = this.m_Generators[gen_index];
		FallenObjectData fallenObjectData = fallenObjectGenerator.m_GeneratorData.m_Data[data_index];
		fallenObjectData.m_GeneratedObjects.Add(obj);
	}

	public void RemoveItem(Trigger trigger)
	{
		FallenObjectGenerator fallenObjectGenerator = trigger.m_FallenObjectGenerator;
		if (fallenObjectGenerator != null && fallenObjectGenerator)
		{
			FallenObjectGeneratorData generatorData = fallenObjectGenerator.m_GeneratorData;
			for (int i = 0; i < generatorData.m_Data.Count; i++)
			{
				if (generatorData.m_Data[i].m_GeneratedObjects.Contains(trigger.gameObject))
				{
					generatorData.m_Data[i].m_GeneratedObjects.Remove(trigger.gameObject);
				}
			}
		}
	}

	public void OnFallenObjectSourceAdded(GameObject go)
	{
		List<FallenObjectData> list = null;
		if (!this.m_Data.TryGetValue(go.tag, out list))
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			FallenObjectData src = list[i];
			FallenObjectGeneratorData fallenObjectGeneratorData;
			if (go.GetComponent<FallenObjectGenerator>() == null)
			{
				FallenObjectGenerator fallenObjectGenerator = go.AddComponent<FallenObjectGenerator>();
				fallenObjectGeneratorData = new FallenObjectGeneratorData();
				fallenObjectGenerator.m_GeneratorData = fallenObjectGeneratorData;
				this.m_Generators.Add(go.GetComponent<FallenObjectGenerator>());
			}
			else
			{
				fallenObjectGeneratorData = go.GetComponent<FallenObjectGenerator>().m_GeneratorData;
			}
			fallenObjectGeneratorData.m_Data.Add(new FallenObjectData(src));
			fallenObjectGeneratorData.m_Object = go;
		}
	}

	public void OnFallenObjectSourceRemoved(GameObject go)
	{
		FallenObjectGenerator component = go.GetComponent<FallenObjectGenerator>();
		if (component != null)
		{
			for (int i = 0; i < component.m_GeneratorData.m_Data.Count; i++)
			{
				for (int j = 0; j < component.m_GeneratorData.m_Data[i].m_GeneratedObjects.Count; j++)
				{
					GameObject gameObject = component.m_GeneratorData.m_Data[i].m_GeneratedObjects[j];
					if (gameObject != null)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
		}
	}

	public void OnFullLoadEnd()
	{
		for (int i = 0; i < this.m_Generators.Count; i++)
		{
			FallenObjectGenerator fallenObjectGenerator = this.m_Generators[i];
			if (fallenObjectGenerator != null)
			{
				for (int j = 0; j < fallenObjectGenerator.m_GeneratorData.m_Data.Count; j++)
				{
					FallenObjectData fallenObjectData = fallenObjectGenerator.m_GeneratorData.m_Data[j];
					fallenObjectData.m_ObjectsSpawnNextTime = -1f;
				}
			}
		}
	}

	private Dictionary<string, List<FallenObjectData>> m_Data = new Dictionary<string, List<FallenObjectData>>();

	private List<FallenObjectGenerator> m_Generators = new List<FallenObjectGenerator>();

	private const int m_ObjectsPerFrame = 10;

	private int m_CurrentObjectIdx;

	public float m_MinDistToPlayer;

	public static float s_MaxDistToPlayer = 40f;

	private static FallenObjectsManager s_Instance;

	private bool m_InitializeInternalRequested;

	public bool m_Initialized;
}
