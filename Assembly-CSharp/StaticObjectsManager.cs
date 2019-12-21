using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StaticObjectsManager : ReplicatedBehaviour, ISaveLoad
{
	public static StaticObjectsManager Get()
	{
		return StaticObjectsManager.s_Instance;
	}

	private void Awake()
	{
		StaticObjectsManager.s_Instance = this;
		this.ParseScript();
		if (this.m_EnablePooling)
		{
			this.CreatePool();
		}
	}

	private void Start()
	{
		this.Initialize();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("StaticBatching/StaticBatching.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Mesh" && !this.m_ReplaceMap.ContainsKey(key.GetVariable(0).SValue))
			{
				StaticObjectsReplace staticObjectsReplace = new StaticObjectsReplace();
				staticObjectsReplace.m_PrefabName = key.GetVariable(1).SValue;
				staticObjectsReplace.m_Prefab = GreenHellGame.Instance.GetPrefab(staticObjectsReplace.m_PrefabName);
				DebugUtils.Assert(staticObjectsReplace.m_Prefab != null, "[StaticObjectsManager:ParseScript] Can't find prefab - " + staticObjectsReplace.m_PrefabName, true, DebugUtils.AssertType.Info);
				this.m_ReplaceMap.Add(key.GetVariable(0).SValue, staticObjectsReplace);
			}
		}
	}

	public void Initialize()
	{
		if (this.m_Initialized)
		{
			return;
		}
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		if (array.Length == 0)
		{
			return;
		}
		Bounds bounds = default(Bounds);
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			if (i == 0)
			{
				bounds.min = (bounds.max = terrain.GetPosition());
			}
			else
			{
				bounds.Encapsulate(terrain.GetPosition());
			}
			if (!(terrain.terrainData == null))
			{
				bounds.Encapsulate(terrain.terrainData.size);
			}
		}
		this.m_QuadTree = new QuadTreeStaticObjects(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z, 100, 100);
		this.m_DestroyedObjects = new SimpleGrid(new Vector2(bounds.min.x, bounds.min.z), (int)bounds.size.x, (int)bounds.size.z);
		this.m_Initialized = true;
		MainLevel.Instance.OnStaticObjectsManagerInitialized();
	}

	private void CreatePool()
	{
		this.m_Pool = new Dictionary<string, List<GameObject>>();
		for (int i = 0; i < this.m_ReplaceMap.Keys.Count; i++)
		{
			KeyValuePair<string, StaticObjectsReplace> keyValuePair = this.m_ReplaceMap.ElementAt(i);
			this.m_Pool[keyValuePair.Value.m_PrefabName] = new List<GameObject>();
			for (int j = 0; j < this.m_NumObjectsInPool; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(keyValuePair.Value.m_Prefab, this.m_PoolPosition, Quaternion.identity);
				this.m_Pool[keyValuePair.Value.m_PrefabName].Add(gameObject);
				gameObject.SetActive(false);
			}
		}
	}

	public void Save()
	{
		List<Vector3> allPoints = this.m_DestroyedObjects.GetAllPoints();
		SaveGame.SaveVal("SOMDestroyedCount", allPoints.Count);
		for (int i = 0; i < allPoints.Count; i++)
		{
			SaveGame.SaveVal("SOMPos" + i, allPoints[i]);
		}
		ObjectWithTrunk.OnSave();
	}

	public void Load()
	{
		foreach (GameObject obj in this.m_ReplacedMap.Values)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_ReplacedMap.Clear();
		this.m_ObjectsRemovedFromStatic.Clear();
		this.EnableObjectsInQuadTree();
		List<Vector3> allPoints = this.m_DestroyedObjects.GetAllPoints();
		for (int i = 0; i < allPoints.Count; i++)
		{
			StaticObjectClass objectsInPos = this.m_QuadTree.GetObjectsInPos(allPoints[i]);
			if (objectsInPos != null && objectsInPos.m_GameObject != null)
			{
				objectsInPos.m_GameObject.SetActive(true);
				if (objectsInPos.m_GameObject.transform.parent != null)
				{
					objectsInPos.m_GameObject.transform.parent.gameObject.SetActive(true);
				}
				objectsInPos.m_State = 0;
			}
		}
		this.m_DestroyedObjects.Clear();
		int num = 0;
		SaveGame.LoadVal("SOMDestroyedCount", out num, false);
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < num; j++)
		{
			SaveGame.LoadVal("SOMPos" + j, out zero, false);
			StaticObjectClass objectsInPos2 = this.m_QuadTree.GetObjectsInPos(zero);
			if (objectsInPos2 != null && objectsInPos2.m_GameObject != null)
			{
				objectsInPos2.m_GameObject.SetActive(false);
				if (objectsInPos2.m_GameObject.transform.parent != null)
				{
					objectsInPos2.m_GameObject.transform.parent.gameObject.SetActive(false);
				}
			}
			this.m_DestroyedObjects.InsertPoint(zero, false);
		}
		ObjectWithTrunk.OnLoad();
		this.OnLoaded();
	}

	private void Update()
	{
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		List<StaticObjectClass> objectsInRadius = this.m_QuadTree.GetObjectsInRadius(Player.Get().gameObject.transform.position, 10f, false);
		for (int i = 0; i < objectsInRadius.Count; i++)
		{
			StaticObjectClass staticObjectClass = objectsInRadius[i];
			if (staticObjectClass.m_GameObject.activeSelf && staticObjectClass.m_State != 1 && !this.m_ObjectsRemovedFromStatic.Contains(staticObjectClass))
			{
				StaticObjectsReplace staticObjectsReplace = null;
				if (this.m_ReplaceMap.TryGetValue(staticObjectClass.m_GameObject.name, out staticObjectsReplace))
				{
					staticObjectClass.m_GameObject.SetActive(false);
					this.m_ObjectsRemovedFromStatic.Add(staticObjectClass);
					GameObject gameObject;
					if (this.m_EnablePooling)
					{
						if (this.m_Pool[staticObjectsReplace.m_PrefabName].Count > 0)
						{
							gameObject = this.m_Pool[staticObjectsReplace.m_PrefabName].ElementAt(0);
							this.m_Pool[staticObjectsReplace.m_PrefabName].RemoveAt(0);
						}
						else
						{
							gameObject = UnityEngine.Object.Instantiate<GameObject>(staticObjectsReplace.m_Prefab);
							this.m_TempItemList.Clear();
							gameObject.GetComponents<Item>(this.m_TempItemList);
							if (this.m_TempItemList.Count > 0)
							{
								this.m_TempItemList[0].m_CanSaveNotTriggered = false;
							}
						}
						gameObject.SetActive(true);
					}
					else
					{
						gameObject = UnityEngine.Object.Instantiate<GameObject>(staticObjectsReplace.m_Prefab);
						this.m_TempItemList.Clear();
						gameObject.GetComponents<Item>(this.m_TempItemList);
						if (this.m_TempItemList.Count > 0)
						{
							this.m_TempItemList[0].m_CanSaveNotTriggered = false;
						}
					}
					this.m_ReplacedMap.Add(staticObjectClass, gameObject);
					gameObject.transform.position = staticObjectClass.m_GameObject.transform.parent.position;
					gameObject.transform.rotation = staticObjectClass.m_GameObject.transform.parent.rotation;
					gameObject.transform.localScale = staticObjectClass.m_GameObject.transform.parent.localScale;
					Item component = gameObject.GetComponent<Item>();
					if (component)
					{
						component.Initialize(false);
					}
				}
			}
		}
		int j = 0;
		while (j < this.m_ObjectsRemovedFromStatic.Count)
		{
			StaticObjectClass staticObjectClass2 = this.m_ObjectsRemovedFromStatic[j];
			if (!objectsInRadius.Contains(staticObjectClass2))
			{
				GameObject gameObject2 = null;
				if (this.m_ReplacedMap.TryGetValue(staticObjectClass2, out gameObject2))
				{
					if (gameObject2 != null)
					{
						if (this.m_EnablePooling)
						{
							gameObject2.transform.position = this.m_PoolPosition;
							gameObject2.SetActive(false);
							this.m_Pool[this.m_ReplaceMap[staticObjectClass2.m_GameObject.name].m_PrefabName].Add(gameObject2);
							staticObjectClass2.m_GameObject.SetActive(true);
						}
						else
						{
							UnityEngine.Object.Destroy(gameObject2);
							if (staticObjectClass2.m_GameObject == null)
							{
								this.m_ObjectsRemovedFromStatic.Remove(staticObjectClass2);
								continue;
							}
							staticObjectClass2.m_GameObject.SetActive(true);
						}
					}
					else
					{
						if (staticObjectClass2.m_GameObject == null)
						{
							this.m_ObjectsRemovedFromStatic.Remove(staticObjectClass2);
							continue;
						}
						staticObjectClass2.m_State = 1;
						if (staticObjectClass2.m_GameObject.transform.parent != null)
						{
							staticObjectClass2.m_GameObject.transform.parent.gameObject.SetActive(false);
						}
					}
					this.m_ReplacedMap.Remove(staticObjectClass2);
					this.m_ObjectsRemovedFromStatic.Remove(staticObjectClass2);
				}
				else
				{
					j++;
				}
			}
			else
			{
				j++;
			}
		}
		if (this.m_ReplacedMap.Count > 0)
		{
			Dictionary<StaticObjectClass, GameObject>.Enumerator enumerator = this.m_ReplacedMap.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<StaticObjectClass, GameObject> keyValuePair = enumerator.Current;
				if (keyValuePair.Key.m_GameObject == null)
				{
					Dictionary<StaticObjectClass, GameObject> replacedMap = this.m_ReplacedMap;
					keyValuePair = enumerator.Current;
					replacedMap.Remove(keyValuePair.Key);
				}
			}
			enumerator.Dispose();
		}
	}

	public void ObjectDestroyed(GameObject obj)
	{
		if (!obj)
		{
			return;
		}
		foreach (StaticObjectClass staticObjectClass in this.m_ReplacedMap.Keys)
		{
			if (this.m_ReplacedMap[staticObjectClass] == obj)
			{
				this.m_DestroyedObjects.InsertPoint(obj.transform.position, false);
				this.m_ReplacedMap.Remove(staticObjectClass);
				staticObjectClass.m_State = 1;
				if (staticObjectClass.m_GameObject.transform.parent != null)
				{
					staticObjectClass.m_GameObject.transform.parent.gameObject.SetActive(false);
					break;
				}
				break;
			}
		}
	}

	public void ObjectDestroyed(Vector3 pos)
	{
		StaticObjectClass objectsInPos = this.m_QuadTree.GetObjectsInPos(pos);
		if (objectsInPos != null && objectsInPos.m_GameObject != null)
		{
			objectsInPos.m_GameObject.SetActive(false);
			if (objectsInPos.m_GameObject.transform.parent != null)
			{
				objectsInPos.m_GameObject.transform.parent.gameObject.SetActive(false);
			}
			GameObject obj = null;
			if (this.m_ReplacedMap.TryGetValue(objectsInPos, out obj))
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
		this.m_DestroyedObjects.InsertPoint(pos, false);
	}

	public void ScheduleReinit()
	{
	}

	public void OnStaticObjectAdded(GameObject go)
	{
		if (this.m_DestroyedObjects.IsPointInPos(go.transform.position))
		{
			if (go.transform.parent != null)
			{
				go.transform.parent.gameObject.SetActive(false);
			}
			return;
		}
		this.m_QuadTree.InsertObject(go, false);
	}

	public void OnStaticObjectRemoved(GameObject go)
	{
		this.m_QuadTree.RemoveObject(go);
	}

	private void OnLoaded()
	{
		List<Vector3> allPoints = this.m_DestroyedObjects.GetAllPoints();
		for (int i = 0; i < allPoints.Count; i++)
		{
			StaticObjectClass objectsInPos = this.m_QuadTree.GetObjectsInPos(allPoints[i]);
			if (objectsInPos != null && objectsInPos.m_GameObject != null)
			{
				GameObject gameObject = objectsInPos.m_GameObject;
				if (gameObject.transform.parent != null)
				{
					gameObject.transform.parent.gameObject.SetActive(false);
					StaticObjectClass staticObjectClass = objectsInPos;
					staticObjectClass.m_State |= 1;
					GameObject gameObject2 = null;
					if (this.m_ReplacedMap.TryGetValue(objectsInPos, out gameObject2))
					{
						this.m_ReplacedMap.Remove(objectsInPos);
						this.m_ObjectsRemovedFromStatic.Remove(objectsInPos);
					}
				}
			}
		}
	}

	private void EnableObjectsInQuadTree()
	{
		QuadTreeStaticObjectsCell[,] cells = this.m_QuadTree.GetCells();
		for (int i = 0; i < this.m_QuadTree.GetNumCellsX(); i++)
		{
			for (int j = 0; j < this.m_QuadTree.GetNumCellsY(); j++)
			{
				QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = cells[i, j];
				for (int k = 0; k < quadTreeStaticObjectsCell.m_Objects.Count; k++)
				{
					StaticObjectClass staticObjectClass = quadTreeStaticObjectsCell.m_Objects[k];
					if (staticObjectClass.m_GameObject != null)
					{
						staticObjectClass.m_GameObject.GetComponents<StaticObject>(this.m_StaticObjectTempList);
						StaticObject staticObject = null;
						if (this.m_StaticObjectTempList.Count > 0)
						{
							staticObject = this.m_StaticObjectTempList[0];
						}
						if (staticObject != null && !staticObject.m_IsBeingDestroyed)
						{
							staticObject.gameObject.SetActive(true);
						}
					}
				}
			}
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		if (initial_state)
		{
			List<Vector3> allPoints = this.m_DestroyedObjects.GetAllPoints();
			writer.Write((ushort)allPoints.Count);
			for (int i = 0; i < allPoints.Count; i++)
			{
				writer.Write(allPoints[i]);
			}
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		if (initial_state)
		{
			foreach (GameObject obj in this.m_ReplacedMap.Values)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_ReplacedMap.Clear();
			this.m_ObjectsRemovedFromStatic.Clear();
			this.EnableObjectsInQuadTree();
			List<Vector3> allPoints = this.m_DestroyedObjects.GetAllPoints();
			for (int i = 0; i < allPoints.Count; i++)
			{
				StaticObjectClass objectsInPos = this.m_QuadTree.GetObjectsInPos(allPoints[i]);
				if (objectsInPos != null && objectsInPos.m_GameObject != null)
				{
					objectsInPos.m_GameObject.SetActive(true);
					if (objectsInPos.m_GameObject.transform.parent != null)
					{
						objectsInPos.m_GameObject.transform.parent.gameObject.SetActive(true);
					}
					objectsInPos.m_State = 0;
				}
			}
			this.m_DestroyedObjects.Clear();
			ushort num = reader.ReadUInt16();
			for (int j = 0; j < (int)num; j++)
			{
				Vector3 vector = reader.ReadVector3();
				StaticObjectClass objectsInPos2 = this.m_QuadTree.GetObjectsInPos(vector);
				if (objectsInPos2 != null && objectsInPos2.m_GameObject != null)
				{
					objectsInPos2.m_GameObject.SetActive(false);
					if (objectsInPos2.m_GameObject.transform.parent != null)
					{
						objectsInPos2.m_GameObject.transform.parent.gameObject.SetActive(false);
					}
				}
				this.m_DestroyedObjects.InsertPoint(vector, false);
			}
			ObjectWithTrunk.OnLoad();
			this.OnLoaded();
		}
	}

	private QuadTreeStaticObjects m_QuadTree;

	private SimpleGrid m_DestroyedObjects;

	private List<StaticObjectClass> m_ObjectsRemovedFromStatic = new List<StaticObjectClass>();

	private Dictionary<string, StaticObjectsReplace> m_ReplaceMap = new Dictionary<string, StaticObjectsReplace>();

	public Dictionary<StaticObjectClass, GameObject> m_ReplacedMap = new Dictionary<StaticObjectClass, GameObject>();

	private bool m_EnablePooling;

	private Dictionary<string, List<GameObject>> m_Pool;

	private int m_NumObjectsInPool = 10;

	private Vector3 m_PoolPosition = new Vector3(0f, 0f, 0f);

	public bool m_Initialized;

	public static StaticObjectsManager s_Instance;

	private List<Item> m_TempItemList = new List<Item>();

	private List<StaticObject> m_StaticObjectTempList = new List<StaticObject>(5);
}
