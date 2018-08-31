using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_tile_handler_helper.php")]
	public class TileHandlerHelper : VersionedMonoBehaviour
	{
		public void UseSpecifiedHandler(TileHandler newHandler)
		{
			if (!base.enabled)
			{
				throw new InvalidOperationException("TileHandlerHelper is disabled");
			}
			if (this.handler != null)
			{
				NavmeshClipper.RemoveEnableCallback(new Action<NavmeshClipper>(this.HandleOnEnableCallback), new Action<NavmeshClipper>(this.HandleOnDisableCallback));
				NavmeshBase graph = this.handler.graph;
				graph.OnRecalculatedTiles = (Action<NavmeshTile[]>)Delegate.Remove(graph.OnRecalculatedTiles, new Action<NavmeshTile[]>(this.OnRecalculatedTiles));
			}
			this.handler = newHandler;
			if (this.handler != null)
			{
				NavmeshClipper.AddEnableCallback(new Action<NavmeshClipper>(this.HandleOnEnableCallback), new Action<NavmeshClipper>(this.HandleOnDisableCallback));
				NavmeshBase graph2 = this.handler.graph;
				graph2.OnRecalculatedTiles = (Action<NavmeshTile[]>)Delegate.Combine(graph2.OnRecalculatedTiles, new Action<NavmeshTile[]>(this.OnRecalculatedTiles));
			}
		}

		private void OnEnable()
		{
			if (this.handler != null)
			{
				NavmeshClipper.AddEnableCallback(new Action<NavmeshClipper>(this.HandleOnEnableCallback), new Action<NavmeshClipper>(this.HandleOnDisableCallback));
				NavmeshBase graph = this.handler.graph;
				graph.OnRecalculatedTiles = (Action<NavmeshTile[]>)Delegate.Combine(graph.OnRecalculatedTiles, new Action<NavmeshTile[]>(this.OnRecalculatedTiles));
			}
			this.forcedReloadRects.Clear();
		}

		private void OnDisable()
		{
			if (this.handler != null)
			{
				NavmeshClipper.RemoveEnableCallback(new Action<NavmeshClipper>(this.HandleOnEnableCallback), new Action<NavmeshClipper>(this.HandleOnDisableCallback));
				this.forcedReloadRects.Clear();
				NavmeshBase graph = this.handler.graph;
				graph.OnRecalculatedTiles = (Action<NavmeshTile[]>)Delegate.Remove(graph.OnRecalculatedTiles, new Action<NavmeshTile[]>(this.OnRecalculatedTiles));
			}
		}

		public void DiscardPending()
		{
			if (this.handler != null)
			{
				for (GridLookup<NavmeshClipper>.Root root = this.handler.cuts.AllItems; root != null; root = root.next)
				{
					if (root.obj.RequiresUpdate())
					{
						root.obj.NotifyUpdated();
					}
				}
			}
			this.forcedReloadRects.Clear();
		}

		private void Start()
		{
			if (UnityEngine.Object.FindObjectsOfType(typeof(TileHandlerHelper)).Length > 1)
			{
				Debug.LogError("There should only be one TileHandlerHelper per scene. Destroying.");
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (this.handler == null)
			{
				this.FindGraph();
			}
		}

		private void FindGraph()
		{
			if (AstarPath.active != null)
			{
				NavmeshBase navmeshBase = AstarPath.active.data.recastGraph ?? AstarPath.active.data.navmesh;
				if (navmeshBase != null)
				{
					this.UseSpecifiedHandler(new TileHandler(navmeshBase));
					this.handler.CreateTileTypesFromGraph();
				}
			}
		}

		private void OnRecalculatedTiles(NavmeshTile[] tiles)
		{
			if (!this.handler.isValid)
			{
				this.UseSpecifiedHandler(new TileHandler(this.handler.graph));
			}
			this.handler.OnRecalculatedTiles(tiles);
		}

		private void HandleOnEnableCallback(NavmeshClipper obj)
		{
			Rect bounds = obj.GetBounds(this.handler.graph.transform);
			IntRect touchingTilesInGraphSpace = this.handler.graph.GetTouchingTilesInGraphSpace(bounds);
			this.handler.cuts.Add(obj, touchingTilesInGraphSpace);
			obj.ForceUpdate();
		}

		private void HandleOnDisableCallback(NavmeshClipper obj)
		{
			GridLookup<NavmeshClipper>.Root root = this.handler.cuts.GetRoot(obj);
			if (root != null)
			{
				this.forcedReloadRects.Add(root.previousBounds);
				this.handler.cuts.Remove(obj);
			}
			this.lastUpdateTime = float.NegativeInfinity;
		}

		private void Update()
		{
			if (this.handler == null)
			{
				this.FindGraph();
			}
			if (this.handler != null && !AstarPath.active.isScanning && ((this.updateInterval >= 0f && Time.realtimeSinceStartup - this.lastUpdateTime > this.updateInterval) || !this.handler.isValid))
			{
				this.ForceUpdate();
			}
		}

		public void ForceUpdate()
		{
			if (this.handler == null)
			{
				throw new Exception("Cannot update graphs. No TileHandler. Do not call the ForceUpdate method in Awake.");
			}
			this.lastUpdateTime = Time.realtimeSinceStartup;
			if (!this.handler.isValid)
			{
				if (!this.handler.graph.exists)
				{
					this.UseSpecifiedHandler(null);
					return;
				}
				Debug.Log("TileHandler no longer matched the underlaying graph (possibly because of a graph scan). Recreating TileHandler...");
				this.UseSpecifiedHandler(new TileHandler(this.handler.graph));
				this.handler.CreateTileTypesFromGraph();
				this.forcedReloadRects.Add(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
			}
			GridLookup<NavmeshClipper>.Root allItems = this.handler.cuts.AllItems;
			if (this.forcedReloadRects.Count == 0)
			{
				int num = 0;
				for (GridLookup<NavmeshClipper>.Root root = allItems; root != null; root = root.next)
				{
					if (root.obj.RequiresUpdate())
					{
						num++;
						break;
					}
				}
				if (num == 0)
				{
					return;
				}
			}
			bool flag = this.handler.StartBatchLoad();
			for (int i = 0; i < this.forcedReloadRects.Count; i++)
			{
				this.handler.ReloadInBounds(this.forcedReloadRects[i]);
			}
			this.forcedReloadRects.Clear();
			for (GridLookup<NavmeshClipper>.Root root2 = allItems; root2 != null; root2 = root2.next)
			{
				if (root2.obj.RequiresUpdate())
				{
					this.handler.ReloadInBounds(root2.previousBounds);
					Rect bounds = root2.obj.GetBounds(this.handler.graph.transform);
					IntRect touchingTilesInGraphSpace = this.handler.graph.GetTouchingTilesInGraphSpace(bounds);
					this.handler.cuts.Move(root2.obj, touchingTilesInGraphSpace);
					this.handler.ReloadInBounds(touchingTilesInGraphSpace);
				}
			}
			for (GridLookup<NavmeshClipper>.Root root3 = allItems; root3 != null; root3 = root3.next)
			{
				if (root3.obj.RequiresUpdate())
				{
					root3.obj.NotifyUpdated();
				}
			}
			if (flag)
			{
				this.handler.EndBatchLoad();
			}
		}

		private TileHandler handler;

		public float updateInterval;

		private float lastUpdateTime = float.NegativeInfinity;

		private readonly List<IntRect> forcedReloadRects = new List<IntRect>();
	}
}
