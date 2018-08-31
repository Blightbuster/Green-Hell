using System;
using System.Collections.Generic;

namespace Pathfinding
{
	public class PointKDTree
	{
		public PointKDTree()
		{
			this.tree[1] = new PointKDTree.Node
			{
				data = this.GetOrCreateList()
			};
		}

		public void Add(GraphNode node)
		{
			this.numNodes++;
			this.Add(node, 1, 0);
		}

		public void Rebuild(GraphNode[] nodes, int start, int end)
		{
			if (start < 0 || end < start || end > nodes.Length)
			{
				throw new ArgumentException();
			}
			for (int i = 0; i < this.tree.Length; i++)
			{
				if (this.tree[i].data != null)
				{
					this.tree[i].data.Clear();
					this.listCache.Push(this.tree[i].data);
					this.tree[i].data = null;
				}
			}
			this.numNodes = end - start;
			this.Build(1, new List<GraphNode>(nodes), start, end);
		}

		private List<GraphNode> GetOrCreateList()
		{
			return (this.listCache.Count <= 0) ? new List<GraphNode>(PointKDTree.LeafSize * 2 + 1) : this.listCache.Pop();
		}

		private int Size(int index)
		{
			return (this.tree[index].data == null) ? (this.Size(2 * index) + this.Size(2 * index + 1)) : this.tree[index].data.Count;
		}

		private void CollectAndClear(int index, List<GraphNode> buffer)
		{
			List<GraphNode> data = this.tree[index].data;
			if (data != null)
			{
				this.tree[index] = default(PointKDTree.Node);
				for (int i = 0; i < data.Count; i++)
				{
					buffer.Add(data[i]);
				}
				data.Clear();
				this.listCache.Push(data);
			}
			else
			{
				this.CollectAndClear(index * 2, buffer);
				this.CollectAndClear(index * 2 + 1, buffer);
			}
		}

		private static int MaxAllowedSize(int numNodes, int depth)
		{
			return Math.Min(5 * numNodes / 2 >> depth, 3 * numNodes / 4);
		}

		private void Rebalance(int index)
		{
			this.CollectAndClear(index, this.largeList);
			this.Build(index, this.largeList, 0, this.largeList.Count);
			this.largeList.Clear();
		}

		private void EnsureSize(int index)
		{
			if (index >= this.tree.Length)
			{
				PointKDTree.Node[] array = new PointKDTree.Node[Math.Max(index + 1, this.tree.Length * 2)];
				this.tree.CopyTo(array, 0);
				this.tree = array;
			}
		}

		private void Build(int index, List<GraphNode> nodes, int start, int end)
		{
			this.EnsureSize(index);
			if (end - start <= PointKDTree.LeafSize)
			{
				this.tree[index].data = this.GetOrCreateList();
				for (int i = start; i < end; i++)
				{
					this.tree[index].data.Add(nodes[i]);
				}
			}
			else
			{
				Int3 position;
				Int3 lhs = position = nodes[start].position;
				for (int j = start; j < end; j++)
				{
					Int3 position2 = nodes[j].position;
					position = new Int3(Math.Min(position.x, position2.x), Math.Min(position.y, position2.y), Math.Min(position.z, position2.z));
					lhs = new Int3(Math.Max(lhs.x, position2.x), Math.Max(lhs.y, position2.y), Math.Max(lhs.z, position2.z));
				}
				Int3 @int = lhs - position;
				int num = (@int.x <= @int.y) ? ((@int.y <= @int.z) ? 2 : 1) : ((@int.x <= @int.z) ? 2 : 0);
				nodes.Sort(start, end - start, PointKDTree.comparers[num]);
				int num2 = (start + end) / 2;
				this.tree[index].split = (nodes[num2 - 1].position[num] + nodes[num2].position[num] + 1) / 2;
				this.tree[index].splitAxis = (byte)num;
				this.Build(index * 2, nodes, start, num2);
				this.Build(index * 2 + 1, nodes, num2, end);
			}
		}

		private void Add(GraphNode point, int index, int depth = 0)
		{
			while (this.tree[index].data == null)
			{
				index = 2 * index + ((point.position[(int)this.tree[index].splitAxis] >= this.tree[index].split) ? 1 : 0);
				depth++;
			}
			this.tree[index].data.Add(point);
			if (this.tree[index].data.Count > PointKDTree.LeafSize * 2)
			{
				int num = 0;
				while (depth - num > 0 && this.Size(index >> num) > PointKDTree.MaxAllowedSize(this.numNodes, depth - num))
				{
					num++;
				}
				this.Rebalance(index >> num);
			}
		}

		public GraphNode GetNearest(Int3 point, NNConstraint constraint)
		{
			GraphNode result = null;
			long maxValue = long.MaxValue;
			this.GetNearestInternal(1, point, constraint, ref result, ref maxValue);
			return result;
		}

		private void GetNearestInternal(int index, Int3 point, NNConstraint constraint, ref GraphNode best, ref long bestSqrDist)
		{
			List<GraphNode> data = this.tree[index].data;
			if (data != null)
			{
				for (int i = data.Count - 1; i >= 0; i--)
				{
					long sqrMagnitudeLong = (data[i].position - point).sqrMagnitudeLong;
					if (sqrMagnitudeLong < bestSqrDist && (constraint == null || constraint.Suitable(data[i])))
					{
						bestSqrDist = sqrMagnitudeLong;
						best = data[i];
					}
				}
			}
			else
			{
				long num = (long)(point[(int)this.tree[index].splitAxis] - this.tree[index].split);
				int num2 = 2 * index + ((num >= 0L) ? 1 : 0);
				this.GetNearestInternal(num2, point, constraint, ref best, ref bestSqrDist);
				if (num * num < bestSqrDist)
				{
					this.GetNearestInternal(num2 ^ 1, point, constraint, ref best, ref bestSqrDist);
				}
			}
		}

		public void GetInRange(Int3 point, long sqrRadius, List<GraphNode> buffer)
		{
			this.GetInRangeInternal(1, point, sqrRadius, buffer);
		}

		private void GetInRangeInternal(int index, Int3 point, long sqrRadius, List<GraphNode> buffer)
		{
			List<GraphNode> data = this.tree[index].data;
			if (data != null)
			{
				for (int i = data.Count - 1; i >= 0; i--)
				{
					long sqrMagnitudeLong = (data[i].position - point).sqrMagnitudeLong;
					if (sqrMagnitudeLong < sqrRadius)
					{
						buffer.Add(data[i]);
					}
				}
			}
			else
			{
				long num = (long)(point[(int)this.tree[index].splitAxis] - this.tree[index].split);
				int num2 = 2 * index + ((num >= 0L) ? 1 : 0);
				this.GetInRangeInternal(num2, point, sqrRadius, buffer);
				if (num * num < sqrRadius)
				{
					this.GetInRangeInternal(num2 ^ 1, point, sqrRadius, buffer);
				}
			}
		}

		public static int LeafSize = 10;

		private PointKDTree.Node[] tree = new PointKDTree.Node[16];

		private int numNodes;

		private readonly List<GraphNode> largeList = new List<GraphNode>();

		private readonly Stack<List<GraphNode>> listCache = new Stack<List<GraphNode>>();

		private static readonly IComparer<GraphNode>[] comparers = new IComparer<GraphNode>[]
		{
			new PointKDTree.CompareX(),
			new PointKDTree.CompareY(),
			new PointKDTree.CompareZ()
		};

		private struct Node
		{
			public List<GraphNode> data;

			public int split;

			public byte splitAxis;
		}

		private class CompareX : IComparer<GraphNode>
		{
			public int Compare(GraphNode lhs, GraphNode rhs)
			{
				return lhs.position.x.CompareTo(rhs.position.x);
			}
		}

		private class CompareY : IComparer<GraphNode>
		{
			public int Compare(GraphNode lhs, GraphNode rhs)
			{
				return lhs.position.y.CompareTo(rhs.position.y);
			}
		}

		private class CompareZ : IComparer<GraphNode>
		{
			public int Compare(GraphNode lhs, GraphNode rhs)
			{
				return lhs.position.z.CompareTo(rhs.position.z);
			}
		}
	}
}
