using System;
using UnityEngine;

namespace Pathfinding
{
	public struct NNInfoInternal
	{
		public NNInfoInternal(GraphNode node)
		{
			this.node = node;
			this.constrainedNode = null;
			this.clampedPosition = Vector3.zero;
			this.constClampedPosition = Vector3.zero;
			this.UpdateInfo();
		}

		public void UpdateInfo()
		{
			this.clampedPosition = ((this.node == null) ? Vector3.zero : ((Vector3)this.node.position));
			this.constClampedPosition = ((this.constrainedNode == null) ? Vector3.zero : ((Vector3)this.constrainedNode.position));
		}

		public GraphNode node;

		public GraphNode constrainedNode;

		public Vector3 clampedPosition;

		public Vector3 constClampedPosition;
	}
}
