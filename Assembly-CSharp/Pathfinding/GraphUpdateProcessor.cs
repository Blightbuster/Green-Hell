﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	internal class GraphUpdateProcessor
	{
		public GraphUpdateProcessor(AstarPath astar)
		{
			this.astar = astar;
		}

		public event Action OnGraphsUpdated;

		public bool IsAnyGraphUpdateQueued
		{
			get
			{
				return this.graphUpdateQueue.Count > 0;
			}
		}

		public bool IsAnyGraphUpdateInProgress
		{
			get
			{
				return this.anyGraphUpdateInProgress;
			}
		}

		public AstarWorkItem GetWorkItem()
		{
			return new AstarWorkItem(new Action(this.QueueGraphUpdatesInternal), new Func<bool, bool>(this.ProcessGraphUpdates));
		}

		public void EnableMultithreading()
		{
			if (this.graphUpdateThread == null || !this.graphUpdateThread.IsAlive)
			{
				this.graphUpdateThread = new Thread(new ThreadStart(this.ProcessGraphUpdatesAsync));
				this.graphUpdateThread.IsBackground = true;
				this.graphUpdateThread.Priority = System.Threading.ThreadPriority.Lowest;
				this.graphUpdateThread.Start(this);
			}
		}

		public void DisableMultithreading()
		{
			if (this.graphUpdateThread != null && this.graphUpdateThread.IsAlive)
			{
				this.exitAsyncThread.Set();
				if (!this.graphUpdateThread.Join(5000))
				{
					Debug.LogError("Graph update thread did not exit in 5 seconds");
				}
				this.graphUpdateThread = null;
			}
		}

		public void AddToQueue(GraphUpdateObject ob)
		{
			this.graphUpdateQueue.Enqueue(ob);
		}

		private void QueueGraphUpdatesInternal()
		{
			bool flag = false;
			while (this.graphUpdateQueue.Count > 0)
			{
				GraphUpdateObject graphUpdateObject = this.graphUpdateQueue.Dequeue();
				if (graphUpdateObject.requiresFloodFill)
				{
					flag = true;
				}
				IEnumerator enumerator = this.astar.data.GetUpdateableGraphs().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						IUpdatableGraph updatableGraph = (IUpdatableGraph)obj;
						NavGraph graph = updatableGraph as NavGraph;
						if (graphUpdateObject.nnConstraint == null || graphUpdateObject.nnConstraint.SuitableGraph(this.astar.data.GetGraphIndex(graph), graph))
						{
							GraphUpdateProcessor.GUOSingle item = default(GraphUpdateProcessor.GUOSingle);
							item.order = GraphUpdateProcessor.GraphUpdateOrder.GraphUpdate;
							item.obj = graphUpdateObject;
							item.graph = updatableGraph;
							this.graphUpdateQueueRegular.Enqueue(item);
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (flag)
			{
				GraphUpdateProcessor.GUOSingle item2 = default(GraphUpdateProcessor.GUOSingle);
				item2.order = GraphUpdateProcessor.GraphUpdateOrder.FloodFill;
				this.graphUpdateQueueRegular.Enqueue(item2);
			}
			GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
			this.anyGraphUpdateInProgress = true;
		}

		private bool ProcessGraphUpdates(bool force)
		{
			if (force)
			{
				this.asyncGraphUpdatesComplete.WaitOne();
			}
			else if (!this.asyncGraphUpdatesComplete.WaitOne(0))
			{
				return false;
			}
			this.ProcessPostUpdates();
			if (!this.ProcessRegularUpdates(force))
			{
				return false;
			}
			GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
			if (this.OnGraphsUpdated != null)
			{
				this.OnGraphsUpdated();
			}
			this.anyGraphUpdateInProgress = false;
			return true;
		}

		private bool ProcessRegularUpdates(bool force)
		{
			while (this.graphUpdateQueueRegular.Count > 0)
			{
				GraphUpdateProcessor.GUOSingle item = this.graphUpdateQueueRegular.Peek();
				GraphUpdateThreading graphUpdateThreading = (item.order != GraphUpdateProcessor.GraphUpdateOrder.FloodFill) ? item.graph.CanUpdateAsync(item.obj) : GraphUpdateThreading.SeparateThread;
				if (force || !Application.isPlaying || this.graphUpdateThread == null || !this.graphUpdateThread.IsAlive)
				{
					graphUpdateThreading &= (GraphUpdateThreading)(-2);
				}
				if ((graphUpdateThreading & GraphUpdateThreading.UnityInit) != GraphUpdateThreading.UnityThread)
				{
					if (this.StartAsyncUpdatesIfQueued())
					{
						return false;
					}
					item.graph.UpdateAreaInit(item.obj);
				}
				if ((graphUpdateThreading & GraphUpdateThreading.SeparateThread) != GraphUpdateThreading.UnityThread)
				{
					this.graphUpdateQueueRegular.Dequeue();
					this.graphUpdateQueueAsync.Enqueue(item);
					if ((graphUpdateThreading & GraphUpdateThreading.UnityPost) != GraphUpdateThreading.UnityThread && this.StartAsyncUpdatesIfQueued())
					{
						return false;
					}
				}
				else
				{
					if (this.StartAsyncUpdatesIfQueued())
					{
						return false;
					}
					this.graphUpdateQueueRegular.Dequeue();
					if (item.order == GraphUpdateProcessor.GraphUpdateOrder.FloodFill)
					{
						this.FloodFill();
					}
					else
					{
						try
						{
							item.graph.UpdateArea(item.obj);
						}
						catch (Exception arg)
						{
							Debug.LogError("Error while updating graphs\n" + arg);
						}
					}
					if ((graphUpdateThreading & GraphUpdateThreading.UnityPost) != GraphUpdateThreading.UnityThread)
					{
						item.graph.UpdateAreaPost(item.obj);
					}
				}
			}
			return !this.StartAsyncUpdatesIfQueued();
		}

		private bool StartAsyncUpdatesIfQueued()
		{
			if (this.graphUpdateQueueAsync.Count > 0)
			{
				this.asyncGraphUpdatesComplete.Reset();
				this.graphUpdateAsyncEvent.Set();
				return true;
			}
			return false;
		}

		private void ProcessPostUpdates()
		{
			while (this.graphUpdateQueuePost.Count > 0)
			{
				GraphUpdateProcessor.GUOSingle guosingle = this.graphUpdateQueuePost.Dequeue();
				GraphUpdateThreading graphUpdateThreading = guosingle.graph.CanUpdateAsync(guosingle.obj);
				if ((graphUpdateThreading & GraphUpdateThreading.UnityPost) != GraphUpdateThreading.UnityThread)
				{
					try
					{
						guosingle.graph.UpdateAreaPost(guosingle.obj);
					}
					catch (Exception arg)
					{
						Debug.LogError("Error while updating graphs (post step)\n" + arg);
					}
				}
			}
		}

		private void ProcessGraphUpdatesAsync()
		{
			AutoResetEvent[] waitHandles = new AutoResetEvent[]
			{
				this.graphUpdateAsyncEvent,
				this.exitAsyncThread
			};
			for (;;)
			{
				int num = WaitHandle.WaitAny(waitHandles);
				if (num == 1)
				{
					break;
				}
				while (this.graphUpdateQueueAsync.Count > 0)
				{
					GraphUpdateProcessor.GUOSingle item = this.graphUpdateQueueAsync.Dequeue();
					try
					{
						if (item.order == GraphUpdateProcessor.GraphUpdateOrder.GraphUpdate)
						{
							item.graph.UpdateArea(item.obj);
							this.graphUpdateQueuePost.Enqueue(item);
						}
						else
						{
							if (item.order != GraphUpdateProcessor.GraphUpdateOrder.FloodFill)
							{
								throw new NotSupportedException(string.Empty + item.order);
							}
							this.FloodFill();
						}
					}
					catch (Exception arg)
					{
						Debug.LogError("Exception while updating graphs:\n" + arg);
					}
				}
				this.asyncGraphUpdatesComplete.Set();
			}
			this.graphUpdateQueueAsync.Clear();
			this.asyncGraphUpdatesComplete.Set();
		}

		public void FloodFill(GraphNode seed)
		{
			this.FloodFill(seed, this.lastUniqueAreaIndex + 1u);
			this.lastUniqueAreaIndex += 1u;
		}

		public void FloodFill(GraphNode seed, uint area)
		{
			if (area > 131071u)
			{
				Debug.LogError("Too high area index - The maximum area index is " + 131071u);
				return;
			}
			if (area < 0u)
			{
				Debug.LogError("Too low area index - The minimum area index is 0");
				return;
			}
			Stack<GraphNode> stack = StackPool<GraphNode>.Claim();
			stack.Push(seed);
			seed.Area = area;
			while (stack.Count > 0)
			{
				stack.Pop().FloodFill(stack, area);
			}
			StackPool<GraphNode>.Release(stack);
		}

		public void FloodFill()
		{
			NavGraph[] graphs = this.astar.graphs;
			if (graphs == null)
			{
				return;
			}
			foreach (NavGraph navGraph in graphs)
			{
				if (navGraph != null)
				{
					navGraph.GetNodes(delegate(GraphNode node)
					{
						node.Area = 0u;
					});
				}
			}
			this.lastUniqueAreaIndex = 0u;
			uint area = 0u;
			int forcedSmallAreas = 0;
			Stack<GraphNode> stack = StackPool<GraphNode>.Claim();
			foreach (NavGraph navGraph2 in graphs)
			{
				if (navGraph2 != null)
				{
					navGraph2.GetNodes(delegate(GraphNode node)
					{
						if (node.Walkable && node.Area == 0u)
						{
							uint area;
							area += 1u;
							area = area;
							if (area > 131071u)
							{
								area -= 1u;
								area = area;
								if (forcedSmallAreas == 0)
								{
									forcedSmallAreas = 1;
								}
								forcedSmallAreas++;
							}
							stack.Clear();
							stack.Push(node);
							int num = 1;
							node.Area = area;
							while (stack.Count > 0)
							{
								num++;
								stack.Pop().FloodFill(stack, area);
							}
						}
					});
				}
			}
			this.lastUniqueAreaIndex = area;
			if (forcedSmallAreas > 0)
			{
				Debug.LogError(string.Concat(new object[]
				{
					forcedSmallAreas,
					" areas had to share IDs. This usually doesn't affect pathfinding in any significant way (you might get 'Searched whole area but could not find target' as a reason for path failure) however some path requests may take longer to calculate (specifically those that fail with the 'Searched whole area' error).The maximum number of areas is ",
					131071u,
					"."
				}));
			}
			StackPool<GraphNode>.Release(stack);
		}

		private readonly AstarPath astar;

		private Thread graphUpdateThread;

		private bool anyGraphUpdateInProgress;

		private readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

		private readonly Queue<GraphUpdateProcessor.GUOSingle> graphUpdateQueueAsync = new Queue<GraphUpdateProcessor.GUOSingle>();

		private readonly Queue<GraphUpdateProcessor.GUOSingle> graphUpdateQueuePost = new Queue<GraphUpdateProcessor.GUOSingle>();

		private readonly Queue<GraphUpdateProcessor.GUOSingle> graphUpdateQueueRegular = new Queue<GraphUpdateProcessor.GUOSingle>();

		private readonly ManualResetEvent asyncGraphUpdatesComplete = new ManualResetEvent(true);

		private readonly AutoResetEvent graphUpdateAsyncEvent = new AutoResetEvent(false);

		private readonly AutoResetEvent exitAsyncThread = new AutoResetEvent(false);

		private uint lastUniqueAreaIndex;

		private enum GraphUpdateOrder
		{
			GraphUpdate,
			FloodFill
		}

		private struct GUOSingle
		{
			public GraphUpdateProcessor.GraphUpdateOrder order;

			public IUpdatableGraph graph;

			public GraphUpdateObject obj;
		}
	}
}
