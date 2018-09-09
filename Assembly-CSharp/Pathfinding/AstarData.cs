using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Pathfinding.WindowsStore;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding
{
	[Serializable]
	public class AstarData
	{
		public static AstarPath active
		{
			get
			{
				return AstarPath.active;
			}
		}

		public NavMeshGraph navmesh { get; private set; }

		public GridGraph gridGraph { get; private set; }

		public LayerGridGraph layerGridGraph { get; private set; }

		public PointGraph pointGraph { get; private set; }

		public RecastGraph recastGraph { get; private set; }

		public Type[] graphTypes { get; private set; }

		private byte[] data
		{
			get
			{
				if (this.upgradeData != null && this.upgradeData.Length > 0)
				{
					this.data = this.upgradeData;
					this.upgradeData = null;
				}
				return (this.dataString == null) ? null : Convert.FromBase64String(this.dataString);
			}
			set
			{
				this.dataString = ((value == null) ? null : Convert.ToBase64String(value));
			}
		}

		public byte[] GetData()
		{
			return this.data;
		}

		public void SetData(byte[] data)
		{
			this.data = data;
		}

		public void Awake()
		{
			this.graphs = new NavGraph[0];
			if (this.cacheStartup && this.file_cachedStartup != null)
			{
				this.LoadFromCache();
			}
			else
			{
				this.DeserializeGraphs();
			}
		}

		internal void LockGraphStructure(bool allowAddingGraphs = false)
		{
			this.graphStructureLocked.Add(allowAddingGraphs);
		}

		internal void UnlockGraphStructure()
		{
			if (this.graphStructureLocked.Count == 0)
			{
				throw new InvalidOperationException();
			}
			this.graphStructureLocked.RemoveAt(this.graphStructureLocked.Count - 1);
		}

		private PathProcessor.GraphUpdateLock AssertSafe(bool onlyAddingGraph = false)
		{
			if (this.graphStructureLocked.Count > 0)
			{
				bool flag = true;
				for (int i = 0; i < this.graphStructureLocked.Count; i++)
				{
					flag &= this.graphStructureLocked[i];
				}
				if (!onlyAddingGraph || !flag)
				{
					throw new InvalidOperationException("Graphs cannot be added, removed or serialized while the graph structure is locked. This is the case when a graph is currently being scanned and when executing graph updates and work items.\nHowever as a special case, graphs can be added inside work items.");
				}
			}
			PathProcessor.GraphUpdateLock result = AstarData.active.PausePathfinding();
			if (!AstarData.active.IsInsideWorkItem)
			{
				AstarData.active.FlushWorkItems();
				AstarData.active.pathReturnQueue.ReturnPaths(false);
			}
			return result;
		}

		public void UpdateShortcuts()
		{
			this.navmesh = (NavMeshGraph)this.FindGraphOfType(typeof(NavMeshGraph));
			this.gridGraph = (GridGraph)this.FindGraphOfType(typeof(GridGraph));
			this.layerGridGraph = (LayerGridGraph)this.FindGraphOfType(typeof(LayerGridGraph));
			this.pointGraph = (PointGraph)this.FindGraphOfType(typeof(PointGraph));
			this.recastGraph = (RecastGraph)this.FindGraphOfType(typeof(RecastGraph));
		}

		public void LoadFromCache()
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			if (this.file_cachedStartup != null)
			{
				byte[] bytes = this.file_cachedStartup.bytes;
				this.DeserializeGraphs(bytes);
				GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
			}
			else
			{
				Debug.LogError("Can't load from cache since the cache is empty");
			}
			graphUpdateLock.Release();
		}

		public byte[] SerializeGraphs()
		{
			return this.SerializeGraphs(SerializeSettings.Settings);
		}

		public byte[] SerializeGraphs(SerializeSettings settings)
		{
			uint num;
			return this.SerializeGraphs(settings, out num);
		}

		public byte[] SerializeGraphs(SerializeSettings settings, out uint checksum)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			AstarSerializer astarSerializer = new AstarSerializer(this, settings);
			astarSerializer.OpenSerialize();
			this.SerializeGraphsPart(astarSerializer);
			byte[] result = astarSerializer.CloseSerialize();
			checksum = astarSerializer.GetChecksum();
			graphUpdateLock.Release();
			return result;
		}

		public void SerializeGraphsPart(AstarSerializer sr)
		{
			sr.SerializeGraphs(this.graphs);
			sr.SerializeExtraInfo();
		}

		public void DeserializeGraphs()
		{
			if (this.data != null)
			{
				this.DeserializeGraphs(this.data);
			}
		}

		private void ClearGraphs()
		{
			if (this.graphs == null)
			{
				return;
			}
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] != null)
				{
					this.graphs[i].OnDestroy();
					this.graphs[i].active = null;
				}
			}
			this.graphs = null;
			this.UpdateShortcuts();
		}

		public void OnDestroy()
		{
			this.ClearGraphs();
		}

		public void DeserializeGraphs(byte[] bytes)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			this.ClearGraphs();
			this.DeserializeGraphsAdditive(bytes);
			graphUpdateLock.Release();
		}

		public void DeserializeGraphsAdditive(byte[] bytes)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			try
			{
				if (bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				AstarSerializer astarSerializer = new AstarSerializer(this);
				if (astarSerializer.OpenDeserialize(bytes))
				{
					this.DeserializeGraphsPartAdditive(astarSerializer);
					astarSerializer.CloseDeserialize();
				}
				else
				{
					Debug.Log("Invalid data file (cannot read zip).\nThe data is either corrupt or it was saved using a 3.0.x or earlier version of the system");
				}
				AstarData.active.VerifyIntegrity();
			}
			catch (Exception arg)
			{
				Debug.LogError("Caught exception while deserializing data.\n" + arg);
				this.graphs = new NavGraph[0];
				this.data_backup = bytes;
			}
			this.UpdateShortcuts();
			graphUpdateLock.Release();
		}

		public void DeserializeGraphsPart(AstarSerializer sr)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			this.ClearGraphs();
			this.DeserializeGraphsPartAdditive(sr);
			graphUpdateLock.Release();
		}

		public void DeserializeGraphsPartAdditive(AstarSerializer sr)
		{
			if (this.graphs == null)
			{
				this.graphs = new NavGraph[0];
			}
			List<NavGraph> list = new List<NavGraph>(this.graphs);
			sr.SetGraphIndexOffset(list.Count);
			list.AddRange(sr.DeserializeGraphs());
			this.graphs = list.ToArray();
			sr.DeserializeExtraInfo();
			int i;
			for (i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] != null)
				{
					this.graphs[i].GetNodes(delegate(GraphNode node)
					{
						node.GraphIndex = (uint)i;
					});
				}
			}
			for (int k = 0; k < this.graphs.Length; k++)
			{
				for (int j = k + 1; j < this.graphs.Length; j++)
				{
					if (this.graphs[k] != null && this.graphs[j] != null && this.graphs[k].guid == this.graphs[j].guid)
					{
						Debug.LogWarning("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
						this.graphs[k].guid = Pathfinding.Util.Guid.NewGuid();
						break;
					}
				}
			}
			sr.PostDeserialization();
		}

		public void FindGraphTypes()
		{
			Assembly assembly = WindowsStoreCompatibility.GetTypeInfo(typeof(AstarPath)).Assembly;
			Type[] types = assembly.GetTypes();
			List<Type> list = new List<Type>();
			foreach (Type type in types)
			{
				for (Type baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
				{
					if (object.Equals(baseType, typeof(NavGraph)))
					{
						list.Add(type);
						break;
					}
				}
			}
			this.graphTypes = list.ToArray();
		}

		[Obsolete("If really necessary. Use System.Type.GetType instead.")]
		public Type GetGraphType(string type)
		{
			for (int i = 0; i < this.graphTypes.Length; i++)
			{
				if (this.graphTypes[i].Name == type)
				{
					return this.graphTypes[i];
				}
			}
			return null;
		}

		[Obsolete("Use CreateGraph(System.Type) instead")]
		public NavGraph CreateGraph(string type)
		{
			Debug.Log("Creating Graph of type '" + type + "'");
			for (int i = 0; i < this.graphTypes.Length; i++)
			{
				if (this.graphTypes[i].Name == type)
				{
					return this.CreateGraph(this.graphTypes[i]);
				}
			}
			Debug.LogError("Graph type (" + type + ") wasn't found");
			return null;
		}

		internal NavGraph CreateGraph(Type type)
		{
			NavGraph navGraph = Activator.CreateInstance(type) as NavGraph;
			navGraph.active = AstarData.active;
			return navGraph;
		}

		[Obsolete("Use AddGraph(System.Type) instead")]
		public NavGraph AddGraph(string type)
		{
			NavGraph navGraph = null;
			for (int i = 0; i < this.graphTypes.Length; i++)
			{
				if (this.graphTypes[i].Name == type)
				{
					navGraph = this.CreateGraph(this.graphTypes[i]);
				}
			}
			if (navGraph == null)
			{
				Debug.LogError("No NavGraph of type '" + type + "' could be found");
				return null;
			}
			this.AddGraph(navGraph);
			return navGraph;
		}

		public NavGraph AddGraph(Type type)
		{
			NavGraph navGraph = null;
			for (int i = 0; i < this.graphTypes.Length; i++)
			{
				if (object.Equals(this.graphTypes[i], type))
				{
					navGraph = this.CreateGraph(this.graphTypes[i]);
				}
			}
			if (navGraph == null)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"No NavGraph of type '",
					type,
					"' could be found, ",
					this.graphTypes.Length,
					" graph types are avaliable"
				}));
				return null;
			}
			this.AddGraph(navGraph);
			return navGraph;
		}

		private void AddGraph(NavGraph graph)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(true);
			bool flag = false;
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] == null)
				{
					this.graphs[i] = graph;
					graph.graphIndex = (uint)i;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (this.graphs != null && (long)this.graphs.Length >= 255L)
				{
					throw new Exception("Graph Count Limit Reached. You cannot have more than " + 255u + " graphs.");
				}
				this.graphs = new List<NavGraph>(this.graphs ?? new NavGraph[0])
				{
					graph
				}.ToArray();
				graph.graphIndex = (uint)(this.graphs.Length - 1);
			}
			this.UpdateShortcuts();
			graph.active = AstarData.active;
			graphUpdateLock.Release();
		}

		public bool RemoveGraph(NavGraph graph)
		{
			PathProcessor.GraphUpdateLock graphUpdateLock = this.AssertSafe(false);
			graph.OnDestroy();
			graph.active = null;
			int num = Array.IndexOf<NavGraph>(this.graphs, graph);
			if (num != -1)
			{
				this.graphs[num] = null;
			}
			this.UpdateShortcuts();
			graphUpdateLock.Release();
			return num != -1;
		}

		public static NavGraph GetGraph(GraphNode node)
		{
			if (node == null)
			{
				return null;
			}
			AstarPath active = AstarPath.active;
			if (active == null)
			{
				return null;
			}
			AstarData data = active.data;
			if (data == null || data.graphs == null)
			{
				return null;
			}
			uint graphIndex = node.GraphIndex;
			if ((ulong)graphIndex >= (ulong)((long)data.graphs.Length))
			{
				return null;
			}
			return data.graphs[(int)graphIndex];
		}

		public NavGraph FindGraphOfType(Type type)
		{
			if (this.graphs != null)
			{
				for (int i = 0; i < this.graphs.Length; i++)
				{
					if (this.graphs[i] != null && object.Equals(this.graphs[i].GetType(), type))
					{
						return this.graphs[i];
					}
				}
			}
			return null;
		}

		public IEnumerable FindGraphsOfType(Type type)
		{
			if (this.graphs == null)
			{
				yield break;
			}
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] != null && object.Equals(this.graphs[i].GetType(), type))
				{
					yield return this.graphs[i];
				}
			}
			yield break;
		}

		public IEnumerable GetUpdateableGraphs()
		{
			if (this.graphs == null)
			{
				yield break;
			}
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] is IUpdatableGraph)
				{
					yield return this.graphs[i];
				}
			}
			yield break;
		}

		[Obsolete("Obsolete because it is not used by the package internally and the use cases are few. Iterate through the graphs array instead.")]
		public IEnumerable GetRaycastableGraphs()
		{
			if (this.graphs == null)
			{
				yield break;
			}
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] is IRaycastableGraph)
				{
					yield return this.graphs[i];
				}
			}
			yield break;
		}

		public int GetGraphIndex(NavGraph graph)
		{
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}
			int num = -1;
			if (this.graphs != null)
			{
				num = Array.IndexOf<NavGraph>(this.graphs, graph);
				if (num == -1)
				{
					Debug.LogError("Graph doesn't exist");
				}
			}
			return num;
		}

		[NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		[SerializeField]
		private string dataString;

		[SerializeField]
		[FormerlySerializedAs("data")]
		private byte[] upgradeData;

		public byte[] data_backup;

		public TextAsset file_cachedStartup;

		public byte[] data_cachedStartup;

		[SerializeField]
		public bool cacheStartup;

		private List<bool> graphStructureLocked = new List<bool>();
	}
}
