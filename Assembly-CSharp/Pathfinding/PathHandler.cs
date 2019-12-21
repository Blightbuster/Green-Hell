using System;
using System.Collections.Generic;
using System.Text;

namespace Pathfinding
{
	public class PathHandler
	{
		public ushort PathID
		{
			get
			{
				return this.pathID;
			}
		}

		public PathHandler(int threadID, int totalThreadCount)
		{
			this.threadID = threadID;
			this.totalThreadCount = totalThreadCount;
		}

		public void InitializeForPath(Path p)
		{
			this.pathID = p.pathID;
			this.heap.Clear();
		}

		public void DestroyNode(GraphNode node)
		{
			PathNode pathNode = this.GetPathNode(node);
			pathNode.node = null;
			pathNode.parent = null;
			pathNode.pathID = 0;
			pathNode.G = 0u;
			pathNode.H = 0u;
		}

		public void InitializeNode(GraphNode node)
		{
			int nodeIndex = node.NodeIndex;
			int num = nodeIndex >> 10;
			int num2 = nodeIndex & 1023;
			if (num >= this.nodes.Length)
			{
				PathNode[][] array = new PathNode[Math.Max(Math.Max(this.nodes.Length * 3 / 2, num + 1), this.nodes.Length + 2)][];
				for (int i = 0; i < this.nodes.Length; i++)
				{
					array[i] = this.nodes[i];
				}
				bool[] array2 = new bool[array.Length];
				for (int j = 0; j < this.nodes.Length; j++)
				{
					array2[j] = this.bucketNew[j];
				}
				bool[] array3 = new bool[array.Length];
				for (int k = 0; k < this.nodes.Length; k++)
				{
					array3[k] = this.bucketCreated[k];
				}
				this.nodes = array;
				this.bucketNew = array2;
				this.bucketCreated = array3;
			}
			if (this.nodes[num] == null)
			{
				PathNode[] array4;
				if (this.bucketCache.Count > 0)
				{
					array4 = this.bucketCache.Pop();
				}
				else
				{
					array4 = new PathNode[1024];
					for (int l = 0; l < 1024; l++)
					{
						array4[l] = new PathNode();
					}
				}
				this.nodes[num] = array4;
				if (!this.bucketCreated[num])
				{
					this.bucketNew[num] = true;
					this.bucketCreated[num] = true;
				}
				this.filledBuckets++;
			}
			this.nodes[num][num2].node = node;
		}

		public PathNode GetPathNode(int nodeIndex)
		{
			return this.nodes[nodeIndex >> 10][nodeIndex & 1023];
		}

		public PathNode GetPathNode(GraphNode node)
		{
			int nodeIndex = node.NodeIndex;
			return this.nodes[nodeIndex >> 10][nodeIndex & 1023];
		}

		public void ClearPathIDs()
		{
			for (int i = 0; i < this.nodes.Length; i++)
			{
				PathNode[] array = this.nodes[i];
				if (array != null)
				{
					for (int j = 0; j < 1024; j++)
					{
						array[j].pathID = 0;
					}
				}
			}
		}

		private ushort pathID;

		public readonly int threadID;

		public readonly int totalThreadCount;

		public readonly BinaryHeap heap = new BinaryHeap(128);

		private const int BucketSizeLog2 = 10;

		private const int BucketSize = 1024;

		private const int BucketIndexMask = 1023;

		public PathNode[][] nodes = new PathNode[0][];

		private bool[] bucketNew = new bool[0];

		private bool[] bucketCreated = new bool[0];

		private Stack<PathNode[]> bucketCache = new Stack<PathNode[]>();

		private int filledBuckets;

		public readonly StringBuilder DebugStringBuilder = new StringBuilder();
	}
}
