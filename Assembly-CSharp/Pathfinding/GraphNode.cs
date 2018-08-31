﻿using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding
{
	public abstract class GraphNode
	{
		protected GraphNode(AstarPath astar)
		{
			if (!object.ReferenceEquals(astar, null))
			{
				this.nodeIndex = astar.GetNewNodeIndex();
				astar.InitializeNode(this);
				return;
			}
			throw new Exception("No active AstarPath object to bind to");
		}

		internal void Destroy()
		{
			if (this.Destroyed)
			{
				return;
			}
			this.ClearConnections(true);
			if (AstarPath.active != null)
			{
				AstarPath.active.DestroyNode(this);
			}
			this.nodeIndex = -1;
		}

		public bool Destroyed
		{
			get
			{
				return this.nodeIndex == -1;
			}
		}

		public int NodeIndex
		{
			get
			{
				return this.nodeIndex;
			}
		}

		public uint Flags
		{
			get
			{
				return this.flags;
			}
			set
			{
				this.flags = value;
			}
		}

		public uint Penalty
		{
			get
			{
				return this.penalty;
			}
			set
			{
				if (value > 16777215u)
				{
					Debug.LogWarning("Very high penalty applied. Are you sure negative values haven't underflowed?\nPenalty values this high could with long paths cause overflows and in some cases infinity loops because of that.\nPenalty value applied: " + value);
				}
				this.penalty = value;
			}
		}

		public bool Walkable
		{
			get
			{
				return (this.flags & 1u) != 0u;
			}
			set
			{
				this.flags = ((this.flags & 4294967294u) | ((!value) ? 0u : 1u) << 0);
			}
		}

		public uint Area
		{
			get
			{
				return (this.flags & 262142u) >> 1;
			}
			set
			{
				this.flags = ((this.flags & 4294705153u) | value << 1);
			}
		}

		public uint GraphIndex
		{
			get
			{
				return (this.flags & 4278190080u) >> 24;
			}
			set
			{
				this.flags = ((this.flags & 16777215u) | value << 24);
			}
		}

		public uint Tag
		{
			get
			{
				return (this.flags & 16252928u) >> 19;
			}
			set
			{
				this.flags = ((this.flags & 4278714367u) | value << 19);
			}
		}

		public void UpdateG(Path path, PathNode pathNode)
		{
			pathNode.G = pathNode.parent.G + pathNode.cost + path.GetTraversalCost(this);
		}

		public virtual void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
		{
			this.UpdateG(path, pathNode);
			handler.heap.Add(pathNode);
			this.GetConnections(delegate(GraphNode other)
			{
				PathNode pathNode2 = handler.GetPathNode(other);
				if (pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
				{
					other.UpdateRecursiveG(path, pathNode2, handler);
				}
			});
		}

		public virtual void FloodFill(Stack<GraphNode> stack, uint region)
		{
			this.GetConnections(delegate(GraphNode other)
			{
				if (other.Area != region)
				{
					other.Area = region;
					stack.Push(other);
				}
			});
		}

		public abstract void GetConnections(Action<GraphNode> action);

		public abstract void AddConnection(GraphNode node, uint cost);

		public abstract void RemoveConnection(GraphNode node);

		public abstract void ClearConnections(bool alsoReverse);

		public virtual bool ContainsConnection(GraphNode node)
		{
			bool contains = false;
			this.GetConnections(delegate(GraphNode neighbour)
			{
				contains |= (neighbour == node);
			});
			return contains;
		}

		public virtual void RecalculateConnectionCosts()
		{
		}

		public virtual bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
		{
			return false;
		}

		public abstract void Open(Path path, PathNode pathNode, PathHandler handler);

		public virtual float SurfaceArea()
		{
			return 0f;
		}

		public virtual Vector3 RandomPointOnSurface()
		{
			return (Vector3)this.position;
		}

		public virtual int GetGizmoHashCode()
		{
			return this.position.GetHashCode() ^ (int)(19u * this.Penalty) ^ (int)(41u * this.flags);
		}

		public virtual void SerializeNode(GraphSerializationContext ctx)
		{
			ctx.writer.Write(this.Penalty);
			ctx.writer.Write(this.Flags);
		}

		public virtual void DeserializeNode(GraphSerializationContext ctx)
		{
			this.Penalty = ctx.reader.ReadUInt32();
			this.Flags = ctx.reader.ReadUInt32();
			this.GraphIndex = ctx.graphIndex;
		}

		public virtual void SerializeReferences(GraphSerializationContext ctx)
		{
		}

		public virtual void DeserializeReferences(GraphSerializationContext ctx)
		{
		}

		private int nodeIndex;

		protected uint flags;

		private uint penalty;

		public Int3 position;

		private const int FlagsWalkableOffset = 0;

		private const uint FlagsWalkableMask = 1u;

		private const int FlagsAreaOffset = 1;

		private const uint FlagsAreaMask = 262142u;

		private const int FlagsGraphOffset = 24;

		private const uint FlagsGraphMask = 4278190080u;

		public const uint MaxAreaIndex = 131071u;

		public const uint MaxGraphIndex = 255u;

		private const int FlagsTagOffset = 19;

		private const uint FlagsTagMask = 16252928u;
	}
}
