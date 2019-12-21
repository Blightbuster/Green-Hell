using System;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_block_manager.php")]
public class BlockManager : VersionedMonoBehaviour
{
	private void Start()
	{
		if (!AstarPath.active)
		{
			throw new Exception("No AstarPath object in the scene");
		}
	}

	public bool NodeContainsAnyOf(GraphNode node, List<SingleNodeBlocker> selector)
	{
		List<SingleNodeBlocker> list;
		if (!this.blocked.TryGetValue(node, out list))
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			SingleNodeBlocker singleNodeBlocker = list[i];
			for (int j = 0; j < selector.Count; j++)
			{
				if (singleNodeBlocker == selector[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool NodeContainsAnyExcept(GraphNode node, List<SingleNodeBlocker> selector)
	{
		List<SingleNodeBlocker> list;
		if (!this.blocked.TryGetValue(node, out list))
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			SingleNodeBlocker singleNodeBlocker = list[i];
			bool flag = false;
			for (int j = 0; j < selector.Count; j++)
			{
				if (singleNodeBlocker == selector[j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public void InternalBlock(GraphNode node, SingleNodeBlocker blocker)
	{
		AstarPath.active.AddWorkItem(new AstarWorkItem(delegate
		{
			List<SingleNodeBlocker> list;
			if (!this.blocked.TryGetValue(node, out list))
			{
				list = (this.blocked[node] = ListPool<SingleNodeBlocker>.Claim());
			}
			list.Add(blocker);
		}, null));
	}

	public void InternalUnblock(GraphNode node, SingleNodeBlocker blocker)
	{
		AstarPath.active.AddWorkItem(new AstarWorkItem(delegate
		{
			List<SingleNodeBlocker> list;
			if (this.blocked.TryGetValue(node, out list))
			{
				list.Remove(blocker);
				if (list.Count == 0)
				{
					this.blocked.Remove(node);
					ListPool<SingleNodeBlocker>.Release(list);
				}
			}
		}, null));
	}

	private Dictionary<GraphNode, List<SingleNodeBlocker>> blocked = new Dictionary<GraphNode, List<SingleNodeBlocker>>();

	public enum BlockMode
	{
		AllExceptSelector,
		OnlySelector
	}

	public class TraversalProvider : ITraversalProvider
	{
		public BlockManager.BlockMode mode { get; private set; }

		public TraversalProvider(BlockManager blockManager, BlockManager.BlockMode mode, List<SingleNodeBlocker> selector)
		{
			if (blockManager == null)
			{
				throw new ArgumentNullException("blockManager");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			this.blockManager = blockManager;
			this.mode = mode;
			this.selector = selector;
		}

		public bool CanTraverse(Path path, GraphNode node)
		{
			if (!node.Walkable || (path.enabledTags >> (int)node.Tag & 1) == 0)
			{
				return false;
			}
			if (this.mode == BlockManager.BlockMode.OnlySelector)
			{
				return !this.blockManager.NodeContainsAnyOf(node, this.selector);
			}
			return !this.blockManager.NodeContainsAnyExcept(node, this.selector);
		}

		public uint GetTraversalCost(Path path, GraphNode node)
		{
			return path.GetTagPenalty((int)node.Tag) + node.Penalty;
		}

		private readonly BlockManager blockManager;

		private readonly List<SingleNodeBlocker> selector;
	}
}
