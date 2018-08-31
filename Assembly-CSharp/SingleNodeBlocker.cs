using System;
using Pathfinding;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_single_node_blocker.php")]
public class SingleNodeBlocker : VersionedMonoBehaviour
{
	public GraphNode lastBlocked { get; private set; }

	public void BlockAtCurrentPosition()
	{
		this.BlockAt(base.transform.position);
	}

	public void BlockAt(Vector3 position)
	{
		this.Unblock();
		GraphNode node = AstarPath.active.GetNearest(position, NNConstraint.None).node;
		if (node != null)
		{
			this.Block(node);
		}
	}

	public void Block(GraphNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		this.manager.InternalBlock(node, this);
		this.lastBlocked = node;
	}

	public void Unblock()
	{
		if (this.lastBlocked == null || this.lastBlocked.Destroyed)
		{
			this.lastBlocked = null;
			return;
		}
		this.manager.InternalUnblock(this.lastBlocked, this);
		this.lastBlocked = null;
	}

	public BlockManager manager;
}
