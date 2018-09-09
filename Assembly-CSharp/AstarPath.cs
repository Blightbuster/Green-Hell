using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Pathfinding/Pathfinder")]
[HelpURL("http://arongranberg.com/astar/docs/class_astar_path.php")]
[ExecuteInEditMode]
public class AstarPath : VersionedMonoBehaviour
{
	private AstarPath()
	{
		this.pathProcessor = new PathProcessor(this, this.pathReturnQueue, 0, true);
		this.pathReturnQueue = new PathReturnQueue(this);
		this.workItems = new WorkItemProcessor(this);
		this.graphUpdates = new GraphUpdateProcessor(this);
		this.graphUpdates.OnGraphsUpdated += delegate
		{
			if (AstarPath.OnGraphsUpdated != null)
			{
				AstarPath.OnGraphsUpdated(this);
			}
		};
	}

	public static Version Version
	{
		get
		{
			return new Version(4, 0, 11);
		}
	}

	[Obsolete]
	public Type[] graphTypes
	{
		get
		{
			return this.data.graphTypes;
		}
	}

	[Obsolete("The 'astarData' field has been renamed to 'data'")]
	public AstarData astarData
	{
		get
		{
			return this.data;
		}
	}

	public NavGraph[] graphs
	{
		get
		{
			if (this.data == null)
			{
				this.data = new AstarData();
			}
			return this.data.graphs;
		}
	}

	public float maxNearestNodeDistanceSqr
	{
		get
		{
			return this.maxNearestNodeDistance * this.maxNearestNodeDistance;
		}
	}

	[Obsolete("This field has been renamed to 'batchGraphUpdates'")]
	public bool limitGraphUpdates
	{
		get
		{
			return this.batchGraphUpdates;
		}
		set
		{
			this.batchGraphUpdates = value;
		}
	}

	[Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
	public float maxGraphUpdateFreq
	{
		get
		{
			return this.graphUpdateBatchingInterval;
		}
		set
		{
			this.graphUpdateBatchingInterval = value;
		}
	}

	public float lastScanTime { get; private set; }

	public bool isScanning
	{
		get
		{
			return this.isScanningBacking;
		}
		private set
		{
			this.isScanningBacking = value;
		}
	}

	public int NumParallelThreads
	{
		get
		{
			return this.pathProcessor.NumThreads;
		}
	}

	public bool IsUsingMultithreading
	{
		get
		{
			return this.pathProcessor.IsUsingMultithreading;
		}
	}

	[Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
	public bool IsAnyGraphUpdatesQueued
	{
		get
		{
			return this.IsAnyGraphUpdateQueued;
		}
	}

	public bool IsAnyGraphUpdateQueued
	{
		get
		{
			return this.graphUpdates.IsAnyGraphUpdateQueued;
		}
	}

	public bool IsAnyGraphUpdateInProgress
	{
		get
		{
			return this.graphUpdates.IsAnyGraphUpdateInProgress;
		}
	}

	public bool IsAnyWorkItemInProgress
	{
		get
		{
			return this.workItems.workItemsInProgress;
		}
	}

	internal bool IsInsideWorkItem
	{
		get
		{
			return this.workItems.workItemsInProgressRightNow;
		}
	}

	public string[] GetTagNames()
	{
		if (this.tagNames == null || this.tagNames.Length != 32)
		{
			this.tagNames = new string[32];
			for (int i = 0; i < this.tagNames.Length; i++)
			{
				this.tagNames[i] = string.Empty + i;
			}
			this.tagNames[0] = "Basic Ground";
		}
		return this.tagNames;
	}

	public static string[] FindTagNames()
	{
		if (AstarPath.active != null)
		{
			return AstarPath.active.GetTagNames();
		}
		AstarPath astarPath = UnityEngine.Object.FindObjectOfType<AstarPath>();
		if (astarPath != null)
		{
			AstarPath.active = astarPath;
			return astarPath.GetTagNames();
		}
		return new string[]
		{
			"There is no AstarPath component in the scene"
		};
	}

	internal ushort GetNextPathID()
	{
		if (this.nextFreePathID == 0)
		{
			this.nextFreePathID += 1;
			UnityEngine.Debug.Log("65K cleanup (this message is harmless, it just means you have searched a lot of paths)");
			if (AstarPath.On65KOverflow != null)
			{
				Action on65KOverflow = AstarPath.On65KOverflow;
				AstarPath.On65KOverflow = null;
				on65KOverflow();
			}
		}
		ushort result;
		this.nextFreePathID = (result = this.nextFreePathID) + 1;
		return result;
	}

	private void RecalculateDebugLimits()
	{
		this.debugFloor = float.PositiveInfinity;
		this.debugRoof = float.NegativeInfinity;
		bool ignoreSearchTree = !this.showSearchTree || this.debugPathData == null;
		for (int i = 0; i < this.graphs.Length; i++)
		{
			if (this.graphs[i] != null && this.graphs[i].drawGizmos)
			{
				this.graphs[i].GetNodes(delegate(GraphNode node)
				{
					if (ignoreSearchTree || GraphGizmoHelper.InSearchTree(node, this.debugPathData, this.debugPathID))
					{
						if (this.debugMode == GraphDebugMode.Penalty)
						{
							this.debugFloor = Mathf.Min(this.debugFloor, node.Penalty);
							this.debugRoof = Mathf.Max(this.debugRoof, node.Penalty);
						}
						else if (this.debugPathData != null)
						{
							PathNode pathNode = this.debugPathData.GetPathNode(node);
							GraphDebugMode graphDebugMode = this.debugMode;
							if (graphDebugMode != GraphDebugMode.F)
							{
								if (graphDebugMode != GraphDebugMode.G)
								{
									if (graphDebugMode == GraphDebugMode.H)
									{
										this.debugFloor = Mathf.Min(this.debugFloor, pathNode.H);
										this.debugRoof = Mathf.Max(this.debugRoof, pathNode.H);
									}
								}
								else
								{
									this.debugFloor = Mathf.Min(this.debugFloor, pathNode.G);
									this.debugRoof = Mathf.Max(this.debugRoof, pathNode.G);
								}
							}
							else
							{
								this.debugFloor = Mathf.Min(this.debugFloor, pathNode.F);
								this.debugRoof = Mathf.Max(this.debugRoof, pathNode.F);
							}
						}
					}
				});
			}
		}
		if (float.IsInfinity(this.debugFloor))
		{
			this.debugFloor = 0f;
			this.debugRoof = 1f;
		}
		if (this.debugRoof - this.debugFloor < 1f)
		{
			this.debugRoof += 1f;
		}
	}

	private void OnDrawGizmos()
	{
		if (AstarPath.active == null)
		{
			AstarPath.active = this;
		}
		if (AstarPath.active != this || this.graphs == null)
		{
			return;
		}
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (this.workItems.workItemsInProgress || this.isScanning)
		{
			this.gizmos.DrawExisting();
		}
		else
		{
			if (this.showNavGraphs && !this.manualDebugFloorRoof)
			{
				this.RecalculateDebugLimits();
			}
			for (int i = 0; i < this.graphs.Length; i++)
			{
				if (this.graphs[i] != null && this.graphs[i].drawGizmos)
				{
					this.graphs[i].OnDrawGizmos(this.gizmos, this.showNavGraphs);
				}
			}
			if (this.showNavGraphs)
			{
				this.euclideanEmbedding.OnDrawGizmos();
			}
		}
		this.gizmos.FinalizeDraw();
	}

	private void OnGUI()
	{
		if (this.logPathResults == PathLog.InGame && this.inGameDebugPath != string.Empty)
		{
			GUI.Label(new Rect(5f, 5f, 400f, 600f), this.inGameDebugPath);
		}
	}

	internal void Log(string s)
	{
		if (object.ReferenceEquals(AstarPath.active, null))
		{
			UnityEngine.Debug.Log("No AstarPath object was found : " + s);
			return;
		}
		if (AstarPath.active.logPathResults != PathLog.None && AstarPath.active.logPathResults != PathLog.OnlyErrors)
		{
			UnityEngine.Debug.Log(s);
		}
	}

	private void LogPathResults(Path p)
	{
		if (this.logPathResults == PathLog.None || (this.logPathResults == PathLog.OnlyErrors && !p.error))
		{
			return;
		}
		string message = p.DebugString(this.logPathResults);
		if (this.logPathResults == PathLog.InGame)
		{
			this.inGameDebugPath = message;
		}
		else
		{
			UnityEngine.Debug.Log(message);
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!this.isScanning)
		{
			this.PerformBlockingActions(false);
		}
		this.pathProcessor.TickNonMultithreaded();
		this.pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions(bool force = false)
	{
		if (this.workItemLock.Held && this.pathProcessor.queue.AllReceiversBlocked)
		{
			this.pathReturnQueue.ReturnPaths(false);
			if (this.workItems.ProcessWorkItems(force))
			{
				this.workItemLock.Release();
			}
		}
	}

	[Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void QueueWorkItemFloodFill()
	{
		throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	[Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void EnsureValidFloodFill()
	{
		throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	public void AddWorkItem(Action callback)
	{
		this.AddWorkItem(new AstarWorkItem(callback, null));
	}

	public void AddWorkItem(AstarWorkItem item)
	{
		this.workItems.AddWorkItem(item);
		if (!this.workItemLock.Held)
		{
			this.workItemLock = this.PausePathfindingSoon();
		}
	}

	public void QueueGraphUpdates()
	{
		if (!this.graphUpdatesWorkItemAdded)
		{
			this.graphUpdatesWorkItemAdded = true;
			AstarWorkItem workItem = this.graphUpdates.GetWorkItem();
			this.AddWorkItem(new AstarWorkItem(delegate
			{
				this.graphUpdatesWorkItemAdded = false;
				this.lastGraphUpdate = Time.realtimeSinceStartup;
				workItem.init();
			}, workItem.update));
		}
	}

	private IEnumerator DelayedGraphUpdate()
	{
		this.graphUpdateRoutineRunning = true;
		yield return new WaitForSeconds(this.graphUpdateBatchingInterval - (Time.realtimeSinceStartup - this.lastGraphUpdate));
		this.QueueGraphUpdates();
		this.graphUpdateRoutineRunning = false;
		yield break;
	}

	public void UpdateGraphs(Bounds bounds, float delay)
	{
		this.UpdateGraphs(new GraphUpdateObject(bounds), delay);
	}

	public void UpdateGraphs(GraphUpdateObject ob, float delay)
	{
		base.StartCoroutine(this.UpdateGraphsInteral(ob, delay));
	}

	private IEnumerator UpdateGraphsInteral(GraphUpdateObject ob, float delay)
	{
		yield return new WaitForSeconds(delay);
		this.UpdateGraphs(ob);
		yield break;
	}

	public void UpdateGraphs(Bounds bounds)
	{
		this.UpdateGraphs(new GraphUpdateObject(bounds));
	}

	public void UpdateGraphs(GraphUpdateObject ob)
	{
		this.graphUpdates.AddToQueue(ob);
		if (this.batchGraphUpdates && Time.realtimeSinceStartup - this.lastGraphUpdate < this.graphUpdateBatchingInterval)
		{
			if (!this.graphUpdateRoutineRunning)
			{
				base.StartCoroutine(this.DelayedGraphUpdate());
			}
		}
		else
		{
			this.QueueGraphUpdates();
		}
	}

	public void FlushGraphUpdates()
	{
		if (this.IsAnyGraphUpdateQueued)
		{
			this.QueueGraphUpdates();
			this.FlushWorkItems();
		}
	}

	public void FlushWorkItems()
	{
		PathProcessor.GraphUpdateLock graphUpdateLock = this.PausePathfinding();
		this.PerformBlockingActions(true);
		graphUpdateLock.Release();
	}

	[Obsolete("Use FlushWorkItems() instead")]
	public void FlushWorkItems(bool unblockOnComplete, bool block)
	{
		PathProcessor.GraphUpdateLock graphUpdateLock = this.PausePathfinding();
		this.PerformBlockingActions(block);
		graphUpdateLock.Release();
	}

	[Obsolete("Use FlushWorkItems instead")]
	public void FlushThreadSafeCallbacks()
	{
		this.FlushWorkItems();
	}

	public static int CalculateThreadCount(ThreadCount count)
	{
		if (count != ThreadCount.AutomaticLowLoad && count != ThreadCount.AutomaticHighLoad)
		{
			return (int)count;
		}
		int num = Mathf.Max(1, SystemInfo.processorCount);
		int num2 = SystemInfo.systemMemorySize;
		if (num2 <= 0)
		{
			UnityEngine.Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
			num2 = 1024;
		}
		if (num <= 1)
		{
			return 0;
		}
		if (num2 <= 512)
		{
			return 0;
		}
		if (count == ThreadCount.AutomaticHighLoad)
		{
			if (num2 <= 1024)
			{
				num = Math.Min(num, 2);
			}
		}
		else
		{
			num /= 2;
			num = Mathf.Max(1, num);
			if (num2 <= 1024)
			{
				num = Math.Min(num, 2);
			}
			num = Math.Min(num, 6);
		}
		return num;
	}

	protected override void Awake()
	{
		base.Awake();
		AstarPath.active = this;
		if (UnityEngine.Object.FindObjectsOfType(typeof(AstarPath)).Length > 1)
		{
			UnityEngine.Debug.LogError("You should NOT have more than one AstarPath component in the scene at any time.\nThis can cause serious errors since the AstarPath component builds around a singleton pattern.");
		}
		base.useGUILayout = false;
		if (!Application.isPlaying)
		{
			return;
		}
		if (AstarPath.OnAwakeSettings != null)
		{
			AstarPath.OnAwakeSettings();
		}
		GraphModifier.FindAllModifiers();
		RelevantGraphSurface.FindAllGraphSurfaces();
		this.InitializePathProcessor();
		this.InitializeProfiler();
		this.ConfigureReferencesInternal();
		this.InitializeAstarData();
		this.FlushWorkItems();
		this.euclideanEmbedding.dirty = true;
		if (this.scanOnStartup && (!this.data.cacheStartup || this.data.file_cachedStartup == null))
		{
			this.Scan(null);
		}
	}

	private void InitializePathProcessor()
	{
		int num = AstarPath.CalculateThreadCount(this.threadCount);
		int processors = Mathf.Max(num, 1);
		bool flag = num > 0;
		this.pathProcessor = new PathProcessor(this, this.pathReturnQueue, processors, flag);
		this.pathProcessor.OnPathPreSearch += delegate(Path path)
		{
			OnPathDelegate onPathPreSearch = AstarPath.OnPathPreSearch;
			if (onPathPreSearch != null)
			{
				onPathPreSearch(path);
			}
		};
		this.pathProcessor.OnPathPostSearch += delegate(Path path)
		{
			this.LogPathResults(path);
			OnPathDelegate onPathPostSearch = AstarPath.OnPathPostSearch;
			if (onPathPostSearch != null)
			{
				onPathPostSearch(path);
			}
		};
		this.pathProcessor.OnQueueUnblocked += delegate
		{
			if (this.euclideanEmbedding.dirty)
			{
				this.euclideanEmbedding.RecalculateCosts();
			}
		};
		if (flag)
		{
			this.graphUpdates.EnableMultithreading();
		}
	}

	internal void VerifyIntegrity()
	{
		if (AstarPath.active != this)
		{
			throw new Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
		}
		if (this.data == null)
		{
			throw new NullReferenceException("data is null... A* not set up correctly?");
		}
		if (this.data.graphs == null)
		{
			this.data.graphs = new NavGraph[0];
			this.data.UpdateShortcuts();
		}
	}

	public void ConfigureReferencesInternal()
	{
		AstarPath.active = this;
		this.data = (this.data ?? new AstarData());
		this.colorSettings = (this.colorSettings ?? new AstarColor());
		this.colorSettings.OnEnable();
	}

	private void InitializeProfiler()
	{
	}

	private void InitializeAstarData()
	{
		this.data.FindGraphTypes();
		this.data.Awake();
		this.data.UpdateShortcuts();
	}

	private void OnDisable()
	{
		this.gizmos.ClearCache();
	}

	private void OnDestroy()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");
		}
		if (AstarPath.active != this)
		{
			return;
		}
		this.PausePathfinding();
		this.euclideanEmbedding.dirty = false;
		this.FlushWorkItems();
		this.pathProcessor.queue.TerminateReceivers();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Processing Possible Work Items");
		}
		this.graphUpdates.DisableMultithreading();
		this.pathProcessor.JoinThreads();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Returning Paths");
		}
		this.pathReturnQueue.ReturnPaths(false);
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Destroying Graphs");
		}
		this.data.OnDestroy();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Cleaning up variables");
		}
		AstarPath.OnAwakeSettings = null;
		AstarPath.OnGraphPreScan = null;
		AstarPath.OnGraphPostScan = null;
		AstarPath.OnPathPreSearch = null;
		AstarPath.OnPathPostSearch = null;
		AstarPath.OnPreScan = null;
		AstarPath.OnPostScan = null;
		AstarPath.OnLatePostScan = null;
		AstarPath.On65KOverflow = null;
		AstarPath.OnGraphsUpdated = null;
		AstarPath.active = null;
	}

	public void FloodFill(GraphNode seed)
	{
		this.graphUpdates.FloodFill(seed);
	}

	public void FloodFill(GraphNode seed, uint area)
	{
		this.graphUpdates.FloodFill(seed, area);
	}

	[ContextMenu("Flood Fill Graphs")]
	public void FloodFill()
	{
		this.graphUpdates.FloodFill();
		this.workItems.OnFloodFill();
	}

	internal int GetNewNodeIndex()
	{
		return this.pathProcessor.GetNewNodeIndex();
	}

	internal void InitializeNode(GraphNode node)
	{
		this.pathProcessor.InitializeNode(node);
	}

	internal void DestroyNode(GraphNode node)
	{
		this.pathProcessor.DestroyNode(node);
	}

	[Obsolete("Use PausePathfinding instead. Make sure to call Release on the returned lock.", true)]
	public void BlockUntilPathQueueBlocked()
	{
	}

	public PathProcessor.GraphUpdateLock PausePathfinding()
	{
		return this.pathProcessor.PausePathfinding(true);
	}

	private PathProcessor.GraphUpdateLock PausePathfindingSoon()
	{
		return this.pathProcessor.PausePathfinding(false);
	}

	public void Scan(NavGraph graphToScan)
	{
		if (graphToScan == null)
		{
			throw new ArgumentNullException();
		}
		this.Scan(new NavGraph[]
		{
			graphToScan
		});
	}

	public void Scan(NavGraph[] graphsToScan = null)
	{
		foreach (Progress progress in this.ScanAsync(graphsToScan))
		{
		}
	}

	[Obsolete("ScanLoop is now named ScanAsync and is an IEnumerable<Progress>. Use foreach to iterate over the progress insead")]
	public void ScanLoop(OnScanStatus statusCallback)
	{
		foreach (Progress progress in this.ScanAsync(null))
		{
			statusCallback(progress);
		}
	}

	public IEnumerable<Progress> ScanAsync(NavGraph graphToScan)
	{
		if (graphToScan == null)
		{
			throw new ArgumentNullException();
		}
		return this.ScanAsync(new NavGraph[]
		{
			graphToScan
		});
	}

	public IEnumerable<Progress> ScanAsync(NavGraph[] graphsToScan = null)
	{
		if (graphsToScan == null)
		{
			graphsToScan = this.graphs;
		}
		if (graphsToScan == null)
		{
			yield break;
		}
		if (this.isScanning)
		{
			throw new InvalidOperationException("Another async scan is already running");
		}
		this.isScanning = true;
		this.VerifyIntegrity();
		PathProcessor.GraphUpdateLock graphUpdateLock = this.PausePathfinding();
		this.pathReturnQueue.ReturnPaths(false);
		if (!Application.isPlaying)
		{
			this.data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
		}
		yield return new Progress(0.05f, "Pre processing graphs");
		if (AstarPath.OnPreScan != null)
		{
			AstarPath.OnPreScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);
		this.data.LockGraphStructure(false);
		Stopwatch watch = Stopwatch.StartNew();
		for (int j = 0; j < graphsToScan.Length; j++)
		{
			if (graphsToScan[j] != null)
			{
				graphsToScan[j].DestroyAllNodesInternal();
			}
		}
		for (int i = 0; i < graphsToScan.Length; i++)
		{
			if (graphsToScan[i] != null)
			{
				float minp = Mathf.Lerp(0.1f, 0.8f, (float)i / (float)graphsToScan.Length);
				float maxp = Mathf.Lerp(0.1f, 0.8f, ((float)i + 0.95f) / (float)graphsToScan.Length);
				string progressDescriptionPrefix = string.Concat(new object[]
				{
					"Scanning graph ",
					i + 1,
					" of ",
					graphsToScan.Length,
					" - "
				});
				IEnumerator<Progress> coroutine = this.ScanGraph(graphsToScan[i]).GetEnumerator();
				for (;;)
				{
					try
					{
						if (!coroutine.MoveNext())
						{
							break;
						}
					}
					catch
					{
						this.isScanning = false;
						this.data.UnlockGraphStructure();
						graphUpdateLock.Release();
						throw;
					}
					float a = minp;
					float b = maxp;
					Progress progress = coroutine.Current;
					float p = Mathf.Lerp(a, b, progress.progress);
					string str = progressDescriptionPrefix;
					Progress progress2 = coroutine.Current;
					yield return new Progress(p, str + progress2.description);
				}
			}
		}
		this.data.UnlockGraphStructure();
		yield return new Progress(0.8f, "Post processing graphs");
		if (AstarPath.OnPostScan != null)
		{
			AstarPath.OnPostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);
		this.FlushWorkItems();
		yield return new Progress(0.9f, "Computing areas");
		this.FloodFill();
		yield return new Progress(0.95f, "Late post processing");
		this.isScanning = false;
		if (AstarPath.OnLatePostScan != null)
		{
			AstarPath.OnLatePostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);
		this.euclideanEmbedding.dirty = true;
		this.euclideanEmbedding.RecalculatePivots();
		this.FlushWorkItems();
		graphUpdateLock.Release();
		watch.Stop();
		this.lastScanTime = (float)watch.Elapsed.TotalSeconds;
		GC.Collect();
		this.Log("Scanning - Process took " + (this.lastScanTime * 1000f).ToString("0") + " ms to complete");
		yield break;
	}

	private IEnumerable<Progress> ScanGraph(NavGraph graph)
	{
		if (AstarPath.OnGraphPreScan != null)
		{
			yield return new Progress(0f, "Pre processing");
			AstarPath.OnGraphPreScan(graph);
		}
		yield return new Progress(0f, string.Empty);
		foreach (Progress p in graph.ScanInternal())
		{
			yield return new Progress(Mathf.Lerp(0f, 0.95f, p.progress), p.description);
		}
		yield return new Progress(0.95f, "Assigning graph indices");
		graph.GetNodes(delegate(GraphNode node)
		{
			node.GraphIndex = graph.graphIndex;
		});
		if (AstarPath.OnGraphPostScan != null)
		{
			yield return new Progress(0.99f, "Post processing");
			AstarPath.OnGraphPostScan(graph);
		}
		yield break;
	}

	[Obsolete("This method has been renamed to BlockUntilCalculated")]
	public static void WaitForPath(Path p)
	{
		AstarPath.BlockUntilCalculated(p);
	}

	public static void BlockUntilCalculated(Path p)
	{
		if (AstarPath.active == null)
		{
			throw new Exception("Pathfinding is not correctly initialized in this scene (yet?). AstarPath.active is null.\nDo not call this function in Awake");
		}
		if (p == null)
		{
			throw new ArgumentNullException("Path must not be null");
		}
		if (AstarPath.active.pathProcessor.queue.IsTerminating)
		{
			return;
		}
		if (p.PipelineState == PathState.Created)
		{
			throw new Exception("The specified path has not been started yet.");
		}
		AstarPath.waitForPathDepth++;
		if (AstarPath.waitForPathDepth == 5)
		{
			UnityEngine.Debug.LogError("You are calling the BlockUntilCalculated function recursively (maybe from a path callback). Please don't do this.");
		}
		if (p.PipelineState < PathState.ReturnQueue)
		{
			if (AstarPath.active.IsUsingMultithreading)
			{
				while (p.PipelineState < PathState.ReturnQueue)
				{
					if (AstarPath.active.pathProcessor.queue.IsTerminating)
					{
						AstarPath.waitForPathDepth--;
						throw new Exception("Pathfinding Threads seem to have crashed.");
					}
					Thread.Sleep(1);
					AstarPath.active.PerformBlockingActions(true);
				}
			}
			else
			{
				while (p.PipelineState < PathState.ReturnQueue)
				{
					if (AstarPath.active.pathProcessor.queue.IsEmpty && p.PipelineState != PathState.Processing)
					{
						AstarPath.waitForPathDepth--;
						throw new Exception("Critical error. Path Queue is empty but the path state is '" + p.PipelineState + "'");
					}
					AstarPath.active.pathProcessor.TickNonMultithreaded();
					AstarPath.active.PerformBlockingActions(true);
				}
			}
		}
		AstarPath.active.pathReturnQueue.ReturnPaths(false);
		AstarPath.waitForPathDepth--;
	}

	[Obsolete("The threadSafe parameter has been deprecated")]
	public static void RegisterSafeUpdate(Action callback, bool threadSafe)
	{
		AstarPath.RegisterSafeUpdate(callback);
	}

	[Obsolete("Use AddWorkItem(System.Action) instead. Note the slight change in behavior (mentioned in the documentation).")]
	public static void RegisterSafeUpdate(Action callback)
	{
		AstarPath.active.AddWorkItem(new AstarWorkItem(callback, null));
	}

	public static void StartPath(Path p, bool pushToFront = false)
	{
		AstarPath astarPath = AstarPath.active;
		if (object.ReferenceEquals(astarPath, null))
		{
			UnityEngine.Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
			return;
		}
		if (p.PipelineState != PathState.Created)
		{
			throw new Exception(string.Concat(new object[]
			{
				"The path has an invalid state. Expected ",
				PathState.Created,
				" found ",
				p.PipelineState,
				"\nMake sure you are not requesting the same path twice"
			}));
		}
		if (astarPath.pathProcessor.queue.IsTerminating)
		{
			p.Error();
			p.LogError("No new paths are accepted");
			return;
		}
		if (astarPath.graphs == null || astarPath.graphs.Length == 0)
		{
			UnityEngine.Debug.LogError("There are no graphs in the scene");
			p.Error();
			p.LogError("There are no graphs in the scene");
			UnityEngine.Debug.LogError(p.errorLog);
			return;
		}
		p.Claim(astarPath);
		((IPathInternals)p).AdvanceState(PathState.PathQueue);
		if (pushToFront)
		{
			astarPath.pathProcessor.queue.PushFront(p);
		}
		else
		{
			astarPath.pathProcessor.queue.Push(p);
		}
	}

	private void OnApplicationQuit()
	{
		this.OnDestroy();
		this.pathProcessor.AbortThreads();
	}

	public NNInfo GetNearest(Vector3 position)
	{
		return this.GetNearest(position, NNConstraint.None);
	}

	public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
	{
		return this.GetNearest(position, constraint, null);
	}

	public NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		NavGraph[] graphs = this.graphs;
		float num = float.PositiveInfinity;
		NNInfoInternal internalInfo = default(NNInfoInternal);
		int num2 = -1;
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				NavGraph navGraph = graphs[i];
				if (navGraph != null && constraint.SuitableGraph(i, navGraph))
				{
					NNInfoInternal nninfoInternal;
					if (this.fullGetNearestSearch)
					{
						nninfoInternal = navGraph.GetNearestForce(position, constraint);
					}
					else
					{
						nninfoInternal = navGraph.GetNearest(position, constraint);
					}
					if (nninfoInternal.node != null)
					{
						float magnitude = (nninfoInternal.clampedPosition - position).magnitude;
						if (this.prioritizeGraphs && magnitude < this.prioritizeGraphsLimit)
						{
							internalInfo = nninfoInternal;
							num2 = i;
							break;
						}
						if (magnitude < num)
						{
							num = magnitude;
							internalInfo = nninfoInternal;
							num2 = i;
						}
					}
				}
			}
		}
		if (num2 == -1)
		{
			return default(NNInfo);
		}
		if (internalInfo.constrainedNode != null)
		{
			internalInfo.node = internalInfo.constrainedNode;
			internalInfo.clampedPosition = internalInfo.constClampedPosition;
		}
		if (!this.fullGetNearestSearch && internalInfo.node != null && !constraint.Suitable(internalInfo.node))
		{
			NNInfoInternal nearestForce = graphs[num2].GetNearestForce(position, constraint);
			if (nearestForce.node != null)
			{
				internalInfo = nearestForce;
			}
		}
		if (!constraint.Suitable(internalInfo.node) || (constraint.constrainDistance && (internalInfo.clampedPosition - position).sqrMagnitude > this.maxNearestNodeDistanceSqr))
		{
			return default(NNInfo);
		}
		return new NNInfo(internalInfo);
	}

	public GraphNode GetNearest(Ray ray)
	{
		if (this.graphs == null)
		{
			return null;
		}
		float minDist = float.PositiveInfinity;
		GraphNode nearestNode = null;
		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;
		for (int i = 0; i < this.graphs.Length; i++)
		{
			NavGraph navGraph = this.graphs[i];
			navGraph.GetNodes(delegate(GraphNode node)
			{
				Vector3 vector = (Vector3)node.position;
				Vector3 a = lineOrigin + Vector3.Dot(vector - lineOrigin, lineDirection) * lineDirection;
				float num = Mathf.Abs(a.x - vector.x);
				num *= num;
				if (num > minDist)
				{
					return;
				}
				num = Mathf.Abs(a.z - vector.z);
				num *= num;
				if (num > minDist)
				{
					return;
				}
				float sqrMagnitude = (a - vector).sqrMagnitude;
				if (sqrMagnitude < minDist)
				{
					minDist = sqrMagnitude;
					nearestNode = node;
				}
			});
		}
		return nearestNode;
	}

	public static readonly AstarPath.AstarDistribution Distribution = AstarPath.AstarDistribution.AssetStore;

	public static readonly string Branch = "master_Pro";

	[FormerlySerializedAs("astarData")]
	public AstarData data;

	public static AstarPath active;

	public bool showNavGraphs = true;

	public bool showUnwalkableNodes = true;

	public GraphDebugMode debugMode;

	public float debugFloor;

	public float debugRoof = 20000f;

	public bool manualDebugFloorRoof;

	public bool showSearchTree;

	public float unwalkableNodeDebugSize = 0.3f;

	public PathLog logPathResults = PathLog.Normal;

	public float maxNearestNodeDistance = 100f;

	public bool scanOnStartup = true;

	public bool fullGetNearestSearch;

	public bool prioritizeGraphs;

	public float prioritizeGraphsLimit = 1f;

	public AstarColor colorSettings;

	[SerializeField]
	protected string[] tagNames;

	public Heuristic heuristic = Heuristic.Euclidean;

	public float heuristicScale = 1f;

	public ThreadCount threadCount;

	public float maxFrameTime = 1f;

	[Obsolete("Minimum area size is mostly obsolete since the limit has been raised significantly, and the edge cases are handled automatically")]
	public int minAreaSize;

	public bool batchGraphUpdates;

	public float graphUpdateBatchingInterval = 0.2f;

	[NonSerialized]
	public PathHandler debugPathData;

	[NonSerialized]
	public ushort debugPathID;

	private string inGameDebugPath;

	[NonSerialized]
	private bool isScanningBacking;

	public static Action OnAwakeSettings;

	public static OnGraphDelegate OnGraphPreScan;

	public static OnGraphDelegate OnGraphPostScan;

	public static OnPathDelegate OnPathPreSearch;

	public static OnPathDelegate OnPathPostSearch;

	public static OnScanDelegate OnPreScan;

	public static OnScanDelegate OnPostScan;

	public static OnScanDelegate OnLatePostScan;

	public static OnScanDelegate OnGraphsUpdated;

	public static Action On65KOverflow;

	[Obsolete]
	public Action OnGraphsWillBeUpdated;

	[Obsolete]
	public Action OnGraphsWillBeUpdated2;

	private readonly GraphUpdateProcessor graphUpdates;

	private readonly WorkItemProcessor workItems;

	private PathProcessor pathProcessor;

	private bool graphUpdateRoutineRunning;

	private bool graphUpdatesWorkItemAdded;

	private float lastGraphUpdate = -9999f;

	private PathProcessor.GraphUpdateLock workItemLock;

	internal readonly PathReturnQueue pathReturnQueue;

	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	public bool showGraphs;

	private ushort nextFreePathID = 1;

	private RetainedGizmos gizmos = new RetainedGizmos();

	private static int waitForPathDepth;

	public enum AstarDistribution
	{
		WebsiteDownload,
		AssetStore
	}
}
